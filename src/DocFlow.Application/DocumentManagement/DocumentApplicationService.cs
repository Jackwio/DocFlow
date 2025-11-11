using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.Documents;
using DocFlow.Documents.Dtos;
using DocFlow.Enums;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;

namespace DocFlow.DocumentManagement;

/// <summary>
/// Application service for document management operations.
/// Implements US1 (Upload), US2 (Status Tracking), US3 (Failed Retry), US4 (Search & Filter).
/// </summary>
[Authorize]
public class DocumentApplicationService : ApplicationService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobContainer<DocFlowBlobContainer> _blobContainer;

    public DocumentApplicationService(
        IDocumentRepository documentRepository,
        IBlobContainer<DocFlowBlobContainer> blobContainer)
    {
        _documentRepository = documentRepository;
        _blobContainer = blobContainer;
    }

    /// <summary>
    /// US1: Upload a document with file stream.
    /// Backend uploads file to blob storage.
    /// </summary>
    public async Task<DocumentDto> UploadDocumentAsync(Stream fileStream, string originalFileName, long fileLength, string contentType, UploadDocumentDto input)
    {
        if (fileStream == null || fileLength == 0)
        {
            throw new ArgumentException("File is required");
        }

        // Validate file size (50MB max)
        const long maxFileSize = 50 * 1024 * 1024; // 50MB
        if (fileLength > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
        }

        // Validate MIME type
        var allowedMimeTypes = new[] { "application/pdf", "image/png", "image/jpeg", "image/tiff" };
        var mimeTypeLower = contentType.ToLowerInvariant();
        if (!allowedMimeTypes.Contains(mimeTypeLower))
        {
            throw new ArgumentException($"File type {contentType} is not allowed. Allowed types: PDF, PNG, JPG, TIFF");
        }

        // Use custom filename if provided, otherwise use original
        var fileNameStr = string.IsNullOrWhiteSpace(input.FileName) ? originalFileName : input.FileName;
        
        // Create value objects
        var fileName = FileName.Create(fileNameStr);
        var fileSize = FileSize.Create(fileLength);
        var mimeType = MimeType.Create(mimeTypeLower);
        var inbox = !string.IsNullOrWhiteSpace(input.Inbox) ? InboxName.Create(input.Inbox) : null;

        // Generate unique blob name with tenant-specific path
        // Format: {tenantId}/{guid}.{extension} or host/{guid}.{extension} for null tenant
        var tenantFolder = CurrentTenant.Id?.ToString() ?? "host";
        var uniqueFileName = $"{GuidGenerator.Create()}{Path.GetExtension(fileNameStr)}";
        var blobName = $"{tenantFolder}/{uniqueFileName}";
        var containerName = "documents";

        // Upload file to blob storage (will be saved to wwwroot/documents/{tenantId}/{guid}.ext)
        await _blobContainer.SaveAsync(blobName, fileStream, overrideExisting: true);

        var blobReference = BlobReference.Create(containerName, blobName);

        // Create document aggregate
        var document = Document.RegisterUpload(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            fileName,
            fileSize,
            mimeType,
            blobReference,
            inbox);

        // Save to repository
        await _documentRepository.InsertAsync(document, autoSave: true);

        // Map to DTO
        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// US2: Get a document by ID.
    /// </summary>
    public async Task<DocumentDto> GetDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetAsync(id);
        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// US2: Get document list with pagination and filtering.
    /// </summary>
    public async Task<PagedResultDto<DocumentListDto>> GetDocumentListAsync(
        DocumentStatus? status = null,
        DateTime? uploadedAfter = null,
        DateTime? uploadedBefore = null,
        int skipCount = 0,
        int maxResultCount = 10)
    {
        var documents = await _documentRepository.SearchAsync(
            status: status,
            uploadedAfter: uploadedAfter,
            uploadedBefore: uploadedBefore,
            maxResults: maxResultCount,
            skipCount: skipCount);

        var totalCount = documents.Count; // Simplified - should use count query in production

        var dtos = ObjectMapper.Map<List<Document>, List<DocumentListDto>>(documents);

        return new PagedResultDto<DocumentListDto>(totalCount, dtos);
    }

    /// <summary>
    /// US3: Retry classification for a failed document.
    /// </summary>
    public async Task<DocumentDto> RetryClassificationAsync(Guid id)
    {
        var document = await _documentRepository.GetAsync(id);

        // Validate document is in Failed status
        if (document.Status != DocumentStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot retry document in status {document.Status}. Expected Failed.");
        }

        // Reset document for retry
        document.RetryClassification();

        // Save changes
        await _documentRepository.UpdateAsync(document, autoSave: true);

        // Note: Classification job would be triggered by domain event handler

        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// US4: Search documents with advanced filtering.
    /// </summary>
    public async Task<PagedResultDto<DocumentListDto>> SearchDocumentsAsync(DocumentSearchDto input)
    {
        var documents = await _documentRepository.SearchAsync(
            status: input.Status,
            tags: input.Tags,
            fileNameContains: input.FileNameContains,
            uploadedAfter: input.UploadedAfter,
            uploadedBefore: input.UploadedBefore,
            maxResults: input.MaxResults,
            skipCount: input.SkipCount);

        var totalCount = documents.Count; // Simplified - should use count query in production

        var dtos = ObjectMapper.Map<List<Document>, List<DocumentListDto>>(documents);

        return new PagedResultDto<DocumentListDto>(totalCount, dtos);
    }

    /// <summary>
    /// US10: Add a manual tag to a document.
    /// </summary>
    public async Task<DocumentDto> AddManualTagAsync(Guid id, AddManualTagDto input)
    {
        var document = await _documentRepository.GetAsync(id);
        var tagName = TagName.Create(input.TagName);

        document.AddManualTag(tagName);

        await _documentRepository.UpdateAsync(document, autoSave: true);

        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// US10: Remove a manual tag from a document.
    /// </summary>
    public async Task RemoveManualTagAsync(Guid id, string tagName)
    {
        var document = await _documentRepository.GetAsync(id);
        var tag = TagName.Create(tagName);

        document.RemoveManualTag(tag);

        await _documentRepository.UpdateAsync(document, autoSave: true);
    }

    /// <summary>
    /// Update the inbox (category) of a document.
    /// </summary>
    public async Task<DocumentDto> UpdateInboxAsync(Guid id, string? inboxName)
    {
        var document = await _documentRepository.GetAsync(id);
        var inbox = !string.IsNullOrWhiteSpace(inboxName) ? InboxName.Create(inboxName) : null;

        document.UpdateInbox(inbox);

        await _documentRepository.UpdateAsync(document, autoSave: true);

        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// Get document file content for viewing or downloading.
    /// </summary>
    public async Task<(Stream stream, string fileName, string contentType)> GetDocumentFileAsync(Guid id)
    {
        var document = await _documentRepository.GetAsync(id);
        
        var blobName = document.BlobReference.BlobName;
        var stream = await _blobContainer.GetAsync(blobName);
        
        return (stream, document.FileName.Value, document.MimeType.Value);
    }
}

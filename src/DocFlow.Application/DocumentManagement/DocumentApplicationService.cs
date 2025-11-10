using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.Documents;
using DocFlow.Documents.Dtos;
using DocFlow.Enums;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DocFlow.DocumentManagement;

/// <summary>
/// Application service for document management operations.
/// Implements US1 (Upload), US2 (Status Tracking), US3 (Failed Retry), US4 (Search & Filter).
/// </summary>
[Authorize]
public class DocumentApplicationService : ApplicationService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentApplicationService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    /// <summary>
    /// US1: Upload a document with blob storage and validation.
    /// </summary>
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto input)
    {
        // Validate file size (50MB max)
        const long maxFileSize = 50 * 1024 * 1024; // 50MB
        if (input.FileSizeBytes > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
        }

        // Validate MIME type
        var allowedMimeTypes = new[] { "application/pdf", "image/png", "image/jpeg", "image/tiff" };
        if (!allowedMimeTypes.Contains(input.MimeType.ToLowerInvariant()))
        {
            throw new ArgumentException($"File type {input.MimeType} is not allowed");
        }

        // Create value objects
        var fileName = FileName.Create(input.FileName);
        var fileSize = FileSize.Create(input.FileSizeBytes);
        var mimeType = MimeType.Create(input.MimeType);
        var blobReference = BlobReference.Create(input.BlobContainerName, input.BlobName);

        // Create document aggregate
        var document = Document.RegisterUpload(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            fileName,
            fileSize,
            mimeType,
            blobReference);

        // Save to repository
        await _documentRepository.InsertAsync(document, autoSave: true);

        // Map to DTO
        return ObjectMapper.Map<Document, DocumentDto>(document);
    }

    /// <summary>
    /// US1: Upload multiple documents in batch.
    /// </summary>
    public async Task<List<DocumentDto>> UploadBatchDocumentsAsync(List<UploadDocumentDto> inputs)
    {
        var results = new List<DocumentDto>();

        foreach (var input in inputs)
        {
            var result = await UploadDocumentAsync(input);
            results.Add(result);
        }

        return results;
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
}

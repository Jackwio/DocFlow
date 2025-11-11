using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocFlow.DocumentManagement;
using DocFlow.Documents.Dtos;
using DocFlow.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace DocFlow.Controllers;

/// <summary>
/// REST API controller for document management operations.
/// Implements US1 (Upload), US2 (Status Tracking), US3 (Failed Retry), US4 (Search & Filter), US10 (Manual Tags), US12 (Audit).
/// </summary>
[ApiController]
[Route("api/documents")]
public class DocumentsController : AbpControllerBase
{
    private readonly DocumentApplicationService _documentService;

    public DocumentsController(DocumentApplicationService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// US1: Upload a document with file content (multipart/form-data).
    /// Backend receives the file and uploads it to blob storage.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="fileName">Optional custom filename (defaults to original filename)</param>
    /// <param name="description">Optional description</param>
    /// <returns>Created document details</returns>
    [HttpPost]
    [Route("upload")]
    [Consumes("multipart/form-data")]
    public async Task<DocumentDto> UploadAsync([FromForm] UploadDocumentWithFileDto input)
    {
        var file = input?.File ?? throw new ArgumentException("File is required");

        var uploadInput = new UploadDocumentDto
        {
            FileName = input.FileName,
            Description = input.Description,
            Inbox = input.Inbox
        };

        await using var stream = file.OpenReadStream();
        return await _documentService.UploadDocumentAsync(
            stream,
            file.FileName,
            file.Length,
            file.ContentType,
            uploadInput);
    }

    /// <summary>
    /// US1: Upload multiple documents in batch (multipart/form-data).
    /// Backend receives multiple files and uploads them to blob storage.
    /// </summary>
    /// <param name="files">The files to upload</param>
    /// <returns>List of created document details</returns>
    [HttpPost]
    [Route("upload-batch")]
    [Consumes("multipart/form-data")]
    public async Task<List<DocumentDto>> UploadBatchAsync([FromForm] IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
        {
            throw new ArgumentException("At least one file is required");
        }

        var results = new List<DocumentDto>();

        foreach (var file in files)
        {
            var input = new UploadDocumentDto();
            await using var stream = file.OpenReadStream();
            
            var result = await _documentService.UploadDocumentAsync(
                stream,
                file.FileName,
                file.Length,
                file.ContentType,
                input);
            
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// US2: Get a document by ID.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document details</returns>
    [HttpGet]
    [Route("{id}")]
    public async Task<DocumentDto> GetAsync(Guid id)
    {
        return await _documentService.GetDocumentAsync(id);
    }

    /// <summary>
    /// US2: Get document list with pagination and filtering.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <param name="uploadedAfter">Optional date filter (after)</param>
    /// <param name="uploadedBefore">Optional date filter (before)</param>
    /// <param name="skipCount">Number of records to skip</param>
    /// <param name="maxResultCount">Maximum number of results</param>
    /// <returns>Paginated document list</returns>
    [HttpGet]
    public async Task<PagedResultDto<DocumentListDto>> GetListAsync(
        [FromQuery] DocumentStatus? status = null,
        [FromQuery] DateTime? uploadedAfter = null,
        [FromQuery] DateTime? uploadedBefore = null,
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10)
    {
        return await _documentService.GetDocumentListAsync(
            status,
            uploadedAfter,
            uploadedBefore,
            skipCount,
            maxResultCount);
    }

    /// <summary>
    /// US3: Retry classification for a failed document.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Updated document details</returns>
    [HttpPost]
    [Route("{id}/retry")]
    public async Task<DocumentDto> RetryClassificationAsync(Guid id)
    {
        return await _documentService.RetryClassificationAsync(id);
    }

    /// <summary>
    /// US4: Search documents with advanced filtering.
    /// </summary>
    /// <param name="input">Search criteria</param>
    /// <returns>Paginated search results</returns>
    [HttpPost]
    [Route("search")]
    public async Task<PagedResultDto<DocumentListDto>> SearchAsync([FromBody] DocumentSearchDto input)
    {
        return await _documentService.SearchDocumentsAsync(input);
    }

    /// <summary>
    /// US10: Add a manual tag to a document.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="input">Tag information</param>
    /// <returns>Updated document details</returns>
    [HttpPost]
    [Route("{id}/tags")]
    public async Task<DocumentDto> AddManualTagAsync(Guid id, [FromBody] AddManualTagDto input)
    {
        return await _documentService.AddManualTagAsync(id, input);
    }

    /// <summary>
    /// US10: Remove a manual tag from a document.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="tagName">Tag name to remove</param>
    [HttpDelete]
    [Route("{id}/tags/{tagName}")]
    public async Task RemoveManualTagAsync(Guid id, string tagName)
    {
        await _documentService.RemoveManualTagAsync(id, tagName);
    }

    /// <summary>
    /// US12: Get classification history for a document (audit trail).
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document with full classification history</returns>
    [HttpGet]
    [Route("{id}/history")]
    public async Task<DocumentDto> GetClassificationHistoryAsync(Guid id)
    {
        // Returns full document including classification history
        return await _documentService.GetDocumentAsync(id);
    }

    /// <summary>
    /// View a document file inline in the browser.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>File content for viewing</returns>
    [HttpGet]
    [Route("{id}/view")]
    public async Task<IActionResult> ViewAsync(Guid id)
    {
        var (stream, fileName, contentType) = await _documentService.GetDocumentFileAsync(id);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// Download a document file.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>File content for download</returns>
    [HttpGet]
    [Route("{id}/download")]
    public async Task<IActionResult> DownloadAsync(Guid id)
    {
        var (stream, fileName, contentType) = await _documentService.GetDocumentFileAsync(id);
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// Update the inbox (category) of a document.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="inboxName">New inbox name (null to clear)</param>
    /// <returns>Updated document details</returns>
    [HttpPut]
    [Route("{id}/inbox")]
    public async Task<DocumentDto> UpdateInboxAsync(Guid id, [FromBody] string? inboxName)
    {
        return await _documentService.UpdateInboxAsync(id, inboxName);
    }
}

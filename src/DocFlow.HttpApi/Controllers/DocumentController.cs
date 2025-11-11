using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocFlow.Controllers;
using DocFlow.DocumentManagement;
using DocFlow.Documents.Dtos;
using DocFlow.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace DocFlow.HttpApi.Controllers;

/// <summary>
/// API controller for document management operations.
/// </summary>
[Route("api/documents")]
public class DocumentController : DocFlowController
{
    private readonly IDocumentApplicationService _documentService;

    public DocumentController(IDocumentApplicationService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Upload a new document.
    /// </summary>
    [HttpPost("upload")]
    public async Task<DocumentDto> UploadDocument([FromForm] IFormFile file, [FromForm] UploadDocumentDto input)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        await using var stream = file.OpenReadStream();
        return await _documentService.UploadDocumentAsync(
            stream,
            file.FileName,
            file.Length,
            file.ContentType,
            input);
    }

    /// <summary>
    /// Get document by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<DocumentDto> GetDocument(Guid id)
    {
        return await _documentService.GetDocumentAsync(id);
    }

    /// <summary>
    /// Get paginated document list.
    /// </summary>
    [HttpGet]
    public async Task<PagedResultDto<DocumentListDto>> GetDocumentList(
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
    /// Search documents with filters.
    /// </summary>
    [HttpPost("search")]
    public async Task<PagedResultDto<DocumentListDto>> SearchDocuments([FromBody] DocumentSearchDto input)
    {
        return await _documentService.SearchDocumentsAsync(input);
    }

    /// <summary>
    /// Retry classification for a failed document.
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<DocumentDto> RetryClassification(Guid id)
    {
        return await _documentService.RetryClassificationAsync(id);
    }

    /// <summary>
    /// Add a manual tag to a document.
    /// </summary>
    [HttpPost("{id}/tags")]
    public async Task<DocumentDto> AddManualTag(Guid id, [FromBody] AddManualTagDto input)
    {
        return await _documentService.AddManualTagAsync(id, input);
    }

    /// <summary>
    /// Remove a manual tag from a document.
    /// </summary>
    [HttpDelete("{id}/tags/{tagName}")]
    public async Task RemoveManualTag(Guid id, string tagName)
    {
        await _documentService.RemoveManualTagAsync(id, tagName);
    }

    /// <summary>
    /// Get upload statistics for the current user.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<UploadStatisticsDto> GetUploadStatistics()
    {
        return await _documentService.GetUploadStatisticsAsync();
    }

    /// <summary>
    /// Get recently uploaded documents (within last 24 hours).
    /// </summary>
    [HttpGet("recent")]
    public async Task<List<RecentUploadDto>> GetRecentUploads([FromQuery] int maxResults = 50)
    {
        return await _documentService.GetRecentUploadsAsync(maxResults);
    }
}

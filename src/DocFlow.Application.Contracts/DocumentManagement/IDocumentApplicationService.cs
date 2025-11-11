using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocFlow.Documents.Dtos;
using DocFlow.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DocFlow.DocumentManagement;

/// <summary>
/// Interface for document management application service.
/// </summary>
public interface IDocumentApplicationService : IApplicationService
{
    /// <summary>
    /// Upload a document with file stream.
    /// </summary>
    Task<DocumentDto> UploadDocumentAsync(Stream fileStream, string originalFileName, long fileLength, string contentType, UploadDocumentDto input);

    /// <summary>
    /// Get a document by ID.
    /// </summary>
    Task<DocumentDto> GetDocumentAsync(Guid id);

    /// <summary>
    /// Get document list with pagination and filtering.
    /// </summary>
    Task<PagedResultDto<DocumentListDto>> GetDocumentListAsync(
        DocumentStatus? status = null,
        DateTime? uploadedAfter = null,
        DateTime? uploadedBefore = null,
        int skipCount = 0,
        int maxResultCount = 10);

    /// <summary>
    /// Retry classification for a failed document.
    /// </summary>
    Task<DocumentDto> RetryClassificationAsync(Guid id);

    /// <summary>
    /// Search documents with advanced filtering.
    /// </summary>
    Task<PagedResultDto<DocumentListDto>> SearchDocumentsAsync(DocumentSearchDto input);

    /// <summary>
    /// Add a manual tag to a document.
    /// </summary>
    Task<DocumentDto> AddManualTagAsync(Guid id, AddManualTagDto input);

    /// <summary>
    /// Remove a manual tag from a document.
    /// </summary>
    Task RemoveManualTagAsync(Guid id, string tagName);

    /// <summary>
    /// Get upload statistics for the current user.
    /// </summary>
    Task<UploadStatisticsDto> GetUploadStatisticsAsync();

    /// <summary>
    /// Get recently uploaded documents (within last 24 hours).
    /// </summary>
    Task<List<RecentUploadDto>> GetRecentUploadsAsync(int maxResults = 50);
}

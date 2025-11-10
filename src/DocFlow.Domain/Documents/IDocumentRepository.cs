using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.Enums;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Documents;

/// <summary>
/// Repository interface for Document aggregate.
/// Defines business-specific query methods beyond standard CRUD.
/// </summary>
public interface IDocumentRepository : IRepository<Document, Guid>
{
    /// <summary>
    /// Finds documents by their processing status.
    /// </summary>
    Task<List<Document>> FindByStatusAsync(
        DocumentStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches documents with flexible filtering criteria.
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <param name="tags">Optional tag names to filter by</param>
    /// <param name="fileNameContains">Optional filename search term</param>
    /// <param name="uploadedAfter">Optional minimum upload date</param>
    /// <param name="uploadedBefore">Optional maximum upload date</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="skipCount">Number of results to skip for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<Document>> SearchAsync(
        DocumentStatus? status = null,
        List<string>? tags = null,
        string? fileNameContains = null,
        DateTime? uploadedAfter = null,
        DateTime? uploadedBefore = null,
        int maxResults = 100,
        int skipCount = 0,
        CancellationToken cancellationToken = default);
}

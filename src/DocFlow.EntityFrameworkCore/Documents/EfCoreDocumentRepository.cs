using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.Documents;
using DocFlow.Enums;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.EntityFrameworkCore.Documents;

/// <summary>
/// EF Core repository implementation for Document aggregate.
/// </summary>
public sealed class EfCoreDocumentRepository : EfCoreRepository<DocFlowDbContext, Document, Guid>, IDocumentRepository
{
    public EfCoreDocumentRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Document>> FindByStatusAsync(
        DocumentStatus status,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Documents
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> SearchAsync(
        DocumentStatus? status = null,
        List<string>? tags = null,
        string? fileNameContains = null,
        DateTime? uploadedAfter = null,
        DateTime? uploadedBefore = null,
        int maxResults = 100,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = dbContext.Documents.AsQueryable();

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(fileNameContains))
        {
            query = query.Where(d => EF.Functions.Like(
                EF.Property<string>(d.FileName, "Value"),
                $"%{fileNameContains}%"));
        }

        if (uploadedAfter.HasValue)
        {
            query = query.Where(d => d.CreationTime >= uploadedAfter.Value);
        }

        if (uploadedBefore.HasValue)
        {
            query = query.Where(d => d.CreationTime <= uploadedBefore.Value);
        }

        // Tag filtering - Note: This is simplified, real implementation would need proper tag querying
        if (tags != null && tags.Any())
        {
            // This would require proper includes and filtering on the Tags collection
            // For now, keeping it simple
        }

        return await query
            .OrderByDescending(d => d.CreationTime)
            .Skip(skipCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUploadCountAsync(
        DateTime after,
        DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = dbContext.Documents
            .Where(d => d.CreationTime >= after);

        if (before.HasValue)
        {
            query = query.Where(d => d.CreationTime <= before.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<long> GetTotalStorageUsedAsync(
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        
        // Sum up all file sizes
        var totalBytes = await dbContext.Documents
            .SumAsync(d => EF.Property<long>(d.FileSize, "Bytes"), cancellationToken);

        return totalBytes;
    }

    public async Task<List<Document>> GetRecentUploadsAsync(
        DateTime after,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        
        return await dbContext.Documents
            .Where(d => d.CreationTime >= after)
            .OrderByDescending(d => d.CreationTime)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.Documents;

public class DocumentRepository : EfCoreRepository<DocFlowDbContext, Document, Guid>, IDocumentRepository
{
    public DocumentRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Document>> GetPendingDocumentsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<Document>()
            .Where(d => d.Status == DocumentStatus.Pending)
            .OrderBy(d => d.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetFailedDocumentsForRetryAsync(int maxRetries, int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<Document>()
            .Where(d => d.Status == DocumentStatus.Failed && d.RetryCount < maxRetries)
            .OrderBy(d => d.LastRetryTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;
        return await dbContext.Set<Document>()
            .Where(d => d.RetentionExpiryDate.HasValue && d.RetentionExpiryDate.Value <= now && d.Status != DocumentStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetTenantStorageUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<Document>()
            .Where(d => d.TenantId == tenantId && d.Status != DocumentStatus.Expired)
            .SumAsync(d => d.FileSize, cancellationToken);
    }

    public async Task<int> GetTenantDocumentCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<Document>()
            .CountAsync(d => d.TenantId == tenantId && d.Status != DocumentStatus.Expired, cancellationToken);
    }
}

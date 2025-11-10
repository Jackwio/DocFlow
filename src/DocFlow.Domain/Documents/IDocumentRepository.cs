using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Documents;

public interface IDocumentRepository : IRepository<Document, Guid>
{
    Task<List<Document>> GetPendingDocumentsAsync(int maxCount, CancellationToken cancellationToken = default);
    Task<List<Document>> GetFailedDocumentsForRetryAsync(int maxRetries, int maxCount, CancellationToken cancellationToken = default);
    Task<List<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default);
    Task<long> GetTenantStorageUsageAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<int> GetTenantDocumentCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

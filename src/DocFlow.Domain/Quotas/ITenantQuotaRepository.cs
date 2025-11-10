using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Quotas;

public interface ITenantQuotaRepository : IRepository<TenantQuota, Guid>
{
    Task<TenantQuota?> FindByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

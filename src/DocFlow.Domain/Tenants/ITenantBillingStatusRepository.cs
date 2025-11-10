using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Tenants;

public interface ITenantBillingStatusRepository : IRepository<TenantBillingStatus, Guid>
{
    Task<TenantBillingStatus?> FindByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<TenantBillingStatus>> GetTenantsWithExpiredGracePeriodAsync(CancellationToken cancellationToken = default);
}

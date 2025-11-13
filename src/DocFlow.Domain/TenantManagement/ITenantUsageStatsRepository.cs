using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.TenantManagement;

/// <summary>
/// Repository interface for TenantUsageStats aggregate.
/// </summary>
public interface ITenantUsageStatsRepository : IRepository<TenantUsageStats, Guid>
{
    /// <summary>
    /// Gets the usage statistics for a specific tenant.
    /// </summary>
    Task<TenantUsageStats?> FindByTenantIdAsync(Guid? tenantId, CancellationToken cancellationToken = default);
}

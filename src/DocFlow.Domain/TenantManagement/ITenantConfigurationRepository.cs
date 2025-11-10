using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.TenantManagement;

/// <summary>
/// Repository interface for TenantConfiguration aggregate.
/// </summary>
public interface ITenantConfigurationRepository : IRepository<TenantConfiguration, Guid>
{
    /// <summary>
    /// Gets the configuration for a specific tenant.
    /// </summary>
    Task<TenantConfiguration?> FindByTenantIdAsync(Guid? tenantId, CancellationToken cancellationToken = default);
}

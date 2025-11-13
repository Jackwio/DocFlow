using System.Threading.Tasks;
using DocFlow.TenantManagement.Dtos;
using Volo.Abp.Application.Services;

namespace DocFlow.TenantManagement;

/// <summary>
/// Application service for viewing tenant usage statistics.
/// </summary>
public interface ITenantUsageStatsApplicationService : IApplicationService
{
    /// <summary>
    /// Gets the current tenant's usage statistics.
    /// </summary>
    Task<TenantUsageStatsDto> GetAsync();

    /// <summary>
    /// Refreshes the usage statistics from actual counts.
    /// </summary>
    Task<TenantUsageStatsDto> RefreshAsync();
}

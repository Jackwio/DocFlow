using System;
using System.Threading.Tasks;
using DocFlow.Tenants;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class TenantSuspensionJob : AsyncBackgroundJob<TenantSuspensionJobArgs>, ITransientDependency
{
    private readonly ITenantBillingStatusRepository _tenantBillingStatusRepository;
    private readonly ILogger<TenantSuspensionJob> _logger;

    public TenantSuspensionJob(
        ITenantBillingStatusRepository tenantBillingStatusRepository,
        ILogger<TenantSuspensionJob> logger)
    {
        _tenantBillingStatusRepository = tenantBillingStatusRepository;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(TenantSuspensionJobArgs args)
    {
        try
        {
            var tenantsWithExpiredGracePeriod = await _tenantBillingStatusRepository.GetTenantsWithExpiredGracePeriodAsync();

            _logger.LogInformation(
                "Found {Count} tenants with expired grace period to convert to read-only",
                tenantsWithExpiredGracePeriod.Count);

            foreach (var tenantBillingStatus in tenantsWithExpiredGracePeriod)
            {
                if (tenantBillingStatus.IsGracePeriodExpired())
                {
                    tenantBillingStatus.MarkAsReadOnly();
                    await _tenantBillingStatusRepository.UpdateAsync(tenantBillingStatus);

                    _logger.LogWarning(
                        "Tenant {TenantId} converted to read-only mode due to expired payment grace period",
                        tenantBillingStatus.TenantId);
                    
                    // TODO: Notify tenant about suspension
                    await NotifyTenantAboutSuspension(tenantBillingStatus.TenantId);
                }
            }

            _logger.LogInformation(
                "Tenant suspension job completed. Processed {Count} tenants",
                tenantsWithExpiredGracePeriod.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tenant suspension job");
            throw;
        }
    }

    private Task NotifyTenantAboutSuspension(Guid tenantId)
    {
        // Placeholder for tenant notification logic
        // In production, this would send an email or other notification
        _logger.LogDebug("Sending suspension notification to tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }
}

public class TenantSuspensionJobArgs
{
    // No specific arguments needed for now
}

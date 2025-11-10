using System;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.Documents;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class UsageReportingJob : AsyncBackgroundJob<UsageReportingJobArgs>, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<UsageReportingJob> _logger;

    public UsageReportingJob(
        ITenantRepository tenantRepository,
        IDocumentRepository documentRepository,
        ICurrentTenant currentTenant,
        ILogger<UsageReportingJob> logger)
    {
        _tenantRepository = tenantRepository;
        _documentRepository = documentRepository;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(UsageReportingJobArgs args)
    {
        try
        {
            var tenants = await _tenantRepository.GetListAsync();

            foreach (var tenant in tenants)
            {
                using (_currentTenant.Change(tenant.Id))
                {
                    var documentCount = await _documentRepository.GetTenantDocumentCountAsync(tenant.Id);
                    var storageUsage = await _documentRepository.GetTenantStorageUsageAsync(tenant.Id);

                    // TODO: Send usage report to billing service
                    // This would typically be an HTTP call to a billing API
                    await SendUsageReportToBillingService(tenant.Id, documentCount, storageUsage);

                    _logger.LogInformation(
                        "Usage report sent for tenant {TenantId}: {DocumentCount} documents, {StorageUsage} bytes",
                        tenant.Id, documentCount, storageUsage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing usage reporting job");
            throw;
        }
    }

    private Task SendUsageReportToBillingService(Guid tenantId, int documentCount, long storageUsage)
    {
        // Placeholder for actual billing service integration
        // In production, this would make an HTTP call to the billing API
        _logger.LogDebug(
            "Sending usage report to billing service for tenant {TenantId}: Documents={DocumentCount}, Storage={StorageBytes}",
            tenantId, documentCount, storageUsage);
        
        return Task.CompletedTask;
    }
}

public class UsageReportingJobArgs
{
    // No specific arguments needed for now
}

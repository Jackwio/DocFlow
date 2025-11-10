using System;
using System.Threading.Tasks;
using DocFlow.Documents;
using DocFlow.Quotas;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace DocFlow.BackgroundJobs;

public class QuotaCheckJob : AsyncBackgroundJob<QuotaCheckJobArgs>, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantQuotaRepository _tenantQuotaRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<QuotaCheckJob> _logger;

    public QuotaCheckJob(
        ITenantRepository tenantRepository,
        ITenantQuotaRepository tenantQuotaRepository,
        IDocumentRepository documentRepository,
        ICurrentTenant currentTenant,
        ILogger<QuotaCheckJob> logger)
    {
        _tenantRepository = tenantRepository;
        _tenantQuotaRepository = tenantQuotaRepository;
        _documentRepository = documentRepository;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(QuotaCheckJobArgs args)
    {
        try
        {
            var tenants = await _tenantRepository.GetListAsync();

            foreach (var tenant in tenants)
            {
                using (_currentTenant.Change(tenant.Id))
                {
                    var quota = await _tenantQuotaRepository.FindByTenantIdAsync(tenant.Id);
                    if (quota == null)
                    {
                        _logger.LogWarning("No quota found for tenant {TenantId}", tenant.Id);
                        continue;
                    }

                    var currentDocumentCount = await _documentRepository.GetTenantDocumentCountAsync(tenant.Id);
                    var currentStorageUsage = await _documentRepository.GetTenantStorageUsageAsync(tenant.Id);

                    quota.UpdateUsage(currentDocumentCount, currentStorageUsage);
                    await _tenantQuotaRepository.UpdateAsync(quota);

                    if (quota.IsQuotaExceeded())
                    {
                        _logger.LogWarning(
                            "Tenant {TenantId} exceeded quota. Documents: {CurrentCount}/{MaxCount}, Storage: {CurrentStorage}/{MaxStorage} bytes",
                            tenant.Id,
                            currentDocumentCount, quota.MaxDocuments,
                            currentStorageUsage, quota.MaxStorageBytes);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Tenant {TenantId} quota check passed. Documents: {CurrentCount}/{MaxCount}, Storage: {CurrentStorage}/{MaxStorage} bytes",
                            tenant.Id,
                            currentDocumentCount, quota.MaxDocuments,
                            currentStorageUsage, quota.MaxStorageBytes);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing quota check job");
            throw;
        }
    }
}

public class QuotaCheckJobArgs
{
    // No specific arguments needed for now
}

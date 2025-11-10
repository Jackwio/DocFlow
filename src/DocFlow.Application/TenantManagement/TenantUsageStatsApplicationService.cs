using System;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.ClassificationRules;
using DocFlow.Documents;
using DocFlow.TenantManagement;
using DocFlow.TenantManagement.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.TenantManagement;

/// <summary>
/// Application service for viewing tenant usage statistics.
/// Provides tenant administrators with current plan usage information.
/// </summary>
public sealed class TenantUsageStatsApplicationService : ApplicationService, ITenantUsageStatsApplicationService
{
    private readonly ITenantUsageStatsRepository _usageStatsRepository;
    private readonly IRepository<TenantUsageStats, Guid> _repository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IClassificationRuleRepository _ruleRepository;

    public TenantUsageStatsApplicationService(
        ITenantUsageStatsRepository usageStatsRepository,
        IRepository<TenantUsageStats, Guid> repository,
        IDocumentRepository documentRepository,
        IClassificationRuleRepository ruleRepository)
    {
        _usageStatsRepository = usageStatsRepository;
        _repository = repository;
        _documentRepository = documentRepository;
        _ruleRepository = ruleRepository;
    }

    public async Task<TenantUsageStatsDto> GetAsync()
    {
        var stats = await GetOrCreateStatsAsync();
        return MapToDto(stats);
    }

    public async Task<TenantUsageStatsDto> RefreshAsync()
    {
        var stats = await GetOrCreateStatsAsync();
        
        // Get actual counts from repositories
        var documents = await _documentRepository.GetListAsync();
        var tenantDocuments = documents.Where(d => d.TenantId == CurrentTenant.Id).ToList();
        
        var documentCount = tenantDocuments.Count;
        var storageBytes = tenantDocuments.Sum(d => d.FileSize.Bytes);
        
        var rules = await _ruleRepository.GetListAsync();
        var ruleCount = rules.Count(r => r.TenantId == CurrentTenant.Id);
        
        // Refresh the stats
        stats.Refresh(documentCount, storageBytes, ruleCount);
        
        await _repository.UpdateAsync(stats, autoSave: true);
        
        return MapToDto(stats);
    }

    private async Task<TenantUsageStats> GetOrCreateStatsAsync()
    {
        var stats = await _usageStatsRepository.FindByTenantIdAsync(CurrentTenant.Id);
        
        if (stats == null)
        {
            stats = TenantUsageStats.Create(
                GuidGenerator.Create(),
                CurrentTenant.Id);

            await _repository.InsertAsync(stats, autoSave: true);
        }

        return stats;
    }

    private static TenantUsageStatsDto MapToDto(TenantUsageStats stats)
    {
        return new TenantUsageStatsDto
        {
            Id = stats.Id,
            DocumentCount = stats.DocumentCount.Value,
            StorageUsageBytes = stats.StorageUsage.Bytes,
            StorageUsageMB = stats.StorageUsage.ToMegabytes(),
            StorageUsageGB = stats.StorageUsage.ToGigabytes(),
            RuleCount = stats.RuleCount.Value,
            LastUpdated = stats.LastUpdated
        };
    }
}

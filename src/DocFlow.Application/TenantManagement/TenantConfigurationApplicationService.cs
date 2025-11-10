using System;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.TenantManagement;
using DocFlow.TenantManagement.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.TenantManagement;

/// <summary>
/// Application service for managing tenant configuration.
/// Allows tenant administrators to configure document management settings.
/// </summary>
public sealed class TenantConfigurationApplicationService : ApplicationService, ITenantConfigurationApplicationService
{
    private readonly ITenantConfigurationRepository _configRepository;
    private readonly IRepository<TenantConfiguration, Guid> _repository;

    public TenantConfigurationApplicationService(
        ITenantConfigurationRepository configRepository,
        IRepository<TenantConfiguration, Guid> repository)
    {
        _configRepository = configRepository;
        _repository = repository;
    }

    public async Task<TenantConfigurationDto> GetAsync()
    {
        var config = await GetOrCreateConfigurationAsync();
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> UpdateRetentionDaysAsync(UpdateRetentionDaysDto input)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        config.UpdateRetentionDays(RetentionDays.Create(input.RetentionDays));
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> UpdatePrivacyModeAsync(UpdatePrivacyModeDto input)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        config.SetPrivacyStrictMode(input.PrivacyStrictMode);
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> UpdateWebhookSignatureKeyAsync(UpdateWebhookSignatureKeyDto input)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        var signatureKey = string.IsNullOrWhiteSpace(input.WebhookSignatureKey) 
            ? null 
            : WebhookSignatureKey.Create(input.WebhookSignatureKey);
        
        config.SetWebhookSignatureKey(signatureKey);
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> UpdateMaxFileSizeAsync(UpdateMaxFileSizeDto input)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        config.UpdateMaxFileSize(MaxFileSize.FromMegabytes(input.MaxFileSizeMB));
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> ConfigureTagColorAsync(ConfigureTagColorDto input)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        config.ConfigureTagColor(input.TagName, input.ColorHex);
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    public async Task<TenantConfigurationDto> RemoveTagColorAsync(string tagName)
    {
        var config = await GetOrCreateConfigurationAsync();
        
        config.RemoveTagColor(tagName);
        
        await _repository.UpdateAsync(config, autoSave: true);
        
        return MapToDto(config);
    }

    private async Task<TenantConfiguration> GetOrCreateConfigurationAsync()
    {
        var config = await _configRepository.FindByTenantIdAsync(CurrentTenant.Id);
        
        if (config == null)
        {
            // Create default configuration
            config = TenantConfiguration.Create(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                RetentionDays.Create(365), // Default: 1 year
                MaxFileSize.FromMegabytes(50)); // Default: 50MB

            await _repository.InsertAsync(config, autoSave: true);
        }

        return config;
    }

    private static TenantConfigurationDto MapToDto(TenantConfiguration config)
    {
        return new TenantConfigurationDto
        {
            Id = config.Id,
            RetentionDays = config.RetentionDays.Value,
            PrivacyStrictMode = config.PrivacyStrictMode,
            WebhookSignatureKey = config.WebhookSignatureKey?.Value,
            MaxFileSizeBytes = config.MaxFileSize.Bytes,
            MaxFileSizeMB = config.MaxFileSize.ToMegabytes(),
            TagColors = config.TagColors.Select(tc => new TagColorConfigurationDto
            {
                TagName = tc.TagName,
                ColorHex = tc.ColorHex
            }).ToArray()
        };
    }
}

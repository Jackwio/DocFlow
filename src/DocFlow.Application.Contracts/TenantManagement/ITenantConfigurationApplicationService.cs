using System.Threading.Tasks;
using DocFlow.TenantManagement.Dtos;
using Volo.Abp.Application.Services;

namespace DocFlow.TenantManagement;

/// <summary>
/// Application service for managing tenant configuration.
/// </summary>
public interface ITenantConfigurationApplicationService : IApplicationService
{
    /// <summary>
    /// Gets the current tenant's configuration.
    /// </summary>
    Task<TenantConfigurationDto> GetAsync();

    /// <summary>
    /// Updates the document retention days.
    /// </summary>
    Task<TenantConfigurationDto> UpdateRetentionDaysAsync(UpdateRetentionDaysDto input);

    /// <summary>
    /// Updates the privacy strict mode setting.
    /// </summary>
    Task<TenantConfigurationDto> UpdatePrivacyModeAsync(UpdatePrivacyModeDto input);

    /// <summary>
    /// Updates the webhook signature key.
    /// </summary>
    Task<TenantConfigurationDto> UpdateWebhookSignatureKeyAsync(UpdateWebhookSignatureKeyDto input);

    /// <summary>
    /// Updates the maximum file size.
    /// </summary>
    Task<TenantConfigurationDto> UpdateMaxFileSizeAsync(UpdateMaxFileSizeDto input);

    /// <summary>
    /// Configures a custom tag color.
    /// </summary>
    Task<TenantConfigurationDto> ConfigureTagColorAsync(ConfigureTagColorDto input);

    /// <summary>
    /// Removes a tag color configuration.
    /// </summary>
    Task<TenantConfigurationDto> RemoveTagColorAsync(string tagName);
}

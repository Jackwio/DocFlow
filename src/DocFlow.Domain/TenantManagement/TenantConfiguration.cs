using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.TenantManagement;

/// <summary>
/// Aggregate root representing tenant-specific configuration for document management.
/// </summary>
public sealed class TenantConfiguration : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<TagColorConfiguration> _tagColors = new();

    public Guid? TenantId { get; private set; }
    public RetentionDays RetentionDays { get; private set; }
    public bool PrivacyStrictMode { get; private set; }
    public WebhookSignatureKey? WebhookSignatureKey { get; private set; }
    public MaxFileSize MaxFileSize { get; private set; }
    public IReadOnlyCollection<TagColorConfiguration> TagColors => _tagColors.AsReadOnly();

    // Private constructor for EF Core
    private TenantConfiguration()
    {
        RetentionDays = null!;
        MaxFileSize = null!;
    }

    private TenantConfiguration(
        Guid id,
        Guid? tenantId,
        RetentionDays retentionDays,
        MaxFileSize maxFileSize)
    {
        Id = id;
        TenantId = tenantId;
        RetentionDays = retentionDays ?? throw new ArgumentNullException(nameof(retentionDays));
        MaxFileSize = maxFileSize ?? throw new ArgumentNullException(nameof(maxFileSize));
        PrivacyStrictMode = false;
    }

    /// <summary>
    /// Factory method to create tenant configuration with default values.
    /// </summary>
    public static TenantConfiguration Create(
        Guid id,
        Guid? tenantId,
        RetentionDays retentionDays,
        MaxFileSize maxFileSize)
    {
        return new TenantConfiguration(id, tenantId, retentionDays, maxFileSize);
    }

    /// <summary>
    /// Updates document retention days within plan limits.
    /// </summary>
    public void UpdateRetentionDays(RetentionDays retentionDays)
    {
        RetentionDays = retentionDays ?? throw new ArgumentNullException(nameof(retentionDays));
    }

    /// <summary>
    /// Enables or disables privacy strict mode.
    /// When enabled, text extraction results are not retained.
    /// </summary>
    public void SetPrivacyStrictMode(bool enabled)
    {
        PrivacyStrictMode = enabled;
    }

    /// <summary>
    /// Sets the webhook signature key for external system verification.
    /// </summary>
    public void SetWebhookSignatureKey(WebhookSignatureKey? signatureKey)
    {
        WebhookSignatureKey = signatureKey;
    }

    /// <summary>
    /// Updates the maximum file size limit.
    /// </summary>
    public void UpdateMaxFileSize(MaxFileSize maxFileSize)
    {
        MaxFileSize = maxFileSize ?? throw new ArgumentNullException(nameof(maxFileSize));
    }

    /// <summary>
    /// Configures a custom tag color and name.
    /// </summary>
    public void ConfigureTagColor(string tagName, string colorHex)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty", nameof(tagName));
        
        if (string.IsNullOrWhiteSpace(colorHex))
            throw new ArgumentException("Color hex cannot be empty", nameof(colorHex));

        var existingConfig = _tagColors.Find(tc => tc.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        if (existingConfig != null)
        {
            _tagColors.Remove(existingConfig);
        }

        _tagColors.Add(TagColorConfiguration.Create(tagName, colorHex));
    }

    /// <summary>
    /// Removes a tag color configuration.
    /// </summary>
    public void RemoveTagColor(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty", nameof(tagName));

        var config = _tagColors.Find(tc => tc.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        if (config != null)
        {
            _tagColors.Remove(config);
        }
    }
}

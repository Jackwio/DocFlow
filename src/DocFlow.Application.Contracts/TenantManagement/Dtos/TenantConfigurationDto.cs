using System;

namespace DocFlow.TenantManagement.Dtos;

public class TenantConfigurationDto
{
    public Guid Id { get; set; }
    public int RetentionDays { get; set; }
    public bool PrivacyStrictMode { get; set; }
    public string? WebhookSignatureKey { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public double MaxFileSizeMB { get; set; }
    public TagColorConfigurationDto[] TagColors { get; set; } = Array.Empty<TagColorConfigurationDto>();
}

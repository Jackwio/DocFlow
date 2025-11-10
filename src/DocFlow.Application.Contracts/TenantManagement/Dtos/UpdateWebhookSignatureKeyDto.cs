using System.ComponentModel.DataAnnotations;

namespace DocFlow.TenantManagement.Dtos;

public class UpdateWebhookSignatureKeyDto
{
    [StringLength(256, MinimumLength = 32)]
    public string? WebhookSignatureKey { get; set; }
}

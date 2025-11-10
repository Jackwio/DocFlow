using System;
using Volo.Abp.Application.Dtos;

namespace DocFlow.RoutingQueues.Dtos;

/// <summary>
/// DTO for webhook delivery with tracking information.
/// </summary>
public sealed class WebhookDeliveryDto : EntityDto<Guid>
{
    public Guid DocumentId { get; set; }
    public Guid QueueId { get; set; }
    public int AttemptCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastError { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SucceededAt { get; set; }
}

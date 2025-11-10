using System;
using DocFlow.Enums;
using Volo.Abp.Application.Dtos;

namespace DocFlow.RoutingQueues.Dtos;

/// <summary>
/// DTO for routing queue with full details.
/// </summary>
public sealed class RoutingQueueDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QueueType Type { get; set; }
    public string? FolderPath { get; set; }
    public WebhookConfigurationDto? WebhookConfiguration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// DTO for webhook configuration.
/// </summary>
public sealed class WebhookConfigurationDto
{
    public string Url { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; }
    public int RetryDelaySeconds { get; set; }
}

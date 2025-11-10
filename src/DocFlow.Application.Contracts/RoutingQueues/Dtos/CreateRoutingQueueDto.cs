using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DocFlow.Enums;

namespace DocFlow.RoutingQueues.Dtos;

/// <summary>
/// DTO for creating a routing queue.
/// </summary>
public sealed class CreateRoutingQueueDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public QueueType Type { get; set; }

    [StringLength(500)]
    public string? FolderPath { get; set; }

    public WebhookConfigurationDto? WebhookConfiguration { get; set; }
}

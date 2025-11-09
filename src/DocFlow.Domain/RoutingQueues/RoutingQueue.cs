using System;
using DocFlow.Enums;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Aggregate root representing a destination queue for routing documents.
/// Supports both folder-based and webhook-based routing.
/// </summary>
public sealed class RoutingQueue : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public QueueType Type { get; private set; }
    public FolderPath? FolderPath { get; private set; }
    public WebhookConfiguration? WebhookConfiguration { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private RoutingQueue()
    {
        Name = string.Empty;
    }

    private RoutingQueue(
        Guid id,
        Guid? tenantId,
        string name,
        string? description,
        QueueType type)
    {
        Id = id;
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Type = type;
        IsActive = true;
    }

    /// <summary>
    /// Factory method to create a folder-based routing queue.
    /// </summary>
    public static RoutingQueue CreateFolderQueue(
        Guid id,
        Guid? tenantId,
        string name,
        string? description,
        FolderPath folderPath)
    {
        if (folderPath == null) throw new ArgumentNullException(nameof(folderPath));

        var queue = new RoutingQueue(id, tenantId, name, description, QueueType.Folder)
        {
            FolderPath = folderPath
        };

        return queue;
    }

    /// <summary>
    /// Factory method to create a webhook-based routing queue.
    /// </summary>
    public static RoutingQueue CreateWebhookQueue(
        Guid id,
        Guid? tenantId,
        string name,
        string? description,
        WebhookConfiguration webhookConfiguration)
    {
        if (webhookConfiguration == null) throw new ArgumentNullException(nameof(webhookConfiguration));

        var queue = new RoutingQueue(id, tenantId, name, description, QueueType.Webhook)
        {
            WebhookConfiguration = webhookConfiguration
        };

        return queue;
    }

    /// <summary>
    /// Updates the destination configuration for this queue.
    /// </summary>
    public void UpdateDestination(FolderPath? folderPath = null, WebhookConfiguration? webhookConfiguration = null)
    {
        if (Type == QueueType.Folder)
        {
            if (folderPath == null)
                throw new ArgumentNullException(nameof(folderPath), "Folder path is required for folder queue");
            
            FolderPath = folderPath;
            WebhookConfiguration = null;
        }
        else if (Type == QueueType.Webhook)
        {
            if (webhookConfiguration == null)
                throw new ArgumentNullException(nameof(webhookConfiguration), "Webhook configuration is required for webhook queue");
            
            WebhookConfiguration = webhookConfiguration;
            FolderPath = null;
        }
    }

    /// <summary>
    /// Activates the queue for routing.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the queue, preventing routing to this destination.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}

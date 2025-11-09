using System;
using DocFlow.Shared;
using Volo.Abp.Domain.Entities;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Entity representing a webhook delivery attempt for a document.
/// Tracks delivery status, attempts, and errors for monitoring and retry.
/// </summary>
public sealed class WebhookDelivery : Entity<Guid>
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public Guid QueueId { get; private set; }
    public int AttemptCount { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public ErrorMessage? LastError { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? SucceededAt { get; private set; }

    // Private constructor for EF Core
    private WebhookDelivery()
    {
    }

    private WebhookDelivery(Guid id, Guid documentId, Guid queueId)
    {
        Id = id;
        DocumentId = documentId;
        QueueId = queueId;
        AttemptCount = 0;
        Status = DeliveryStatus.Pending;
    }

    /// <summary>
    /// Creates a new webhook delivery for a document.
    /// </summary>
    public static WebhookDelivery Create(Guid id, Guid documentId, Guid queueId)
    {
        if (id == Guid.Empty) throw new ArgumentException("Delivery ID cannot be empty", nameof(id));
        if (documentId == Guid.Empty) throw new ArgumentException("Document ID cannot be empty", nameof(documentId));
        if (queueId == Guid.Empty) throw new ArgumentException("Queue ID cannot be empty", nameof(queueId));

        return new WebhookDelivery(id, documentId, queueId);
    }

    /// <summary>
    /// Records a delivery attempt with its result.
    /// </summary>
    public void RecordDeliveryAttempt(bool success, ErrorMessage? errorMessage = null)
    {
        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;

        if (success)
        {
            Status = DeliveryStatus.Succeeded;
            SucceededAt = DateTime.UtcNow;
            LastError = null;
        }
        else
        {
            Status = DeliveryStatus.Failed;
            LastError = errorMessage;
        }
    }

    /// <summary>
    /// Resets delivery status to retry after a failure.
    /// </summary>
    public void RetryDelivery()
    {
        if (Status != DeliveryStatus.Failed)
            throw new InvalidOperationException($"Cannot retry delivery in status {Status}. Expected Failed.");

        Status = DeliveryStatus.Pending;
        LastError = null;
    }
}

/// <summary>
/// Status of webhook delivery.
/// </summary>
public enum DeliveryStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

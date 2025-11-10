using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.Webhooks;

public sealed class WebhookEvent : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public string TargetUrl { get; private set; }
    public string? HmacSignature { get; private set; }
    public WebhookStatus Status { get; private set; }
    public DateTime? SentTime { get; private set; }
    public int RetryCount { get; private set; }

    private WebhookEvent()
    {
        // For EF Core
        EventType = string.Empty;
        Payload = string.Empty;
        TargetUrl = string.Empty;
    }

    private WebhookEvent(
        Guid id,
        Guid? tenantId,
        string eventType,
        string payload,
        string targetUrl) : base(id)
    {
        TenantId = tenantId;
        EventType = eventType;
        Payload = payload;
        TargetUrl = targetUrl;
        Status = WebhookStatus.Pending;
        RetryCount = 0;
    }

    public static WebhookEvent Create(
        Guid id,
        Guid? tenantId,
        string eventType,
        string payload,
        string targetUrl)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload cannot be empty", nameof(payload));
        }

        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            throw new ArgumentException("Target URL cannot be empty", nameof(targetUrl));
        }

        return new WebhookEvent(id, tenantId, eventType, payload, targetUrl);
    }

    public void SetHmacSignature(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature cannot be empty", nameof(signature));
        }

        HmacSignature = signature;
    }

    public void MarkAsSending()
    {
        if (Status != WebhookStatus.Pending && Status != WebhookStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot send webhook in {Status} status");
        }
        Status = WebhookStatus.Sending;
    }

    public void MarkAsSent()
    {
        Status = WebhookStatus.Sent;
        SentTime = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = WebhookStatus.Failed;
        RetryCount++;
    }

    public bool CanRetry(int maxRetries)
    {
        return RetryCount < maxRetries && Status == WebhookStatus.Failed;
    }
}

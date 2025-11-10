using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.Documents;

public sealed class Document : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSize { get; private set; }
    public string ContentType { get; private set; }
    public DocumentStatus Status { get; private set; }
    public string? Classification { get; private set; }
    public string? RoutingDestination { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? LastRetryTime { get; private set; }
    public DateTime? RetentionExpiryDate { get; private set; }

    private Document()
    {
        // For EF Core
        FileName = string.Empty;
        FilePath = string.Empty;
        ContentType = string.Empty;
    }

    private Document(
        Guid id,
        Guid? tenantId,
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        DateTime? retentionExpiryDate) : base(id)
    {
        TenantId = tenantId;
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        ContentType = contentType;
        Status = DocumentStatus.Pending;
        RetryCount = 0;
        RetentionExpiryDate = retentionExpiryDate;
    }

    public static Document Create(
        Guid id,
        Guid? tenantId,
        string fileName,
        string filePath,
        long fileSize,
        string contentType,
        DateTime? retentionExpiryDate = null)
    {
        return new Document(id, tenantId, fileName, filePath, fileSize, contentType, retentionExpiryDate);
    }

    public void MarkAsClassifying()
    {
        if (Status != DocumentStatus.Pending && Status != DocumentStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot classify document in {Status} status");
        }
        Status = DocumentStatus.Classifying;
    }

    public void MarkAsClassified(string classification, string routingDestination)
    {
        if (Status != DocumentStatus.Classifying)
        {
            throw new InvalidOperationException($"Cannot mark as classified from {Status} status");
        }
        Status = DocumentStatus.Classified;
        Classification = classification;
        RoutingDestination = routingDestination;
        RetryCount = 0;
        LastRetryTime = null;
    }

    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
        RetryCount++;
        LastRetryTime = DateTime.UtcNow;
    }

    public void SendToDeadLetterQueue()
    {
        Status = DocumentStatus.DeadLetter;
    }

    public bool CanRetry(int maxRetries)
    {
        return RetryCount < maxRetries && Status == DocumentStatus.Failed;
    }

    public void MarkAsExpired()
    {
        Status = DocumentStatus.Expired;
    }
}

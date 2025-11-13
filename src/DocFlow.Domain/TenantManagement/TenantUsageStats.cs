using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.TenantManagement;

/// <summary>
/// Aggregate root representing usage statistics for a tenant's plan.
/// Tracks document count, storage usage, and rule count.
/// </summary>
public sealed class TenantUsageStats : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public DocumentCount DocumentCount { get; private set; }
    public StorageUsage StorageUsage { get; private set; }
    public RuleCount RuleCount { get; private set; }
    public DateTime LastUpdated { get; private set; }

    // Private constructor for EF Core
    private TenantUsageStats()
    {
        DocumentCount = null!;
        StorageUsage = null!;
        RuleCount = null!;
    }

    private TenantUsageStats(
        Guid id,
        Guid? tenantId)
    {
        Id = id;
        TenantId = tenantId;
        DocumentCount = DocumentCount.Create(0);
        StorageUsage = StorageUsage.Create(0);
        RuleCount = RuleCount.Create(0);
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create initial usage statistics.
    /// </summary>
    public static TenantUsageStats Create(Guid id, Guid? tenantId)
    {
        return new TenantUsageStats(id, tenantId);
    }

    /// <summary>
    /// Records a new document upload.
    /// </summary>
    public void RecordDocumentUpload(long fileSizeBytes)
    {
        if (fileSizeBytes < 0)
            throw new ArgumentException("File size cannot be negative", nameof(fileSizeBytes));

        DocumentCount = DocumentCount.Increment();
        StorageUsage = StorageUsage.Add(fileSizeBytes);
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a document deletion.
    /// </summary>
    public void RecordDocumentDeletion(long fileSizeBytes)
    {
        if (fileSizeBytes < 0)
            throw new ArgumentException("File size cannot be negative", nameof(fileSizeBytes));

        DocumentCount = DocumentCount.Decrement();
        StorageUsage = StorageUsage.Subtract(fileSizeBytes);
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a new rule creation.
    /// </summary>
    public void RecordRuleCreated()
    {
        RuleCount = RuleCount.Increment();
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a rule deletion.
    /// </summary>
    public void RecordRuleDeleted()
    {
        RuleCount = RuleCount.Decrement();
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Refreshes the usage statistics from actual counts.
    /// </summary>
    public void Refresh(int documentCount, long storageBytes, int ruleCount)
    {
        DocumentCount = DocumentCount.Create(documentCount);
        StorageUsage = StorageUsage.Create(storageBytes);
        RuleCount = RuleCount.Create(ruleCount);
        LastUpdated = DateTime.UtcNow;
    }
}

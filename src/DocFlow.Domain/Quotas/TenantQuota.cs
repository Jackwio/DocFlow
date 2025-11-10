using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DocFlow.Quotas;

public sealed class TenantQuota : AuditedAggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public int MaxDocuments { get; private set; }
    public long MaxStorageBytes { get; private set; }
    public int CurrentDocumentCount { get; private set; }
    public long CurrentStorageBytes { get; private set; }
    public bool IsBlocked { get; private set; }

    private TenantQuota()
    {
        // For EF Core
    }

    private TenantQuota(
        Guid id,
        Guid tenantId,
        int maxDocuments,
        long maxStorageBytes) : base(id)
    {
        TenantId = tenantId;
        MaxDocuments = maxDocuments;
        MaxStorageBytes = maxStorageBytes;
        CurrentDocumentCount = 0;
        CurrentStorageBytes = 0;
        IsBlocked = false;
    }

    public static TenantQuota Create(
        Guid id,
        Guid tenantId,
        int maxDocuments,
        long maxStorageBytes)
    {
        if (maxDocuments <= 0)
        {
            throw new ArgumentException("Max documents must be greater than zero", nameof(maxDocuments));
        }

        if (maxStorageBytes <= 0)
        {
            throw new ArgumentException("Max storage must be greater than zero", nameof(maxStorageBytes));
        }

        return new TenantQuota(id, tenantId, maxDocuments, maxStorageBytes);
    }

    public void UpdateUsage(int documentCount, long storageBytes)
    {
        CurrentDocumentCount = documentCount;
        CurrentStorageBytes = storageBytes;
        
        CheckAndUpdateBlockStatus();
    }

    public void UpdateLimits(int maxDocuments, long maxStorageBytes)
    {
        if (maxDocuments <= 0)
        {
            throw new ArgumentException("Max documents must be greater than zero", nameof(maxDocuments));
        }

        if (maxStorageBytes <= 0)
        {
            throw new ArgumentException("Max storage must be greater than zero", nameof(maxStorageBytes));
        }

        MaxDocuments = maxDocuments;
        MaxStorageBytes = maxStorageBytes;
        
        CheckAndUpdateBlockStatus();
    }

    public bool IsQuotaExceeded()
    {
        return CurrentDocumentCount >= MaxDocuments || CurrentStorageBytes >= MaxStorageBytes;
    }

    public void BlockUploads()
    {
        IsBlocked = true;
    }

    public void UnblockUploads()
    {
        IsBlocked = false;
    }

    private void CheckAndUpdateBlockStatus()
    {
        if (IsQuotaExceeded())
        {
            BlockUploads();
        }
        else if (!IsQuotaExceeded() && IsBlocked)
        {
            UnblockUploads();
        }
    }
}

using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DocFlow.Tenants;

public sealed class TenantBillingStatus : AuditedAggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public BillingStatus Status { get; private set; }
    public DateTime? PaymentFailureDate { get; private set; }
    public DateTime? GracePeriodEndDate { get; private set; }
    public int GracePeriodDays { get; private set; }

    private TenantBillingStatus()
    {
        // For EF Core
    }

    private TenantBillingStatus(
        Guid id,
        Guid tenantId,
        int gracePeriodDays = 7) : base(id)
    {
        TenantId = tenantId;
        Status = BillingStatus.Active;
        GracePeriodDays = gracePeriodDays;
    }

    public static TenantBillingStatus Create(
        Guid id,
        Guid tenantId,
        int gracePeriodDays = 7)
    {
        if (gracePeriodDays < 0)
        {
            throw new ArgumentException("Grace period days cannot be negative", nameof(gracePeriodDays));
        }

        return new TenantBillingStatus(id, tenantId, gracePeriodDays);
    }

    public void MarkPaymentFailed()
    {
        if (Status == BillingStatus.Active)
        {
            Status = BillingStatus.PaymentFailed;
            PaymentFailureDate = DateTime.UtcNow;
            GracePeriodEndDate = DateTime.UtcNow.AddDays(GracePeriodDays);
        }
    }

    public void MarkAsReadOnly()
    {
        if (Status == BillingStatus.PaymentFailed)
        {
            Status = BillingStatus.ReadOnly;
        }
    }

    public void RestoreToActive()
    {
        Status = BillingStatus.Active;
        PaymentFailureDate = null;
        GracePeriodEndDate = null;
    }

    public bool IsGracePeriodExpired()
    {
        return Status == BillingStatus.PaymentFailed && 
               GracePeriodEndDate.HasValue && 
               DateTime.UtcNow >= GracePeriodEndDate.Value;
    }
}

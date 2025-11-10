using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.Inboxes;

/// <summary>
/// Aggregate root representing an inbox for document intake.
/// Inboxes allow tenants to organize document sources by process or department.
/// </summary>
public sealed class Inbox : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public InboxName Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private Inbox()
    {
        Name = null!;
    }

    private Inbox(
        Guid id,
        Guid? tenantId,
        InboxName name,
        string? description)
    {
        Id = id;
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Factory method to create a new inbox.
    /// </summary>
    public static Inbox Create(
        Guid id,
        Guid? tenantId,
        InboxName name,
        string? description)
    {
        return new Inbox(id, tenantId, name, description);
    }

    /// <summary>
    /// Updates the inbox details.
    /// </summary>
    public void UpdateDetails(InboxName name, string? description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
    }

    /// <summary>
    /// Activates the inbox for document intake.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the inbox, preventing new document intake.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}

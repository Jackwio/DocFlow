using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Inboxes;

/// <summary>
/// Repository interface for Inbox aggregate.
/// </summary>
public interface IInboxRepository : IRepository<Inbox, Guid>
{
    /// <summary>
    /// Gets the count of active inboxes for a tenant.
    /// </summary>
    Task<int> GetActiveCountByTenantAsync(Guid? tenantId, CancellationToken cancellationToken = default);
}

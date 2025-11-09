using System;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Repository interface for RoutingQueue aggregate.
/// </summary>
public interface IRoutingQueueRepository : IRepository<RoutingQueue, Guid>
{
}

using System;
using DocFlow.EntityFrameworkCore;
using DocFlow.RoutingQueues;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.EntityFrameworkCore.RoutingQueues;

/// <summary>
/// EF Core repository implementation for RoutingQueue aggregate.
/// </summary>
public sealed class EfCoreRoutingQueueRepository : EfCoreRepository<DocFlowDbContext, RoutingQueue, Guid>, IRoutingQueueRepository
{
    public EfCoreRoutingQueueRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.EntityFrameworkCore;
using DocFlow.RoutingQueues;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.EntityFrameworkCore.RoutingQueues;

/// <summary>
/// EF Core repository implementation for WebhookDelivery entity.
/// </summary>
public sealed class EfCoreWebhookDeliveryRepository : EfCoreRepository<DocFlowDbContext, WebhookDelivery, Guid>, IWebhookDeliveryRepository
{
    public EfCoreWebhookDeliveryRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<WebhookDelivery>> FindFailedDeliveriesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.WebhookDeliveries
            .Where(d => d.Status == DeliveryStatus.Failed)
            .OrderBy(d => d.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.RoutingQueues;

/// <summary>
/// Repository interface for WebhookDelivery entity.
/// </summary>
public interface IWebhookDeliveryRepository : IRepository<WebhookDelivery, Guid>
{
    /// <summary>
    /// Finds all failed webhook deliveries that can be retried.
    /// </summary>
    Task<List<WebhookDelivery>> FindFailedDeliveriesAsync(CancellationToken cancellationToken = default);
}

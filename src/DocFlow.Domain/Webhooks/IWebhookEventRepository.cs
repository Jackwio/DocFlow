using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.Webhooks;

public interface IWebhookEventRepository : IRepository<WebhookEvent, Guid>
{
    Task<List<WebhookEvent>> GetPendingWebhooksAsync(int maxCount, CancellationToken cancellationToken = default);
}

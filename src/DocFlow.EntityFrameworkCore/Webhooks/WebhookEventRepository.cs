using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.Webhooks;

public class WebhookEventRepository : EfCoreRepository<DocFlowDbContext, WebhookEvent, Guid>, IWebhookEventRepository
{
    public WebhookEventRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<WebhookEvent>> GetPendingWebhooksAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<WebhookEvent>()
            .Where(w => w.Status == WebhookStatus.Pending)
            .OrderBy(w => w.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }
}

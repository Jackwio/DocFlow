using System;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.Quotas;

public class TenantQuotaRepository : EfCoreRepository<DocFlowDbContext, TenantQuota, Guid>, ITenantQuotaRepository
{
    public TenantQuotaRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<TenantQuota?> FindByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<TenantQuota>()
            .FirstOrDefaultAsync(q => q.TenantId == tenantId, cancellationToken);
    }
}

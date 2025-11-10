using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.Tenants;

public class TenantBillingStatusRepository : EfCoreRepository<DocFlowDbContext, TenantBillingStatus, Guid>, ITenantBillingStatusRepository
{
    public TenantBillingStatusRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<TenantBillingStatus?> FindByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<TenantBillingStatus>()
            .FirstOrDefaultAsync(b => b.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<TenantBillingStatus>> GetTenantsWithExpiredGracePeriodAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;
        return await dbContext.Set<TenantBillingStatus>()
            .Where(b => b.Status == BillingStatus.PaymentFailed && 
                       b.GracePeriodEndDate.HasValue && 
                       b.GracePeriodEndDate.Value <= now)
            .ToListAsync(cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFlow.ClassificationRules;
using DocFlow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DocFlow.EntityFrameworkCore.ClassificationRules;

/// <summary>
/// EF Core repository implementation for ClassificationRule aggregate.
/// </summary>
public sealed class EfCoreClassificationRuleRepository : EfCoreRepository<DocFlowDbContext, ClassificationRule, Guid>, IClassificationRuleRepository
{
    public EfCoreClassificationRuleRepository(IDbContextProvider<DocFlowDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<ClassificationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.ClassificationRules
            .Where(r => r.IsActive)
            .OrderBy(r => EF.Property<int>(r.Priority, "Value"))
            .ToListAsync(cancellationToken);
    }
}

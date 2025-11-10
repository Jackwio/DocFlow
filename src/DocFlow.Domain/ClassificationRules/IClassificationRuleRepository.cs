using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DocFlow.ClassificationRules;

/// <summary>
/// Repository interface for ClassificationRule aggregate.
/// </summary>
public interface IClassificationRuleRepository : IRepository<ClassificationRule, Guid>
{
    /// <summary>
    /// Gets all active classification rules ordered by priority.
    /// </summary>
    Task<List<ClassificationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
}

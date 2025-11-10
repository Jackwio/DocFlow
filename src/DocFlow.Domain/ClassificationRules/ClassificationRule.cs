using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocFlow.ClassificationRules.Events;
using DocFlow.Documents;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.ClassificationRules;

/// <summary>
/// Aggregate root representing a classification rule.
/// Rules are evaluated against documents to automatically assign tags.
/// </summary>
public sealed class ClassificationRule : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<TagName> _applyTags = new();
    private readonly List<RuleCondition> _conditions = new();

    public Guid? TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public RulePriority Priority { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<TagName> ApplyTags => _applyTags.AsReadOnly();
    public IReadOnlyCollection<RuleCondition> Conditions => _conditions.AsReadOnly();

    // Private constructor for EF Core
    private ClassificationRule()
    {
        Name = string.Empty;
        Priority = null!;
    }

    private ClassificationRule(
        Guid id,
        Guid? tenantId,
        string name,
        string? description,
        RulePriority priority,
        IEnumerable<TagName> applyTags,
        IEnumerable<RuleCondition> conditions)
    {
        Id = id;
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        IsActive = true;

        var tagList = applyTags?.ToList() ?? throw new ArgumentNullException(nameof(applyTags));
        if (!tagList.Any())
            throw new ArgumentException("At least one tag must be specified", nameof(applyTags));

        var conditionList = conditions?.ToList() ?? throw new ArgumentNullException(nameof(conditions));
        if (!conditionList.Any())
            throw new ArgumentException("At least one condition must be specified", nameof(conditions));

        _applyTags.AddRange(tagList);
        _conditions.AddRange(conditionList);
    }

    /// <summary>
    /// Factory method to define a new classification rule.
    /// </summary>
    public static ClassificationRule DefineRule(
        Guid id,
        Guid? tenantId,
        string name,
        string? description,
        RulePriority priority,
        IEnumerable<TagName> applyTags,
        IEnumerable<RuleCondition> conditions)
    {
        var rule = new ClassificationRule(id, tenantId, name, description, priority, applyTags, conditions);

        rule.AddLocalEvent(new ClassificationRuleCreatedEvent(id, name));

        return rule;
    }

    /// <summary>
    /// Updates the conditions for this rule.
    /// </summary>
    public void UpdateConditions(IEnumerable<RuleCondition> conditions)
    {
        if (conditions == null) throw new ArgumentNullException(nameof(conditions));

        var conditionList = conditions.ToList();
        if (!conditionList.Any())
            throw new ArgumentException("At least one condition must be specified", nameof(conditions));

        _conditions.Clear();
        _conditions.AddRange(conditionList);

        AddLocalEvent(new ClassificationRuleUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Updates the tags to apply when this rule matches.
    /// </summary>
    public void UpdateTags(IEnumerable<TagName> tags)
    {
        if (tags == null) throw new ArgumentNullException(nameof(tags));

        var tagList = tags.ToList();
        if (!tagList.Any())
            throw new ArgumentException("At least one tag must be specified", nameof(tags));

        _applyTags.Clear();
        _applyTags.AddRange(tagList);

        AddLocalEvent(new ClassificationRuleUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Updates the priority of this rule.
    /// </summary>
    public void UpdatePriority(RulePriority priority)
    {
        if (priority == null) throw new ArgumentNullException(nameof(priority));

        Priority = priority;

        AddLocalEvent(new ClassificationRuleUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Activates the rule for use in classification.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return; // Already active

        IsActive = true;

        AddLocalEvent(new ClassificationRuleActivatedEvent(Id, Name));
    }

    /// <summary>
    /// Deactivates the rule, preventing it from being used in classification.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return; // Already inactive

        IsActive = false;

        AddLocalEvent(new ClassificationRuleDeactivatedEvent(Id, Name));
    }

    /// <summary>
    /// Evaluates if this rule matches the given document.
    /// This is a placeholder - actual matching logic would be implemented in domain service.
    /// </summary>
    public Task<bool> MatchesAsync(Document document)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (!IsActive) return Task.FromResult(false);

        // Actual matching logic would be in a domain service (ClassificationRuleManager)
        // This is a placeholder that always returns false
        return Task.FromResult(false);
    }
}

using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.ClassificationRules.Events;

/// <summary>
/// Domain event raised when a new classification rule is created.
/// </summary>
[Serializable]
public sealed class ClassificationRuleCreatedEvent : EntityCreatedEventData<Guid>
{
    public string RuleName { get; }

    public ClassificationRuleCreatedEvent(Guid ruleId, string ruleName) : base(ruleId)
    {
        RuleName = ruleName;
    }
}

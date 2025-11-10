using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.ClassificationRules.Events;

/// <summary>
/// Domain event raised when a classification rule is updated.
/// </summary>
[Serializable]
public sealed class ClassificationRuleUpdatedEvent : EntityChangedEventData<Guid>
{
    public string RuleName { get; }

    public ClassificationRuleUpdatedEvent(Guid ruleId, string ruleName) : base(ruleId)
    {
        RuleName = ruleName;
    }
}

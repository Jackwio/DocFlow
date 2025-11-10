using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.ClassificationRules.Events;

/// <summary>
/// Domain event raised when a classification rule is deactivated.
/// </summary>
[Serializable]
public sealed class ClassificationRuleDeactivatedEvent : EntityChangedEventData<Guid>
{
    public string RuleName { get; }

    public ClassificationRuleDeactivatedEvent(Guid ruleId, string ruleName) : base(ruleId)
    {
        RuleName = ruleName;
    }
}

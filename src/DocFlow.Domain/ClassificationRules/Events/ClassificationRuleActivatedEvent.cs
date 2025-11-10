using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.ClassificationRules.Events;

/// <summary>
/// Domain event raised when a classification rule is activated.
/// </summary>
[Serializable]
public sealed class ClassificationRuleActivatedEvent : EntityChangedEventData<Guid>
{
    public string RuleName { get; }

    public ClassificationRuleActivatedEvent(Guid ruleId, string ruleName) : base(ruleId)
    {
        RuleName = ruleName;
    }
}

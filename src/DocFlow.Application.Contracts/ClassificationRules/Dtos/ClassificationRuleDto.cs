using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace DocFlow.ClassificationRules.Dtos;

/// <summary>
/// DTO for classification rule with full details.
/// </summary>
public sealed class ClassificationRuleDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public List<string> ApplyTags { get; set; } = new();
    public List<RuleConditionDto> Conditions { get; set; } = new();
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// DTO for rule condition.
/// </summary>
public sealed class RuleConditionDto
{
    public string Type { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string? MatchValue { get; set; }
}

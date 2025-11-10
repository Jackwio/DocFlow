using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DocFlow.ClassificationRules.Dtos;

/// <summary>
/// DTO for updating a classification rule.
/// </summary>
public sealed class UpdateClassificationRuleDto
{
    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 1000)]
    public int? Priority { get; set; }

    public List<string>? ApplyTags { get; set; }

    public List<RuleConditionDto>? Conditions { get; set; }
}

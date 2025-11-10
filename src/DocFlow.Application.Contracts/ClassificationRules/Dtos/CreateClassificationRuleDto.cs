using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DocFlow.ClassificationRules.Dtos;

/// <summary>
/// DTO for creating a classification rule.
/// </summary>
public sealed class CreateClassificationRuleDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 1000)]
    public int Priority { get; set; } = 500;

    [Required]
    [MinLength(1)]
    public List<string> ApplyTags { get; set; } = new();

    [Required]
    [MinLength(1)]
    public List<RuleConditionDto> Conditions { get; set; } = new();
}

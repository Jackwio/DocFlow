using System;
using System.Collections.Generic;

namespace DocFlow.ClassificationRules.Dtos;

/// <summary>
/// DTO for dry-run classification result.
/// </summary>
public sealed class DryRunResultDto
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public bool Matches { get; set; }
    public List<string> MatchedConditions { get; set; } = new();
    public List<string> TagsToApply { get; set; } = new();
}

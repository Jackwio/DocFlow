using System;
using Volo.Abp.Application.Dtos;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO for classification history entry.
/// </summary>
public sealed class ClassificationHistoryDto : EntityDto<Guid>
{
    public Guid RuleId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string MatchedCondition { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public DateTime MatchedAt { get; set; }
}

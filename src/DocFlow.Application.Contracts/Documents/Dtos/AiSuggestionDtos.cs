using System;
using System.Collections.Generic;

namespace DocFlow.Documents.Dtos;

/// <summary>
/// DTO representing AI-generated suggestions for document classification.
/// </summary>
public class AiSuggestionDto
{
    public List<SuggestedTagDto> SuggestedTags { get; set; } = new();
    public Guid? SuggestedQueueId { get; set; }
    public double Confidence { get; set; }
    public string? Summary { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO representing a single suggested tag.
/// </summary>
public class SuggestedTagDto
{
    public string TagName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Reasoning { get; set; }
}

/// <summary>
/// DTO for requesting document summary generation.
/// </summary>
public class GenerateSummaryDto
{
    public Guid DocumentId { get; set; }
    public int? MaxLength { get; set; }
}

/// <summary>
/// DTO for document summary response.
/// </summary>
public class DocumentSummaryDto
{
    public Guid DocumentId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO for applying AI suggestions to a document.
/// </summary>
public class ApplyAiSuggestionsDto
{
    public Guid DocumentId { get; set; }
}

/// <summary>
/// DTO for tenant AI settings.
/// </summary>
public class TenantAiSettingsDto
{
    public bool AiEnabled { get; set; }
    public bool AutoApplyAiSuggestions { get; set; }
    public string? OpenAiApiKey { get; set; }
    public string? OpenAiModel { get; set; }
    public double MinConfidenceThreshold { get; set; }
}

/// <summary>
/// DTO for updating tenant AI settings.
/// </summary>
public class UpdateTenantAiSettingsDto
{
    public bool AiEnabled { get; set; }
    public bool AutoApplyAiSuggestions { get; set; }
    public string? OpenAiApiKey { get; set; }
    public string? OpenAiModel { get; set; }
    public double MinConfidenceThreshold { get; set; }
}

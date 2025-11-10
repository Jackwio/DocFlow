using System;
using System.Collections.Generic;
using System.Linq;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing AI-generated suggestions for document classification.
/// Stores suggested tags, recommended queue, and confidence scores.
/// </summary>
public sealed class AiSuggestion
{
    public IReadOnlyList<SuggestedTag> SuggestedTags { get; private set; }
    public Guid? SuggestedQueueId { get; private set; }
    public ConfidenceScore Confidence { get; private set; }
    public string? Summary { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    // Private constructor for EF Core
    private AiSuggestion()
    {
        SuggestedTags = new List<SuggestedTag>();
        Confidence = null!;
    }

    private AiSuggestion(
        IReadOnlyList<SuggestedTag> suggestedTags,
        Guid? suggestedQueueId,
        ConfidenceScore confidence,
        string? summary,
        DateTime generatedAt)
    {
        SuggestedTags = suggestedTags ?? throw new ArgumentNullException(nameof(suggestedTags));
        SuggestedQueueId = suggestedQueueId;
        Confidence = confidence ?? throw new ArgumentNullException(nameof(confidence));
        Summary = summary;
        GeneratedAt = generatedAt;
    }

    /// <summary>
    /// Creates a new AI suggestion.
    /// </summary>
    public static AiSuggestion Create(
        IReadOnlyList<SuggestedTag> suggestedTags,
        Guid? suggestedQueueId,
        ConfidenceScore confidence,
        string? summary = null)
    {
        if (suggestedTags == null || !suggestedTags.Any())
            throw new ArgumentException("At least one suggested tag is required", nameof(suggestedTags));

        return new AiSuggestion(
            suggestedTags,
            suggestedQueueId,
            confidence,
            summary,
            DateTime.UtcNow);
    }
}

/// <summary>
/// Represents a single tag suggestion with confidence score.
/// </summary>
public sealed class SuggestedTag
{
    public TagName TagName { get; private set; }
    public ConfidenceScore Confidence { get; private set; }
    public string? Reasoning { get; private set; }

    // Private constructor for EF Core
    private SuggestedTag()
    {
        TagName = null!;
        Confidence = null!;
    }

    private SuggestedTag(TagName tagName, ConfidenceScore confidence, string? reasoning)
    {
        TagName = tagName ?? throw new ArgumentNullException(nameof(tagName));
        Confidence = confidence ?? throw new ArgumentNullException(nameof(confidence));
        Reasoning = reasoning;
    }

    /// <summary>
    /// Creates a new suggested tag.
    /// </summary>
    public static SuggestedTag Create(TagName tagName, ConfidenceScore confidence, string? reasoning = null)
    {
        return new SuggestedTag(tagName, confidence, reasoning);
    }
}

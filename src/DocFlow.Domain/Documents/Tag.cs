using System;
using System.Linq;
using DocFlow.Enums;

namespace DocFlow.Documents;

/// <summary>
/// Entity representing a classification tag applied to a document.
/// Tags can be applied automatically by rules, manually by operators, or by AI.
/// </summary>
public sealed class Tag
{
    public TagName Name { get; private set; }
    public TagSource Source { get; private set; }
    public ConfidenceScore? Confidence { get; private set; }

    // Private constructor for EF Core
    private Tag()
    {
        Name = null!;
    }

    private Tag(TagName name, TagSource source, ConfidenceScore? confidence = null)
    {
        Name = name;
        Source = source;
        Confidence = confidence;
    }

    /// <summary>
    /// Creates a new Tag from automatic classification.
    /// </summary>
    public static Tag CreateAutomatic(TagName name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new Tag(name, TagSource.Automatic);
    }

    /// <summary>
    /// Creates a new Tag from manual operator input.
    /// </summary>
    public static Tag CreateManual(TagName name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new Tag(name, TagSource.Manual);
    }

    /// <summary>
    /// Creates a new Tag from AI suggestion (suggested but not yet applied).
    /// </summary>
    public static Tag CreateAiSuggested(TagName name, ConfidenceScore confidence)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (confidence == null) throw new ArgumentNullException(nameof(confidence));
        return new Tag(name, TagSource.AiSuggested, confidence);
    }

    /// <summary>
    /// Creates a new Tag from applied AI suggestion.
    /// </summary>
    public static Tag CreateAiApplied(TagName name, ConfidenceScore confidence)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (confidence == null) throw new ArgumentNullException(nameof(confidence));
        return new Tag(name, TagSource.AiApplied, confidence);
    }
}

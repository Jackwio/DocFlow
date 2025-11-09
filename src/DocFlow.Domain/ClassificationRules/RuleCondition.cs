using System;
using DocFlow.Enums;

namespace DocFlow.ClassificationRules;

/// <summary>
/// Value object representing a condition to match against a document.
/// </summary>
public sealed record RuleCondition
{
    public RuleConditionType Type { get; }
    public string Pattern { get; }
    public string? MatchValue { get; }

    private RuleCondition(RuleConditionType type, string pattern, string? matchValue)
    {
        Type = type;
        Pattern = pattern;
        MatchValue = matchValue;
    }

    /// <summary>
    /// Creates a filename regex condition.
    /// </summary>
    public static RuleCondition CreateFileNameRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Regex pattern cannot be empty", nameof(pattern));

        return new RuleCondition(RuleConditionType.FileNameRegex, pattern, null);
    }

    /// <summary>
    /// Creates a MIME type condition.
    /// </summary>
    public static RuleCondition CreateMimeType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type cannot be empty", nameof(mimeType));

        return new RuleCondition(RuleConditionType.MimeType, mimeType, null);
    }

    /// <summary>
    /// Creates a file size condition.
    /// </summary>
    public static RuleCondition CreateFileSize(long minBytes, long maxBytes)
    {
        if (minBytes < 0)
            throw new ArgumentException("Minimum size cannot be negative", nameof(minBytes));
        if (maxBytes < minBytes)
            throw new ArgumentException("Maximum size cannot be less than minimum", nameof(maxBytes));

        return new RuleCondition(RuleConditionType.FileSize, $"{minBytes}-{maxBytes}", null);
    }

    /// <summary>
    /// Creates a text content condition.
    /// </summary>
    public static RuleCondition CreateTextContent(string textSnippet)
    {
        if (string.IsNullOrWhiteSpace(textSnippet))
            throw new ArgumentException("Text snippet cannot be empty", nameof(textSnippet));

        return new RuleCondition(RuleConditionType.TextContent, textSnippet, null);
    }
}

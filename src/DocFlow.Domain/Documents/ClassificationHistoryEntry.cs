using System;

namespace DocFlow.Documents;

/// <summary>
/// Entity representing a record of a classification rule match.
/// Used for audit trail and understanding classification decisions.
/// </summary>
public sealed class ClassificationHistoryEntry
{
    public Guid RuleId { get; private set; }
    public TagName TagName { get; private set; }
    public string MatchedCondition { get; private set; }
    public ConfidenceScore ConfidenceScore { get; private set; }
    public DateTime MatchedAt { get; private set; }

    // Private constructor for EF Core
    private ClassificationHistoryEntry()
    {
        TagName = null!;
        MatchedCondition = string.Empty;
        ConfidenceScore = null!;
    }

    private ClassificationHistoryEntry(
        Guid ruleId,
        TagName tagName,
        string matchedCondition,
        ConfidenceScore confidenceScore,
        DateTime matchedAt)
    {
        RuleId = ruleId;
        TagName = tagName;
        MatchedCondition = matchedCondition;
        ConfidenceScore = confidenceScore;
        MatchedAt = matchedAt;
    }

    /// <summary>
    /// Creates a new classification history entry.
    /// </summary>
    public static ClassificationHistoryEntry Create(
        Guid ruleId,
        TagName tagName,
        string matchedCondition,
        ConfidenceScore confidenceScore)
    {
        if (ruleId == Guid.Empty) throw new ArgumentException("Rule ID cannot be empty", nameof(ruleId));
        if (tagName == null) throw new ArgumentNullException(nameof(tagName));
        if (string.IsNullOrWhiteSpace(matchedCondition))
            throw new ArgumentException("Matched condition cannot be empty", nameof(matchedCondition));
        if (confidenceScore == null) throw new ArgumentNullException(nameof(confidenceScore));

        return new ClassificationHistoryEntry(
            ruleId,
            tagName,
            matchedCondition,
            confidenceScore,
            DateTime.UtcNow);
    }
}

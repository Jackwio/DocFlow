using System;

namespace DocFlow.ClassificationRules;

/// <summary>
/// Value object representing the priority order for rule evaluation.
/// Lower number = higher priority (evaluated first).
/// </summary>
public sealed record RulePriority
{
    private const int MinPriority = 1;
    private const int MaxPriority = 1000;

    public int Value { get; }

    private RulePriority(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new RulePriority with validation.
    /// </summary>
    public static RulePriority Create(int value)
    {
        if (value < MinPriority || value > MaxPriority)
            throw new ArgumentException($"Priority must be between {MinPriority} and {MaxPriority}", nameof(value));

        return new RulePriority(value);
    }

    /// <summary>
    /// Creates the default priority (middle of range).
    /// </summary>
    public static RulePriority Default() => new(500);

    public override string ToString() => Value.ToString();
}

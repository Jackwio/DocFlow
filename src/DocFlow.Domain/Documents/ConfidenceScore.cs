using System;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a confidence score for classification results.
/// Score must be between 0.0 and 1.0 inclusive.
/// </summary>
public sealed record ConfidenceScore
{
    private const double MinScore = 0.0;
    private const double MaxScore = 1.0;

    public double Value { get; }

    private ConfidenceScore(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ConfidenceScore value object with validation.
    /// </summary>
    /// <param name="value">The confidence score (0.0 to 1.0).</param>
    /// <returns>A validated ConfidenceScore instance.</returns>
    /// <exception cref="ArgumentException">Thrown when score is outside valid range.</exception>
    public static ConfidenceScore Create(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Confidence score must be a valid number", nameof(value));

        if (value < MinScore || value > MaxScore)
            throw new ArgumentException($"Confidence score must be between {MinScore} and {MaxScore}", nameof(value));

        return new ConfidenceScore(value);
    }

    /// <summary>
    /// Creates a ConfidenceScore representing perfect confidence (1.0).
    /// </summary>
    public static ConfidenceScore Perfect() => new(MaxScore);

    /// <summary>
    /// Creates a ConfidenceScore representing no confidence (0.0).
    /// </summary>
    public static ConfidenceScore None() => new(MinScore);

    public override string ToString() => $"{Value:P0}";
}

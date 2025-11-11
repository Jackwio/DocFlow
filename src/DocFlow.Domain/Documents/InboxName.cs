using System;
using System.Linq;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing an inbox/category name with validation.
/// </summary>
public sealed record InboxName
{
    private const int MaxLength = 100;

    public string Value { get; }

    private InboxName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new InboxName value object with validation.
    /// </summary>
    /// <param name="value">The inbox name to validate.</param>
    /// <returns>A validated InboxName instance.</returns>
    /// <exception cref="ArgumentException">Thrown when inbox name is invalid.</exception>
    public static InboxName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Inbox name cannot be empty or whitespace", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"Inbox name cannot exceed {MaxLength} characters", nameof(value));

        var trimmed = value.Trim();
        
        return new InboxName(trimmed);
    }

    public override string ToString() => Value;
}

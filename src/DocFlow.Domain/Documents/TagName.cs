using System;
using System.Linq;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a classification tag name with validation.
/// </summary>
public sealed record TagName
{
    private const int MaxLength = 50;

    public string Value { get; }

    private TagName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new TagName value object with validation.
    /// </summary>
    /// <param name="value">The tag name to validate.</param>
    /// <returns>A validated TagName instance.</returns>
    /// <exception cref="ArgumentException">Thrown when tag name is invalid.</exception>
    public static TagName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tag name cannot be empty or whitespace", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"Tag name cannot exceed {MaxLength} characters", nameof(value));

        var trimmed = value.Trim();
        if (ContainsInvalidCharacters(trimmed))
            throw new ArgumentException("Tag name contains invalid characters. Only letters, numbers, hyphens, and underscores are allowed", nameof(value));

        return new TagName(trimmed);
    }

    private static bool ContainsInvalidCharacters(string tagName)
    {
        return !tagName.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    public override string ToString() => Value;
}

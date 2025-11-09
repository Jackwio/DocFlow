using System;

namespace DocFlow.Shared;

/// <summary>
/// Value object representing an error message with validation.
/// </summary>
public sealed record ErrorMessage
{
    private const int MaxLength = 1000;

    public string Value { get; }

    private ErrorMessage(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ErrorMessage value object with validation.
    /// </summary>
    /// <param name="value">The error message text.</param>
    /// <returns>A validated ErrorMessage instance.</returns>
    /// <exception cref="ArgumentException">Thrown when error message is invalid.</exception>
    public static ErrorMessage Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Error message cannot be empty or whitespace", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"Error message cannot exceed {MaxLength} characters", nameof(value));

        return new ErrorMessage(value);
    }

    public override string ToString() => Value;
}

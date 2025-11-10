using System;
using System.IO;
using System.Linq;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a validated filename.
/// Ensures filename meets security and format requirements.
/// </summary>
public sealed record FileName
{
    private const int MaxLength = 255;
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public string Value { get; }

    private FileName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new FileName value object with validation.
    /// </summary>
    /// <param name="value">The filename to validate.</param>
    /// <returns>A validated FileName instance.</returns>
    /// <exception cref="ArgumentException">Thrown when filename is invalid.</exception>
    public static FileName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Filename cannot be empty or whitespace", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"Filename cannot exceed {MaxLength} characters", nameof(value));

        if (ContainsInvalidCharacters(value))
            throw new ArgumentException("Filename contains invalid characters", nameof(value));

        if (IsSuspiciousFileName(value))
            throw new ArgumentException("Filename appears suspicious or potentially malicious", nameof(value));

        return new FileName(value);
    }

    private static bool ContainsInvalidCharacters(string filename)
    {
        return filename.IndexOfAny(InvalidFileNameChars) >= 0;
    }

    private static bool IsSuspiciousFileName(string filename)
    {
        var suspicious = new[] { "..", "~", "$" };
        return suspicious.Any(pattern => filename.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString() => Value;
}

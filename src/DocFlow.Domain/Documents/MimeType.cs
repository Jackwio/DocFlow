using System;
using System.Collections.Generic;
using System.Linq;

namespace DocFlow.Documents;

/// <summary>
/// Value object representing a MIME type with whitelist validation.
/// Only allows approved document formats.
/// </summary>
public sealed record MimeType
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/tiff",
        "image/tif"
    };

    public string Value { get; }

    private MimeType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new MimeType value object with whitelist validation.
    /// </summary>
    /// <param name="value">The MIME type to validate.</param>
    /// <returns>A validated MimeType instance.</returns>
    /// <exception cref="ArgumentException">Thrown when MIME type is not in the whitelist.</exception>
    public static MimeType Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MIME type cannot be empty or whitespace", nameof(value));

        if (!AllowedMimeTypes.Contains(value))
            throw new ArgumentException(
                $"MIME type '{value}' is not supported. Supported types: {string.Join(", ", AllowedMimeTypes)}",
                nameof(value));

        return new MimeType(value);
    }

    /// <summary>
    /// Gets the list of all supported MIME types.
    /// </summary>
    public static IReadOnlyCollection<string> GetSupportedMimeTypes() => AllowedMimeTypes;

    public override string ToString() => Value;
}

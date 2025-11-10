using System;
using Volo.Abp.Domain.Values;

namespace DocFlow.TenantManagement;

/// <summary>
/// Value object representing tag color configuration.
/// </summary>
public sealed class TagColorConfiguration : ValueObject
{
    public string TagName { get; }
    public string ColorHex { get; }

    private TagColorConfiguration(string tagName, string colorHex)
    {
        TagName = tagName;
        ColorHex = colorHex;
    }

    /// <summary>
    /// Factory method to create tag color configuration with validation.
    /// </summary>
    public static TagColorConfiguration Create(string tagName, string colorHex)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty", nameof(tagName));

        if (string.IsNullOrWhiteSpace(colorHex))
            throw new ArgumentException("Color hex cannot be empty", nameof(colorHex));

        // Validate hex color format
        var trimmedColor = colorHex.Trim();
        if (!trimmedColor.StartsWith('#'))
            trimmedColor = "#" + trimmedColor;

        if (trimmedColor.Length != 7 || !System.Text.RegularExpressions.Regex.IsMatch(trimmedColor, "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException("Color must be a valid 6-digit hex color (e.g., #FF5733)", nameof(colorHex));

        return new TagColorConfiguration(tagName.Trim(), trimmedColor.ToUpperInvariant());
    }

    protected override System.Collections.Generic.IEnumerable<object> GetAtomicValues()
    {
        yield return TagName;
        yield return ColorHex;
    }

    public override string ToString() => $"{TagName}: {ColorHex}";
}

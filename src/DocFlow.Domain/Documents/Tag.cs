using System;
using System.Linq;
using DocFlow.Enums;

namespace DocFlow.Documents;

/// <summary>
/// Entity representing a classification tag applied to a document.
/// Tags can be applied automatically by rules or manually by operators.
/// </summary>
public sealed class Tag
{
    public TagName Name { get; private set; }
    public TagSource Source { get; private set; }

    // Private constructor for EF Core
    private Tag()
    {
        Name = null!;
    }

    private Tag(TagName name, TagSource source)
    {
        Name = name;
        Source = source;
    }

    /// <summary>
    /// Creates a new Tag from automatic classification.
    /// </summary>
    public static Tag CreateAutomatic(TagName name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new Tag(name, TagSource.Automatic);
    }

    /// <summary>
    /// Creates a new Tag from manual operator input.
    /// </summary>
    public static Tag CreateManual(TagName name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new Tag(name, TagSource.Manual);
    }
}

using DocFlow.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for TagName value object.
/// </summary>
public sealed class TagNameConverter : ValueConverter<TagName, string>
{
    public TagNameConverter()
        : base(
            v => v.Value,
            v => TagName.Create(v))
    {
    }
}

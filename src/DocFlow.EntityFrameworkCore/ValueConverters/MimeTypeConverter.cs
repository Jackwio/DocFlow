using DocFlow.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for MimeType value object.
/// </summary>
public sealed class MimeTypeConverter : ValueConverter<MimeType, string>
{
    public MimeTypeConverter()
        : base(
            v => v.Value,
            v => MimeType.Create(v))
    {
    }
}

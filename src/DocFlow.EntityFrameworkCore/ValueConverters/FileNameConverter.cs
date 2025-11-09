using DocFlow.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for FileName value object.
/// </summary>
public sealed class FileNameConverter : ValueConverter<FileName, string>
{
    public FileNameConverter()
        : base(
            v => v.Value,
            v => FileName.Create(v))
    {
    }
}

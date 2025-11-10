using DocFlow.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for FileSize value object.
/// </summary>
public sealed class FileSizeConverter : ValueConverter<FileSize, long>
{
    public FileSizeConverter()
        : base(
            v => v.Bytes,
            v => FileSize.Create(v))
    {
    }
}

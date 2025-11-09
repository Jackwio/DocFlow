using DocFlow.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for ErrorMessage value object.
/// </summary>
public sealed class ErrorMessageConverter : ValueConverter<ErrorMessage, string>
{
    public ErrorMessageConverter()
        : base(
            v => v.Value,
            v => ErrorMessage.Create(v))
    {
    }
}

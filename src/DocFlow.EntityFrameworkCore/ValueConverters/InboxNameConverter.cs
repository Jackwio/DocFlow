using DocFlow.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DocFlow.EntityFrameworkCore.ValueConverters;

/// <summary>
/// EF Core value converter for InboxName value object.
/// </summary>
public sealed class InboxNameConverter : ValueConverter<InboxName?, string?>
{
    public InboxNameConverter()
        : base(
            v => v != null ? v.Value : null,
            v => v != null ? InboxName.Create(v) : null)
    {
    }
}

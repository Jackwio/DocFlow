using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when an operator manually adds a tag to a document.
/// </summary>
[Serializable]
public sealed class ManualTagAddedEvent : EntityChangedEventData<Guid>
{
    public TagName TagName { get; }

    public ManualTagAddedEvent(Guid documentId, TagName tagName) : base(documentId)
    {
        TagName = tagName;
    }
}

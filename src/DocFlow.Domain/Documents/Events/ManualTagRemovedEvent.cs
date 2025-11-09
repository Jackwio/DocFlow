using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when an operator manually removes a tag from a document.
/// </summary>
[Serializable]
public sealed class ManualTagRemovedEvent : EntityChangedEventData<Guid>
{
    public TagName TagName { get; }

    public ManualTagRemovedEvent(Guid documentId, TagName tagName) : base(documentId)
    {
        TagName = tagName;
    }
}

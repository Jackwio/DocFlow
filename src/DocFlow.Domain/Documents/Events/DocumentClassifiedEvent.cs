using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when a document has been successfully classified.
/// Triggers routing workflow.
/// </summary>
[Serializable]
public sealed class DocumentClassifiedEvent : EntityChangedEventData<Guid>
{
    public IReadOnlyCollection<TagName> AppliedTags { get; }

    public DocumentClassifiedEvent(Guid documentId, IReadOnlyCollection<TagName> appliedTags) : base(documentId)
    {
        AppliedTags = appliedTags;
    }
}

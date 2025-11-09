using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when a document has been routed to its destination.
/// </summary>
[Serializable]
public sealed class DocumentRoutedEvent : EntityChangedEventData<Guid>
{
    public Guid QueueId { get; }

    public DocumentRoutedEvent(Guid documentId, Guid queueId) : base(documentId)
    {
        QueueId = queueId;
    }
}

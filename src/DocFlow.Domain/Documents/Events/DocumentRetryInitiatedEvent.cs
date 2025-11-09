using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when an operator initiates a retry for a failed document.
/// </summary>
[Serializable]
public sealed class DocumentRetryInitiatedEvent : EntityChangedEventData<Guid>
{
    public DocumentRetryInitiatedEvent(Guid documentId) : base(documentId)
    {
    }
}

using System;
using DocFlow.Shared;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when document classification fails.
/// Allows operators to identify and retry failed documents.
/// </summary>
[Serializable]
public sealed class DocumentClassificationFailedEvent : EntityChangedEventData<Guid>
{
    public ErrorMessage ErrorMessage { get; }

    public DocumentClassificationFailedEvent(Guid documentId, ErrorMessage errorMessage) : base(documentId)
    {
        ErrorMessage = errorMessage;
    }
}

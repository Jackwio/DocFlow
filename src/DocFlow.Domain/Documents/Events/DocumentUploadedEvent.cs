using System;
using Volo.Abp.Domain.Entities.Events;

namespace DocFlow.Documents.Events;

/// <summary>
/// Domain event raised when a new document is uploaded to the system.
/// Triggers the classification workflow.
/// </summary>
[Serializable]
public sealed class DocumentUploadedEvent : EntityCreatedEventData<Guid>
{
    public FileName FileName { get; }
    public FileSize FileSize { get; }
    public MimeType MimeType { get; }
    public BlobReference BlobReference { get; }

    public DocumentUploadedEvent(
        Guid documentId,
        FileName fileName,
        FileSize fileSize,
        MimeType mimeType,
        BlobReference blobReference) : base(documentId)
    {
        FileName = fileName;
        FileSize = fileSize;
        MimeType = mimeType;
        BlobReference = blobReference;
    }
}

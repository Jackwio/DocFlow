using System;
using System.Collections.Generic;
using System.Linq;
using DocFlow.Documents.Events;
using DocFlow.Enums;
using DocFlow.Shared;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace DocFlow.Documents;

/// <summary>
/// Aggregate root representing a document in the system.
/// Manages document lifecycle from upload through classification to routing.
/// </summary>
public sealed class Document : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<Tag> _tags = new();
    private readonly List<ClassificationHistoryEntry> _classificationHistory = new();

    public Guid? TenantId { get; private set; }
    public FileName FileName { get; private set; }
    public FileSize FileSize { get; private set; }
    public MimeType MimeType { get; private set; }
    public BlobReference BlobReference { get; private set; }
    public DocumentStatus Status { get; private set; }
    public ErrorMessage? LastError { get; private set; }
    public Guid? RoutedToQueueId { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<ClassificationHistoryEntry> ClassificationHistory => _classificationHistory.AsReadOnly();

    // Private constructor for EF Core
    private Document()
    {
        FileName = null!;
        FileSize = null!;
        MimeType = null!;
        BlobReference = null!;
    }

    private Document(
        Guid id,
        Guid? tenantId,
        FileName fileName,
        FileSize fileSize,
        MimeType mimeType,
        BlobReference blobReference)
    {
        Id = id;
        TenantId = tenantId;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FileSize = fileSize ?? throw new ArgumentNullException(nameof(fileSize));
        MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
        BlobReference = blobReference ?? throw new ArgumentNullException(nameof(blobReference));
        Status = DocumentStatus.Pending;
    }

    /// <summary>
    /// Factory method to register a new document upload.
    /// </summary>
    public static Document RegisterUpload(
        Guid id,
        Guid? tenantId,
        FileName fileName,
        FileSize fileSize,
        MimeType mimeType,
        BlobReference blobReference)
    {
        var document = new Document(id, tenantId, fileName, fileSize, mimeType, blobReference);
        
        document.AddLocalEvent(new DocumentUploadedEvent(
            id,
            fileName,
            fileSize,
            mimeType,
            blobReference));

        return document;
    }

    /// <summary>
    /// Applies classification results to the document.
    /// </summary>
    public void ApplyClassificationResult(IEnumerable<Tag> tags, IEnumerable<ClassificationHistoryEntry> historyEntries)
    {
        if (tags == null) throw new ArgumentNullException(nameof(tags));
        if (historyEntries == null) throw new ArgumentNullException(nameof(historyEntries));

        if (Status != DocumentStatus.Pending)
            throw new InvalidOperationException($"Cannot classify document in status {Status}. Expected Pending.");

        var tagList = tags.ToList();
        if (!tagList.Any())
            throw new ArgumentException("At least one tag must be applied during classification", nameof(tags));

        _tags.AddRange(tagList);
        _classificationHistory.AddRange(historyEntries);
        Status = DocumentStatus.Classified;
        LastError = null;

        AddLocalEvent(new DocumentClassifiedEvent(Id, _tags.Select(t => t.Name).ToList()));
    }

    /// <summary>
    /// Marks document as routed to a destination queue.
    /// </summary>
    public void MarkAsRouted(Guid queueId)
    {
        if (queueId == Guid.Empty)
            throw new ArgumentException("Queue ID cannot be empty", nameof(queueId));

        if (Status != DocumentStatus.Classified)
            throw new InvalidOperationException($"Cannot route document in status {Status}. Expected Classified.");

        RoutedToQueueId = queueId;
        Status = DocumentStatus.Routed;

        AddLocalEvent(new DocumentRoutedEvent(Id, queueId));
    }

    /// <summary>
    /// Records a classification failure with error details.
    /// </summary>
    public void RecordClassificationFailure(ErrorMessage errorMessage)
    {
        if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

        Status = DocumentStatus.Failed;
        LastError = errorMessage;

        AddLocalEvent(new DocumentClassificationFailedEvent(Id, errorMessage));
    }

    /// <summary>
    /// Resets document to retry classification after a failure.
    /// </summary>
    public void RetryClassification()
    {
        if (Status != DocumentStatus.Failed)
            throw new InvalidOperationException($"Cannot retry document in status {Status}. Expected Failed.");

        _tags.Clear();
        _classificationHistory.Clear();
        Status = DocumentStatus.Pending;
        LastError = null;
        RoutedToQueueId = null;

        AddLocalEvent(new DocumentRetryInitiatedEvent(Id));
    }

    /// <summary>
    /// Adds a manual tag to the document.
    /// </summary>
    public void AddManualTag(TagName tagName)
    {
        if (tagName == null) throw new ArgumentNullException(nameof(tagName));

        if (AlreadyHasTag(tagName))
            return; // Idempotent - tag already exists

        var manualTag = Tag.CreateManual(tagName);
        _tags.Add(manualTag);

        AddLocalEvent(new ManualTagAddedEvent(Id, tagName));
    }

    /// <summary>
    /// Removes a manual tag from the document.
    /// </summary>
    public void RemoveManualTag(TagName tagName)
    {
        if (tagName == null) throw new ArgumentNullException(nameof(tagName));

        var tagToRemove = _tags.FirstOrDefault(t => 
            t.Source == TagSource.Manual && 
            t.Name.Value.Equals(tagName.Value, StringComparison.OrdinalIgnoreCase));

        if (tagToRemove == null)
            throw new InvalidOperationException($"Manual tag '{tagName}' not found on document");

        _tags.Remove(tagToRemove);

        AddLocalEvent(new ManualTagRemovedEvent(Id, tagName));
    }

    private bool AlreadyHasTag(TagName tagName)
    {
        return _tags.Any(t => t.Name.Value.Equals(tagName.Value, StringComparison.OrdinalIgnoreCase));
    }
}

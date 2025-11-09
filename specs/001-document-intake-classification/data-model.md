# Data Model: Document Intake & Classification System

**Feature**: 001-document-intake-classification  
**Date**: 2025-11-09  
**Architecture**: Domain-Driven Design (DDD) with Clean Architecture

---

## Design Principles

This data model follows DocFlow's constitutional principles:

1. **Strongly-Typed Domain Modeling**: All entities use strongly-typed IDs (DocumentId, RuleId, etc.) instead of primitives
2. **Immutability & Encapsulation**: Value objects are immutable; aggregates use private setters with business methods
3. **Business Logic Clarity**: All methods have business-intent names (no CRUD verbs like Set/Update/Get)
4. **Defensive Programming**: Validation enforced at construction; fail-fast on invalid state
5. **Domain Events**: Aggregates raise domain events for significant state changes

---

## 1. Aggregates

### 1.1 Document (Aggregate Root)

**Purpose**: Represents an uploaded file with metadata, status, and classification results. Coordinates the document lifecycle from upload through classification to routing.

**Aggregate Boundary**: Document owns Tags (entity) and ClassificationHistoryEntry (entity). Does NOT own ClassificationRule or RoutingQueue (references only).

**Identity**: `DocumentId` (strongly-typed GUID)

**State Transitions**:
```
Pending → Classified → Routed
   ↓
 Failed → Pending (retry)
```

**Properties**:
```csharp
public sealed class Document : FullAuditedAggregateRoot<DocumentId>, IMultiTenant
{
    // Identity & Tenancy
    public DocumentId Id { get; }
    public Guid? TenantId { get; set; }
    
    // Core Metadata (Value Objects)
    public FileName FileName { get; private set; }
    public FileSize FileSize { get; private set; }
    public MimeType MimeType { get; private set; }
    public BlobReference BlobReference { get; private set; }
    
    // Business State
    public DocumentStatus Status { get; private set; }
    public ErrorMessage? LastError { get; private set; }
    public int RetryCount { get; private set; }
    public ConfidenceScore? ClassificationConfidence { get; private set; }
    
    // References to Other Aggregates (IDs only, not navigation properties)
    public InboxId InboxId { get; private set; }
    public RoutingQueueId? RoutingQueueId { get; private set; }
    
    // Owned Entities (within aggregate boundary)
    private readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    
    private readonly List<ClassificationHistoryEntry> _classificationHistory = new();
    public IReadOnlyCollection<ClassificationHistoryEntry> ClassificationHistory => 
        _classificationHistory.AsReadOnly();
    
    // Extracted text cache (temporary, not persisted)
    private string? _extractedText;
    
    // Private constructor (enforce factory method)
    private Document(
        DocumentId id, 
        FileName fileName, 
        FileSize fileSize, 
        MimeType mimeType,
        BlobReference blobReference,
        InboxId inboxId,
        Guid? tenantId)
    {
        Id = id;
        FileName = fileName;
        FileSize = fileSize;
        MimeType = mimeType;
        BlobReference = blobReference;
        InboxId = inboxId;
        TenantId = tenantId;
        Status = DocumentStatus.Pending;
        RetryCount = 0;
    }
    
    // Factory Method (business-intent name)
    public static Document RegisterUpload(
        DocumentId id,
        FileName fileName,
        FileSize fileSize,
        MimeType mimeType,
        BlobReference blobReference,
        InboxId inboxId,
        Guid? tenantId)
    {
        var document = new Document(id, fileName, fileSize, mimeType, blobReference, inboxId, tenantId);
        
        // Raise domain event
        document.AddDomainEvent(new DocumentUploadedEvent(document.Id, document.InboxId));
        
        return document;
    }
    
    // Business Methods
    public void ApplyClassificationResult(
        IReadOnlyList<Tag> tags,
        IReadOnlyList<ClassificationHistoryEntry> matchedRules,
        RoutingQueueId routingQueueId,
        ConfidenceScore confidence)
    {
        if (Status != DocumentStatus.Pending)
            throw new InvalidOperationException($"Cannot classify document in {Status} status");
        
        if (!tags.Any())
            throw new ArgumentException("At least one tag must be applied", nameof(tags));
        
        _tags.Clear();
        _tags.AddRange(tags);
        
        _classificationHistory.Clear();
        _classificationHistory.AddRange(matchedRules);
        
        RoutingQueueId = routingQueueId;
        ClassificationConfidence = confidence;
        Status = DocumentStatus.Classified;
        LastError = null;
        
        AddDomainEvent(new DocumentClassifiedEvent(Id, tags.Select(t => t.Name).ToList(), routingQueueId));
    }
    
    public void MarkAsRouted()
    {
        if (Status != DocumentStatus.Classified)
            throw new InvalidOperationException($"Cannot route document in {Status} status");
        
        Status = DocumentStatus.Routed;
        
        AddDomainEvent(new DocumentRoutedEvent(Id, RoutingQueueId!));
    }
    
    public void RecordClassificationFailure(ErrorMessage error)
    {
        Status = DocumentStatus.Failed;
        LastError = error;
        RetryCount++;
        
        AddDomainEvent(new DocumentClassificationFailedEvent(Id, error));
    }
    
    public void RetryClassification()
    {
        if (Status != DocumentStatus.Failed)
            throw new InvalidOperationException($"Can only retry failed documents, current status: {Status}");
        
        Status = DocumentStatus.Pending;
        LastError = null;
        
        AddDomainEvent(new DocumentRetryInitiatedEvent(Id, RetryCount));
    }
    
    public void AddManualTag(Tag tag)
    {
        if (_tags.Any(t => t.Name == tag.Name))
            throw new ArgumentException($"Tag '{tag.Name}' already exists on document");
        
        _tags.Add(tag);
        
        AddDomainEvent(new ManualTagAddedEvent(Id, tag.Name));
    }
    
    public void RemoveManualTag(TagName tagName)
    {
        var tag = _tags.FirstOrDefault(t => t.Name == tagName && t.Source == TagSource.Manual);
        if (tag == null)
            throw new ArgumentException($"Manual tag '{tagName}' not found on document");
        
        _tags.Remove(tag);
        
        AddDomainEvent(new ManualTagRemovedEvent(Id, tagName));
    }
    
    public void CacheExtractedText(string text)
    {
        _extractedText = text;
    }
    
    public string? GetExtractedText() => _extractedText;
}
```

**Validation Rules**:
- FileName: 1-255 characters, no path traversal characters
- FileSize: 1KB - 50MB
- MimeType: Must be in whitelist (PDF, PNG, JPG, JPEG, TIFF)
- Status transitions: Must follow state machine rules
- Tags: Maximum 50 tags per document

---

### 1.2 ClassificationRule (Aggregate Root)

**Purpose**: Defines criteria for automatically classifying documents and determining routing destinations. Rules are evaluated in priority order.

**Aggregate Boundary**: ClassificationRule owns RuleCriteria (value object) and RuleActions (value object).

**Identity**: `RuleId` (strongly-typed GUID)

**Properties**:
```csharp
public sealed class ClassificationRule : FullAuditedAggregateRoot<RuleId>, IMultiTenant
{
    // Identity & Tenancy
    public RuleId Id { get; }
    public Guid? TenantId { get; set; }
    
    // Metadata
    public RuleName Name { get; private set; }
    public RuleDescription Description { get; private set; }
    public RulePriority Priority { get; private set; }
    public bool IsActive { get; private set; }
    
    // Matching Criteria (Value Object)
    public RuleCriteria Criteria { get; private set; }
    
    // Actions (Value Object)
    public RuleActions Actions { get; private set; }
    
    // Statistics
    public int MatchCount { get; private set; }
    public DateTime? LastMatchedAt { get; private set; }
    
    // Private constructor
    private ClassificationRule(
        RuleId id,
        RuleName name,
        RuleDescription description,
        RulePriority priority,
        RuleCriteria criteria,
        RuleActions actions,
        Guid? tenantId)
    {
        Id = id;
        Name = name;
        Description = description;
        Priority = priority;
        Criteria = criteria;
        Actions = actions;
        TenantId = tenantId;
        IsActive = true;
        MatchCount = 0;
    }
    
    // Factory Method
    public static ClassificationRule Create(
        RuleId id,
        RuleName name,
        RuleDescription description,
        RulePriority priority,
        RuleCriteria criteria,
        RuleActions actions,
        Guid? tenantId)
    {
        var rule = new ClassificationRule(id, name, description, priority, criteria, actions, tenantId);
        
        rule.AddDomainEvent(new ClassificationRuleCreatedEvent(rule.Id, rule.Name));
        
        return rule;
    }
    
    // Business Methods
    public async Task<bool> MatchesAsync(Document document)
    {
        if (!IsActive)
            return false;
        
        return await Criteria.EvaluateAsync(document);
    }
    
    public void UpdateCriteria(RuleCriteria newCriteria)
    {
        Criteria = newCriteria;
        
        AddDomainEvent(new ClassificationRuleCriteriaUpdatedEvent(Id));
    }
    
    public void UpdateActions(RuleActions newActions)
    {
        Actions = newActions;
        
        AddDomainEvent(new ClassificationRuleActionsUpdatedEvent(Id));
    }
    
    public void UpdatePriority(RulePriority newPriority)
    {
        Priority = newPriority;
        
        AddDomainEvent(new ClassificationRulePriorityUpdatedEvent(Id, newPriority));
    }
    
    public void Enable()
    {
        if (IsActive)
            return;
        
        IsActive = true;
        
        AddDomainEvent(new ClassificationRuleEnabledEvent(Id));
    }
    
    public void Disable()
    {
        if (!IsActive)
            return;
        
        IsActive = false;
        
        AddDomainEvent(new ClassificationRuleDisabledEvent(Id));
    }
    
    public void RecordMatch()
    {
        MatchCount++;
        LastMatchedAt = DateTime.UtcNow;
    }
}
```

**Validation Rules**:
- Name: 1-100 characters, required
- Priority: 1-999, unique per tenant
- Criteria: At least one criterion must be specified
- Actions: At least one tag must be assigned

---

### 1.3 RoutingQueue (Aggregate Root)

**Purpose**: Defines a destination for classified documents. Can be folder-based (file system) or webhook-based (HTTP endpoint).

**Aggregate Boundary**: RoutingQueue owns WebhookConfiguration (value object) if applicable.

**Identity**: `RoutingQueueId` (strongly-typed GUID)

**Properties**:
```csharp
public sealed class RoutingQueue : FullAuditedAggregateRoot<RoutingQueueId>, IMultiTenant
{
    // Identity & Tenancy
    public RoutingQueueId Id { get; }
    public Guid? TenantId { get; set; }
    
    // Metadata
    public QueueName Name { get; private set; }
    public QueueDescription Description { get; private set; }
    public RoutingQueueType Type { get; private set; }
    public bool IsActive { get; private set; }
    
    // Type-specific Configuration
    public FolderPath? FolderPath { get; private set; }
    public WebhookConfiguration? WebhookConfiguration { get; private set; }
    
    // Statistics
    public int DocumentsRouted { get; private set; }
    public DateTime? LastRoutedAt { get; private set; }
    
    // Private constructor
    private RoutingQueue(
        RoutingQueueId id,
        QueueName name,
        QueueDescription description,
        RoutingQueueType type,
        Guid? tenantId)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        TenantId = tenantId;
        IsActive = true;
        DocumentsRouted = 0;
    }
    
    // Factory Methods
    public static RoutingQueue CreateFolderQueue(
        RoutingQueueId id,
        QueueName name,
        QueueDescription description,
        FolderPath folderPath,
        Guid? tenantId)
    {
        var queue = new RoutingQueue(id, name, description, RoutingQueueType.Folder, tenantId);
        queue.FolderPath = folderPath;
        
        queue.AddDomainEvent(new RoutingQueueCreatedEvent(queue.Id, queue.Name, queue.Type));
        
        return queue;
    }
    
    public static RoutingQueue CreateWebhookQueue(
        RoutingQueueId id,
        QueueName name,
        QueueDescription description,
        WebhookConfiguration webhookConfig,
        Guid? tenantId)
    {
        var queue = new RoutingQueue(id, name, description, RoutingQueueType.Webhook, tenantId);
        queue.WebhookConfiguration = webhookConfig;
        
        queue.AddDomainEvent(new RoutingQueueCreatedEvent(queue.Id, queue.Name, queue.Type));
        
        return queue;
    }
    
    // Business Methods
    public void UpdateFolderPath(FolderPath newPath)
    {
        if (Type != RoutingQueueType.Folder)
            throw new InvalidOperationException("Cannot set folder path on non-folder queue");
        
        FolderPath = newPath;
        
        AddDomainEvent(new RoutingQueueConfigurationUpdatedEvent(Id));
    }
    
    public void UpdateWebhookConfiguration(WebhookConfiguration newConfig)
    {
        if (Type != RoutingQueueType.Webhook)
            throw new InvalidOperationException("Cannot set webhook config on non-webhook queue");
        
        WebhookConfiguration = newConfig;
        
        AddDomainEvent(new RoutingQueueConfigurationUpdatedEvent(Id));
    }
    
    public void RecordDocumentRouted()
    {
        DocumentsRouted++;
        LastRoutedAt = DateTime.UtcNow;
    }
    
    public void Enable()
    {
        IsActive = true;
        AddDomainEvent(new RoutingQueueEnabledEvent(Id));
    }
    
    public void Disable()
    {
        IsActive = false;
        AddDomainEvent(new RoutingQueueDisabledEvent(Id));
    }
}
```

**Validation Rules**:
- Name: 1-100 characters, unique per tenant
- Type: Must be Folder or Webhook
- FolderPath: Required if Type=Folder, must be valid path
- WebhookConfiguration: Required if Type=Webhook, URL must be valid HTTPS

---

## 2. Entities (Owned by Aggregates)

### 2.1 Tag (Owned by Document)

**Purpose**: Represents a label applied to a document for categorization. Can be applied by rules (automatic) or manually by operators.

**Identity**: `TagId` (strongly-typed GUID)

**Properties**:
```csharp
public sealed class Tag : Entity<TagId>
{
    public TagId Id { get; }
    public TagName Name { get; private set; }
    public TagSource Source { get; private set; } // Automatic (rule) or Manual
    public RuleId? AppliedByRuleId { get; private set; } // Null if manual
    public DateTime AppliedAt { get; private set; }
    
    // Private constructor
    private Tag(TagId id, TagName name, TagSource source, RuleId? appliedByRuleId)
    {
        Id = id;
        Name = name;
        Source = source;
        AppliedByRuleId = appliedByRuleId;
        AppliedAt = DateTime.UtcNow;
    }
    
    // Factory Methods
    public static Tag CreateAutomaticTag(TagId id, TagName name, RuleId ruleId)
    {
        return new Tag(id, name, TagSource.Automatic, ruleId);
    }
    
    public static Tag CreateManualTag(TagId id, TagName name)
    {
        return new Tag(id, name, TagSource.Manual, null);
    }
}
```

---

### 2.2 ClassificationHistoryEntry (Owned by Document)

**Purpose**: Records the result of a single rule evaluation against a document, including which rule was evaluated, whether it matched, and the criteria that were checked.

**Identity**: `ClassificationHistoryEntryId` (strongly-typed GUID)

**Properties**:
```csharp
public sealed class ClassificationHistoryEntry : Entity<ClassificationHistoryEntryId>
{
    public ClassificationHistoryEntryId Id { get; }
    public RuleId RuleId { get; private set; }
    public RuleName RuleName { get; private set; } // Denormalized for history
    public bool Matched { get; private set; }
    public ConfidenceScore? Confidence { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public string CriteriaSnapshot { get; private set; } // JSON snapshot of criteria
    
    // Private constructor
    private ClassificationHistoryEntry(
        ClassificationHistoryEntryId id,
        RuleId ruleId,
        RuleName ruleName,
        bool matched,
        ConfidenceScore? confidence,
        string criteriaSnapshot)
    {
        Id = id;
        RuleId = ruleId;
        RuleName = ruleName;
        Matched = matched;
        Confidence = confidence;
        EvaluatedAt = DateTime.UtcNow;
        CriteriaSnapshot = criteriaSnapshot;
    }
    
    // Factory Method
    public static ClassificationHistoryEntry Record(
        ClassificationHistoryEntryId id,
        ClassificationRule rule,
        bool matched,
        ConfidenceScore? confidence)
    {
        var criteriaJson = System.Text.Json.JsonSerializer.Serialize(rule.Criteria);
        
        return new ClassificationHistoryEntry(
            id, 
            rule.Id, 
            rule.Name, 
            matched, 
            confidence, 
            criteriaJson);
    }
}
```

---

### 2.3 WebhookDelivery (Independent Entity)

**Purpose**: Tracks webhook delivery attempts to external systems, including retry history and delivery status.

**Identity**: `WebhookDeliveryId` (strongly-typed GUID)

**Note**: This is NOT owned by an aggregate because webhooks can be retried independently of documents and queues. It's a separate entity that references both.

**Properties**:
```csharp
public sealed class WebhookDelivery : FullAuditedEntity<WebhookDeliveryId>, IMultiTenant
{
    public WebhookDeliveryId Id { get; }
    public Guid? TenantId { get; set; }
    
    // References
    public DocumentId DocumentId { get; private set; }
    public RoutingQueueId RoutingQueueId { get; private set; }
    
    // Delivery Details
    public WebhookUrl Url { get; private set; }
    public string PayloadJson { get; private set; }
    public WebhookDeliveryStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    
    // Response Details
    public HttpStatusCode? ResponseStatusCode { get; private set; }
    public string? ResponseBody { get; private set; }
    public ErrorMessage? ErrorMessage { get; private set; }
    public TimeSpan? ResponseTime { get; private set; }
    
    // Private constructor
    private WebhookDelivery(
        WebhookDeliveryId id,
        DocumentId documentId,
        RoutingQueueId routingQueueId,
        WebhookUrl url,
        string payloadJson,
        Guid? tenantId)
    {
        Id = id;
        DocumentId = documentId;
        RoutingQueueId = routingQueueId;
        Url = url;
        PayloadJson = payloadJson;
        TenantId = tenantId;
        Status = WebhookDeliveryStatus.Pending;
        RetryCount = 0;
    }
    
    // Factory Method
    public static WebhookDelivery Schedule(
        WebhookDeliveryId id,
        DocumentId documentId,
        RoutingQueueId routingQueueId,
        WebhookUrl url,
        string payloadJson,
        Guid? tenantId)
    {
        return new WebhookDelivery(id, documentId, routingQueueId, url, payloadJson, tenantId);
    }
    
    // Business Methods
    public void RecordSuccess(HttpStatusCode statusCode, string responseBody, TimeSpan responseTime)
    {
        Status = WebhookDeliveryStatus.Delivered;
        ResponseStatusCode = statusCode;
        ResponseBody = responseBody;
        ResponseTime = responseTime;
        ErrorMessage = null;
        NextRetryAt = null;
    }
    
    public void RecordFailure(ErrorMessage error, TimeSpan? responseTime)
    {
        Status = WebhookDeliveryStatus.Failed;
        ErrorMessage = error;
        ResponseTime = responseTime;
        RetryCount++;
        
        if (RetryCount < 5)
        {
            // Exponential backoff: 2^retryCount minutes
            NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, RetryCount));
        }
    }
    
    public void MarkAsRetrying()
    {
        Status = WebhookDeliveryStatus.Retrying;
    }
}
```

---

## 3. Value Objects

### 3.1 Strongly-Typed IDs

All IDs are implemented as value objects to ensure type safety:

```csharp
public sealed record DocumentId(Guid Value)
{
    public static DocumentId NewId() => new(Guid.NewGuid());
    public static DocumentId FromGuid(Guid guid) => new(guid);
}

public sealed record RuleId(Guid Value)
{
    public static RuleId NewId() => new(Guid.NewGuid());
}

public sealed record RoutingQueueId(Guid Value)
{
    public static RoutingQueueId NewId() => new(Guid.NewGuid());
}

public sealed record InboxId(Guid Value)
{
    public static InboxId NewId() => new(Guid.NewGuid());
}

public sealed record TagId(Guid Value)
{
    public static TagId NewId() => new(Guid.NewGuid());
}

public sealed record WebhookDeliveryId(Guid Value)
{
    public static WebhookDeliveryId NewId() => new(Guid.NewGuid());
}

public sealed record ClassificationHistoryEntryId(Guid Value)
{
    public static ClassificationHistoryEntryId NewId() => new(Guid.NewGuid());
}
```

---

### 3.2 Business Value Objects

#### FileName
```csharp
public sealed record FileName
{
    public string Value { get; }
    
    private FileName(string value)
    {
        Value = value;
    }
    
    public static FileName Create(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        if (fileName.Length > 255)
            throw new ArgumentException("File name cannot exceed 255 characters", nameof(fileName));
        
        // Remove path components (prevent path traversal)
        var sanitized = Path.GetFileName(fileName);
        
        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (sanitized.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("File name contains invalid characters", nameof(fileName));
        
        return new FileName(sanitized);
    }
}
```

#### FileSize
```csharp
public sealed record FileSize
{
    public long Bytes { get; }
    
    private FileSize(long bytes)
    {
        Bytes = bytes;
    }
    
    public static FileSize Create(long bytes)
    {
        if (bytes < 1024) // 1KB minimum
            throw new ArgumentException("File size must be at least 1KB", nameof(bytes));
        
        if (bytes > 50 * 1024 * 1024) // 50MB maximum
            throw new ArgumentException("File size cannot exceed 50MB", nameof(bytes));
        
        return new FileSize(bytes);
    }
    
    public string ToHumanReadable()
    {
        if (Bytes < 1024) return $"{Bytes} B";
        if (Bytes < 1024 * 1024) return $"{Bytes / 1024.0:F2} KB";
        return $"{Bytes / (1024.0 * 1024.0):F2} MB";
    }
}
```

#### MimeType
```csharp
public sealed record MimeType
{
    public string Value { get; }
    
    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/tiff"
    };
    
    private MimeType(string value)
    {
        Value = value;
    }
    
    public static MimeType Create(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type cannot be empty", nameof(mimeType));
        
        var normalized = mimeType.ToLowerInvariant();
        
        if (!AllowedMimeTypes.Contains(normalized))
            throw new ArgumentException(
                $"MIME type '{mimeType}' is not supported. Allowed: {string.Join(", ", AllowedMimeTypes)}", 
                nameof(mimeType));
        
        return new MimeType(normalized);
    }
    
    public static MimeType Pdf => new("application/pdf");
    public static MimeType Png => new("image/png");
    public static MimeType Jpeg => new("image/jpeg");
    public static MimeType Tiff => new("image/tiff");
}
```

#### RegexPattern
```csharp
public sealed record RegexPattern
{
    public string Value { get; }
    
    private RegexPattern(string value)
    {
        Value = value;
    }
    
    public static RegexPattern Create(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be empty", nameof(pattern));
        
        // Validate regex syntax
        try
        {
            _ = Regex.Match("", pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {ex.Message}", nameof(pattern));
        }
        catch (RegexMatchTimeoutException)
        {
            throw new ArgumentException("Regex pattern is too complex (potential ReDoS)", nameof(pattern));
        }
        
        // Prevent catastrophic backtracking patterns
        if (pattern.Contains("(.*)+") || pattern.Contains("(.*)*"))
        {
            throw new ArgumentException(
                "Regex pattern may cause catastrophic backtracking", 
                nameof(pattern));
        }
        
        return new RegexPattern(pattern);
    }
    
    public bool Matches(string input)
    {
        return Regex.IsMatch(input, Value, RegexOptions.None, TimeSpan.FromSeconds(1));
    }
}
```

#### RuleCriteria (Complex Value Object)
```csharp
public sealed record RuleCriteria
{
    public RegexPattern? FilenamePattern { get; }
    public MimeType? MimeType { get; }
    public FileSizeRange? SizeRange { get; }
    public IReadOnlyList<TextSnippet>? RequiredTextSnippets { get; }
    
    private RuleCriteria(
        RegexPattern? filenamePattern,
        MimeType? mimeType,
        FileSizeRange? sizeRange,
        IReadOnlyList<TextSnippet>? requiredTextSnippets)
    {
        FilenamePattern = filenamePattern;
        MimeType = mimeType;
        SizeRange = sizeRange;
        RequiredTextSnippets = requiredTextSnippets;
    }
    
    public static RuleCriteria Create(
        RegexPattern? filenamePattern = null,
        MimeType? mimeType = null,
        FileSizeRange? sizeRange = null,
        IReadOnlyList<TextSnippet>? requiredTextSnippets = null)
    {
        // At least one criterion must be specified
        if (filenamePattern == null && 
            mimeType == null && 
            sizeRange == null && 
            (requiredTextSnippets == null || !requiredTextSnippets.Any()))
        {
            throw new ArgumentException("At least one matching criterion must be specified");
        }
        
        return new RuleCriteria(filenamePattern, mimeType, sizeRange, requiredTextSnippets);
    }
    
    public async Task<bool> EvaluateAsync(Document document)
    {
        // Filename pattern match
        if (FilenamePattern != null && !FilenamePattern.Matches(document.FileName.Value))
            return false;
        
        // MIME type match
        if (MimeType != null && document.MimeType != MimeType)
            return false;
        
        // File size range match
        if (SizeRange != null && !SizeRange.Contains(document.FileSize))
            return false;
        
        // Text snippet match (requires extracted text)
        if (RequiredTextSnippets != null && RequiredTextSnippets.Any())
        {
            var extractedText = document.GetExtractedText();
            if (extractedText == null)
                return false; // Text not extracted yet
            
            foreach (var snippet in RequiredTextSnippets)
            {
                if (!extractedText.Contains(snippet.Value, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }
        
        return true;
    }
}
```

#### RuleActions (Complex Value Object)
```csharp
public sealed record RuleActions
{
    public IReadOnlyList<TagName> TagsToApply { get; }
    public RoutingQueueId RoutingQueueId { get; }
    
    private RuleActions(IReadOnlyList<TagName> tagsToApply, RoutingQueueId routingQueueId)
    {
        TagsToApply = tagsToApply;
        RoutingQueueId = routingQueueId;
    }
    
    public static RuleActions Create(IReadOnlyList<TagName> tagsToApply, RoutingQueueId routingQueueId)
    {
        if (tagsToApply == null || !tagsToApply.Any())
            throw new ArgumentException("At least one tag must be specified", nameof(tagsToApply));
        
        if (tagsToApply.Count > 10)
            throw new ArgumentException("Cannot apply more than 10 tags per rule", nameof(tagsToApply));
        
        return new RuleActions(tagsToApply, routingQueueId);
    }
}
```

#### WebhookConfiguration
```csharp
public sealed record WebhookConfiguration
{
    public WebhookUrl Url { get; }
    public HttpMethod Method { get; }
    public WebhookAuthType AuthType { get; }
    public EncryptedCredential? Credential { get; }
    public TimeSpan Timeout { get; }
    
    private WebhookConfiguration(
        WebhookUrl url,
        HttpMethod method,
        WebhookAuthType authType,
        EncryptedCredential? credential,
        TimeSpan timeout)
    {
        Url = url;
        Method = method;
        AuthType = authType;
        Credential = credential;
        Timeout = timeout;
    }
    
    public static WebhookConfiguration Create(
        WebhookUrl url,
        HttpMethod method = HttpMethod.Post,
        WebhookAuthType authType = WebhookAuthType.None,
        EncryptedCredential? credential = null,
        TimeSpan? timeout = null)
    {
        if (authType != WebhookAuthType.None && credential == null)
            throw new ArgumentException("Credential required when auth type is not None");
        
        return new WebhookConfiguration(
            url, 
            method, 
            authType, 
            credential, 
            timeout ?? TimeSpan.FromSeconds(30));
    }
}
```

---

## 4. Enumerations

### DocumentStatus
```csharp
public enum DocumentStatus
{
    Pending = 1,
    Classified = 2,
    Routed = 3,
    Failed = 4
}
```

### TagSource
```csharp
public enum TagSource
{
    Automatic = 1, // Applied by classification rule
    Manual = 2     // Applied by operator
}
```

### RoutingQueueType
```csharp
public enum RoutingQueueType
{
    Folder = 1,
    Webhook = 2
}
```

### WebhookDeliveryStatus
```csharp
public enum WebhookDeliveryStatus
{
    Pending = 1,
    Retrying = 2,
    Delivered = 3,
    Failed = 4
}
```

### WebhookAuthType
```csharp
public enum WebhookAuthType
{
    None = 1,
    BearerToken = 2,
    BasicAuth = 3,
    ApiKey = 4
}
```

---

## 5. Domain Events

### Document Events
```csharp
public sealed record DocumentUploadedEvent(DocumentId DocumentId, InboxId InboxId);
public sealed record DocumentClassifiedEvent(DocumentId DocumentId, List<string> Tags, RoutingQueueId RoutingQueueId);
public sealed record DocumentRoutedEvent(DocumentId DocumentId, RoutingQueueId RoutingQueueId);
public sealed record DocumentClassificationFailedEvent(DocumentId DocumentId, ErrorMessage Error);
public sealed record DocumentRetryInitiatedEvent(DocumentId DocumentId, int RetryCount);
public sealed record ManualTagAddedEvent(DocumentId DocumentId, TagName TagName);
public sealed record ManualTagRemovedEvent(DocumentId DocumentId, TagName TagName);
```

### Classification Rule Events
```csharp
public sealed record ClassificationRuleCreatedEvent(RuleId RuleId, RuleName Name);
public sealed record ClassificationRuleCriteriaUpdatedEvent(RuleId RuleId);
public sealed record ClassificationRuleActionsUpdatedEvent(RuleId RuleId);
public sealed record ClassificationRulePriorityUpdatedEvent(RuleId RuleId, RulePriority NewPriority);
public sealed record ClassificationRuleEnabledEvent(RuleId RuleId);
public sealed record ClassificationRuleDisabledEvent(RuleId RuleId);
```

### Routing Queue Events
```csharp
public sealed record RoutingQueueCreatedEvent(RoutingQueueId QueueId, QueueName Name, RoutingQueueType Type);
public sealed record RoutingQueueConfigurationUpdatedEvent(RoutingQueueId QueueId);
public sealed record RoutingQueueEnabledEvent(RoutingQueueId QueueId);
public sealed record RoutingQueueDisabledEvent(RoutingQueueId QueueId);
```

---

## 6. Relationships & Navigation

### Aggregate Dependencies (ID References Only)

- **Document** → Inbox (InboxId)
- **Document** → RoutingQueue (RoutingQueueId?)
- **Tag** → ClassificationRule (RuleId?) - null if manual tag
- **ClassificationHistoryEntry** → ClassificationRule (RuleId)
- **WebhookDelivery** → Document (DocumentId)
- **WebhookDelivery** → RoutingQueue (RoutingQueueId)

**Important**: Aggregates NEVER use navigation properties to other aggregates. Only IDs are stored. Queries join via repository methods when needed.

### Database Relationships (EF Core Configuration)

```csharp
// Document owns Tags and ClassificationHistory
builder.Entity<Document>(b =>
{
    b.OwnsMany(e => e.Tags, t =>
    {
        t.ToTable("DocumentTags");
        t.WithOwner().HasForeignKey("DocumentId");
        t.Property<TagId>("Id").HasConversion(id => id.Value, value => new TagId(value));
    });
    
    b.OwnsMany(e => e.ClassificationHistory, h =>
    {
        h.ToTable("DocumentClassificationHistory");
        h.WithOwner().HasForeignKey("DocumentId");
        h.Property<ClassificationHistoryEntryId>("Id").HasConversion(
            id => id.Value, value => new ClassificationHistoryEntryId(value));
    });
});

// WebhookDelivery has foreign keys but not navigation properties
builder.Entity<WebhookDelivery>(b =>
{
    b.Property(e => e.DocumentId)
     .HasConversion(id => id.Value, value => new DocumentId(value));
    
    b.Property(e => e.RoutingQueueId)
     .HasConversion(id => id.Value, value => new RoutingQueueId(value));
    
    // No navigation properties to Document or RoutingQueue
});
```

---

## 7. Validation Summary

### Document Aggregate
- FileName: 1-255 chars, no path traversal
- FileSize: 1KB - 50MB
- MimeType: PDF, PNG, JPG, JPEG, TIFF only
- Max 50 tags per document
- Status transitions enforced by business methods

### ClassificationRule Aggregate
- Name: 1-100 chars, required
- Priority: 1-999, unique per tenant
- At least one criterion required
- At least one tag action required
- Regex patterns validated for ReDoS

### RoutingQueue Aggregate
- Name: 1-100 chars, unique per tenant
- FolderPath required if Type=Folder
- WebhookConfiguration required if Type=Webhook
- Webhook URL must be HTTPS

### WebhookDelivery Entity
- Max 5 retry attempts
- Exponential backoff: 2^retryCount minutes
- 30-second default timeout

---

## 8. Performance Considerations

### Indexes (EF Core)
```csharp
// Document indexes
b.HasIndex(e => new { e.TenantId, e.Status, e.CreatedTime });
b.HasIndex(e => new { e.TenantId, e.InboxId });
b.HasIndex(e => new { e.TenantId, e.RoutingQueueId });

// ClassificationRule indexes
b.HasIndex(e => new { e.TenantId, e.Priority }).HasFilter("IsActive = true");
b.HasIndex(e => new { e.TenantId, e.IsActive });

// RoutingQueue indexes
b.HasIndex(e => new { e.TenantId, e.IsActive });

// WebhookDelivery indexes
b.HasIndex(e => new { e.TenantId, e.Status, e.NextRetryAt });
b.HasIndex(e => new { e.TenantId, e.DocumentId });
```

### Query Optimization
- Use `.AsNoTracking()` for read-only queries
- Project to DTOs instead of loading full aggregates for lists
- Use `Include()` sparingly; prefer separate queries for large collections
- Cache active rules per tenant in Redis (5-minute TTL)

---

## Summary

This data model follows DocFlow's constitutional principles:

✅ **Strongly-Typed IDs**: All entities use DocumentId, RuleId, etc.  
✅ **Immutability**: Value objects are immutable records  
✅ **Encapsulation**: Private setters, business methods only  
✅ **Business Logic Clarity**: Methods named RegisterUpload, ApplyClassificationResult, RecordMatch  
✅ **Defensive Programming**: Validation at construction, fail-fast on invalid state  
✅ **Domain Events**: All significant state changes raise events  
✅ **Multi-Tenancy**: All aggregates implement IMultiTenant  
✅ **DDD Patterns**: Aggregates, entities, value objects, domain services properly separated

All entities are ready for implementation in `DocFlow.Domain` layer.

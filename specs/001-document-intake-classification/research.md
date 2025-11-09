# Technical Research: Document Intake & Classification System

**Feature**: 001-document-intake-classification  
**Date**: 2025-11-09  
**Purpose**: Research and justify technical decisions for implementation

---

## 1. PDF Processing Library

### Decision: **UglyToad.PdfPig 0.1.8+**

### Rationale:
- **Pure .NET implementation**: No native dependencies, works cross-platform (Linux containers)
- **MIT License**: Commercial-friendly, no licensing concerns
- **Text extraction**: Robust text extraction from PDFs including multi-page documents
- **Maintained**: Active development with regular updates
- **Performance**: Efficient memory usage with streaming support for large PDFs
- **ABP compatible**: Works seamlessly with .NET 8 and ABP Framework

### Alternatives Considered:
1. **iTextSharp/iText 7**
   - Rejected: AGPL license requires open-sourcing derivative work; commercial license expensive
   - Strong feature set but licensing incompatible with commercial SaaS

2. **PDFsharp**
   - Rejected: Primarily focused on PDF generation, text extraction capabilities limited
   - Less mature text extraction compared to PdfPig

3. **Docotic.Pdf**
   - Rejected: Commercial license required, cost per developer
   - Good features but not cost-effective for this use case

### Implementation Approach:
```csharp
// Extract text from PDF using PdfPig
using (var document = PdfDocument.Open(stream))
{
    foreach (var page in document.GetPages())
    {
        var text = page.Text;
        // Process text for classification rules
    }
}
```

### Performance Considerations:
- Stream-based processing to avoid loading entire PDF into memory
- Text extraction runs in background job to avoid blocking API requests
- Cache extracted text temporarily for rule evaluation

---

## 2. Rule Engine Pattern

### Decision: **Custom Domain Service with Priority-Based Evaluation**

### Rationale:
- **Simple and explicit**: Rules are straightforward (regex, MIME type, file size, text match)
- **Domain-driven**: Rule evaluation logic belongs in domain layer as `ClassificationRuleManager`
- **Performance**: Direct evaluation faster than external rule engine overhead
- **Maintainability**: No external DSL to learn; rules stored as structured data in database
- **Testability**: Easy to unit test with mock documents

### Alternatives Considered:
1. **NRules (Forward-chaining rule engine)**
   - Rejected: Overhead of rule compilation and Rete algorithm unnecessary for simple pattern matching
   - Adds complexity without significant benefit for this use case

2. **C# Scripting (Roslyn)**
   - Rejected: Security risk allowing arbitrary code execution
   - Complex sandboxing required, performance overhead

3. **Workflow Foundation**
   - Rejected: Deprecated technology, not actively maintained
   - Over-engineered for simple rule evaluation

### Implementation Approach:
```csharp
public class ClassificationRuleManager : DomainService
{
    public async Task<ClassificationResult> EvaluateRulesAsync(
        Document document, 
        IReadOnlyList<ClassificationRule> rules)
    {
        var matchedRules = new List<ClassificationRule>();
        
        // Evaluate rules in priority order
        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            if (await rule.MatchesAsync(document))
            {
                matchedRules.Add(rule);
            }
        }
        
        return new ClassificationResult(matchedRules);
    }
}
```

### Rule Matching Logic:
```csharp
public class ClassificationRule : AggregateRoot<RuleId>
{
    public async Task<bool> MatchesAsync(Document document)
    {
        // Filename regex match
        if (FilenamePattern != null && 
            !Regex.IsMatch(document.FileName.Value, FilenamePattern.Value, 
                           RegexOptions.None, TimeSpan.FromSeconds(1)))
            return false;
        
        // MIME type match
        if (MimeType != null && document.MimeType != MimeType)
            return false;
        
        // File size range match
        if (!FileSizeRange.Contains(document.FileSize))
            return false;
        
        // Text snippet match (if applicable)
        if (RequiredTextSnippets.Any())
        {
            var extractedText = await document.GetExtractedTextAsync();
            if (!RequiredTextSnippets.All(snippet => 
                extractedText.Contains(snippet.Value, StringComparison.OrdinalIgnoreCase)))
                return false;
        }
        
        return true;
    }
}
```

### Caching Strategy:
- Cache active rules per tenant in Redis (5-minute TTL)
- Invalidate cache when rules are created/updated/deleted
- Cache key: `rules:tenant:{tenantId}:active`

---

## 3. Webhook Resilience Pattern

### Decision: **Polly + ABP Background Jobs with Circuit Breaker**

### Rationale:
- **Polly 8+**: Industry-standard resilience library for .NET
- **Circuit breaker**: Prevents cascading failures when webhook endpoints are down
- **Retry with exponential backoff**: Handles transient failures gracefully
- **ABP Background Jobs**: Built-in job scheduling and persistence
- **Monitoring**: Track webhook delivery success/failure rates

### Alternatives Considered:
1. **Manual retry logic**
   - Rejected: Reinventing the wheel, error-prone
   - Polly provides battle-tested resilience patterns

2. **Azure Service Bus / RabbitMQ**
   - Rejected: Adds infrastructure complexity
   - ABP Background Jobs sufficient for current scale (< 10K webhooks/day)

### Implementation Approach:
```csharp
public class WebhookDeliveryService : ITransientDependency
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;
    
    public WebhookDeliveryService()
    {
        _policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Logger.LogWarning($"Webhook retry {retryAttempt} after {timespan}");
                })
            .WrapAsync(Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromMinutes(1)));
    }
    
    public async Task<WebhookDeliveryResult> DeliverAsync(
        WebhookUrl url, 
        DocumentMetadata payload)
    {
        var response = await _policy.ExecuteAsync(async () =>
        {
            using var client = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(payload), 
                Encoding.UTF8, 
                "application/json");
            return await client.PostAsync(url.Value, content);
        });
        
        return new WebhookDeliveryResult(response);
    }
}
```

### Circuit Breaker Configuration:
- **Threshold**: 5 consecutive failures
- **Break duration**: 60 seconds
- **Half-open state**: Test with single request after break duration
- **Monitoring**: Log circuit breaker state changes

### Background Job for Retry:
```csharp
[BackgroundJobName("retry-webhook")]
public class RetryWebhookJob : AsyncBackgroundJob<RetryWebhookArgs>, ITransientDependency
{
    public override async Task ExecuteAsync(RetryWebhookArgs args)
    {
        var delivery = await _repository.GetAsync(args.DeliveryId);
        var result = await _webhookService.DeliverAsync(delivery.Url, delivery.Payload);
        
        if (result.IsSuccess)
        {
            delivery.MarkAsDelivered();
        }
        else
        {
            delivery.RecordFailure(result.ErrorMessage);
            
            if (delivery.RetryCount < 5)
            {
                // Schedule another retry
                await _backgroundJobManager.EnqueueAsync(
                    new RetryWebhookArgs(delivery.Id),
                    delay: TimeSpan.FromMinutes(Math.Pow(2, delivery.RetryCount)));
            }
        }
    }
}
```

---

## 4. Multi-Tenancy Data Isolation

### Decision: **ABP Multi-Tenancy with Global Query Filter**

### Rationale:
- **ABP built-in**: Leverages ABP's mature multi-tenancy framework
- **Automatic filtering**: `IMultiTenant` interface enables automatic tenant filtering
- **Database per tenant optional**: Start with shared database, migrate to database-per-tenant if needed
- **Security**: Query filter prevents cross-tenant data leakage
- **Connection string resolver**: Supports different databases per tenant

### Alternatives Considered:
1. **Manual tenant filtering in every query**
   - Rejected: Error-prone, easy to forget
   - ABP's automatic filtering is safer

2. **Row-level security (PostgreSQL RLS)**
   - Rejected: Adds database-level complexity
   - ABP's approach more portable across databases

3. **Separate microservices per tenant**
   - Rejected: Over-engineered for current scale (100-500 tenants)
   - Operational complexity not justified

### Implementation Approach:
```csharp
public class Document : FullAuditedAggregateRoot<DocumentId>, IMultiTenant
{
    public Guid? TenantId { get; set; } // ABP automatically filters by this
    
    public FileName FileName { get; private set; }
    public FileSize FileSize { get; private set; }
    public DocumentStatus Status { get; private set; }
    // ...
}
```

### Query Filter Configuration:
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    // ABP automatically configures global query filter:
    // builder.Entity<Document>().HasQueryFilter(e => e.TenantId == CurrentTenant.Id);
    
    builder.Entity<Document>(b =>
    {
        b.ToTable("Documents");
        b.ConfigureByConvention(); // Applies multi-tenancy filter
        
        // Additional indexes for tenant-specific queries
        b.HasIndex(e => new { e.TenantId, e.Status, e.CreatedTime });
    });
}
```

### Blob Storage Partitioning:
```csharp
public class TenantBlobNamingNormalizer : IBlobNamingNormalizer, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    
    public string NormalizeContainerName(string containerName)
    {
        if (_currentTenant.Id.HasValue)
        {
            return $"tenant-{_currentTenant.Id}/{containerName}";
        }
        return $"host/{containerName}";
    }
}
```

### Redis Cache Partitioning:
- All cache keys prefixed with tenant ID: `tenant:{tenantId}:rules`
- Prevents cache pollution between tenants
- Automatic invalidation when tenant switches

---

## 5. Background Job Processing

### Decision: **Hangfire with ABP Background Jobs Abstraction**

### Rationale:
- **ABP abstraction**: Vendor-neutral, can switch between Hangfire/RabbitMQ later
- **Hangfire**: Mature, reliable, built-in dashboard for monitoring
- **PostgreSQL storage**: Uses existing database, no additional infrastructure
- **Retry policies**: Built-in automatic retry with exponential backoff
- **Job scheduling**: Supports delayed jobs, recurring jobs, continuations

### Alternatives Considered:
1. **Azure Queue Storage / AWS SQS**
   - Rejected: Vendor lock-in, additional infrastructure
   - Hangfire sufficient for current scale

2. **RabbitMQ**
   - Rejected: Operational complexity (separate service to maintain)
   - Hangfire's PostgreSQL storage simpler

3. **Azure Service Bus**
   - Rejected: Cloud-specific, expensive at scale
   - Not suitable for self-hosted deployments

### Implementation Approach:
```csharp
// Job definition
[BackgroundJobName("classify-document")]
public class ClassifyDocumentJob : AsyncBackgroundJob<ClassifyDocumentArgs>, ITransientDependency
{
    private readonly DocumentClassificationManager _manager;
    private readonly ILogger<ClassifyDocumentJob> _logger;
    
    public override async Task ExecuteAsync(ClassifyDocumentArgs args)
    {
        _logger.LogInformation("Starting classification for document {DocumentId}", 
            args.DocumentId);
        
        var document = await _repository.GetAsync(args.DocumentId);
        var rules = await _ruleCache.GetActiveRulesAsync(document.TenantId);
        
        var result = await _manager.ClassifyDocumentAsync(document, rules);
        
        if (result.IsSuccess)
        {
            // Enqueue routing job
            await _backgroundJobManager.EnqueueAsync(
                new RouteDocumentArgs(document.Id, result.RoutingQueueId));
        }
        else
        {
            document.MarkAsFailed(result.ErrorMessage);
        }
    }
}

// Job enqueuing
await _backgroundJobManager.EnqueueAsync(
    new ClassifyDocumentArgs(documentId),
    priority: BackgroundJobPriority.High);
```

### Hangfire Configuration:
```csharp
services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(configuration.GetConnectionString("Default"));
});

services.AddHangfireServer(options =>
{
    options.WorkerCount = 10; // 10 concurrent workers
    options.Queues = new[] { "critical", "default", "low" };
});
```

### Job Monitoring:
- Hangfire Dashboard: `/hangfire` endpoint
- Custom metrics: job success rate, average processing time, queue depth
- Alerts: queue depth > 1000, failure rate > 10%

---

## 6. Caching Strategy

### Decision: **Redis 7+ for Distributed Cache with ABP Caching Abstraction**

### Rationale:
- **ABP abstraction**: Works with Redis, MemoryCache, or other providers
- **Distributed**: Shared cache across multiple app instances (K8s pods)
- **Redis 7+**: Mature, fast, reliable
- **JSON serialization**: Cache complex objects (rules, tenant config)
- **TTL management**: Automatic expiration with sliding/absolute TTL

### Alternatives Considered:
1. **In-memory cache**
   - Rejected: Doesn't work in multi-instance deployment (K8s)
   - Cache warming required on each pod

2. **SQL Server caching**
   - Rejected: Slower than Redis, not designed for caching
   - PostgreSQL already used for persistence

3. **Memcached**
   - Rejected: Less feature-rich than Redis
   - Redis supports more data structures and pub/sub

### Implementation Approach:
```csharp
public class ClassificationRuleCacheService : ITransientDependency
{
    private readonly IDistributedCache<List<ClassificationRuleDto>> _cache;
    private const int CacheDurationMinutes = 5;
    
    public async Task<List<ClassificationRuleDto>> GetActiveRulesAsync(Guid tenantId)
    {
        var cacheKey = $"rules:tenant:{tenantId}:active";
        
        return await _cache.GetOrAddAsync(
            cacheKey,
            async () => await _repository.GetActiveRulesByTenantAsync(tenantId),
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
            });
    }
    
    public async Task InvalidateRuleCacheAsync(Guid tenantId)
    {
        var cacheKey = $"rules:tenant:{tenantId}:active";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

### Cache Key Patterns:
- **Classification rules**: `rules:tenant:{tenantId}:active`
- **Tenant feature flags**: `features:tenant:{tenantId}`
- **Document metadata** (short-term): `document:{documentId}:metadata` (30 seconds TTL)
- **Extracted text** (temporary): `document:{documentId}:text` (5 minutes TTL)

### Redis Configuration:
```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "DocFlow:",
    "ConnectionTimeout": 5000,
    "SyncTimeout": 5000
  }
}
```

### Cache Invalidation Strategy:
- **Event-driven**: Domain events trigger cache invalidation
- **Write-through**: Update database first, then invalidate cache
- **Graceful degradation**: If Redis unavailable, fall back to direct database queries

---

## 7. Large File Handling

### Decision: **Streaming with ABP Blob Storing and Chunked Uploads**

### Rationale:
- **Streaming**: Process files in chunks without loading entire file into memory
- **ABP Blob Storing**: Abstraction over Azure Blob, AWS S3, file system
- **Chunked uploads**: Client sends file in 5MB chunks, server assembles
- **Memory efficiency**: < 100MB memory per upload regardless of file size
- **Progress tracking**: Report upload progress to client

### Alternatives Considered:
1. **Load entire file into memory**
   - Rejected: 50MB files would consume excessive memory
   - Not scalable for concurrent uploads

2. **Direct blob storage URLs (pre-signed URLs)**
   - Considered: Could reduce server load
   - Deferred: Adds complexity, implement if needed for scale

### Implementation Approach:
```csharp
// Chunked upload controller
[HttpPost("upload-chunk")]
public async Task<IActionResult> UploadChunk(
    [FromForm] string uploadId,
    [FromForm] int chunkIndex,
    [FromForm] int totalChunks,
    [FromForm] IFormFile chunk)
{
    // Save chunk to temporary storage
    var chunkPath = Path.Combine(_tempPath, uploadId, $"chunk-{chunkIndex}");
    await using var stream = System.IO.File.Create(chunkPath);
    await chunk.CopyToAsync(stream);
    
    // If last chunk, assemble and process
    if (chunkIndex == totalChunks - 1)
    {
        await AssembleAndProcessAsync(uploadId, totalChunks);
    }
    
    return Ok(new { ChunkIndex = chunkIndex, Uploaded = true });
}

// Stream-based blob storage
public async Task SaveDocumentAsync(Document document, Stream fileStream)
{
    var blobName = $"{document.TenantId}/{document.Id}/{document.FileName.Value}";
    
    await _blobContainer.SaveAsync(
        blobName,
        fileStream,
        overrideExisting: false);
}

// Stream-based PDF text extraction
public async Task<string> ExtractTextFromPdfAsync(Stream pdfStream)
{
    using var document = PdfDocument.Open(pdfStream, new ParsingOptions 
    { 
        UseLenientParsing = true 
    });
    
    var textBuilder = new StringBuilder();
    foreach (var page in document.GetPages())
    {
        textBuilder.AppendLine(page.Text);
    }
    
    return textBuilder.ToString();
}
```

### Upload Flow:
1. Client splits file into 5MB chunks
2. Upload each chunk with `uploadId` and `chunkIndex`
3. Server saves chunks to temp storage
4. On final chunk, assemble file and move to blob storage
5. Enqueue background job for classification
6. Clean up temp chunks

### Memory Management:
- Use `Stream` instead of `byte[]` throughout
- Dispose streams properly with `using` statements
- Limit concurrent uploads per user (5 max)
- Background job processes classification to avoid blocking API

---

## 8. Database Optimization

### Decision: **Strategic Indexes and Query Optimization with EF Core**

### Rationale:
- **Covering indexes**: Speed up common queries (status, tenant, date range)
- **Partial indexes**: Index only active rules (PostgreSQL feature)
- **Composite indexes**: Multi-column indexes for complex filters
- **Query optimization**: `.AsNoTracking()` for read-only, projections for lists
- **Pagination**: ABP's `PagedResultDto` with offset/limit

### Implementation Approach:
```csharp
// Index configuration in EF Core
builder.Entity<Document>(b =>
{
    // Composite index for common filtering query
    b.HasIndex(e => new { e.TenantId, e.Status, e.CreatedTime })
     .HasDatabaseName("IX_Documents_Tenant_Status_CreatedTime");
    
    // Covering index for list queries
    b.HasIndex(e => new { e.TenantId, e.Status })
     .IncludeProperties(e => new { e.FileName, e.FileSize, e.MimeType })
     .HasDatabaseName("IX_Documents_ListQuery");
    
    // Full-text search on filename (PostgreSQL)
    b.HasIndex(e => e.FileName)
     .HasMethod("gin")
     .HasDatabaseName("IX_Documents_FileName_FullText");
});

builder.Entity<ClassificationRule>(b =>
{
    // Partial index for active rules only
    b.HasIndex(e => new { e.TenantId, e.Priority })
     .HasFilter("IsActive = true")
     .HasDatabaseName("IX_Rules_Tenant_Priority_Active");
});
```

### Query Optimization Patterns:
```csharp
// Use projections instead of full entity load
public async Task<PagedResultDto<DocumentListDto>> GetListAsync(
    DocumentSearchFilterDto input)
{
    var query = await _repository.GetQueryableAsync();
    
    // Apply filters
    query = query
        .WhereIf(!input.Status.IsNullOrEmpty(), 
            e => e.Status == input.Status.Value)
        .WhereIf(input.StartDate.HasValue, 
            e => e.CreatedTime >= input.StartDate.Value)
        .WhereIf(!input.Tags.IsNullOrEmpty(), 
            e => e.Tags.Any(t => input.Tags.Contains(t.Name)));
    
    // Get total count
    var totalCount = await query.CountAsync();
    
    // Apply pagination and projection
    var items = await query
        .OrderByDescending(e => e.CreatedTime)
        .Skip(input.SkipCount)
        .Take(input.MaxResultCount)
        .Select(e => new DocumentListDto
        {
            Id = e.Id,
            FileName = e.FileName.Value,
            Status = e.Status,
            CreatedTime = e.CreatedTime
        })
        .AsNoTracking() // Read-only, no change tracking overhead
        .ToListAsync();
    
    return new PagedResultDto<DocumentListDto>(totalCount, items);
}
```

### Pagination Best Practices:
- Default page size: 20 items
- Maximum page size: 100 items
- Use `Skip/Take` for small offsets (< 10,000 records)
- Consider keyset pagination for large offsets (future optimization)

---

## 9. Security Best Practices

### Decision: **Defense in Depth with Multiple Security Layers**

### Rationale:
- **ABP Authorization**: Role-based and permission-based access control
- **Input validation**: FluentValidation + custom validators
- **File sanitization**: Remove special characters, prevent path traversal
- **Regex timeout**: Prevent ReDoS (Regular Expression Denial of Service)
- **Encryption**: AES-256 at rest, TLS 1.2+ in transit
- **Audit logging**: All operations logged with user identity

### Implementation Approach:

#### 1. File Upload Validation
```csharp
public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(BeValidFileSize).WithMessage("File size must be between 1KB and 50MB")
            .Must(BeValidFileType).WithMessage("Only PDF, PNG, JPG, JPEG, TIFF files allowed");
        
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(BeValidFileName).WithMessage("File name contains invalid characters");
    }
    
    private bool BeValidFileSize(IFormFile file)
    {
        return file.Length > 1024 && file.Length <= 50 * 1024 * 1024; // 1KB - 50MB
    }
    
    private bool BeValidFileType(IFormFile file)
    {
        var allowedTypes = new[] 
        { 
            "application/pdf", 
            "image/png", 
            "image/jpeg", 
            "image/tiff" 
        };
        return allowedTypes.Contains(file.ContentType);
    }
    
    private bool BeValidFileName(string fileName)
    {
        // Remove path traversal attempts
        var sanitized = Path.GetFileName(fileName);
        
        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        return !sanitized.Any(c => invalidChars.Contains(c));
    }
}
```

#### 2. Regex Pattern Validation
```csharp
public class RegexPattern : ValueObject
{
    public string Value { get; private set; }
    
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
            throw new InvalidRegexPatternException($"Invalid regex pattern: {ex.Message}");
        }
        catch (RegexMatchTimeoutException)
        {
            throw new InvalidRegexPatternException("Regex pattern is too complex (potential ReDoS)");
        }
        
        // Additional checks for known problematic patterns
        if (pattern.Contains("(.*)+") || pattern.Contains("(.*)*"))
        {
            throw new InvalidRegexPatternException("Regex pattern may cause catastrophic backtracking");
        }
        
        return new RegexPattern(pattern);
    }
    
    public bool Matches(string input)
    {
        return Regex.IsMatch(input, Value, RegexOptions.None, TimeSpan.FromSeconds(1));
    }
}
```

#### 3. Permission Definitions
```csharp
public static class DocFlowPermissions
{
    public const string GroupName = "DocFlow";
    
    public static class Documents
    {
        public const string Default = GroupName + ".Documents";
        public const string Upload = Default + ".Upload";
        public const string View = Default + ".View";
        public const string Retry = Default + ".Retry";
        public const string Delete = Default + ".Delete";
    }
    
    public static class Rules
    {
        public const string Default = GroupName + ".Rules";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Test = Default + ".Test";
    }
}

// Usage in controller
[Authorize(DocFlowPermissions.Documents.Upload)]
[HttpPost]
public async Task<DocumentDto> UploadAsync(CreateDocumentDto input)
{
    // ...
}
```

#### 4. Audit Logging Configuration
```csharp
[Audited] // Logs all method calls automatically
public class DocumentApplicationService : ApplicationService, IDocumentAppService
{
    [DisableAuditing] // Opt-out for performance-critical read operations
    public async Task<PagedResultDto<DocumentListDto>> GetListAsync(
        DocumentSearchFilterDto input)
    {
        // ...
    }
}
```

### Security Checklist:
- [x] Input validation on all DTOs
- [x] File type whitelist (no blacklist)
- [x] File size limits enforced
- [x] Path traversal prevention
- [x] ReDoS prevention with regex timeouts
- [x] Permission-based authorization
- [x] Tenant isolation via query filters
- [x] Audit logging for all mutations
- [x] Webhook credential encryption
- [x] TLS 1.2+ enforced

---

## 10. Testing Strategy

### Decision: **Comprehensive Testing Pyramid with 80%+ Coverage**

### Rationale:
- **Unit tests**: Fast, isolated, test business logic
- **Integration tests**: Test infrastructure and API
- **Performance tests**: Ensure SLA compliance
- **Security tests**: Verify authorization and isolation

### Implementation Approach:

#### 1. Unit Tests (xUnit + FakeItEasy)
```csharp
public class DocumentTests
{
    [Fact]
    public void Create_WithValidInput_ShouldCreateDocument()
    {
        // Arrange
        var id = DocumentId.NewId();
        var fileName = FileName.Create("invoice.pdf");
        var fileSize = FileSize.Create(1024 * 1024); // 1MB
        var mimeType = MimeType.Pdf;
        
        // Act
        var document = Document.Create(id, fileName, fileSize, mimeType);
        
        // Assert
        Assert.NotNull(document);
        Assert.Equal(DocumentStatus.Pending, document.Status);
        Assert.Equal(fileName, document.FileName);
    }
    
    [Fact]
    public void ApplyClassificationTag_WithValidTag_ShouldAddTag()
    {
        // Arrange
        var document = CreateTestDocument();
        var tag = Tag.Create("Invoice");
        
        // Act
        document.ApplyClassificationTag(tag);
        
        // Assert
        Assert.Contains(tag, document.Tags);
    }
}

public class ClassificationRuleManagerTests
{
    [Fact]
    public async Task ClassifyDocumentAsync_WithMatchingRule_ShouldApplyTag()
    {
        // Arrange
        var document = CreateTestDocument("invoice_001.pdf");
        var rule = CreateTestRule(pattern: "invoice.*\\.pdf", tag: "Invoice");
        var rules = new List<ClassificationRule> { rule };
        
        var manager = new ClassificationRuleManager(
            A.Fake<ILogger<ClassificationRuleManager>>());
        
        // Act
        var result = await manager.ClassifyDocumentAsync(document, rules);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.MatchedRules);
        Assert.Contains("Invoice", result.AppliedTags);
    }
}
```

#### 2. Integration Tests (Testcontainers)
```csharp
public class DocumentsController_IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly RedisTestcontainer _redis;
    
    public DocumentsController_IntegrationTests()
    {
        _postgres = new PostgreSqlBuilder().Build();
        _redis = new RedisBuilder().Build();
        
        await _postgres.StartAsync();
        await _redis.StartAsync();
        
        _client = _factory
            .WithPostgreSQL(_postgres.GetConnectionString())
            .WithRedis(_redis.GetConnectionString())
            .CreateClient();
    }
    
    [Fact]
    public async Task UploadDocument_WithValidFile_ShouldReturn200()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(GenerateTestPdf());
        content.Add(fileContent, "file", "test.pdf");
        
        // Act
        var response = await _client.PostAsync("/api/documents/upload", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(result);
        Assert.Equal(DocumentStatus.Pending, result.Status);
    }
}
```

#### 3. Performance Tests (BenchmarkDotNet)
```csharp
[MemoryDiagnoser]
public class ClassificationBenchmarks
{
    private ClassificationRuleManager _manager;
    private Document _document;
    private List<ClassificationRule> _rules;
    
    [GlobalSetup]
    public void Setup()
    {
        _manager = new ClassificationRuleManager(NullLogger<ClassificationRuleManager>.Instance);
        _document = CreateLargeDocument();
        _rules = CreateTestRules(count: 100);
    }
    
    [Benchmark]
    public async Task<ClassificationResult> ClassifyDocument()
    {
        return await _manager.ClassifyDocumentAsync(_document, _rules);
    }
}
```

### Test Coverage Goals:
- **Domain layer**: 90%+ (business logic is critical)
- **Application layer**: 85%+
- **Infrastructure layer**: 70%+ (focus on custom logic, not framework code)
- **API layer**: 80%+ (integration tests)
- **Critical paths**: 100% (classification engine, routing, security)

---

## Summary of Key Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| PDF Processing | UglyToad.PdfPig | Pure .NET, MIT license, cross-platform |
| Rule Engine | Custom Domain Service | Simple, explicit, no external DSL |
| Webhook Resilience | Polly + Circuit Breaker | Industry-standard, proven patterns |
| Multi-Tenancy | ABP Multi-Tenancy | Built-in, automatic filtering, secure |
| Background Jobs | Hangfire with ABP | Mature, reliable, built-in monitoring |
| Caching | Redis 7+ | Distributed, fast, K8s-compatible |
| Large Files | Streaming + Chunked Upload | Memory-efficient, scalable |
| Database | PostgreSQL 16 with Indexes | Robust, supports full-text search |
| Security | Defense in Depth | Multiple layers, ABP authorization |
| Testing | Pyramid with 80%+ Coverage | Unit + Integration + Performance |

All decisions align with DocFlow constitutional principles and ABP Framework best practices.

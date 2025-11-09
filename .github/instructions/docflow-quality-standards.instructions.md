---
applyTo: '**/*.cs'
---

# DocFlow Code Quality, Testing & Performance Standards

## Purpose
This document defines code quality principles, testing standards, and performance requirements specifically for the DocFlow document intake and classification system. These standards ensure the system can handle large files, maintain security, scale across multiple tenants, and provide reliable document processing.

---

## 1. Code Quality Principles

### 1.1 Business Logic Clarity
- **Use intention-revealing names**: All methods, classes, and variables must clearly express business intent
  - ✅ Good: `ClassifyDocumentByRules()`, `RouteToApprovalQueue()`, `ValidateDocumentSize()`
  - ❌ Bad: `Process()`, `DoWork()`, `Handle()`
- **Domain-specific language**: Use terminology from document management (intake, classification, routing, queue, audit trail)
- **Avoid technical leakage**: Don't expose infrastructure concerns (e.g., blob storage, database) in domain/application layers

### 1.2 Defensive Programming for Document Processing
- **Always validate file inputs**:
  ```csharp
  if (file == null) throw new ArgumentNullException(nameof(file));
  if (file.Size > MaxFileSizeInBytes) throw new FileSizeExceededException(file.Size, MaxFileSizeInBytes);
  if (!SupportedFileTypes.Contains(file.Extension)) throw new UnsupportedFileTypeException(file.Extension);
  ```
- **Guard against malformed PDFs**: Use try-catch blocks when parsing/reading documents
- **Validate classification rules**: Ensure rule expressions are safe and won't cause infinite loops
- **Sanitize file names**: Prevent path traversal and injection attacks

### 1.3 Immutability Where Possible
- Use `readonly` fields for dependencies and configuration
- Make value objects (e.g., `DocumentMetadata`, `ClassificationResult`) immutable
- Use `IReadOnlyCollection<T>` for exposed collections in aggregates
- Example:
  ```csharp
  public sealed record DocumentMetadata(
      string FileName,
      long FileSizeBytes,
      string MimeType,
      DateTime ReceivedAt);
  ```

### 1.4 Fail-Fast Principle
- Validate inputs at entry points (API controllers, application services)
- Throw specific exceptions early (e.g., `InvalidDocumentFormatException`, `ClassificationRuleNotFoundException`)
- Use guard clauses at the beginning of methods
- Don't allow invalid state to propagate through the system

### 1.5 Logging & Observability
- **Structured logging**: Use semantic logging with proper log levels
  - `LogInformation`: Document received, classification completed, routing finished
  - `LogWarning`: Classification rule matched multiple times, processing took longer than expected
  - `LogError`: File processing failed, external API call failed, rule engine exception
- **Include correlation IDs**: Track documents through the entire pipeline
- **Log performance metrics**: Document size, processing time, queue depth
- **Audit trail**: All document state changes must be logged for compliance

### 1.6 Encapsulation & Information Hiding
- Keep aggregate internals private; expose only business methods
- Use private setters or init-only properties
- Don't expose collection implementations (use `IReadOnlyCollection<T>`)
- Example:
  ```csharp
  public sealed class Document
  {
      private readonly List<ClassificationTag> _tags = new();
      public IReadOnlyCollection<ClassificationTag> Tags => _tags.AsReadOnly();
      
      private Document() { } // EF Core constructor
      
      public void ApplyClassificationTag(ClassificationTag tag)
      {
          if (tag == null) throw new ArgumentNullException(nameof(tag));
          if (_tags.Contains(tag)) return; // Idempotent
          _tags.Add(tag);
          // Raise domain event...
      }
  }
  ```

### 1.7 Avoid Primitive Obsession
- Use strongly-typed IDs: `DocumentId`, `TenantId`, `QueueId`, `RuleId`
- Wrap business concepts: `FileSize`, `MimeType`, `ConfidenceScore`
- Example:
  ```csharp
  public sealed record DocumentId(Guid Value);
  public sealed record FileSize
  {
      public long Bytes { get; }
      public FileSize(long bytes)
      {
          if (bytes < 0) throw new ArgumentException("File size cannot be negative", nameof(bytes));
          Bytes = bytes;
      }
      public double ToMegabytes() => Bytes / (1024.0 * 1024.0);
  }
  ```

---

## 2. Testing Standards

### 2.1 Test Coverage Requirements
- **Minimum coverage**: 80% for Domain and Application layers
- **Critical paths**: 100% coverage for:
  - Document classification logic
  - Rule engine evaluation
  - Routing decisions
  - Security/access control
  - Tenant isolation logic
  - File encryption/decryption
- Use code coverage tools and enforce in CI/CD pipeline

### 2.2 Unit Testing Strategy
- **Location**: `tests/DocFlow.Domain.Tests/` and `tests/DocFlow.Application.Tests/`
- **Framework**: xUnit v3
- **Mocking**: FakeItEasy
- **Test structure**: Arrange-Act-Assert (AAA)
- **Naming convention**: `MethodName_Scenario_ExpectedBehavior`
  ```csharp
  [Fact]
  public void ClassifyDocument_WhenRuleMatches_ShouldApplyCorrectTag()
  {
      // Arrange
      var document = Document.Create(/* ... */);
      var rule = ClassificationRule.Create(/* ... */);
      
      // Act
      var result = document.ApplyRule(rule);
      
      // Assert
      Assert.True(result.IsSuccess);
      Assert.Contains(rule.Tag, document.Tags);
  }
  ```

#### 2.2.1 Domain Layer Tests
- Test aggregate behavior and invariants
- Verify domain events are raised correctly
- Test value object validation
- Test strongly-typed ID equality and hashing
- Example test areas:
  - `Document` aggregate: state transitions, tag application, routing
  - `ClassificationRule` entity: rule evaluation, priority, conditions
  - `Queue` aggregate: document enqueueing, capacity limits

#### 2.2.2 Application Layer Tests
- Test application service orchestration
- Mock all infrastructure dependencies (repositories, blob storage, external APIs)
- Verify proper exception handling and error messages
- Test authorization/tenant isolation logic
- Example test areas:
  - `DocumentIntakeApplicationService`: file upload, validation, initial processing
  - `ClassificationApplicationService`: rule matching, AI fallback (if enabled), confidence scoring
  - `RoutingApplicationService`: queue selection, priority handling, load balancing

### 2.3 Integration Testing Strategy
- **Location**: `tests/DocFlow.EntityFrameworkCore.Tests/` and `tests/DocFlow.HttpApi.Host.Tests/`
- **Use Testcontainers**: Spin up real PostgreSQL/SQL Server containers
- **Use Microcks** (if applicable): Mock Dropbox/Google Drive API responses
- **Test end-to-end flows**:
  - Upload document → Store in blob → Classify → Route → Audit
  - Verify database persistence and blob storage integration
  - Test multi-tenancy isolation (data from one tenant should not leak to another)

#### 2.3.1 Integration Test Focus Areas
- **File storage**: Upload large files (up to max size), verify blob storage, verify encryption
- **Database operations**: CRUD operations, complex queries, transaction handling
- **Background jobs**: Classification job, routing job, cleanup job
- **External API integration**: Dropbox sync, Google Drive sync (use Microcks or test accounts)
- **Multi-tenancy**: Verify tenant data isolation, quota enforcement, per-tenant rules
- **Feature flags**: Test feature toggling (AI classification on/off)

### 2.4 Performance Testing
- **Location**: `tests/DocFlow.PerformanceTests/` (create if needed)
- **Tools**: BenchmarkDotNet for micro-benchmarks, k6 or JMeter for load testing
- **Critical scenarios to test**:
  - Document upload throughput (documents/second)
  - Classification rule evaluation time (< 500ms for rule-based, < 5s with AI)
  - Large file processing (e.g., 50MB PDF)
  - Concurrent user load (e.g., 100 concurrent uploads)
  - Queue processing rate (documents/second)
- **Performance baselines** (to be established):
  - Rule-based classification: < 500ms for documents up to 10MB
  - Document intake API response time: < 2s (excluding blob upload)
  - Queue throughput: > 100 documents/minute per worker
- **Regression tests**: Run performance tests in CI/CD and fail if metrics degrade by > 20%

### 2.5 Security Testing
- **Authentication/Authorization**: Test user roles (Accounting, Legal Assistant) and permissions
- **Tenant isolation**: Verify users cannot access documents from other tenants
- **File upload security**: Test malicious file uploads, oversized files, invalid file types
- **Encryption**: Verify files are encrypted at rest, audit logs cannot be tampered
- **API security**: Test rate limiting, input validation, SQL injection prevention

### 2.6 Test Data Management
- **Use builders/factories** for test data creation:
  ```csharp
  public class DocumentBuilder
  {
      private DocumentId _id = DocumentId.NewId();
      private string _fileName = "test.pdf";
      private FileSize _fileSize = new FileSize(1024);
      
      public DocumentBuilder WithFileName(string fileName)
      {
          _fileName = fileName;
          return this;
      }
      
      public Document Build() => Document.Create(_id, _fileName, _fileSize);
  }
  ```
- **Avoid hardcoded test data** in tests; use builders for flexibility
- **Clean up test data** after integration tests (use transactions or cleanup hooks)

### 2.7 Continuous Testing
- **Run tests on every commit**: Unit tests must pass before merge
- **Run integration tests on PR**: Automated in CI/CD pipeline
- **Run performance tests nightly**: Catch performance regressions early
- **Test coverage reports**: Publish to PR comments, fail if coverage drops below threshold

---

## 3. Performance Requirements

### 3.1 Response Time Requirements
- **API endpoints**:
  - Document upload (metadata only, excluding blob transfer): < 2s (p95)
  - Document classification status query: < 200ms (p95)
  - Queue listing: < 500ms (p95)
  - Rule CRUD operations: < 300ms (p95)
- **Background jobs**:
  - Rule-based classification: < 500ms per document (p95)
  - AI-enhanced classification (if enabled): < 5s per document (p95)
  - Document routing: < 100ms per document (p95)

### 3.2 Throughput Requirements
- **Document intake**: Support at least 100 concurrent uploads
- **Classification processing**: Process at least 100 documents/minute (rule-based)
- **Queue processing**: Route at least 200 documents/minute
- **Scalability**: System must scale horizontally (add more worker pods in K8s)

### 3.3 File Size Handling
- **Maximum file size**: 50MB per document (configurable per tenant)
- **Large file handling**:
  - Use streaming for file uploads/downloads (avoid loading entire file into memory)
  - Implement chunked uploads for files > 10MB
  - Use asynchronous processing for large files (background jobs)
- **Memory management**:
  - Dispose streams and file handles properly (`using` statements)
  - Avoid keeping large files in memory; process in chunks

### 3.4 Database Performance
- **Use indexes**: On frequently queried fields (TenantId, DocumentId, Status, CreatedAt)
- **Pagination**: All list endpoints must support pagination (default page size: 20, max: 100)
- **Optimize queries**:
  - Avoid N+1 queries (use eager loading or projections)
  - Use `.AsNoTracking()` for read-only queries
  - Limit projections to required fields only
- **Connection pooling**: Configure proper connection pool size (min: 5, max: 100)
- **Query timeout**: Set reasonable query timeout (default: 30s, adjustable per query)

### 3.5 Caching Strategy
- **Cache classification rules**: Rules change infrequently; cache in memory with expiration (e.g., 5 minutes)
- **Cache tenant configuration**: Quota limits, feature flags, custom settings
- **Distributed cache**: Use Redis for multi-instance scenarios (K8s deployments)
- **Cache invalidation**: Invalidate cache when rules/configuration changes
- **Example**:
  ```csharp
  public async Task<IReadOnlyList<ClassificationRule>> GetActiveRulesAsync(TenantId tenantId)
  {
      var cacheKey = $"rules:tenant:{tenantId}";
      return await _cache.GetOrCreateAsync(cacheKey, async entry =>
      {
          entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
          return await _ruleRepository.GetActiveRulesForTenantAsync(tenantId);
      });
  }
  ```

### 3.6 Background Job Performance
- **Use ABP Background Jobs**: Leverage ABP's background job system for async processing
- **Job priorities**: High (user-initiated), Normal (scheduled), Low (cleanup)
- **Retry policies**: Exponential backoff for transient failures (max retries: 5)
- **Job timeout**: Set per-job timeout (classification: 30s, routing: 10s)
- **Monitoring**: Track job queue depth, processing time, failure rate
- **Back-pressure handling**:
  - If queue depth > 1000, throttle new document intake
  - If processing time > 2x average, scale up workers
  - If failure rate > 10%, alert operations team

### 3.7 External API Performance
- **Timeout settings**:
  - Dropbox API: 30s
  - Google Drive API: 30s
  - AI classification service (if enabled): 10s
- **Circuit breaker**: Implement circuit breaker pattern for external APIs (use Polly)
  - Open circuit after 5 consecutive failures
  - Half-open after 60s
  - Close circuit after 3 successful calls
- **Rate limiting**: Respect external API rate limits (use token bucket or sliding window)
- **Retry logic**: Use exponential backoff with jitter for retries

### 3.8 Blob Storage Performance
- **Use ABP Blob Storage**: Leverage ABP's blob storage abstraction
- **Chunked upload/download**: For files > 10MB, use chunked transfer
- **Parallel upload**: For multi-file uploads, process in parallel (max 5 concurrent)
- **Pre-signed URLs**: Use pre-signed URLs for direct browser-to-blob uploads (bypass server)
- **Blob cleanup**: Periodically delete orphaned blobs (files not linked to any document)

### 3.9 Multi-Tenancy Performance
- **Tenant isolation**: Use tenant-based partitioning for data (TenantId filter on all queries)
- **Quota enforcement**: Check tenant quotas before accepting uploads (document count, storage size)
- **Per-tenant rate limiting**: Prevent one tenant from overwhelming the system
- **Tenant-specific rules**: Cache rules per tenant to avoid cross-tenant queries
- **Database sharding** (future consideration): If tenant data grows large, consider sharding by tenant

### 3.10 Monitoring & Alerting
- **Application Performance Monitoring (APM)**: Integrate with Application Insights, Datadog, or similar
- **Metrics to track**:
  - Request rate, response time, error rate (per endpoint)
  - Document processing time (intake, classification, routing)
  - Queue depth and processing rate
  - Blob storage usage (per tenant)
  - Database connection pool usage
  - External API call success/failure rate
  - Memory and CPU usage (per pod in K8s)
- **Alerts**:
  - Response time > 5s (p95)
  - Error rate > 5%
  - Queue depth > 5000
  - Disk/blob storage > 80% capacity
  - External API circuit breaker open

---

## 4. Kubernetes-Specific Considerations

### 4.1 Resource Limits
- Set memory and CPU limits for all pods
- **API pods**: Memory limit 512MB, CPU limit 500m
- **Worker pods**: Memory limit 1GB, CPU limit 1000m (for large file processing)
- Use horizontal pod autoscaling (HPA) based on CPU/memory usage

### 4.2 Health Checks
- Implement liveness and readiness probes
- **Liveness probe**: `/health/live` (returns 200 if app is running)
- **Readiness probe**: `/health/ready` (returns 200 if app can accept traffic, checks DB connection, blob storage)
- Probe interval: 10s, timeout: 5s, failure threshold: 3

### 4.3 Graceful Shutdown
- Handle SIGTERM signal for graceful shutdown
- Allow in-flight requests to complete (grace period: 30s)
- Stop accepting new background jobs during shutdown
- Persist job state before shutdown

### 4.4 Retry & Back-Pressure
- **Retry logic**: Use exponential backoff with jitter for transient failures
- **Back-pressure**: If downstream services are slow, apply back-pressure to upstream (e.g., return 503 Service Unavailable)
- **Queue management**: If job queue is full, reject new document intake or throttle

---

## 5. Security & Privacy Standards

### 5.1 File Encryption
- **Encrypt at rest**: All documents must be encrypted in blob storage (AES-256)
- **Encrypt in transit**: Use TLS 1.2+ for all API calls and blob transfers
- **Key management**: Use Azure Key Vault or AWS KMS for encryption key management
- **Per-tenant keys** (optional): Consider per-tenant encryption keys for enhanced isolation

### 5.2 Access Control
- **Authentication**: Use ABP's authentication (JWT tokens, IdentityServer, or external IdP)
- **Authorization**: Enforce role-based access control (RBAC)
  - `Accounting` role: Can upload invoices, view accounting documents
  - `LegalAssistant` role: Can upload contracts, view legal documents
  - `Admin` role: Can manage rules, queues, users
- **Tenant isolation**: Users can only access documents within their tenant
- **Audit permissions**: All permission checks must be logged

### 5.3 Audit Trail
- **Use ABP Auditing**: Leverage ABP's built-in audit logging
- **Log all state changes**:
  - Document uploaded (user, timestamp, file size, mime type)
  - Classification applied (rule ID, confidence score, timestamp)
  - Document routed (from queue, to queue, timestamp)
  - Document accessed (user, timestamp, action: view/download)
- **Immutable audit logs**: Audit logs cannot be modified or deleted
- **Retention policy**: Retain audit logs for at least 7 years (compliance requirement)

### 5.4 Input Validation & Sanitization
- **Validate all inputs**: File names, file sizes, mime types, rule expressions
- **Sanitize file names**: Remove special characters, prevent path traversal
- **Validate rule expressions**: Ensure rule expressions are safe (no code injection)
- **Rate limiting**: Limit API calls per user/tenant to prevent abuse

---

## 6. Code Review Checklist

Before approving any PR, verify:
- [ ] Business logic is in the Domain layer (not in Application/Infrastructure)
- [ ] All public methods have XML documentation comments
- [ ] All inputs are validated with guard clauses
- [ ] Exceptions are specific and meaningful (not generic `Exception`)
- [ ] Strongly-typed IDs are used (no primitive obsession)
- [ ] Aggregates expose only business methods (no public setters)
- [ ] Unit tests cover all business logic (80%+ coverage)
- [ ] Integration tests cover critical paths (file upload, classification, routing)
- [ ] Performance tests exist for critical scenarios (if applicable)
- [ ] Logging is structured with correlation IDs
- [ ] Security concerns are addressed (authentication, authorization, encryption)
- [ ] Multi-tenancy isolation is enforced
- [ ] Proper disposal of resources (`using` statements for streams, DB connections)
- [ ] No hardcoded values (use configuration or constants)
- [ ] Follow conventional commit message format

---

## 7. Continuous Improvement

### 7.1 Performance Monitoring
- Establish performance baselines after MVP
- Track performance metrics over time
- Run monthly performance regression tests
- Optimize bottlenecks identified through monitoring

### 7.2 Code Quality Metrics
- Track code coverage trends (should not decrease)
- Track cyclomatic complexity (warn if > 10 per method)
- Track technical debt (use SonarQube or similar)
- Schedule refactoring sprints to address technical debt

### 7.3 Learning & Retrospectives
- Conduct retrospectives after major releases
- Document lessons learned
- Update this instruction file based on learnings
- Share best practices with the team

---

## 8. References & Resources

- [ABP Framework Documentation](https://docs.abp.io/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/TheCleanArchitecture.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [xUnit Best Practices](https://xunit.net/docs/getting-started/netcore/cmdline)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Polly Resilience Framework](https://github.com/App-vNext/Polly)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)

---

**Last Updated**: November 9, 2025  
**Version**: 1.0  
**Maintained By**: DocFlow Development Team

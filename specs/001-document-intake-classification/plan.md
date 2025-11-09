# Implementation Plan: Document Intake & Classification System

**Branch**: `001-document-intake-classification` | **Date**: 2025-11-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-intake-classification/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This feature implements a comprehensive document intake and classification system for operators to upload PDF and image files, automatically classify them using rule-based logic, route documents to appropriate destinations (folders/webhooks), and monitor the entire workflow with status tracking, manual interventions, and audit trails.

**Primary Requirements**:
- Upload PDF/image files to designated inboxes (single and batch)
- Rule-based classification with filename regex, MIME type, file size, text content matching
- Automatic routing to folder-based or webhook-based queues
- Status tracking (Pending, Classified, Routed, Failed) with search/filtering
- Manual interventions: retry failed documents, adjust tags, retry webhooks
- Comprehensive audit trail with rule matching history

**Technical Approach**:
- ABP Framework 8.3+ for multi-tenancy, background jobs, blob storage, audit logging
- PostgreSQL 16 with EF Core for data persistence
- UglyToad.PdfPig for PDF text extraction
- ABP Background Jobs with Hangfire for async classification and routing
- Redis for caching rules and tenant configurations
- Rule engine implemented as domain service with priority-based evaluation
- Strongly-typed domain model with DDD patterns (aggregates, value objects, domain events)

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: 
- ABP Commercial Framework 8.3+ (multi-tenancy, identity, feature management, background jobs, audit logging, blob storing)
- Entity Framework Core 8.0 (ORM)
- PostgreSQL 16 with pgvector extension
- UglyToad.PdfPig 0.1.8+ (PDF text extraction, MIT license)
- FluentValidation 11+ (input validation)
- Redis 7+ (distributed caching)
- Hangfire or ABP Background Jobs (async processing)
- Polly 8+ (resilience patterns for webhooks)

**Storage**: 
- Database: PostgreSQL 16 (documents metadata, rules, queues, audit logs)
- Blob Storage: ABP Blob Storing abstraction (Azure Blob Storage, AWS S3, or file system)
- Cache: Redis 7+ (classification rules, tenant configurations)

**Testing**: 
- xUnit v3 (unit and integration tests)
- FakeItEasy (mocking framework)
- Testcontainers (PostgreSQL, Redis for integration tests)
- BenchmarkDotNet (performance testing)

**Target Platform**: 
- Linux containers (Kubernetes deployment)
- Docker Compose for local development
- ASP.NET Core 8.0 Web API (Blazor WebAssembly frontend optional)

**Project Type**: Web application (backend API + frontend)

**Performance Goals**:
- API response time: < 2s (p95) for document upload
- Classification throughput: 100 documents/minute
- Rule evaluation: < 500ms (p95) per document
- Routing: < 100ms (p95) per document
- Concurrent uploads: 50+ simultaneous users
- Large file support: 50MB PDFs with streaming

**Constraints**:
- Multi-tenancy: strict data isolation between tenants
- Security: AES-256 encryption at rest, TLS 1.2+ in transit
- Compliance: 7-year audit log retention
- File size: 50MB maximum per document (configurable per tenant)
- Scalability: horizontal scaling in Kubernetes
- Memory: efficient streaming for large files (< 500MB memory per worker)

**Scale/Scope**:
- Expected tenants: 100-500 organizations
- Documents per tenant: 10K-1M per year
- Classification rules per tenant: 10-100 rules
- Concurrent background jobs: 10-50 workers
- Webhook integrations: 5-20 endpoints per tenant

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Core Principles Compliance

- [x] **Business Logic Clarity**: All methods, classes use domain-specific language (no generic `Process()`, `Handle()`, etc.)
  - Domain services: `DocumentClassificationManager`, `RoutingManager`, `RuleEvaluationManager`
  - Methods: `ClassifyDocumentByRules()`, `RouteToQueue()`, `EvaluateRulePriority()`, `ApplyClassificationTag()`
  - No generic naming; all express business intent

- [x] **Defensive Programming**: Input validation implemented at all entry points with specific exceptions
  - Custom exceptions: `InvalidDocumentFormatException`, `FileSizeExceededException`, `ClassificationRuleNotFoundException`, `UnsupportedFileTypeException`
  - Guard clauses in application services and domain entities
  - FluentValidation for DTO validation
  - Regex pattern validation to prevent ReDoS attacks

- [x] **Immutability & Encapsulation**: Value objects are immutable; aggregates expose only business methods
  - Value objects: `DocumentId`, `RuleId`, `QueueId`, `FileSize`, `MimeType`, `FileName`, `ConfidenceScore`
  - Immutable using C# `record` types
  - Aggregates (`Document`, `ClassificationRule`, `RoutingQueue`) expose only business methods
  - Private setters, `IReadOnlyCollection<T>` for exposed collections

- [x] **Strongly-Typed IDs**: All domain concepts use strongly-typed IDs and value objects (no primitive obsession)
  - All IDs: `DocumentId`, `RuleId`, `QueueId`, `TagId`, `InboxId`, `WebhookDeliveryId`, `TenantId`
  - Business concepts wrapped: `FileSize`, `MimeType`, `FileName`, `RegexPattern`, `WebhookUrl`

- [x] **Testing Coverage**: Plan includes unit tests (80%+ coverage), integration tests, performance tests, security tests
  - Unit tests: Domain and Application layers (target 85%+)
  - Integration tests: API endpoints, database operations, blob storage, background jobs
  - Performance tests: BenchmarkDotNet for classification, routing, rule evaluation
  - Security tests: tenant isolation, input validation, encryption, RBAC

- [x] **Structured Logging**: Logging includes correlation IDs, structured data, proper log levels, audit trail
  - ABP Audit Logging for all state changes
  - Custom correlation IDs for document tracking through pipeline
  - Structured logging with Serilog (configured in ABP)
  - Performance metrics logged: processing time, file size, queue depth

- [x] **Performance Targets**: Implementation meets defined SLAs (response times, throughput, scalability)
  - All targets defined in Technical Context
  - Background jobs for async processing
  - Caching strategy for rules and tenant config
  - Streaming for large files

### Security & Privacy Compliance

- [x] **File Encryption**: Documents encrypted at rest (AES-256) and in transit (TLS 1.2+)
  - ABP Blob Storing with encryption enabled
  - Azure Blob Storage or AWS S3 encryption
  - TLS 1.2+ enforced in Kestrel configuration

- [x] **Access Control**: RBAC implemented with role definitions (Accounting, LegalAssistant, Admin)
  - ABP Authorization with custom permissions
  - Roles: `DocFlow.Operator.Accounting`, `DocFlow.Operator.Legal`, `DocFlow.Admin`
  - Permission-based API authorization

- [x] **Tenant Isolation**: Multi-tenancy isolation enforced at data and application layers
  - ABP Multi-Tenancy with data filter
  - All queries filtered by `TenantId`
  - Blob storage partitioned by tenant
  - Redis cache keys include tenant prefix

- [x] **Audit Trail**: All state changes logged with 7-year retention policy
  - ABP Audit Logging with custom retention policy
  - All document state transitions logged
  - Rule matching history preserved
  - Manual tag changes recorded with operator identity

- [x] **Input Validation**: File names sanitized, sizes validated, rule expressions validated
  - File name sanitization (remove special chars, prevent path traversal)
  - File size validation (50MB max, configurable per tenant)
  - MIME type validation (PDF, PNG, JPG, JPEG, TIFF only)
  - Regex pattern validation with timeout to prevent ReDoS

### Performance & Scalability Compliance

- [x] **Response Time**: API endpoints meet < 2s (p95), classification < 500ms (p95), routing < 100ms (p95)
  - Async operations for file upload and classification
  - Background jobs for heavy processing
  - Database query optimization with indexes

- [x] **Throughput**: Supports 100 concurrent uploads, 100 docs/min classification, 200 docs/min routing
  - Horizontal scaling with multiple worker pods
  - ABP Background Jobs with configurable concurrency
  - Connection pooling for database

- [x] **Large Files**: Streaming implemented for files > 10MB, chunked uploads for files > 10MB
  - ABP Blob Storing supports streaming
  - Chunked upload API endpoint
  - Stream-based PDF text extraction

- [x] **Database Optimization**: Indexes on TenantId/DocumentId/Status/CreatedAt, pagination on all lists
  - Indexes defined in EF Core migrations
  - Pagination using ABP PagedResultDto
  - Query optimization with `.AsNoTracking()` for read-only operations

- [x] **Caching**: Classification rules and tenant config cached with appropriate TTL
  - Redis distributed cache
  - Rules cached per tenant (5-minute TTL)
  - Tenant feature flags cached (10-minute TTL)
  - Cache invalidation on rule updates

- [x] **Background Jobs**: ABP Background Jobs used with retry policies and back-pressure handling
  - Hangfire for job scheduling
  - Exponential backoff retry (max 5 retries)
  - Queue depth monitoring
  - Throttling when queue > 1000 documents

- [x] **K8s Readiness**: Resource limits set, health probes implemented, graceful shutdown handled
  - Health check endpoints: `/health/live`, `/health/ready`
  - Resource limits in Kubernetes deployment manifest
  - Graceful shutdown with 30s grace period
  - SIGTERM handler to complete in-flight jobs

**Violations Requiring Justification**: None - all constitutional requirements are met by the technical approach.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-intake-classification/
├── plan.md                    # This file (/speckit.plan command output)
├── research.md                # Phase 0 output - technology research and decisions
├── data-model.md              # Phase 1 output - domain entities and relationships
├── quickstart.md              # Phase 1 output - developer setup guide
├── checklists/
│   └── requirements.md        # Spec validation checklist (already created)
└── contracts/                 # Phase 1 output - API contracts
    ├── documents-api.yaml     # Document upload, status, retry endpoints
    ├── rules-api.yaml         # Classification rule management endpoints
    ├── queues-api.yaml        # Routing queue management endpoints
    ├── webhooks-api.yaml      # Webhook delivery monitoring endpoints
    └── search-api.yaml        # Document search and filtering endpoints
```

### Source Code (repository root)

**Structure Decision**: Web application using ABP Framework layered architecture. The solution already exists with the following structure, and we'll add feature modules within each layer.

```text
src/
├── DocFlow.Domain/                          # Domain Layer (business logic, entities, domain services)
│   ├── DocumentManagement/                  # NEW: Document intake feature module
│   │   ├── Document.cs                      # Document aggregate root
│   │   ├── DocumentManager.cs               # Domain service for document operations
│   │   ├── DocumentId.cs                    # Strongly-typed ID
│   │   ├── DocumentStatus.cs                # Status enum
│   │   ├── FileName.cs                      # Value object
│   │   ├── FileSize.cs                      # Value object
│   │   ├── MimeType.cs                      # Value object
│   │   └── DocumentClassifiedEvent.cs       # Domain event
│   ├── ClassificationManagement/            # NEW: Classification feature module
│   │   ├── ClassificationRule.cs            # Rule aggregate root
│   │   ├── ClassificationRuleManager.cs     # Domain service for rule evaluation
│   │   ├── RuleId.cs                        # Strongly-typed ID
│   │   ├── RegexPattern.cs                  # Value object
│   │   ├── TextSnippet.cs                   # Value object
│   │   ├── Tag.cs                           # Entity
│   │   ├── TagId.cs                         # Strongly-typed ID
│   │   └── RuleMatchedEvent.cs              # Domain event
│   ├── RoutingManagement/                   # NEW: Routing feature module
│   │   ├── RoutingQueue.cs                  # Queue aggregate root
│   │   ├── RoutingManager.cs                # Domain service for routing logic
│   │   ├── QueueId.cs                       # Strongly-typed ID
│   │   ├── QueueType.cs                     # Enum (Folder, Webhook)
│   │   ├── WebhookUrl.cs                    # Value object
│   │   ├── WebhookDelivery.cs               # Entity
│   │   └── DocumentRoutedEvent.cs           # Domain event
│   └── Shared/                              # Existing shared domain concepts
│       └── TenantId.cs                      # Strongly-typed tenant ID
│
├── DocFlow.Domain.Shared/                   # Shared Domain Layer (enums, constants)
│   ├── DocumentManagement/                  # NEW
│   │   └── DocumentStatusConsts.cs
│   ├── ClassificationManagement/            # NEW
│   │   └── RulePriorityConsts.cs
│   └── RoutingManagement/                   # NEW
│       └── QueueTypeConsts.cs
│
├── DocFlow.Application/                     # Application Layer (use cases, DTOs, services)
│   ├── DocumentManagement/                  # NEW
│   │   ├── DocumentApplicationService.cs    # Upload, status, retry operations
│   │   ├── DocumentClassificationService.cs # Classification orchestration
│   │   └── DocumentSearchService.cs         # Search and filtering
│   ├── ClassificationManagement/            # NEW
│   │   ├── RuleApplicationService.cs        # Rule CRUD and testing
│   │   └── RuleEvaluationService.cs         # Rule dry-run testing
│   ├── RoutingManagement/                   # NEW
│   │   ├── QueueApplicationService.cs       # Queue CRUD operations
│   │   └── WebhookApplicationService.cs     # Webhook monitoring and retry
│   └── BackgroundJobs/                      # NEW
│       ├── ClassifyDocumentJob.cs           # Background job for classification
│       ├── RouteDocumentJob.cs              # Background job for routing
│       └── RetryWebhookJob.cs               # Background job for webhook retry
│
├── DocFlow.Application.Contracts/           # Application Contracts (DTOs, interfaces)
│   ├── DocumentManagement/                  # NEW
│   │   ├── Dtos/
│   │   │   ├── DocumentDto.cs
│   │   │   ├── CreateDocumentDto.cs
│   │   │   ├── DocumentListDto.cs
│   │   │   ├── DocumentDetailDto.cs
│   │   │   └── DocumentSearchFilterDto.cs
│   │   └── IDocumentAppService.cs
│   ├── ClassificationManagement/            # NEW
│   │   ├── Dtos/
│   │   │   ├── ClassificationRuleDto.cs
│   │   │   ├── CreateRuleDto.cs
│   │   │   ├── UpdateRuleDto.cs
│   │   │   ├── RuleDryRunDto.cs
│   │   │   └── TagDto.cs
│   │   └── IClassificationRuleAppService.cs
│   └── RoutingManagement/                   # NEW
│       ├── Dtos/
│       │   ├── RoutingQueueDto.cs
│       │   ├── CreateQueueDto.cs
│       │   ├── WebhookDeliveryDto.cs
│       │   └── WebhookRetryDto.cs
│       └── IRoutingQueueAppService.cs
│
├── DocFlow.EntityFrameworkCore/             # Infrastructure Layer (EF Core, repositories)
│   ├── EntityFrameworkCore/
│   │   ├── DocFlowDbContext.cs              # UPDATED: Add new DbSets
│   │   └── DocFlowDbContextModelCreatingExtensions.cs  # UPDATED: Configure entities
│   ├── DocumentManagement/                  # NEW
│   │   └── EfCoreDocumentRepository.cs      # Custom repository if needed
│   ├── ClassificationManagement/            # NEW
│   │   └── EfCoreClassificationRuleRepository.cs
│   └── Migrations/                          # NEW migrations will be generated
│
├── DocFlow.HttpApi.Host/                    # Presentation Layer (API controllers, startup)
│   ├── Controllers/
│   │   ├── DocumentsController.cs           # NEW: Document operations
│   │   ├── ClassificationRulesController.cs # NEW: Rule management
│   │   ├── RoutingQueuesController.cs       # NEW: Queue management
│   │   └── WebhooksController.cs            # NEW: Webhook monitoring
│   ├── appsettings.json                     # UPDATED: Add Redis, Hangfire config
│   └── HealthChecks/                        # NEW: Custom health checks
│       ├── BlobStorageHealthCheck.cs
│       └── DocumentQueueHealthCheck.cs
│
└── DocFlow.Blazor.WebApp.Client/            # Frontend (Blazor WebAssembly) - OPTIONAL
    └── Pages/
        └── Documents/                        # NEW: Document management UI
            ├── Index.razor                   # Document list with search/filter
            ├── Upload.razor                  # Document upload interface
            └── Details.razor                 # Document details and history

test/
├── DocFlow.Domain.Tests/                    # Domain unit tests
│   ├── DocumentManagement/                  # NEW
│   │   ├── DocumentTests.cs
│   │   ├── DocumentManagerTests.cs
│   │   └── ValueObjects/
│   │       ├── FileNameTests.cs
│   │       └── FileSizeTests.cs
│   ├── ClassificationManagement/            # NEW
│   │   ├── ClassificationRuleTests.cs
│   │   └── ClassificationRuleManagerTests.cs
│   └── RoutingManagement/                   # NEW
│       ├── RoutingQueueTests.cs
│       └── RoutingManagerTests.cs
│
├── DocFlow.Application.Tests/               # Application unit tests
│   ├── DocumentManagement/                  # NEW
│   │   ├── DocumentApplicationServiceTests.cs
│   │   └── DocumentClassificationServiceTests.cs
│   ├── ClassificationManagement/            # NEW
│   │   └── RuleApplicationServiceTests.cs
│   └── BackgroundJobs/                      # NEW
│       ├── ClassifyDocumentJobTests.cs
│       └── RouteDocumentJobTests.cs
│
└── DocFlow.HttpApi.Host.Tests/              # Integration tests (API, database, blob storage)
    ├── DocumentManagement/                  # NEW
    │   ├── DocumentsController_IntegrationTests.cs
    │   └── DocumentUpload_IntegrationTests.cs
    ├── ClassificationManagement/            # NEW
    │   └── ClassificationRules_IntegrationTests.cs
    └── PerformanceTests/                    # NEW: Performance benchmarks
        ├── ClassificationBenchmarks.cs
        └── RoutingBenchmarks.cs
```

**Key Architecture Decisions**:
1. **Feature-based folder structure**: Each feature (DocumentManagement, ClassificationManagement, RoutingManagement) has its own folder in each layer
2. **ABP Framework layered architecture**: Domain, Application, Application.Contracts, EntityFrameworkCore, HttpApi.Host
3. **DDD tactical patterns**: Aggregates, Entities, Value Objects, Domain Services, Domain Events
4. **Strongly-typed IDs**: All domain concepts use value objects for IDs
5. **Background jobs**: Heavy processing (classification, routing) moved to background jobs
6. **Multi-tenancy**: ABP's built-in multi-tenancy with data isolation
7. **Testing pyramid**: Unit tests for Domain/Application, integration tests for API/Infrastructure, performance tests for critical paths

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations** - All constitutional requirements are satisfied by the proposed technical approach. The solution leverages ABP Framework's built-in capabilities for multi-tenancy, security, audit logging, and background jobs, which align perfectly with DocFlow's constitutional principles.

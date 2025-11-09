<!--
SYNC IMPACT REPORT
==================
Version Change: 0.0.0 → 1.0.0
Modified Principles: Initial constitution creation with 7 core quality principles
Added Sections: Code Quality Principles, Testing Standards, Performance Requirements, Security & Privacy Standards
Templates Requiring Updates:
  ✅ plan-template.md - Updated with constitution check gates
  ✅ spec-template.md - Aligned with quality requirements
  ✅ tasks-template.md - Updated with testing and quality task types
Follow-up TODOs: None - all placeholders filled
Rationale for MAJOR version (1.0.0): Initial constitution establishment with comprehensive governance framework
-->

# DocFlow Constitution

## Core Principles

### I. Business Logic Clarity
**All code MUST express business intent using domain-specific language.**

- Use intention-revealing method, class, and variable names that clearly express business operations
- Employ terminology from document management domain: intake, classification, routing, queue, audit trail
- Avoid generic names: `Process()`, `Handle()`, `DoWork()` are forbidden
- Use explicit names: `ClassifyDocumentByRules()`, `RouteToApprovalQueue()`, `ValidateDocumentSize()`
- Never expose infrastructure concerns (blob storage, database) in domain/application layers

**Rationale**: Clear business-focused code ensures maintainability and alignment with domain experts' understanding. Generic technical names obscure business intent and make code harder to understand and modify.

### II. Defensive Programming & Fail-Fast
**All inputs MUST be validated immediately; invalid state MUST NOT propagate.**

- Validate all file inputs: null checks, size limits, supported file types
- Guard against malformed PDFs with try-catch blocks during parsing
- Validate classification rules to prevent infinite loops or unsafe expressions
- Sanitize file names to prevent path traversal and injection attacks
- Throw specific exceptions early: `InvalidDocumentFormatException`, `FileSizeExceededException`, `ClassificationRuleNotFoundException`
- Use guard clauses at method entry points
- Validate at API controllers and application services before processing

**Rationale**: Document processing systems face diverse, potentially malicious input. Early validation prevents cascading failures, improves debugging, and ensures system security and reliability.

### III. Immutability & Encapsulation
**Domain objects MUST encapsulate state and minimize mutability.**

- Use `readonly` fields for dependencies and configuration
- Make value objects immutable (use `record` types in C#)
- Use `IReadOnlyCollection<T>` for exposed collections in aggregates
- Keep aggregate internals private; expose only business methods
- Use private setters or init-only properties
- Never expose collection implementations directly

**Rationale**: Immutability prevents unintended state changes, makes code thread-safe, simplifies reasoning about behavior, and aligns with DDD aggregate patterns.

### IV. Strongly-Typed Domain Modeling
**Primitive types MUST NOT represent domain concepts; use strongly-typed IDs and value objects.**

- Use strongly-typed IDs: `DocumentId`, `TenantId`, `QueueId`, `RuleId`
- Wrap business concepts in value objects: `FileSize`, `MimeType`, `ConfidenceScore`
- Implement validation within value object constructors
- Prevent primitive obsession throughout the domain layer

**Rationale**: Strongly-typed IDs prevent type confusion errors, enable compile-time safety, express business semantics clearly, and leverage the type system for validation.

### V. Comprehensive Testing (NON-NEGOTIABLE)
**Minimum 80% code coverage for Domain/Application layers; 100% for critical paths.**

- **Unit Tests**: Domain and Application layers using xUnit v3 and FakeItEasy for mocks
- **Integration Tests**: Infrastructure and API layers using Testcontainers for real dependencies
- **Performance Tests**: Use BenchmarkDotNet for critical operations; establish and enforce baselines
- **Security Tests**: Verify authentication, authorization, tenant isolation, input validation, encryption
- **Critical paths requiring 100% coverage**:
  - Document classification logic
  - Rule engine evaluation
  - Routing decisions
  - Security/access control
  - Tenant isolation logic
  - File encryption/decryption
- Follow TDD: Tests written → Tests fail → Implement → Tests pass
- Use test builders/factories for test data; avoid hardcoded values
- Run tests on every commit (unit), every PR (integration), nightly (performance)

**Rationale**: Document intake systems are mission-critical. Comprehensive testing ensures reliability, prevents regressions, validates security requirements, and enables confident refactoring.

### VI. Structured Logging & Observability
**All state changes and operations MUST be logged with structured data and correlation IDs.**

- Use structured logging with semantic log levels:
  - `LogInformation`: Document received, classification completed, routing finished
  - `LogWarning`: Rule matched multiple times, processing exceeded expected time
  - `LogError`: File processing failed, external API call failed, rule engine exception
- Include correlation IDs to track documents through entire pipeline
- Log performance metrics: document size, processing time, queue depth
- Maintain immutable audit trail for all document state changes (compliance requirement)
- Integrate with APM tools (Application Insights, Datadog, etc.)
- Track metrics: request rate, response time, error rate, queue depth, blob storage usage, external API success/failure

**Rationale**: Document processing involves asynchronous workflows and external integrations. Structured logging with correlation IDs enables tracing, debugging, compliance auditing, and operational visibility.

### VII. Performance & Scalability
**System MUST meet defined performance targets and scale horizontally in Kubernetes.**

- **Response Time Targets**:
  - Document upload API (metadata only): < 2s (p95)
  - Rule-based classification: < 500ms per document (p95)
  - Document routing: < 100ms per document (p95)
  - Query endpoints: < 200-500ms (p95)
- **Throughput Requirements**:
  - Document intake: 100 concurrent uploads
  - Classification processing: 100 documents/minute
  - Queue processing: 200 documents/minute
- **Large File Handling**:
  - Maximum file size: 50MB (configurable per tenant)
  - Use streaming for uploads/downloads (no full-file memory loading)
  - Chunked uploads for files > 10MB
  - Asynchronous processing via background jobs
- **Optimization Strategies**:
  - Cache classification rules (5-minute expiration)
  - Cache tenant configuration and feature flags
  - Use Redis for distributed caching in K8s
  - Database indexes on frequently queried fields (TenantId, DocumentId, Status, CreatedAt)
  - Pagination on all list endpoints (default: 20, max: 100)
  - Use `.AsNoTracking()` for read-only queries
  - Connection pooling (min: 5, max: 100)
- **Background Jobs**:
  - Use ABP Background Jobs framework
  - Implement retry policies with exponential backoff (max retries: 5)
  - Set per-job timeouts (classification: 30s, routing: 10s)
  - Monitor queue depth, processing time, failure rate
  - Apply back-pressure if queue depth > 1000
- **External APIs**:
  - Implement circuit breakers using Polly (open after 5 failures, half-open after 60s)
  - Set timeouts (Dropbox/Google Drive: 30s, AI service: 10s)
  - Respect rate limits with token bucket or sliding window
- **Kubernetes**:
  - Set resource limits (API pods: 512MB/500m CPU, Worker pods: 1GB/1000m CPU)
  - Implement liveness and readiness probes (`/health/live`, `/health/ready`)
  - Use horizontal pod autoscaling based on CPU/memory
  - Handle SIGTERM for graceful shutdown (30s grace period)

**Rationale**: DocFlow processes large files for multiple tenants with high volume. Performance targets ensure responsiveness, scalability enables growth, and K8s-specific practices ensure reliability in production.

## Security & Privacy Standards

### File Encryption & Key Management
- **MUST** encrypt all documents at rest in blob storage (AES-256)
- **MUST** use TLS 1.2+ for all API calls and blob transfers (encryption in transit)
- **MUST** use Azure Key Vault or AWS KMS for encryption key management
- **Consider** per-tenant encryption keys for enhanced isolation

### Access Control & Authorization
- **MUST** use ABP authentication (JWT tokens, IdentityServer, or external IdP)
- **MUST** enforce role-based access control (RBAC):
  - `Accounting` role: Upload invoices, view accounting documents
  - `LegalAssistant` role: Upload contracts, view legal documents
  - `Admin` role: Manage rules, queues, users
- **MUST** enforce tenant isolation: users can only access documents within their tenant
- **MUST** log all permission checks in audit trail

### Audit Trail Requirements
- **MUST** use ABP Auditing framework
- **MUST** log all document state changes:
  - Document uploaded (user, timestamp, file size, mime type)
  - Classification applied (rule ID, confidence score, timestamp)
  - Document routed (from queue, to queue, timestamp)
  - Document accessed (user, timestamp, action: view/download)
- Audit logs **MUST** be immutable (no modification or deletion allowed)
- **MUST** retain audit logs for at least 7 years (compliance requirement)

### Input Validation & Sanitization
- **MUST** validate all inputs: file names, file sizes, mime types, rule expressions
- **MUST** sanitize file names: remove special characters, prevent path traversal
- **MUST** validate rule expressions to prevent code injection
- **MUST** implement rate limiting per user/tenant to prevent abuse

## Performance Benchmarks & SLAs

### Response Time SLAs
- Document upload (metadata only, excluding blob transfer): < 2s (p95)
- Document classification status query: < 200ms (p95)
- Queue listing: < 500ms (p95)
- Rule CRUD operations: < 300ms (p95)
- Rule-based classification: < 500ms per document (p95)
- AI-enhanced classification (if enabled): < 5s per document (p95)
- Document routing: < 100ms per document (p95)

### Throughput & Scalability SLAs
- Support at least 100 concurrent document uploads
- Process at least 100 documents/minute (rule-based classification)
- Route at least 200 documents/minute
- System **MUST** scale horizontally (add more worker pods in K8s)

### Database Performance Requirements
- Use indexes on: TenantId, DocumentId, Status, CreatedAt
- All list endpoints **MUST** support pagination (default: 20, max: 100)
- Avoid N+1 queries (use eager loading or projections)
- Use `.AsNoTracking()` for read-only queries
- Connection pool: min 5, max 100
- Query timeout: default 30s (adjustable per query)

### Monitoring & Alerting Thresholds
- **Alert** if response time > 5s (p95)
- **Alert** if error rate > 5%
- **Alert** if queue depth > 5000
- **Alert** if blob storage > 80% capacity
- **Alert** if external API circuit breaker opens

## Governance

### Constitution Authority
This constitution supersedes all other development practices. When conflicts arise, constitutional principles take precedence. All architectural and design decisions **MUST** align with these principles.

### Amendment Process
1. Proposed amendments **MUST** be documented with rationale
2. Amendments require approval from technical lead and product owner
3. Version number **MUST** be updated according to semantic versioning:
   - **MAJOR**: Backward incompatible governance/principle removals or redefinitions
   - **MINOR**: New principle/section added or materially expanded guidance
   - **PATCH**: Clarifications, wording, typo fixes, non-semantic refinements
4. Amendments **MUST** include migration plan for existing code
5. All dependent templates (plan, spec, tasks) **MUST** be updated to reflect changes

### Compliance Review
- All PRs **MUST** verify compliance with constitutional principles (see Code Review Checklist in `.github/instructions/docflow-quality-standards.instructions.md`)
- Constitution check **MUST** pass before Phase 0 research in implementation plans
- Violations **MUST** be justified with documented rationale in "Complexity Tracking" section of plan
- Complexity introduced without justification will be rejected

### Runtime Development Guidance
For detailed implementation guidance, coding standards, and best practices, refer to:
- `.github/instructions/docflow-quality-standards.instructions.md` (Code Quality, Testing & Performance)
- `.github/instructions/domain-driven-design.instructions.md` (DDD patterns)
- `.github/instructions/clean-architecture.instructions.md` (Layering and dependencies)
- `.github/instructions/coding-style-csharp.instructions.md` (C# coding conventions)
- `.github/instructions/unit-and-integration-tests.instructions.md` (Testing practices)

**Version**: 1.0.0 | **Ratified**: 2025-11-09 | **Last Amended**: 2025-11-09

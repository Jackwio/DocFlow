# Implementation Tasks: Document Intake & Classification System

**Feature**: 001-document-intake-classification  
**Branch**: `feat/document-intake-classification`  
**Generated**: 2025-01-09  
**Spec**: [spec.md](spec.md) | [Plan](plan.md) | [Data Model](data-model.md)

---

## Task Legend

- **Format**: `- [ ] [TaskID] [P?] [Story?] Description (file path)`
- **[P]**: Parallelizable task (can be done concurrently with adjacent tasks)
- **[US#]**: Maps to User Story # from spec.md
- **Priority**: Stories executed in order P1 → P2 → P3

---

## Phase 0: Setup & Prerequisites

**Purpose**: Initialize project structure, dependencies, and development environment.

- [x] [T001] [P] Create feature branch `feat/document-intake-classification` from `production`
- [x] [T002] [P] Install UglyToad.PdfPig 0.1.8+ NuGet package (src/DocFlow.Domain/DocFlow.Domain.csproj)
- [x] [T003] [P] Install FluentValidation 11+ NuGet package (src/DocFlow.Application.Contracts/DocFlow.Application.Contracts.csproj)
- [x] [T004] [P] Install Polly 8+ NuGet package (src/DocFlow.Application/DocFlow.Application.csproj)
- [ ] [T005] [P] Configure Redis connection string in appsettings.json (src/DocFlow.HttpApi.Host/appsettings.json)
- [ ] [T006] [P] Configure Hangfire with ABP Background Jobs (src/DocFlow.HttpApi.Host/DocFlowHttpApiHostModule.cs)
- [ ] [T007] [P] Create Docker Compose file for PostgreSQL 16 + Redis 7 (docker-compose.yml)
- [ ] [T008] Verify ABP CLI version 8.3+ installed (`abp --version`)
- [ ] [T009] Run Docker Compose to start local dependencies (`docker-compose up -d`)
- [ ] [T010] Apply database migrations with DbMigrator (src/DocFlow.DbMigrator/Program.cs)

---

## Phase 1: Domain Model - Core Value Objects (Blocking)

**Purpose**: Create foundational value objects with business validation logic used by all aggregates.

- [x] [T011] [P] Create FileName value object with validation (src/DocFlow.Domain/Documents/FileName.cs)
- [x] [T012] [P] Create FileSize value object with range validation (src/DocFlow.Domain/Documents/FileSize.cs)
- [x] [T013] [P] Create MimeType value object with whitelist validation (src/DocFlow.Domain/Documents/MimeType.cs)
- [x] [T014] [P] Create BlobReference value object (src/DocFlow.Domain/Documents/BlobReference.cs)
- [x] [T015] [P] Create TagName value object (src/DocFlow.Domain/Documents/TagName.cs)
- [x] [T016] [P] Create ConfidenceScore value object (src/DocFlow.Domain/Documents/ConfidenceScore.cs)
- [x] [T017] [P] Create ErrorMessage value object (src/DocFlow.Domain/Shared/ErrorMessage.cs)
- [x] [T018] [P] Create DocumentStatus enum (Pending/Classified/Routed/Failed) (src/DocFlow.Domain.Shared/Enums/DocumentStatus.cs)
- [x] [T019] [P] Create TagSource enum (Automatic/Manual) (src/DocFlow.Domain.Shared/Enums/TagSource.cs)
- [x] [T020] [P] Create RuleConditionType enum (FileNameRegex/MimeType/FileSize/TextContent) (src/DocFlow.Domain.Shared/Enums/RuleConditionType.cs)
- [x] [T021] [P] Create QueueType enum (Folder/Webhook) (src/DocFlow.Domain.Shared/Enums/QueueType.cs)

---

## Phase 2: Domain Model - Document Aggregate (US1, US2, US3, US4, US10, US12)

**Purpose**: Implement Document aggregate root with all business methods.

- [x] [T022] Create Tag entity with Name and Source properties (src/DocFlow.Domain/Documents/Tag.cs)
- [x] [T023] Create ClassificationHistoryEntry entity with RuleId, TagName, MatchedCondition (src/DocFlow.Domain/Documents/ClassificationHistoryEntry.cs)
- [x] [T024] [US1] Create Document aggregate root with RegisterUpload factory method using Guid Id (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T025] [US2] Add Status property with private setter to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T026] [US2] Add ApplyClassificationResult method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T027] [US2] Add MarkAsRouted method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T028] [US3] Add RecordClassificationFailure method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T029] [US3] Add RetryClassification method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T030] [US10] Add AddManualTag method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T031] [US10] Add RemoveManualTag method to Document (src/DocFlow.Domain/Documents/Document.cs)
- [x] [T032] [US1] Create DocumentUploadedEvent domain event (src/DocFlow.Domain/Documents/Events/DocumentUploadedEvent.cs)
- [x] [T033] [US2] Create DocumentClassifiedEvent domain event (src/DocFlow.Domain/Documents/Events/DocumentClassifiedEvent.cs)
- [x] [T034] [US2] Create DocumentRoutedEvent domain event (src/DocFlow.Domain/Documents/Events/DocumentRoutedEvent.cs)
- [x] [T035] [US3] Create DocumentClassificationFailedEvent domain event (src/DocFlow.Domain/Documents/Events/DocumentClassificationFailedEvent.cs)
- [x] [T036] [US3] Create DocumentRetryInitiatedEvent domain event (src/DocFlow.Domain/Documents/Events/DocumentRetryInitiatedEvent.cs)
- [x] [T037] [US10] Create ManualTagAddedEvent domain event (src/DocFlow.Domain/Documents/Events/ManualTagAddedEvent.cs)
- [x] [T038] [US10] Create ManualTagRemovedEvent domain event (src/DocFlow.Domain/Documents/Events/ManualTagRemovedEvent.cs)
- [x] [T039] Create IDocumentRepository interface (src/DocFlow.Domain/Documents/IDocumentRepository.cs)
- [x] [T040] [US4] Add FindByStatusAsync method to IDocumentRepository (src/DocFlow.Domain/Documents/IDocumentRepository.cs)
- [x] [T041] [US4] Add SearchAsync method with filtering to IDocumentRepository (src/DocFlow.Domain/Documents/IDocumentRepository.cs)

---

## Phase 3: Domain Model - ClassificationRule Aggregate (US5, US6, US7, US8)

**Purpose**: Implement classification rule aggregate with condition matching logic.

- [x] [T042] Create RuleCondition value object with Type, Pattern, MatchValue (src/DocFlow.Domain/ClassificationRules/RuleCondition.cs)
- [x] [T043] Create RulePriority value object (src/DocFlow.Domain/ClassificationRules/RulePriority.cs)
- [x] [T044] [US5] Create ClassificationRule aggregate root with DefineRule factory method using Guid Id (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T045] [US5] Add ApplyTags list property to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T046] [US5] Add Conditions list property to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T047] [US5] Add UpdateConditions method to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T048] [US5] Add UpdateTags method to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T049] [US7] Add UpdatePriority method to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T050] [US8] Add Activate/Deactivate methods to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T051] [US5] Add MatchesAsync method to ClassificationRule (src/DocFlow.Domain/ClassificationRules/ClassificationRule.cs)
- [x] [T052] [US5] Create ClassificationRuleCreatedEvent domain event (src/DocFlow.Domain/ClassificationRules/Events/ClassificationRuleCreatedEvent.cs)
- [x] [T053] [US5] Create ClassificationRuleUpdatedEvent domain event (src/DocFlow.Domain/ClassificationRules/Events/ClassificationRuleUpdatedEvent.cs)
- [x] [T054] [US8] Create ClassificationRuleActivatedEvent domain event (src/DocFlow.Domain/ClassificationRules/Events/ClassificationRuleActivatedEvent.cs)
- [x] [T055] [US8] Create ClassificationRuleDeactivatedEvent domain event (src/DocFlow.Domain/ClassificationRules/Events/ClassificationRuleDeactivatedEvent.cs)
- [x] [T056] Create IClassificationRuleRepository interface (src/DocFlow.Domain/ClassificationRules/IClassificationRuleRepository.cs)
- [x] [T057] [US5] Add GetActiveRulesAsync method to IClassificationRuleRepository (src/DocFlow.Domain/ClassificationRules/IClassificationRuleRepository.cs)

---

## Phase 4: Domain Model - RoutingQueue Aggregate (US9, US11)

**Purpose**: Implement routing queue aggregate for folder and webhook destinations.

- [x] [T058] Create WebhookConfiguration value object with URL, headers, retry policy (src/DocFlow.Domain/RoutingQueues/WebhookConfiguration.cs)
- [x] [T059] Create FolderPath value object with path validation (src/DocFlow.Domain/RoutingQueues/FolderPath.cs)
- [x] [T060] [US9] Create RoutingQueue aggregate root with CreateFolderQueue factory method using Guid Id (src/DocFlow.Domain/RoutingQueues/RoutingQueue.cs)
- [x] [T061] [US9] Add CreateWebhookQueue factory method to RoutingQueue (src/DocFlow.Domain/RoutingQueues/RoutingQueue.cs)
- [x] [T062] [US9] Add UpdateDestination method to RoutingQueue (src/DocFlow.Domain/RoutingQueues/RoutingQueue.cs)
- [x] [T063] [US11] Create WebhookDelivery entity with DocumentId, AttemptCount, Status, LastError (src/DocFlow.Domain/RoutingQueues/WebhookDelivery.cs)
- [x] [T064] [US11] Add RecordDeliveryAttempt method to WebhookDelivery (src/DocFlow.Domain/RoutingQueues/WebhookDelivery.cs)
- [x] [T065] [US11] Add RetryDelivery method to WebhookDelivery (src/DocFlow.Domain/RoutingQueues/WebhookDelivery.cs)
- [x] [T066] Create IRoutingQueueRepository interface (src/DocFlow.Domain/RoutingQueues/IRoutingQueueRepository.cs)
- [x] [T067] [US11] Create IWebhookDeliveryRepository interface with FindFailedDeliveriesAsync (src/DocFlow.Domain/RoutingQueues/IWebhookDeliveryRepository.cs)

---

## Phase 5: Domain Services (US1, US5, US6, US9)

**Purpose**: Implement domain services for classification and routing logic.

- [x] [T068] [US5] Create ClassificationRuleManager domain service (src/DocFlow.Domain/ClassificationRules/ClassificationRuleManager.cs)
- [x] [T069] [US5] Add EvaluateRulesAsync method to ClassificationRuleManager (src/DocFlow.Domain/ClassificationRules/ClassificationRuleManager.cs)
- [x] [T070] [US6] Add EvaluateRuleInDryRunModeAsync method to ClassificationRuleManager (src/DocFlow.Domain/ClassificationRules/ClassificationRuleManager.cs)
- [x] [T071] [US1] Create PdfTextExtractionManager domain service using PdfPig (src/DocFlow.Domain/Documents/PdfTextExtractionManager.cs)
- [x] [T072] [US1] Add ExtractTextAsync method to PdfTextExtractionManager (src/DocFlow.Domain/Documents/PdfTextExtractionManager.cs)
- [x] [T073] [US9] Create RoutingManager domain service (src/DocFlow.Domain/RoutingQueues/RoutingManager.cs)
- [x] [T074] [US9] Add RouteDocumentToQueueAsync method to RoutingManager (src/DocFlow.Domain/RoutingQueues/RoutingManager.cs)

---

## Phase 6: Infrastructure - EF Core Repositories

**Purpose**: Implement EF Core repositories and database mappings.

- [x] [T075] [P] Configure Document entity mapping with owned entities (src/DocFlow.EntityFrameworkCore/EntityConfigurations/DocumentConfiguration.cs)
- [x] [T076] [P] Configure ClassificationRule entity mapping (src/DocFlow.EntityFrameworkCore/EntityConfigurations/ClassificationRuleConfiguration.cs)
- [x] [T077] [P] Configure RoutingQueue entity mapping (src/DocFlow.EntityFrameworkCore/EntityConfigurations/RoutingQueueConfiguration.cs)
- [x] [T078] [P] Configure WebhookDelivery entity mapping (src/DocFlow.EntityFrameworkCore/EntityConfigurations/WebhookDeliveryConfiguration.cs)
- [x] [T079] [P] Create value object converters for FileName, FileSize, MimeType, etc. (src/DocFlow.EntityFrameworkCore/ValueConverters/)
- [x] [T080] Implement EfCoreDocumentRepository (src/DocFlow.EntityFrameworkCore/Documents/EfCoreDocumentRepository.cs)
- [x] [T081] [US4] Implement SearchAsync with IQueryable filters in EfCoreDocumentRepository (src/DocFlow.EntityFrameworkCore/Documents/EfCoreDocumentRepository.cs)
- [x] [T082] Implement EfCoreClassificationRuleRepository (src/DocFlow.EntityFrameworkCore/ClassificationRules/EfCoreClassificationRuleRepository.cs)
- [x] [T083] [US5] Implement GetActiveRulesAsync with caching in EfCoreClassificationRuleRepository (src/DocFlow.EntityFrameworkCore/ClassificationRules/EfCoreClassificationRuleRepository.cs)
- [x] [T084] Implement EfCoreRoutingQueueRepository (src/DocFlow.EntityFrameworkCore/RoutingQueues/EfCoreRoutingQueueRepository.cs)
- [x] [T085] [US11] Implement EfCoreWebhookDeliveryRepository (src/DocFlow.EntityFrameworkCore/RoutingQueues/EfCoreWebhookDeliveryRepository.cs)
- [ ] [T086] Create database migration for Document/Rule/Queue tables (`Add-Migration InitialDocumentIntake`)

---

## Phase 7: Application Contracts - DTOs (All Stories)

**Purpose**: Create DTOs for all application services.

- [x] [T091] [P] [US1] Create UploadDocumentDto with file validation (src/DocFlow.Application.Contracts/Documents/Dtos/UploadDocumentDto.cs)
- [x] [T092] [P] [US1] Create DocumentDto with Status, Tags, LastError (src/DocFlow.Application.Contracts/Documents/Dtos/DocumentDto.cs)
- [x] [T093] [P] [US2] Create DocumentListDto for list views (src/DocFlow.Application.Contracts/Documents/Dtos/DocumentListDto.cs)
- [x] [T094] [P] [US4] Create DocumentSearchDto with filtering parameters (src/DocFlow.Application.Contracts/Documents/Dtos/DocumentSearchDto.cs)
- [x] [T095] [P] [US5] Create ClassificationRuleDto (src/DocFlow.Application.Contracts/ClassificationRules/Dtos/ClassificationRuleDto.cs)
- [x] [T096] [P] [US5] Create CreateClassificationRuleDto (src/DocFlow.Application.Contracts/ClassificationRules/Dtos/CreateClassificationRuleDto.cs)
- [x] [T097] [P] [US5] Create UpdateClassificationRuleDto (src/DocFlow.Application.Contracts/ClassificationRules/Dtos/UpdateClassificationRuleDto.cs)
- [x] [T098] [P] [US6] Create DryRunResultDto with matched rules (src/DocFlow.Application.Contracts/ClassificationRules/Dtos/DryRunResultDto.cs)
- [x] [T099] [P] [US9] Create RoutingQueueDto (src/DocFlow.Application.Contracts/RoutingQueues/Dtos/RoutingQueueDto.cs)
- [x] [T100] [P] [US9] Create CreateRoutingQueueDto (src/DocFlow.Application.Contracts/RoutingQueues/Dtos/CreateRoutingQueueDto.cs)
- [x] [T101] [P] [US10] Create AddManualTagDto (src/DocFlow.Application.Contracts/Documents/Dtos/AddManualTagDto.cs)
- [x] [T102] [P] [US11] Create WebhookDeliveryDto (src/DocFlow.Application.Contracts/RoutingQueues/Dtos/WebhookDeliveryDto.cs)
- [x] [T103] [P] [US12] Create ClassificationHistoryDto (src/DocFlow.Application.Contracts/Documents/Dtos/ClassificationHistoryDto.cs)

---

## Phase 8: Application Layer - US1 (Document Upload) [P1]

**Purpose**: Implement document upload with blob storage and validation.

- [x] [T104] [US1] Create DocumentManagement folder (src/DocFlow.Application/DocumentManagement/)
- [x] [T105] [US1] Create DocumentApplicationService class (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T106] [US1] Implement UploadDocumentAsync method with blob storage (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T107] [US1] Add file MIME type validation in UploadDocumentAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T108] [US1] Add file size validation (50MB max) in UploadDocumentAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T109] [US1] Implement UploadBatchDocumentsAsync for batch upload (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T110] [US1] Create DocumentUploadedEventHandler to trigger classification job (src/DocFlow.Application/DocumentManagement/DocumentUploadedEventHandler.cs)
- [x] [T111] [US1] Create AutoMapper profile for Document → DocumentDto (src/DocFlow.Application/DocFlowApplicationAutoMapperProfile.cs)

---

## Phase 9: Application Layer - US2 (Status Tracking) [P1]

**Purpose**: Implement document listing and status filtering.

- [x] [T112] [US2] Implement GetDocumentAsync by ID (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T113] [US2] Implement GetDocumentListAsync with pagination (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T114] [US2] Add status filter parameter to GetDocumentListAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T115] [US2] Add date range filter to GetDocumentListAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)

---

## Phase 10: Application Layer - US3 (Failed Retry) [P1]

**Purpose**: Implement manual retry for failed documents.

- [x] [T116] [US3] Implement RetryClassificationAsync method (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T117] [US3] Add validation to ensure document is in Failed status (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T118] [US3] Trigger classification job after retry (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)

---

## Phase 11: Application Layer - US4 (Search & Filter) [P1]

**Purpose**: Implement advanced document search.

- [x] [T119] [US4] Implement SearchDocumentsAsync method (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T120] [US4] Add tag filtering to SearchDocumentsAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T121] [US4] Add filename search to SearchDocumentsAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [x] [T122] [US4] Add multi-status filtering to SearchDocumentsAsync (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)

---

## Phase 12: Background Jobs - Classification & Routing

**Purpose**: Implement async classification and routing jobs.

- [ ] [T123] [US1] Create ClassifyDocumentJob with Hangfire integration (src/DocFlow.Application/DocumentManagement/Jobs/ClassifyDocumentJob.cs)
- [ ] [T124] [US1] Call PdfTextExtractionManager in ClassifyDocumentJob (src/DocFlow.Application/DocumentManagement/Jobs/ClassifyDocumentJob.cs)
- [ ] [T125] [US1] Call ClassificationRuleManager.EvaluateRulesAsync in ClassifyDocumentJob (src/DocFlow.Application/DocumentManagement/Jobs/ClassifyDocumentJob.cs)
- [ ] [T126] [US1] Apply classification result to Document aggregate (src/DocFlow.Application/DocumentManagement/Jobs/ClassifyDocumentJob.cs)
- [ ] [T127] [US1] Handle classification failures with error logging (src/DocFlow.Application/DocumentManagement/Jobs/ClassifyDocumentJob.cs)
- [ ] [T128] [US9] Create RouteDocumentJob for routing logic (src/DocFlow.Application/DocumentManagement/Jobs/RouteDocumentJob.cs)
- [ ] [T129] [US9] Implement folder-based routing in RouteDocumentJob (src/DocFlow.Application/DocumentManagement/Jobs/RouteDocumentJob.cs)
- [ ] [T130] [US9] Implement webhook-based routing with Polly retry in RouteDocumentJob (src/DocFlow.Application/DocumentManagement/Jobs/RouteDocumentJob.cs)
- [ ] [T131] [US2] Create DocumentClassifiedEventHandler to trigger RouteDocumentJob (src/DocFlow.Application/DocumentManagement/DocumentClassifiedEventHandler.cs)

---

## Phase 13: Application Layer - US5 (Rule Management) [P2]

**Purpose**: Implement CRUD for classification rules.

- [ ] [T132] [US5] Create ClassificationRuleManagement folder (src/DocFlow.Application/ClassificationRuleManagement/)
- [ ] [T133] [US5] Create ClassificationRuleApplicationService class (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T134] [US5] Implement CreateRuleAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T135] [US5] Implement UpdateRuleAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T136] [US5] Implement DeleteRuleAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T137] [US5] Implement GetRuleAsync by ID (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T138] [US5] Implement GetRuleListAsync with pagination (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T139] [US5] Add AutoMapper profile for ClassificationRule → ClassificationRuleDto (src/DocFlow.Application/DocFlowApplicationAutoMapperProfile.cs)
- [ ] [T140] [US5] Add validation for regex patterns to prevent ReDoS attacks (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)

---

## Phase 14: Application Layer - US6 (Dry-Run Testing) [P2]

**Purpose**: Implement dry-run mode for testing rules.

- [ ] [T141] [US6] Implement TestRuleInDryRunAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T142] [US6] Call ClassificationRuleManager.EvaluateRuleInDryRunModeAsync (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T143] [US6] Return matched conditions without applying tags (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)

---

## Phase 15: Application Layer - US7 (Priority Management) [P2]

**Purpose**: Implement rule priority updates.

- [ ] [T144] [US7] Implement UpdateRulePriorityAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T145] [US7] Add validation to ensure priority is unique per tenant (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T146] [US7] Invalidate Redis cache after priority change (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)

---

## Phase 16: Application Layer - US8 (Enable/Disable Rules) [P3]

**Purpose**: Implement rule activation toggle.

- [ ] [T147] [US8] Implement ActivateRuleAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T148] [US8] Implement DeactivateRuleAsync method (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)
- [ ] [T149] [US8] Invalidate Redis cache after activation state change (src/DocFlow.Application/ClassificationRuleManagement/ClassificationRuleApplicationService.cs)

---

## Phase 17: Application Layer - US9 (Queue Management) [P2]

**Purpose**: Implement CRUD for routing queues.

- [ ] [T150] [US9] Create RoutingQueueManagement folder (src/DocFlow.Application/RoutingQueueManagement/)
- [ ] [T151] [US9] Create RoutingQueueApplicationService class (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T152] [US9] Implement CreateFolderQueueAsync method (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T153] [US9] Implement CreateWebhookQueueAsync method (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T154] [US9] Implement UpdateQueueAsync method (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T155] [US9] Implement DeleteQueueAsync method (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T156] [US9] Implement GetQueueListAsync with pagination (src/DocFlow.Application/RoutingQueueManagement/RoutingQueueApplicationService.cs)
- [ ] [T157] [US9] Add AutoMapper profile for RoutingQueue → RoutingQueueDto (src/DocFlow.Application/DocFlowApplicationAutoMapperProfile.cs)

---

## Phase 18: Application Layer - US10 (Manual Tags) [P3]

**Purpose**: Implement manual tag adjustments.

- [ ] [T158] [US10] Implement AddManualTagAsync method (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T159] [US10] Implement RemoveManualTagAsync method (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T160] [US10] Add validation to prevent duplicate tag names (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)

---

## Phase 19: Application Layer - US11 (Webhook Monitoring) [P3]

**Purpose**: Implement webhook delivery tracking.

- [ ] [T161] [US11] Create WebhookDeliveryManagement folder (src/DocFlow.Application/WebhookDeliveryManagement/)
- [ ] [T162] [US11] Create WebhookDeliveryApplicationService class (src/DocFlow.Application/WebhookDeliveryManagement/WebhookDeliveryApplicationService.cs)
- [ ] [T163] [US11] Implement GetFailedDeliveriesAsync method (src/DocFlow.Application/WebhookDeliveryManagement/WebhookDeliveryApplicationService.cs)
- [ ] [T164] [US11] Implement RetryWebhookDeliveryAsync method (src/DocFlow.Application/WebhookDeliveryManagement/WebhookDeliveryApplicationService.cs)
- [ ] [T165] [US11] Add AutoMapper profile for WebhookDelivery → WebhookDeliveryDto (src/DocFlow.Application/DocFlowApplicationAutoMapperProfile.cs)

---

## Phase 20: Application Layer - US12 (Classification Audit) [P3]

**Purpose**: Implement classification history display.

- [ ] [T166] [US12] Implement GetClassificationHistoryAsync method (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T167] [US12] Return list of matched rules with conditions (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T168] [US12] Add AutoMapper profile for ClassificationHistoryEntry → ClassificationHistoryDto (src/DocFlow.Application/DocFlowApplicationAutoMapperProfile.cs)

---

## Phase 21: HTTP API Layer - Controllers

**Purpose**: Expose REST APIs for all application services.

- [x] [T169] [P] [US1] Create DocumentsController with UploadAsync endpoint (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T170] [P] [US1] Add UploadBatchAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T171] [P] [US2] Add GetAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T172] [P] [US2] Add GetListAsync endpoint with status filter to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T173] [P] [US3] Add RetryClassificationAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T174] [P] [US4] Add SearchAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T175] [P] [US10] Add AddManualTagAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T176] [P] [US10] Add RemoveManualTagAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [x] [T177] [P] [US12] Add GetClassificationHistoryAsync endpoint to DocumentsController (src/DocFlow.HttpApi.Host/Controllers/DocumentsController.cs)
- [ ] [T178] [P] [US5] Create ClassificationRulesController with CRUD endpoints (src/DocFlow.HttpApi.Host/Controllers/ClassificationRulesController.cs)
- [ ] [T179] [P] [US6] Add TestRuleInDryRunAsync endpoint to ClassificationRulesController (src/DocFlow.HttpApi.Host/Controllers/ClassificationRulesController.cs)
- [ ] [T180] [P] [US7] Add UpdatePriorityAsync endpoint to ClassificationRulesController (src/DocFlow.HttpApi.Host/Controllers/ClassificationRulesController.cs)
- [ ] [T181] [P] [US8] Add ActivateAsync/DeactivateAsync endpoints to ClassificationRulesController (src/DocFlow.HttpApi.Host/Controllers/ClassificationRulesController.cs)
- [ ] [T182] [P] [US9] Create RoutingQueuesController with CRUD endpoints (src/DocFlow.HttpApi.Host/Controllers/RoutingQueuesController.cs)
- [ ] [T183] [P] [US11] Create WebhookDeliveriesController with monitoring endpoints (src/DocFlow.HttpApi.Host/Controllers/WebhookDeliveriesController.cs)

---

## Phase 22: Unit Tests - Domain Layer

**Purpose**: Test domain aggregates, value objects, and domain services.

- [ ] [T184] [P] Create DocumentTests with RegisterUpload factory method test (test/DocFlow.Domain.Tests/Documents/DocumentTests.cs)
- [ ] [T185] [P] [US2] Add ApplyClassificationResult test to DocumentTests (test/DocFlow.Domain.Tests/Documents/DocumentTests.cs)
- [ ] [T186] [P] [US3] Add RetryClassification test to DocumentTests (test/DocFlow.Domain.Tests/Documents/DocumentTests.cs)
- [ ] [T187] [P] [US10] Add AddManualTag test to DocumentTests (test/DocFlow.Domain.Tests/Documents/DocumentTests.cs)
- [ ] [T188] [P] Create ClassificationRuleTests with DefineRule factory method test (test/DocFlow.Domain.Tests/ClassificationRules/ClassificationRuleTests.cs)
- [ ] [T189] [P] [US5] Add MatchesAsync test with filename regex to ClassificationRuleTests (test/DocFlow.Domain.Tests/ClassificationRules/ClassificationRuleTests.cs)
- [ ] [T190] [P] [US8] Add Activate/Deactivate test to ClassificationRuleTests (test/DocFlow.Domain.Tests/ClassificationRules/ClassificationRuleTests.cs)
- [ ] [T191] [P] Create RoutingQueueTests with CreateFolderQueue factory method test (test/DocFlow.Domain.Tests/RoutingQueues/RoutingQueueTests.cs)
- [ ] [T192] [P] Create ClassificationRuleManagerTests (test/DocFlow.Domain.Tests/ClassificationRules/ClassificationRuleManagerTests.cs)
- [ ] [T193] [P] [US5] Add EvaluateRulesAsync test with priority ordering to ClassificationRuleManagerTests (test/DocFlow.Domain.Tests/ClassificationRules/ClassificationRuleManagerTests.cs)
- [ ] [T194] [P] Create PdfTextExtractionManagerTests (test/DocFlow.Domain.Tests/Documents/PdfTextExtractionManagerTests.cs)

---

## Phase 23: Integration Tests - Application Layer

**Purpose**: Test application services with real database and background jobs.

- [ ] [T195] [P] Create DocumentApplicationServiceTests with Testcontainers (test/DocFlow.Application.Tests/DocumentManagement/DocumentApplicationServiceTests.cs)
- [ ] [T196] [P] [US1] Add UploadDocumentAsync integration test (test/DocFlow.Application.Tests/DocumentManagement/DocumentApplicationServiceTests.cs)
- [ ] [T197] [P] [US2] Add GetDocumentListAsync with status filter test (test/DocFlow.Application.Tests/DocumentManagement/DocumentApplicationServiceTests.cs)
- [ ] [T198] [P] [US3] Add RetryClassificationAsync integration test (test/DocFlow.Application.Tests/DocumentManagement/DocumentApplicationServiceTests.cs)
- [ ] [T199] [P] [US4] Add SearchDocumentsAsync integration test (test/DocFlow.Application.Tests/DocumentManagement/DocumentApplicationServiceTests.cs)
- [ ] [T200] [P] Create ClassificationRuleApplicationServiceTests (test/DocFlow.Application.Tests/ClassificationRuleManagement/ClassificationRuleApplicationServiceTests.cs)
- [ ] [T201] [P] [US5] Add CreateRuleAsync integration test (test/DocFlow.Application.Tests/ClassificationRuleManagement/ClassificationRuleApplicationServiceTests.cs)
- [ ] [T202] [P] [US6] Add TestRuleInDryRunAsync integration test (test/DocFlow.Application.Tests/ClassificationRuleManagement/ClassificationRuleApplicationServiceTests.cs)
- [ ] [T203] [P] Create RoutingQueueApplicationServiceTests (test/DocFlow.Application.Tests/RoutingQueueManagement/RoutingQueueApplicationServiceTests.cs)
- [ ] [T204] [P] [US9] Add CreateFolderQueueAsync integration test (test/DocFlow.Application.Tests/RoutingQueueManagement/RoutingQueueApplicationServiceTests.cs)

---

## Phase 24: Performance & Security

**Purpose**: Optimize performance and secure endpoints.

- [ ] [T205] [P] Add Redis caching for GetActiveRulesAsync (src/DocFlow.EntityFrameworkCore/ClassificationRules/EfCoreClassificationRuleRepository.cs)
- [ ] [T206] [P] Configure ABP permissions for Document endpoints (src/DocFlow.Application.Contracts/Permissions/DocFlowPermissions.cs)
- [ ] [T207] [P] Configure ABP permissions for ClassificationRule endpoints (src/DocFlow.Application.Contracts/Permissions/DocFlowPermissions.cs)
- [ ] [T208] [P] Add authorization checks in DocumentApplicationService (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T209] [P] Add rate limiting for document upload endpoint (src/DocFlow.HttpApi.Host/Startup.cs)
- [ ] [T210] [P] Add streaming upload for large files (src/DocFlow.Application/DocumentManagement/DocumentApplicationService.cs)
- [ ] [T211] [P] Add database indexes for Document.Status, Document.TenantId (src/DocFlow.EntityFrameworkCore/Migrations/)
- [ ] [T212] [P] Run BenchmarkDotNet tests for ClassifyDocumentJob (test/DocFlow.Application.Tests/Performance/)

---

## Phase 25: Documentation & Polish

**Purpose**: Finalize documentation and code quality.

- [ ] [T213] [P] Add XML documentation comments to all public APIs (src/DocFlow.Application/)
- [ ] [T214] [P] Generate Swagger/OpenAPI documentation (src/DocFlow.HttpApi.Host/Startup.cs)
- [ ] [T215] [P] Update README.md with feature overview (README.md)
- [ ] [T216] [P] Add quickstart guide to docs/ (docs/quickstart-document-intake.md)
- [ ] [T217] [P] Run code analysis and fix warnings (dotnet build /p:TreatWarningsAsErrors=true)
- [ ] [T218] [P] Format code with dotnet format (dotnet format)
- [ ] [T219] [P] Verify all tests pass (dotnet test)
- [ ] [T220] Create pull request with feature branch

---

## Task Summary

**Total Tasks**: 216  
**Parallelizable**: 91 tasks marked with [P]  
**Critical Path**: Phase 0-6 (Setup → Domain → Infrastructure) must complete before application layer

### Tasks by Priority:
- **P1 (MVP)**: 68 tasks (US1-US4: Upload, Status, Retry, Search)
- **P2**: 58 tasks (US5-US7, US9: Rules Management, Dry-Run, Priority, Queues)
- **P3**: 30 tasks (US8, US10-US12: Enable/Disable, Manual Tags, Webhooks, Audit)
- **Cross-Cutting**: 60 tasks (Setup, Infrastructure, Tests, Polish)

### Story Completion Order:
1. **Phase 0-7**: Foundation (Setup, Domain, Infrastructure, DTOs)
2. **Phase 8-11**: US1-US4 (P1 stories)
3. **Phase 12**: Background Jobs (Classification & Routing)
4. **Phase 13-17**: US5-US9 (P2 stories)
5. **Phase 18-20**: US8, US10-US12 (P3 stories)
6. **Phase 21-25**: API, Tests, Performance, Documentation

### MVP Scope (US1 Only):
If implementing minimal viable product, complete only:
- Phase 0-7 (Foundation): T001-T099
- Phase 8 (US1 Upload): T100-T107
- Phase 12 (Classification Job): T119-T123
- Phase 21 (API): T165-T166
- Phase 22-23 (Tests): T180-T192
Total: ~66 tasks for MVP

---

## Dependency Graph

```
Phase 0 (Setup)
    ↓
Phase 1 (Value Objects) ← BLOCKING
    ↓
Phase 2-4 (Aggregates) ← CAN PARALLELIZE
    ↓
Phase 5 (Domain Services)
    ↓
Phase 6 (Repositories) ← BLOCKING
    ↓
Phase 7 (DTOs) ← CAN PARALLELIZE
    ↓
Phase 8-11 (US1-US4 Application) ← CAN PARALLELIZE BY STORY
    ↓
Phase 12 (Background Jobs) ← BLOCKING
    ↓
Phase 13-20 (US5-US12 Application) ← CAN PARALLELIZE BY STORY
    ↓
Phase 21 (Controllers) ← CAN PARALLELIZE
    ↓
Phase 22-23 (Tests) ← CAN PARALLELIZE
    ↓
Phase 24-25 (Polish) ← CAN PARALLELIZE
```

---

## Next Steps

1. Review and approve this task breakdown
2. Create feature branch: `feat/document-intake-classification`
3. Start with Phase 0 (Setup) to verify environment
4. Begin Phase 1 (Value Objects) - foundational work
5. Parallelize Phase 2-4 (Aggregates) across team members
6. Continue sequential execution through Phase 6
7. Parallelize Phase 8-11 by user story (one developer per story)

**Estimated Timeline**: 4-6 weeks for full implementation (all 216 tasks)  
**MVP Timeline**: 1-2 weeks (US1 only, ~66 tasks)

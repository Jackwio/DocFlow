# Feature Specification: Document Intake & Classification System

**Feature Branch**: `001-document-intake-classification`  
**Created**: 2025-11-09  
**Status**: Draft  
**Input**: User description: "作為操作人員，我想上傳 PDF/影像文件到指定 Inbox 以便進入分類流程。作為操作人員，我想一次選取多檔並批次上傳以節省時間。作為操作人員，我想查看每個文件的當前狀態（Pending、Classified、Routed、Failed）以便決定下一步。作為操作人員，我想針對失敗文件手動重試分類以快速修復。作為操作人員，我想建立分類規則（以檔名 regex、MIME、大小、簡易文本片段）以自動加上標籤與路由。作為操作人員，我想在建立規則時進行 Dry-run 測試一個文件以驗證命中結果。作為操作人員，我想調整規則優先順序以便控制執行順序。作為操作人員，我想停用規則但保留設定以便後續再啟用。作為操作人員，我想新增路由隊列（Folder/Webhook）以便分類後自動分派。作為操作人員，我想在文件已分類後手動加減標籤調整結果。作為操作人員，我想查看 Webhook 送達紀錄與重試按鈕以處理外部系統故障。作為操作人員，我想查看單一文件的規則命中紀錄以瞭解分類決策。作為操作人員，我想在列表使用搜尋與過濾（標籤、日期、狀態）以提高查找效率。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Document Upload to Inbox (Priority: P1)

Operators need to upload PDF and image files to a designated inbox to initiate the classification workflow. This includes both single file and batch multi-file uploads to save time and improve operational efficiency.

**Why this priority**: This is the entry point for all documents into the system. Without the ability to upload documents, no other functionality can be utilized. This represents the core value proposition and must be delivered first to have a viable MVP.

**Independent Test**: Can be fully tested by uploading a single PDF file to an inbox, verifying the file is received and stored, and confirming the document appears in the document list with "Pending" status. Batch upload can be tested by selecting multiple files and verifying all are uploaded successfully.

**Acceptance Scenarios**:

1. **Given** an operator is logged into the system, **When** they select a single PDF file from their computer and upload it to the "Accounting" inbox, **Then** the document appears in the document list with status "Pending" and shows the correct filename, file size, and upload timestamp
2. **Given** an operator is viewing the upload interface, **When** they select multiple files (3 PDFs, 2 images) and initiate batch upload, **Then** all 5 files are uploaded successfully, each appears as a separate document with "Pending" status, and a progress indicator shows upload completion
3. **Given** an operator attempts to upload a file, **When** the file exceeds the maximum size limit (50MB), **Then** the system rejects the upload with a clear error message stating "File exceeds maximum size of 50MB"
4. **Given** an operator attempts to upload a file, **When** the file format is not supported (e.g., .exe, .zip), **Then** the system rejects the upload with an error message listing supported formats (PDF, PNG, JPG, JPEG, TIFF)
5. **Given** an operator is uploading a large file (40MB), **When** the upload is in progress, **Then** a progress bar shows the upload percentage and allows the operator to cancel if needed

---

### User Story 2 - Document Status Tracking (Priority: P1)

Operators need to view the current status of each document (Pending, Classified, Routed, Failed) to understand its position in the workflow and determine appropriate next actions. This includes the ability to filter and search documents by status, tags, and date.

**Why this priority**: Status visibility is critical for operational monitoring and intervention. Operators must know which documents require attention, especially failed documents. This is essential for the MVP as it enables operators to monitor the system's operation.

**Independent Test**: Can be fully tested by uploading several documents, triggering classification (manual or automatic), and verifying that the document list accurately displays the status of each document. Search and filter functionality can be tested by applying various filters and verifying results match the criteria.

**Acceptance Scenarios**:

1. **Given** multiple documents exist in the system with different statuses, **When** an operator views the document list, **Then** each document displays its current status (Pending, Classified, Routed, Failed) with a clear visual indicator (color coding or icon)
2. **Given** an operator is viewing the document list, **When** they apply a status filter "Failed", **Then** only documents with "Failed" status are displayed
3. **Given** documents have been tagged with labels like "Invoice" and "Contract", **When** an operator searches for tag "Invoice", **Then** only documents with the "Invoice" tag are displayed
4. **Given** documents were uploaded over the past week, **When** an operator filters by date range (last 7 days), **Then** only documents uploaded within that timeframe are displayed
5. **Given** an operator is viewing the document list, **When** they click on a document, **Then** a detailed view opens showing the document's full metadata, status history, applied tags, classification rules that matched, and routing destination

---

### User Story 3 - Failed Document Retry (Priority: P1)

Operators need to manually retry classification for failed documents to quickly resolve processing errors without re-uploading the document. This enables rapid recovery from transient failures or system issues.

**Why this priority**: Documents can fail for various reasons (malformed PDF, external service timeout, rule engine error). Operators must be able to recover without losing the document or requiring re-upload. This is critical for operational reliability and must be in the MVP.

**Independent Test**: Can be fully tested by creating a scenario where a document fails classification (simulate by temporarily disabling a rule or service), verifying the document shows "Failed" status with an error message, then using the retry button to re-initiate classification and confirming the document processes successfully.

**Acceptance Scenarios**:

1. **Given** a document has status "Failed" due to a classification error, **When** an operator views the document details, **Then** the error message is displayed clearly explaining the failure reason (e.g., "Classification rule engine timeout")
2. **Given** a document has status "Failed", **When** an operator clicks the "Retry Classification" button, **Then** the document status changes to "Pending", classification is re-initiated, and the operator receives confirmation that retry has been queued
3. **Given** a document failed and is being retried, **When** the retry completes successfully, **Then** the document status updates to "Classified" and the previously displayed error is cleared from the document details
4. **Given** a document failed and is being retried, **When** the retry fails again, **Then** the document remains in "Failed" status, the new error message is displayed, and the operator can retry again or escalate the issue
5. **Given** multiple documents have failed, **When** an operator selects multiple failed documents and clicks "Retry All", **Then** all selected documents are queued for retry and their statuses update accordingly

---

### User Story 4 - Document Search and Filtering (Priority: P1)

Operators need to efficiently search and filter the document list using multiple criteria including tags, date ranges, status, filename, and other metadata to quickly locate specific documents or groups of documents.

**Why this priority**: As the number of documents grows, findability becomes critical for operational efficiency. Search and filter capabilities are essential for making the system usable at scale and should be included in the MVP to prevent operational bottlenecks.

**Independent Test**: Can be fully tested by uploading a diverse set of documents with different tags, dates, and statuses, then applying various search and filter combinations and verifying the results match the expected criteria.

**Acceptance Scenarios**:

1. **Given** the document list contains hundreds of documents, **When** an operator enters "invoice" in the search box, **Then** the list filters to show only documents whose filename or tags contain "invoice"
2. **Given** an operator wants to find all failed documents from last week, **When** they apply filters: Status="Failed" AND Date Range="Last 7 Days", **Then** only documents matching both criteria are displayed
3. **Given** documents have various tags, **When** an operator selects multiple tags "Invoice" and "Urgent" using a tag filter, **Then** documents with either tag are displayed (OR logic), unless "Match All" is selected (AND logic)
4. **Given** an operator has applied multiple filters, **When** they click "Clear Filters", **Then** all filters are removed and the full document list is displayed
5. **Given** search and filter results are displayed, **When** an operator sorts by column (filename, upload date, size, status), **Then** the results are re-ordered accordingly (ascending/descending toggle)

---

### User Story 5 - Classification Rule Management (Priority: P2)

Operators need to create, modify, and manage classification rules based on filename patterns (regex), MIME types, file size, and simple text content snippets. Rules automatically apply tags and determine routing destinations when documents match the criteria.

**Why this priority**: Rule-based classification is the core intelligence of the system. While manual classification could work temporarily, automated rules are essential for scalability and operational efficiency. This is prioritized after the basic upload/status/retry workflow to allow incremental value delivery.

**Independent Test**: Can be fully tested by creating a new rule (e.g., "if filename contains 'invoice' then tag as 'Invoice' and route to 'Accounting Queue'"), uploading a matching document, and verifying the rule applies the correct tag and routing. Rule management can be tested by editing, disabling, and re-enabling rules and verifying behavior changes accordingly.

**Acceptance Scenarios**:

1. **Given** an operator is creating a new classification rule, **When** they define a filename regex pattern "invoice.*\\.pdf", select MIME type "application/pdf", add a text snippet "Total Amount", assign tag "Invoice", and set routing to "Accounting Queue", **Then** the rule is saved successfully and appears in the rule list
2. **Given** a classification rule exists for invoices, **When** an operator uploads a PDF named "invoice_2023_001.pdf" containing text "Total Amount", **Then** the document is automatically classified with the "Invoice" tag and routed to "Accounting Queue"
3. **Given** an operator is creating a rule, **When** they specify file size range "Min: 100KB, Max: 10MB", **Then** the rule only matches documents within that size range
4. **Given** multiple classification rules exist, **When** a document matches multiple rules, **Then** all matching rules are applied (multiple tags can be assigned), and the routing destination is determined by the highest priority rule
5. **Given** an operator is editing an existing rule, **When** they change the filename pattern from "invoice" to "bill", **Then** the rule is updated, and subsequently uploaded documents are classified according to the new pattern

---

### User Story 6 - Rule Dry-Run Testing (Priority: P2)

Operators need to test classification rules against existing documents using a dry-run mode before activating them. This allows verification of rule logic and expected outcomes without affecting actual document classification.

**Why this priority**: Rule creation can be error-prone. Dry-run testing prevents incorrect classifications and allows operators to validate rules safely. This is important for operational confidence but can be added after basic rule creation is working.

**Independent Test**: Can be fully tested by creating a new rule, selecting an existing document, running dry-run mode, and verifying the system displays which tags would be applied and where the document would be routed without actually modifying the document's classification.

**Acceptance Scenarios**:

1. **Given** an operator has created a new classification rule (not yet active), **When** they select an existing document and click "Test Rule", **Then** the system displays a preview showing: "Would apply tag: Invoice", "Would route to: Accounting Queue", "Rule matched on: filename pattern 'invoice'"
2. **Given** an operator is testing a rule against a document, **When** the rule does not match the document, **Then** the system displays "No match" with an explanation of which conditions failed (e.g., "Filename pattern did not match", "Required text snippet not found")
3. **Given** an operator is testing a rule that matches multiple conditions, **When** they run the dry-run, **Then** the system shows which specific conditions were matched (e.g., "✓ Filename matches", "✓ MIME type matches", "✗ Text snippet not found", "Overall: Partial match")
4. **Given** an operator has tested a rule successfully, **When** they activate the rule, **Then** the rule becomes active and applies to all new documents going forward
5. **Given** multiple rules exist, **When** an operator tests a document against all rules using "Test All Rules", **Then** the system shows which rules would match and in what priority order they would be applied

---

### User Story 7 - Rule Priority Management (Priority: P2)

Operators need to adjust the execution order of classification rules to control which rules are evaluated first and determine routing when multiple rules match a document.

**Why this priority**: Rule priority prevents conflicts and ensures deterministic classification behavior. This is important for rule management but is secondary to basic rule creation and testing.

**Independent Test**: Can be fully tested by creating multiple overlapping rules with different priorities, uploading a document that matches all rules, and verifying that routing is determined by the highest priority rule while all tags are applied.

**Acceptance Scenarios**:

1. **Given** three classification rules exist with priorities: Rule A (Priority 1), Rule B (Priority 2), Rule C (Priority 3), **When** an operator views the rule list, **Then** rules are displayed in priority order from highest (1) to lowest (3)
2. **Given** an operator wants to change rule priority, **When** they drag Rule C to position 1 or use up/down arrows, **Then** Rule C becomes Priority 1, and other rules shift down (Rule C→1, Rule A→2, Rule B→3)
3. **Given** a document matches three rules with different routing destinations (Rule A→Queue 1, Rule B→Queue 2, Rule C→Queue 3), **When** classification executes, **Then** all three rules' tags are applied, but routing follows the highest priority rule's destination
4. **Given** an operator has reordered rules, **When** they save the new priority order, **Then** the system displays a confirmation and subsequent document classifications use the new priority order
5. **Given** rules have been reordered, **When** an operator uploads a document matching multiple rules, **Then** the classification result reflects the current priority order and the document detail view shows which rule determined the routing

---

### User Story 8 - Rule Enable/Disable (Priority: P3)

Operators need to disable classification rules temporarily without deleting them, preserving their configuration for later re-activation. This allows temporary rule adjustments without losing rule definitions.

**Why this priority**: Rule enable/disable provides operational flexibility but is not essential for core functionality. It's a quality-of-life improvement that can be added after the main classification workflow is stable.

**Independent Test**: Can be fully tested by disabling an active rule, uploading a document that would match the rule, verifying the rule does not apply, then re-enabling the rule and confirming it applies to new documents.

**Acceptance Scenarios**:

1. **Given** an active classification rule exists, **When** an operator clicks the "Disable" toggle/button, **Then** the rule status changes to "Disabled" and is visually indicated (greyed out or with a disabled badge)
2. **Given** a rule is disabled, **When** a new document is uploaded that would match the rule, **Then** the rule does not apply (no tag added, no routing change)
3. **Given** a disabled rule exists, **When** an operator clicks "Enable", **Then** the rule status changes to "Active" and applies to all newly uploaded or re-classified documents
4. **Given** multiple rules are selected, **When** an operator clicks "Bulk Disable", **Then** all selected rules are disabled simultaneously
5. **Given** a rule was disabled 30 days ago, **When** an operator views the rule list, **Then** the rule shows its disabled date and who disabled it for audit purposes

---

### User Story 9 - Routing Queue Management (Priority: P2)

Operators need to create and configure routing destinations (queues) that can be either folder-based or webhook-based. After classification, documents are automatically dispatched to their assigned queue.

**Why this priority**: Routing is essential for completing the classification workflow and delivering documents to their intended destinations. This is prioritized after classification rules because rules determine routing, making this a natural next step.

**Independent Test**: Can be fully tested by creating a folder-based queue (pointing to a specific folder path), creating a webhook-based queue (with a target URL), assigning these queues to classification rules, and verifying that classified documents are routed to the correct destination (file appears in folder or webhook receives notification).

**Acceptance Scenarios**:

1. **Given** an operator is creating a new routing queue, **When** they select "Folder" type and specify a folder path "/shared/accounting/inbox", **Then** the queue is created and available for assignment to classification rules
2. **Given** an operator is creating a webhook queue, **When** they select "Webhook" type, enter URL "https://erp.company.com/api/documents", and configure authentication headers, **Then** the queue is created and webhook configuration is validated
3. **Given** a document has been classified with a rule that routes to a folder queue, **When** routing executes, **Then** the document file is copied/moved to the specified folder and the document status updates to "Routed"
4. **Given** a document is routed to a webhook queue, **When** routing executes, **Then** a POST request is sent to the webhook URL with document metadata (ID, filename, tags, inbox, classification timestamp) in JSON format
5. **Given** an operator is managing queues, **When** they edit a queue's configuration (change folder path or webhook URL), **Then** the system validates the new configuration and updates the queue, affecting all subsequent routing operations

---

### User Story 10 - Manual Tag Adjustment (Priority: P3)

Operators need to manually add or remove tags on documents that have already been classified. This allows correction of automatic classification errors or addition of supplementary tags based on human review.

**Why this priority**: Manual tag adjustment provides flexibility and error correction but is not required for the core automated workflow. This is a refinement feature that enhances usability after the main system is operational.

**Independent Test**: Can be fully tested by classifying a document automatically (which applies certain tags), then manually adding an additional tag or removing an existing tag, and verifying the document's tag list updates correctly and the audit trail records the manual change.

**Acceptance Scenarios**:

1. **Given** a document has been automatically classified with tags "Invoice" and "Accounting", **When** an operator opens the document details and adds a new tag "Urgent", **Then** the document now has three tags: "Invoice", "Accounting", "Urgent"
2. **Given** a document has tags "Invoice" and "Contract" (incorrect), **When** an operator removes the "Contract" tag, **Then** the document retains only "Invoice" and the audit trail shows "[Operator Name] removed tag 'Contract' at [timestamp]"
3. **Given** an operator is viewing a document, **When** they add a tag that already exists on the document, **Then** the system prevents duplicate tags and displays a message "Tag already applied"
4. **Given** a document has been manually tagged, **When** the document is re-classified (via retry), **Then** the system preserves manually added tags unless they conflict with new automatic classification, and notifies the operator of any changes
5. **Given** an operator manually adds or removes tags, **When** the change is saved, **Then** the document's routing destination is recalculated based on the new tag set if applicable

---

### User Story 11 - Webhook Delivery Monitoring (Priority: P3)

Operators need to view webhook delivery records showing successful and failed webhook calls, including the ability to manually retry failed webhook deliveries to handle external system outages or temporary failures.

**Why this priority**: Webhook monitoring is important for operational visibility but is secondary to establishing the basic routing mechanism. This is a monitoring and troubleshooting feature that adds value after the main workflow is established.

**Independent Test**: Can be fully tested by configuring a webhook queue, routing a document to it, simulating a webhook failure (using an unreachable URL), verifying the failure is recorded with error details, then manually retrying the webhook and confirming successful delivery.

**Acceptance Scenarios**:

1. **Given** documents have been routed to webhook queues, **When** an operator views the "Webhook Delivery Log", **Then** a list is displayed showing: document name, webhook URL, timestamp, status (Success/Failed), HTTP response code, and response time
2. **Given** a webhook delivery failed (e.g., HTTP 500 error), **When** an operator views the delivery details, **Then** the full error message and response body are displayed for troubleshooting
3. **Given** a webhook delivery failed, **When** an operator clicks "Retry", **Then** the webhook is called again with the same payload, and the new attempt is logged as a separate entry
4. **Given** multiple webhook deliveries failed due to external system downtime, **When** an operator selects multiple failed deliveries and clicks "Retry All", **Then** all selected webhooks are re-sent and results are logged
5. **Given** a webhook was successfully delivered, **When** an operator views the delivery log entry, **Then** details include the request payload (JSON), response body, response headers, and delivery duration

---

### User Story 12 - Classification Decision Audit (Priority: P3)

Operators need to view the classification rule matching history for individual documents to understand which rules were evaluated, which matched, and how the final classification decision was made. This provides transparency and aids troubleshooting.

**Why this priority**: Classification transparency is valuable for understanding and debugging but is not essential for the core workflow. This is an advanced diagnostic feature that can be added after the system is stable.

**Independent Test**: Can be fully tested by creating multiple classification rules, uploading a document, triggering classification, then viewing the document's "Rule Matching History" and verifying it shows all evaluated rules, match results, confidence scores (if applicable), and the final decision rationale.

**Acceptance Scenarios**:

1. **Given** a document has been classified, **When** an operator views the "Classification History" section in document details, **Then** a timeline is displayed showing: classification timestamp, rules evaluated (by priority order), match results for each rule (matched/not matched with reason), and final tags and routing applied
2. **Given** a document matched multiple rules, **When** an operator reviews the classification history, **Then** each matched rule is listed with details: "Rule Name: Invoice Detection", "Matched on: filename regex", "Tag applied: Invoice", "Priority: 1"
3. **Given** a document did not match any rules, **When** an operator views the classification history, **Then** the system displays "No rules matched" with a summary of evaluated rules and why each didn't match (e.g., "Rule A: filename pattern failed", "Rule B: MIME type mismatch")
4. **Given** a document was manually re-classified, **When** an operator views the classification history, **Then** both the original automatic classification and the manual re-classification are shown in chronological order with actor information
5. **Given** classification rules were modified, **When** an operator views an old document's classification history, **Then** the historical rule definitions at the time of classification are preserved and displayed accurately

---

### Edge Cases

- **What happens when a document upload fails mid-transfer?**
  - System logs partial upload, displays error to operator, allows retry, cleans up incomplete file from storage

- **What happens when a document matches no classification rules?**
  - Document remains in "Classified" status with no tags applied, stays in the inbox until manual intervention or a default "Unclassified" tag is applied

- **What happens when a webhook URL is unreachable or returns an error?**
  - System retries webhook delivery with exponential backoff (configured retry policy), logs failure details, allows manual retry, and alerts operator after max retries

- **What happens when two rules with the same priority match a document?**
  - System applies tags from both rules but uses the first matching rule (by creation order) to determine routing, logs ambiguity warning in classification history

- **What happens when an operator uploads a document larger than 50MB?**
  - System rejects upload before processing, displays clear error message with size limit, suggests document compression or splitting

- **What happens when a PDF is corrupted or malformed?**
  - Classification attempts to extract text but fails gracefully, document marked as "Failed" with specific error, operator can retry or manually classify

- **What happens when an operator deletes a classification rule that is referenced in existing document classification history?**
  - Historical classification records preserve the rule definition snapshot, rule marked as "Deleted" but history remains intact for audit purposes

- **What happens when multiple operators attempt to modify the same document or rule simultaneously?**
  - System uses optimistic locking, last write wins with conflict detection, losing operator receives notification of conflict and can review/retry

- **What happens when storage quota is reached?**
  - System rejects new uploads, displays quota warning to operators, administrators receive alert, documents can be archived or storage expanded

- **What happens when an operator manually changes tags but the document is later re-classified?**
  - System preserves manual tags unless explicitly overridden, displays warning before re-classification that manual changes may be affected, audit trail records both manual and automatic changes

## Requirements *(mandatory)*

### Functional Requirements

#### Document Upload

- **FR-001**: System MUST allow operators to upload PDF files (application/pdf MIME type) to designated inboxes
- **FR-002**: System MUST allow operators to upload image files (PNG, JPG, JPEG, TIFF formats) to designated inboxes
- **FR-003**: System MUST support single file upload with progress indication
- **FR-004**: System MUST support batch upload of multiple files (up to 20 files per batch) with aggregate progress indication
- **FR-005**: System MUST validate file size limit (50MB maximum per file, configurable per tenant)
- **FR-006**: System MUST validate file format against allowed types and reject unsupported formats with specific error messages
- **FR-007**: System MUST sanitize filenames to prevent path traversal and special character issues
- **FR-008**: System MUST generate unique document identifiers for each uploaded file
- **FR-009**: System MUST record upload metadata: filename, file size, MIME type, upload timestamp, uploader identity, inbox designation
- **FR-010**: System MUST allow operators to cancel in-progress uploads

#### Document Status Management

- **FR-011**: System MUST maintain document status with four possible states: Pending, Classified, Routed, Failed
- **FR-012**: System MUST display document status prominently in the document list with visual indicators (icons/colors)
- **FR-013**: System MUST allow filtering documents by status (single or multiple statuses)
- **FR-014**: System MUST show status history for each document with timestamps and state transitions
- **FR-015**: System MUST display error details for documents in "Failed" status including error type and message

#### Classification Rule Management

- **FR-016**: System MUST allow operators to create classification rules with multiple matching criteria: filename regex pattern, MIME type, file size range, text content snippets
- **FR-017**: System MUST allow operators to assign one or more tags to a rule for automatic application on match
- **FR-018**: System MUST allow operators to assign a routing destination (queue) to each rule
- **FR-019**: System MUST support priority ordering of rules with numeric priority values
- **FR-020**: System MUST evaluate rules in priority order when classifying documents
- **FR-021**: System MUST allow operators to enable/disable rules without deleting them
- **FR-022**: System MUST preserve rule configuration when disabled
- **FR-023**: System MUST validate regex patterns for syntax errors before saving rules
- **FR-024**: System MUST support testing rules in dry-run mode against existing documents without modifying document state
- **FR-025**: System MUST display dry-run results showing which conditions matched/failed and what actions would be taken
- **FR-026**: System MUST allow operators to edit existing rules with validation
- **FR-027**: System MUST allow operators to delete rules with confirmation prompt
- **FR-028**: System MUST apply all matching rules' tags to a document (multiple rules can match)
- **FR-029**: System MUST use highest-priority matching rule to determine routing destination

#### Document Classification

- **FR-030**: System MUST automatically classify uploaded documents in "Pending" status by evaluating all active rules
- **FR-031**: System MUST extract text content from PDFs and images (OCR) for text-based rule matching
- **FR-032**: System MUST apply tags from all matching rules to the document
- **FR-033**: System MUST determine routing destination based on highest-priority matching rule
- **FR-034**: System MUST update document status to "Classified" upon successful classification
- **FR-035**: System MUST update document status to "Failed" if classification encounters an error
- **FR-036**: System MUST record classification timestamp and rule matching details for audit purposes
- **FR-037**: System MUST allow operators to manually retry classification for "Failed" documents
- **FR-038**: System MUST allow operators to manually re-classify documents regardless of current status
- **FR-039**: System MUST preserve classification history when documents are re-classified

#### Routing Queue Management

- **FR-040**: System MUST allow operators to create routing queues with two types: Folder and Webhook
- **FR-041**: System MUST validate folder paths for folder-type queues
- **FR-042**: System MUST validate webhook URLs and authentication configuration for webhook-type queues
- **FR-043**: System MUST allow operators to configure webhook authentication (API key, bearer token, basic auth)
- **FR-044**: System MUST route classified documents to their assigned queue asynchronously
- **FR-045**: System MUST copy/move document files to folder paths for folder-type queues
- **FR-046**: System MUST send HTTP POST requests with document metadata (JSON payload) to webhook URLs for webhook-type queues
- **FR-047**: System MUST update document status to "Routed" upon successful routing
- **FR-048**: System MUST retry failed routing operations with exponential backoff (configurable retry policy)
- **FR-049**: System MUST log webhook delivery attempts with timestamp, status code, response body, and error details
- **FR-050**: System MUST allow operators to manually retry failed webhook deliveries
- **FR-051**: System MUST allow operators to view webhook delivery history for each document

#### Manual Tag Management

- **FR-052**: System MUST allow operators to manually add tags to documents after classification
- **FR-053**: System MUST allow operators to manually remove tags from documents
- **FR-054**: System MUST prevent duplicate tags on a single document
- **FR-055**: System MUST record manual tag changes in audit trail with operator identity and timestamp
- **FR-056**: System MUST allow operators to view the source of each tag (automatic rule vs. manual addition)

#### Search and Filtering

- **FR-057**: System MUST provide full-text search across document filenames
- **FR-058**: System MUST allow filtering documents by one or more tags (OR logic)
- **FR-059**: System MUST allow filtering documents by status (Pending, Classified, Routed, Failed)
- **FR-060**: System MUST allow filtering documents by date range (upload date)
- **FR-061**: System MUST allow filtering documents by file size range
- **FR-062**: System MUST allow filtering documents by inbox designation
- **FR-063**: System MUST allow combining multiple filters (AND logic between different filter types)
- **FR-064**: System MUST allow sorting results by filename, upload date, file size, status
- **FR-065**: System MUST support pagination for document list (default 20 items per page, configurable up to 100)
- **FR-066**: System MUST display filter and search criteria clearly and allow easy clearing

#### Audit and History

- **FR-067**: System MUST record all document state transitions with timestamp, previous state, new state, and actor (system or operator)
- **FR-068**: System MUST record classification rule matching history for each document including rules evaluated, match results, and applied tags
- **FR-069**: System MUST preserve historical rule definitions even after rules are modified or deleted
- **FR-070**: System MUST record all manual tag additions/removals with operator identity and timestamp
- **FR-071**: System MUST allow operators to view complete document history including uploads, classifications, routing, and manual changes
- **FR-072**: System MUST retain audit logs according to compliance requirements (7-year retention per DocFlow constitution)

### Key Entities

- **Document**: Represents an uploaded file with metadata (filename, size, MIME type, upload timestamp, uploader, inbox, status, tags, routing destination, classification history)
- **Inbox**: A logical container for uploaded documents, represents different intake points (e.g., "Accounting Inbox", "Legal Inbox", "HR Inbox")
- **Classification Rule**: Defines matching criteria (filename regex, MIME type, file size range, text snippets) and actions (tags to apply, routing queue) with priority ordering and enabled/disabled state
- **Tag**: A label applied to documents for categorization (e.g., "Invoice", "Contract", "Urgent"), can be applied by rules or manually
- **Routing Queue**: A destination for classified documents, can be folder-based (file system path) or webhook-based (HTTP endpoint URL with authentication)
- **Webhook Delivery Record**: Tracks webhook call attempts with timestamp, status, HTTP response, error details, and retry count
- **Classification History Entry**: Records rule evaluation results for a document including rules matched, conditions evaluated, tags applied, and routing destination determined
- **Audit Log Entry**: Immutable record of document state changes, manual interventions, and system actions with timestamp and actor identity

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Operators can upload a single document to an inbox and see it appear in the document list within 5 seconds
- **SC-002**: Operators can upload up to 20 documents in batch and all complete successfully within 60 seconds (for files averaging 2MB each)
- **SC-003**: 95% of documents are automatically classified successfully on first attempt without manual intervention
- **SC-004**: Operators can locate any document using search/filter functionality within 10 seconds regardless of total document count
- **SC-005**: Failed documents can be identified and retried by operators within 30 seconds of failure
- **SC-006**: Classification rules can be created and tested in dry-run mode within 2 minutes, including validation feedback
- **SC-007**: Webhook delivery success rate exceeds 95% for reachable endpoints with automatic retry handling
- **SC-008**: Operators spend 50% less time on manual document sorting and routing compared to pre-system baseline

### Performance Criteria (Required for DocFlow)

- **SC-009**: API responds in < 2s (p95) for document upload (metadata registration, file written to blob storage separately)
- **SC-010**: System processes at least 100 documents/minute through classification pipeline
- **SC-011**: System handles 50 concurrent document uploads without degradation
- **SC-012**: System successfully processes 50MB PDF files using streaming (no full-file memory loading)
- **SC-013**: Document list loads and displays within 1s (p95) for up to 10,000 documents with pagination
- **SC-014**: Search and filter operations return results within 500ms (p95) across 100,000+ documents using database indexes
- **SC-015**: Rule evaluation completes within 500ms (p95) per document for rule-based classification
- **SC-016**: Classification rule dry-run testing completes within 1s (p95) per document

### Security & Compliance Criteria (Required for DocFlow)

- **SC-017**: All documents are encrypted at rest in blob storage using AES-256 encryption
- **SC-018**: All API communication uses TLS 1.2+ for encryption in transit
- **SC-019**: Operators can only access documents and rules within their assigned tenant (multi-tenancy isolation enforced)
- **SC-020**: All document state changes are logged in immutable audit trail with timestamp, actor, and action details
- **SC-021**: Audit logs are retained for 7 years per compliance requirements
- **SC-022**: System rejects malformed files and invalid inputs with specific error messages (file size exceeded, unsupported format, invalid regex pattern)
- **SC-023**: File names are sanitized to prevent path traversal attacks (../, absolute paths blocked)
- **SC-024**: Classification rule expressions are validated to prevent regex denial-of-service (ReDoS) attacks
- **SC-025**: Webhook authentication credentials are stored encrypted and never exposed in logs or API responses

### Quality Criteria (Required for DocFlow)

- **SC-026**: 80% code coverage for Domain and Application layers; 100% coverage for classification engine and routing logic (critical paths)
- **SC-027**: All document upload, classification, routing, and state transition operations are logged with correlation IDs for end-to-end tracing
- **SC-028**: System provides clear, actionable error messages for all failure scenarios (upload failed, classification failed, routing failed, rule validation failed) with suggested remediation steps
- **SC-029**: Document classification decisions are fully auditable and transparent, showing which rules matched and why routing was chosen
- **SC-030**: System recovers gracefully from transient failures (external service timeouts, blob storage unavailability) with automatic retry and operator notification

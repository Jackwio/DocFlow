# [PROJECT_NAME] Constitution
<!-- Example: Spec Constitution, TaskFlow Constitution, etc. -->

## Core Principles

### [PRINCIPLE_1_NAME]
<!-- Example: I. Library-First -->
[PRINCIPLE_1_DESCRIPTION]
<!-- Example: Every feature starts as a standalone library; Libraries must be self-contained, independently testable, documented; Clear purpose required - no organizational-only libraries -->

### [PRINCIPLE_2_NAME]
<!-- Example: II. CLI Interface -->
[PRINCIPLE_2_DESCRIPTION]
<!-- Example: Every library exposes functionality via CLI; Text in/out protocol: stdin/args → stdout, errors → stderr; Support JSON + human-readable formats -->

### [PRINCIPLE_3_NAME]
<!-- Example: III. Test-First (NON-NEGOTIABLE) -->
[PRINCIPLE_3_DESCRIPTION]
<!-- Example: TDD mandatory: Tests written → User approved → Tests fail → Then implement; Red-Green-Refactor cycle strictly enforced -->

### [PRINCIPLE_4_NAME]
<!-- Example: IV. Integration Testing -->
[PRINCIPLE_4_DESCRIPTION]
<!-- Example: Focus areas requiring integration tests: New library contract tests, Contract changes, Inter-service communication, Shared schemas -->

### [PRINCIPLE_5_NAME]
<!-- Example: V. Observability, VI. Versioning & Breaking Changes, VII. Simplicity -->
[PRINCIPLE_5_DESCRIPTION]
<!-- Example: Text I/O ensures debuggability; Structured logging required; Or: MAJOR.MINOR.BUILD format; Or: Start simple, YAGNI principles -->

## [SECTION_2_NAME]
<!-- Example: Additional Constraints, Security Requirements, Performance Standards, etc. -->

[SECTION_2_CONTENT]
<!-- Example: Technology stack requirements, compliance standards, deployment policies, etc. -->

## [SECTION_3_NAME]
<!-- Example: Development Workflow, Review Process, Quality Gates, etc. -->

[SECTION_3_CONTENT]
<!-- Example: Code review requirements, testing gates, deployment approval process, etc. -->

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

```markdown
# DocFlow Constitution
<!-- Sync Impact Report: See top-of-file HTML comment inserted by update script -->

## Core Principles

### I. Privacy-first & Tenant Isolation
All design and implementation decisions MUST prioritize tenant isolation and data privacy. Tenant context
validation is mandatory for every cross-tenant operation. Personal data access MUST follow the
principle of least privilege and be logged for auditability. Rationale: DocFlow must be safe to run
on-premises or in hybrid environments where strict tenant separation and data locality are required.

### II. Modular Domain-Driven Design (DDD) & Clear Bounded Contexts
The system MUST follow clean architecture and DDD: each ABP module owns a single bounded context,
exposes an explicit API, and minimizes coupling. Aggregates and domain logic MUST enforce invariants
inside the domain layer. Aggregates SHOULD be created via factory methods and avoid base-class
inheritance unless justified by a real business need. Rationale: Maintainability and clear ownership
reduce cross-service coupling and improve testability.

### III. Security-first (NON-NEGOTIABLE)
All external APIs MUST require OIDC authentication and token validation. Every request that touches
tenant-scoped data MUST include a validated tenant context. Secrets, credentials, and keys MUST not
be stored in source control. Security reviews and automated security checks are required in CI.
Rationale: Security is integral — a breach undermines tenant trust and on-prem deployments.

### IV. Observability, Auditability & Operational Simplicity
Core services MUST be observable: structured logs, traces, and metrics are first-class deliverables.
Audit logs for sensitive operations MUST be persisted and tamper-evident. Systems MUST favor simple,
deterministic behavior over clever optimizations that reduce operability. Rationale: Reliable
operations and the ability to investigate incidents are key for on-prem and hybrid deployments.

### V. Deterministic Pipelines, Idempotency & Incremental Delivery
CI/CD pipelines and classification workflows MUST be deterministic: builds and infra provisioning
produce repeatable artifacts. All pipelines MUST be idempotent and retry-safe. Features MUST be
introduced behind feature flags and validated via automated tests (unit, integration, contract)
before merging. Deliver by vertical slices to ensure each iteration is production-ready.
Rationale: Predictability in delivery and runtime behavior reduces risk and supports safe rollouts.

## Additional Constraints & Operational Standards

- Core services MUST be stateless where possible and designed for horizontal scaling in Kubernetes.
- All APIs MUST validate OIDC tokens and tenant context; multi-tenant data access MUST be explicitly
	authorized and logged.
- Audit logs, metrics, and tracing MUST be produced by default for all services touching tenant data.
- Pipelines and classification rules MUST be idempotent, retry-safe, and produce deterministic outputs.
- All feature rollouts MUST use feature flags and be accompanied by automated tests and monitoring.
- Documentation (design, runbooks, quickstart), release notes, and audit trails are required for any
	production-impacting change.

## Development Workflow, Review Process & Quality Gates

- Code reviews are mandatory for all changes; at least one domain expert and one reviewer familiar
	with operational concerns must approve.
- PRs MUST reference relevant spec/plan and include tests demonstrating the change (unit and
	integration/contract where applicable). Tests MUST fail before implementation when adding new
	behaviors (test-first preferred).
- CI gates MUST include: build, lint/typecheck, unit tests, contract/integration tests (where
	applicable), security scans, and basic observability smoke tests (metrics/logs present).
- Releases SHOULD follow semantic versioning for public APIs and Conventional Commits for branch/commit
	naming. See `.github/instructions/conventional-commits.instructions.md` for details.
- Any production-impacting migration MUST include a rollback plan, runbook, and a staged rollout via
	feature flags.

## Governance

Amendments: Changes to this constitution MUST be proposed in a spec, include a migration or
compliance plan, and be approved by the core maintainers. Major changes that alter tenant or
security guarantees are breaking and require a formal review and migration strategy.

Versioning policy:
- MAJOR: Backward-incompatible governance or principle removals/redefinitions.
- MINOR: Addition of a new principle or material expansion of guidance.
- PATCH: Clarifications, wording fixes, or non-semantic refinements.

Compliance reviews: Every release that touches security, tenancy, or data handling MUST include a
compliance checklist and sign-off from the security/ops lead.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date unknown | **Last Amended**: 2025-11-08

<!--
Sync Impact Report (generated):
- Version change: TODO(previous) -> 1.0.0
- Modified principles: Added explicit constitution reflecting privacy-first, DDD, security,
	observability, deterministic pipelines, feature flags, and incremental delivery.
- Added sections: "Additional Constraints & Operational Standards", "Development Workflow, Review Process & Quality Gates"
- Removed sections: none
- Templates requiring updates: .specify/templates/plan-template.md ✅ updated
	.specify/templates/spec-template.md ✅ updated
	.specify/templates/tasks-template.md ✅ updated
	.specify/templates/agent-file-template.md ⚠ pending (manual review recommended)
- Follow-up TODOs: RATIFICATION_DATE must be supplied; confirm any project-specific thresholds for
	observability/retention and attach to runbooks.
-->

```

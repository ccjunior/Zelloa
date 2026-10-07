# Zelloa — Current Development State

**Last Updated:** 2026-10-06  
**Current Phase:** Phase 5 — Payments
**Phase Status:** IN PROGRESS — development-only simulated payment flow implemented; PSP selection and real sandbox integration remain pending
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete and committed as `261d1da`. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. Phase 5 currently provides a development-only simulated payment flow. It does not authorize PSP-specific integration or satisfy the real sandbox requirement; select a PSP before implementing that integration.

Phase 0 established the technical foundation; Phase 1 implemented authentication, authorization, and tenant context. Phase 2 completed school and academic records, guardian links, and invitations.

---

## 2. Completed Work

On 2026-10-06, the owner approved keeping authentication and `TenantContext` in Phase 1. Document 02 section 86 was updated to clarify the Phase 0 boundary.

Phase 0 is complete: .NET solution and layer/test projects; Angular workspace with Family, School, and the four planned shared libraries; PostgreSQL persistence, baseline migration, and health check; local Docker Compose and Dockerfiles; GitHub Actions workflow; local EF tool manifest; README instructions.

Project specification and architecture documentation are defined.

Available normative documentation:

- `01-Zelloa-Product-Specification.md`
- `02-Zelloa-Technical-Architecture.md`
- `03-Zelloa-Development-Plan.md`
- `04-Zelloa-AI-Instructions.md`
- `05-Zelloa-Payment-Integration.md`
- `06-Zelloa-API-Specification.md`

---

## 3. Work In Progress

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Phase 4 is committed as `261d1da` (`feat: implement ordering phase`) and adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. Phase 5 now has a persisted development simulation behind `IPaymentGateway`, with owner-scoped create/query/confirm/expire operations and no real PSP calls. No production payment provider has been integrated.

---

## 4. Pending Work — Current Phase

The development-only payment simulation is implemented behind `IPaymentGateway`. It persists payment and order/tenant ownership; supports simulated Pix charge creation, guardian-only query, and development-only simulated confirmation/expiration; uses `SimulatedConfirmed` without setting `ConfirmedAt` or moving the order to `Paid`; returns conspicuous simulation markers; and rejects startup if simulation is enabled outside `Development`. The endpoints are mapped only when `Payments:SimulationEnabled=true`; the Development settings enable it and base settings disable it. The Phase 5 Gate still requires PSP selection and a real Pix charge in provider homologation/sandbox; webhooks remain in Phase 6.

---

## 5. Database State

**Database:** PostgreSQL 17; Testcontainers and Compose connectivity validated; local Compose volume retained  
**Latest Migration:** `DevelopmentPaymentSimulation`, following `OrderingFoundation`. Migration generated; `dotnet ef migrations has-pending-model-changes` reports no pending model changes. This migration has not yet been applied to a database.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects
**Build:** PASS, Release, zero warnings, after simulated payment implementation.
**Tests:** Not run after the Phase 5 changes. The last Phase 4 run passed 6 Architecture Tests and 18 PostgreSQL integration/security tests; Domain/Application test projects contain no tests.

---

## 7. Frontend State

**Angular Workspace:** Initialized under `web/`; Angular 22.2.1

**Zelloa Family:** Initialized with routing, HTTP client, PWA, and base layout  
**Zelloa School:** Initialized with routing, HTTP client, and base layout

**Lint:** PASS — all 6 projects  
**Tests:** PASS — 8 tests across both apps and 4 libraries  
**Build:** PASS — Family and School production builds

---

## 8. Infrastructure State

**Docker:** Compose images built; all 4 services ran successfully and were stopped after validation  
**PostgreSQL:** Compose health check and baseline migration validated; named volume retained  
**CI:** GitHub Actions workflow created; its restore/build/test and npm ci/lint/test/build commands passed locally. Git is now initialized with remote `origin`; hosted workflow status has not yet been verified.

---

## 9. Known Issues

The hosted GitHub Actions workflow has not yet been verified after Git was configured. Invitation activation UI is outside Phase 2; configure `InvitationUrls__Family` and `InvitationUrls__School` to valid frontend activation routes when those routes are implemented. Invitation URLs are returned once for manual delivery and must not be published or logged.

### Technical Debt

- **TD-001 — Product image management:** catalog currently stores an optional HTTPS `imageUrl` supplied by the school. Zelloa does not upload files or manage image storage/lifecycle. Decide whether to add managed storage, continue using externally hosted URLs, or defer images before the catalog image experience is used in a pilot.
- **TD-002 — Remove development payment simulation:** remove the application-level simulated payment gateway after the real PSP integration has been validated in homologation. Keep any fake gateway used exclusively by automated tests separate; the simulator must never be available in production.

---

## 10. Blockers

No open blocker remains in Phase 4. PSP selection is the next prerequisite for provider-specific implementation; the real PSP sandbox charge remains required for the Phase 5 Gate.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin, and SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; product image uploads/storage remain open as technical debt TD-001, with optional HTTPS URLs supported for now; the development-only payment simulation is implemented, with removal after real PSP homologation tracked by TD-002. Guardians do not select a delivery date, and order cutoffs are configured per class shift by the school.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`. Phase 5 simulation code, migration, and documentation updates are currently uncommitted. The migration was generated but not applied to a database. No tests were run after the Phase 5 implementation.

---

## 13. Next Recommended Task

Evaluate and select the pilot PSP using the criteria in documents 03 and 05, then implement its sandbox integration. The Phase 5 Gate still requires a real Pix charge and payment query in homologation; after that Gate, remove the application simulation tracked by TD-002. Run and resolve the applicable automated tests before considering Phase 5 complete.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`. Phase 3 provides tenant-scoped catalog and records deferred image upload/managed storage as TD-001. Phase 4 reached its Gate with order creation, server-side price calculation, snapshots, unpaid cancellation, repeat revalidation, guardian history, per-shift school-local cutoffs, and the OrderingFoundation migration. The guardian does not choose a delivery date. Phase 5 development simulation is implemented with a `Payments` migration and environment-gated endpoints. Release build passes with zero warnings; EF reports no pending model changes. Automated tests were not run after these changes, and the migration has not been applied to a database. Next: select a PSP and implement/test the real sandbox integration; only then remove the application simulation (TD-002). The simulation never marks an order paid; automated-test fakes remain separate.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

# Zelloa — Current Development State

**Last Updated:** 2026-10-06  
**Current Phase:** Phase 4 — Ordering
**Phase Status:** COMPLETE — Phase 4 Gate reached; awaiting explicit authorization for Phase 5
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. Do not start Phase 5 until explicitly authorized.

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

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Technical debt TD-001 records deferred managed image storage. Phase 4 adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. No real payment integration was added.

---

## 4. Pending Work — Current Phase

Phase 4 is implemented. CreateOrder validates guardian/student link, tenant, active student/classroom, active/available products, quantities, and school-local cutoff; it calculates prices on the server and stores item/class snapshots. Guardians can list, cancel unpaid, and repeat orders; repeat revalidates current price and availability. SchoolAdmins configure cutoff times per class shift. The guardian does not supply a delivery date; the server assigns the institution-local operational date. Missing or passed cutoffs reject order creation. No payment provider or paid-order transitions are implemented in this phase.

---

## 5. Database State

**Database:** PostgreSQL 17; Testcontainers and Compose connectivity validated; local Compose volume retained  
**Latest Migration:** `OrderingFoundation`, following `CatalogFoundation`. EF reports no pending model changes. The integration suite applies migrations against PostgreSQL Testcontainers.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects
**Build:** PASS, Release, zero warnings.
**Tests:** PASS — 6 Architecture Tests and 18 PostgreSQL integration/security tests. Domain/Application test projects contain no tests.

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

---

## 10. Blockers

No open blocker remains in Phase 4. Phase 5 requires explicit authorization and PSP selection before integration work begins.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin, and SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; product image uploads/storage remain open as technical debt TD-001, with optional HTTPS URLs supported for now. Guardians do not select a delivery date, and order cutoffs are configured per class shift by the school.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`. Phase 4 code, migration, tests, and documentation are uncommitted. No Phase 4 commit has been requested.

---

## 13. Next Recommended Task

Review and commit Phase 4 when requested. Do not start Phase 5 until explicitly authorized; Phase 5 begins with selecting a PSP.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`. Phase 3 provides tenant-scoped catalog and records deferred image upload/managed storage as TD-001. Phase 4 has reached its Gate: order creation, server-side price calculation, snapshots, unpaid cancellation, repeat revalidation, guardian history, per-shift school-local cutoffs, and the OrderingFoundation migration are implemented. The guardian does not choose a delivery date. Release build passed with zero warnings, all 6 architecture tests and all 18 PostgreSQL integration/security tests passed, and EF reports no pending model changes. Changes remain uncommitted. Await explicit authorization before Phase 5; its first task is PSP selection.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

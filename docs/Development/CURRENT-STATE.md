# Zelloa — Current Development State

**Last Updated:** 2026-10-07
**Current Phase:** Phase 9 — Zelloa School (early registration scope)
**Phase Status:** IN PROGRESS — owner-approved reordering for SchoolAdmin authentication and registration testing; Phases 5–8 remain incomplete/deferred
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete and committed as `261d1da`. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. The owner has deferred the remaining work in Phase 5 and authorized an early start on the School frontend registration subset in Phase 9. This does not complete or waive Phases 5–8 or the Phase 9 Gate.

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

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Phase 4 is committed as `261d1da` (`feat: implement ordering phase`) and adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. Phase 5 has a persisted development simulation behind `IPaymentGateway`, with owner-scoped create/query/confirm/expire operations and no real PSP calls. By owner decision, defer PSP selection and real payment integration until after the school presentation. No production payment provider has been integrated. Phase 9 has been authorized early for SchoolAdmin login/activation and academic-registration UI so the school can begin testing records. The initial frontend implementation is now in place; live registration against the API/database still needs a manual smoke test after the institution and SchoolAdmin account are provisioned.

---

## 4. Pending Work — Current Phase

Authorized current scope: School frontend for SchoolAdmin login/account activation and registration testing for classrooms, students, guardian invitations, and guardian-student links, using existing tenant-scoped APIs. Implemented in this session: responsive admin shell/navigation, dashboard, login and activation screens, classroom and student forms/lists, guardian invitation with one-time activation URL display and link retry, plus route protection for the SchoolAdmin role. The development-only payment simulation remains available for demonstration. Phases 5–8 are not complete; the full Phase 9 Gate, including catalog, orders, and cafeteria operation, remains pending.

---

## 5. Database State

**Database:** PostgreSQL 17 Compose container is healthy; local Compose volume retained. All seven migrations through `DevelopmentPaymentSimulation` are applied. The local database has 1 `PlatformAdmin`, 4 roles, and no school or `SchoolAdmin` records. A manual role change that left the bootstrap account as a tenantless `SchoolAdmin` was reverted to `PlatformAdmin`; API startup and `/health` were verified afterward.
**Latest Migration:** `DevelopmentPaymentSimulation`, following `OrderingFoundation`. Migration applied to the current local database; `dotnet ef migrations has-pending-model-changes` previously reported no pending model changes.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects
**Build:** PASS, Release, zero warnings, after simulated payment implementation.
**Tests:** Not run after the Phase 5 changes. The last Phase 4 run passed 6 Architecture Tests and 18 PostgreSQL integration/security tests; Domain/Application test projects contain no tests.

---

## 7. Frontend State

**Angular Workspace:** Initialized under `web/`; Angular 22.2.1

**Zelloa Family:** Initialized with routing, HTTP client, PWA, and base layout  
**Zelloa School:** Early Phase 9 registration interface implemented: login/activation, protected admin shell, dashboard, classroom/student registration, and guardian invitation/link workflows. Development server: `npm start -- zelloa-school --port 4201` (matches the configured local invitation URL and API CORS origin). The Angular development environment targets the local .NET HTTP profile at port 5027; Docker Compose maps its API to port 8080.

**School Lint:** PASS — `npm run ng -- lint zelloa-school`
**School Tests:** PASS — 2 tests
**School Build:** PASS — `npm run build:school` (production)

---

## 8. Infrastructure State

**Docker:** Compose images built; all 4 services ran successfully and were stopped after validation  
**PostgreSQL:** Compose health check and baseline migration validated; named volume retained  
**CI:** GitHub Actions workflow created; its restore/build/test and npm ci/lint/test/build commands passed locally. Git is now initialized with remote `origin`; hosted workflow status has not yet been verified.

---

## 9. Known Issues

The hosted GitHub Actions workflow has not yet been verified after Git was configured. A `PlatformAdmin` now exists in the local database, but there is no school or `SchoolAdmin`. Use the PlatformAdmin API endpoints to create a school and invite its first SchoolAdmin; the PlatformAdmin account cannot log in to the School panel. A browser capture showed login `204`, `/api/me` `200`, and logout `204`, confirming the role rejection. The login now reports that rejection without navigating away, and async screen state uses Angular signals so updates render in the zoneless app. Ensure `InvitationUrls__School` points to `/activate-invitation` on the School frontend (port 4201 locally). Guardian invitation URLs are returned once for manual delivery and must not be published or logged. The School UI has not yet been manually smoke-tested against a SchoolAdmin account.

### Technical Debt

- **TD-001 — Product image management:** catalog currently stores an optional HTTPS `imageUrl` supplied by the school. Zelloa does not upload files or manage image storage/lifecycle. Decide whether to add managed storage, continue using externally hosted URLs, or defer images before the catalog image experience is used in a pilot.
- **TD-002 — Remove development payment simulation:** keep the application-level simulated payment gateway for the school presentation. After the school identifies its preferred provider and the real PSP integration is validated in homologation, remove the simulator. Keep any fake gateway used exclusively by automated tests separate; the simulator must never be available outside `Development`.

---

## 10. Blockers

Phase 5 remains deferred until after the school presentation/provider discussion. Phase 9 registration work is an approved early subset; its full Gate is not yet in scope. Cafeteria UI remains dependent on Phase 7 order operations and the preceding payment/webhook flow.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin, and SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; product image uploads/storage remain open as technical debt TD-001, with optional HTTPS URLs supported for now; retain the development-only payment simulation for the school presentation and defer PSP selection/integration until the school identifies its preferred provider; start the Phase 9 SchoolAdmin authentication/registration subset early for school data-entry testing; remove the application simulation after real PSP homologation under TD-002. Guardians do not select a delivery date, and order cutoffs are configured per class shift by the school.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`; the development payment simulation is committed as `8db6093`; presentation and PSP-selection sequencing is documented in `f791486`. Phase 9 frontend/docs changes are being committed separately from pre-existing local configuration changes in `src/Zelloa.Api/Zelloa.Api.csproj`, `src/Zelloa.Api/appsettings.Development.json`, and `web/angular.json`. All seven migrations, including the development payment migration, are applied locally. No tests were run after the Phase 5 backend implementation.

---

## 13. Next Recommended Task

Use the existing local PlatformAdmin credentials to create a school and invite its first SchoolAdmin through the API. Activate the invitation and manually smoke-test SchoolAdmin login, classroom/student registration, guardian invitation, and guardian-student linking against the running API/database. Capture integration/API issues before expanding this early Phase 9 scope. Preserve tenant isolation and antiforgery protections. Defer catalog, orders, and cafeteria screens until their planned dependencies are available; do not mark Phases 5–8 or the Phase 9 Gate complete.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`; the development payment simulation as `8db6093`; presentation and PSP-selection sequence as `f791486`. Phase 3 provides tenant-scoped catalog and records deferred image upload/managed storage as TD-001. Phase 4 reached its Gate with order creation, server-side price calculation, snapshots, unpaid cancellation, repeat revalidation, guardian history, per-shift school-local cutoffs, and the OrderingFoundation migration. Phase 5 development simulation is available only in `Development`; real PSP selection/integration is deferred. The owner authorized an early Phase 9 subset for SchoolAdmin authentication and registration testing. The responsive School UI is implemented and its production build, School lint, and two School unit tests pass. All migrations are applied locally; one PlatformAdmin exists, but there is no school or SchoolAdmin yet. Next, create a school and invite the SchoolAdmin via the API, then run manual API-backed registration smoke tests. This does not mark Phases 5–8 or the Phase 9 Gate complete; defer school catalog, orders, and cafeteria operation until their dependencies are satisfied.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

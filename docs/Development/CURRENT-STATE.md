# Zelloa — Current Development State

**Last Updated:** 2026-10-07
**Current Phase:** Phase 10 — Zelloa Admin (PlatformAdmin)
**Phase Status:** IN PROGRESS — owner-approved start before the Phase 9 Gate to provision institutions and invite SchoolAdmins; Phase 9 remains incomplete
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete and committed as `261d1da`. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. The owner deferred real PSP selection/integration until after the school presentation. The owner authorized an early subset of Phase 9 for the School registration frontend, committed as `01003bd`, and then authorized Phase 10 to create the separate PlatformAdmin portal before the Phase 9 Gate. Phase 10 does not complete or waive the Phase 9 Gate or Phases 5–8.

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

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Phase 4 is committed as `261d1da` (`feat: implement ordering phase`) and adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. Phase 5 has a persisted development simulation behind `IPaymentGateway`; real PSP selection/integration is deferred until after the school presentation. Phase 9's early SchoolAdmin login/activation and registration interface is committed as `01003bd`, but it has not yet been smoke-tested against a provisioned school. The owner approved a separate `zelloa-admin` Angular application as Phase 10 to create that institution and invite its initial SchoolAdmin.

---

## 4. Pending Work — Current Phase

Authorized current scope: Phase 10 — build `zelloa-admin` as a separate Angular application inside `web/`. Use existing authentication and school endpoints so `PlatformAdmin` can sign in, create a school and its tenant, and invite the first `SchoolAdmin`. Display the returned `TenantId` and one-time activation URL securely. Do not add school listing/editing until the API contract supports it. After the Phase 10 Gate, return to the unfinished Phase 9 School smoke test; the full Phase 9 Gate, including catalog, orders, and cafeteria operation, remains pending.

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
**Zelloa Admin:** Authorized as the next phase; Angular application not created yet. Planned local development port: `4202`.

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

The hosted GitHub Actions workflow has not yet been verified after Git was configured. A `PlatformAdmin` exists in the local database, but there is no school or `SchoolAdmin`. The account must use the new Admin portal; it cannot log in to the School panel. A browser capture showed login `204`, `/api/me` `200`, and logout `204`, confirming the School role rejection. The School UI has not yet been manually smoke-tested against a SchoolAdmin account. Invitation URLs are returned once for manual delivery and must not be published or logged.

### Technical Debt

- **TD-001 — Product image management:** catalog currently stores an optional HTTPS `imageUrl` supplied by the school. Zelloa does not upload files or manage image storage/lifecycle. Decide whether to add managed storage, continue using externally hosted URLs, or defer images before the catalog image experience is used in a pilot.
- **TD-002 — Remove development payment simulation:** keep the application-level simulated payment gateway for the school presentation. After the school identifies its preferred provider and the real PSP integration is validated in homologation, remove the simulator. Keep any fake gateway used exclusively by automated tests separate; the simulator must never be available outside `Development`.

---

## 10. Blockers

Phase 5 remains deferred until after the school presentation/provider discussion. Phase 9 registration work is an approved early subset; its full Gate is not yet in scope. Cafeteria UI remains dependent on Phase 7 order operations and the preceding payment/webhook flow.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin, and SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; a separate Angular `zelloa-admin` application provides the PlatformAdmin interface; begin Phase 10 before the full Phase 9 Gate to provision the school required for testing; product image uploads/storage remain open as TD-001; retain the development payment simulation for presentation and defer PSP selection until the school identifies its preferred provider; remove the application simulation after real PSP homologation under TD-002. Guardians do not select a delivery date, and order cutoffs are configured per class shift by the school.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 9's early School frontend and documentation subset is committed as `01003bd`. Existing local configuration changes in `src/Zelloa.Api/Zelloa.Api.csproj`, `src/Zelloa.Api/appsettings.Development.json`, and `web/angular.json` are preserved and uncommitted. The new Phase 10 scope is documented; the `zelloa-admin` project is not yet implemented. All seven migrations, including the development payment migration, are applied locally.

---

## 13. Next Recommended Task

Implement the Phase 10 `zelloa-admin` Angular project, then validate PlatformAdmin login, school/tenant creation, invitation of the initial SchoolAdmin, and account activation. Preserve cookie authentication, antiforgery protections, and existing tenant ownership. After the Phase 10 Gate, return to the Phase 9 manual School registration smoke test; do not mark the Phase 9 Gate or Phases 5–8 complete.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`; the development payment simulation as `8db6093`; presentation and PSP-selection sequence as `f791486`; early Phase 9 School UI as `01003bd`. School workspace tests, lint, and Family/School production builds passed before that commit. A direct database role change was reverted to `PlatformAdmin`; the local API starts and `/health` returns 200. The owner then approved Phase 10 and a dedicated `zelloa-admin` Angular application before the full Phase 9 Gate. Implement Phase 10 first, then resume School API-backed registration testing; Phase 9 and Phases 5–8 are not complete.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

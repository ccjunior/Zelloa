# Zelloa — Current Development State

**Last Updated:** 2026-10-07
**Current Phase:** Phase 10 — Zelloa Admin (PlatformAdmin)
**Phase Status:** IN PROGRESS — `zelloa-admin` is implemented and validated; browser-driven institution creation/SchoolAdmin activation remains pending; Phase 9 remains incomplete
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete and committed as `261d1da`. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. The owner deferred real PSP selection/integration until after the school presentation. The owner authorized an early subset of Phase 9 for the School registration frontend, committed as `01003bd`, and then authorized Phase 10 to create the separate PlatformAdmin portal before the Phase 9 Gate. Phase 10 does not complete or waive the Phase 9 Gate or Phases 5–8.

Phase 0 established the technical foundation; Phase 1 implemented authentication, authorization, and tenant context. Phase 2 completed school and academic records, guardian links, and invitations. The owner approved an early Phase 8 slice for Guardian activation, login/session, and the linked-students home page; the full Family Gate remains open.

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

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Phase 4 is committed as `261d1da` (`feat: implement ordering phase`) and adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. Phase 5 has a persisted development simulation behind `IPaymentGateway`; real PSP selection/integration is deferred until after the school presentation. Phase 9's early SchoolAdmin login/activation and registration interface is committed as `01003bd`, but it has not yet been smoke-tested against a provisioned school. The owner approved a separate `zelloa-admin` Angular application as Phase 10 to create that institution and invite its initial SchoolAdmin. The early Phase 8 slice now provides Guardian activation, login, protected session, and linked-student display; catalog and ordering remain pending.

---

## 4. Pending Work — Current Phase

Authorized current scope: Phase 10 — the separate Angular `zelloa-admin` app provides PlatformAdmin login, institution/tenant creation, initial SchoolAdmin invitations, and invitation renewal by email for pending SchoolAdmin accounts. Do not add school listing/editing. Also authorized as an early Phase 8 slice: Guardian invitation activation, login/session, and a protected home showing linked students. Family production build and lint pass. Browser validation of CORS, School invitation renewal, and the Family activation/login flow remains pending after restarting the API and using an invitation present in its configured database. A browser-driven school creation and invitation activation was not performed, so the Phase 10 Gate remains open. The full Phase 8 Gate (catalog through order history/PWA) and full Phase 9 Gate remain open.

---

## 5. Database State

**Database:** PostgreSQL 17 Compose container is healthy; local Compose volume retained. All seven migrations through `DevelopmentPaymentSimulation` are applied. Current read-only counts show 1 school, 1 `PlatformAdmin`, 4 roles, and no `SchoolAdmin` or pending invitation records. The expired link reported by the owner is not present in this Compose database; verify the API connection if renewal returns 404.
**Latest Migration:** `DevelopmentPaymentSimulation`, following `OrderingFoundation`. Migration applied to the current local database; `dotnet ef migrations has-pending-model-changes` previously reported no pending model changes.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects
**Build:** PASS, Release, zero warnings, after simulated payment implementation. Development CORS and invitation renewal changes build successfully with zero warnings/errors using a temporary output directory because the local API executable is running.
**Tests:** Not run after the Phase 5 changes. The last Phase 4 run passed 6 Architecture Tests and 18 PostgreSQL integration/security tests; Domain/Application test projects contain no tests.

---

## 7. Frontend State

**Angular Workspace:** Initialized under `web/`; Angular 22.2.1

**Zelloa Family:** Early Phase 8 slice implemented: invitation activation with password validation, Guardian login/session, role-protected home, linked-student list, empty/error states, and logout. Local API environment targets port 5027. Production uses same-origin `/api` routing through Nginx. Production build and lint pass; browser validation awaits a valid Guardian invite/account in the API's configured database.
**Zelloa School:** Early Phase 9 registration interface implemented: login/activation, protected admin shell, dashboard, classroom/student registration, and guardian invitation/link workflows. Development server: `npm start -- zelloa-school --port 4201` (matches the configured local invitation URL). The Angular development environment targets the local .NET HTTP profile at port 5027; Docker Compose maps its API to port 8080.
**Zelloa Admin:** Phase 10 application implemented with PlatformAdmin login, protected shell, institution/tenant registration, secure one-time invitation display/copy, renewal of a pending SchoolAdmin invitation by email, and route guard. Start with `npm start -- zelloa-admin --port 4202`; the Development API accepts loopback origins on dynamically selected ports.

**Workspace Lint:** PASS — `npm run lint`
**Workspace Tests:** PASS — `npm test` (10 tests)
**Workspace Build:** PASS — `npm run build` (Family, School, Admin production builds; zero warnings)

---

## 8. Infrastructure State

**Docker:** Compose images built; all 4 services ran successfully and were stopped after validation  
**PostgreSQL:** Compose health check and baseline migration validated; named volume retained  
**CI:** GitHub Actions workflow created; its restore/build/test and npm ci/lint/test/build commands passed locally. Git is now initialized with remote `origin`; hosted workflow status has not yet been verified.

---

## 9. Known Issues

The hosted GitHub Actions workflow has not yet been verified after Git was configured. Admin API login previously returned `204`, `/api/me` returned the `PlatformAdmin` role with null `TenantId`, and the Development API accepts loopback origins on dynamically selected ports. The local School app is currently on port `59603`; `InvitationUrls:School` was aligned to this port for local activation links. The Compose database currently has one school but no pending SchoolAdmin account, so the owner's expired invitation may belong to another database. Renewal and Guardian activation have not yet been exercised in the browser. The Family app currently listens on port `4200`. Invitation URLs are returned once for manual delivery and must not be published or logged.

### Technical Debt

- **TD-001 — Product image management:** catalog currently stores an optional HTTPS `imageUrl` supplied by the school. Zelloa does not upload files or manage image storage/lifecycle. Decide whether to add managed storage, continue using externally hosted URLs, or defer images before the catalog image experience is used in a pilot.
- **TD-002 — Remove development payment simulation:** keep the application-level simulated payment gateway for the school presentation. After the school identifies its preferred provider and the real PSP integration is validated in homologation, remove the simulator. Keep any fake gateway used exclusively by automated tests separate; the simulator must never be available outside `Development`.

---

## 10. Blockers

Phase 5 remains deferred until after the school presentation/provider discussion. Phase 9 registration work is an approved early subset; its full Gate is not yet in scope. Cafeteria UI remains dependent on Phase 7 order operations and the preceding payment/webhook flow.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin and can renew its pending invitation by email, invalidating the prior token and issuing a fresh 24-hour link; SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; a separate Angular `zelloa-admin` application provides the PlatformAdmin interface; begin Phase 10 before the full Phase 9 Gate to provision the school required for testing; early Phase 8 slice limited to Guardian activation, login/session, and linked-student home; product image uploads/storage remain open as TD-001; retain the development payment simulation for presentation and defer PSP selection until the school identifies its preferred provider; remove the application simulation after real PSP homologation under TD-002. Guardians do not select a delivery date, and order cutoffs are configured per class shift by the school.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 9's early School frontend/docs is committed as `01003bd`; Phase 10 scope/architecture as `dff71c0`; the Admin app, local CORS origin, Docker service, and build/test integration as `33d4390`. Pre-existing local changes remain uncommitted and preserved in `src/Zelloa.Api/Zelloa.Api.csproj` (User Secrets ID), `src/Zelloa.Api/appsettings.Development.json` (local connection string and invitation URL), and the analytics setting in `web/angular.json`. Current work adds Development loopback CORS, the SchoolAdmin invitation-renewal API/UI, and Guardian activation/login/home, plus documentation. No migration was needed. Do not commit the local configuration changes listed above.

---

## 13. Next Recommended Task

Restart the local API so the current CORS and invitation-renewal changes load. Ensure the Guardian invitation/account exists in the database configured for the API, open the invite in the Family app on port 4200, activate it, sign in, and confirm the linked-student list. Also verify the Admin renewal flow using an eligible pending SchoolAdmin invitation. Do not mark Phase 10, Phase 9, or Phase 8 Gates complete until their full UI-backed flows succeed.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`; the development payment simulation as `8db6093`; presentation and PSP sequencing as `f791486`; early Phase 9 School UI as `01003bd`; Phase 10 architecture/plan as `dff71c0`; and the `zelloa-admin` portal as `33d4390`. The Admin app provides login, a PlatformAdmin guard, school/tenant creation, one-time administrator invitations, and a pending-invitation renewal page. The Family app now provides the authorized early Guardian access slice. Family production build and lint pass; tests were not run. Restart the local API before using CORS/invitation changes. The Compose database has no pending SchoolAdmin invitation; verify the Guardian invitation in the API's configured database before activation. Phase 10 browser validation, Phase 9 Gate, and the full Phase 8 Gate remain incomplete.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

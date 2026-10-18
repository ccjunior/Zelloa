# Zelloa — Current Development State

**Last Updated:** 2026-10-18
**Current Phase:** Phase 5 — Payments (resumed by owner; initial PSP confirmation pending)
**Phase Status:** IN PROGRESS — the owner authorized resuming Phase 5 because payments gate dependent work. The existing payment implementation consists of `IPaymentGateway`, the `Payment` domain/persistence, and a Development-only simulation; no real PSP adapter is implemented. Banco do Brasil is on standby and is not the active provider choice. The pilot school's bank/provider, eligible business account/Pix key, API access, commercial terms, and provider preference must be confirmed before provider-specific coding. Phase 5's Gate is still open and requires creating a real Pix charge in sandbox/homologation and exposing its QR Code, Pix Copia e Cola, expiry, and financial status. The owner reports local validation of the previously authorized Family and School slices; their full Phase 8 and Phase 9 Gates remain open. A Coolify staging deployment was attempted. The PostgreSQL container started, but Docker marked it unhealthy because the Compose healthcheck passed variable names as extra `pg_isready` arguments. The healthcheck has been corrected locally to check `127.0.0.1:5432`; redeployment and health confirmation are pending. This staging work does not enable payment processing, cafeteria preparation, or a production pilot. Phase 10 invitation renewal has not been browser-validated.
**Last Completed Phase:** Phase 4 — Ordering
**Last Completed Gate:** Gate — Phase 4

---

## 1. Current Objective

Phase 4 — Ordering is complete and committed as `261d1da`. Its Gate is met: a valid order can be created without real payment integration, with financial values calculated and validated by the backend. The owner has now authorized resuming Phase 5; the real PSP decision remains pending the pilot school's provider/account confirmation. Provider-specific implementation must wait for that prerequisite, as required by the Development Plan and Payment Integration specification. The owner authorized an early subset of Phase 9 for the School registration frontend, committed as `01003bd`, and then authorized Phase 10 to create the separate PlatformAdmin portal before the Phase 9 Gate. Phase 10 does not complete or waive the Phase 9 Gate or Phases 5–8.

Phase 0 established the technical foundation; Phase 1 implemented authentication, authorization, and tenant context. Phase 2 completed school and academic records, guardian links, and invitations. The owner confirmed that the institution and SchoolAdmin account are registered and active, and that the early Phase 8 Guardian activation/login/access flow works. The owner authorized the School catalog management slice and then the Family catalog view. The full Phase 9 and Family Gates remain open.

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

Phase 3 is committed as `d3fc61b` (`feat: add tenant-scoped catalog management`). Phase 1 is committed as `b6e3f45`; Phase 2 as `7421fa0` (`feat: implement school and academic phase`). Phase 4 is committed as `261d1da` (`feat: implement ordering phase`) and adds server-priced orders, item snapshots, unpaid-order cancellation, repeat-order revalidation, guardian order history, and SchoolAdmin-configured cutoffs by class shift. Phase 5 has a Development-only simulation behind `IPaymentGateway`; on 2026-10-11, the owner authorized resuming Phase 5 because payment is a dependency for later phases. Banco do Brasil is on standby; confirm the pilot school's preferred bank/provider and account eligibility before provider-specific implementation. Phase 9's early SchoolAdmin login/activation and registration interface is committed as `01003bd`. The separate `zelloa-admin` portal was created in Phase 10 to provision the institution; the owner confirms institution registration and SchoolAdmin activation are complete. Commit `a8808d7` adds the Guardian access slice and SchoolAdmin invitation renewal. The School catalog now separates Products and Categories; create/edit forms replace their lists while open. The Family mobile catalog view is implemented and browser-validated by the owner. The authorized Family order slice is implemented through server-created `AwaitingPayment`, and the Family recent-orders page lists its state and details; the owner reports local validation of order creation, order consultation, the School recent-orders screen, the opening/cutoff behavior, and loading/saving the SchoolAdmin settings. These application and deployment changes were committed in `cdf385a` and are present on `origin/main`; the documentation handoff was committed in `57362cd`.

---

## 4. Pending Work — Current Phase

The owner reports completing local validation of the authorized Family and School scope: cutoff behavior before opening, within the window, and after cutoff; Family order submission and recent-order consultation; SchoolAdmin order configuration load/save; and the SchoolAdmin recent-orders page. Those slices stop at backend-created `AwaitingPayment`; the lists are read-only and do not release orders for cafeteria preparation. The owner is now authorizing Phase 5 work. Its first prerequisite is to confirm the pilot school's bank/provider and account eligibility; no real payment integration should be coded until that is confirmed. Staging deployment has been attempted; the Compose PostgreSQL healthcheck fix is local and still needs redeployment and health confirmation. Mobile-device validation follows staging validation. Staging does not enable Pix, payment confirmation, cafeteria preparation, or a production pilot. The full Phase 8 and Phase 9 Gates remain open. The institution and active SchoolAdmin account are confirmed by the owner; do not recreate them. Phase 10 invitation renewal remains implemented but lacks browser validation against a pending invitation.

---

## 5. Database State

**Database:** PostgreSQL 17 Compose container was previously verified healthy locally; all eight migrations through `OrderCutoffOpeningTime` are applied in the local database. In the attempted Coolify staging deploy, PostgreSQL initialized and accepted connections, but its healthcheck failed because `CMD-SHELL` passed the literal `ZELLOA_POSTGRES_USER` and `ZELLOA_POSTGRES_DB` placeholders as extra arguments. The root Compose healthcheck now uses a direct `CMD` readiness check against `127.0.0.1:5432`; the correction has not yet been pushed or redeployed. After health is confirmed, apply migrations through the documented SSH tunnel only after verifying the target and backup. A prior local snapshot showed one school; the owner has since confirmed the institution and SchoolAdmin account are active.
**Latest Migration:** `OrderCutoffOpeningTime` adds nullable `OpeningTime` to `OrderCutoffs` and is applied to the local development database. The Coolify database does not exist yet; apply migrations through the documented SSH tunnel after verifying the target and backup.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects
**Build:** PASS, Release, zero warnings, including `OrderCutoffOpeningTime`, opening-window validation, and the SchoolAdmin orders endpoint.
**Tests:** Not run for the current SchoolAdmin order-list change. The last Phase 4 run passed 6 Architecture Tests and 18 PostgreSQL integration/security tests; Domain/Application test projects contain no tests.

**Payments:** `Payment`, persistence, `IPaymentGateway`, and Development simulation exist. `PaymentOperations` and the payment routes currently expose simulated charge creation/status only; there is no real provider adapter or provider credential configuration. Phase 5 implementation has been authorized to resume, pending selection/confirmation of the pilot school's provider.

---

## 7. Frontend State

**Angular Workspace:** Initialized under `web/`; Angular 22.2.1

**Zelloa Family:** Early Phase 8 slices include invitation activation, Guardian login/session, protected linked-student home, and the mobile catalog view backed by `/api/catalog/available`. The owner confirmed the catalog is browser-validated with real catalog data. The order slice adds an in-memory cart, linked-student selection, server-priced order submission, and an `AwaitingPayment` confirmation. The `/orders` page loads recent orders from `/api/me/orders`, resolves student names from `/api/me/students`, and displays item snapshots, total, dates, class/shift, and Portuguese status labels; awaiting-payment orders are described as not yet payable or available for preparation. The page is read-only. The owner reports local validation of order creation and recent-order consultation. Payment creation and school preparation remain outside the slice. Local API environment targets port 5027. Production uses same-origin `/api` routing through Nginx. The full Phase 8 Gate remains open.
**Zelloa School:** Early Phase 9 registration interface includes login/activation, protected admin shell, dashboard, classroom/student registration, guardian invitation/link workflows, category/product management, SchoolAdmin order windows per shift, and a read-only recent-orders page. `GET /api/school/orders` returns up to 100 latest orders for the authenticated tenant, including student name and item snapshots; the endpoint requires `CanManageSchool` and uses the global tenant filters. Pending orders are highlighted and explicitly not released for preparation. The page has local search and status filters but no payment/status actions. The owner reports local validation of SchoolAdmin settings load/save and the recent-orders page. The local database migration is recorded as applied. School production build and targeted lint pass according to the previous handoff; no new build was run for this status update. Development server: `npm start -- zelloa-school --port 4201`; the local invitation URL in the user's configuration may use a different port. Angular development targets the local .NET HTTP profile at port 5027; Docker Compose maps its API to port 8080.
**Zelloa Admin:** Phase 10 application implemented with PlatformAdmin login, protected shell, institution/tenant registration, secure one-time invitation display/copy, renewal of a pending SchoolAdmin invitation by email, and route guard. Start with `npm start -- zelloa-admin --port 4202`; the Development API accepts loopback origins on dynamically selected ports.

**Workspace Lint:** Targeted School lint passes with the catalog, order-cutoff, and recent-orders pages. Targeted Family lint passes with its catalog, order creation, and recent-orders page; the last full-workspace lint passed before these changes.
**Workspace Tests:** The previous full run passed `npm test` (10 tests); tests were not run for the current catalog and order-flow changes.
**Workspace Build:** Targeted School production build passes with the catalog, order-cutoff, and recent-orders pages. Targeted Family production build passes with its catalog, order creation, and recent-orders page with the Family component-style warning budget set to 5 kB; the full build last passed before the current Family order changes.

---

## 8. Infrastructure State

**Docker:** Local Compose images were previously built and all 4 services ran successfully. The base Compose definition is now intended for Coolify: API/frontend ports are private to the Compose network, PostgreSQL binds to server loopback for SSH-tunneled migrations, and local published ports live in `docker-compose.override.yml`. Coolify staging is not deployed or validated yet.
**PostgreSQL:** Compose health check and baseline migration validated; named volume retained  
**CI:** GitHub Actions workflow created; its restore/build/test and npm ci/lint/test/build commands passed locally. Git is now initialized with remote `origin`; hosted workflow status has not yet been verified.

---

## 9. Known Issues

The hosted GitHub Actions workflow has not yet been verified after Git was configured. Admin API login previously returned `204`, `/api/me` returned the `PlatformAdmin` role with null `TenantId`, and the Development API accepts loopback origins on dynamically selected ports. The local School app was configured at port `59603`; the current user's configuration may differ. The owner confirms the institution and SchoolAdmin account are active and the Family Guardian access flow works. The pending-invitation renewal screen still needs a pending SchoolAdmin invitation for browser validation. The Family app currently listens on port `4200`. Invitation URLs are returned once for manual delivery and must not be published or logged.

### Technical Debt

- **TD-001 — Product image management:** catalog currently stores an optional HTTPS `imageUrl` supplied by the school. Zelloa does not upload files or manage image storage/lifecycle. Decide whether to add managed storage, continue using externally hosted URLs, or defer images before the catalog image experience is used in a pilot.
- **TD-002 — Remove development payment simulation:** keep the application-level simulated payment gateway for the school presentation. After the school identifies its preferred provider and the real PSP integration is validated in homologation, remove the simulator. Keep any fake gateway used exclusively by automated tests separate; the simulator must never be available outside `Development`.

---

## 10. Blockers

Phase 5 is active by owner authorization, but the pilot school's bank/provider, account eligibility, API access, costs, and direct-receipt arrangement are not confirmed. The existing Development simulation does not satisfy Phase 5's Gate. Provider-specific coding must wait for the school/provider decision. The owner reports local validation complete for Family order submission/consultation, the School recent-orders page, SchoolAdmin order-window configuration, and before/inside/after cutoff behavior. The School cafeteria queue remains outside the current slice. The owner is preparing the staging environment; deployment, server migrations, runtime checks, and subsequent mobile validation must wait for the owner's readiness confirmation. Phase 9's full Gate remains open. Phase 10 invitation renewal awaits an eligible pending account for the final browser validation pass.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin and can renew its pending invitation by email, invalidating the prior token and issuing a fresh 24-hour link; SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours; a separate Angular `zelloa-admin` application provides the PlatformAdmin interface; begin Phase 10 before the full Phase 9 Gate to provision the school required for testing; early Phase 8 slices include Guardian access, mobile catalog viewing, order creation through `AwaitingPayment`, and a read-only Family recent-orders list; SchoolAdmin has a separate read-only recent-orders view that temporarily includes `AwaitingPayment` only for functional validation; neither list creates payment or makes awaiting-payment orders eligible for cafeteria preparation; school catalog administration supports the Family catalog; product image uploads/storage remain open as TD-001; retain the development payment simulation for presentation and defer PSP selection until the school identifies its preferred provider; remove the application simulation after real PSP homologation under TD-002. Guardians do not select a delivery date. SchoolAdmin may set an optional local opening time and a required same-day cutoff per class shift; orders outside the window are rejected in the institution timezone and remain assigned to the current operational date.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Repository Status

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. The application/deployment changes are recorded in `cdf385a`, and the documentation handoff updates are in the subsequent Git history. Check live Git status at the start of the next session rather than relying on a remembered ahead/behind count. The staging runbook uses environment-driven PostgreSQL credentials, private Compose networking, a loopback-only database port for SSH-tunneled migrations, and local-only port mappings in `docker-compose.override.yml`. Do not commit `.env`, real credentials, invitation tokens, or bootstrap passwords. The owner is preparing the staging environment; wait for explicit readiness before deployment.

---

## 13. Next Recommended Task

The owner authorized resuming Phase 5 because it gates dependent work. First confirm which bank/provider the pilot school uses and whether it has the business account, active Pix key, API access, and commercial terms needed for direct receipts. Banco do Brasil is on standby and should not be treated as the selected or preferred provider. Then verify the school's provider's current official charge, QR/payload, expiry, status-query, sandbox, authentication, idempotency, webhook capability, fees, and contract requirements. Once the provider is confirmed, implement only the Phase 5 sandbox charge flow through the existing `IPaymentGateway` abstraction; leave webhook processing to Phase 6 and do not mark orders Paid from simulation. Separately, the owner is preparing the Coolify environment. Do not deploy until the owner confirms readiness. Do not treat staging as the Phase 13 pilot.

---

## 14. Session Handoff

Phase 1 was completed and committed as `b6e3f45`; Phase 2 as `7421fa0`; Phase 3 as `d3fc61b`; Phase 4 as `261d1da`; the development payment simulation as `8db6093`; presentation and PSP sequencing as `f791486`; early Phase 9 School UI as `01003bd`; Phase 10 architecture/plan as `dff71c0`; the original Admin portal as `33d4390`; CORS, SchoolAdmin invitation renewal, and Family Guardian access as `a8808d7`; and the current catalog, Family order, order-window, migration, and staging Compose changes as `cdf385a`. The handoff updates document the owner's local validation and the current staging-readiness boundary. On 2026-10-11, the owner authorized resuming Phase 5; provider selection remains pending confirmation with the pilot school, Banco do Brasil is on standby, and no real PSP adapter is implemented. The owner confirms institution/SchoolAdmin activation and the Family access flow. The owner reports local validation complete for cutoff behavior, Family order creation and consultation, SchoolAdmin order settings, and the School recent-orders view. Payment creation and School preparation remain pending. The Coolify staging deployment was attempted; redeploy the local PostgreSQL healthcheck correction and confirm database health before applying migrations through the documented SSH tunnel. Mobile checks follow staging validation. Phase 8 and 9 Gates remain open; Phase 10 invitation renewal awaits an eligible pending account.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

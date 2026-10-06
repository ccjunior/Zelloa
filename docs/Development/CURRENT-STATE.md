# Zelloa — Current Development State

**Last Updated:** 2026-10-06  
**Current Phase:** Phase 2 — School and Academic
**Phase Status:** COMPLETE — Phase 2 Gate met; awaiting owner checkpoint before any Phase 3 work
**Last Completed Phase:** Phase 2 — School and Academic
**Last Completed Gate:** Gate — Phase 2

---

## 1. Current Objective

Phase 2 — School and Academic is authorized. Implement only its scope and Gate requirements.

Phase 0 established the technical foundation; Phase 1 implemented authentication, authorization, and tenant context. Phase 2 is complete and its checkpoint is pending presentation; Phase 3 has not been authorized.

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

No Phase 2 implementation work remains. Phase 1 is committed as `b6e3f45`. Phase 2 changes are currently uncommitted.

---

## 4. Pending Work — Current Phase

The Phase 2 Gate is complete: schools, classrooms, students, guardian profiles and links are implemented; tenant ownership and guardian access checks are covered; PlatformAdmin invites the first SchoolAdmin; SchoolAdmin invites guardians; activation links are single-use and valid for 24 hours. Invitation delivery is manual. No Phase 3 work is authorized until the Phase 2 checkpoint is presented and the owner explicitly approves the next phase.

---

## 5. Database State

**Database:** PostgreSQL 17; Testcontainers and Compose connectivity validated; local Compose volume retained  
**Latest Migration:** `SchoolAcademicAndInvitations`, following `AllowMultipleAccountsPerTenant`. EF reports no pending model changes. All migrations were applied successfully by PostgreSQL Testcontainers integration tests.

---

## 6. Backend State

**Solution:** `Zelloa.sln` initialized with four source projects and four test projects  
**Build:** PASS, Release, zero warnings  
**Tests:** PASS — 6 Architecture Tests and 14 PostgreSQL integration/security tests. Domain/Application test projects contain no tests.

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

---

## 10. Blockers

None currently identified. Docker is installed and its server is reachable.

---

## 11. Decisions Made During Development

Owner-approved decisions: ASP.NET Core Identity with cookie sessions; one tenant per institutional account, with multiple accounts per tenant; school owns student registration and initiates guardian access; guardian context selection across schools is future scope; PlatformAdmin invites the initial SchoolAdmin, and SchoolAdmin invites Guardians via manually delivered, single-use activation links expiring after 24 hours.

Architectural and product decisions already defined in the normative documentation must not be duplicated here.

Only record decisions made during implementation that are relevant for future development sessions.

---

## 12. Uncommitted / Interrupted Work

Git is initialized on `main` tracking `origin/main`; remote `origin` is configured. Phase 1 is committed as `b6e3f45`. Phase 2 changes are uncommitted.

---

## 13. Next Recommended Task

Present the Phase 2 checkpoint to the owner and wait for explicit authorization before starting Phase 3 — Menu and Products.

---

## 14. Session Handoff

Phase 1 was authorized and completed on 2026-10-06, committed as `b6e3f45`. ASP.NET Core Identity cookie sessions, CSRF, role policies, `/api/me`, tenant context and query filtering, PlatformAdmin bootstrap, and one-to-many account-to-tenant association are implemented. Phase 2 was authorized on 2026-10-06. The owner confirmed school-managed student registration, guardian invitations, and future cross-school context selection. The owner approved manual delivery of single-use activation links, valid for 24 hours, for the initial SchoolAdmin and Guardians; no external mail provider will be added. Normative documents 01, 02, 03, and 06 record this flow. Phase 2 adds school/classroom/student management, guardian accounts and links, invitation activation, and tenant/guardian access checks. The consolidated `SchoolAcademicAndInvitations` migration was applied by integration tests; EF reports no model changes. On 2026-10-06 the Release build passed with zero warnings, 6 Architecture Tests passed, and 14 Integration Tests passed. README and this handoff were updated. The Phase 2 changes remain uncommitted. Do not begin Phase 3 until the owner approves after the checkpoint.

When development begins, update this document whenever meaningful progress is made and before ending the session.

The next coding agent must not rely solely on this document.

Always verify the actual repository state, Git status, build status, tests, and relevant implementation before continuing.

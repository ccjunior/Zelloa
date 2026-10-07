# Zelloa

Zelloa is a multi-institution school services platform. The current work includes the School registration frontend and the **Phase 10 — Zelloa Admin** portal for platform administration. Each institutional account belongs to one school; a school may have multiple accounts.

## Local requirements

- .NET 10 SDK
- Node.js 24 and npm
- Docker Engine with Docker Compose

## Run the local environment

From the project root:

```powershell
docker compose build api
docker compose build family
docker compose build school
docker compose build admin
docker compose up -d postgres
```

After PostgreSQL starts, apply migrations before starting the API. Set bootstrap credentials through a secret manager before starting the API if this is the first environment setup.

Services:

- API health: <http://localhost:8080/health>
- API OpenAPI: <http://localhost:8080/openapi/v1.json>
- Zelloa Family: <http://localhost:8081>
- Zelloa School: <http://localhost:8082>
- Zelloa Admin: <http://localhost:8083>
- PostgreSQL: `localhost:5432`, database `zelloa`, user `zelloa`

Apply the database migrations after PostgreSQL starts:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Zelloa.Infrastructure/Zelloa.Infrastructure.csproj --startup-project src/Zelloa.Api/Zelloa.Api.csproj
docker compose up -d api family school admin
```

## Authentication

The API uses ASP.NET Core Identity with an authenticated cookie session. Before a login request, call `GET /api/auth/csrf` and send its `requestToken` in the `X-XSRF-TOKEN` header to `POST /api/auth/login`. The login body contains `email` and `password`. After login, call `GET /api/me` to read the authenticated user, roles, and tenant. Call `GET /api/auth/csrf` again before `POST /api/auth/logout`.

There is no public account-registration endpoint. The first `PlatformAdmin` can be bootstrapped once by setting `IdentityBootstrap__PlatformAdmin__Email` and `IdentityBootstrap__PlatformAdmin__Password` through a secret manager after applying migrations. Do not put these values in `appsettings.json` or commit them. Remove the bootstrap secret settings after the account is created. `PlatformAdmin` is global; each institutional account belongs to one tenant, and each tenant may have multiple accounts.

`PlatformAdmin` creates a school and invites its initial `SchoolAdmin`. A `SchoolAdmin` creates guardian accounts and links guardians to students. Invitations are single-use activation links valid for 24 hours; delivery is manual in this phase. Configure `InvitationUrls__School` and `InvitationUrls__Family` to the corresponding frontend activation routes. Do not publish invitation links or store them in logs. Public self-registration and automated email delivery are not available.

In the Development environment, the API accepts credentialed requests from HTTP/HTTPS loopback origins (`localhost` and loopback IPs) on any port, so Angular's dynamically selected development port works. In other environments, only explicitly configured origins are allowed through `Cors__AllowedOrigins__0` and additional indexed values as needed.

When the School development server uses a non-default port, set `InvitationUrls__School` to that port's `/activate-invitation` URL so copied activation links open the running app.

Local database credentials are development-only values defined in `docker-compose.yml` and `appsettings.json`. Replace them through environment-specific secret configuration before any shared or production deployment.

Stop services with `Ctrl+C`. Remove the local database volume only when its data is no longer needed:

```powershell
docker compose down --volumes
```

## Build and test the backend

```powershell
dotnet restore Zelloa.sln
dotnet build Zelloa.sln --no-restore
dotnet test Zelloa.sln --no-build
```

Integration tests use Testcontainers and require Docker.
On Windows with Docker Desktop's `desktop-linux` context, set its named pipe for Testcontainers before running the suite:

```powershell
$env:DOCKER_HOST = 'npipe://./pipe/dockerDesktopLinuxEngine'
dotnet test Zelloa.sln --no-build
```

## Build and check the frontend

From `web/`:

```powershell
npm ci
npm run lint
npm test
npm run build
```

The applications can also be built separately:

```powershell
npx ng build zelloa-family --configuration production
npx ng build zelloa-school --configuration production
npx ng build zelloa-admin --configuration production
```
# Zelloa

# Zelloa

Zelloa is a multi-institution school services platform. The current implementation includes the School catalog, the Family flow through backend-created `AwaitingPayment` orders, and the PlatformAdmin portal. The complete Phase 8 and Phase 9 Gates remain open.

## Requirements

- .NET 10 SDK
- Node.js 24 and npm
- Docker Engine with Docker Compose
- SSH access to the Coolify server for staging database migrations

## Local environment

Create a local environment file from the example. It contains development-only values; never copy it to Coolify or commit `.env`.

```powershell
Copy-Item .env.example .env
```

The default `docker compose` command automatically loads `docker-compose.override.yml`, which publishes local development ports. The base `docker-compose.yml` is the Coolify deployment definition and does not publish the API or frontend ports on the server.

```powershell
docker compose build api family school admin
docker compose up -d postgres
```

Apply migrations from the host after PostgreSQL starts:

```powershell
dotnet tool restore
$env:ConnectionStrings__ZelloaDatabase = 'Host=127.0.0.1;Port=15432;Database=zelloa;Username=zelloa;Password=zelloa-local'
dotnet tool run dotnet-ef database update --project src/Zelloa.Infrastructure/Zelloa.Infrastructure.csproj --startup-project src/Zelloa.Api/Zelloa.Api.csproj
Remove-Item Env:ConnectionStrings__ZelloaDatabase
```

Then start the applications:

```powershell
docker compose up -d api family school admin
```

Local URLs:

- API health: <http://localhost:8080/health>
- API OpenAPI: <http://localhost:8080/openapi/v1.json>
- Zelloa Family: <http://localhost:8081>
- Zelloa School: <http://localhost:8082>
- Zelloa Admin: <http://localhost:8083>
- PostgreSQL: `localhost:15432`, database/user `zelloa`

Stop the services with `docker compose down`. This preserves the database volume. Remove the local database volume only when its data is no longer needed:

```powershell
docker compose down --volumes
```

## Deploy a staging environment with Coolify

This is an early **staging deployment for technical and mobile validation**. It does not complete the Phase 13 Pilot Deployment Gate and is not ready for real payments or a school-wide launch. Pix generation, payment confirmation, and cafeteria preparation are outside the currently authorized Family slice.

### 1. Prepare DNS and the repository

Use three hostnames that resolve to the Coolify server, for example:

- `https://app.example.com` — Family
- `https://school.example.com` — School
- `https://admin.example.com` — Admin

Replace these examples with your own domains. Do not add a public domain for the API or PostgreSQL: the frontend Nginx containers proxy `/api/` to the private `api:8080` Compose service.

Coolify deploys from Git. Push the intended code and deployment files to the branch you will select. Before pushing, review the changed files and exclude local development configuration, database credentials, `.env`, invitation tokens, and bootstrap passwords.

### 2. Create the Coolify Compose application

In Coolify, create a project/environment for staging, then create an **Application** from the Git repository. Select the `Docker Compose` build pack, the repository branch, and `docker-compose.yml` at the repository root. Do not select `docker-compose.override.yml`; it is only for local development. Keep Coolify's normal Compose deployment mode so it can create proxy routes for the configured domains. See the [Coolify Docker Compose guide](https://coolify.io/docs/applications/builds/docker-compose).

The stack builds these services from the repository:

- `postgres` — PostgreSQL 17 with the named `zelloa-postgres-data` volume.
- `api` — ASP.NET Core on internal port `8080`.
- `family`, `school`, `admin` — Nginx on internal port `80`, each proxying `/api/` to the API on the private Compose network.

The PostgreSQL port `15432` is bound to the server's loopback address for the one-time migration tunnel. It is not a public database endpoint. Do not add a Coolify domain to `postgres` or `api`.

### 3. Configure Coolify environment variables

Add these values in the resource's Environment Variables page. Use unique staging credentials. Generate `ZELLOA_POSTGRES_PASSWORD` as a long alphanumeric value because it is embedded in the PostgreSQL connection string. Do not commit these values.

| Variable | Example value |
| --- | --- |
| `ZELLOA_POSTGRES_DB` | `zelloa` |
| `ZELLOA_POSTGRES_USER` | a private database username |
| `ZELLOA_POSTGRES_PASSWORD` | a generated secret with letters and numbers |
| `ZELLOA_FAMILY_URL` | `https://app.example.com` |
| `ZELLOA_SCHOOL_URL` | `https://school.example.com` |
| `ZELLOA_ADMIN_URL` | `https://admin.example.com` |

Use the exact HTTPS origins with no trailing path. The Family and School URLs are also used to build invitation activation links. Leave `ZELLOA_BOOTSTRAP_ADMIN_EMAIL` and `ZELLOA_BOOTSTRAP_ADMIN_PASSWORD` empty for the first deployment; set them only after database migrations have completed.

### 4. Add the three Coolify domains

Under the resource's Domains settings, add each route and select its receiving Compose service and internal port:

| Public URL | Compose service | Internal port |
| --- | --- | --- |
| `https://app.example.com` | `family` | `80` |
| `https://school.example.com` | `school` | `80` |
| `https://admin.example.com` | `admin` | `80` |

Coolify's proxy handles HTTPS. The Compose file does not publish the API or frontend ports directly on the server. See [Coolify domain configuration](https://coolify.io/docs/core/networking/domains) for DNS and TLS details.

### 5. Deploy, then apply database migrations

Select **Deploy** and review the deployment logs. Once the `postgres` service is running, open an SSH tunnel from your Windows workstation to the server. Keep this PowerShell command running in its own window:

```powershell
ssh -N -L 15432:127.0.0.1:15432 <SSH_USER>@<SERVER_IP>
```

In another PowerShell window at the repository root, restore the local EF tool and run the migrations through that tunnel. Replace the database name, username, and password with the values stored in Coolify; do not save them in the repository.

```powershell
dotnet tool restore
$env:ConnectionStrings__ZelloaDatabase = 'Host=127.0.0.1;Port=15432;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>'
dotnet tool run dotnet-ef database update --project src/Zelloa.Infrastructure/Zelloa.Infrastructure.csproj --startup-project src/Zelloa.Api/Zelloa.Api.csproj
Remove-Item Env:ConnectionStrings__ZelloaDatabase
```

After the command succeeds, close the SSH tunnel. This project does not apply EF migrations automatically when the API starts.

### 6. Create the first PlatformAdmin

In Coolify, temporarily set `ZELLOA_BOOTSTRAP_ADMIN_EMAIL` and `ZELLOA_BOOTSTRAP_ADMIN_PASSWORD`, then redeploy the resource. Sign in at the Admin URL and confirm the PlatformAdmin account works. Remove both bootstrap variables from Coolify and redeploy so the initial credential is not retained as runtime configuration.

### 7. Verify the staging deployment

- Open `https://app.example.com`, `https://school.example.com`, and `https://admin.example.com` over HTTPS.
- Open `https://app.example.com/health` to confirm the API and its database connection are healthy through the Family proxy.
- Sign in to the Admin portal and verify `/api/me` reports the PlatformAdmin role.
- Check Coolify deployment and runtime logs for startup or database errors.
- Confirm the PostgreSQL service has its persistent volume and configure a separate database backup before storing data you need to retain. A persistent volume is not a backup.

Keep this environment restricted to staging data and users. The later mobile browser validation should happen after these domain, session, API, and database checks pass.

## Authentication and invitations

The API uses ASP.NET Core Identity with an authenticated cookie session. Before a login request, call `GET /api/auth/csrf` and send its `requestToken` in the `X-XSRF-TOKEN` header to `POST /api/auth/login`. The login body contains `email` and `password`. After login, call `GET /api/me` to read the authenticated user, roles, and tenant. Call `GET /api/auth/csrf` again before `POST /api/auth/logout`.

There is no public account-registration endpoint. For local development, set `ZELLOA_BOOTSTRAP_ADMIN_EMAIL` and `ZELLOA_BOOTSTRAP_ADMIN_PASSWORD` in the ignored `.env` before starting the API. For Coolify, use the one-time variables above. Remove the bootstrap values after the account is created. `PlatformAdmin` is global; each institutional account belongs to one tenant, and each tenant may have multiple accounts.

`PlatformAdmin` creates a school and invites its initial `SchoolAdmin`. A `SchoolAdmin` creates guardian accounts and links guardians to students. Invitations are single-use activation links valid for 24 hours; delivery is manual in this phase. The API builds links using `InvitationUrls__School` and `InvitationUrls__Family`. Do not publish invitation links or store them in logs. Public self-registration and automated email delivery are not available.

## Build and test the backend

```powershell
dotnet restore Zelloa.sln
dotnet build Zelloa.sln --no-restore
dotnet test Zelloa.sln --no-build
```

Integration tests use Testcontainers and require Docker. On Windows with Docker Desktop's `desktop-linux` context, set its named pipe before running the suite:

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

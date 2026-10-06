# Zelloa

Zelloa is a multi-institution school services platform. The current implementation phase is **Phase 0 — Foundation**. Business features, authentication, and tenant context are not implemented in this phase.

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
docker compose up -d
```

Services:

- API health: <http://localhost:8080/health>
- API OpenAPI: <http://localhost:8080/openapi/v1.json>
- Zelloa Family: <http://localhost:8081>
- Zelloa School: <http://localhost:8082>
- PostgreSQL: `localhost:5432`, database `zelloa`, user `zelloa`

Apply the empty foundation migration once after PostgreSQL starts:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Zelloa.Infrastructure/Zelloa.Infrastructure.csproj --startup-project src/Zelloa.Api/Zelloa.Api.csproj
```

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

The two applications can also be built separately:

```powershell
npx ng build zelloa-family --configuration production
npx ng build zelloa-school --configuration production
```
# Zelloa

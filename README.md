# E-Ticket

The repository contains the target e-ticket platform and the original Android
prototype retained as a workflow reference.

Повний україномовний опис поточного стану, структури, класів і roadmap:
[PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md).

The accepted architecture is documented in
[ADR-0001](docs/adr/0001-target-client-platform.md):

- Angular 21 production frontend;
- .NET 10 authoritative backend;
- Microsoft SQL Server 2019 in the target environment;
- Windows 11 web interface for landscape tablet and stationary workstations;
- Android/Kotlin application retained as a prototype, not a production client.

## Repository layout

```text
frontend/                       Angular 21 web client
backend/
├── src/ETicket.Domain/      Domain models and state machine
├── src/ETicket.Application/ Application ports and result contracts
├── src/ETicket.Infrastructure/ Persistence, audit, and system services
├── src/ETicket.Integrations/ Mock and future external adapters
├── src/ETicket.Api/         .NET 10 HTTP composition root
└── tests/                   Backend tests
app/                            Android prototype
docs/                           ADRs, architecture, and audit baselines
```

See [Target architecture](docs/architecture/target-architecture.md) for the
dependency rules and Mock-to-API boundaries.

## Prerequisites

Target platform development:

- Node.js 24 and npm 11;
- .NET SDK 10.0.400 or a compatible .NET 10 feature band.

Android prototype maintenance additionally requires:

- JDK 21;
- Android SDK Platform 36;
- Android SDK Build Tools 36.0.0 or a compatible AGP-selected version;
- `local.properties` with a machine-local `sdk.dir`.

Do not commit machine-specific SDK paths.

## Frontend checks

```sh
cd frontend
npm ci
npm run check
```

The combined check runs Prettier validation, Angular ESLint, Vitest, and the
production Angular build.

Start the local Angular development server with:

```sh
npm start
```

## Backend checks

```sh
cd backend
dotnet restore ETicket.slnx --locked-mode
dotnet build ETicket.slnx --no-restore
dotnet test ETicket.slnx --no-build
```

NuGet dependency versions are centrally managed and checked through committed
lock files. Build warnings are treated as errors and NuGet audits all dependency
levels.

## Android prototype checks

```sh
./gradlew build --stacktrace
```

Instrumented tests still require a running emulator or connected device.

## Configuration and adapters

Both target applications currently default to mock adapters:

- Angular: `DEFAULT_APP_CONFIG.adapterMode = 'mock'`;
- .NET: `ETicket:AdapterMode = Mock`.

Mock data is contained in infrastructure adapters, never in UI components. The
.NET API deliberately fails startup if a non-Mock adapter mode is selected before
real adapters are implemented.

## Security rules

- Never commit signing keys, credentials, tokens, private keys, connection
  strings, or local secret configuration.
- UI permission checks are not an authorization boundary.
- The .NET backend is authoritative for permissions, visibility zones, state
  transitions, idempotency, concurrency, and audit.
- The Android Room database and backup policy remain prototype-only and are not
  approved for operational data.

## Current phase

Phase 4 adds manual vehicle intake, conditional service-vehicle validation,
e-Queue lookup behind an adapter, explicit allow/deny decisions, initial visit
states, audit, and per-user idempotency. Checkpoint, gate, mode, direction, and
permission remain server-owned. Real КЕП/MFA, SQL Server, camera/VMS, photo
storage, and production external integrations remain deferred.

Verification evidence is recorded in
[Phase 4 quality baseline](docs/audit/phase-4-quality-baseline.md). See
[Vehicle entry](docs/architecture/vehicle-entry.md) for the business rules and
HTTP contracts.

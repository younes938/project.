# Target architecture

## Decision

The production system uses Angular 21, .NET 10, and SQL Server 2019. The existing
Android application remains a separately buildable prototype and is not a source
of production state.

## Dependency direction

```text
Angular feature
    -> application port
    -> API or mock adapter

ETicket.Api
    -> ETicket.Application
    -> ETicket.Infrastructure
    -> ETicket.Integrations

ETicket.Infrastructure -> ETicket.Application -> ETicket.Domain
ETicket.Integrations   -> ETicket.Application -> ETicket.Domain
```

`ETicket.Domain` has no project or package dependency on infrastructure,
framework UI, HTTP, SQL Server, or external integrations.

## Authoritative boundaries

Backend responsibilities:

- legal vehicle state transitions;
- server-side validation;
- RBAC and checkpoint visibility;
- idempotency and optimistic concurrency;
- structured audit events;
- external integration orchestration;
- authoritative timestamps and persistence.

Frontend responsibilities:

- input and presentation state;
- loading, success, empty, and error states;
- permission-aware presentation as a usability layer;
- mapping internal API DTOs to feature view models;
- prevention of accidental repeated UI actions.

Frontend guards never replace backend authorization or transition validation.

## Domain foundation

Phase 1 defines the following shared vocabulary in the backend domain:

- Vehicle, Trailer, Checkpoint, Gate, GateMode;
- VehicleVisit and VehicleVisitStatus;
- ControlResult, Inspection, QueueEntry, CameraRecognition;
- User, Role, Permission, VisibilityZone;
- StatisticsFilter and AuditEvent.

The initial `VehicleStateTransitionService` implements only the conservative
transitions approved during the audit. Entry-denied, passed, returned, and
cancelled states are terminal. Decision changes and rechecks remain deferred
until their business rules are clarified.

## Ports and adapters

Application ports isolate:

- vehicle visit persistence;
- clock/time acquisition;
- audit output;
- AuthAdapter;
- VmsAdapter;
- EQueueAdapter;
- CustomsAdapter;
- BorderControlAdapter.

Phase 1 supplies empty mocks and an in-memory repository. Phase 2 replaces the
empty auth mock with a fixed Development-only identity. Phase 4 adds a
deterministic e-Queue lookup while VMS, customs, and border-control mocks remain
empty. Real SQL Server and external API implementations must map external DTOs
into internal domain models:

```text
External DTO -> integration mapper -> domain/application model -> API DTO
```

## Configuration

Adapter selection is explicit and fail-fast. Mock mode is the only enabled .NET
mode through Phase 4. Mock sign-in is independently disabled in the base
configuration and mapped only in Development. No production URL, credential,
token, private key, or connection string is stored in the repository.

## Authentication and authorization

Phase 2 adds a server-issued cookie session, CSRF protection, permission
policies, checkpoint visibility checks, route guards, a UI permission directive,
and automatic idle logout. UI checks improve navigation but never replace API
authorization. The detailed trust model and deferred КЕП/MFA decisions are in
[Authentication and authorization](authentication-and-authorization.md).

## Operating context

Phase 3 adds an authenticated checkpoint/gate selection before entering working
features. `Gate` remains the single owner of `GateMode`; saved selections are
revalidated against active catalog records and the user's current visibility
zones. Details are documented in
[Operating context](operating-context.md).

## Vehicle entry

Phase 4 adds a manual vehicle-intake use case behind the `VehicleVisitGateway`.
The API derives authority-bearing context data from the authenticated user's
persisted selection, validates service-vehicle conditions, snapshots the
e-Queue response, and atomically applies an idempotency key before auditing the
new visit. Details are documented in [Vehicle entry](vehicle-entry.md).

## Error handling

Angular routes uncaught errors to an application error reporter with a safe user
message. The .NET API uses centralized exception handling and RFC Problem Details;
responses contain a trace ID but not exception details.

## Concurrency

The in-memory repository exposes expected-version updates and atomic per-user
idempotency. Production persistence must enforce row versions and idempotency
with SQL Server constraints; UI button disabling is not a concurrency boundary.

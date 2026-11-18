# ADR-0001: Target client platform

- Status: Accepted
- Date: 2026-08-28
- Accepted: 2026-08-28
- Owners: Product owner and solution architect

## Context

The repository currently contains a native Android application implemented with
Kotlin and Jetpack Compose. It stores operational records locally through Room
and has no backend or HTTP integration layer.

The e-ticket specification defines a different target:

- Angular 21 frontend;
- .NET 10 backend;
- Microsoft SQL Server 2019;
- web interface on Windows 11;
- tablet and stationary workstations, primarily in landscape orientation.

Android Compose components cannot be reused directly by Angular, and Room is not
a substitute for the authoritative .NET/SQL Server backend. Continuing to add
business features to the Android prototype before resolving this mismatch would
increase migration cost and create two competing implementations.

## Decision

Treat the specification stack as authoritative:

1. The production client will be an Angular 21 web application.
2. Business rules, authorization, visibility filtering, audit, idempotency, and
   integration orchestration will be enforced by a .NET 10 backend.
3. SQL Server 2019 will be the authoritative operational datastore.
4. The current Android application will remain buildable as a read-only design
   and workflow reference until the relevant behavior has been accepted in the
   web application.
5. The Android prototype will not receive new production business modules.
6. Moving the current project under `prototype-android/` and creating
   `frontend/` and `backend/` will be done only in a separately approved change.

## Alternatives considered

### Continue with Android as the only client

This reuses current Compose code, but does not satisfy the required web interface
or ordinary Windows 11 deployment model.

### Maintain Android and Angular clients as equal products

This could support dedicated Android tablets, but doubles UI implementation,
testing, release, and security work. It should be selected only if native Android
is an explicit operational requirement.

### Attempt an incremental Compose-to-Angular conversion

There is no direct component migration path. Only requirements, domain concepts,
test scenarios, and selected visual behavior are reusable.

## Consequences

Positive:

- compliance with the stated target environment;
- one authoritative backend for all clients and integrations;
- server-side RBAC, visibility zones, audit, and concurrency control;
- web deployment to both tablet and stationary workstations.

Negative:

- most current UI code will remain prototype-only;
- initial delivery requires new frontend and backend workspaces;
- UX behavior must be revalidated in a browser at 1280x800 through 1920x1200.

## Approval record

The product owner accepted the recommended target on 2026-08-28:

- [x] Accept the proposed Angular/.NET target and retain Android as a prototype.
- [ ] Keep Android as a separately supported client in addition to Angular.
- [ ] Replace the specification target with native Android.

Phase 1 architecture work may proceed using the accepted Angular/.NET target.

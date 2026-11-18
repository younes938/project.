# Operating context

## Phase 3 scope

After authentication, a user selects an available checkpoint and an active gate
context. The gate owns the `GateMode`; the browser cannot submit a separate mode
value that conflicts with the selected gate.

The four names currently retained from the requirements are:

- `RearExit` — «ВШ Тил Виїзд»;
- `BorderExit` — «ВШ Кордон Виїзд»;
- `BorderEntry` — «ВШ Кордон В’їзд»;
- `RearEntry` — «ВШ Тил В’їзд».

Their operational semantics are not expanded into vehicle workflow rules in this
phase. Direction and control-transition ambiguities remain product decisions for
the relevant business phases.

## Authoritative validation

```text
Angular selection
    -> OperatingContextGateway
    -> PUT /api/operating-context/current
    -> current backend User
    -> active Checkpoint in user's VisibilityZone
    -> active Gate belonging to that Checkpoint
    -> server-derived GateMode
    -> persistence port + structured audit event
```

Every read revalidates a stored context against the current user, checkpoint,
gate, and visibility zone. An invalid or stale context is discarded. An unknown
or inaccessible selection returns 404 to avoid exposing another checkpoint;
mixing a visible checkpoint with a gate from another checkpoint returns 400.

Repeating the same `PUT` is idempotent: it returns the existing context without
changing its timestamp or creating a duplicate audit event. State-changing
requests use the Phase 2 antiforgery protection.

Operating context is not an authorization grant. Future business endpoints must
still enforce their own permission, visibility-zone, state-transition, and
entity checks on the backend.

## Persistence boundary

`IOperatingContextRepository` and `ICheckpointCatalog` isolate storage from the
use case. Phase 3 uses process-local implementations because SQL Server remains a
later phase.

The Angular mock gateway stores only the demo user's checkpoint ID, gate ID, and
selection timestamp in `localStorage` so a browser refresh restores the choice.
It validates those identifiers against its internal mock catalog before use.
This local value is a prototype convenience and is never accepted by the API as
proof of visibility or permission.

## API

| Endpoint | Purpose |
|---|---|
| `GET /api/operating-context/options` | visible active checkpoints and gates |
| `GET /api/operating-context/current` | last valid selection for the user |
| `PUT /api/operating-context/current` | select or change checkpoint/gate |

All endpoints require an authenticated session. The options list is filtered on
the server before serialization.

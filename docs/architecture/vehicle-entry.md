# Vehicle entry

## Phase 4 scope

Phase 4 implements manual vehicle intake onto checkpoint territory. It includes
the Angular form, server-owned validation and state creation, an e-Queue lookup
port with a deterministic mock, allow/deny decisions, idempotency, and audit.
Camera capture and VMS recognition remain separate integration concerns.

## Processing flow

```text
authenticated operator with vehicle.process
    -> valid persisted checkpoint/gate context
    -> manual plate and vehicle details
    -> optional GET /api/vehicle-entry/equeue
    -> explicit allow or deny decision
    -> antiforgery-protected POST /api/vehicle-entry
    -> server validation + e-Queue snapshot + atomic idempotency check
    -> Vehicle + VehicleVisit + audit event
```

The backend obtains checkpoint, gate, mode, direction, and user from trusted
session/context state. The client cannot submit those authority-bearing values.

## Gate modes and direction

Only gates receiving a vehicle onto checkpoint territory support this use case:

| Gate mode | Movement direction | Entry form |
| --- | --- | --- |
| `rearExit` / ВШ Тил Виїзд | `exit` | enabled |
| `borderEntry` / ВШ Кордон В’їзд | `entry` | enabled |
| `rearEntry` / ВШ Тил В’їзд | — | rejected |
| `borderExit` / ВШ Кордон Виїзд | — | rejected |

This mapping captures the Phase 4 interpretation of the checkpoint workflow and
must be confirmed against the approved operational process before production.

## Validation

The server is authoritative. The Angular form repeats the main rules only for
immediate feedback.

- plate numbers are normalized with Unicode compatibility normalization,
  uppercase conversion, and removal of spaces/hyphens;
- normalized plates contain 2–16 letters or digits;
- brand and type are required and limited to 80 characters;
- a regular vehicle requires 1–100 persons and supports at most two unique
  trailer plates;
- vehicle and trailer plates must be unique within the command;
- a service vehicle must send no persons count and no trailers;
- photo references are optional and limited to 2048 characters;
- the decision must explicitly be `allow` or `deny`.

The plate rule intentionally accepts national and foreign alphanumeric plates.
A country-specific registration-number catalogue is still a product decision.

## Decision and initial state

- `allow` creates the visit as `awaitingControls` with a pending vehicle
  inspection control;
- `deny` creates the terminal `entryDenied` visit with no pending control.

The e-Queue result is informational in Phase 4. Its absence does not
automatically override the operator's explicit decision because no such policy
is defined in the source requirements.

## Idempotency and audit

Each POST carries an 8–100 character idempotency key. The repository atomically
indexes it per user. Repeating identical normalized data returns the original
visit; reusing the key for different data returns HTTP 409. Only the first
successful creation writes an audit event.

The in-memory implementation models the future SQL transaction boundary. SQL
Server must enforce a unique `(CreatedByUserId, IdempotencyKey)` constraint.

## HTTP endpoints

| Endpoint | Purpose | Protection |
| --- | --- | --- |
| `GET /api/vehicle-entry/equeue?plateNumber=...` | normalized e-Queue lookup for current context | session + `vehicle.process` |
| `POST /api/vehicle-entry` | create allow/deny entry | session + permission + antiforgery |
| `GET /api/vehicle-entry/{visitId}` | read the created resource | session + permission + checkpoint visibility |

Unknown and out-of-zone IDs both return 404 to prevent object enumeration.

## Mock behavior and deferred work

`AA1234BB` returns the deterministic `eq-demo-001` e-Queue record. Other valid
plates return “not found”. Mock records live only for the browser/API process.

Deferred items:

- camera capture and photo upload/storage;
- VMS/ANPR ingestion and recognition expiry;
- real e-Queue protocol, credentials, status mapping, and failure policy;
- SQL Server persistence and database uniqueness constraints;
- the approved catalogue of vehicle types/brands and national plate formats.

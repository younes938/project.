# E-Ticket frontend

The production web client uses Angular 21 with strict TypeScript, standalone
components, zoneless change detection, SCSS, Angular ESLint, Prettier, and Vitest.

## Architecture

```text
src/app/
├── core/             Configuration, errors, and reusable state
├── domain/           Client view contracts and permission vocabulary
├── application/      Ports used by features
├── infrastructure/   API and mock adapter implementations
└── features/         Lazy-loaded business features
```

Components must depend on application ports rather than concrete HTTP payloads
or mock arrays. External DTOs must be mapped before entering feature UI.

The backend remains authoritative for business state transitions and access
control. Frontend status types exist for rendering API responses, not for
reimplementing the backend state machine.

## Commands

```sh
npm ci
npm start
npm run format:check
npm run lint
npm run test:ci
npm run build
npm run check
```

`npm run check` executes the complete frontend quality gate.

## Adapter selection

`DEFAULT_APP_CONFIG` currently selects `mock`. The Phase 2 auth mock creates one
fixed, in-memory demo session; a browser refresh intentionally clears it.
Switching to `api` activates the cookie-based API auth adapter and the HTTP
`VehicleVisitGateway`. Auth, VMS, e-Queue, customs, and border-control boundaries
remain isolated behind ports. Only auth has a functional mock; the other
integration mocks remain empty until their feature phases.

Route guards and `etHasPermission` are presentation controls only. Every data or
command endpoint must repeat permission and checkpoint-zone checks in the .NET
backend.

Phase 3 adds `OperatingContextGateway`, `OperatingContextStore`, and a route
guard that requires a valid checkpoint/gate selection before the work area can
open. Mock mode persists only validated demo selection IDs in `localStorage`;
API mode persists and revalidates the selection on the backend.

Phase 4 adds the lazy `/vehicle-entry` route, reactive manual-entry form,
conditional service-vehicle controls, e-Queue lookup, and guarded allow/deny
submission through `VehicleVisitGateway`. The UI generates an idempotency key,
but only the backend establishes uniqueness and creates the authoritative state.
In mock mode, plate `AA1234BB` has a deterministic e-Queue result.

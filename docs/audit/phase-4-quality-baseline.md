# Phase 4 quality baseline

Date: 2026-09-07

## Scope

- manual vehicle-entry form for landscape tablet/workstation layouts;
- conditional service-vehicle fields;
- e-Queue lookup abstraction and deterministic mock;
- server-owned context, permission, normalization, and validation;
- allow/deny state creation, audit, and idempotency;
- route, API, mock, store, and domain tests.

## Automated checks

| Check | Result |
| --- | --- |
| Frontend Prettier | pass |
| Angular ESLint | pass |
| Frontend Vitest | 21/21 pass across 7 files |
| Angular production build | pass |
| .NET Release build | pass, 0 warnings and 0 errors |
| .NET domain tests | 39/39 pass |

Angular production output was 271.98 kB raw for initial chunks (75.17 kB
estimated transfer). The lazy vehicle-entry chunk was 59.76 kB raw (12.94 kB
estimated transfer).

## Runtime verification

API smoke verification covers unauthenticated denial, permission-protected
lookup/create, antiforgery rejection, entry-mode rejection, deterministic queue
lookup, input validation, allow/deny state mapping, idempotent replay, conflicting
key rejection, and retrieval of the created resource.

Observed results:

- unauthenticated lookup: 401;
- missing context and unsupported gate mode: 409;
- missing antiforgery token, missing decision, invalid plate, and invalid service
  fields: 400;
- deterministic e-Queue lookup: 200 with `eq-demo-001`;
- first allow and deny commands: 201 with `awaitingControls` and `entryDenied`;
- identical idempotent replay: 200 with the original ID;
- changed payload with a reused key: 409;
- retrieval of the created resource: 200.

The development-only HTTP listener logged the expected HTTPS-redirection warning
because the smoke run intentionally bound only `http://127.0.0.1`; production
cookie policy continues to require HTTPS.

Visual browser verification remains unavailable in this environment because the
configured browser automation executable is not installed. Compilation,
template accessibility linting, unit tests, and responsive CSS checks completed;
a manual 1280×800 landscape review is still required before acceptance.

## Known prototype boundaries

- all backend visit/context data is process-local;
- mock frontend state is not an authorization boundary;
- e-Queue lookup is informational until operational policy is approved;
- camera/VMS and binary photo storage are deferred;
- exact plate and vehicle catalogues require product confirmation.

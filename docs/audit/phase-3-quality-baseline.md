# Phase 3 quality baseline

- Date: 2026-09-07
- Angular: 21.2.x
- Node.js used for verification: 24.20.0
- npm used for verification: 11.19.0
- .NET SDK used for verification: 10.0.400

## Delivered scope

- centralized `Checkpoint`, `Gate`, `GateMode`, and `OperatingContext` model;
- server-filtered visible checkpoint and active gate options;
- backend validation of active user, checkpoint, gate, zone, and gate ownership;
- server-derived mode, idempotent selection, persisted-context revalidation, and
  structured context-change audit;
- repository and catalog ports with process-local Phase 3 implementations;
- Angular gateway/store/guard boundary for mock and API modes;
- searchable checkpoint selection and accessible gate-mode controls;
- last mock selection restoration using validated, non-authoritative IDs;
- in-session context change action and duplicate-submit protection.

## Frontend

| Check | Result |
|---|---|
| Reproducible `npm ci` install | PASS |
| Prettier | PASS |
| Angular ESLint | PASS, 0 errors |
| Vitest | PASS, 14/14 tests in 6 files |
| Production build | PASS |
| npm audit | PASS, 0 vulnerabilities |
| Initial production bundle | 246.79 kB raw, 67.47 kB estimated transfer |

Six new tests cover all four mock gate modes, persisted selection restoration,
unknown-gate rejection, store refresh/select behavior, and duplicate request
suppression.

During verification, npm reported a new moderate advisory against transitive
`qs@6.15.3` through Angular CLI tooling. A non-force `npm audit fix` updated only
the affected transitive package to `qs@6.16.0`; the full frontend gate passed
afterward.

Automated visual verification remains unavailable because the environment does
not provide the browser skill's `agent-browser` executable. The production
template/compiler checks pass, but a future E2E environment must still verify the
1280×800 target viewport.

## Backend

| Check | Result |
|---|---|
| NuGet locked restore and audit | PASS |
| `dotnet format --verify-no-changes` | PASS |
| Release build with warnings as errors | PASS, 0 warnings and 0 errors |
| xUnit | PASS, 29/29 tests |

Six new domain tests cover accepted selection and denial for inactive users,
inactive checkpoints, invisible checkpoints, inactive gates, and gates belonging
to another checkpoint.

## API runtime smoke test

The Development API was exercised locally with a temporary cookie jar and JSON
response files; all temporary files were removed after the run.

| Check | Expected | Actual |
|---|---:|---:|
| options before authentication | 401 | 401 |
| sign-in and visible options | 200 | 200 |
| visible checkpoints / gates | 1 / 4 | 1 / 4 |
| hidden checkpoint absent from options | true | true |
| current context before selection | 204 | 204 |
| selection without CSRF token | 400 | 400 |
| hidden checkpoint selection | 404 | 404 |
| gate/checkpoint mismatch | 400 | 400 |
| valid selection | 200 | 200 |
| repeated selection keeps timestamp | true | true |
| context change | 200 | 200 |
| restored changed mode | `borderEntry` | `borderEntry` |

## Deferred production work

- SQL Server implementations of the catalog and per-user context repository;
- active/inactive dictionary administration and cache invalidation;
- product-owner confirmation of the four gate labels and direction semantics;
- multi-device context conflict policy;
- browser E2E and 1280×800 visual regression coverage;
- business endpoint enforcement using the server-side operating context.

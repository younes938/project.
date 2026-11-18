# Phase 1 quality baseline

- Date: 2026-08-28
- Angular: 21.2.x
- Node.js used for verification: 24.20.0
- npm used for verification: 11.19.0
- .NET SDK used for verification: 10.0.400

## Frontend

| Check | Result |
|---|---|
| Prettier | PASS |
| Angular ESLint | PASS, 0 errors |
| Vitest | PASS, 2/2 tests |
| Production build | PASS |
| npm audit | PASS, 0 vulnerabilities |
| Initial production bundle | 209.55 kB raw, 57.93 kB estimated transfer |

The tests cover creation of the Angular shell and the absence of embedded demo
records in the mock vehicle gateway. Business component tests begin with their
respective feature phases.

## Backend

| Check | Result |
|---|---|
| NuGet restore and audit | PASS |
| Build with warnings as errors | PASS, 0 warnings and 0 errors |
| xUnit | PASS, 18/18 tests |

Backend tests cover allowed/illegal vehicle transitions, terminal states, and
the 180-day statistics period boundary.

## Deferred verification

- GitHub Actions execution requires commit/push.
- Browser verification at 1280x800 begins when functional UI is introduced.
- API integration tests begin with actual business endpoints.
- SQL Server, authentication, and external adapters are intentionally absent in
  Phase 1.

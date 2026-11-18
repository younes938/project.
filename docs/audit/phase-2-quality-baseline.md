# Phase 2 quality baseline

- Date: 2026-08-28
- Angular: 21.2.x
- Node.js used for verification: 24.20.0
- npm used for verification: 11.19.0
- .NET SDK used for verification: 10.0.400

## Delivered scope

- isolated mock/API authentication adapters;
- Development-only fixed demo identity;
- HttpOnly cookie session with sliding idle timeout;
- antiforgery token validation for auth POST endpoints;
- backend permission policies and checkpoint visibility enforcement;
- per-request user revalidation and fail-closed inactive/unknown identities;
- login/logout structured audit events;
- Angular session store, bootstrap initialization, route guards, permission UI
  directive, access-denied screen, and idle logout;
- documented КЕП/MFA and production key-management decisions.

## Frontend

| Check | Result |
|---|---|
| Prettier | PASS |
| Angular ESLint | PASS, 0 errors |
| Vitest | PASS, 8/8 tests in 4 files |
| Production build | PASS |
| npm audit | PASS, 0 vulnerabilities |
| Initial production bundle | 235.44 kB raw, 64.29 kB estimated transfer |

Tests cover the application shell, empty vehicle mock, auth mock lifecycle,
least-privilege demo permissions, AuthStore anonymous initialization,
permission/zone derivation, and fail-safe local logout.

The Angular dev server compiled successfully. Automated visual verification at
1280×800 could not run because the environment-provided browser skill had no
`agent-browser` executable; no replacement package was installed.

## Backend

| Check | Result |
|---|---|
| NuGet locked restore and audit | PASS |
| `dotnet format --verify-no-changes` | PASS |
| Release build with warnings as errors | PASS, 0 warnings and 0 errors |
| xUnit | PASS, 23/23 tests |

The five new domain tests cover granted and absent permissions, checkpoint-zone
access, inactive-user denial, and distinct visibility-zone union behavior.

## API runtime smoke test

The Development API was started locally and exercised with a temporary cookie
jar that was removed immediately after the run.

| Request | Expected | Actual |
|---|---:|---:|
| session before sign-in | 401 | 401 |
| issue antiforgery token | 204 | 204 |
| mock sign-in | 200 | 200 |
| authenticated session | 200 | 200 |
| granted permission policy | 204 | 204 |
| allowed checkpoint zone | 204 | 204 |
| denied checkpoint zone | 403 | 403 |
| activity heartbeat | 204 | 204 |
| logout | 204 | 204 |
| session after logout | 401 | 401 |

## Deferred production work

- real КЕП and MFA provider/protocol;
- persisted and protected ASP.NET Data Protection key ring;
- production session concurrency, absolute lifetime, revocation, and recovery
  policies;
- integration/E2E browser suite and visual verification at the target viewport;
- SQL Server identity, role, visibility-zone, audit, and session persistence;
- audit events for permission changes once the administration use case exists.

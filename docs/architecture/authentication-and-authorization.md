# Authentication and authorization

## Phase 2 scope

Phase 2 establishes the authentication boundary, session lifecycle, RBAC,
checkpoint visibility, and idle logout. It does not claim production readiness
for qualified electronic signatures (КЕП) or MFA because the identity provider,
signature protocol, trust chain, supported hardware, and recovery rules are not
yet approved.

The `AuthAdapter` remains the only integration boundary for identity proofing.
The current implementation accepts one fixed, least-privilege demo identity and
is intentionally unavailable outside the Development environment.

## Trust boundary

```text
Angular route / UI guard (usability only)
              |
              v
      HttpOnly session cookie
              |
              v
ASP.NET authentication + authorization policy
              |
              +--> current User reloaded through AuthAdapter on every request
              +--> Permission claim policy
              +--> checkpoint visibility check
              +--> structured authentication audit
```

The browser never creates permission or checkpoint claims. ASP.NET builds them
from the backend `User`, `Role`, and `VisibilityZone` domain models. Cookie
validation reloads the current user on every request, so a missing or inactive
user, an unknown authentication method, or an unavailable identity adapter fails
closed. A future role or zone update is reflected on the next authenticated
request.

## Session and CSRF controls

- the session cookie is `HttpOnly`, `SameSite=Strict`, non-persistent, and uses a
  15-minute sliding idle timeout;
- production cookie names use the `__Host-` prefix and require HTTPS;
- state-changing authentication endpoints validate an antiforgery cookie/header
  pair;
- API authentication failures return HTTP 401/403 instead of HTML redirects;
- the Angular idle monitor resets on keyboard, pointer, or touch activity,
  throttles server activity reports to once per minute, and signs out after the
  server-provided timeout;
- the backend cookie expiration remains authoritative if client timers are
  paused or bypassed.

The Angular API adapter assumes same-origin `/api` deployment so the browser can
apply cookie and XSRF protections. A cross-origin deployment requires an explicit
CORS, credential, and CSRF design and must not be enabled by changing only the
base URL.

## Development endpoints

| Endpoint | Access | Purpose |
|---|---|---|
| `GET /api/auth/antiforgery` | anonymous | issues a request token |
| `POST /api/auth/mock/sign-in` | Development only | creates the fixed demo session |
| `GET /api/auth/session` | authenticated | returns the read-only session summary |
| `POST /api/auth/activity` | authenticated + CSRF | renews active session traffic |
| `POST /api/auth/logout` | authenticated + CSRF | closes the cookie session |
| `GET /api/auth/permissions/vehicle-view` | permission policy | demonstrates backend RBAC enforcement |
| `GET /api/auth/checkpoints/{id}/access` | authenticated | demonstrates backend zone enforcement |

Authentication success, denial, and logout are written through `IAuditSink`
without credentials, signature material, private keys, or request bodies.

## Deferred production decisions

Before implementing КЕП/MFA, the product and security owners must approve:

1. the КЕП provider and supported file/hardware carriers;
2. whether signing proves login only or also authorizes individual operations;
3. MFA factors, enrollment, recovery, lockout, and support procedures;
4. certificate validation, revocation, timestamping, and trust-list rules;
5. session concurrency, absolute lifetime, forced revocation, and reauthentication;
6. persistent ASP.NET Data Protection key storage and rotation topology;
7. the production reverse-proxy/TLS and same-origin deployment model.

Until those decisions are made, production has no mapped sign-in endpoint and
therefore fails closed.

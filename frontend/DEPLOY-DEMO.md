# E-Ticket Border Control — frontend demo deployment

This copy is configured to use the existing mock adapters (`adapterMode: 'mock'`) so the frontend does not call the .NET API for its configured authentication, operating-context, and vehicle-visit gateways.

## Build locally (Windows)
1. Install Node.js LTS (the project declares npm 11).
2. Open PowerShell in `frontend`.
3. Run `npm ci` to install dependencies for your own operating system.
4. Run `npm run build`.
5. Angular's production output is under `dist/` (check `angular.json` if your CLI emits a nested browser folder).
6. Publish the generated static output folder to a static hosting provider such as Netlify or Cloudflare Pages.

## Important limitations
- This is a visual/demo configuration, not a production deployment.
- Mock authentication still requires using the sign-in screen; mock account data and permissions are defined in the source.
- Some pages may be empty or limited because mock adapters intentionally return empty data. Check each page after building.
- Do not put real passenger data, credentials, or secrets in a public demo.
- If deep links return 404 on hosting, configure the host to rewrite unknown routes to `index.html` for Angular routing.

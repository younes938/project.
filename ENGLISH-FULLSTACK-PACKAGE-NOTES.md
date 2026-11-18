# English Frontend + Full Stack Package

This package combines the English-converted Angular frontend with the complete project tree, including the ASP.NET Core backend, SQL Server scripts, docs, and configuration.

## Folders
- `frontend/`: Angular frontend (English-converted version).
- `backend/`: ASP.NET Core solution/projects and SQL Server scripts under `backend/database/`.
- `docs/`: architecture and project documentation.

## Important validation note
The English frontend archive was overlaid onto the full-stack project. This packaging step does not prove that every UI string is translated or that the frontend/backend/database have passed build, integration, or runtime tests. Run the frontend build and backend tests on the development machine before deployment.

## Frontend
From `frontend/` run:
```powershell
npm ci
npm run build
```

## Backend
From `backend/` use the installed .NET SDK and the solution file to restore, build, and test. Review `appsettings*.json` and configure a local SQL Server connection string before running.

## SQL Server
Review and execute the scripts in `backend/database/` in their documented order against a dedicated demo database. Do not use real passenger data in a public demo.

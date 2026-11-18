# Arabic / English status

This package preserves the complete original full-stack project (.NET backend, database scripts, docs, and Angular frontend) and includes the current Arabic/English language-switch foundation in `frontend`.

IMPORTANT: This is not a finished fully translated release. The language service persists `en`/`ar` and updates document `lang`/`dir`, but all page templates, TypeScript messages, mock-data labels, status labels, and date/number formatting have not yet been audited and translated. Do not publish as a completed bilingual demo until that work and build/tests are complete.

Frontend verification from `frontend`:

```powershell
npm ci
npm run build
npm start
```

Backend/database configuration remains the original project's configuration. Review secrets, connection strings, and demo data before any public deployment.

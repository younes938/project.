# Arabic / English language support status

This frontend contains the first-stage language infrastructure:

- `LanguageService` stores `en` or `ar` in localStorage (`eticket-language`).
- English is the default on first visit.
- The root language switch toggles between English and Arabic.
- The service updates the document `lang` and `dir` attributes immediately.
- Global RTL/LTR direction rules are included in `src/styles.scss`.

## Important limitation

This is not yet a complete translation of every user-facing string. Every page template, TypeScript validation/error message, status label, mock-data label, and date/number formatter still needs to be audited and connected to translation keys. Do not treat this build as a fully bilingual release until that audit and the Angular build/tests pass.

## Verify locally

From the `frontend` directory:

```bash
npm ci
npm run build
npm start
```

Use the language button, refresh the browser, and confirm the selection persists. Then check every route in both languages.

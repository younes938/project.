# Arabic / English implementation update

- Language preference remains `eticket-language` in localStorage.
- The root document `lang` and `dir` attributes are switched immediately.
- Removed hard-coded `dir="ltr"` / `dir="rtl"` from Angular templates so the global direction can take effect.
- Added a centralized runtime text dictionary for the shared navigation and common dashboard labels, including the mixed Arabic/English dashboard label.

## Important scope note
This is a partial translation pass, not a certified complete translation of every page. Labels not yet present in the dictionary remain in their source language. Review all routes in both languages and extend the dictionary before presenting this as fully bilingual.

## Build verification
`npm run build` could not be completed in this environment because Angular CLI dependencies are not installed (`ng: not found`). From `frontend`, run `npm ci` and then `npm run build` on the development machine.

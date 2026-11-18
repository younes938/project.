# E-Ticket: загальний технічний огляд проєкту

Актуально станом на 2026-09-08. Документ описує стан репозиторію після
завершення Phase 0–4, пояснює призначення модулів і основних класів та визначає
подальший порядок розробки.

## 1. Призначення системи

ПС «e-ticket» призначена для роботи посадових осіб на автомобільних пунктах
пропуску. Система має супроводжувати транспортний засіб від отримання номерного
знака та рішення про в’їзд через контрольні процедури до остаточного виїзду,
пошуку, експорту, роботи з є-Чергою, статистики та аудиту.

Цільова платформа, затверджена в
[ADR-0001](docs/adr/0001-target-client-platform.md):

- Angular 21 — продукційний web frontend;
- .NET 10 — авторитетний backend;
- Microsoft SQL Server 2019 — майбутня продукційна база даних;
- Windows 11, планшетні та стаціонарні АРМ;
- основний UI-режим — landscape, від 1280×800 до приблизно 1920×1200;
- Android/Kotlin застосунок збережено як прототип і джерело UX-сценаріїв, але
  не як продукційний клієнт.

## 2. Поточний стан

Завершено чотири функціональні етапи після первинної стабілізації:

| Етап | Стан | Що отримано |
| --- | --- | --- |
| Phase 0 — стабілізація | завершено | build baseline, CI, документація prerequisite, executable Gradle wrapper |
| Phase 1 — core architecture | завершено | Angular/.NET workspaces, домен, порти, адаптери, state machine, error handling |
| Phase 2 — auth/session/RBAC | завершено як prototype | cookie session, mock login, CSRF, permissions, visibility zones, idle logout |
| Phase 3 — operating context | завершено як prototype | вибір КПП і шлагбаума, режим із `Gate`, збереження та повторна серверна перевірка |
| Phase 4 — vehicle entry | завершено як prototype | ручне введення ТЗ, валідація, службове ТЗ, є-Черга, allow/deny, audit, idempotency |

Система компілюється і тестується, але ще не готова до production: SQL Server,
КЕП/MFA, реальні зовнішні інтеграції, контрольні процедури, виїзд, пошук,
експорт і статистика ще не реалізовані.

Останній зафіксований quality gate:

- Angular: format, lint і production build — успішно;
- frontend tests: 21/21;
- .NET Release build: 0 warnings, 0 errors;
- backend domain tests: 39/39;
- HTTP smoke flow Phase 4 — успішно;
- Android baseline: build пройдено, lint має 60 відомих warnings, runtime в
  емуляторі не перевірявся.

Деталі: [Phase 4 quality baseline](docs/audit/phase-4-quality-baseline.md) і
[Android build baseline](docs/audit/build-quality-baseline.md).

## 3. Що саме було змінено

### Phase 0 — стабілізація репозиторію

- зафіксовано фактичний стан Android-прототипу;
- виправлено executable-біт `gradlew` для Linux;
- доповнено `.gitignore` для локальних SDK, build output і секретів;
- додано Android CI та target-platform CI;
- задокументовано версії, build-команди, lint warnings і обмеження тестів;
- прийнято рішення не розвивати Android як паралельний production-клієнт.

### Phase 1 — цільова архітектура

- створено `frontend/` на Angular 21;
- створено .NET solution `backend/ETicket.slnx` із розділенням Domain,
  Application, Infrastructure, Integrations та Api;
- створено базові доменні моделі транспортного засобу, візиту, контролів,
  користувачів, черги, камер, статистики та аудиту;
- створено централізовану `VehicleStateTransitionService`;
- створено порти зовнішніх систем і persistence;
- додано mock/API перемикання через dependency injection;
- додано централізоване безпечне опрацювання помилок.

### Phase 2 — автентифікація та доступ

- додано `AuthAdapter` та Development-only mock identity;
- backend видає `HttpOnly`, `SameSite=Strict` cookie session;
- додано antiforgery cookie/header flow для state-changing endpoints;
- додано permission policies і claims;
- реалізовано `Role`, `Permission`, `VisibilityZone` та серверні перевірки;
- Angular отримав `AuthStore`, guards, permission directive й idle logout;
- login/logout і відмови пишуться через `IAuditSink`.

### Phase 3 — робочий контекст

- додано каталог доступних КПП і шлагбаумів;
- `Gate` став єдиним джерелом `GateMode`;
- контекст перевіряється за active state, належністю gate до checkpoint і
  visibility zone користувача;
- останній вибір зберігається за `IOperatingContextRepository`;
- Angular отримав сторінку вибору, пошук, `OperatingContextStore` і route guard;
- повторний однаковий вибір не створює новий audit event.

### Phase 4 — оформлення в’їзду ТЗ

- додано responsive reactive form `/vehicle-entry`;
- реалізовано нормалізацію номерів, валідацію марки, типу, осіб, причепів і
  посилання на фото;
- для службового ТЗ особи та причепи заборонені на backend і вимкнені в UI;
- додано lookup є-Черги через `IEQueueAdapter`;
- mock-номер `AA1234BB` повертає запис `eq-demo-001`;
- `allow` створює `AwaitingControls`, `deny` — `EntryDenied`;
- checkpoint, gate, mode, direction і user беруться із серверного контексту, а
  не з недовірених полів браузера;
- додано атомарний idempotency key на користувача та конфлікт 409;
- додано structured audit для першого успішного створення.

Android-файли `app/` не переписувалися під цільову архітектуру. Наявні локальні
Android-зміни та Room/Compose сценарії збережені окремо від Angular/.NET роботи.

## 4. Загальна архітектура

```text
Користувач у браузері
        |
        v
Angular feature component
        |
        v
Store / application port
        |
        +---- Mock adapter, локальний prototype mode
        |
        +---- API adapter ---- HTTP/JSON ----> ETicket.Api
                                                |
                                                v
                                      ETicket.Application
                                         /           \
                                        v             v
                                ETicket.Domain   application ports
                                                     |
                                 +-------------------+-------------------+
                                 v                   v                   v
                         Infrastructure        Integrations       future SQL Server
                         persistence/audit     external APIs
```

Головне правило залежностей: бізнес-правила спрямовані всередину до Domain.
Domain не залежить від Angular, ASP.NET, SQL Server, Room або зовнішніх API.

### Розподіл відповідальності

| Рівень | Відповідальність |
| --- | --- |
| Angular UI | форма, відображення, loader/error/success, UX-перевірки, запобігання випадковому double click |
| Angular stores | стан feature, координація асинхронних викликів, але не авторитетні бізнес-переходи |
| API | HTTP contracts, session, CSRF, permission policy, Problem Details |
| Application | сценарії використання, orchestration портів, idempotency flow, audit flow |
| Domain | моделі, інваріанти, permission logic, допустимі переходи статусів |
| Infrastructure | persistence, audit sink, clock; зараз process-local |
| Integrations | Auth/VMS/e-Queue/customs/border adapters; зараз mock |
| SQL Server 2019 | майбутній авторитетний durable storage |

## 5. Структура репозиторію

```text
E-Ticket/
├── app/                         Android-прототип
├── backend/
│   ├── src/
│   │   ├── ETicket.Domain/      моделі, enums, бізнес-інваріанти
│   │   ├── ETicket.Application/ use cases і ports
│   │   ├── ETicket.Infrastructure/ persistence, audit, time
│   │   ├── ETicket.Integrations/ зовнішні adapters і mocks
│   │   └── ETicket.Api/         ASP.NET endpoints, auth, DI
│   ├── tests/                   xUnit domain tests
│   ├── Directory.Build.props    спільні C# quality rules
│   ├── Directory.Packages.props централізовані NuGet versions
│   └── ETicket.slnx             .NET solution
├── frontend/
│   ├── src/app/
│   │   ├── domain/              frontend types і permission vocabulary
│   │   ├── application/ports/   UI-facing interfaces
│   │   ├── core/                stores, guards, session, errors
│   │   ├── infrastructure/      API і mock implementations
│   │   └── features/            lazy-loaded screens
│   ├── angular.json             build/test/lint config
│   └── package.json             npm scripts і dependencies
├── docs/
│   ├── adr/                     прийняті архітектурні рішення
│   ├── architecture/            правила модулів і бізнес-потоків
│   └── audit/                   quality baselines по фазах
├── .github/workflows/           Android та Angular/.NET CI
├── README.md                    коротка точка входу
└── PROJECT_OVERVIEW.md          цей повний огляд
```

## 6. Frontend: Angular 21

### Стек

| Частина | Технологія |
| --- | --- |
| Framework | Angular 21.2 |
| Мова | TypeScript 5.9, strict mode і strict templates |
| UI | standalone components, HTML, SCSS |
| Forms | Angular Reactive Forms |
| State | Angular signals у feature stores |
| Routing | Angular Router, lazy-loaded components |
| HTTP | `HttpClient`, cookie credentials, XSRF header |
| Tests | Vitest через Angular unit-test builder |
| Quality | Angular ESLint + Prettier + production budgets |

### Запуск і composition root

| Файл/клас | Відповідальність |
| --- | --- |
| `main.ts` | bootstrap Angular application |
| `App` | мінімальна оболонка з `RouterOutlet` |
| `app.config.ts` | router, global error handler, app initializer, infrastructure providers |
| `app.routes.ts` | усі маршрути та guards |
| `providers.ts` | вибір mock або API implementations через DI tokens |
| `AppConfig` / `DEFAULT_APP_CONFIG` | `adapterMode` і `apiBaseUrl`; зараз `mock` та `/api` |

### Маршрути

| URL | Компонент | Захист |
| --- | --- | --- |
| `/sign-in` | `SignIn` | лише anonymous user |
| `/forbidden` | `AccessDenied` | authenticated user |
| `/operating-context` | `OperatingContextSelection` | authentication |
| `/vehicle-entry` | `VehicleEntry` | auth + operating context + `vehicle.process` |
| `/` | `PlatformStatus` | auth + operating context |
| `**` | redirect `/` | fallback |

### Core-класи

| Клас/функція | Відповідальність |
| --- | --- |
| `AuthStore` | session state, sign-in/out, activity renewal, permission/zone helpers |
| `IdleSessionService` | слухає keyboard/pointer/touch activity, throttling activity ping, automatic logout |
| `authGuard` | не пропускає anonymous user |
| `anonymousOnlyGuard` | не показує sign-in авторизованому користувачу |
| `requirePermission()` | route-level UX permission check |
| `requireCheckpointAccess()` | route-level checkpoint UX check для майбутніх detail routes |
| `HasPermissionDirective` | умовно показує UI елемент за permission |
| `OperatingContextStore` | options/current context, loading і saving states |
| `operatingContextGuard` | вимагає чинний контекст або redirect на вибір |
| `VehicleEntryStore` | queue lookup, submit, busy/success/error state, UI double-submit guard |
| `GlobalErrorHandler` | перехоплює uncaught Angular errors |
| `ErrorReporter` | безпечна точка для майбутнього telemetry/logging adapter |
| `AsyncState<T>` | загальний typed state contract, підготовлений для інших features |

Frontend guards і directive є лише UX-рівнем. Вони не замінюють backend
authorization.

### Application ports і adapters

| Порт | API implementation | Mock implementation | Призначення |
| --- | --- | --- | --- |
| `AuthAdapter` | `ApiAuthAdapter` | `MockAuthAdapter` | session і authentication |
| `OperatingContextGateway` | `ApiOperatingContextGateway` | `MockOperatingContextGateway` | checkpoint/gate options і поточний context |
| `VehicleVisitGateway` | `ApiVehicleVisitGateway` | `MockVehicleVisitGateway` | vehicle entry, lookup, read/list contracts |
| `VmsAdapter` | ще немає | `MockVmsAdapter` | майбутні ANPR/VMS recognition events |
| `EQueueAdapter` | UI напряму не викликає real external API | `MockEQueueAdapter` | frontend boundary, основна orchestration виконується backend |
| `CustomsAdapter` | ще немає | `MockCustomsAdapter` | майбутній митний контроль |
| `BorderControlAdapter` | ще немає | `MockBorderControlAdapter` | майбутній прикордонний контроль |

### Feature-компоненти

| Компонент | Відповідальність |
| --- | --- |
| `SignIn` | Development/mock sign-in flow |
| `AccessDenied` | повідомлення про відсутність permission |
| `OperatingContextSelection` | пошук КПП, вибір gate, збереження context |
| `PlatformStatus` | стартове робоче місце, session/context summary і навігація |
| `VehicleEntry` | reactive form, service-vehicle UX, e-Queue lookup, allow/deny result |

### Frontend domain types

- `GateOption`, `CheckpointOption`, `OperatingContextSummary` — робочий контекст;
- `VehicleVisitStatus`, `ControlKind`, `ControlStatus`, `GateMode` — vocabulary
  для відображення серверного стану;
- `CreateVehicleEntry`, `CreatedVehicleEntry`, `QueueLookupResult` — Phase 4
  contracts;
- `Permission` — typed перелік permission claims.

Frontend types не є окремим бізнес-доменом: вони описують API/view contracts, а
авторитетні правила залишаються у .NET.

## 7. Backend: .NET 10

### Проєкти solution

| Проєкт | Призначення |
| --- | --- |
| `ETicket.Domain` | чистий домен без HTTP, БД та integration dependencies |
| `ETicket.Application` | use cases, result contracts і ports |
| `ETicket.Infrastructure` | реалізації persistence, clock та audit |
| `ETicket.Integrations` | mock і майбутні external adapters |
| `ETicket.Api` | ASP.NET composition root і HTTP endpoints |
| `ETicket.Domain.Tests` | xUnit unit tests доменних правил |

`Directory.Build.props` вмикає nullable reference types, deterministic build,
locked restore, NuGet audit і `TreatWarningsAsErrors`.

### Domain models

| Модель | Основні дані |
| --- | --- |
| `Vehicle` | номер, марка, тип, service flag, photo reference, trailers |
| `Trailer` | номер і послідовність 1/2 |
| `Checkpoint` | ID, код, назва, active state |
| `Gate` | checkpoint, код, назва, authoritative `GateMode`, active state |
| `OperatingContext` | user, checkpoint, gate, mode, timestamp |
| `VehicleVisit` | vehicle/user/context, direction, persons, queue ID, status, controls, timestamps, version |
| `ControlResult` | вид контролю, state, хто/коли вирішив, reason/explanation |
| `Inspection` | окреме рішення ОТЗ і negative reason |
| `QueueEntry` | external queue ID/status і last sync time |
| `CameraRecognition` | camera/location/time/plate/photo та expiration |
| `User` | external identity, roles, zones, active state |
| `Role` | код/назва і набір permissions |
| `VisibilityZone` | перелік доступних checkpoint IDs |
| `StatisticsFilter` | майбутні валідовані фільтри статистики |
| `AuditEvent` | структурований журнал дії з previous/new value |

### Основні enums

- `VehicleVisitStatus`: `EntryDenied`, `AwaitingControls`, `ControlFailed`,
  `AwaitingExit`, `Passed`, `Return`, `Cancelled`;
- `ControlKind`: `VehicleInspection`, `Border`, `Customs`;
- `ControlStatus`: `Pending`, `Passed`, `Failed`, `NotRequired`, `Cancelled`;
- `GateMode`: `RearExit`, `BorderExit`, `BorderEntry`, `RearEntry`;
- `MovementDirection`: `Entry`, `Exit`;
- `VehicleEntryDecision`: `Unspecified`, `Allow`, `Deny`;
- `InspectionRejectionReason`: prohibited items, driver request, registration
  mismatch, other;
- `Permission`: processing, inspection, search, view/edit/cancel/export, queue,
  statistics, persons correction;
- `AuthenticationMethod`: `Mock`, `Kep`.

### Domain services

| Клас | Відповідальність |
| --- | --- |
| `AccessControlService` | permission checks і checkpoint visibility |
| `OperatingContextSelectionService` | active user/checkpoint/gate, visibility і ownership validation |
| `VehicleEntryValidationService` | normalization та conditional validation Phase 4; mode→direction mapping |
| `VehicleStateTransitionService` | централізована матриця legal status transitions |
| `DomainRuleViolationException` | typed exception для порушення інваріанта |

### Application services і contracts

| Клас/record | Відповідальність |
| --- | --- |
| `OperationResult<T>` / `ApplicationError` | очікувана success/failure відповідь use case без exceptions як control flow |
| `AuthenticationRequest` | внутрішній контракт authentication adapter |
| `OperatingContextService` | options, current context, select, повторна validation, audit |
| `VehicleEntryService` | permission/context validation, e-Queue lookup, create/read, idempotency, audit |
| `CreateVehicleEntryCommand` | недовірені поля команди, що приходять від клієнта |
| `CreatedVehicleEntry` | результат use case разом із `WasCreated` для replay |
| `VehicleEntryErrorCodes` | стабільні machine-readable коди помилок Phase 4 |

### Ports

| Interface | Призначення |
| --- | --- |
| `IAuthAdapter` | authentication та актуальне завантаження user |
| `IVmsAdapter` | список recognition events і mark processed |
| `IEQueueAdapter` | lookup за checkpoint/plate і pause/resume |
| `ICustomsAdapter` | результат митного контролю |
| `IBorderControlAdapter` | результат прикордонного контролю |
| `IVehicleVisitRepository` | get/list/add-or-get-idempotent/update-by-version |
| `IOperatingContextRepository` | get/upsert/delete context за user |
| `ICheckpointCatalog` | checkpoints і gates |
| `IAuditSink` | append structured audit event |
| `IClock` | контрольоване джерело authoritative UTC time |

### Infrastructure implementations

| Клас | Поточна поведінка |
| --- | --- |
| `InMemoryCheckpointCatalog` | один доступний demo КПП, чотири gates і один hidden КПП для security checks |
| `InMemoryOperatingContextRepository` | process-local context на user |
| `InMemoryVehicleVisitRepository` | process-local visits, optimistic version і atomic idempotency index |
| `InMemoryAuditSink` | process-local concurrent audit queue |
| `SystemClock` | системний UTC час |

Усі `InMemory*` дані зникають після перезапуску API. Це навмисна prototype
межа, а не production persistence.

### Integration mocks

| Клас | Стан |
| --- | --- |
| `MockAuthAdapter` | активний demo operator із least-privilege permissions і однією zone |
| `MockEQueueAdapter` | `AA1234BB` → `eq-demo-001`; інші valid plates → not found |
| `MockVmsAdapter` | порожній, Phase 5 |
| `MockCustomsAdapter` | порожній, Phase 7/13 |
| `MockBorderControlAdapter` | порожній, Phase 7/13 |

### API та security-класи

| Клас | Відповідальність |
| --- | --- |
| `Program` | configuration, DI, cookie auth, policies, middleware, endpoint mapping |
| `OperatingContextEndpoints` | options/current/select HTTP contracts |
| `VehicleEntryEndpoints` | queue lookup, create і get created entry |
| `AntiforgeryValidationFilter` | перевірка XSRF cookie/header для commands |
| `UserPrincipalFactory` | створення claims лише з backend `User` |
| `PermissionClaims` / `PermissionPolicies` | mapping domain permissions на policy names |
| `ClaimsPrincipalExtensions` | checkpoint claim helper |
| `SessionResponse` | безпечний read-only session DTO |
| `ETicketSessionOptions` | mock sign-in flag та idle timeout |
| `AuthAudit` | структуровані auth events без credentials |
| `GlobalExceptionHandler` | RFC Problem Details без витоку exception details |

## 8. HTTP API

| Method і URL | Призначення | Захист |
| --- | --- | --- |
| `GET /health` | readiness/health відповідь | public |
| `GET /api/system/info` | service, phase, adapter mode | public |
| `GET /api/auth/antiforgery` | видає XSRF token | public |
| `POST /api/auth/mock/sign-in` | demo login | лише Development + CSRF |
| `GET /api/auth/session` | current session | authenticated |
| `POST /api/auth/activity` | продовження активності | auth + CSRF |
| `POST /api/auth/logout` | завершення session | auth + CSRF |
| `GET /api/operating-context/options` | видимі active checkpoints/gates | auth |
| `GET /api/operating-context/current` | поточний revalidated context | auth |
| `PUT /api/operating-context/current` | вибрати/change context | auth + CSRF |
| `GET /api/vehicle-entry/equeue` | lookup current checkpoint + plate | `vehicle.process` |
| `POST /api/vehicle-entry` | створити allow/deny visit | `vehicle.process` + CSRF |
| `GET /api/vehicle-entry/{id}` | прочитати створений visit | permission + zone/404 |

API використовує JSON enums у camelCase і RFC Problem Details. Для unknown та
out-of-zone object IDs застосовується однакова 404 відповідь, щоб не розкривати
існування чужих даних.

## 9. Основні робочі потоки

### Authentication

```text
SignIn component
  -> AuthStore
  -> AuthAdapter
  -> antiforgery token
  -> Development mock sign-in
  -> HttpOnly session cookie
  -> backend reloads current User on every authenticated request
```

Production КЕП/MFA ще не підключені. У production mock sign-in endpoint взагалі
не реєструється, тому система fail-closed.

### Вибір робочого контексту

```text
OperatingContextSelection
  -> OperatingContextStore
  -> OperatingContextGateway
  -> backend current User
  -> active + visible Checkpoint
  -> active Gate belonging to Checkpoint
  -> Gate.Mode
  -> repository + audit
```

### Оформлення в’їзду

```text
VehicleEntry form
  -> optional e-Queue lookup
  -> explicit allow/deny
  -> VehicleEntryStore blocks repeated click
  -> POST with idempotency key + CSRF
  -> backend repeats permission/context/input validation
  -> queue snapshot
  -> atomic repository operation
  -> Vehicle + VehicleVisit + AuditEvent
```

Для Phase 4 в’їзд на територію ПП дозволений у контекстах `RearExit` і
`BorderEntry`. Це поточна інтерпретація ТЗ і потребує підтвердження product owner.

## 10. Поточна state machine транспортного засобу

| Поточний статус | Дозволені наступні статуси |
| --- | --- |
| `EntryDenied` | немає, terminal |
| `AwaitingControls` | `ControlFailed`, `AwaitingExit`, `Cancelled` |
| `ControlFailed` | `Return`, `Cancelled` |
| `AwaitingExit` | `Passed`, `Return`, `Cancelled` |
| `Passed` | немає, terminal |
| `Return` | немає, terminal |
| `Cancelled` | немає, terminal |

UI не має права довільно змінювати ці статуси. Майбутні команди передають
бажаний результат, а backend `VehicleStateTransitionService` підтверджує legal
transition.

## 11. Android-прототип

### Стек

- Kotlin 2.0.21;
- Jetpack Compose + Material 3;
- Room 2.8.4 + KSP;
- Navigation Compose 2.9.8;
- AGP 8.13.2, compile/target SDK 36, min SDK 24;
- Gradle 8.13, JDK 21 для build environment.

### Основні класи

| Клас/файл | Відповідальність |
| --- | --- |
| `MainActivity` | створює Room database, repository, ViewModel і Compose navigation |
| `AppDatabase` / `DatabaseProvider` | локальна Room database |
| `TransportPassEntity` | локальна prototype-сутність талона і stage/check fields |
| `TransportPassDao` | Room queries для списків за роллю/етапом |
| `Converters` | Room enum conversions |
| `TransportRepository` | створення green/red ticket та послідовність PD→DMS→BORDER |
| `TransportViewModel` | StateFlow списку й coroutine actions create/approve/reject |
| `TransportViewModelFactory` | створення ViewModel із repository dependency |
| `AppNavigation` | маршрути role selection, ticket forms, lists, details |
| `RoleSelectionScreen` | вибір prototype role |
| `TylHomeScreen` | стартові дії тилового шлагбаума |
| `GreenTicketScreen` / `RedTicketScreen` | локальні prototype форми талона |
| `CheckpointListScreen` | список локальних записів за роллю |
| `TransportDetailsScreen` | detail та approve/reject дії |
| `DateUtils` | форматування prototype timestamp |

Папка `duty/` і частина `data/` містять ранні заготовки/legacy-класи. Також є
паралельні enum-моделі в `data/` і `domain/` та файл `TickeType.kt` з помилкою в
імені. Їх не слід механічно видаляти, доки Android використовується як UX
reference, але вони не повинні переноситися в production domain.

Android Room зберігає лише локальні prototype-дані й не є джерелом істини для
Angular/.NET. Зміни в `frontend/` або `backend/` не з’являються в Android
емуляторі.

## 12. Конфігурація і режими запуску

### Frontend mock mode

`DEFAULT_APP_CONFIG.adapterMode` зараз дорівнює `mock`. Можна перевіряти UI без
backend:

```sh
cd frontend
npm ci
npm start
```

Послідовність у браузері:

1. виконати mock sign-in;
2. вибрати КПП;
3. для vehicle entry вибрати «ВШ Тил Виїзд» або «ВШ Кордон В’їзд»;
4. відкрити «Оформити транспортний засіб»;
5. використати `AA1234BB`, щоб побачити mock є-Черги;
6. перевірити звичайний і службовий ТЗ, allow та deny.

### Backend mock mode

```sh
cd backend
dotnet restore ETicket.slnx --locked-mode
dotnet run --project src/ETicket.Api/ETicket.Api.csproj
```

Base configuration має `AdapterMode=Mock`, але `EnableMockSignIn=false`.
Development configuration вмикає demo sign-in. Не слід вмикати його у
production.

### API mode frontend

`ApiAuthAdapter`, `ApiOperatingContextGateway` і `ApiVehicleVisitGateway` вже
реалізовані. Однак повний локальний one-command Angular→API запуск ще потребує
узгодженого same-origin hosting або Angular dev proxy. Просте перемикання на
`api` без proxy/CORS/CSRF конфігурації не є завершеним deployment setup.

## 13. Persistence, audit і час

Зараз persistence process-local:

- context і visits зникають після restart backend;
- frontend mock session зникає після browser refresh;
- frontend mock context може відновитися з `localStorage`, але це лише UX
  convenience і не доказ authorization;
- audit events зберігаються в пам’яті;
- queue mock є детермінованим, а не реально синхронізованим.

Backend `IClock` задає authoritative UTC timestamps. Клієнтський час не
використовується як джерело істини для бізнес-подій.

При переході на SQL Server необхідні:

- EF Core або погоджений data-access підхід;
- migrations;
- row version/optimistic concurrency;
- unique constraint для `(CreatedByUserId, IdempotencyKey)`;
- transaction для visit + vehicle + queue snapshot + audit/outbox;
- індекси для пошуку, pagination і списків контролю/виїзду;
- retention, backup, restore і data-classification правила.

## 14. CI та локальні перевірки

### Angular

```sh
cd frontend
npm ci
npm run check
```

`npm run check` запускає Prettier check, Angular ESLint, Vitest і production
build.

### .NET

```sh
cd backend
dotnet restore ETicket.slnx --locked-mode
dotnet format ETicket.slnx --verify-no-changes --no-restore
dotnet build ETicket.slnx --configuration Release --no-restore
dotnet test ETicket.slnx --configuration Release --no-build
```

### Android

```sh
./gradlew build --stacktrace
```

Instrumented Android tests потребують emulator/device. Web UI слід перевіряти в
браузері, а не в Android emulator.

### GitHub Actions

- `android-ci.yml` — wrapper validation, JDK/SDK setup, Gradle build/test/lint;
- `target-platform-ci.yml` — Node 24 Angular quality gate і .NET 10 locked
  restore/format/build/test.

## 15. Функціональне покриття

| Модуль вимог | Стан | Коментар |
| --- | --- | --- |
| Авторизація | partial | secure session boundary і mock є; КЕП/MFA немає |
| RBAC / visibility zones | prototype implemented | backend policies і zones є; admin management немає |
| Режим оформлення | prototype implemented | checkpoint/gate selection і persistence port |
| Ручний в’їзд ТЗ | prototype implemented | form, validation, allow/deny, idempotency |
| Фото ТЗ | partial | лише reference field; capture/upload/storage немає |
| є-Черга при в’їзді | partial | port і deterministic lookup; real API/policy немає |
| Камера планшета | missing | Phase 5 |
| АРНЗ/VMS | model/port only | Phase 5 і Phase 13 |
| Огляд ТЗ | model/reasons only | Phase 6 |
| Прикордонний/митний контроль | model/ports only | Phase 7 і Phase 13 |
| Автоматичний status calculation | state foundation only | Phase 7 |
| Виїзд ТЗ | missing | Phase 8 |
| Пошук оформлених ТЗ | missing | Phase 9 |
| Pagination/load more | missing | Phase 9 |
| XLSX export | missing | Phase 9/11 |
| Повний модуль є-Черги | missing | Phase 10 |
| Статистика | filter foundation only | Phase 11 |
| Audit | partial | auth/context/entry events; усі майбутні actions ще не покриті |
| SQL Server | missing | production persistence phase |
| E2E/regression | missing | Phase 14 |

## 16. Відомі ризики та неоднозначності

### Product/spec ambiguities

1. Потрібно офіційно підтвердити семантику чотирьох gate modes і mapping
   `RearExit`/`BorderEntry` як точок приймання авто на територію ПП.
2. Не визначено повний перелік допустимих форматів українських та іноземних
   номерних знаків. Поточне правило навмисно загальне: 2–16 букв/цифр.
3. Не визначено, чи відсутність/статус є-Черги автоматично забороняє в’їзд.
   Phase 4 трактує lookup як інформаційний.
4. Не затверджено, які контролі є обов’язковими для кожного mode/category і як
   negative result веде до `Return`, `Cancelled` або іншого стану.
5. Не визначено правила редагування вже прийнятого рішення та reinspection.
6. Не погоджені протоколи КЕП/MFA, e-Queue, VMS, customs і border control.
7. Не погоджені storage/retention/privacy правила для фотографій і audit data.

### Технічні обмеження

- process-local repositories не переживають restart і не масштабуються на
  декілька API instances;
- ASP.NET Data Protection keys ще не мають production shared storage;
- немає local Angular dev proxy/full-stack launcher;
- немає cancellation UX для довгих HTTP calls;
- немає integration tests реальних adapters та database;
- автоматичний browser screenshot/e2e runner у поточному середовищі відсутній;
- responsive CSS зібрано, але потрібна ручна перевірка 1280×800 і 1920×1200;
- поточна git working tree містить незакомічені та untracked файли — перед
  наступним великим етапом потрібен review і контрольний commit.

## 17. Подальший порядок дій

| Порядок | Етап | Priority | Залежності | Основний результат |
| --- | --- | --- | --- | --- |
| 1 | контрольна точка після Phase 4 | P0 | поточні зміни | manual browser acceptance, review diff, commit/tag baseline |
| 2 | Phase 5 — Camera integrations | P1 | підтвердження mode semantics і photo policy | camera port, VMS events, 5-minute lifecycle, processed removal |
| 3 | Phase 6 — Vehicle Inspection | P1 | Phase 4 visits, reason catalogue | inspection queue, yes/no, required reason/other text, edit rules |
| 4 | Phase 7 — Control State Machine | P1 | Phase 6, control policy | ОТЗ/border/customs aggregation і automatic vehicle status |
| 5 | Phase 8 — Vehicle Exit | P1 | Phase 7 | waiting-exit list, eligibility validation, confirmation, return, idempotency |
| 6 | Phase 9 — Processed Vehicles | P1 | durable query model | filters, 50 + load more, view/edit/cancel, full-result XLSX |
| 7 | Phase 10 — Queue | P2 | e-Queue protocol | list/search/detail/history, pause/resume, sync status |
| 8 | Phase 11 — Statistics | P2 | SQL/query model | 180-day filters, KPIs, charts, longest-stay list, XLSX |
| 9 | Phase 12 — Audit/Security hardening | P0 before release | усі commands | complete audit coverage, session/key/file/export hardening |
| 10 | Phase 13 — Real integrations | P1 | approved protocols/secrets | production adapters, mapping, retries, timeouts, observability |
| 11 | Phase 14 — Testing | P0 before release | stable workflows | integration, E2E happy/negative paths, regression suite |
| 12 | Phase 15 — Release preparation | P0 before release | production infrastructure | SQL deployment, staging, runbooks, acceptance, release |

### Найближчі конкретні дії

1. В Android Studio або іншому Git UI переглянути всі Local Changes і не
   змішувати target-platform changes із випадковими Android edits.
2. Запустити frontend у mock mode та вручну пройти sign-in → context → vehicle
   entry при 1280×800.
3. Перевірити звичайний ТЗ, службовий ТЗ, `AA1234BB`, allow, deny та повторний
   click.
4. Запустити `npm run check`, .NET build/tests і Android build.
5. Зафіксувати Phase 0–4 окремим контрольним commit після review.
6. Отримати від product owner відповіді щодо gate direction, photo storage і
   VMS event lifecycle.
7. Після підтвердження починати лише Phase 5.

## 18. Рекомендований scope Phase 5

### Мета

Додати отримання номерного знака з камери планшета та від АРНЗ/VMS, не
вбудовуючи емуляцію зовнішньої системи безпосередньо у UI.

### Backend

- розширити `IVmsAdapter` для отримання recognition events за gate/mode;
- створити application service для list/select/mark-processed;
- використовувати `CameraRecognition` як внутрішню модель;
- додати repository/TTL abstraction для 5-хвилинного lifecycle;
- видаляти або позначати event processed атомарно з vehicle-entry command;
- додати permission, visibility, idempotency та audit;
- створити API endpoints і unit/integration tests.

### Frontend

- додати `CameraCapturePort` для browser MediaDevices, не змішувати його з VMS;
- додати список розпізнаних номерів, що залежить від current gate mode;
- дозволити перенести recognition plate/photo у `VehicleEntry` form;
- показати loading, empty, expired і error states;
- не створювати timer/subscription leaks;
- додати component/store tests і ручну landscape-перевірку.

### Definition of Done

- manual entry із Phase 4 не зламано;
- camera і VMS є різними ports/adapters;
- список фільтрується backend за current user/context;
- processed event не обробляється повторно;
- unprocessed event зникає приблизно через 5 хвилин;
- фото передається як безпечне reference, а не uncontrolled base64 у JSON;
- tests, lint, build, HTTP smoke і 1280×800 browser check проходять;
- правила та відкриті питання задокументовані.

## 19. Правила для подальшої розробки

- не додавати production business logic в Android prototype;
- не переносити random `if` зі screen/component у бізнес-процес;
- UI ніколи не є єдиним permission/validation barrier;
- не довіряти client-provided checkpoint, gate, mode, user, status або time;
- кожна критична command повинна мати server-side idempotency;
- external API payload проходить `External DTO → mapper → internal model`;
- mock arrays живуть в adapters, а не в components;
- не логувати credentials, КЕП material, session tokens або приватні ключі;
- кожний етап завершується format/lint/tests/build, security review,
  документацією і manual landscape acceptance;
- неоднозначні бізнес-правила не вигадувати: фіксувати питання product owner.

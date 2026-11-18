# Build and quality baseline

- Audit date: 2026-08-28
- Repository revision: `98ae77b`
- Gradle: 8.13
- Android Gradle Plugin: 8.13.2
- Kotlin: 2.0.21
- Build JVM used for verification: JetBrains Runtime 21.0.10
- Android compile/target SDK: 36

This baseline records the existing project state. It does not declare the
prototype production-ready and does not suppress the findings below.

## Reproducibility findings

Before stabilization, `gradlew` was stored with Git mode `100644`, so a standard
Linux invocation failed with `Permission denied`. Phase 0 changes the wrapper to
mode `100755`.

The local shell did not expose `java` or `JAVA_HOME`. The build succeeded after
using the JBR bundled with Android Studio. Machine-specific JDK paths must not be
committed; JDK 21 is now documented as a prerequisite and configured in CI.

## Verified commands

With JDK 21 and Android SDK 36 configured:

```sh
./gradlew assembleDebug testDebugUnitTest lintDebug --stacktrace
./gradlew build --stacktrace
```

Results:

| Check | Result | Evidence |
|---|---|---|
| Debug build | PASS | Debug APK produced |
| Release build | PASS | Unsigned release APK produced |
| Kotlin/Room/KSP compilation | PASS | No compile errors |
| JVM unit tests | PASS | 1 template test, 0 failures |
| Android lint | PASS WITH WARNINGS | 0 errors, 60 warnings |
| Instrumented tests | NOT RUN | No emulator or device attached |
| Runtime/landscape verification | NOT RUN | No emulator or device attached |

Approximate artifacts from the audited build:

- debug APK: 11 MB;
- unsigned release APK: 7.2 MB.

## Lint warning baseline

| Rule | Count |
|---|---:|
| HardcodedText | 23 |
| UnusedResources | 14 |
| Autofill | 7 |
| TextFields | 5 |
| GradleDependency | 4 |
| NewerVersionAvailable | 2 |
| ButtonStyle | 2 |
| AndroidGradlePluginVersion | 2 |
| RedundantLabel | 1 |

Most UI resource warnings originate from legacy XML layouts that are not used by
the active Compose application. They should be removed only after confirming that
Compose is the sole Android prototype UI path.

Dependency update warnings are informational. Versions must not be upgraded in a
bulk mechanical change; each compatible group should be updated and verified
independently.

## Test baseline limitations

The only JVM test checks that `2 + 2 == 4`. The instrumentation test checks the
application package name. There are no tests for validation, state transitions,
permissions, persistence, navigation, error handling, or duplicate commands.

## Dependency security check

An OSV query of eight declared runtime/test artifacts returned no known
vulnerabilities on the audit date. This is not a complete software composition
analysis because the repository has no dependency lockfile, SBOM, or automated
transitive dependency scanner.

## Phase 0 quality gate

The stabilization gate is:

1. A fresh Linux checkout can execute `./gradlew build`.
2. GitHub CI runs the same full build on pushes and pull requests.
3. Android lint has zero errors; the 60 existing warnings are visible and must
   not be hidden.
4. New production code must add relevant tests instead of relying on template
   tests.
5. Instrumented and 1280x800 landscape verification remain required before any
   UI release.
6. ADR-0001 must be accepted before Phase 1 begins.

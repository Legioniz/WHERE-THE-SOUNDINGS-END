# Development Log

> Project: WHERE THE SOUNDINGS END  
> Status: Active working log  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-11

## 2026-11-11 — M0 implementation audit and Greyhaven compile-risk correction

**Commit scope:** Audit the current repository foundation, correct a confirmed source-level compile blocker, and establish the ordered vertical-slice backlog.

### Implemented / recorded
- Audited the tracked Git tree, Unity/package configuration, runtime owners, generated-scene bootstrap, existing tests, CI workflow, open dependency PRs and repository protection limitations.
- Found an undeclared `trim` argument in the Inez Sarmiento call inside `GreyhavenEnvironmentBuilder.Build`. Replaced it with the already-declared `concrete` material in commit [`06a7004`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/06a700406bad809e6671af26163e147dbd309811).
- Recorded the current implementation boundary and M0 exit criteria in `IMPLEMENTATION-AUDIT.md`.
- Replaced the Task Backlog placeholder with P0 foundation tasks and a scoped 45–90 minute Greyhaven vertical-slice backlog.

### Verification and limits
- The pre-correction main baseline had a successful GitHub Actions run for repository/configuration validation, C# syntax parsing and repository safeguard self-tests.
- The source correction is not considered Unity-verified until Unity `6000.3.28f1` imports and compiles the project.
- Seven Edit Mode test files are present (59 `[Test]` attributes and four `[TestCase]` attributes); Unity Test Runner execution remains pending.
- No Play Mode test scripts, committed generated scenes, package lock, or tracked Unity `.meta` files were present in the audited tree.
- Play Mode, Windows build, profiling and manual gameplay remain unverified.

### Compatibility and risk
- No save schema, stable content ID, input action name or package version changed.
- M0 remains in progress. A green syntax parser does not prove symbol resolution or Unity runtime behavior.
- Remote branch ruleset enforcement remains unavailable/unverified for the current private-repository plan; no visibility or plan setting was changed.

## 2026-11-11 — Movement-aware ocean camera

**Commit scope:** Upgrade the Greyhaven camera from a position-only fixed-angle follower to a velocity-aware, sea-responsive orthographic camera.

### Implemented
- Added horizontal Rigidbody velocity look-ahead with a hard distance cap. Reverse and drift frame in the actual travel direction.
- Added subtle yaw-rate camera banking with clamped angles and damped rotation.
- Added shared-wave sampling for camera heave, pitch, and roll. The camera samples the same MaritimeSurfaceBehaviour used by dead-calm and wrong-way-swell incidents.
- Made the Greyhaven Editor builder create the surface explicitly and wire it into the camera, while retaining the atmosphere director's existing incident ownership.
- Added Edit Mode tests for movement look-ahead, stationary behaviour, clamping, invalid tuning, and turn-bank direction.
- Updated camera awareness and technical design documentation, plus the root implementation summary.

### Verification
- Repository source and current scene-generation path were inspected before editing.
- GitHub Actions static project and C# syntax validation will run on the push.
- Unity 6000.3.28f1 compilation, Edit Mode test execution, Play Mode feel/readability checks, reduced-motion review, and Windows build remain unverified in this environment.

### Compatibility and risk
- No save schema or persisted identifiers changed.
- Orthographic projection, camera pitch, initial offset, boat Rigidbody ownership, action-driven sea clock, and atmosphere incident scheduling remain unchanged.
- Wave response is a presentation effect only; it does not imply physical buoyancy or wave forces on the prototype boat.

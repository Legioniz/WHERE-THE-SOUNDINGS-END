# Development Log

> Project: WHERE THE SOUNDINGS END  
> Status: Active working log  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-12

## 2026-11-12 — Vessel collision and impact response

**Commit:** Pending CI verification.

### Implemented
- Replaced total relative-speed damage with contact-normal impact speed, so tangential scraping does not count as a full-speed collision.
- Coalesced the strongest collision contact during a physics step and processed one damage event, reducing duplicate damage from multiple contact reports.
- Removed manual post-impact linear/angular velocity multipliers; Unity's Rigidbody solver now owns physical collision response.
- Added NaN/infinity guards, a deterministic capped hull-damage curve, proportional engine damage and existing local cargo-cell damage.
- Published accepted damaging impacts through the existing acoustic event bus as `HullImpact` signals with region and game-time context.
- Replaced Greyhaven's overlapping visual-mesh colliders with one root-level hull BoxCollider. Deep-water survey boats retain one root collider.
- Added deterministic Edit Mode regression tests, static validator checks and Play Mode acceptance criteria.

### Verification
- Current source and generated-scene collider setup were inspected before editing.
- GitHub Actions static project, C# syntax and repository safeguard checks will run after push.
- Unity 6000.3.28f1 semantic compilation, Edit Mode execution, Play Mode collision feel/contact geometry, acoustic propagation and Windows build remain unverified.

### Compatibility and risk
- No save schema, persisted ID, input action name or package version changed.
- Existing vessel hull/engine persistence and protected quest-cargo behavior remain the owning data contracts.
- The simple box hull and damage curve are gameplay approximations. Validate pier approach, rocks, side scrapes, hard head-on impacts and cargo loss in Play Mode.

## 2026-11-11 — Boat, buoyancy, waves and camera integration

**Commit:** [`1c58d63`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/1c58d63aa8bbe3d4ddb99588f2ae27768c18ce53)

### Implemented
- Replaced absolute-time wave phase calculation with a continuous phase accumulator. Wrong-way swell eases the phase rate through reversal and blends the travel direction instead of instantly switching the wave pattern.
- Changed water vertical-velocity sampling to compare the previous and current complete wave states. Buoyancy damping now sees height changes caused by phase, direction and amplitude transitions.
- Initialized the generated mesh from the exact same initial wave state used by surface sampling, avoiding a flat first-frame mesh/query mismatch.
- Applied the configured 0.65 m maximum-submersion limit to each buoyancy probe.
- Removed the camera's independent wave-height bob. The camera now follows the vessel's actual buoyancy-driven heave and adds only a restrained surface-normal tilt, avoiding double-counted motion.
- Passed the active water-surface reference to the Outer Shoals and Hollow Trench survey cameras as well as Greyhaven.
- Made BoatPrototypeController resolve VesselConditionController lazily because the generated-scene builders attach the condition component after the controller's Awake method.
- Added seven deterministic Edit Mode tests for continuous wave phase, blended direction, amplitude-aware surface velocity, invalid time input and buoyancy depth clamping.
- Extended the static project validator and updated the boat/camera specifications.

### Verification
- Source changes and generated-scene wiring were inspected in GitHub.
- The repository integrity workflow will validate required files and C# syntax on this push.
- Unity 6000.3.28f1 semantic compilation, Edit Mode execution, Play Mode handling/feel, profiling and Windows build remain unverified. The new Edit Mode tests are authored but have not been run in Unity.

### Compatibility and risk
- No save schema, persisted ID, input action name or package version changed.
- Boat and camera remain separate owners: boat Rigidbody owns movement; the surface owns wave state; camera owns presentation only.
- Final handling values still require Play Mode observation. Do not tune by visual intuition alone.

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

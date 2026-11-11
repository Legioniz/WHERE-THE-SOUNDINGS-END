# Implementation Audit — M0 Foundation Baseline

> Project: WHERE THE SOUNDINGS END  
> Status: M0 in progress — source audit recorded; Unity verification pending  
> Audit date: 2026-11-12  
> Audited baseline: `main` at `3f882eb` (before the fixed-step cadence patch in this review)  
> Source correction: `06a7004`  
> Owner: Project lead

## 1. Executive summary

The repository is a **connected Unity gameplay prototype**, not a directly playable or commercially complete game. It has meaningful foundations for boat handling, ocean presentation, action-driven time, harvesting, cargo, the Sounding Rig, acoustic events, Greyhaven NPCs, and save/autosave.

The most important M0 result is a confirmed source-level C# compilation defect in the Greyhaven scene builder. The builder passed an undeclared `trim` identifier when creating Inez Sarmiento. It has been corrected to use the existing `concrete` material in commit [`06a7004`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/06a700406bad809e6671af26163e147dbd309811). This is a source correction, **not proof of Unity compilation**. The next gate is a clean import and compile in Unity `6000.3.28f1`.

M0 is not complete. The repository's latest verified CI baseline was green for repository/configuration checks and C# syntax parsing. The boat/wave/camera source integration has since been updated in commit `1c58d63`; the resulting CI run and the new test-file inventory must be checked separately. These checks do not resolve C# symbols against Unity and package assemblies, run Edit Mode tests, execute Play Mode, or build Windows.

## 2. Repository snapshot

The audited Git tree contained 337 entries (292 blobs and 45 trees). The current technical baseline is:

| Area | Observed baseline |
|---|---|
| Engine | Unity 6.3 LTS — `6000.3.28f1` |
| Render pipeline | URP 17.3.0 |
| Language | C# |
| Input | Input System 1.20.1 |
| Dialogue | Yarn Spinner Unity 3.2.8 |
| Test framework | Unity Test Framework 1.4.6 |
| Runtime/editor structure | Separate runtime and Editor assembly definitions |
| Automated tests | Eight Edit Mode test source files (seven existing suites plus new wave-integration math coverage) |
| CI | Repository/configuration validator, C# syntax parser, repository safeguard validator |
| Initial target | Windows 64-bit, single-player |

The repository is currently code-and-documentation-led. At the audited revision, the tracked tree did **not** contain Unity scene files (`.unity`), prefabs, serialized materials/data assets, `.meta` files, `Packages/packages-lock.json`, or `ProjectSettings/EditorBuildSettings.asset`. The Editor bootstrap creates folders, rendering assets, generated materials/cargo definitions, the Greyhaven blockout, and the deep-ocean survey scenes when the project is initialized.

This generated-scene approach is documented, but its clean-checkout behavior still needs to be verified. It also means a fresh clone is not yet a ready-to-play scene/build until the Unity Editor bootstrap has run successfully.

## 3. Implementation inventory

Status terms are deliberately strict: **implemented** means source/data is present and connected; **partial** means a prototype exists with material gaps; **unverified** means runtime behavior has not been executed in the target Unity editor.

| System | Current status | Evidence and boundary |
|---|---|---|
| Boat propulsion and handling | Implemented foundation; unverified | Rigidbody forces/torque, forward/reverse thrust, opposing-throttle braking, speed taper, rudder flow, lateral resistance and drag. |
| Buoyancy and ocean surface | Implemented approximation; unverified | Four hull probes sample the same analytic wave function used by the generated surface. It is not a full fluid/current simulation. |
| Follow camera | Implemented foundation; unverified | Velocity look-ahead, clamped turning bank, and shared-surface pitch/roll response. |
| Sea-time and atmosphere | Implemented foundation; unverified | Time advances with actual vessel movement or supported harvesting/survey actions; pause/dialogue holds it. Weather, panic and maritime incidents are connected. |
| Fishing and dredging | Partial prototype; unverified | Timed harvesting minigames, catalogue-based catch selection, salvage-site depletion and cargo placement exist. Catch selection is not yet a region/habitat/population simulation. |
| Cargo and vessel condition | Partial prototype; unverified | Spatial cargo placement, blocked/damaged cells, impact damage, and hull/engine condition exist. A complete service/economy loop is absent. |
| Sounding Rig and acoustic signals | Implemented foundation; unverified | Regional survey profiles, signal confidence, repeated-ping verification, authored site markers, acoustic event publishing/listening and save participation exist. |
| Greyhaven NPCs | Partial foundation; unverified | Four residents, stable IDs, time-based schedules, waypoint movement, Yarn dialogue and relationship persistence exist. Obstacle-aware navigation and quest/world-state progression remain incomplete. |
| Persistence | Implemented runtime foundation; integration incomplete | Five manual JSON slots, a separate autosave, backups, schema migrations and save participants exist. No complete player-facing save/load screen or automatic cross-scene restore flow exists. |
| Player interface | Partial foundation; unverified | A UI Toolkit dialogue presenter and spatial cargo inventory are present, alongside prototype status/interaction displays. Main menu, settings, map/chart, market and full save UI remain absent. |
| Creature AI and living population | Planned / not established in runtime | The repository has design documents, but no complete creature perception/behavior or population simulation runtime. |
| Economy, quests and regional travel | Planned / partial design | No complete sell/repair/fuel/upgrade loop, quest campaign, or interconnected regional travel system is established in runtime. |
| Environments and assets | Generated greybox only | The scene builders construct blockout geometry and placeholder materials. Final art/audio content and committed scene outputs are not present in the audited tree. |

## 4. Findings and actions

| ID | Priority | Finding | Impact / required action | State |
|---|---|---|---|---|
| AUD-01 | P0 | Greyhaven builder referenced an undeclared `trim` variable in Inez's resident setup. | Would fail semantic C# compilation despite passing syntax parsing. Replaced the argument with the declared `concrete` material. Confirm with Unity compiler. | Source corrected; Unity verification pending |
| AUD-02 | P0 | A clean repository checkout has no committed generated scenes, build-scene list, `.meta` files or package lock. | First import must prove the Editor bootstrap works and the project resolves reproducibly. Generate/commit stable Unity metadata and the resolved package lock; decide whether generated scenes are committed or regenerated as a documented build step. | Open |
| AUD-03 | P0 | CI does not compile Unity assemblies or execute Unity tests. | Keep static checks, but require a target-editor import/compile, Edit Mode run, Play Mode/integration run and Windows build before M0 exit. Do not label a passing syntax job as a successful Unity build. | Open |
| AUD-04 | P0 | Edit Mode tests are authored but have not been executed in the target editor. | Eight Edit Mode test files provide deterministic coverage, including the new fixed-step surface-velocity and wave-render interpolation regressions. Run the suite in Unity `6000.3.28f1` and record results. | Open |
| AUD-05 | P0 | No Play Mode test scripts or Play Mode test assembly are tracked. | Add a small integration suite for boat/water/camera, input, action-driven time, harvesting, NPC/dialogue pause behavior and save lifecycle. | Open |
| AUD-06 | P0 | Boat, water and camera are connected in source but have no observed runtime acceptance evidence. | The source integration now includes continuous wave transitions, amplitude-aware velocity, capped probe depth, single-source camera heave, shared-surface camera wiring, and fixed-step wave snapshots before buoyancy with render-only interpolation. Still validate stable draft, wave response, frame-rate independence, stopping distance, turning/reverse behavior, camera framing, drift look-ahead and sea-clock interaction in Unity. | Source pass committed; Unity acceptance open |
| AUD-07 | P0 | Save participants are discovered without a declared restore ordering contract. | Exercise full capture/restore with boat, world clock, cargo, vessel condition, NPC and Yarn data. If any participant depends on another, introduce explicit restore phases/order and regression tests. | Open |
| AUD-08 | P1 | The repository is private and the GitHub rulesets endpoint reports that rulesets require GitHub Pro or a public repository. | Required review/status-check enforcement is not established by the committed files alone. Preserve PR discipline and decide separately whether to change plan or visibility; do not change repository visibility automatically. | Blocked by account/repository plan choice |
| AUD-09 | P1 | Fishing resolves from a catalogue rather than regional availability. | For the slice, constrain catches to Greyhaven's authored species/conditions; later connect habitat, time, disturbance and population rules through the existing design contracts. | Open |
| AUD-10 | P1 | The water mesh updates 4,225 vertices and recalculates normals/bounds each frame. | Profile on target hardware before adding more water surfaces or dense simulation. Optimize only from measured results. | Open |
| AUD-11 | P1 | Two Dependabot pull requests are open for major GitHub Actions updates. | Review workflow compatibility and both PR checks before merging. Keep action references pinned to full commit SHAs. | Open |
| AUD-12 | P0 | Wave state advanced in render-frame `Update`, while buoyancy sampled it in `FixedUpdate`; vertical water velocity was divided by render-frame delta rather than the physics interval. | Advance and snapshot wave state in an early-order `FixedUpdate` using `Time.fixedDeltaTime`; interpolate only the visible mesh in `LateUpdate`; add deterministic interpolation tests and static cadence guards. | Source corrected in this review; Unity acceptance open |
| AUD-13 | P0 | Collision damage used total relative speed and manually damped Rigidbody velocity after the solver response; Greyhaven visual colliders could report overlapping contacts. | Use contact-normal impact speed, aggregate same-step contacts, preserve solver-owned velocity, consolidate Greyhaven to one root collision hull, publish accepted impacts to the acoustic bus and add regression coverage. | Source corrected in this review; Unity acceptance open |

### CI evidence

The last completed pre-patch `main` run was [run 43038724085](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/actions/runs/43038724085) on SHA `3f882eb`; both jobs passed. The fixed-step cadence patch at [`6543cc7`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/6543cc78561f2d0eab1d89dd9b95804555a3bd63) then passed [run 43067808872](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/actions/runs/43067808872). Both jobs completed successfully: **Repository safeguards** and **Validate Unity project and C# syntax**. Within the validation job, project integrity and C# syntax checks both passed.

These jobs run repository/configuration validation, Tree-sitter C# syntax parsing and the repository safeguard self-test. The new `MaritimeSurfaceMathTests` cases are present and required by the validator, but remain unexecuted until Unity Test Runner is used. CI does not perform Unity semantic compilation, execute the NUnit tests, open generated scenes, run Play Mode, profile performance, or create a Windows player.

## Review update — 2026-11-12

The current source review confirmed that the bounded water mesh is intentionally scaled by each generated scene: Greyhaven uses a 29×27 transform scale and the survey sandboxes use a 220×220 scale. The local ±5 sampling boundary therefore matches the mesh footprint; no ocean-coverage expansion was justified.

The actual integration issue was temporal: render-frame wave updates were being sampled by fixed-step Rigidbody buoyancy. This review moves wave-state advancement to an early-order fixed step, computes vertical surface velocity over `Time.fixedDeltaTime`, and interpolates only the water mesh between physics snapshots. Deterministic tests and the repository validator now cover this contract.

The code-level fix does not close M0-06. Unity compilation, Edit Mode execution, Play Mode checks at 30/60/120 FPS, a complete Greyhaven expedition, and a Windows build are still outstanding. Static CI success must not be presented as gameplay verification.

## Collision review update — 2026-11-12

The collision pass extends the existing `VesselConditionController`. Damage now uses contact-normal speed, so a fast tangential scrape is not treated like a head-on strike. Contact reports are coalesced per physics step, and the component no longer applies arbitrary post-collision velocity multipliers. Accepted damaging impacts publish the existing acoustic `HullImpact` signal. Greyhaven now uses one root-level hull BoxCollider while visible hull/bow meshes are non-colliding.

Deterministic Edit Mode cases and static repository guards have been added. This is source-level verification only. Unity compilation, Test Runner execution, physical separation/contact behavior, acoustic propagation, cargo-loss balance and Windows build remain open.

## 5. M0 exit criteria

M0 remains **in progress** until all of the following are evidenced:

1. A fresh clone opens in Unity `6000.3.28f1` and resolves the pinned packages without manual repair.
2. The resolved package lock and required Unity `.meta` files are committed; stable asset GUIDs survive a second clean checkout.
3. `Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation` completes without compile/import errors and generates the expected scenes/build list. Re-running setup is safe and repeatable.
4. Unity reports zero C# compiler errors and no missing scripts in the generated scenes.
5. All existing Edit Mode tests pass in the target editor.
6. The new Play Mode smoke tests pass for boat, water, camera, sea-time, harvesting, NPC interaction and save/restore.
7. Manual Greyhaven testing confirms the boat/camera/ocean foundation is stable and understandable.
8. A clean Windows 64-bit development build launches into Greyhaven.
9. Known remote repository protection limitations are recorded and an explicit owner decision is made.

## 6. Immediate production direction

Do not expand into all six regions or the full creature catalog yet. The first vertical slice should validate one complete 45–90 minute Greyhaven experience: prepare, depart, navigate, fish/survey/salvage, encounter one authored threat, return, sell/repair, talk to one resident, progress one quest, save and resume.

The authoritative work sequence and acceptance criteria are in [Task Backlog](../13_PRODUCTION_MANAGEMENT/TASK-BACKLOG.md). The commercial roadmap remains an estimate and should be reforecast after the slice is playable.

## 7. Related documents

- [Task Backlog](../13_PRODUCTION_MANAGEMENT/TASK-BACKLOG.md)
- [Development Roadmap](../13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md)
- [Project Milestones](../13_PRODUCTION_MANAGEMENT/PROJECT-MILESTONES.md)
- [Feature Priorities](../13_PRODUCTION_MANAGEMENT/FEATURE-PRIORITIES.md)
- [Testing Strategy](TESTING-STRATEGY.md)
- [C# Compile-Risk Audit](CSHARP-COMPILE-RISK-AUDIT.md)
- [Repository Safeguards](REPOSITORY-SAFEGUARDS.md)

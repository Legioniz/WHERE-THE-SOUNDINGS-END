# Implementation Audit — M0 Foundation Baseline

> Project: WHERE THE SOUNDINGS END  
> Status: M0 in progress — source audit recorded; Unity verification pending  
> Audit date: 2026-11-13  
> Audited baseline: `main` at `c1ff1975` after the dockside modal-ownership reconciliation  
> Source correction: `06a7004`  
> Owner: Project lead

## 1. Executive summary

The repository is a **connected Unity gameplay prototype**, not a directly playable or commercially complete game. It has meaningful foundations for boat handling, ocean presentation, action-driven time, harvesting, cargo, the Sounding Rig, acoustic events, Greyhaven NPCs, and save/autosave.

The most important M0 result remains the need to validate the generated Unity project in the pinned editor. Earlier source-level C# defects in the Greyhaven scene builder were corrected and repository CI now parses all C# sources. The builder passed an undeclared `trim` identifier when creating Inez Sarmiento. It has been corrected to use the existing `concrete` material in commit [`06a7004`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/06a700406bad809e6671af26163e147dbd309811). This is a source correction, **not proof of Unity compilation**. The next gate is a clean import and compile in Unity `6000.3.28f1`.

M0 is not complete. The repository's latest verified CI baseline was green for repository/configuration checks and C# syntax parsing. The boat/wave/camera source integration has since been updated in commit `1c58d63`; the resulting CI run and the new test-file inventory must be checked separately. These checks do not resolve C# symbols against Unity and package assemblies, run Edit Mode tests, execute Play Mode, or build Windows.

## 2. Repository snapshot

The audited main snapshot contains 315 tracked files at base SHA c1ff1975. The current technical baseline is:

| Area | Observed baseline |
|---|---|
| Engine | Unity 6.3 LTS — `6000.3.28f1` |
| Render pipeline | URP 17.3.0 |
| Language | C# |
| Input | Input System 1.20.1 |
| Dialogue | Yarn Spinner Unity 3.2.8 |
| Test framework | Unity Test Framework 1.4.6 |
| Runtime/editor structure | Separate runtime and Editor assembly definitions |
| Automated tests | Thirteen Edit Mode test source files and one Play Mode transition test source, in separate assemblies; Unity execution pending |
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
| Fishing and dredging | Connected source foundation; Unity unverified | Multi-stage tackle/technique encounters, regional species eligibility, habitat/depth profiles, aggregate stock depletion/recovery/migration, catch provenance, seaweed gathering and separate salvage reservations are connected. Habitat/depth resolution and population migration remain prototype approximations; Unity input, encounter balance and save/load acceptance are open. |
| Cargo and vessel condition | Connected source foundation; Unity unverified | Spatial cargo, catch sale quantities/provenance, equipment slots and durability, hull/engine/hold damage, fuel, repair services, wallet transactions and market persistence are connected. Full expedition balance and in-Editor transaction/save validation remain open. |
| Sounding Rig and acoustic signals | Implemented foundation; unverified | Regional survey profiles, signal confidence, repeated-ping verification, authored site markers, acoustic event publishing/listening and save participation exist. |
| Greyhaven NPCs | Partial foundation; unverified | Four residents, stable IDs, time-based schedules, waypoint movement, Yarn dialogue and relationship persistence exist. A persistent dockside hub routes the four resident conversations through existing Yarn nodes and relationship gates. Obstacle-aware navigation and quest/world-state progression remain incomplete. |
| Persistence | Implemented runtime foundation; integration incomplete | Five manual JSON slots, a separate autosave, backups, schema v9 migrations, save participants and a title/pause save/load UI exist. Historical artifact inspection now has its own ledger; automatic cross-scene restore and complete campaign resume flow remain unverified/incomplete. |
| Player interface | Connected prototype; Unity UX unverified | UI Toolkit field HUD, cargo inventory, dialogue/relationship meter, title/pause/save/load/settings, regional discovery chart, seven-location dockside hub and source-integrated market/service/contract/research panels exist. Full quest UI, complete controller navigation, responsive usability and accessibility acceptance remain open. |
| Creature AI and living population | Planned / not established in runtime | The repository has design documents, but no complete creature perception/behavior or population simulation runtime. |
| Economy | Connected source implementation; physical destination access and Unity validation open | The generated catalogue has 155 scheduled seafood records. Regional/habitat eligibility, catch legality and persisted provenance, harvest permits, destination-specific market factors and per-region stock, atomic sales/purchases, fuel, repairs, equipment, contracts, research and up-to-50-sample expedition telemetry are wired in source. Greyhaven is the only generated physical market; other destination terminals, Unity verification and measured balance playtests remain open. |
| Historical archive | Six Greyhaven records implemented in source; Unity unverified | Stable map, tool, letter, logbook, photograph and survey-equipment anchors use dockside/direct inspection and a separate schema-v9 ledger. The five non-Greyhaven families remain proposals. |
| Quests and regional travel | Planned / partial design | No complete quest campaign or interconnected regional travel system is established in runtime. |
| Environments and assets | Generated greybox; area streaming source implemented; Editor preflight updated | Editor builders construct Greyhaven terrain, services, maritime props, six static additive areas and deep-water sandbox scenes. Generated scenes/build settings/package lock and broad stable .meta coverage are still not committed; clean import, rebuild, visual review and streaming execution remain open. |

## 4. Findings and actions

| ID | Priority | Finding | Impact / required action | State |
|---|---|---|---|---|
| AUD-01 | P0 | Greyhaven builder referenced an undeclared `trim` variable in Inez's resident setup. | Would fail semantic C# compilation despite passing syntax parsing. Replaced the argument with the declared `concrete` material. Confirm with Unity compiler. | Source corrected; Unity verification pending |
| AUD-02 | P0 | A clean repository checkout has no committed generated scenes, build-scene list, `.meta` files or package lock. | First import must prove the Editor bootstrap works and the project resolves reproducibly. Generate/commit stable Unity metadata and the resolved package lock; decide whether generated scenes are committed or regenerated as a documented build step. | Open |
| AUD-03 | P0 | CI does not compile Unity assemblies or execute Unity tests. | Keep static checks, but require a target-editor import/compile, Edit Mode run, Play Mode/integration run and Windows build before M0 exit. Do not label a passing syntax job as a successful Unity build. | Open |
| AUD-04 | P0 | Edit Mode tests are authored but have not been executed in the target editor. | Thirteen Edit Mode test source files now cover boat physics, streaming/access policies, modal exclusion, terrain, NPC, save, cargo and surface math. Run the full suite in Unity `6000.3.28f1` and record results. | Open |
| AUD-05 | P0 | Play Mode coverage is still narrow and has not been executed. | A Play Mode assembly and Greyhaven additive-area transition test are now authored. Expand coverage to boat/water/camera, input, action-driven time, harvesting, dockside modal transitions, NPC/dialogue and save lifecycle; run in Unity. | Source added; Unity execution open |
| AUD-06 | P0 | Boat, water and camera are connected in source but have no observed runtime acceptance evidence. | The source integration now includes continuous wave transitions, amplitude-aware velocity, capped probe depth, single-source camera heave, shared-surface camera wiring, and fixed-step wave snapshots before buoyancy with render-only interpolation. Still validate stable draft, wave response, frame-rate independence, stopping distance, turning/reverse behavior, camera framing, drift look-ahead and sea-clock interaction in Unity. | Source pass committed; Unity acceptance open |
| AUD-07 | P0 | Save participants are discovered without a declared restore ordering contract. | Exercise full capture/restore with boat, world clock, cargo, vessel condition, NPC and Yarn data. If any participant depends on another, introduce explicit restore phases/order and regression tests. | Open |
| AUD-08 | P1 | The repository is private and the GitHub rulesets endpoint reports that rulesets require GitHub Pro or a public repository. | Required review/status-check enforcement is not established by the committed files alone. Preserve PR discipline and decide separately whether to change plan or visibility; do not change repository visibility automatically. | Blocked by account/repository plan choice |
| AUD-09 | P1 | Fishing resolves from a catalogue rather than regional availability. | For the slice, constrain catches to Greyhaven's authored species/conditions; later connect habitat, time, disturbance and population rules through the existing design contracts. | Open |
| AUD-10 | P1 | The water mesh updates 4,225 vertices and recalculates normals/bounds each frame. | Profile on target hardware before adding more water surfaces or dense simulation. Optimize only from measured results. | Open |
| AUD-11 | P1 | Two Dependabot pull requests are open for major GitHub Actions updates. | Review workflow compatibility and both PR checks before merging. Keep action references pinned to full commit SHAs. | Open |
| AUD-12 | P0 | Wave state advanced in render-frame `Update`, while buoyancy sampled it in `FixedUpdate`; vertical water velocity was divided by render-frame delta rather than the physics interval. | Advance and snapshot wave state in an early-order `FixedUpdate` using `Time.fixedDeltaTime`; interpolate only the visible mesh in `LateUpdate`; add deterministic interpolation tests and static cadence guards. | Source corrected in this review; Unity acceptance open |
| AUD-13 | P0 | Collision damage used total relative speed and manually damped Rigidbody velocity after the solver response; Greyhaven visual colliders could report overlapping contacts. | Use contact-normal impact speed, aggregate same-step contacts, preserve solver-owned velocity, consolidate Greyhaven to one root collision hull, publish accepted impacts to the acoustic bus and add regression coverage. | Source corrected in this review; Unity acceptance open |

| AUD-14 | P0 | Dockside, inventory and front-end pause overlays had independent pause/input ownership, allowing overlapping panels and competing Time.timeScale restoration. | Added DocksideModalPolicy and runtime gates across DocksideHubController, GreyhavenRuntimeUI and GreyhavenFrontEndUI; added deterministic Edit Mode regressions. Verify keyboard/gamepad event ordering and pause restoration in Unity. | Source corrected; Unity verification pending |
| AUD-15 | P1 | Generated resident trigger components could still enable/disable the shared Interact action even though the dockside hub owns resident selection. | Externally managed HarbourmasterInteraction instances now skip shared-action ownership. Verify the hub remains responsive when residents are enabled/disabled or scenes are rebuilt. | Source corrected; Unity verification pending |

## Follow-up reconciliation — 2026-11-13

The current source now includes a persistent seven-location Greyhaven dockside hub, furniture inspection actions, existing resident Yarn handoff, cargo/vessel diagnostics, and an Inn checkpoint through the existing autosave. The hub is a menu presentation, not walkable 3D interiors. Six additive static Greyhaven area scenes and transition telemetry are also authored; the streaming Play Mode test remains unexecuted.

A second source pass introduced shared modal policy across dockside, inventory, dialogue and front-end overlays, including gamepad Start/Esc close behavior and generated resident input ownership. The corresponding tests and validator guards are authored. PR #8 CI run #274, PR #9 CI run #275 and PR #10 CI run #279 passed repository integrity, C# syntax and safeguard jobs. PR #10 also corrected a false-positive validator expectation and reran both jobs successfully. These are static checks only; Unity import/semantic compilation, Edit Mode execution, Play Mode, scene regeneration, hardware profiling and Windows build remain open.

### CI evidence

The last completed pre-patch `main` run was [run 43038724085](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/actions/runs/43038724085) on SHA `3f882eb`; both jobs passed. The fixed-step cadence patch at [`6543cc7`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/6543cc78561f2d0eab1d89dd9b95804555a3bd63) then passed [run 43067808872](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/actions/runs/43067808872). Both jobs completed successfully: **Repository safeguards** and **Validate Unity project and C# syntax**. Within the validation job, project integrity and C# syntax checks both passed.

These jobs run repository/configuration validation, Tree-sitter C# syntax parsing and the repository safeguard self-test. The new `MaritimeSurfaceMathTests` cases are present and required by the validator, but remain unexecuted until Unity Test Runner is used. CI does not perform Unity semantic compilation, execute the NUnit tests, open generated scenes, run Play Mode, profile performance, or create a Windows player.

## Review update — 2026-11-12

The current source review confirmed that the bounded water mesh is intentionally scaled by each generated scene: Greyhaven uses a 29×27 transform scale and the survey sandboxes use a 220×220 scale. The local ±5 sampling boundary therefore matches the mesh footprint; no ocean-coverage expansion was justified.

The actual integration issue was temporal: render-frame wave updates were being sampled by fixed-step Rigidbody buoyancy. This review moves wave-state advancement to an early-order fixed step, computes vertical surface velocity over `Time.fixedDeltaTime`, and interpolates only the water mesh between physics snapshots. Deterministic tests and the repository validator now cover this contract.

The code-level fix does not close M0-06. Unity compilation, Edit Mode execution, Play Mode checks at 30/60/120 FPS, a complete Greyhaven expedition, and a Windows build are still outstanding. Static CI success must not be presented as gameplay verification.

## Collision review update — 2026-11-12

The collision pass extends the existing `VesselConditionController`. Damage now uses contact-point relative velocity (including angular point velocity) projected onto the contact normal, so a fast tangential scrape is not treated like a head-on strike while rotation-driven bow/stern impacts are still measured. Contact reports are coalesced per physics step, zero impact-damage tuning cleanly disables damage, and the component no longer applies arbitrary post-collision velocity multipliers. Accepted damaging impacts publish the existing acoustic `HullImpact` signal. Greyhaven now uses one root-level hull BoxCollider while visible hull/bow meshes are non-colliding.

Deterministic Edit Mode cases and static repository guards have been added. This is source-level verification only. Unity compilation, Test Runner execution, physical separation/contact behavior, acoustic propagation, cargo-loss balance and Windows build remain open.

## 5. M0 exit criteria

M0 remains **in progress** until all of the following are evidenced:

1. A fresh clone opens in Unity `6000.3.28f1` and resolves the pinned packages without manual repair.
2. The resolved package lock and required Unity `.meta` files are committed; stable asset GUIDs survive a second clean checkout.
3. `Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation` completes without compile/import errors and generates the expected scenes/build list. Re-running setup is safe and repeatable.
4. Unity reports zero C# compiler errors and no missing scripts in the generated scenes.
5. All existing Edit Mode tests pass in the target editor.
6. The new Play Mode smoke tests pass for boat, water, camera, sea-time, harvesting, NPC interaction, dockside modal transitions, streaming and save/restore.
7. Manual Greyhaven testing confirms the boat/camera/ocean foundation is stable and understandable.
8. The dockside hub, inventory, dialogue and front-end overlays pass modal exclusivity and pause restoration checks on keyboard and gamepad.
9. A clean Windows 64-bit development build launches into Greyhaven.
10. Known remote repository protection limitations are recorded and an explicit owner decision is made.

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

## Area-streaming implementation update — 2026-11-12

A source-level additive area-streaming foundation has been added for Greyhaven. The generated blockout now separates static terrain, harbour, village, west/east estuary, and lighthouse scenery into six additive scenes. The persistent scene retains the vessel, water simulation, camera, dynamic sites, NPCs and anchors, dialogue, world clock, and save participants. A distance-based preloader uses asynchronous additive scene loads, startup preloading for the three central areas, a larger unload radius, unload grace, and a one-load-at-a-time default. Area roots are serialized inactive and validated for stable ID/revision, root count, static-only contents, mesh/material/shader integrity, and submesh material coverage before activation. Generated terrain and basalt outcrop meshes are saved as reusable Unity assets. Atmosphere caches refresh when area roots activate or unload.

The feature currently exists as source and generated-scene authoring logic, not as committed generated Unity scene outputs. Existing generated Greyhaven scenes require explicit confirmation before rebuilding; declining keeps the current blockout. The clean-checkout path generates the new area scenes through Initialize Project Foundation. Unity Editor compilation, Edit Mode tests, Play Mode travel/boundary/save/NPC tests, and Windows profiling/build remain unperformed. Do not present source-level CI as evidence of zero hitches or a completed runtime integration.

## 2026-11-13 addendum — regional chart discovery source update

The original audit above records its historical baseline at c1ff1975. Since that snapshot, the source has gained a local fog-of-war regional chart:

- RegionalChartController records physically visited regions and adds first visits to the existing unlockedRegionIds progression collection.
- GreyhavenRuntimeUI now has a Chart [M] overlay. It shows the vessel in the active region and only Sounding Rig-discovered phenomenon sites.
- Unknown region names and unknown site marks are omitted; the chart supplies no compass, GPS, route line or fabricated bearing.
- Save schema v7 adds WorldSaveData.visitedRegionIds and a v6-to-v7 migration that starts legacy saves with no invented visits.
- RegionalChartPolicyTests and RegionalChartSaveTests cover unknown suppression, idempotent visits, discovered-only site marks and save migration/preservation.

**Current status:** source implemented; Unity verification pending. This is a local survey plot, not a world-scale map or inter-region travel system. The Outer Shoals and Hollow Trench remain isolated test scenes. Run the generated-scene rebuild and verify UI, region detection, modal transitions and save/load in Unity 6000.3.28f1 before treating the feature as runtime-accepted.


## 2026-11-13 addendum — Greyhaven environmental artifacts

The Greyhaven Editor builder now authors deterministic static prop clusters for harbour, village services, estuary, lighthouse and the outer-channel wreck. The pass adds working-life residue, domestic details, repair scraps, weathered equipment and shoreline debris. It uses project-authored shared materials and primitive greybox geometry; it is not final production art.

Artifacts are collider-free and add no runtime scripts, rigidbodies, lights, save participants, interactions, journal entries, objectives or map discoveries. Parent-local cylinder/sphere placement preserves the wreck cluster under its offset salvage-site root. Edit Mode tests were added for district hierarchy, static constraints and local-space placement. The streamed-area revision was incremented; generated scenes must be rebuilt.

**Status:** source implemented; Unity Editor import, tests, scene rebuild, visual inspection, Play Mode and Windows build remain unverified.

### Historical artifact inspection — 2026-11-14

Six Greyhaven artifact anchors (map, tool, letter, logbook, photograph and survey equipment) are generated beneath a persistent scene root; their greybox visual props remain beneath the correct environment/streamed-area roots. `HistoricalArtifact` is a standalone Unity MonoBehaviour with a stable script asset identity. Chart, tool roll, keeper ledger and photograph are exposed through dockside furniture archive entries, while the landing letter and wreck recorder use direct Interact inspection. DocksideHubController runs before the direct inspector to prevent input races. A separate schema-v9 ledger stores inspected artifact IDs; the v8-to-v9 migration initializes it empty and preserves inspected phenomenon-site IDs.

The five non-Greyhaven artifact families are design proposals only. CI static validation and syntax parsing must pass, but Unity script import, regenerated scene persistence, Play Mode first/repeat read, save/load and area-unload review remain required.


## 5. Cross-system source integration — current as of 2026-11-15

The earliest economy note in this audit recorded the design-only stage and is superseded by the later source integration described here. The runtime source now connects:

- The authoritative 155-record seafood schedule (100 fish, 47 invertebrates, 8 algae/seaweed), with region/habitat encounter profiles and seaweed harvesting.
- Catch legality, protected/contract harvest permits, source-region/technique/weight provenance, fines, and ordinary-sale rejection for illegal catches.
- Destination-specific price factors and independent regional market stock/demand; atomic sales and purchases through the wallet and spatial cargo hold.
- Fuel use tied to propulsion and installed-engine condition, emergency reserve, paid vessel/hold/equipment repair, equipment slots/durability and service inventory.
- Harbourmaster salvage contracts and discovery-gated research, plus persisted permits and bounded expedition telemetry grouped by destination and loadout band.
- Greyhaven's dockside service, trade, contract and research panels using existing NPC schedule/trust gates and modal ownership.

This is **source integration**, not verified game behavior. The generated Greyhaven scene is the only physical market/service hub; non-Greyhaven destination terminals and their access triggers remain open. No representative Unity expedition data has been collected, so the balance model is still a hypothesis. Unity semantic compilation, Editor scene regeneration, Edit Mode and Play Mode execution, save/load round-trip and a Windows Development Build are mandatory before acceptance.

## 6. Historical archive and generated-area revision update — 2026-11-15

The six Greyhaven historical artifact anchors, dockside archive actions, direct close-range inspection and independent schema-v9 read ledger remain integrated in source. The other five regional artifact sets remain design proposals.

The generated-area revision preflight advances the static-area content revision to 5, opens each generated additive scene during initialization, and checks its single root, stable area ID, content revision and render-asset integrity. Missing, stale or invalid scenes require confirmation before all six area scenes and the Greyhaven blockout are regenerated. A regression suite covers exact ID/revision matching and stale/wrong-area rejection.

The source and regression coverage are not yet Unity-verified. The preflight must be exercised in the target Editor against both valid current scenes and deliberately stale/missing fixtures. The six areas must be rebuilt so the new historical visual props are present.

## 7. Remaining M0 blockers and repository state

- Clean Unity 6000.3.28f1 import and semantic compilation.
- Unity-generated Packages/packages-lock.json, stable .meta files for tracked Unity assets/scripts, and Editor Build Settings/scene outputs from a clean bootstrap. Do not fabricate these artifacts outside the Unity Editor.
- Execute all Edit Mode suites, including artifact, save migration, economy, scene-root revision and builder tests.
- Run Play Mode for area preloading/unloading, dockside modal/input ownership, vendor transactions, archive first/repeat reads, save/restore, and artifact-anchor persistence while visual scenes unload.
- Rebuild the six Greyhaven areas, inspect camera-height presentation/grounding/clearance, and run a Windows Development Build with a complete expedition smoke test.
- Review the two open Dependabot major-action update PRs; merge only after their workflow checks and pinned-SHA updates are verified.

The PR #31 workflow passed repository safeguards, project integrity and C# syntax parsing. These checks do not compile Unity assemblies, open generated scenes, execute tests, measure performance or build Windows. M0 remains in progress.

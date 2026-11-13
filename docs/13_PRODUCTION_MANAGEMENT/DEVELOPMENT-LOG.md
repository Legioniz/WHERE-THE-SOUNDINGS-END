# Development Log

> Project: WHERE THE SOUNDINGS END  
> Status: Active working log  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-13

## 2026-11-13 — Cross-system modal ownership and repository documentation reconciliation

**Commit:** `c1ff19756dc62936f2c6c78f3b344b0fdbcbb093` (PR #10 squash merge).

### Implemented
- Added a shared DocksideModalPolicy used by the dockside hub, inventory/dialogue presenter and front-end pause overlay. Dockside, inventory, dialogue and title/pause/save/settings panels cannot open over one another.
- Added keyboard Esc and gamepad Start handling for closing the dockside menu without also opening the pause overlay. The hub also refuses to open while the front end is paused and releases its pause/prompt ownership if disabled.
- Routed generated resident interaction handling through DocksideHubController; externally managed legacy resident triggers no longer enable or disable the shared Interact action-map input.
- Added Edit Mode policy regressions for dockside/inventory/pause modal exclusion and expanded repository validation to enforce runtime wiring.
- Reconciled README, Greyhaven world/settlement, dialogue/NPC, save schema, technical architecture, streaming, testing, production and UI/UX documentation with the current source.

### Verification boundary
- Edit Mode tests are authored but have not been executed in Unity 6000.3.28f1.
- GitHub Actions run #279 passed repository integrity, Unity project validation, C# syntax parsing and repository safeguards.
- UI Toolkit rendering, actual Input System event ordering, keyboard/gamepad focus, pause restoration, generated scene rebuild and Play Mode remain unverified.

### Compatibility and risk
- No save schema or stable NPC/dialogue IDs changed. The dockside hub continues to use existing save participants and checkpoint autosave.
- The modal policy is source-level integration coverage; Unity runtime behavior still needs human verification.
- The first CI attempt exposed an over-specific validator expectation that looked for the pause-policy call in the hub instead of its actual front-end owner. The validator was corrected to inspect the actual owner; run #279 then passed both jobs.

## 2026-11-13 — Greyhaven boat-operated dockside interaction hub

**Commit:** `c95478b25b8ed9ba55acf30ce8804a1e5bfdd27e` (PR #8 squash merge).

### Implemented
- Added a persistent dockside hub UI with seven location menus: Main Dock, Harbour Office, Fish Market, Boat Repair Shed, Net Mender Shed, Harbour Inn and Western Lighthouse.
- Added shore-approach access checks requiring the vessel to be within the authored horizontal radius and moving at no more than 0.85 m/s. Opening the menu claims the existing interaction gate and pauses simulation; closing restores the prior time scale.
- Added menu-based room presentations and furniture actions for tide records, harbour chart/notices, catch appraisal, vessel diagnosis, net-care notes, guest-ledger checkpoint, rumours, watch status and lighthouse records.
- Routed the four existing residents through their existing Yarn nodes and NpcAgent relationship gates. Disabled duplicate E-key resident trigger handling for generated Greyhaven NPCs while retaining the component for hand-authored scenes.
- Added deterministic Edit Mode coverage for dockside access policy and documented that trade, paid repair, net crafting, sleep/time-skip, residential visits and walkable 3D interiors remain unimplemented.

### Verification boundary
- Source and generated-scene builder ownership were inspected against the current main branch before editing.
- Edit Mode tests are authored but have not been executed in Unity 6000.3.28f1.
- GitHub Actions run #274 passed repository integrity, C# syntax and safeguard checks.
- The generated Greyhaven scene must be rebuilt in Unity. UI rendering, sorting order, input behaviour, resident dialogue handoff, checkpoint writes, Play Mode and Windows build remain unverified.

### Compatibility and risk
- No save schema, stable NPC ID, dialogue node, relationship state, input action, package version, boat physics or streamed-area-root contract changed.
- The dockside menu is a functional UI greybox, not final interior art. Its read-only market/repair/net interactions do not fabricate missing economy or equipment systems.

## 2026-11-13 — Greyhaven streaming transition verification and profiling

**Commit:** `3b02d0ce205b828869deb30ff4a32f72c3f8e64d` (PR #7 squash merge).

### Implemented
- Added a re-entry safeguard: an area requested while its unload is in progress is queued for reload after unload completes, and the queue rechecks current player distance before starting.
- Added a final distance guard immediately before the streamed root is deactivated for unload.
- Added Development Build / Editor telemetry for additive load duration, per-area unique mesh/material/texture references and estimated asset bytes, global allocated/reserved memory, managed heap use and GC memory.
- Added a Play Mode test that loads Greyhaven, verifies startup areas, teleports the test vessel to the lighthouse to exercise additive preload, returns to the harbour, and checks the lighthouse unload while the persistent gameplay scene remains loaded.
- Extended static repository validation and the streaming/test documentation.

### Verification boundary
- GitHub Actions run #272 passed `Validate Unity project and C# syntax` and `Repository safeguards`.
- The Play Mode test is authored but must be executed in Unity 6000.3.28f1. No claim is made that the transition test has run.
- Runtime profiling telemetry is implemented, but no real Windows hardware measurements, frame-time captures, GPU VRAM measurements or memory budgets have been collected.

### Remaining scope
- At the time of PR #7, the dockside hub and menu-based furniture actions were not yet included. PR #8 subsequently added seven menu-based location/interior presentations and furniture actions without changing the boat-only movement model. Physical walkable 3D interiors, an on-foot controller, trade, paid repair, net crafting and residential visits remain future work.
- No save schema, NPC identity/schedule/dialogue contract, boat physics, or static area-scene ownership changed.

## 2026-11-13 — Greyhaven resident furniture and personality pass

**Commit:** [`82cecc7`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/82cecc7844e983881c5febd400ee24d9e29c46ab) — merged through [PR #5](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/pull/5).

### Implemented
- Added a Harbourmaster quay-watch log desk with a tide register, channel chart board, chronometer, chart roller and stool, plus an office-side cabinet of tide books and a weather annotation board.
- Added Mara Venn's catch-sorting station with weighing scale, tally board, sample jars, sea-glass shelf and a small shell collection box.
- Added Tomas Vale's repair workbench with engine test housing, vice, pegboard tools, spare propeller and a compact radio-tuning experiment.
- Added Inez Sarmiento's net-mending table and loom, mesh strands, pinned knot samples, thread spools, sorting tray and stool.
- Added a modest shared table, benches and lantern to the Harbour Inn common area.
- Kept every furnishing as static collider-free generated geometry, owned by the existing harbour/village builder roots so area-streaming extraction keeps each prop with its correct area.
- Preserved NPC IDs, schedule anchors, dialogue nodes, relationship/save contracts, boat systems and streaming definitions.

### Verification boundary
- GitHub Actions [run #268](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/actions/runs/43271865246) passed both the Unity project/C# syntax validation job and the repository safeguards job.
- Unity `6000.3.28f1` semantic compilation, Editor scene regeneration, camera-scale visual review, Play Mode NPC route clearance, streamed-area transitions and Windows profiling remain unverified.
- At the time of this PR, the furniture was visual set dressing only; no new furniture interaction, usable interior, inventory item, hobby progression or save data was claimed. PR #8 later added menu-based furniture actions without introducing walkable rooms or new save data.

## 2026-11-12 — Relationship-reactive Greyhaven dialogue expansion

**Commit:** Pending repository validation.

### Implemented
- Audited the full relationship call path: each generated interaction receives its own resident's `NpcAgent`, the UI seeds Yarn relationship variables before `StartDialogue`, the Yarn command updates only the active resident, and the meter/variables refresh after accepted events.
- Added a safe `NpcAgent` auto-resolution fallback for hand-placed interactions. If no agent can be resolved, the interaction prompt reports missing NPC data and dialogue is not started with a misleading neutral default.
- Registered the relationship command with Yarn Spinner's explicitly typed `AddCommandHandler<string, int>` signature.
- Expanded all four resident conversations with seven relationship-specific greetings, everyday topics, occupation-specific facts, personal sayings and optional subjects at Familiar, Trusted and Ally thresholds.
- Kept story knowledge separated: the Harbourmaster remains procedural, Mara remains focused on the market and catch quality, Tomas on mechanical evidence, and Inez on physical net/line evidence.
- Extended the repository validator to check relationship-tier branches, quote options and representative daily topics in each resident node.

### Verification
- Audited the Yarn flow-control structure for balanced `if/endif` blocks and conditional-option syntax against the official Yarn Spinner language reference.
- Verified in source that the active NPC relationship is synchronized before Yarn starts and that relationship events refresh the meter and Yarn values.
- GitHub Actions will validate project contracts and C# syntax after this change is pushed.
- Yarn compilation in Unity, line/option presentation, relationship-tier Play Mode testing and Edit Mode execution remain unverified in Unity 6000.3.28f1.

### Compatibility and risk
- No save schema, NPC stable ID, dialogue node ID, input action or package version changed.
- Existing Yarn flags and relationship event IDs are retained. The expanded content adds dialogue branches without resetting saved campaign dialogue state.
- Trade multipliers remain policy-only until a sale transaction exists.
- Before full campaign release, add non-conversation recovery actions for NPCs who refuse service at hostile trust levels.

## 2026-11-12 — Greyhaven settlement buildings and dock chart

**Commit:** Pending repository validation.

### Implemented
- Replaced identical blank building boxes with service-specific greybox architecture: pitched gable roofs and end caps, foundation plinths, visible corner posts, framed doors, divided/shuttered windows, front steps and public service signs.
- Added distinct work fronts for the Fish Market, Harbour Office, Boat Repair Shed, Net Mender Shed and Harbour Inn, plus porch/chimney details for the four residential houses.
- Repositioned the Harbourmaster's quay-watch anchor onto the inner main pier and moved the off-duty office anchor to the Harbour Office entrance. Mara, Tomas and Inez remain at their respective market, repair and net-mender frontages. Stable IDs, schedules, Yarn nodes and save contracts are unchanged.
- Added a static physical chart table on the east half of the T-pier. Its schematic island silhouette samples GreyhavenTerrainProfile and marks the dock, village workplaces, lighthouse and Outer Channel Wreck Field, with a workplace key for all four named residents.
- Added a settlement/wayfinding design note and updated Greyhaven, NPC, technical, test and documentation index material.

### Verification
- Current Greyhaven builder, terrain profile, NPC schedule design, and generated-scene ownership were inspected before editing.
- GitHub repository-integrity, C# syntax and safeguard CI results are pending.
- Unity `6000.3.28f1` semantic compilation, scene rebuild, built-in TextMesh font rendering, orthographic-camera legibility, resident waypoint movement, dock chart placement, boat clearance and Play Mode collision remain unverified.

### Compatibility and risk
- The scene is generated by the Editor builder; the generated scene output itself is not updated until **Rebuild Greyhaven Environment** runs in Unity.
- The map is static and schematic, not a live map/GPS or calibrated nautical chart.
- The added roofs/signs/map/porches are greybox geometry, not final art. Static visual props intentionally do not add new interaction, schedule, persistence or market behavior.

## 2026-11-12 — Greyhaven island topography and graphic model

**Commit:** Pending repository validation.

### Implemented
- Added a shared deterministic Greyhaven terrain profile with a varied bay shoreline, west headland, east ridge, inland spine, rear terrace, multi-scale surface relief and bounded height samples.
- Increased the generated terrain grid to 144 × 96 quads and added world-space UVs for future tiled materials.
- Replaced floating rectangular sand segments with a shoreline-conforming sand submesh.
- Split the continuous terrain mesh into soil, sand, grass and basalt submeshes using elevation, slope and shoreline distance while retaining one shared terrain MeshCollider.
- Replaced smooth stretched-sphere inland outcrops with deterministic faceted basalt meshes and static mesh colliders.
- Added Edit Mode regression tests for deterministic shoreline shape, landmark elevations, finite/bounded terrain samples, material classification and invalid-input handling.
- Updated Greyhaven, technical structure, testing and documentation index references. No world-region IDs, save contracts, player controls, sea-time rules or water simulation ownership changed.

### Verification
- Repository source, Greyhaven generator, art direction and terrain acceptance requirements were inspected before editing.
- The new Edit Mode test source is authored but has not been run in Unity.
- GitHub static project, C# syntax and safeguard checks are pending CI results.
- Unity `6000.3.28f1` semantic compilation, generated-scene rebuild, Editor rendering, Play Mode boat/rock collision, performance profiling and Windows build remain unverified.

### Compatibility and risk
- Generated scene output is not changed by the source edits alone. Rebuild Greyhaven through the documented Editor menu after pulling this branch.
- The island geometry remains a prototype landform; six-region geography, final terrain art, textures, erosion, shoreline foam and vegetation production assets remain out of scope.
- Terrain resolution and mesh-collider cost require review in the pinned Unity Editor on representative hardware.

## 2026-11-12 — Vessel collision and impact response

**Commit:** Pending CI verification.

### Implemented
- Replaced total relative-speed damage with contact-point relative velocity projected onto the contact normal. Rotational point velocity is included, so tangential scraping is not mistaken for a full-speed collision and a rotating bow/stern can still register a real strike.
- Coalesced the strongest collision contact during a physics step and processed one damage event, reducing duplicate damage from multiple contact reports.
- Removed manual post-impact linear/angular velocity multipliers; Unity's Rigidbody solver now owns physical collision response.
- Added NaN/infinity guards, a deterministic capped hull-damage curve, proportional engine damage, a true zero-rate damage disable, and existing local cargo-cell damage.
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

## 2026-11-13 — Greyhaven dialogue and lore expansion

**Source change:** Added a cross-witness evidence thread to the four existing Greyhaven Yarn conversations.

### Implemented in source
- Added four shared, saveable Yarn bool flags for the bell log, cold-centred catches, returned-hull scoring and empty/untorn net.
- Expanded each resident's dialogue with an occupation-bounded observation and a comparison topic that changes from a partial summary to a four-record synthesis once all clues are found.
- Added personal-life details at the existing Familiar relationship threshold: Harbourmaster chart habits, Mara's market philosophy, Tomas's radio restoration hobby and Inez's woven-line birds.
- Kept clue discovery separate from relationship rewards; no repeatable trust farming was added.
- Updated the World Bible, Main Story, Character Bible, NPC Relationships, Greyhaven NPC System, Dialogue System, README and Task Backlog.

### Compatibility and verification
- No C# runtime code, NPC stable ID, Yarn node ID, relationship event ID, quest service, input binding, save DTO or schema version changed.
- The four flags use the existing Yarn typed-variable persistence path through YarnDialogueSaveParticipant and SaveGameData.dialogue.
- Repository validation and C# syntax checks are the next automated gates. Yarn compilation, conditional-option filtering, save/load restoration, Unity presentation, Edit Mode/Play Mode tests and Windows build remain unverified until executed in Unity 6000.3.28f1.
- The Choir's cause and the new witness synthesis remain unresolved; this is narrative evidence, not a canonical supernatural explanation.

## 2026-11-13 — Regional chart discovery and fog-of-war

**Source status:** Implemented in the gameplay branch; CI and Unity verification pending.

### Implemented
- Added RegionalChartPolicy and RegionalChartController. The persistent vessel records a region only when it enters that region's authored survey bounds; first visits add a stable ID to visitedRegionIds and the existing unlockedRegionIds progression collection.
- Added a UI Toolkit regional chart opened with M or the HUD button and closed with Esc. The local plot shows the vessel and only sites already discovered by the Sounding Rig.
- Unknown regions are omitted. Unknown sites receive no name, marker, tooltip, bearing, compass pointer or route. The existing Greyhaven dockside settlement chart remains a separate static wayfinding prop.
- Attached chart discovery to Greyhaven and both deep-ocean survey sandbox builders. The controller remains on the persistent vessel, never on streamed static area roots.
- Added save schema v7 with WorldSaveData.visitedRegionIds. The v6-to-v7 migration initializes an empty visit list and preserves existing unlockedRegionIds.
- Added four policy tests and three save migration/persistence tests. Updated progression, world-map, acoustic-navigation, HUD, controls, save, test and production documentation.

### Compatibility and boundaries
- Existing SoundingRigController remains authoritative for discovered site IDs. Chart markers are derived from that discovery set; no duplicate site discovery ledger is added.
- No boat physics, input action asset, NPC identity/dialogue, streamed-area ownership or quest schema changed.
- Unlocking currently means chart knowledge after physical entry. It does not connect the Outer Shoals or Hollow Trench sandboxes into inter-region travel and does not gate scene loading.
- The local plot uses authored survey bounds; it is not a calibrated nautical projection, GPS, compass, world map, coastline raster or route planner.

### Verification boundary
- Edit Mode tests are authored but not executed in Unity 6000.3.28f1.
- GitHub CI can check project integrity and C# syntax, but does not prove Unity semantic compilation, UI Toolkit rendering, runtime region detection, map marker placement or save/load integration.
- Rebuild Greyhaven and both survey sandboxes, then verify region entry, site discovery, modal exclusivity and schema 6/7 restoration in Unity.


## 2026-11-13 — Unexplained warning indicator

Added a rare, transient visual anomaly to the existing FishermanAtmosphereDirector. A 5.5% reserved incident-roll band produces a brief, non-directional broken-corner flash without text, audio, bearing, map mark, objective, journal entry, or gameplay effect. Remaining incident rolls are remapped to preserve the established incident mix. The cue is not persisted and does not change save schema. Edit Mode policy coverage and repository validation guards were added. Unity visual inspection and test execution remain pending.


## 2026-11-13 — Greyhaven environmental artifact and set-dressing pass

**Source status:** Implemented in the Editor scene builder; generated scene rebuild and Unity visual acceptance pending.

- Added deterministic clusters of maritime work residue, personal belongings, repair scraps, shoreline debris and keeper equipment across the harbour, village, estuary, lighthouse and outer-channel wreck.
- Added shared muted material variants for oxidized metal, chipped ceramic, salt-faded canvas and tarred rope. Existing material creation remains shared and GPU-instanced.
- Kept artifacts static, collider-free and free of runtime scripts, lights, rigidbodies, save state and interaction behavior. They do not create journal entries, objectives, chart discoveries or NPC state changes.
- Corrected artifact cylinder/sphere transforms to use parent-local coordinates, including the wreck whose parent is offset from world origin.
- Added Edit Mode coverage for district hierarchy, no-collider/no-runtime-component constraints, renderer presence and wreck local-space placement.
- Incremented the generated streamed-area revision so existing generated scenes are recognized as stale and must be rebuilt.

**Verification boundary:** Repository CI/static checks can verify source contracts and syntax only. Unity Editor import/compilation, Edit Mode execution, generated scene inspection, camera readability, clipping/grounding, Play Mode streaming, profiling and Windows build remain pending.

## 2026-11-15 — Cinematic presentation major upgrade

**Branch:** feature/cinematic-presentation-upgrade-20261115.

- Reworked cinematic cards into a guided sequence with record/beat counters and a progress indicator.
- Added progressive caption reveals with a deterministic, bounded timing policy.
- Added a persistent Reduce cinematic camera movement option. It suppresses authored camera moves/zoom, displays captions immediately and shortens transitions without changing the story sequence.
- Added Edit Mode coverage for caption reveal timing and invalid floating-point inputs.
- Extended repository validation to guard the director UI, accessibility option, policy and regression tests.
- No story facts, save schema, Yarn node, resident ID, input action or external media changed.

**Verification boundary:** repository CI checks source contracts and C# syntax only. Unity Editor rendering, semantic compilation, Play Mode camera/input tests and Windows launch remain required.

---

## 2026-11-15 — Greyhaven opening and first-contact cinematics

**Branch:** feature/greyhaven-opening-cinematics-20261115.

### Implemented in source
- Added a first-launch in-world archive sequence with four restrained beats establishing the Orison loss and the protagonist's return to Greyhaven. It is skippable and replayable from the title menu.
- Added a four-beat fresh-voyage prologue when Continue finds neither a restorable manual save nor autosave. Existing save restore is unchanged.
- Added first-contact camera/name-card presentations for the Harbourmaster, Mara Venn, Tomas Vale and Inez Sarmiento. The existing Yarn dialogue node begins only after the cinematic restores the live camera.
- Added temporary letterbox bars, low-contrast archive shading, scanline accents, lower-third captions and fades. Cinematics use unscaled time and pause gameplay; keyboard, gamepad and on-screen skip paths are supported.
- Added deterministic first-contact registration tests and updated repository validation, the README, documentation index and cinematic acceptance checklist.
- No save schema, NPC stable ID, Yarn node, input action, package, voiceover or licensed media changed.

### Verification boundary
- Repository CI is expected to validate project contracts, C# syntax and safeguards when the pull request is opened.
- Unity 6000.3.28f1 semantic compilation, UI Toolkit rendering, actual camera framing, input focus, Edit Mode execution, Play Mode and Windows launch/playtesting remain unperformed in this environment.
- First-contact presentation state is session-local and cosmetic; it is intentionally not persisted to campaign saves. Re-launching may show a resident's introductory card again.
- The opening/prologue use the generated Greyhaven scene and authored text/camera movement; they are not pre-rendered movies and do not include voice acting or music.

---

## 2026-11-15 — Import-safe first-open bootstrap hardening

### Research finding
- Unity's official Editor documentation warns that `InitializeOnLoad` runs before non-script asset import completes and that `AssetDatabase.LoadAssetAtPath` can return null for assets still importing.
- The previous first-open hook used `InitializeOnLoad` plus `delayCall` to load the Input Actions asset. Although delayed, this left the bootstrap dependent on import timing.

### Correction
- Replaced the first-open hook with `AssetPostprocessor.OnPostprocessAllAssets(..., didDomainReload)`, which Unity calls after asset import completes.
- The callback schedules the scene generation on the Editor main thread, coalesces duplicate notifications, and retries if compilation is still active.
- Existing scenes remain untouched by the fresh-checkout hook. The explicit normal-Editor stale-scene confirmation remains in place.
- Extended the repository validator to reject a return to the pre-import `InitializeOnLoad` asset-loading pattern.

### Verification boundary
- Static validation can check the import-safe bootstrap contract; only opening a fresh clone in Unity 6000.3.28f1 can prove the actual import and generated scene sequence.
- Unity semantic compilation, Edit/Play Mode execution and Windows player launch remain pending on the locally licensed runner.

---

## 2026-11-15 — Fresh-checkout bootstrap and Unity playable verification path

### Implemented in source
- Added a first-open Editor bootstrap for a fresh checkout with no generated Greyhaven scene. It waits until Unity finishes importing, generates the starter environment and project rendering settings, and does not silently rebuild an existing scene.
- Added a non-interactive setup entry point for unattended Unity validation. Batch setup repairs stale generated areas without opening a modal confirmation dialog; normal Editor setup retains the explicit confirmation.
- Added a Play Mode startup smoke test covering the generated boat, Rigidbody/hydrodynamics, assigned Input System controls, harvesting, Sounding Rig, cargo hold, economy components, camera follow target, sea-state director, save/autosave and area streaming.
- Added a deterministic Windows Development Build entry point. It prepares generated scenes, checks that all six Greyhaven streamed areas and both deep-water test ranges are enabled in Build Settings, then fails unless Unity reports a successful Windows 64-bit build.
- Added a manually dispatched Unity workflow for a locally licensed self-hosted Windows runner: all Edit/Play Mode tests first, Windows Development Build second, with test reports and build artifact upload. The workflow receives no repository secrets.
- Expanded the static validator to guard the bootstrap, build entry point, startup smoke test and Unity workflow. Updated setup and verification documentation.

### Verification boundary
- Repository source changes can be statically validated in GitHub CI, but this work session cannot launch Unity Editor or a Windows player. No Unity semantic compile, Edit Mode/Play Mode pass, Windows build or manual play session is claimed here.
- The Unity workflow requires a self-hosted Windows runner with Unity 6000.3.28f1 installed, locally activated and labelled `unity-6000.3.28f1`. After the workflow succeeds, the downloaded Windows Development Build must still be launched on Windows and exercised through a complete Greyhaven expedition before the game is considered play-verified.
- Generated `.unity`, `.meta`, `packages-lock.json` and editor build settings remain Unity-generated outputs; they must not be fabricated in a source-only change.

---

# Development Log

## 2026-11-15 — Revision-aware area-scene bootstrap and repository reconciliation

**Branch:** fix/revision-aware-bootstrap-and-doc-reconciliation-20261115

### Implemented in source

- Advanced Greyhaven streamed-area content revision from 4 to 5 to include historical archive prop clusters beneath their owning static area roots.
- Changed the Editor preflight from file-existence-only detection to additive scene inspection. It verifies the single scene root, stable area ID, current content revision and referenced render asset integrity.
- Preserved scenes already loaded before preflight; scenes opened only for validation are closed, and the previously active scene is restored.
- Changed project initialization to show a clear rebuild confirmation for missing, stale or invalid generated area scenes. Declining preserves existing scenes and logs the validation failure.
- Added deterministic Edit Mode coverage for exact scene identity/revision matches and rejection of stale revision, wrong area, and missing identifiers.
- Updated the project validator, README, documentation index, streaming technical spec, testing strategy, implementation audit, milestone checkpoint and backlog. Reconciled the README economy status with connected source implementation and corrected escaped newline formatting.

### Verification boundary

- Pull-request CI will run repository integrity checks, Tree-sitter C# syntax parsing and repository safeguard checks.
- Unity Editor semantic compilation, Editor preflight against real generated scene assets, Edit Mode execution, Play Mode streaming/archive/save-load, visual inspection and Windows build remain unperformed.
- Packages/packages-lock.json, broad Unity-generated .meta files, generated .unity scenes and ProjectSettings/EditorBuildSettings.asset must be generated/validated by Unity 6000.3.28f1. They are intentionally not fabricated in this source-only update.

---
## 2026-11-14 — Seafood catalogue, catch legality, regional markets and expedition measurement

**Branch:** feature/seafood-legality-regional-markets-balance-20261114

### Implemented in source

- Expanded the generated seafood catalog from the five starter fish to the full 155 scheduled records: 100 fish, 47 invertebrates and 8 algae/seaweed. The builder authors prices, units, market class, regional availability, habitats, catch profiles and tier-based stock/recovery.
- Extended fishing encounter selection to include shellfish and cephalopods. Added bait-free Seaweed Harvesting for Kelp habitat, with renewable stock and no rod-wear/rod-unlock progression.
- Added game-authored catch legality for species region, minimum landing weight, protected species, specific harvest permits, prohibited technique or unapproved hook and the Hollow Trench closure. Flagged catches retain source region, technique, weight, status and reason; fines use the wallet ledger and ordinary buyers reject illegal lots.
- Connected harvest permits to Harbourmaster research: Reef Stewardship, Licensed Sea Cucumber Harvest and Managed Abalone Harvest. Harvest permits persist in the existing unlocked-permit collection but are filtered out of chart-region names to preserve fog of war.
- Connected the six-destination market factor matrix to the market controller. Stock/demand is keyed by regional market and item; the sale UI displays destination factors and labels licensed contract intake.
- Expanded expedition telemetry to retain up to 50 completed samples, grouped by destination and departure loadout band (STARTER / AVERAGE / OPTIMIZED). Samples record gross receipts, expenses, fuel, game-clock duration, catches and legal-catch rate. Active trip catch/loadout state survives save/reload.
- Added Edit Mode source tests for market factors, catch legality, permits, catch provenance save/restore, seaweed technique compatibility and telemetry sample persistence.
- Expanded repository validation to check 155 catalog rows against the authoritative price schedule and to inspect the cross-system links.

### Verification boundary

- GitHub Actions must run repository safeguards, project-file validation and C# syntax parsing for the pull request.
- Unity 6000.3.28f1 compilation, Edit Mode test execution, Play Mode, Greyhaven scene regeneration and Development Build have not been performed in this environment.
- Greyhaven is currently the only generated dockside market scene. Destination market factors and per-region stock logic are implemented; physical terminals and access triggers for non-Greyhaven streamed destinations remain scene work.
- No real Unity expedition balance samples have been collected. Provisional target envelopes and a controlled sample protocol are documented; no actual measured balance result is claimed.
- The tiered catch-weight thresholds are fictional game rules, not real-world legal limits.

### Compatibility

- Save schema remains v8. Catch provenance is additive on cargo placements; regional stock reuses existing market stock entries; harvest permits reuse unlockedRegionIds; telemetry samples use namespaced progress flags.
- Legacy cargo placements with no catch-region metadata remain treated as legacy inventory.
- No regional chart location is revealed or marked visited by a harvest permit.

---



## 2026-11-14 — Greyhaven fishing encounter, tackle and population foundation

**Branch:** fishing implementation work in progress; awaiting repository CI and review.

### Implemented in source
- Added FishingSystemDirector to the persistent vessel. HarvestingMinigameController routes the existing Fish input to it while retaining the independent dredging reservation/commit flow.
- Added rod, line, hook, reel, bait and seven fishing methods. Tackle tiers unlock at five and twelve lifetime catches; selected loadout, technique and catch count use existing namespaced progress flags.
- Added cast/deployment, variable bite waiting, a hook window, multi-beat fish fight, tension management, tackle-dependent line failure, missed-pull recovery, fish escape and manual line retrieval.
- Extended fish cargo definitions with habitat, depth, preferred bait/method, resistance, burst frequency, stock, recovery, migration and catch-size metadata. Greyhaven's generated catalog now authors distinct profiles for its ordinary and anomalous starter fish.
- Added per-species/per-habitat aggregate stock depletion, bounded daily recovery and six-in-game-hour habitat migration. Stock/timing uses existing regional stock and world-flag save fields; save schema remains unchanged.
- Added deterministic Edit Mode policy tests and authoritative gameplay/technical documentation.

### Verification boundary
- Repository CI, C# syntax validation and Edit Mode test execution must be checked after the PR is opened.
- Unity 6000.3.28f1 compilation, Greyhaven asset/scene rebuild, Play Mode encounter/input/cargo/save-load testing and Windows Development Build remain outstanding.
- The habitat resolver and depths are temporary Greyhaven coordinate heuristics. Population migration is a simple cyclic transfer, not a validated marine ecology model.
- The loadout/encounter UI is a temporary IMGUI prototype; market-to-loadout equipment integration, gear durability, per-catch length/weight records, seasonal ecology and full economy integration remain open.

### Compatibility
- No save schema version bump or new save DTO is required; namespaced fishing flags and regional stock entries reuse existing fields.
- Dredging continues to use its existing salvage reservation contract.
- The new fishing director is attached to the persistent vessel, never to streamed scenery roots.


> Project: WHERE THE SOUNDINGS END  
> Status: Active working log  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-14

## 2026-11-14 — Greyhaven bespoke maritime assets and regional environment pass

**Branch:** `art/greyhaven-bespoke-maritime-environments-20261114` (pending review and integration).

### Implemented in source
- Added `GreyhavenMaritimeAssetBuilder`, called by the deterministic Greyhaven scene generator.
- Replaced generic prop language in key silhouettes with assembled mooring bollards, deck cleats, lanterns, anchors, lobster/crab pots, fish crates, stave barrels, a repair winch, propeller, net rack and keeper weather station.
- Added mangrove variants with bent trunks, exposed prop roots and layered crowns, plus reeds/salt grass, storm driftwood, abandoned float lines, shoreline wrack and broad wear patches.
- Established four visual identities—working harbour, village work yards, estuary/tide line and lighthouse headland—using the existing muted material palette.
- Kept dressing deterministic and static; no colliders, rigidbodies, runtime scripts, lights, save data or gameplay interaction are added.
- Incremented the streamed-area revision to 3 and added regression coverage for district ownership and static-only constraints.

### Verification boundary
- GitHub Actions must validate repository contracts and C# syntax after the pull request is opened.
- Edit Mode tests, generated-scene rebuild, camera-height placement review, Play Mode streaming, profiling and Windows build remain Unity-only acceptance work.
- The meshes are assembled procedural greybox-quality art, not final authored texture/UV/LOD production assets.

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


## 2026-11-14 — Researched economy and trading integration

**Branch:** `feature/greyhaven-economy-trading-20261114`  
**Source status:** Integrated economy/trading design specification authored; runtime work not yet implemented.

### Research and integration
- Reviewed developer commentary and design analysis for DREDGE's cargo/freshness/return loop and its decision to avoid fuel busywork, plus Sunless Sea's route planning and destination-specific trade design.
- Reviewed Unity's ScriptableObject guidance to preserve the boundary between static item/vendor definitions and mutable saved runtime market state.
- Cross-checked the existing seafood price schedule and economy mathematics rather than creating competing prices or modifier ranges.
- Connected the economy design to Greyhaven's existing four residents, spatial cargo hold, vessel condition, action-driven sea clock, dockside modal ownership and save architecture.
- Authored `docs/08_TECHNICAL_DESIGN/ECONOMY-AND-TRADING-SYSTEM.md` and added ECO-01–ECO-11 to the implementation backlog.

### Runtime boundary
- No economy runtime code, wallet transactions, market demand simulation, fuel consumption, paid repairs, purchasing or service UI is implemented by this documentation change.
- Next implementation starts with sale units/base prices and schema migration, then deterministic policies, atomic transactions, propulsion fuel integration, vendor services and dockside UI.


## 2026-11-14 — Economy runtime foundation (ECO-01–03 partial)

**Branch:** `feature/economy-runtime-foundation-20261114`

- Added authored sale price, unit label and explicit ordinary-sale eligibility to cargo definitions; unconfigured items remain unsellable.
- Persisted per-instance sale quantity in cargo placements/save records and connected the fishing catch's calculated kilogram weight to its cargo instance.
- Advanced save schema to v8 with integer-cent wallet storage and transaction-id ledger; schema-v7 legacy whole-dollar currency is converted to cents without deleting the legacy field.
- Added deterministic sale, demand, repair/fuel quote and overflow-safe wallet policies, plus an idempotent `EconomyWalletController` attached to the generated vessel and Edit Mode test source.
- Static validator now requires the policy and tests.

**Boundary:** This is a cohesive data/policy foundation, not the completed economy. Authored schedule values on assets, vendor transactions, market stock, purchases, fuel consumption, repairs, NPC trade gates and UI remain outstanding. Unity compile/Test Runner execution has not been performed in this environment.


## 2026-11-14 — Mara fish market transactions (ECO-04 partial)

- Added MaraFishMarketController to the persistent generated vessel. It discovers Mara by stable NPC ID and uses her existing market-shift schedule and relationship trade gate.
- Added the first Greyhaven seafood price authoring from the existing USD/kg schedule for FSH-001, FSH-002, FSH-003, FSH-005 and FSH-007. Unpriced, aberrant, salvage, quest and equipment cargo remains ineligible.
- Added itemized sale offers using each cargo instance's retained weight, quality and freshness; buyer-specific demand responds to persisted stock and remains bounded by EconomyPolicy.
- Added per-vendor/item stock persistence to the existing schema-v8 economy DTO. Legacy v8 saves normalize the new list safely without a schema bump.
- Added a synchronous cargo removal commit boundary: all selected instances are validated, the idempotent wallet payment is committed, and only then is the complete selection removed. Failed commits leave cargo intact.
- Added Edit Mode test source for failed, successful, duplicate and missing-item cargo commits; required-file checks now include the market runtime and tests.

**Boundary:** callable Mara market runtime is present; the dockside UI, purchases, passive restock, salvage buyers, relationship price modifiers and other vendor services remain open. Run the Greyhaven editor builder in Unity to regenerate the boat with the controller and apply the authored starter price fields. Unity compilation, Edit Mode execution and Play Mode testing have not been run here.


## 2026-11-14 — Connected Greyhaven economy services (ECO-04–10 partial)

- Added persisted vessel fuel with propulsion-load consumption, no idle/paused drain, and a separately saved emergency reserve for low-power return after the main tank empties.
- Added Tomas's schedule/trust-gated service counter: itemized hull/engine repair quotes, per-cell hold repair, refuelling and bounded relationship discounts. Charges are committed before deterministic repairs are applied.
- Added Inez's schedule/trust-gated supply counter: stocked starter equipment and four bait types, relationship discounts, spatial cargo fit validation and rollback if payment fails. Bait is now consumed one unit only when the selected fishing cast is deployed.
- Added Inez's salvage appraisal and atomic purchase of the three authored Greyhaven salvage materials. Their provisional per-piece USD values are explicitly recorded in the price schedule; unlisted salvage remains ineligible.
- Added once-per-game-day bounded restocking for Mara's seafood buyer and Inez's supply/material inventory, persisting current stock, target stock and last restock day in the existing schema-v8 economy DTO.
- Integrated itemized seafood/salvage offers, sale confirmation, purchases, repairs and refuelling into the existing dockside modal. Vessel inspection now reports main and emergency fuel.
- Added a one-time $25.00 starting wallet grant for untouched zero-balance campaigns; a depleted wallet with transaction history is never replenished on load.
- Added Edit Mode test source for propulsion fuel behavior, emergency reserve and stack-aware bait consumption; required-file validation now includes the new runtime services and tests.

**Remaining:** full legal-catch/closed-area/quota checks, Harbourmaster contracts and research rewards, complete catalogue prices, active equipment-slot application, destination-market travel pricing, trip range/net-income telemetry and economy playtests. Unity Editor compilation, Edit Mode execution, generated-scene rebuild and Play Mode testing have not been run here.


## 2026-11-14 — Harbourmaster salvage contract board

**Branch:** `feature/harbourmaster-contracts-equipment-20261114`

### Implemented in source
- Added a persistent three-job Greyhaven delivery board for Corroded Ship Fittings, Salt-Stiff Canvas and Waterlogged Timber.
- Connected the board to the existing dockside Harbourmaster service panel; no separate pause/menu owner was introduced.
- Contract completion removes the selected salvage placement only when the idempotent wallet reward transaction commits. The board accepts one active contract at a time and prevents completed jobs from being repeated.
- Active contract and completion history reuse the existing progress world-flag save channel; schema v8 is unchanged. Added save validation and Edit Mode test source.

### Verification boundary
- Static repository validation and C# syntax checks are required on the pull request.
- Unity 6000.3.28f1 compilation, Edit Mode execution, Greyhaven scene regeneration and Play Mode contract acceptance/submission/save-load remain outstanding.
- Rewards and three salvage jobs are prototype content, not final economy balance. Research submissions, permits and region-linked contracts remain future work.


## 2026-11-14 — Equipment progression, Harbourmaster research and expedition telemetry

- Added persistent engine, fishing-rod and deck-lantern slots. Equipping moves the item out of the spatial hold; replacing/storing equipment returns it to the hold where space permits. Slot IDs and condition reuse the existing schema-v8 `InventorySaveData.equipment` list.
- Added equipment wear: engine wear follows actual propulsion load, rod wear follows successful landings, and lantern wear accumulates during night operation. Tomas can repair installed equipment through idempotent wallet transactions.
- Applied gameplay effects: engine condition changes propulsion and fuel efficiency; rod condition changes cast distance and fishing control; lantern condition changes a real deck point-light's intensity/range. Inez's dockside panel now supports install/store actions and Tomas's panel supports servicing.
- Added Harbourmaster research submissions using confirmed Sounding Rig discoveries plus one matching salvage item. Three finite submissions grant research stipends and chart permits; permit unlocks are persisted without adding visited-region records or revealing unknown chart locations.
- Added expedition accounting that starts on propulsion and closes after the dockside service session. It records sale/contract/research receipts, operating expenses, actual fuel consumed, game-clock duration, trip net and net income/hour. Equipment purchases are excluded as capital investment. Results are surfaced in the dockside panel.
- Added Edit Mode test source for equipment installation/durability/repair/save validation, research permit persistence and expedition telemetry; updated the project validator.

**Verification boundary:** source delimiter checks pass. Unity 6000.3.28f1 compilation, Edit Mode execution, Greyhaven scene regeneration, Play Mode tests and actual starter/average/optimized economy playtests remain unrun. The telemetry provides measured campaign data once played; it does not itself prove the current economy is balanced. Full seafood catalogue, complete per-catch legality and destination-market trading routes remain open.

## 2026-11-14 — Lore foundation and six-region narrative continuity

**Branch:** `lore/foundation-20261114`  
**Source status:** Documentation-only narrative expansion. New historical details, regional names and character additions remain proposed pending project-lead review.

### Lore work
- Expanded the World Bible into the central reference for the island chain, six-region identity, the Choir, the Orison, Greyhaven's working community and open canon decisions.
- Replaced lore placeholders with linked guides for evidence terminology, island history, relative chronology, supernatural consistency, social institutions, original local folklore and plural everyday belief.
- Filled all five regional lore placeholders and the location catalogue, distinguishing named design concepts from implemented scenes and chart discoveries.
- Added a character-arc specification that preserves existing NPC IDs, schedules, Yarn nodes, relationship contracts and the unresolved fate of Nell and the Orison crew.
- Carried current resident details into their arcs: the Harbourmaster's records, Mara's kept crew crate, Tomas's radio-repair hobby and Inez's line craft. Proposed furniture motifs are explicitly not implementation claims.

### Continuity and implementation boundary
- Fixed story intervals remain eleven years from the lighthouse bell's last confirmed sounding to the present, and eight years from the Orison loss to the present; therefore the bell ceased sounding three years before the Orison incident.
- The Drowned Parish bell tradition and the lighthouse bell are treated as separate objects unless future evidence establishes a connection.
- The Choir remains a human label for an unresolved family of acoustic anomalies. Ordinary wildlife, natural ocean phenomena and legendary encounters are not automatically part of it.
- No runtime code, quest state, dialogue, save schema, scene, asset, chart marker or gameplay mechanic was changed by this documentation pass.
- The lore package remains **Proposed**. Unity compilation and runtime verification are not applicable to these documentation changes; repository validation and CI are still required.


## 2026-11-14 — Regional physical remains and wreck structure

- Extended the deterministic Greyhaven environment builder with a partial lower-inlet landing, a low former-settlement foundation outline, and exposed framing on the existing outer-channel wreck.
- Corrected estuary streaming ownership: western and eastern mudflat dressing now sit beneath distinct top-level roots at their actual world-space X positions. Incremented the streamed-area content revision to 4 so generated area scenes are rebuilt.
- Added Edit Mode regression coverage for regional root ownership, lower landing/foundation nodes and wreck framing; added corresponding static-validator contracts.
- Added a six-region physical-remains specification covering reef markers, seasonal Blackrock camps, Outer Shoals survey debris, staged Drowned Parish relocation and sparse Hollow Trench expedition traces.
- New objects remain deterministic render-only geometry: no new colliders, loot, interactions, discoveries, quest/save state or runtime scripts.
- Unity Editor generation, streamed-scene transition review, camera visibility/grounding, Play Mode tests and performance profiling remain unverified. The other five regional treatments are proposed, not implemented.


## 2026-11-14 — Historical artifacts and archival evidence

- Added six stable-ID Greyhaven records: lower-inlet working chart, repair-yard tool roll, unposted letter from the lower landing, lighthouse keeper weather ledger, glass-plate photograph, and recovered bottom recorder from the outer-channel wreck.
- Added static, collider-free visual representations beneath their owning harbour, village, estuary, lighthouse and wreck roots. Separate metadata anchors remain beneath a persistent scene root, outside the static streamed-area candidates.
- Reused the existing boat Interact and DeepOceanSiteInteractionController path. The chart, tool roll, keeper's ledger and photograph are available through existing dockside furniture menus; the lower-landing letter and recovered recorder use direct close-range inspection. DocksideHubController has deterministic input priority so menu access does not race direct inspection. Historical artifact reading remains distinct from phenomenon-site inspection: no sonar discovery, chart mark, quest flag, NPC relationship change or site-disturbance response is added.
- Added the independent `inspectedHistoricalArtifactIds` save ledger and schema v8-to-v9 migration. Legacy saves start with an empty archive ledger; existing inspected site IDs remain unchanged.
- Added Edit Mode source coverage for metadata validity, six distinct artifact types, first/repeat inspection, save/restore isolation and schema migration; extended repository validation.
- Added a cross-region archival-content specification. The five non-Greyhaven regional artifact families remain proposals, not implemented interactables.
- Unity-generated scene rebuild, visual grounding, camera readability, interaction range, Play Mode save/reload and Windows build remain unverified.

# Task Backlog

> Project: WHERE THE SOUNDINGS END  
> Status: Active M0 and first vertical-slice backlog  
> Owner: Project lead  
> Version: 0.4  
> Last reviewed: 2026-11-13

This is the authoritative implementation backlog. The [Implementation Audit](IMPLEMENTATION-AUDIT.md) records what exists and what is still unverified. The [Development Roadmap](DEVELOPMENT-ROADMAP.md) owns the release forecast; this file owns work order, dependencies and acceptance criteria.

## Planning rules

- Work in rolling two-week cycles. At 25–40 hours/week, theoretical capacity is 50–80 hours per cycle; commit less than that and reserve meaningful time for tests, integration, documentation and defects.
- Size guide: **S** = roughly 2–6 focused hours, **M** = 6–14, **L** = 14–30. These are AI-assisted planning estimates, not commitments; exclude Unity import surprises, asset production, playtest iteration and release QA.
- No task is complete because code was generated or a document exists. Complete means integrated, documented, tested at the appropriate layer, and evidence recorded.
- Do not replace existing systems without an evidenced reason. Extend the current owners and preserve save IDs/schema compatibility.
- Keep the first slice small. The six-region world and 20+ hour target are not M1 acceptance requirements.

## M0 — Foundation stabilization and verification

> **Status: M0 in progress.** Unity verification is still required.

| ID | Priority | Task | Size | Dependencies | Status |
|---|---|---|---|---|---|
| M0-01 | P0 | Resolve known Greyhaven builder semantic compile defect | S | None | Source corrected in `06a7004`; Unity compiler confirmation pending |
| M0-02 | P0 | Perform clean Unity import and lock project metadata | M | M0-01 | Open |
| M0-03 | P0 | Verify generated scenes and Build Settings bootstrap | M | M0-02 | Open |
| M0-04 | P0 | Run all 13 current Edit Mode test source files in Unity | M | M0-02 | Authored; Unity execution open |
| M0-05 | P0 | Expand and run focused Play Mode integration tests | L | M0-03, M0-04 | Greyhaven streaming test authored; Unity execution and broader coverage open |
| M0-06 | P0 | Validate boat, water and camera behavior together | L | M0-03, M0-05 | Source integration/cadence corrected; Unity acceptance open |
| M0-07 | P0 | Verify save/load, autosave, backups and participant restore | M | M0-03, M0-05 | Front-end save UI and schema v6 source present; Unity integration open |
| M0-08 | P0 | Produce and smoke-test a clean Windows 64-bit development build | M | M0-03 through M0-07 | Open |
| M0-10 | P0 | Verify dockside hub, modal exclusivity and Greyhaven additive streaming in Unity | M | M0-03, M0-04, M0-05 | Source/tests authored; Unity verification open |
| M0-09 | P1 | Decide how to enforce main-branch review/check requirements | S | Owner plan/visibility decision | Blocked by private-repository plan limitation |

### M0-01 — Resolve the known semantic compile defect

**Changed:** `Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs` now passes the already-declared `concrete` material for Inez's placeholder head instead of the undeclared `trim` identifier.

**Acceptance:**
- Unity `6000.3.28f1` imports and compiles the entire project with zero C# compiler errors.
- Greyhaven builder opens and runs without missing symbol errors.
- No other compile/import failures are hidden behind the first error.

### M0-02 — Clean import and reproducible metadata

**Work:**
- Open a fresh clone with the exact editor version and allow package resolution to finish.
- Commit `Packages/packages-lock.json` after a successful resolve.
- Commit Unity-generated `.meta` files for tracked assets/scripts and verify GUID stability across a second checkout.
- Confirm the Yarn project and Input System asset import without warnings/errors.
- Record package versions and any importer warnings.

**Acceptance:** two clean checkouts resolve the same direct/transitive package set and stable GUIDs; no missing references appear.

### M0-03 — Bootstrap and generated-scene verification

**Work:**
- Run `Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation`.
- Verify Greyhaven, Outer Shoals and Hollow Trench are generated and registered in Build Settings in the intended order.
- Re-run initialization and confirm it is safe/idempotent.
- Rebuild Greyhaven and deep-ocean sandboxes once; confirm the destructive warning protects manual scene edits.
- Decide and document whether generated scenes/assets are committed outputs or reproducible local build outputs.

**Acceptance:** the clean-clone procedure works exactly as documented and opens Greyhaven in the Editor without missing scripts or assets.

### M0-04 — Existing Edit Mode verification

Run all current Edit Mode test files in Unity Test Runner. Record the editor version, pass/fail counts, failures and commit. Fix root causes; do not disable tests to obtain a green result.

### M0-05 — Play Mode integration smoke tests

Add a Play Mode test assembly and tests for:
- vessel input/forward/reverse/braking/rudder and Rigidbody velocity;
- four-point buoyancy against the rendered/shared surface;
- camera movement lead, bank and wave response without feedback into physics;
- sea-time advance/hold behavior during movement, harvesting, dialogue and inventory;
- fishing/dredging completion, cargo capacity and salvage depletion;
- NPC schedules, dockside resident handoff, modal exclusivity, dialogue pause/resume and relationship persistence;
- save, load, autosave lifecycle and recovery.

Keep deterministic math in Edit Mode; use Play Mode only where Unity scene/runtime integration matters.

### M0-06 — Boat/ocean/camera acceptance pass

**Source-level integration update:** the source now provides continuous incident wave transitions, amplitude-aware surface velocity, a configured probe-depth cap, single-source camera heave, and shared-surface camera wiring in Greyhaven plus the two survey sandboxes. The controller resolves vessel condition after generated-scene wiring. This review also moves wave-state advancement to an early-order `FixedUpdate` with `Time.fixedDeltaTime`, then interpolates the mesh in `LateUpdate` without affecting physics samples. The collision pass now uses contact-normal damage, per-step contact aggregation, a root-level Greyhaven hull collision proxy, unchanged solver-owned collision velocities and acoustic HullImpact publication. This does not close M0-06; all handling observations and Play Mode acceptance remain open until run in Unity.

Record repeatable observations for:
- stable draft with no sinking or launching;
- small wave heave/pitch/roll without runaway rotation;
- forward/reverse acceleration, opposing-throttle braking and coasting stop distance;
- rudder response at rest, low speed, cruising speed and reverse;
- camera readability while accelerating, drifting, reversing and turning;
- actual vessel velocity driving sea-time progression;
- no major visual/physics mismatch between water mesh and buoyancy probes;
- hard impacts separate the hull from piers, breakwater rocks, terrain and wrecks without visual-collider seams;
- damage follows contact-point normal speed including angular motion, one strongest impact is applied per physics step, and glancing scrapes are not treated as head-on impacts;
- accepted impacts emit one acoustic HullImpact signal and preserve quest-critical cargo;
- stable buoyancy response with render caps at 30, 60 and 120 FPS while the fixed timestep remains unchanged;
- smooth rendered wave interpolation with no render-frame delta feeding the physics water-velocity sample.

Tune only after recording observations. Keep the small workboat approximation; do not introduce a heavyweight fluid solver without measured need.

### M0-07 — Persistence integration pass

Verify:
- five isolated manual slots and dedicated autosave;
- temp/backup recovery for both manual and autosave files;
- schema migrations from every supported older version;
- boat transform plus linear/angular velocity restore without impulse spikes;
- cargo placements, damaged cells, vessel condition, world time/weather/omens, sonar discoveries, acoustic history and NPC/Yarn state restore together;
- active-scene mismatch reports a safe, understandable failure;
- participant order is either proven independent or explicitly staged;
- application pause/focus loss, scene unload, quit and checkpoint saves do not corrupt or spam saves.

### M0-08 — Windows build smoke test

Build from the verified Unity editor and generated Build Settings. Launch on a clean Windows environment, start Greyhaven, exercise boat movement and one harvesting/survey action, save, quit and relaunch. Record build configuration, machine, results and any known limitations.

### M0-10 — Dockside and streaming Unity acceptance

Run the generated Greyhaven scene in Unity 6000.3.28f1. Rebuild the six additive area scenes, execute the streaming Play Mode test, run all dockside Edit Mode tests, verify all seven location menus, test Escape/gamepad Start close, prove inventory/dialogue/pause mutual exclusion, and verify the Inn checkpoint uses the existing autosave. Record editor version, hardware, pass/fail and any transition hitch observations.

### M0-09 — Remote repository protection decision

The repository is private, and the GitHub rulesets endpoint reported that rulesets require GitHub Pro or a public repository. Do not make the repository public or change plans automatically. Until an owner decision is made, keep least-privilege CI, code-owner files, PR templates and the local safeguard validator; treat remote required-check/approval enforcement as unverified.

## First vertical slice — Greyhaven, 45–90 minutes

**Proposed scope:** one complete, replayable expedition in Greyhaven. The slice should demonstrate the product's fishing, navigation, maritime-horror and narrative identity without building the full world first.

| ID | Priority | Task | Size | Dependencies | Status |
|---|---|---|---|---|---|
| VS-01 | P1 | Lock the slice route, objective, NPC arc and one threat | S | M0 baseline | Ready for owner review |
| VS-02 | P1 | Complete player-facing expedition HUD and onboarding | L | M0-03 | Open |
| VS-03 | P1 | Complete departure, navigation and return decisions | L | VS-02 | Open |
| VS-04 | P1 | Make Greyhaven fishing/salvage outcomes region-aware | M | M0-06 | Open |
| VS-05 | P1 | Complete sell, repair and basic upgrade/equipment loop | L | VS-04 | Open |
| VS-06 | P1 | Deliver one persistent NPC quest and evidence trail | L | M0-07 | Open |
| VS-07 | P1 | Deliver one meaningful threat encounter with counterplay | L | M0-05, VS-03 | Open |
| VS-08 | P1 | Integrate authored audio/atmosphere and accessible cues | M | VS-07 | Open |
| VS-09 | P1 | Deliver player-facing save/load and slice resume flow | M | M0-07 | Open |
| VS-10 | P1 | Assemble, build and playtest the complete vertical slice | L | VS-02 through VS-09 | Open |

### Vertical-slice acceptance

A new player can start the slice and complete this sequence without developer commands or debug-only shortcuts:

1. Understand controls, objective and basic vessel condition.
2. Prepare the boat and choose a simple gear/loadout decision.
3. Depart Greyhaven and navigate using landmarks, chart/instrument cues and the sea state.
4. Fish, salvage or survey at a meaningful location; understand why the result was available there.
5. Encounter one authored threat/anomaly with a readable warning, a fair response window and at least one viable counterplay/escape route.
6. Return to harbour with cargo and a vessel condition that matters.
7. Sell or otherwise resolve cargo, repair the vessel and receive a clear progression reward.
8. Complete one NPC/quest beat and retain its clue/state.
9. Save, quit, reload and continue without losing critical progress.
10. Finish in approximately 45–90 minutes, then provide feedback on clarity, pacing, fear, navigation and the expedition loop.

**Slice boundaries:** Greyhaven plus one nearby fishing/incident area; one quest/NPC arc; one meaningful threat; a small fish/salvage selection; one market/repair service; one complete save/resume path. Defer all six-region completion, full fish and creature counts, full population ecology, every legendary encounter, multiple endings and the 20+ hour content target until the slice has been tested.

## Recommended cycle order

1. **Cycle A — M0 import and compile:** M0-01 to M0-04. Resolve editor/package/asset issues before adding mechanics.
2. **Cycle B — M0 integration:** M0-05 to M0-08. Stabilize movement, clock, scene behavior and persistence; produce the first Windows build.
3. **Cycle C — Slice playability:** VS-01 to VS-04. Establish the Greyhaven route, player feedback, navigation, fishing and salvage.
4. **Cycle D — Consequence and story:** VS-05, VS-06 and VS-09. Complete the return economy, resident arc and save/resume path.
5. **Cycle E — Horror and validation:** VS-07, VS-08 and VS-10. Add the threat/audio layer, then playtest and re-scope from evidence.

## Definition of done for every task

- Owning design/technical documents reflect the implemented behavior.
- Existing architecture and stable IDs/save compatibility are preserved.
- Edit Mode tests cover deterministic rules; Play Mode tests cover Unity integration where applicable.
- Available repository checks are run and their output recorded.
- Unity compilation, gameplay tests and builds are reported only when actually executed.
- Development log records material changes, test evidence, known gaps and next actions.

## Greyhaven narrative content pass — 2026-11-13

**Source status:** Dialogue and lore expansion authored; Unity/Yarn verification open.

- Extend all four existing resident nodes rather than introducing replacement NPCs or duplicate dialogue infrastructure.
- Add four shared Yarn bool discovery flags and a cross-witness comparison topic. Keep the synthesis cautious and avoid confirming the cause of the Choir.
- Add occupation-specific personal details while keeping core evidence available at neutral-or-better trust.
- Preserve stable NPC IDs, relationship event memory, Yarn command contracts, save schema v6 and dockside routing.
- **Acceptance still required:** compile the Yarn project in Unity 6000.3.28f1; verify all four clue options and partial/complete comparison branches; save after each clue and confirm flags restore; test neutral/low-trust access and Familiar personal-topic gates; review line pacing and UI overflow in Play Mode.

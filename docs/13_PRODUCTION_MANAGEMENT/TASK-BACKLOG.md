# Task Backlog

> Project: WHERE THE SOUNDINGS END  
> Status: Active M0 and first vertical-slice backlog  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

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
| M0-04 | P0 | Run the full existing Edit Mode suite | M | M0-02 | Open |
| M0-05 | P0 | Add and run focused Play Mode integration tests | L | M0-03, M0-04 | Open |
| M0-06 | P0 | Validate boat, water and camera behavior together | L | M0-03, M0-05 | Open |
| M0-07 | P0 | Verify save/load, autosave, backups and participant restore | M | M0-03, M0-05 | Open |
| M0-08 | P0 | Produce and smoke-test a clean Windows 64-bit development build | M | M0-03 through M0-07 | Open |
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

Run all seven existing Edit Mode test files in Unity Test Runner, including the current 59 `[Test]` attributes and four `[TestCase]` attributes. Record the editor version, pass/fail counts, failures and commit. Fix root causes; do not disable tests to obtain a green result.

### M0-05 — Play Mode integration smoke tests

Add a Play Mode test assembly and tests for:
- vessel input/forward/reverse/braking/rudder and Rigidbody velocity;
- four-point buoyancy against the rendered/shared surface;
- camera movement lead, bank and wave response without feedback into physics;
- sea-time advance/hold behavior during movement, harvesting, dialogue and inventory;
- fishing/dredging completion, cargo capacity and salvage depletion;
- NPC schedules, interaction, dialogue pause/resume and relationship persistence;
- save, load, autosave lifecycle and recovery.

Keep deterministic math in Edit Mode; use Play Mode only where Unity scene/runtime integration matters.

### M0-06 — Boat/ocean/camera acceptance pass

Record repeatable observations for:
- stable draft with no sinking or launching;
- small wave heave/pitch/roll without runaway rotation;
- forward/reverse acceleration, opposing-throttle braking and coasting stop distance;
- rudder response at rest, low speed, cruising speed and reverse;
- camera readability while accelerating, drifting, reversing and turning;
- actual vessel velocity driving sea-time progression;
- no major visual/physics mismatch between water mesh and buoyancy probes.

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

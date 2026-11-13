# Fishing System

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime foundation implemented; Unity playtest verification open  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-14

## Player promise

Fishing is a deliberate boat-side activity: choose tackle and method, cast into a suitable ground, read the bite, hook the fish, manage a multi-beat fight, land and identify the catch, then secure it in the spatial cargo hold. A catch is not a generic random item and a fight is not a single timing press.

## Encounter lifecycle

1. **Loadout:** select rod, line, hook, reel, bait and technique before casting.
2. **Casting/deployment:** cast distance is affected by rod, line and method. The landing point is used to select a fishing habitat.
3. **Waiting:** an action-driven fishing activity advances the existing sea clock. The bite wait is variable.
4. **Bite and hook:** an eligible species is selected using local stock, habitat/depth, method preference and bait preference. The player must respond during the hook window.
5. **Fight:** several timed pulls are required. Species resistance and burst frequency affect tension. Good timing tires the fish and advances retrieval; poor timing loses progress and raises tension. Rod, line, hook and reel affect control and safe tension.
6. **Landing:** catch quality is derived from successful pulls and final line tension. Species-specific size bounds determine the displayed catch size.
7. **Cargo:** the catch uses the existing spatial hold and stores its quality/freshness fields. A full or damaged hold can cause the fish to escape.

F begins a cast. Space / Enter or the mapped gamepad Harvest Beat action is used to hook and fight. The in-game loadout panel provides mouse selection of equipment, bait and method. G continues to invoke the separate dredging minigame.

## Equipment

The initial runtime loadout contains three rod, line, hook and reel tiers. Better tackle is not a universal catch-rate multiplier:

- rods alter casting distance, control and tolerance;
- lines alter cast reach and tension tolerance;
- hooks alter hooking/fight control;
- reels alter tension tolerance and retrieval progress;
- bait changes species weighting;
- techniques alter casting distance and must suit the vessel state and species.

The starter tackle is available immediately. The second tier unlocks after five landed fish and the third after twelve. Unlocks and selected loadout are stored using stable fishing-prefixed progress flags in the existing save DTO. These are prototype milestones and require balance/playtest review.

## Techniques

The current Greyhaven prototype supports still fishing, bottom fishing, float fishing, jigging, trolling, drifting and handlining. Trolling requires vessel movement. Still, bottom, float, jigging and handlining require a slow/stationary vessel; drifting permits movement. Technique/species preferences affect candidate weighting rather than making every non-preferred method impossible.

## Species and habitats

Fish metadata is authored on CargoItemDefinition: habitat flags, preferred technique and bait, depth range, resistance, burst frequency, initial population, daily recovery, migration rate and catch-size bounds. Greyhaven's Editor scene builder assigns distinct profiles to the five ordinary starter fish and three anomalous specimens.

The current habitat resolver is a Greyhaven prototype heuristic based on cast position and distance from the settlement. Depth is an authored habitat estimate, not live bathymetry. It must be replaced by a region-owned fishing-ground/depth data source before expanding beyond Greyhaven.

## Population model

The runtime tracks aggregate per-species stocks per habitat, not individual fish GameObjects. A landed fish removes one unit from the local stock. Stocks recover by the authored daily recovery amount (bounded at twice the initial population). Every six in-game hours, a bounded share may migrate to the next eligible habitat in the prototype habitat cycle. Population values, simulation timestamps and migration clock are saved through existing ProgressSaveData.regionalStocks and fishing-prefixed worldFlags; no save schema bump is required.

This is an intentionally lightweight simulation. It does not yet model spawning seasons, age cohorts, temperature/current fields, fishing mortality caps, food webs, catch quotas, or cross-region migration. Do not describe it as a complete marine ecosystem.

## Identification and quality

The landed result reports species, estimated catch size in kilograms and a quality grade. Quality is persisted through the existing cargo placement quality value. The current prototype does not yet store per-catch measured length/weight, fish age, sex, protected status, market freshness decay presentation or research journal identification; these remain follow-up work.

## Cancellation and failure

- Retrieving the line manually ends the active encounter without a catch.
- Missing the bite returns to the waiting phase; the bait remains deployed.
- Missed fight beats reduce progress and increase tension.
- Exceeding the tackle's safe tension snaps the line and loses the fish.
- The fish escapes after the fight outlasts the configured beat budget.
- If cargo cannot accept the fish, it is not awarded.
- Pause/dialogue interruption retrieves the line and releases the action-driven clock lock.

## Persistence and ownership

FishingSystemDirector is attached to the persistent vessel and is the owner of fishing phase, loadout, catch count and aggregate stocks. HarvestingMinigameController routes F to the fishing director while retaining ownership of dredging. SeaStateDirector remains the owner of action-driven game time and interaction exclusivity. SpatialCargoHold remains the owner of catch placement. No streamed scenery root owns fishing state.

## Verification and limitations

Edit Mode policy tests cover technique matching, migration bounds, depth eligibility, catch quality and catch-size bounds. CI/static checks are not Unity runtime proof. The project still requires Unity 6000.3.28f1 import/compile, all Edit Mode tests, a generated Greyhaven rebuild, Play Mode tests for input/encounter/cargo/save-load, and Windows Development Build profiling.

## Acceptance criteria

- Loadout selection cannot equip a tier before its catch milestone.
- Each supported technique obeys its vessel movement preconditions.
- Cast location determines a consistent habitat and depth estimate.
- Ineligible species cannot be selected.
- Bait and technique preferences affect candidate weighting.
- Bite, hook and multi-beat fight transitions are repeatable and cancellable.
- Tension failure and fish escape are possible; successful play is not guaranteed.
- Species profiles produce distinct resistance, burst, size and habitat behavior.
- Catch quality and stock depletion are reflected in cargo and saved regional stocks.
- Daily recovery and six-hour migration are bounded and deterministic for a given simulation state.
- Existing dredging behavior and save schema remain compatible.
- The entire sequence is demonstrated in Unity Play Mode before this system is called verified.

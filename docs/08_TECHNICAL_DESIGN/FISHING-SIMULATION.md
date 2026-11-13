# Fishing Simulation Runtime

> Project: WHERE THE SOUNDINGS END  
> Status: Source implementation; Unity verification pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-14

## Runtime ownership

- FishingSystemDirector lives on the persistent boat and implements ISaveParticipant.
- HarvestingMinigameController forwards the Fish action to the fishing director and continues to own dredging.
- SeaStateDirector owns action-driven game time and interaction claims.
- SpatialCargoHold owns physical cargo placement and quality/freshness values.
- CargoItemDefinition owns the authored species profile; non-fish items ignore fish-only fields.
- FishingSimulationPolicy contains deterministic rules that can be tested without a scene.

No additional package, scene root, ScriptableObject dependency or save schema migration is introduced.

## Encounter states

Idle → WaitingForBite → HookWindow → Fighting → Idle

A missed bite returns to WaitingForBite once; a second missed strike spooks the fish and ends the deployment. Cancellation, line snap, fight timeout, manual retrieval, pause or dialogue interruption returns to Idle. Fishing sets the existing harvest activity flag for its entire deployed-line lifecycle. Dredging retains its existing reservation/commit/release contract.

## Authored species data

Fish-only CargoItemDefinition fields include habitat flags, preferred technique/bait, minimum/maximum habitat depth, resistance, burst frequency, initial stock, daily recovery, migration rate and minimum/maximum catch size. Greyhaven's deterministic Editor builder writes distinct profiles to the ordinary and anomalous starter species assets. Rebuilding Greyhaven updates these assets; it does not create a second catalogue.

## Habitat and ecology approximation

The current Greyhaven habitat resolver classifies the cast point as estuary, nearshore, kelp, open water or deep water from local coordinates/distance. It maps each habitat to a fixed estimated depth, then applies technique-specific depth multipliers (bottom deeper, float shallower, with intermediate values for jigging/trolling/handlining). Bite wait timing is also method-adjusted. This is deliberately a prototype heuristic because the current ocean surface and survey bathymetry systems do not expose a shared fishing-depth query. Do not use these estimates as nautical depth data.

Population is aggregate integer stock keyed by species and habitat. Each landed fish removes one stock unit. Recovery is applied in elapsed in-game-day increments and capped at twice initial stock. Every six in-game hours, a bounded fraction migrates to the next habitat in the authored habitat order that the species can inhabit. Migration is a simplified cyclic transfer, not a biologically validated route. All stock and timing values use existing save collections under namespaced IDs.

## Gear and progression

The first three gear tiers are code-authored prototype choices. Tier two unlocks at five lifetime catches; tier three at twelve. Rod/line/hook/reel stats affect cast range, tension tolerance, control or retrieval progress. Species resistance narrows hook/fight timing windows; tackle control widens them only modestly. Fight timing windows become tighter as the encounter progresses, fish burst loads are sharper, and missed pulls cost more progress while raising tension. Selected gear, bait, technique and lifetime catch count are persisted through namespaced ProgressSaveData.worldFlags. No global inventory/economy purchase flow is implied; the current unlock milestone is a fishing-specific prototype progression.

## Known follow-up work

- Replace GUI.OnGUI loadout/encounter presentation with the project's UI Toolkit HUD conventions.
- Replace coordinate habitat heuristic and fixed depths with authored fishing-ground volumes and depth/bathymetry queries.
- Add time-of-day, season, weather, current, temperature and tide preferences.
- Add real cast power/aim input, line trajectory, bait deployment and line retrieval animations/audio.
- Add per-catch instance metadata for measured length, weight, freshness and identification/research status.
- Add more technique-specific control patterns, species escape strategies, hook damage, tackle wear and repair.
- Tune the hard skill curve with controller/keyboard playtests; current timing is deliberately demanding but not yet player-validated.
- Validate population recovery/migration against an explicit ecological design model and add deterministic random streams.
- Integrate fishing gear purchases/upgrades with the future Greyhaven economy and market NPC.
- Add Play Mode tests for input routing, action-clock pause, encounter cancellation, cargo-full failure, save/restore and existing dredging regression.

## Verification

Edit Mode policy tests are authored. Unity compilation, Edit Mode execution, Play Mode integration, generated asset rebuild, save migration regression and Windows build are still required. Source presence and CI syntax validation must not be represented as a playable or fully balanced fishing simulation.

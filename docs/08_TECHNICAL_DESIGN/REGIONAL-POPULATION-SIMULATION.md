# Regional Population Simulation — Unity Architecture

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed technical specification — no runtime implementation implied  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10  
> Engine baseline: Unity 6 LTS + C# + URP (exact editor patch to be pinned at project initialization)

## 1. Purpose

Implement persistent, regional wildlife populations that respond to fishing, natural growth, breeding, predation, migration, habitat condition, and time. A fishing ground must remember what the player removed. Living creatures must grow between visits. A heavily harvested ground must become less productive, while a healthy connected ground can support migration and recovery.

This document defines runtime boundaries and data contracts. It is not executable code and does not claim that the Unity project/runtime has been created.

The authoritative gameplay formulas and balance rules live in [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md).

## 2. Simulation architecture

Recommended services:

| Service | Responsibility |
|---|---|
| `WorldClock` | Owns the authoritative in-game timestamp and game-day index |
| `EcologyTickScheduler` | Runs one idempotent update for each elapsed ecology day |
| `RegionalEcologyService` | Queries and updates regional stock, habitat, migration, and recovery |
| `PopulationRepository` | Loads / saves population records by stable population key |
| `PopulationGrowthSystem` | Ages cohorts and advances size / weight |
| `RecruitmentSystem` | Schedules breeding opportunities, settlement delay, and cohort creation |
| `FishingRemovalResolver` | Converts resolved catches / bycatch into cohort removals |
| `MigrationSystem` | Transfers cohorts between connected habitat patches without duplication |
| `PredationResolver` | Removes prey stock after a successful wildlife feeding event |
| `HabitatConditionSystem` | Tracks nursery, spawning, feeding, and patch damage / recovery |
| `PopulationPresentationAdapter` | Converts abundance into school density, encounter weighting, and survey feedback |

The systems should communicate through typed events. Example event names: `CatchResolved`, `FishingMortalityApplied`, `SpawnWindowOpened`, `RecruitmentSettled`, `CohortMatured`, `DailyGrowthApplied`, `MigrationTransferred`, `PredationConsumed`, and `HabitatDamaged`.

A catch event has a stable ID. Applying the same event twice must not remove stock twice.

## 3. Authoring data: ScriptableObjects

Use ScriptableObjects for authored, immutable definitions:

### 3.1 SpeciesLifeHistoryDefinition

Required fields:
- Stable catalogue species ID.
- Population model family: fish cohort, crustacean molt, cephalopod, bivalve / echinoderm, algae patch, or authored exception.
- Growth coefficients and maximum size / weight parameters.
- Maturity size / age and cohort transition rules.
- Natural mortality and predator-vulnerability profile.
- Spawning windows, breeding gates, recruitment delay, settlement survival.
- Seasonal habitat and nursery suitability.
- Migration routes / home range.
- Gear selectivity by size and life stage.
- Recovery profile and any species-specific overrides.

### 3.2 FishingGroundDefinition

Required fields:
- Stable ground ID and parent region.
- Polygon / volume or authored fishing-area bounds.
- Habitat type, depth band, substrate, structure, current exposure.
- Carrying capacity and seasonal stock reference.
- Nursery / spawning / feeding suitability.
- Neighbour ground IDs and migration route IDs.
- Habitat-damage and recovery parameters.
- Valid species list and legal closures.

### 3.3 RegionalPopulationDefinition

Defines which species use which grounds, starting abundance, cohort proportions, seasonal offsets, and initial condition. Initial population values should be authored by a deterministic scenario seed or fixed data set, not rolled anew every time a scene loads.

Do not store changing population numbers in the ScriptableObject.

## 4. Mutable runtime state

Runtime state belongs in serializable records, not shared ScriptableObject assets.

### 4.1 PopulationStockState

Suggested fields:
- `PopulationKey` (species ID + ground ID).
- `LastProcessedGameDay`.
- `TotalAbundance` and total biomass.
- Cohort records keyed by stage / age or size band.
- Spawning-stock biomass and recruitment buffer.
- Rolling 14-day fishing-removal and natural-production summaries.
- Seasonal reference abundance / vulnerable biomass.
- Habitat quality / nursery condition.
- Population status and last status-change day.
- Pending migration / reproduction event IDs.

### 4.2 PopulationCohortState

Suggested fields:
- Cohort ID and origin event ID.
- Life stage.
- Age in game days.
- Abundance.
- Mean length / size and weight.
- Condition / reproductive fraction if required.
- Next molt or maturation boundary.
- Last processed game day.

For common schooling fish, aggregate individuals into a small number of age / size bands. A cohort may split only when a meaningful gear selectivity or maturity boundary is crossed. Do not create thousands of per-fish objects.

## 5. Daily update pipeline

Process the following in a stable order for each elapsed game day:

1. Snapshot the day's environmental inputs (season, temperature, tide summary, current, habitat condition).
2. Advance age and species-specific growth in each living cohort.
3. Resolve scheduled molt, maturity, and cohort transitions.
4. Resolve natural mortality and wildlife predation events.
5. Resolve eligible spawning opportunities and add offspring to a delayed recruitment buffer.
6. Settle offspring whose species-specific delay has completed into juvenile cohorts.
7. Apply migration transfers between connected grounds.
8. Recalculate abundance, biomass, spawning stock, catchable stock, and status labels.
9. Store the processed day and deterministic event IDs.
10. Publish presentation updates for nearby schools, sonar surveys, maps, and NPC market reports.

Catches occur immediately when the player lands or loses a fish; do not defer a catch removal until the next daily tick. The daily pipeline handles the subsequent day-to-day biology, not delayed application of player actions.

If several days pass while the player sleeps or fast-forwards, process every day in chronological order. A catch removed on day 4 must not be restored by a day-5 update unless valid growth / recruitment / migration actually adds population.

## 6. Catch resolution contract

When a catch is confirmed:
1. Resolve the exact species ID and fishing-ground ID.
2. Determine landed weight, estimated individual count, size class, and gear type.
3. Apply gear selectivity to choose the affected cohort(s).
4. Remove the corresponding abundance and biomass immediately.
5. Record bycatch, release mortality, and retained catch separately.
6. Recalculate vulnerable stock and the next catch opportunity.
7. Emit one stable `FishingMortalityApplied` event.

A catch from one fishing ground cannot reduce the stock of every ground in the island. A catch cannot remove more fish than the cohort currently contains. Illegal or protected catch still affects the ecology if it causes mortality; legal eligibility controls sale, not whether the animal biologically existed.

## 7. Catch-rate and spawning adapters

Expose read-only queries to fishing and encounter systems:

- `GetVulnerableBiomass(speciesId, groundId, gearProfile)`
- `GetAbundanceCatchMultiplier(speciesId, groundId)`
- `GetSpawningStockRatio(speciesId, groundId)`
- `GetPopulationStatus(speciesId, groundId)`
- `GetExpectedCatchPerEffort(speciesId, groundId, gearProfile, conditions)`

Fishing encounter probability should use the abundance multiplier once. Avoid applying the same stock penalty to both bite chance and final landed quantity unless each term has a distinct, documented purpose; otherwise depletion is double-counted.

Spawn events use species-specific breeding windows and spawning-stock feedback. A spawn event creates a delayed recruitment record, not instant catchable adults.

## 8. Regional and inactive simulation

- Nearby grounds: update local movement and active school presentation as required; the authoritative stock remains aggregate.
- Nearby non-visible grounds: use the daily cohort model without full scene objects.
- Inactive islands: run coarse but deterministic day ticks; do not instantiate their fish schools.
- Distant archipelago regions: use aggregate cohort transfers and stock updates only.
- Named, quest-critical creatures: maintain dedicated persistence rules where the narrative requires them; do not let ordinary stock mortality delete them.
- Cosmetic schools: no gameplay stock effect and no consumption of gameplay RNG.

All grounds use the same authoritative state whether loaded or unloaded. Loading a scene must never reset its stock to the authored starting values.

## 9. Save / load and deterministic integrity

Persist:
- Current game-day index and last processed ecology day.
- Every changed PopulationStockState and cohort.
- Recruitment buffers and scheduled event IDs.
- Habitat condition.
- Migration transfers in progress if the game can save mid-transfer.
- Recent catch / mortality event IDs needed to reject duplicates.
- Simulation version number for migrations of save data.

Daily random choices must be derived from stable inputs such as save seed, population key, species ID, and game-day index. Do not use a global random stream whose result changes when unrelated cosmetic effects spawn.

When loading an older save, provide versioned migration defaults. Never reset an established depleted ground simply because a new field was added.

## 10. Performance budget and LOD

Population simulation is data-oriented and runs on scheduled ticks. Unity GameObjects represent nearby visible actors, not every simulated animal.

Suggested first prototype:
- 1 island.
- 3 fishing grounds with connected migration.
- 1 fast-cycle fish, 1 moderate fish, and 1 slow fish.
- 3 life stages per species.
- 1 predator-prey relationship.
- 1 habitat-damage and recovery loop.
- 30–180 in-game days of automated testing.

Only increase species coverage after catch removal, daily growth, recruitment, migration, persistence, and catch-rate feedback pass the tests.

## 11. Debugging and telemetry

A developer-only population inspector should display:
- Species and ground IDs.
- Total abundance, biomass, and cohort split.
- Mean length / weight and daily growth delta.
- Spawning-stock biomass and next breeding window.
- Recruitment buffer and settlement date.
- Current catch-rate multiplier.
- 14-day fishing removals and natural production.
- Habitat condition and migration flows.
- Last processed day and recent event IDs.

Log every stock-changing event with old value, delta, new value, source, game day, and unique event ID. This makes an apparent population reset or duplicated catch diagnosable.

## 12. Technical acceptance tests

- A catch changes only the correct ground and species.
- Replaying a catch event does not double-remove stock.
- A cohort gains age and size once per game day, including while its scene is unloaded.
- Growth raises biomass but does not raise abundance.
- Juveniles mature only at authored thresholds.
- Spawned offspring remain unavailable until the settlement / maturation delay.
- A heavily harvested adult cohort produces fewer future recruits.
- Catch rates respond to abundance and return to baseline only through valid recovery.
- Migration removes the same quantity from the source that it adds to the destination.
- Save/load and time skipping produce the same population state as uninterrupted simulation.
- Cosmetic school spawning cannot change stock or gameplay RNG.
- Quest-critical named wildlife survives aggregate stock updates unless an authored story event says otherwise.

## 13. Related specifications

- [Fish Population and Food-Web Model](../07_GAME_RULES_AND_BALANCING/FISH-POPULATION-MODEL.md)
- [Creature life cycles and ecological behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md)
- [Regional ecology and creature distribution](../05_WORLD_AND_LEVEL_DESIGN/REGIONAL-ECOLOGY-AND-CREATURES.md)
- [Balancing and simulation](../07_GAME_RULES_AND_BALANCING/BALANCING-AND-SIMULATION.md)
- [Economy mathematics](../07_GAME_RULES_AND_BALANCING/ECONOMY-MATHEMATICS.md)

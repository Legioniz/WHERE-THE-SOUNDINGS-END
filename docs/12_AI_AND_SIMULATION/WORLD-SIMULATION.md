# World Simulation and Event Scheduler

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## 1. Simulation layers

Separate the world into four update layers:

1. **Frame presentation:** camera, particles, audio playback, animation, UI. May run each rendered frame.
2. **Fixed gameplay simulation:** boat physics, collision, current forces, damage resolution.
3. **Throttled AI:** perception, decisions, creature steering, NPC decisions. Runs at a defined cadence.
4. **Scheduled world events:** tides, weather transitions, migration, rare signals, seasonal events, quest timers. Runs only at event boundaries.

Do not update every fish, NPC, or threat at full frame rate while its region is inactive. Use region activation and coarse time advancement for off-screen simulation.

## 2. Event record

Every scheduled or player-generated event should have a stable ID and typed payload:

```csharp
public readonly struct WorldEvent
{
    public readonly string EventId;
    public readonly string RegionId;
    public readonly string EventType;
    public readonly long SimulationTick;
    public readonly string SourceId;
    public readonly ulong RandomKey;
}
```

The runtime may use richer typed C# records/classes; this compact form establishes the data contract.

## 3. Event lifecycle

Dormant → Eligible → Evaluated → Scheduled → Active → Resolved → Cooldown / Completed

- **Eligible:** all story, region, and time conditions are true.
- **Evaluated:** chance is rolled once for the current opportunity ID.
- **Scheduled:** the event has a start time and location.
- **Active:** player can perceive or interact with it.
- **Resolved:** success, failure, retreat, or expiry is recorded.
- **Cooldown / Completed:** replay rules are applied.

Save state at transitions that can affect progression. On load, an already evaluated opportunity must not roll again.

## 4. Deterministic event opportunity IDs

Use explicit IDs rather than “random chance every 10 seconds”. Examples:
- rig_ping:outer_shoals:quest_voice:ping_03
- tide_turn:channel_guardian:cycle_12
- night_listen:hollow_trench:night_08
- region_entry:reef_answers:voyage_04

Opportunity IDs make events testable and prevent save/load rerolls.

## 5. Timing and scheduling

Prototype time rules:
- World time advances on the game's simulation clock, not wall-clock time.
- Weather transition checks: every 5 in-game minutes.
- NPC schedule changes: on authored schedule boundaries.
- Threat perception: 4 Hz nearby, 1 Hz in an active region at long distance, paused outside active simulation.
- Wildlife / fish movement: 0.5–2 Hz depending on importance and proximity.
- Cosmetic water particles are independent of simulation and must not alter gameplay.

Use a priority queue or sorted event list for future scheduled events. Cancel or reschedule by stable event ID when quest state changes.

## 6. Randomness and reproducibility

Use independent streams for:
- fishing and catch outcomes;
- fish population / migration;
- hostile threat selection;
- ambient anomalies;
- cosmetic variation.

Gameplay decisions must not change because a particle effect consumed a random value. Use a versioned PRNG and stable event key. Cosmetic randomness can be non-deterministic because it must never affect gameplay.

## 7. Quest event conditions

A quest event definition needs:
- event ID;
- required quest flags;
- required region and bounds;
- allowed time, tide, weather, and depth;
- opportunity boundary;
- chance or deterministic guarantee;
- cooldown;
- location selection rule;
- warning cue;
- success / failure / retreat outcomes;
- save-state key;
- accessibility cue.

Main story and required quest evidence should never be permanently gated by RNG. Randomization may vary approach, audio variant, or discovery order after the mandatory clue is secured.

## 8. Offline / region-inactive simulation

When the player leaves a region:
- advance fish populations and scheduled tide/weather states in coarse steps;
- resolve only events that have explicit off-screen outcomes;
- do not run local creature chase or attacks;
- preserve unresolved quest triggers until the player returns;
- never let a background simulation silently destroy a quest-critical item or NPC.

## 8.1 Creature schedules and life-cycle boundaries

Creature schedules use simulation time. A species profile may define daylight, night, dawn/dusk, tide-linked, weather-linked, or depth-linked activity. These are suitability windows, not rigid global rules: injury, disturbance, food availability, and authored anomalies may change the selected behaviour.

Schedule boundaries should emit stable opportunities such as:
- activity_window:species_id:region_id:night_12
- rest_cycle:creature_id:cycle_04
- feeding_window:species_id:feeding_ground:cycle_09
- breeding_window:species_id:region_id:season_02
- migration_step:species_id:corridor_id:season_02

Life-state transitions are event-driven where possible. Do not poll every creature every rendered frame. Needs such as hunger and energy can be advanced in bounded time steps, while activity, migration, feeding, and breeding are evaluated at authored opportunity boundaries.

## 8.2 Inactive-region life simulation

When a region is inactive, use aggregate simulation instead of local GameObject behaviour:
- Advance regional fish stock, migration, and breeding cohorts in coarse steps.
- Advance only hunger / energy / injury values needed to choose a future state.
- Resolve ordinary feeding or predation as bounded stock changes, not full local chases.
- Resolve rest and recovery as elapsed-time outcomes.
- Preserve named and quest-critical creatures unless an explicit authored story event changes them.
- Do not run off-screen local pursuit, attack, or high-frequency anomaly movement.

For an inactive region, a creature may be represented by a stable regional record containing species ID, habitat, life phase, condition band, route / feeding-ground ID, and next scheduled opportunity. Reconstruct presentation objects only when the region becomes active.

All background life events use stable IDs and deterministic random streams. Feeding, reproduction, migration, and anomaly outcomes must apply once across save / load. Cosmetic schooling never consumes gameplay RNG.

## 9. Acceptance tests

- Loading an evaluated event never rerolls it.
- Inactive-region simulation does not run expensive local AI.
- Cosmetic RNG cannot alter catches or threats.
- Quest state transitions are idempotent.
- Event logs reproduce a test encounter from seed + opportunity ID.
- A missing schedule entry uses a safe default and records a diagnostic.

## 10. Legendary sightings and location-bound world events

The World Simulation owns the opportunity clock and deterministic event identity for rare legendary appearances. It does not independently spawn or move a legendary creature; the Threat Director selects an eligible event and the local encounter controller runs its authored phase sequence.

Use opportunity boundaries rather than frame checks:
- region / fishing-ground entry;
- voyage segment completion;
- tide turn or current reversal;
- named weather / visibility window;
- Sounding Rig deployment or qualifying pulse;
- salvage / anchor / winch tension event;
- authored lunar, seasonal, or progression window.

Every opportunity receives a stable ID such as legend_hafgufa_outer_shoals_segment_014. Persist the evaluated result so the same opportunity cannot be rerolled after a scene transition, save/load, or repeated tool input.

The scheduler must check region, depth, time, tide, weather, progression flags, global legendary cooldown, individual cooldown, safe-zone state, and the availability of warning and escape anchors. An inactive region must not simulate a lethal chase. Defer the event until the player returns or resolve only a harmless distant omen.

A rare event may first appear as a rumor, surface sign, call, instrument mismatch, or distant silhouette. A later opportunity can produce contact if the player enters the defined boundary. Do not escalate a harmless sighting to lethal contact without a new trigger and a readable warning.

All legendary events share the existing major-threat lock with The Sleeper. If a major hostile encounter is active, defer the legendary event. Harmless ambient signs may still occur only when they cannot become contact.

The encounter phases, trigger matrix, rarity bands, escape rules, and save-state contract are defined in [Legendary Maritime Encounters](LEGENDARY-ENCOUNTER-SCHEDULER.md).

## 11. Maritime anomaly scheduling contract

The research and balancing authority is [Maritime Anomaly Rules](../07_GAME_RULES_AND_BALANCING/MARITIME-ANOMALY-RULES.md). Natural phenomena and human incidents must not bypass this scheduler.

- Natural ecology events are evaluated by habitat, depth, time, tide, light, weather, and population state. A deep scattering layer is a moving biological return volume, not a moving seabed.
- Human maritime incidents are evaluated by physical context: line tension, gear condition, vessel drift, hull contact, weather, and salvage state.
- Psychological perception events require their own exposure / workload context. They must not be triggered by darkness alone.
- Fictional anomalies use the same stable opportunity IDs, deterministic RNG, cooldown persistence, and event lifecycle as other scheduled events.
- The same opportunity cannot be re-rolled after save/load or repeated player input.
- Ambient events must not become lethal attacks without the Legendary Encounter Scheduler or an explicitly authored hazard controller.
- Region-inactive simulation may advance ecological state, but cannot run local lethal pursuit or silently destroy quest-critical evidence.
- Events that affect equipment, site condition, or journal progression must persist their resolved outcome.

Do not create a separate “deep-sea scare spawner”. The existing World Simulation owns scheduling; the existing Threat Director owns hostile selection; the local phenomenon controller owns presentation and physical interactions.


## 12. Acoustic event delivery

The first runtime publisher is the Sounding Rig. It emits typed AcousticSignal metadata through AcousticEventBus; this is a gameplay event channel, not an AudioSource registry.

Listener rules:
- Subscribe on enable and unsubscribe on disable / destroy.
- Reject events from inactive regions unless an explicit aggregate simulation rule applies.
- Evaluate range, region attenuation, occlusion, background noise and listener sensitivity before assigning a hearing score.
- Route a heard signal into the creature's existing perception state machine (curiosity / investigate / ignore / flee / stalk only when species and context permit).
- Do not promote a heard pulse directly to pursuit or attack.
- Use event IDs to prevent duplicate reaction to the same pulse.
- Keep presentation audio and gameplay perception separate; muting a clip must not change AI hearing, and playing a clip must not create a gameplay signal by itself.
- Do not poll every AudioSource per creature or per rendered frame.

Current status: AcousticEventBus and Sounding Rig publication are implemented. No creature, navigation or radio listener adapter is currently connected. A future adapter must use the existing threat and creature state machines, not create a parallel acoustic threat spawner.

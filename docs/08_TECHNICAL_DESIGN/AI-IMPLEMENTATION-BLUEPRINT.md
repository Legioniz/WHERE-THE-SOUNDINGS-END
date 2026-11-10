# AI and Event Systems — Unity Implementation Blueprint

> Project: WHERE THE SOUNDINGS END  
> Status: Engineering blueprint — not yet executable project code  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## 1. Purpose

This document converts the AI and event design into Unity/C# responsibilities. It is a build plan for the runtime systems; the repository does not yet contain a configured Unity project or these runtime scripts. Do not treat the pseudocode below as a compiled implementation.

## 2. Recommended component boundaries

| Component | Responsibility | Must not own |
|---|---|---|
| WorldEventScheduler | Game-time events, tide boundaries, quest timers | Creature movement |
| ThreatDirector | Eligibility, probability, selection, cooldowns, safe spawn anchors | FOV and attacks |
| ThreatBrain | HFSM state transitions | Audio clip playback |
| ThreatPerception | Visual cone, occlusion, SoundEvent intake | Quest completion |
| ThreatMemory | Last-known position and confidence decay | Transform movement |
| ThreatMotor | Open-water steering and collision avoidance | RNG selection |
| ThreatAttack | Telegraph, one collision / damage event, recovery | Spawn selection |
| SoundEventBus | Typed sound metadata distribution | Audio playback |
| ThreatAudioPresenter | AudioSource / AudioMixer / caption mapping | AI state decisions |
| QuestEventController | Quest-specific trigger conditions and resolution flags | Global random state |
| EnvironmentResponse | Current, wake, fish response, disturbance | Direct arbitrary player damage |
| SaveCoordinator | Persist event keys, RNG state, cooldowns, quest flags | Runtime decision logic |

## 3. Suggested data assets

### ThreatDefinition (ScriptableObject)
- Stable threat ID.
- Habitat / region / depth gates.
- Total FOV angle and max visual range.
- Visual acquisition dwell and memory decay.
- Acoustic sensitivity by sound band.
- Investigate / stalk / pursuit speeds.
- Turn rate, acceleration, and water-volume constraints.
- Pursuit maximum duration and break-off distance.
- Telegraph duration, hull damage percentage, recovery, attack cooldown.
- Audio cue IDs.
- Safe-zone exclusions and allowed spawn anchors.

ScriptableObjects hold authoring data only. Each active creature gets its own runtime state object; do not mutate shared asset fields during play.

### QuestEventDefinition
- Quest ID, event ID, opportunity unit.
- Required story flags / region / bounds.
- Time, tide, weather, and depth gates.
- Probability, modifiers, and cap.
- Seed key / draw index.
- Cooldown and retry fallback.
- Event location selection rule.
- Warning / audio / UI cue IDs.
- Success, retreat, fail, and completed flags.

### SoundSourceDefinition
- Source ID and category.
- Frequency band, base loudness, duration.
- Attenuation profile and pattern.
- Reliability / known-source flag.
- Caption and direction cue.

## 4. Event evaluation pseudocode

```csharp
public bool TryEvaluate(QuestEventDefinition definition,
                        EventOpportunity opportunity,
                        WorldSnapshot world,
                        IDeterministicRng rng)
{
    if (save.IsOpportunityResolved(opportunity.Id))
        return false;

    if (!definition.IsHardEligible(world))
        return false;

    float chance = definition.CalculateChance(world);
    float roll = rng.NextFloat(opportunity.StableKey);

    save.RecordEvaluation(opportunity.Id, chance, roll);

    if (roll >= chance)
        return false;

    if (!spawnValidator.TryGetValidAnchor(definition, world, out var anchor))
    {
        save.RecordSpawnRejected(opportunity.Id);
        return false;
    }

    eventQueue.Schedule(definition.EventId, opportunity.Id, anchor);
    return true;
}
```

Production implementation should distinguish a probability roll from weighted selection when several events compete. A failed location validation cancels the opportunity; it must not repeatedly reroll for a dangerous spawn.

## 5. Visual FOV pseudocode

```csharp
bool CanSeeTarget(Transform eye, Transform target,
                  float maxRange, float totalFovDegrees,
                  LayerMask occlusionMask)
{
    Vector3 offset = target.position - eye.position;
    float distance = offset.magnitude;

    if (distance > maxRange || distance <= 0.001f)
        return false;

    Vector3 direction = offset / distance;
    float halfAngle = totalFovDegrees * 0.5f;
    float minDot = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

    if (Vector3.Dot(eye.forward, direction) < minDot)
        return false;

    return !Physics.Raycast(
        eye.position,
        direction,
        distance,
        occlusionMask,
        QueryTriggerInteraction.Ignore);
}
```

The sample is a first-pass binary occlusion check. Add the visibility score, fog transmission, and acquisition dwell from [Entity Perception and Encounter Math](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md). Run sensing at the configured cadence, not every rendered frame.

## 6. Sound perception pseudocode

Each gameplay sound emits a SoundEvent once. The bus sends it to listeners inside a broad spatial query. Each listener evaluates band sensitivity, attenuation, masking, and event age. It stores the last-known sound position and category. It does not receive the player's live transform from the event bus.

```csharp
void OnSoundEvent(in SoundEvent sound)
{
    float received = CalculateReceivedLevel(sound, transform.position);

    if (received < definition.MinimumHearingLevel)
        return;

    memory.RecordSound(sound.Position, sound.Category,
                       sound.SimulationTick, received);
    brain.TryTransition(ThreatState.Investigating);
}
```

## 7. The Sleeper attack contract

1. ThreatBrain requests an attack only when in Pursuing state and within attack eligibility range.
2. ThreatAttack chooses an authored attack lane, checks retreat / safe-zone / hull gates, then emits a telegraph event.
3. During the 2.2–3.0 second telegraph, the player can turn away or change speed.
4. At impact, resolve one collision window. Apply 12–18% starter hull damage and temporary steering instability, not an instant kill.
5. Enter recovery for at least 6 seconds and attack cooldown for 18–24 seconds.
6. ThreatBrain reassesses; if target confidence or pursuit time has expired, disengage.

Only ThreatAttack may call the boat damage service for creature damage. No audio event or quest anomaly can directly subtract hull integrity.

## 8. Update cadence

| System | Prototype cadence |
|---|---:|
| Boat physics / collision | FixedUpdate physics step |
| Nearby hostile perception | 4 Hz |
| Distant active-region perception | 1 Hz |
| Threat decision evaluation | 2–4 Hz |
| Creature steering | FixedUpdate / bounded steering |
| Fish / wildlife movement | 0.5–2 Hz |
| Tide / weather scheduling | Event boundary |
| Quest trigger evaluation | Explicit opportunity boundary |
| Cosmetic audio / particles | Presentation frame |

Use Profiler measurements before increasing cadence or adding job-system complexity.

## 9. Save-state contract

Persist:
- run seed and RNG algorithm version;
- event opportunity IDs already evaluated;
- event selection / outcome;
- active threat ID and state;
- last-known target position, memory timestamp, and confidence where continuation is required;
- attack cooldown / global quiet lock;
- tension and regional disturbance;
- quest stage and resolution flags;
- tide-cycle and seasonal-event IDs.

Do not persist a transient audio source handle or a Unity instance reference. Rebuild those from stable IDs on load.

## 10. Build order

1. Implement SoundEvent data type and event bus.
2. Implement event opportunity IDs and deterministic RNG wrapper.
3. Implement ThreatDirector with hard gates, chance, and cooldowns.
4. Implement a dummy threat with HFSM but no attacks.
5. Implement FOV, occlusion, and acoustic sensing.
6. Implement The Sleeper steering and warning state.
7. Add one telegraphed strike and recovery.
8. Integrate fog, sound presentation, and accessibility cues.
9. Integrate quest-specific environmental events.
10. Add save/load and deterministic regression tests.

## 11. Required tests

- Same run seed + same opportunity history yields same event selection.
- Replaying an opportunity after save/load does not reroll.
- Threat outside visual cone can hear a Rig ping.
- Fog reduces visual detection but not the sound-event path.
- Losing line of sight does not reveal live player position.
- Threat cannot attack through walls, safe zones, or invalid retreat space.
- Warning always precedes impact.
- Only hostile attack code can damage the boat through a creature strike.
- All twelve maritime quest entries resolve their required clue without unbounded RNG failure.

## Related AI behaviour specification

For advanced predator behaviour, evidence-based escalation, utility action selection, predictive pursuit, stalking, limited group coordination, destructive tendencies, fairness, and validation, see [AI Coordination and Hunting Logic](../12_AI_AND_SIMULATION/AI-COORDINATION-AND-HUNTING-LOGIC.md). This is a design specification; it does not mean runtime AI code has been implemented in the Unity project.


## 12. Creature life-cycle and ecology runtime

The threat components above only describe player-directed hostile encounters. Ordinary creature life behaviours use separate services defined in [Creature Life Cycles and Ecological Behaviour](../12_AI_AND_SIMULATION/CREATURE-LIFE-CYCLES-AND-ECOLOGY.md).

### Additional component boundaries

| Component | Responsibility | Must not own |
|---|---|---|
| CreatureLifeController | Daily / seasonal schedule and life-state transitions | Player boat damage |
| CreatureNeeds | Hunger, energy, health, injury, stress, reproductive readiness | Movement or target selection |
| CreatureBehaviourSelector | Rank legal life and tactical actions | Global event scheduling |
| CreaturePerception | Prey, predator, sound, light, and habitat observations | Quest completion |
| CreatureMemory | Feeding sites, habitat routes, threats, and learned disturbance | Global population stock |
| CreatureMotor | Wander, patrol, shelter, migration, and bounded movement | Feeding / stock mutation |
| CreatureFeedingController | Hunt resolution, feeding, appetite, and prey-stock event | Player-directed attack damage |
| CreatureRecoveryController | Rest, healing, and injury recovery | Instant full-health reset |
| ReproductionScheduler | Breeding windows, cohort recruitment, maturation opportunities | Individual creature locomotion |
| RegionalEcologyService | Population, migration, food availability, and off-screen aggregation | High-frequency local AI |
| AnomalyController | Authored impossible rules, evidence, and anomaly phase | Arbitrary untelegraphed damage |

### Additional authoring assets

- CreatureDefinition: stable species ID, classification, habitat, senses, locomotion limits, feature flags.
- CreatureBehaviourProfile: active periods, allowed behaviours, personality weights, resting / feeding tendencies.
- DietDefinition: prey categories, feeding grounds, food value, capture constraints.
- HabitatDefinition: region, depth, current, cover, refuge, capacity, and environmental suitability.
- LifeCycleDefinition: activity windows, seasonal gates, maturity abstraction, breeding and migration schedules.
- AnomalyDefinition: baseline behaviour, explicit violated rule, trigger, domain, repetition, evidence, constraints, resolution.

### Runtime ownership rules

1. ScriptableObjects contain immutable authoring values; runtime needs and mutable condition belong to a per-instance state object.
2. Life state, tactical action, hostile encounter state, and anomaly state are separate fields. A creature can be hungry and resting while its player encounter state remains unaware.
3. Ordinary predator feeding mutates regional prey stock only through a validated, idempotent ecology event. It never calls the player boat damage service.
4. Only the hostile attack controller can apply creature attack damage to the boat.
5. AnomalyController may override only explicitly authored life rules. It cannot bypass save integrity, safe-zone rules, or quest-critical safeguards by default.
6. Inactive regions store aggregate records and scheduled outcomes; they do not run local movement and perception loops.

### Recommended extension build order

1. One non-hostile species with active / rest windows.
2. Wandering, patrol, shelter, and return-to-habitat.
3. One prey school and one simple predator feeding event.
4. Hunger, energy, and stock updates.
5. Rest and gradual injury recovery.
6. One seasonal recruitment / breeding event.
7. The Sleeper's hostile encounter, still using the separate ThreatBrain pipeline.
8. One authored anomalous behaviour with a discoverable clue.
9. Inactive-region aggregation and save / load regression tests.

This is an architecture extension, not a claim that the Unity runtime scripts already exist. Implement and profile these systems incrementally; do not build a full ecosystem simulator before the first region prototype.

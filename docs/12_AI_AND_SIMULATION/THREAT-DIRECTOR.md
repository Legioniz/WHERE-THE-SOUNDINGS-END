# Threat Director

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Responsibility

The Threat Director decides **when an encounter opportunity may occur and which eligible event is selected**. It does not move creatures, calculate FOV, perform attacks, or directly damage the boat. Those responsibilities belong to local AI and environment systems.

## Event evaluation boundaries

Never roll chance every frame. Evaluate only at meaningful boundaries:
- player enters a region's encounter volume;
- a voyage reaches a defined time / distance interval;
- the player deploys or pings the Sounding Rig;
- a tide transition occurs;
- a quest flag changes;
- a threshold of accumulated noise / disturbance is crossed;
- an authored one-time story beat becomes eligible.

An event may be evaluated only once per boundary ID. Save the event ID and result to prevent duplicate triggers after loading.

## Eligibility gates

Every candidate must pass hard gates before RNG:
1. correct region and depth band;
2. required quest and world-state flags;
3. valid time / weather / tide window;
4. not in safe harbour, cutscene, dialogue lock, or disabled gameplay state;
5. encounter not already completed or currently active;
6. individual and global cooldowns expired;
7. valid spawn / event location exists;
8. enough warning and escape space remain.

If no candidate is eligible, create normal ambient behavior. Never force a threat merely because the list is empty.

## Chance model

For candidate (i):

[
P_i = operatorname{clamp}(P_{base,i} \, M_r \, M_t \, M_w \, M_q \, M_n \, M_c, 0, P_{max,i})
]

Where:
- (P_{base}): base probability per explicitly named opportunity;
- (M_r): region / depth multiplier;
- (M_t): time-window multiplier;
- (M_w): weather / tide multiplier;
- (M_q): quest-state multiplier;
- (M_n): player-noise / disturbance multiplier;
- (M_c): cooldown / encounter-fatigue multiplier.

All multipliers and probability caps live in authored data. Do not duplicate constants in C# code. A hard eligibility gate overrides a 0% or 100% tuning value.

For mutually exclusive ambient candidates, first roll whether *any event* occurs; then select one candidate by non-negative weights. This avoids accidentally summing several individual probabilities into a chance above 100%.

## Weighted candidate selection

[
P(i) = rac{w_i}{sum_{j=1}^{n}w_j}
]

Only eligible candidates enter the list. If total weight is zero, use the ambient fallback. Reject NaN, infinity, negative, and empty weights. Record candidates and normalized weights in debug logs.

## Tension director

Maintain a bounded tension score (T in [0,100]). Tension rises from exposed travel, repeated pings, damage, severe weather, and recent discoveries; it falls during safe travel, shelter, and time without contact.

Prototype:
[
T_{next} = operatorname{clamp}(T + G - R, 0, 100)
]
- (G): accumulated tension gain from authored events.
- (R): relaxation rate multiplied by minutes since last meaningful threat cue.

Tension changes *eligibility and weight*, not direct damage or unavoidable spawn position. High tension increases warning / investigation events before attacks. It must not create an infinite escalation loop.

## Anti-frustration rules

- One hostile encounter at a time in the MVP.
- After a hostile encounter ends, start a global quiet interval of 90–150 game seconds.
- After an attack, prohibit new hostile selection for at least 120 game seconds.
- If hull integrity is below 25%, halve hostile encounter weight and prefer warning-only events.
- Never select a spawn point inside the player's immediate attack radius.
- A player can always retreat toward an authored safe route.
- Main-story quest events use deterministic eligibility and a guaranteed fallback, not a low random chance.

## Prototype encounter opportunities

| Candidate | Opportunity unit | Base chance | Hard conditions |
|---|---|---:|---|
| The Voice signal anomaly | Each valid Sounding Rig deployment | 0.18 | Outer Shoals, quest clue found, fog or night |
| Sleeper investigation | Each eligible high-noise event slot | 0.12 | Hollow Trench / deep band, threat not on cooldown |
| Channel current surge | Each tide transition | 0.20 | Channel entered, adverse tide state |
| Reef echo anomaly | Each valid Rig ping inside reef bounds | 0.25 | Quest active, correct range |
| Long Call wildlife event | Each night-time listening window | 0.15 | Trench edge, whale season flag |

Numbers are initial tuning targets, not final player-facing odds. Quest completion should not depend on winning a probability roll: after three eligible failures, offer a deterministic clue or guaranteed next trigger.

## Randomness contract

Use a dedicated deterministic RNG stream for threat events, separate from fishing, loot, world scheduling, and cosmetic ambience. Persist its state or derive draws from a stable tuple such as:

`Hash(runSeed, regionId, voyageIndex, opportunityId, eventId)`

The exact PRNG algorithm and version must be recorded in the save metadata. Do not use a shared global random stream for gameplay-critical choices.

## Spawn placement

Choose from authored spawn anchors with checks:
- outside player collision and attack volumes;
- not inside visible empty water immediately beside the boat;
- valid depth and region;
- no impossible rock / island overlap;
- between 1.5 and 2.5 times the threat's telegraph travel distance from the player for a first warning;
- not on the direct return route unless an escape lane exists.

The threat should approach through the water, not appear beside the player.

## Save and telemetry

Persist active event ID, event state, opportunity ID, cooldown timestamps, tension, and quest flags. Debug log: seed, opportunity, eligible list, probability, roll, selected event, spawn anchor, and failure reason.

## Tests

- No rolls in safe harbour or while input is locked.
- Duplicate event boundary is ignored.
- Candidate probability never exceeds its cap.
- No eligible candidate produces ambient fallback.
- Same seed + same opportunity history reproduces the selection.
- Quest-critical clue is reachable after the configured retry limit.
- Damaged boat and recent attacks reduce, rather than increase, hostile pressure.

# Probability and RNG

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Randomness contract

Randomness is used to vary a fair system, not to decide whether the player is allowed to understand a required mechanic. Every probability declares its opportunity unit: per cast, Rig ping, region entry, tide transition, voyage, night window, or eligible threat slot.

Never roll gameplay chance every frame. Filter eligibility first, draw once at a named event boundary, and persist the outcome.

## 2. Independent streams

Use separately seeded streams for:
- fishing / catch;
- fish ecology;
- hostile threat selection;
- quest and anomaly variation;
- loot;
- cosmetic ambient variation.

Cosmetic effects must not consume the threat or fishing stream. Record the RNG algorithm version in save metadata. A small project-owned PCG32 implementation or other versioned generator is preferred over relying on a shared global Unity random state.

Stable draw key:

`Hash(runSeed, subsystemId, regionId, opportunityId, drawIndex, algorithmVersion)`

## 3. Chance and weights

First evaluate all hard eligibility conditions. Then compute:

[
P = operatorname{clamp}(P_{base}prod_k M_k, 0, P_{cap})
]

Roll one uniform value (u in [0,1)). The event occurs if (u < P).

For candidate selection:

[
P(i) = rac{w_i}{sum_j w_j}
]

Candidates with invalid or zero weights are removed. Empty candidate sets use a deterministic fallback.

## 4. Retry / guarantee rules

Quest-critical evidence uses a guaranteed path. Optional encounter variation can use chance. When an optional clue is missed repeatedly, use a bounded fallback:
- after 3 failed eligible opportunities, increase the next opportunity chance;
- after 5 failed eligible opportunities, guarantee a non-hostile clue or alternate evidence route.

The player must not be forced to repeat a long trip endlessly for a required clue. This guarantee does not force a hostile encounter; it guarantees the information needed to progress.

## 5. Probability reporting

A design entry should state:
- probability value;
- denominator / opportunity unit;
- hard gates;
- modifiers and cap;
- cooldown;
- guaranteed fallback;
- whether player can intentionally create the opportunity.

Example: `P = 0.18 per eligible Rig deployment` does **not** mean 18% per second or 18% per frame.

## 6. Validation

For each probability system:
- test zero, one, and maximum probabilities;
- test invalid weights;
- test fixed-seed repeatability;
- run Monte Carlo distributions (10,000+ draws) and compare observed rates with expected tolerance;
- verify no quest lock from unlucky draws;
- verify changing cosmetic RNG does not change gameplay outcomes;
- inspect event logs from real playtests.

See [Threat Director](../12_AI_AND_SIMULATION/THREAT-DIRECTOR.md) and [Entity Perception and Encounter Math](ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md).

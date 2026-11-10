# Threat Director Logic

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

The Threat Director schedules opportunities; local entities own perception and behavior. This division is mandatory.

## Event pipeline

`Opportunity → Hard Eligibility → Probability Roll → Weighted Event Selection → Spawn / Activate → Warning → Local AI → Resolve → Cooldown`

1. Opportunity is raised by a stable event boundary (Rig ping, tide change, region entry, voyage interval, quest state).
2. Hard gates remove invalid candidates before any random draw.
3. A probability is rolled once for the opportunity.
4. If multiple eligible events remain, select one by weighted random selection.
5. Validate a safe and readable event location.
6. Issue an early cue before danger.
7. Local AI owns pursuit / attack and can choose to disengage.
8. Resolve and persist result, cooldown, and telemetry.

## Threat budget

Prototype maximums:
- One active hostile threat.
- One active major acoustic anomaly.
- Two minor ambient events.
- No hostile activation during harbour, dialogue, map screen, pause, or quest cutscene.
- Do not stack a severe weather strike and a hostile attack in the same short interval.

## Encounter fairness

- Warning window must be at least 2.2 s before the first hostile impact.
- The first encounter in a region is a warning / investigation state, not a surprise attack.
- If the boat is below 25% hull integrity, hostile event weight is halved and warning-only events are preferred.
- A hostile event is not eligible when there is no valid retreat lane.
- A failed spawn-position validation cancels the encounter and records a diagnostic; it must not reroll repeatedly until a bad location happens to pass.

## Cooldowns and tension

- Post-hostile global quiet interval: 90–150 game seconds.
- Post-attack hostile-selection lock: at least 120 game seconds.
- A per-creature cooldown is longer than its attack recovery.
- Tension modifies probability / candidate weights but never bypasses hard eligibility.
- Tension decays during low-noise travel and safe harbor.

## Debug output

Show event ID, seed key, opportunity, hard gates, probability, random draw, normalized weights, spawn anchor, telegraph start, attack result, and exit reason. A debug overlay must show why no event occurred.

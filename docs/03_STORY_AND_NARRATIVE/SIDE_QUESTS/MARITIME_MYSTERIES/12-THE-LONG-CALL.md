# The Long Call

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Whale acoustics / exploration / acoustic navigation  
> Inspiration: Real whale vocalizations combined with a fictional deep-water mystery

## Short lore

> “The call travelled beneath the engine noise and kept going after the radio went quiet. The player marked the direction on the chart. At dawn, the same call returned from the opposite side of the trench.”

Several nights of unusual low-frequency calls have been reported near the Hollow Trench. The signals are long, repeated, and sometimes overlap with known whale-like patterns.

## Player hook

The lighthouse keeper asks the player to record a sequence of calls after a fishing boat reports hearing them through a closed radio. The objective is to determine whether the sound can help map a safe route along the trench edge.

## Player activity

1. Record the call from a safe distance.
2. Compare repetition intervals and apparent bearings across multiple listening points.
3. Use the Sounding Rig to test whether the source is near the seabed or in open water.
4. Follow the call only when the boat's position can be independently confirmed.
5. Decide whether to publish the route as a whale-migration note or keep the final unexplained segment private.

## Outcome

The player builds an acoustic route through the trench edge. Some call patterns align with ordinary whale-like vocalizations; one persistent sequence remains unexplained within the story. The quest does not require harming or capturing an animal.

## Reward

A safe route annotation, a wildlife log entry, and a new acoustic landmark.

## Dependencies

- Audio event playback / recording.
- Acoustic Navigation.
- Sounding Rig depth bands.
- Whale or marine-life ambient behavior.
- Quest flags and map annotation.

## Research and originality note

Whales produce complex vocalizations, but the report's use of “Leviathan” is mythic rather than scientific. Do not present whale calls as proof of a giant monster. Keep the real animal behavior grounded and the supernatural interpretation explicitly uncertain.

Reference starting point: https://en.wikipedia.org/wiki/Whale_vocalization  
Use scientific and conservation sources for final audio, migration, and behavior details.

## Scope note

The first version can use a short authored audio sequence, three listening locations, and a map task. Do not create a full whale simulation or a boss encounter.



## Runtime Behaviour and Implementation

### Event trigger and RNG
- Quest begins after the lighthouse keeper receives two separate reports.
- Evaluate on the night listening slot at the trench edge. Initial call chance: **15% per eligible night**; first quest clue is guaranteed by the second eligible night.
- The unexplained phrase is a seeded audio variation after the natural call is recorded; it never signals a hostile spawn.

### Behaviour, perception, and FOV
States: `Wildlife Window → Call Heard → Bearing Recorded → Second Call → Route Inferred → Complete`. A distant whale / large marine animal follows an authored migration spline at low simulation frequency. It does not target the player. No hostile FOV. A small surface plume may appear at 80–120 m in clear weather, but visual spotting is not required.

### Sound behaviour
Play a long animal-like call over low surf ambience. A second phrase repeats after an unusually long gap from a different bearing. Captions: `[long marine call, distant]`, then `[the phrase repeats from another direction]`. Maintain safe dynamic range.

### Attacks and chase
**None.** No bite, ram, tail strike, pursuit, or damage. If the player approaches too closely, the animal changes course and the event ends. This is avoidance, not aggression.

### Lore reveal
Lighthouse notes show the call was heard by different crews decades apart. Some entries align with natural vocalization; one repeated phrase remains unexplained. The game does not claim whales produce supernatural messages.

### Acceptance tests
- Wildlife route is repeatable for the same season / seed.
- Player can finish without seeing the animal.
- Animal never targets or damages the boat.
- Clue is guaranteed by the second eligible night.

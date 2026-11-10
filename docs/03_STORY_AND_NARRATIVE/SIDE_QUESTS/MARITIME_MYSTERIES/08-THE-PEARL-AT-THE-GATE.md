# The Pearl at the Gate

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Relic recovery / environmental puzzle  
> Inspiration: South Asian aquatic guardian iconography, including Makara motifs

## Short lore

> “The gate was carved with a creature that was neither fish nor crocodile. A pearl sat in its open mouth. The pearl was not a jewel. It was the only part of the carving that had not been worn smooth by the water.”

A stone gate fragment lies in a collapsed underwater structure. Its aquatic carving resembles no fish in the local catalogue.

## Player hook

The village archivist asks the player to recover a small inscription plate from the site. The archivist believes the carving is a warning about the movement of water through the old settlement.

## Player activity

1. Photograph or sketch the carving from the boat.
2. Use the Sounding Rig to identify the gate's hollow chamber.
3. Find a safe approach through the collapsed structure.
4. Recover the inscription plate without removing the central carved stone.
5. Compare the marks with the village archive and add a new interpretation to the map.

## Outcome

The inscription reveals that the gate directed water through a settlement channel. The player unlocks a submerged route and a new historical clue, but the carved figure remains unidentified.

## Reward

A new map passage, an archive entry, and salvage value for the plate. The pearl-like stone stays with the structure; it is not converted into a magic upgrade.

## Dependencies

- Drowned Parish location.
- Sounding Rig cavity detection.
- Environmental puzzle / route unlock.
- Archive or lore catalogue.

## Research and originality note

Makara is a varied South Asian mythological and artistic motif appearing in multiple religious and architectural contexts. The first report did not establish one universal Makara ritual, seasonal event, or fishing practice. This quest uses an invented aquatic architectural carving and does not reproduce a sacred image or claim a single shared meaning.

Further research must be specific to the artwork, region, period, and religious context before any visual borrowing.

## Scope note

A single carving, a chamber-detection puzzle, and a map unlock are sufficient. Avoid a generic Makara boss or a collectible sacred relic.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- The chamber response triggers deterministically when the player deploys the Rig within the authored scan bounds and completes the required two scan angles.
- Optional water-surge variant: **10% per eligible scan** before the chamber is mapped. It creates a short current warning only; the inscription plate remains recoverable.
- Once the gate is mapped, do not roll again for the same scan ID.

### Behaviour states
`Unmapped Gate → First Return → Second Angle Required → Chamber Mapped → Plate Available → Resolved`.
The structure is static. The Rig reads a hollow chamber behind the carved gate. The player must compare two return angles to reveal an opening. The system uses authored echo profiles, not physical sound-wave simulation.

### Perception / FOV
No hostile FOV. The carving becomes inspectable within 8–12 m. A small air / sediment release appears when the chamber is correctly identified. Do not make the player search an invisible pixel; outline the inspection hotspot after the scan is complete.

### Sound behaviour
Two hollow returns have different decay lengths. The correct angle produces a short clean resonance. The optional surge plays a low current rush and a visual sediment plume.

### Attacks and chase
**None.** No entity attacks or chases. If the optional surge is used, it is an environmental push with a 2-second warning and no direct damage. Collision with the structure is the only potential hull damage.

### Lore reveal
The inscription describes water management / a gate mechanism in the old settlement. It gives evidence of human engineering but not the identity of the carved aquatic figure. The pearl-like stone stays in the structure; no sacred relic is extracted for power.

### Acceptance tests
- Two distinct scan angles are required.
- Plate availability is deterministic after a valid scan.
- The optional surge cannot destroy or permanently block the plate.
- No hostile entity or attack is spawned.

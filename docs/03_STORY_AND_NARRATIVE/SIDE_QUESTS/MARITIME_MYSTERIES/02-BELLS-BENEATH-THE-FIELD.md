# Bells Beneath the Field

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Underwater exploration / salvage  
> Inspiration: Welsh drowned-land legend, especially Cantre'r Gwaelod

## Short lore

> “At dusk, when the tide turns, a bell sounds from somewhere below the fishing grounds. The old people say a field lies under that water. They never agree on whose field it was, but every one of them knows the sound.”

A retired fisher keeps a hand-drawn chart showing a bell tower symbol beyond the Drowned Parish. The tower is absent from every modern map.

## Player hook

The retired fisher asks the player to confirm whether the bell is real. A survey marker and a sequence of faint underwater tones suggest a submerged masonry structure.

## Player activity

1. Collect the fisher's chart and compare it with the current nautical map.
2. Reach the site during the quest's authored tide window.
3. Use the Sounding Rig to map hard surfaces and hollow spaces.
4. Locate a broken bell frame beneath a collapsed roof.
5. Recover one bell fragment and choose where the fragment should be kept: the lighthouse archive, the family, or the village collection.

## Outcome

The recovered fragment changes how one NPC interprets the Drowned Parish. The location remains dangerous, but a new acoustic landmark becomes available after the player records its tone.

## Reward

A chart annotation, a small salvage payment, and an acoustic landmark entry. Avoid an implausibly powerful fishing upgrade for a historical discovery.

## Dependencies

- Drowned Parish or a small submerged ruin location.
- Sounding Rig surface classification.
- Salvage inventory and a simple NPC dialogue branch.
- Tide or time-window quest condition.

## Research and originality note

Cantre'r Gwaelod is a Welsh legendary drowned land associated with stories of inundation and failed protection. It is a legend, not proof of a fully mapped lost kingdom. The game location, bell, people, and history are invented; do not copy Welsh names or claim the fictional ruin is the real place.

Reference starting point: https://en.wikipedia.org/wiki/Cantre%27r_Gwaelod  
Find Welsh cultural, archival, and scholarly sources before making specific historical statements.

## Scope note

The first version can use a single hand-authored ruin pocket, a sonar-like search, and a dialogue choice. Full underwater swimming is not required; the player can investigate from the boat and recover the fragment with equipment.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- Quest becomes active after the retired fisher gives the chart.
- Bell event activates deterministically when the player is inside the ruin bounds during the authored low-tide / turning-tide window.
- Optional second harmonic variation: **35% per eligible listen**, seeded by tide-cycle ID. The required bell clue never depends on this roll.
- The structure can be rediscovered after the quest, but the fragment can only be collected once.

### Behaviour states
`Silent Structure → Tide Window → Bell Resonance → Rig Scan → Fragment Available → Resolved`.
The bell is a static environmental sound source. Tide changes its audible range and decay. The Sounding Rig maps the tower frame and hollow cavity; the UI exposes a broad depth band and confidence, not an exact outline until enough readings are collected.

### Perception / FOV
No creature vision or player FOV cone. The player can see the tower only inside the camera's visible underwater / surface reveal range. Use a faint vertical bubble line and a small debris movement cue when the sound is active. Do not put the full tower in view before the survey step.

### Sound behaviour
One bell strike at tide start, then a decaying harmonic tail. A faint second strike may occur after the optional RNG roll. Sound range is weather- and position-dependent; the quest log provides a text timing clue so players who cannot hear the tail can still progress.

### Attacks and chase
**None.** No hostile entity, chase, or direct damage. Rock collision and current force remain normal world hazards. The ruin does not collapse as a surprise attack.

### Lore reveal
The fragment bears a date notation and a tide mark that conflicts with the village story. It proves that people worked and maintained the submerged structure, but does not prove the legendary drowned kingdom existed as told.

### Acceptance tests
- Bell activates at the same tide boundary for a fixed save.
- The clue is accessible with audio muted.
- Fragment is not duplicated after reload.
- No creature, attack, or chase event is emitted.

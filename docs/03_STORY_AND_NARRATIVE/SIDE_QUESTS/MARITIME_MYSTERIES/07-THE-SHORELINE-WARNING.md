# The Shoreline Warning

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Search and rescue / acoustic navigation  
> Inspiration: General shoreline-warning folklore; no direct depiction of Qalupalik

## Short lore

> “The old warning was simple: never answer a voice from the water when you cannot see who called. The warning was meant for children. The missing deckhand was thirty-two, and he knew the rule better than anyone.”

A deckhand disappears while checking a marker line in heavy fog. A relative insists that someone called his name from beyond the shoreline, but the radio log shows no incoming transmission.

## Player hook

The harbour master asks the player to search the last known marker route before weather makes the area inaccessible. The missing person is an adult crew member, and the rescue is treated as a serious human emergency—not a monster-hunt.

## Player activity

1. Gather the deckhand's planned route and last radio check.
2. Follow a known bell sequence through fog.
3. Compare the radio log with the Sounding Rig's recorded signal.
4. Locate a damaged marker and a sheltered cove.
5. Choose whether to follow the voice-like sound or the physical evidence trail.

## Outcome

The player finds the deckhand sheltering in the cove after losing the marker line. The voice was a combination of a damaged buoy and radio feedback. The result does not prove that all strange sounds have ordinary explanations; it teaches the player to check evidence before reacting.

## Reward

A rescue payment, an NPC relationship change, and a new radio troubleshooting note.

## Dependencies

- Fog and acoustic navigation.
- Radio log data.
- Search location and rescue interaction.
- NPC relationship state.

## Research and originality note

Qalupalik appears in Inuit storytelling, with variants and context that should be sourced to specific communities. The project does **not** depict Qalupalik, reproduce a child-abduction story, or treat Inuit tradition as a generic horror monster. This quest takes only the broad design idea that shoreline warnings can encode practical safety knowledge. If the direct tradition is ever considered, community-specific research and consultation are mandatory.

Reference starting point from the first report: https://en.wikipedia.org/wiki/Qalupalik  
This is not a final cultural source.

## Scope note

Build as an adult rescue mission using existing fog, buoy, and radio systems. Do not introduce a child NPC, kidnapping, or a new supernatural creature.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- The rescue starts deterministically when the player accepts the harbour master's request.
- Search points activate in authored order. At each point, a **30% radio-feedback variant** may play; the actual rescue signal and cove location are guaranteed.
- The variant is seeded by quest ID and search-point ID. Never reroll it on save/load.

### Behaviour states
`Search Route → First Marker → Conflicting Signal → Evidence Comparison → Cove Located → Rescue Complete`.
The missing deckhand is an adult NPC waiting in a sheltered cove. He has no hostile AI. He responds to the player's approach with a scripted wave / lantern signal and a short interaction. Search progression is based on evidence gathered, not the random feedback variant.

### Perception / FOV
No hostile FOV. The deckhand becomes visible within 25–40 m depending on fog, with a subtle lantern or reflective marker cue. The elevated camera may not show the cove behind a rock; the radio bearing and chart provide the alternate route. Player camera does not rotate automatically.

### Sound behaviour
Damaged buoy emits a three-part pattern with one missing beat. Radio feedback repeats a syllable and seems to come from a different bearing. The player can compare both sources against the lighthouse signal. Captions give beat count and approximate direction.

### Attacks and chase
**None.** No predator appears, no NPC flees, and no chase occurs. The environmental risk is navigation through fog and rock shelves. The route must be escapable; the rescue subject cannot be killed by off-screen random events.

### Lore reveal
The deckhand admits he followed the voice because it resembled the skipper, but the radio log shows the signal came from a damaged buoy. The story teaches caution without explaining every sea mystery away.

### Acceptance tests
- The rescue subject always appears after the evidence sequence.
- Radio feedback variation never blocks progress.
- No attack / chase / damage event is emitted.
- Rescue state persists after saving and reloading.

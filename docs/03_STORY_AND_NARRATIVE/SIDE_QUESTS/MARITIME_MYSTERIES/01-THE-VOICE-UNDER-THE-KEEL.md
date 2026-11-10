# The Voice Under the Keel

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Acoustic investigation / navigation  
> Inspiration: Greek siren tradition, transformed into an original signal mystery

## Short lore

> “The first time I heard it, I thought someone was calling from the next boat. The second time, it used my brother's voice. There was no boat beside me. There was no brother left to call.”

Fishermen near the Outer Shoals report a voice travelling against the wind. Each listener hears a different familiar voice, but the tone repeats with the same pause and the same final note.

## Player hook

A harbour resident asks the player to recover a small skiff that drifted away after its owner followed a voice. The last reliable sighting places it near a chain of submerged rock spurs.

## Player activity

1. Reach the marked water during a low-visibility window.
2. Deploy the Sounding Rig and record the repeating phrase.
3. Compare the signal's delay against the apparent direction of the voice.
4. Use two known bell buoys to triangulate the source.
5. Decide whether to follow the voice to its apparent origin or use the signal discrepancy to locate the skiff's actual drift path.

## Outcome

- **Careful interpretation:** Find the skiff and recover its logbook. Unlock a chart note that improves future signal triangulation.
- **Blind pursuit:** The player reaches a hazardous reef pocket, takes a manageable hull hit, and can still recover the skiff by following debris.
- **Optional choice:** Keep the damaged recording or erase it at the lighthouse. Keeping it adds a persistent anomalous entry to the catalogue.

## Reward

A signal-calibration note or a small Sounding Rig tuning component. The reward should improve interpretation, not grant a large arbitrary range bonus.

## Dependencies

- Sounding Rig: record / replay signal.
- Acoustic Navigation: known buoy bearings.
- Boat handling and collision.
- One side-story NPC and one small skiff wreck prop.

## Research and originality note

The broad inspiration is the Greek literary motif of a dangerous voice at sea. Sirens in ancient sources are not simply identical to the later popular mermaid image. This quest does not depict or name a mythological siren. Its voice is a fictional acoustic anomaly with a testable repeating pattern.

Reference starting point: https://en.wikipedia.org/wiki/Siren_(mythology)  
Use the link only to locate classical literary sources; it is not a final scholarly citation.

## Scope note

Build as a short signal-reading mission using existing Sounding Rig and buoy systems. Do not create a new AI creature or voice-imitation system for the first implementation.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- Quest starts after the player receives the skiff report and enters the Outer Shoals with the Sounding Rig installed.
- Evaluate only on a valid Rig deployment at night or in heavy fog. Initial chance: **18% per eligible deployment**.
- If the player has three failed eligible deployments, the next deployment guarantees the clue. The clue is saved by opportunity ID and never rerolls on load.
- The signal variant is selected from a seeded list: familiar voice contour, doubled syllable, or delayed final note. Variant changes presentation, not clue availability.

### Behaviour states
`Dormant → First Echo → Repeated Echo → Bearing Drift → Evidence Resolved / Cooldown`.
The anomaly has no physical body, target, health, collision, or creature AI. Each Rig pulse creates one return event. The second return appears 1.2–2.0 seconds later, with a deliberately inconsistent bearing. The interval repeats exactly enough for the player to recognize a pattern.

### Perception / FOV
There is no creature FOV and no player-facing vision cone. The elevated camera can reveal surface ripples inside the current camera view. At 40–70 m, a subtle ring or wake may appear if fog permits. The camera must not reveal the actual source before the player triangulates it. A non-spatial caption and waveform cue provide accessibility support.

### Sound behaviour
The signal uses a voice-like harmonic contour without copying a real person's voice. First return is low and distant; the second is slightly clearer but arrives from an impossible bearing. It should not be loud enough to mask buoy or lighthouse cues. Caption example: `[a familiar voice repeats beneath the hull]`.

### Attacks and chase
**None.** The anomaly does not pursue, hit, damage, or physically spawn. The danger is navigational: the false bearing can lead toward known reef hazards. The player always has the map, buoy pattern, and a second observation as counter-evidence. Any hull damage comes from an actual collision, never from the voice.

### Lore reveal
The recovered log reveals three different witnesses describing the same pause pattern but hearing different people. This supports the idea that the anomaly imitates remembered voices; it does not confirm whether a creature, geology, or the sea itself causes the signal.

### Acceptance tests
- Same seed and Rig opportunity produce the same echo sequence.
- No damage or pursuit event is emitted.
- Required bearing clue is guaranteed after the retry limit.
- Known buoy signals remain distinguishable from the false voice.



## Runtime Behaviour and Implementation

### Event trigger and RNG
- Quest starts after the skiff report and entry into Outer Shoals with the Sounding Rig installed.
- Evaluate only on a valid Rig deployment at night or in heavy fog. Initial chance: **18% per eligible deployment**.
- After three failed eligible deployments, guarantee the clue on the next deployment. Save by opportunity ID; never reroll on load.
- Seeded signal variants: familiar voice contour, doubled syllable, or delayed final note. Variants change presentation, not clue availability.

### Behaviour, perception, and FOV
States: `Dormant → First Echo → Repeated Echo → Bearing Drift → Resolved / Cooldown`. The anomaly has no physical body, health, collision, or creature AI. No hostile FOV and no player vision cone. Surface rings may appear within 40–70 m in clear weather; fog shortens this. The actual source remains concealed until triangulated.

### Sound behaviour
The first return is low and distant; a second return arrives 1.2–2.0 seconds later from an inconsistent bearing. It must not mask buoy or lighthouse cues. Caption: `[a familiar voice repeats beneath the hull]`.

### Attacks and chase
**None.** The signal does not pursue, hit, damage, or physically spawn. The only danger is navigational misdirection toward known reef hazards. Hull damage can only result from actual collision.

### Lore reveal
Three witnesses describe the same pause pattern but hear different familiar voices. The anomaly imitates memory; its cause remains unknown.

### Acceptance tests
- Fixed seed + opportunity reproduces the same echo sequence.
- No damage or chase event is emitted.
- Required clue is guaranteed after the retry limit.
- Known buoy signals remain distinguishable.

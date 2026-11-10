# The Bell at the Turning Tide

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Timed exploration / seasonal event  
> Inspiration: Original fictional calendar event informed by recurring maritime bell and tide motifs

## Short lore

> “Once each year, the bell is heard from the water. No one sees it ring. The sound lasts only as long as the tide takes to turn, and the lighthouse clock loses one minute before it stops.”

The village calendar contains a single handwritten note: “Listen when the tide changes.” No one remembers who wrote it.

## Player hook

The lighthouse keeper asks the player to investigate a sound that appears only during a narrow tide window. The keeper suspects the bell is a mechanical marker used by the old harbour.

## Player activity

1. Obtain the tide-window prediction from the lighthouse keeper.
2. Position the boat near two known acoustic landmarks.
3. Record the bell's first and second reflections.
4. Use the Sounding Rig to locate a submerged bell housing.
5. Decide whether to repair the mechanism, leave it undisturbed, or record the sound and return later.

## Outcome

Repairing the bell creates a permanent navigational cue during the event window. Leaving it alone preserves the mystery but gives no route benefit. Recording it creates a new clue for a later story beat.

## Reward

A new acoustic navigation landmark, an archive entry, or a later quest lead depending on the choice.

## Dependencies

- Tide and calendar system.
- Audio event trigger.
- Sounding Rig.
- One lighthouse NPC dialogue condition.
- Save/load persistence for the player's choice.

## Research and originality note

The first research report proposed equinox and solstice bell events, but did not establish a universal historical practice. This quest's calendar, bell, and ritual context are entirely fictional and must remain specific to Greyhaven unless separately researched.

## Scope note

The event can be a deterministic one-day-per-game-year trigger. The player should receive warnings and a second opportunity so missing the window does not permanently block content.



## Runtime Behaviour and Implementation

### Event trigger and RNG
- Event occurs deterministically at the authored tide-turn phase on the designated in-game calendar day.
- Only harmonic ornament varies by the saved event seed. The bell itself is not random.
- Keep the window open for one full in-game tide interval; if missed, it returns next cycle. Never gate the quest behind a one-minute real-time window.

### Behaviour, perception, and FOV
States: `Dormant Calendar → Window Forecast → Tide Turning → Bell Sequence → Recording Available → Resolved`. The bell is a static submerged mechanism; it rings as the tide changes, then stops. No creature FOV or player vision cone. Once the first bearing is recorded, reveal the structure at 15–25 m subject to fog.

### Sound behaviour
Three-part sequence: one long tone, two shorter returns. The waveform variation is saved by event ID. Captions provide rhythm and approximate direction.

### Attacks and chase
**None.** No entity or pursuit. One approach route may be unsafe during the tide turn, but a second accessible route remains. Bell audio never causes damage.

### Lore reveal
A lighthouse log contains the same three-part notation from before current village records, suggesting an old maritime marker of unknown origin.

### Acceptance tests
- Follows game calendar and tide simulation.
- Player has a full tide interval to interact.
- Missed event returns next cycle.
- Audio has caption and waveform equivalents.

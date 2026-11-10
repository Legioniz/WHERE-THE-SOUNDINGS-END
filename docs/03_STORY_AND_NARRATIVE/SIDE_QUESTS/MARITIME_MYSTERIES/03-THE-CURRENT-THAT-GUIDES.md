# The Current That Guides

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Navigation / ecological observation  
> Inspiration: Broad maritime stories of guiding sharks and animal helpers

## Short lore

> “A pale shape crossed the bow three times. Each time the current changed with it. My grandfather said the sea sometimes lends you a guide. He never said the guide was kind.”

A small group of fishers describe a recurring pale wake that appears before a safe passage opens through the Outer Shoals. The sightings are inconsistent, and no one agrees whether it is an animal, reflected light, or a current line.

## Player hook

The lighthouse keeper asks the player to verify reports of a safe route that appears only during a particular current state. A supply boat has failed to return after attempting the usual channel.

## Player activity

1. Record current direction and speed at three established buoys.
2. Watch for a moving surface disturbance near the shoal edge.
3. Avoid following the shape blindly; compare it to the Sounding Rig's depth and current readings.
4. Find the channel that forms behind the disturbance.
5. Decide whether to mark the channel publicly or keep the route private.

## Outcome

- **Share the route:** Greyhaven gains a safer trade path and new ambient boat traffic.
- **Keep it private:** The player retains a small economic advantage, but the missing supply boat's family receives less help.
- **Misread the signal:** A minor collision or lost time occurs; the player can still chart the route on a later trip.

## Reward

A permanent chart mark and an NPC trust change. The reward is knowledge and community consequence rather than a supernatural blessing.

## Dependencies

- Living Current model.
- Buoys with known positions.
- Navigation chart updates.
- Small NPC world-state change.

## Research and originality note

The first research report referenced Kamohoaliʻi, a Hawaiian shark deity, in stories concerning guidance at sea. This quest does **not** depict, rename, or turn that deity into a game creature. It uses only the broad and widely recurring narrative motif of an animal or sea presence guiding a vessel. Any future direct use of Hawaiian material requires Hawaiian-authored sources and consultation.

Reference starting point: https://en.wikipedia.org/wiki/K%C4%81mohoali%CA%BBi  
Treat this as an index, not a final source.

## Scope note

Use the existing Living Current simulation and a scripted surface cue. No companion creature AI, ritual system, or new faction is necessary.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- Trigger evaluation on an authored tide transition while the player is inside the shoal corridor and the quest is active.
- Surface wake appearance chance: **25% per eligible tide transition**. The safe current route itself is discoverable deterministically from current readings and buoy observations.
- After two missed wake rolls, guarantee a wake on the next eligible transition. The route clue cannot be RNG-locked.

### Behaviour states
`Normal Current → Current Shift → Wake Appears → Lane Stabilizes → Lane Decays`.
The wake is a wildlife / surface disturbance cue. It follows the boundary of the safe current lane for 15–25 seconds, then fades. It does not steer toward the player or orbit the boat. The current vector changes smoothly over 8–12 seconds, with a visible foam / debris line.

### Perception / FOV
No hostile FOV. Player camera visibility range is governed by fog; the wake has a high-contrast foam edge at 35–60 m in clear conditions and shorter range in fog. The camera can show the wake moving across the screen, but does not auto-rotate or lock to it.

### Sound behaviour
Use a soft water rush and occasional animal movement splash. No roar, sting, or threat heartbeat. The sound should communicate direction only when the wake is close; the compass and current arrow remain the reliable navigation tools.

### Attacks and chase
**None.** The animal/wake is non-hostile and does not follow, ram, or bite the player. Incorrectly entering the channel at an adverse tide can push the boat laterally toward marked rocks. That is a predictable environmental hazard, not an attack. Give at least 3 seconds of visual / instrument warning before severe lateral force.

### Lore reveal
The lighthouse keeper's notes show that earlier fishers recorded the same safe-current window but disagreed about what moved through it. The quest never confirms a divine guide or identifies the wake as a named mythic being.

### Acceptance tests
- Wake and current vector share one saved tide-cycle seed.
- No chase or damage call is made by the wildlife cue.
- Player can chart the lane without seeing the wake by comparing instruments.
- Unsafe current force is preceded by a warning.

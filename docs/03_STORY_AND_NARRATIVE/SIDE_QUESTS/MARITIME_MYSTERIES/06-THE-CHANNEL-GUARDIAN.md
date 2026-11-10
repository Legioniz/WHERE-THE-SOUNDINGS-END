# The Channel Guardian

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Route learning / environmental storytelling  
> Inspiration: Place-associated water guardian motifs; not a depiction of taniwha

## Short lore

> “The channel has a name, but the name is not written on any chart. The people who live beside it do not cross when the water turns black. They say the channel is occupied. The mechanic says it is a pressure current. Neither will take a boat through it.”

A narrow passage between rock shelves is avoided by local fishers. The warning is passed down without a consistent explanation.

## Player hook

A damaged buoy blocks the only reliable route to a remote fishing ground. The harbour master asks the player to inspect the passage and determine whether it can be marked safe.

## Player activity

1. Interview two local fishers and note where their accounts differ.
2. Use the Sounding Rig to map a submerged ridge.
3. Observe how the current changes around the ridge at different tide states.
4. Recover the buoy without disturbing a protected marker placed by the villagers.
5. Decide whether the channel should be charted as dangerous, conditionally navigable, or closed.

## Outcome

The choice changes which NPCs will use the route and how much boat traffic appears. A fully charted route can be safe under known conditions but remains dangerous outside them.

## Reward

A navigation chart upgrade and a small relationship change. The player learns a reusable rule about current direction and rock geometry.

## Dependencies

- Living Current and tide states.
- Acoustic depth map.
- Buoy repair interaction.
- NPC testimony flags.

## Research and originality note

Taniwha are significant in Māori traditions and can be associated with particular places and communities; the meanings and accounts are not uniform. This quest does not use the word taniwha, copy a known being, or turn a cultural guardian into a monster. It develops an invented Greyhaven warning and should remain generic unless culturally appropriate consultation supports a more specific direction.

Reference starting points:
- https://teara.govt.nz/en/taniwha
- https://tmrowe.com/tag/taniwha/

The personal blog is a secondary lead, not an authority.

## Scope note

Use a route hazard, current-state rules, two NPC dialogue flags, and one buoy interaction. No creature encounter is necessary.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- Evaluate on tide transition while the player is inside the channel bounds and the quest is active.
- Current surge presentation chance: **20% per eligible tide transition**; once the quest requires the hazard evidence, the event becomes deterministic on the next adverse tide.
- Tide state, route force, and warning timing are generated from the saved tide-cycle ID. Do not roll a different current every physics frame.

### Behaviour states
`Stable Channel → Adverse Tide Warning → Lateral Current → Safe Window / Surge Decay`.
The "guardian" is the channel itself, not a monster. Current vectors interpolate over 8 seconds. Severe lateral force is applied only inside the marked channel polygon and only during the adverse tide state. A buoy rhythm becomes irregular at the boundary to warn the player.

### Perception / FOV
No creature FOV and no player vision cone. A dark surface band and foam streak indicate the current direction within 30–50 m in clear weather. Fog reduces visual range, but the compass/current indicator and buoy audio still indicate the safe heading.

### Sound behaviour
Buoy bell rate slows and becomes uneven as the current strengthens. The sound is a navigational warning, not a monster voice. Caption: `[channel buoy rings irregularly]`.

### Attacks and chase
**No attack and no chase.** The surge can push the boat laterally; damage only occurs if the player collides with mapped rocks. Apply a 3-second buildup before maximum force, clamp lateral acceleration, and provide a clear escape heading. No force may be applied in safe zones.

### Lore reveal
Two local fishers explain the same warning differently: one says a guardian occupies the channel; the mechanic describes an unusual tide-driven flow around submerged rock. The quest preserves both readings and does not confirm a named Māori being or other real-world deity.

### Acceptance tests
- Current force matches the saved tide state.
- Player receives a warning before severe force.
- No hostile AI or attack component is created.
- A safe heading is discoverable with visual or accessibility cues.

# The Sleeper's Lullaby

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Threat encounter / sound discipline  
> Inspiration: Kraken and giant deep-sea creature folklore

## Short lore

> “The water rose without a wave. Something below the boat moved slowly enough to make the hull sing. The old log says the crew stopped the engine, stopped speaking, and waited for the sound to pass.”

A damaged vessel is found with its anchor chain wrapped around something far below. Its crew survived, but the captain refuses to describe what answered the engine.

## Player hook

The boat mechanic offers a payment for recovering a specific engine component from the vessel's last reported position. The wreck's location overlaps a deep acoustic anomaly.

## Player activity

1. Approach using low engine output.
2. Use the Sounding Rig once to identify the object's depth band.
3. Notice that repeated pings produce a stronger return and a growing hull vibration.
4. Choose between an immediate retreat and a short, low-noise salvage window.
5. Escape by following a known current line rather than accelerating directly away.

## Outcome

A successful quiet approach recovers the engine part and a fragment of the captain's log. Repeated pings raise the encounter's danger state. The entity is never fully shown; the player is left with evidence of scale and movement.

## Reward

A boat component, salvage money, and a new threat-catalogue entry. The component must be useful but not required for main progression.

## Dependencies

- Threat Director with an escalation state.
- Sounding Rig use counter / signal event.
- Boat engine noise parameter.
- Salvage interaction and retreat route.

## Research and originality note

The kraken is a longstanding sea-monster motif in Scandinavian and North Atlantic maritime lore. The encounter here is original: it does not reproduce a named creature design or claim that the folklore described a zoologically verified animal.

Reference starting points:
- https://en.wikipedia.org/wiki/Kraken
- https://en.wikipedia.org/wiki/Sea_monster

These are general-reference indexes; locate specialist historical and folklore research before final lore publication.

## Scope note

For the first build, use a sound-driven threat state, underwater shadow, hull audio, and escape condition. Do not build a complex tentacle boss or naval combat system.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- The first tutorial contact is **scripted and guaranteed** after the player recovers the captain's first log fragment and enters the marked deep-water band.
- Later investigation opportunities occur on high-noise event slots in the Hollow Trench: initial chance **12% per eligible slot**. Eligible noise includes a Rig ping, prolonged high throttle, horn, or collision.
- Hard gates: quest active, deep band, no active hostile, no attack cooldown, valid retreat lane, and player not in a safe zone or cutscene.
- Repeated rolls are not made while the same sound event is being processed. After an encounter, apply at least 120 game seconds of hostile-selection lock.

### Behaviour states
`Dormant → Investigating → Warning → Stalking → Pursuing → Strike Telegraph → Strike → Recovering → Disengaging → Cooldown`.
- **Investigating:** moves toward the last sound source, not directly to the player's live position.
- **Warning:** produces a distant low pulse and a visible pressure ring. Minimum warning is 2.2 seconds before any impact.
- **Stalking:** follows at 2.8–4.0 m/s, offset from the player's wake. It updates its target estimate only on new visual or acoustic evidence.
- **Pursuing:** accelerates to a prototype 6.5–7.5 m/s for no more than 12 seconds, then re-evaluates.
- **Strike:** attempts one lateral hull ram from a telegraphed approach lane.
- **Recovering:** turns away for at least 6 seconds; attack cooldown is 18–24 seconds.
- **Disengaging:** breaks off after 90 m separation, loss of target confidence, boundary reached, or pursuit timeout.

### Vision and FOV
- Total visual FOV: **100°** (±50°), clear-water range 70 m.
- Visual checks: 4 Hz nearby; acquire only after 0.8–1.2 seconds above visibility threshold.
- Fog and lighting modify range / contrast. The creature's eye anchor is near the surface only during warning or strike; while deep, it relies primarily on sound.
- Player camera is elevated, not first-person. The threat must produce a directional wake / vibration / rig cue if outside camera view but within strike range.
- Rocks and island geometry block visual line of sight. Do not let particles or rain sprites occlude the target.

### Hearing and chase memory
- Rig ping is the strongest lure. Engine noise is persistent; collisions and horn are short high-amplitude cues.
- A sound marks a last-known source position. If the boat moves after detection, the creature investigates / intercepts predicted movement rather than snapping to the boat.
- Visual memory confidence decays over 2–4 seconds after line of sight is lost.
- The creature cannot detect through safe-zone boundaries and cannot cross shallow-water exclusion zones.

### Attack
- Telegraph: 2.2–3.0 seconds, unique two-pulse cue plus lateral wake.
- Hit: **12–18% starter hull integrity** and temporary steering instability for 2.5 seconds. Do not apply engine failure and steering instability together on the first hit.
- No one-hit kill. No attack below 25% hull unless player ignores a final clearly presented warning after a later story upgrade; initial prototype should suppress attacks below 25% and prefer stalking / retreat.
- Attack must be resolved through a collision window and cooldown; never apply damage from distance or through an invisible trigger.

### Counterplay
- Stop repeated Rig pulses; lower throttle to reduce detection.
- Change heading during the telegraph; do not simply hold maximum speed in a straight line.
- Follow a known current lane or move toward the trench-edge shallows.
- Rock occlusion can break sight but cannot instantly erase a strong sound trace.
- Retreat is always valid; completing the quest does not require defeating the creature.

### Sound design and lore
The Sleeper has no intelligible speech. It is heard through hull resonance, chain-like vibration, a low pulse, and a distinct lateral attack swell. The logbook says the crew stopped the engine because each pulse returned closer than the last. Evidence suggests it is attracted to repeated mechanical sound, but its origin and intention remain unknown.

### Acceptance tests
- Tutorial encounter is guaranteed once and saved.
- No attack before telegraph completion.
- Attack cooldown prevents chain strikes.
- Creature searches last-known source after visual loss; it does not track exact player position through walls or fog.
- Low hull, safe zone, invalid retreat lane, and cutscene gates suppress attack.
- Fixed seed reproduces later eligible encounter selections.

# Maritime Horror Direction and Greyhaven Runtime Layer

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven prototype implementation + production direction  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Creative target

The horror should feel like the sea has become difficult to read, not like the player has entered a haunted-house attraction. Build from ordinary working-fisherman pressures: isolation, changing weather, poor visibility, uncertain bearings, unfamiliar water, engine and hull noises, fatigue, distant vessels, navigation lights, animal calls and the consequences of being far from help.

The game does not claim that all or most fishermen experience supernatural events. Real maritime conditions provide the grounded vocabulary; the impossible details belong to this fictional setting.

## Rules of restraint

1. **Begin with a plausible explanation.** A bell, knock, call or light should initially be classifiable as equipment, wildlife, another vessel or weather.
2. **Let details disagree.** A light has no wake; a bell's rhythm does not match the buoy; a sound seems to come from the wrong side; a familiar landmark is briefly hard to identify.
3. **Do not make every incident hostile.** Most events are atmosphere, uncertainty or wildlife. A sound is not automatically a threat trigger.
4. **Respect seamanship.** The player should be able to use bearings, landmarks, weather, speed, depth and equipment condition to make better decisions.
5. **Avoid constant stingers.** Long stretches of ordinary work are essential. Incidents are spaced in in-game time and only become eligible under night or elevated psychological pressure.
6. **Keep danger legible.** A genuine attack must still use its own readable warning and escape window. Ambient horror never substitutes for threat telegraphs.
7. **Do not punish curiosity with arbitrary damage.** Ambient incidents cannot damage the hull, remove cargo, change navigation or create collision geometry.

## Greyhaven prototype incidents

Implemented by `FishermanAtmosphereDirector`:

| Incident | Presentation | Player-facing meaning |
|---|---|---|
| Hollow hull knocks | Two damped, low-frequency knocks spatialized near the boat | Something struck or flexed beneath the deck; source remains unknown |
| Distant navigation bell | Water-muted synthesized bell from a point beyond the boat | A familiar navigation reference is audible, but its bearing may be hard to judge |
| False navigation light | Small warm light drifts slightly over the water, then fades without a wake | It resembles a distant working light but cannot be confirmed |
| Distant animal call | Quiet, low, slowly changing tonal call | Could be wildlife; do not label it as a monster |

- Incidents are checked only while the action-driven sea clock is advancing.
- Eligibility begins at night or once panic reaches the configured daytime threshold.
- The first incident has a delay; later incidents use a randomized in-game interval.
- Incidents are atmospheric only: no colliders, damage, cargo loss, forced steering or threat-state mutation.
- The audio is synthesized at runtime for the prototype. These are temporary greybox cues, not final recorded field audio.
- The director is attached after scene load when a `SeaStateDirector` is present.

## Future production layers

### Soundscape
Record or source properly licensed field material: small outboard engines at different loads, hull slap, rope under tension, chain and anchor handling, rain on fiberglass and metal, distant thunder, wind through rigging, mangrove insects, seabirds, reef surf, and underwater hydrophone textures. Keep dry close sounds distinct from distant water-filtered sounds. Avoid relying on sub-bass alone.

### Visibility and navigation
Add authored fog banks, rain curtains, spray, glare, moonless silhouettes, lighthouse rotation, buoy motion and current-driven drift. Preserve readable compass, chart, depth and bearing information. Poor visibility should increase workload rather than simply hide everything.

### Human presence
Use sparse radio traffic, harbor routines, working lights, boat silhouettes, incomplete log entries and NPC accounts that contradict one another. Do not use random voice impersonations or unexplained voices every few minutes. A familiar voice should be a rare, authored story event with context and a corroboration mechanic.

### Ecological unease
Use normal animal behaviour as the baseline: seabirds gathering over bait, fish scattering, nocturnal calls, bioluminescent plankton, floating debris, current lines, feeding activity and animals avoiding a disturbed area. The anomaly is the timing, pattern or impossible scale—not every animal behaving like a monster.

### Encounter escalation
Separate the layers:
- **Ambient uncertainty:** sounds, weather, lights and visibility.
- **Ecological anomaly:** unusual animal distribution or behaviour.
- **Investigation clue:** repeatable evidence tied to a location or time.
- **Active threat:** authored state machine, warning, counterplay and consequences.

Never promote an ambient incident directly into an attack without the threat system's own eligibility checks and telegraph.

## Acceptance checks

- Ordinary daylight travel remains mostly uneventful.
- Night or high pressure can produce an incident, but does not guarantee one on every trip.
- Ambient sounds are spatialized and remain quieter than safety/navigation cues.
- A false light has no collider and cannot be used as a physical waypoint.
- Ambient incidents do not change panic, save data, cargo, vessel condition or creature AI.
- Genuine threats retain their existing warning timings and escape rules.
- Final sound assets require licensing/attribution and mono/accessibility review.

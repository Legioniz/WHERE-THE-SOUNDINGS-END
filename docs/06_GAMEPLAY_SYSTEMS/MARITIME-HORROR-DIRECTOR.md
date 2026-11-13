# Maritime Horror Direction and Greyhaven Runtime Layer

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven prototype implementation + production direction  
> Version: 0.3  
> Last reviewed: 2026-11-13

## Creative target

The horror should feel like the sea has become difficult to read, not like the player has entered a haunted-house attraction. Build from ordinary working-fisherman pressures: isolation, changing weather, poor visibility, uncertain bearings, unfamiliar water, engine and hull noises, fatigue, distant vessels, navigation lights, animal calls and the consequences of being far from help.

The game does not claim that all or most fishermen experience supernatural events. Real maritime conditions provide the grounded vocabulary; the impossible details belong to this fictional setting.

## Rules of restraint

1. **Begin with a plausible explanation.** A bell, knock, call or light should initially be classifiable as equipment, wildlife, another vessel or weather.
2. **Let details disagree.** A light has no wake; a bell's rhythm does not match the buoy; a sound seems to come from the wrong side; a familiar landmark is briefly hard to identify.
3. **Do not make every incident hostile.** Most events are atmosphere, uncertainty or wildlife. A sound is not automatically a threat trigger.
4. **Respect seamanship.** The player should be able to use bearings, landmarks, weather, speed, depth and equipment condition to make better decisions.
5. **Avoid constant stingers.** Long stretches of ordinary work are essential. Incidents are spaced in in-game time and only become eligible under authored environmental / exposure conditions.
6. **Keep danger legible.** A genuine attack must still use its own readable warning and escape window. Ambient horror never substitutes for threat telegraphs.
7. **Do not punish curiosity with arbitrary damage.** Ambient incidents cannot damage the hull, remove cargo, change navigation or create collision geometry.
8. **Separate evidence from interpretation.** A witness report, sonar return, or journal theory is not automatically a confirmed entity.
9. **Do not turn psychology into a monster generator.** Fatigue and sensory uncertainty can affect confidence, but never imply that mental illness causes violence or that every unusual perception is supernatural.

## Greyhaven prototype incidents

Implemented by FishermanAtmosphereDirector:

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
- The director is attached after scene load when a SeaStateDirector is present.

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

## Sudden sea-state anomalies — Greyhaven prototype

The Greyhaven water plane receives a runtime tessellated mesh through MaritimeSurfaceBehaviour. It carries a restrained two-layer swell in ordinary conditions and supports two rare anomalies:

| Event | Surface behaviour | Sound behaviour | Interpretation |
|---|---|---|---|
| Dead calm | Swell amplitude falls almost to zero over a short transition and remains nearly flat | Existing scene AudioSources are rapidly attenuated, then slowly restored | The wind seems to stop; the water against the hull becomes unnaturally absent |
| Wrong-way swell | Wave travel direction reverses and the surface amplitude increases briefly | Existing scene AudioSources dip for a shorter interval | The swell appears to travel against the expected wind direction |

- Events are rare rolls in the existing night / elevated-pressure incident schedule.
- Dead calm lasts approximately 12–20 seconds; wrong-way swell lasts approximately 14–22 seconds.
- Audio attenuation only affects AudioSources present when the atmosphere director initializes. There are not yet authored wind, wave, rigging or hull-ambience loops in the Greyhaven scene, so final silence-and-return mixing still depends on adding those recordings.
- The shared analytic wave surface drives rendered swell and the vessel's lightweight four-point spring-buoyancy approximation; it is not a full fluid/current simulation. The dead-calm and wrong-way-swell horror incidents are presentation events only: those incident flags do not independently alter buoyancy, handling, fishing success, navigation or threat AI.
- The event does not itself increase panic or cause damage.

### Future escalation ideas

1. A buoy continues swinging after the surrounding water has gone flat.
2. Rain strikes the sea, but individual impact ripples do not form.
3. A wave approaches the boat and breaks with no wind or swell behind it.
4. The sea resumes moving, but every visible wave travels toward the boat for several seconds.
5. A distant vessel's wake reaches the player before the vessel can be seen.
6. The player hears water running beneath the hull while the surface remains completely still.

These are authored anomaly candidates, not all implemented. Keep them rare, spatially grounded and separated by ordinary working time.

## Deep-ocean and mariner-experience integration

The research-to-gameplay rules are defined in [Deep-Ocean Phenomena and Mariner Experiences](DEEP-OCEAN-PHENOMENA-AND-MARINER-EXPERIENCES.md). The balancing authority is [Maritime Anomaly Rules](../07_GAME_RULES_AND_BALANCING/MARITIME-ANOMALY-RULES.md).

Design additions:
- The Outer Shoals may host a real deep scattering layer whose apparent depth changes with diel vertical migration. It is an acoustic ecology volume, not a moving seafloor.
- The Hollow Trench may host surveyed brine pools, gas hydrates, cold seeps, hydrothermal vents where geology permits, and whale-fall ecosystems.
- Brinicles are excluded from the current warm-water island-arc setting.
- Hull knocks, damaged longline gear, drifting vessels, and luminous water begin with plausible ordinary explanations.
- “Merfrog,” “Quackers,” the straightened-alloy-hook account, the reflective wall, and the alleged Mid-Atlantic precision holes remain unverified reports / claims unless primary evidence is found.
- Operational Strain is a proposed future hidden workload model, not an implemented meter. It remains separate from the existing Panic system.

## Acceptance checks

- Ordinary daylight travel remains mostly uneventful.
- Natural phenomena follow habitat, depth, time, and environmental rules.
- Night or high pressure can produce an incident, but does not guarantee one on every trip.
- Ambient sounds are spatialized and remain quieter than safety / navigation cues.
- A false light has no collider and cannot be used as a physical waypoint.
- Ambient incidents do not change panic, save data, cargo, vessel condition or creature AI unless a separately specified system explicitly owns that outcome.
- Genuine threats retain their existing warning timings and escape rules.
- Psychological effects never invert controls, conceal real warnings, or gate essential evidence.
- Final sound assets require licensing / attribution and mono / accessibility review.

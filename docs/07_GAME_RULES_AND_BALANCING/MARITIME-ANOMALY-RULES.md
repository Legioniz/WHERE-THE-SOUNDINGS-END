# Maritime Anomaly Rules and Encounter Balancing

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed balancing specification — values are starting targets, not validated tuning  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## 1. Design contract

Maritime anomalies must emerge from the world simulation and existing threat architecture. This document does not create a second threat spawner and does not override the Legendary Encounter Scheduler or The Sleeper.

| Class | Examples | Gameplay authority |
|---|---|---|
| Natural phenomenon | Deep scattering layer, bioluminescence, current front, brine pool, vent plume | Ecology / environment system |
| Human maritime incident | Damaged line, hull knock, drifting vessel, debris, equipment failure | Interaction / world-event system |
| Psychological perception event | False light, auditory misinterpretation, bearing uncertainty | Operational Strain + Panic presentation |
| Fictional anomaly | Impossible return, sound with no source, wrong-way wake, persistent boundary | Existing anomaly / legendary scheduler |

A natural or human event must not silently become supernatural. A fictional anomaly needs a stable identity, eligibility rules, evidence trail, and resolution.

## 2. Event data contract

Every event definition must include:

- Stable event ID and event content version.
- Event class: natural, human, psychological, or fictional.
- Region ID and optional site ID.
- Depth range where relevant.
- Time, tide, weather, and visibility windows.
- Required and excluded progression flags.
- Opportunity type and stable opportunity ID.
- Base chance or deterministic trigger.
- Cooldown and repeat policy.
- Warning channels and caption text.
- Counterplay and valid escape anchors.
- Outcome and evidence-record ID.
- Save policy and accessibility fallback.

Do not use scene object names as permanent event identity. Runtime presentation objects may be rebuilt from the event record.

## 3. Opportunity and RNG rules

1. Evaluate only at a meaningful boundary: region / named-site entry, voyage segment, tide turn, weather transition, qualifying Sounding Rig pulse, salvage action, or authored quest flag.
2. Never roll every frame or every second.
3. Use an independent deterministic RNG stream for ambient anomalies. Cosmetic particles and audio variation must not consume gameplay RNG.
4. Build the random key from event ID, opportunity ID, and world seed.
5. Persist the result as soon as the opportunity is evaluated, including a failed roll.
6. Save/load, scene reload, repeated button press, or repeated rig pulse inside the same opportunity cannot reroll it.
7. If an event is deferred because another major threat is active, preserve the pending opportunity; do not silently convert it into a new roll.
8. Required story evidence must never be gated behind chance. RNG may vary its timing, location within an authored area, or presentation after the mandatory clue is secured.

## 4. Frequency and cooldown starting targets

These are proposed tuning ranges, not live runtime settings unless explicitly implemented.

| Event type | Evaluation boundary | Starting frequency target | Cooldown / repeat rule |
|---|---|---|---|
| Ordinary natural activity | Ecology schedule / environmental update | Common where habitat and time support it | Driven by ecology, not horror cooldown |
| Minor human incident | Voyage segment or relevant interaction | Occasional; target no more than one authored incident in a 20–40 in-game minute window | 1–3 in-game hours per incident family |
| Ambient psychological perception | Long exposure + strain / panic context | Rare; target one noticeable event per 45–90 in-game minutes at most | At least 2 in-game hours; avoid same effect on consecutive opportunities |
| Non-lethal fictional anomaly | Named site / qualifying instrument / environmental boundary | Very rare; target no more than one major anomaly sign per 1–2 in-game days | Individual cooldown 2–4 days |
| Lethal / major contact | Legendary scheduler only | Exceptional; never a background random interruption | Existing legendary global and individual cooldowns |

**Spacing rule:** do not stack multiple high-attention incidents. If a warning, salvage interaction, collision recovery, radio dialogue, or quest scene is active, defer non-essential ambience.

**Quiet rule:** an uneventful voyage is a valid outcome. Do not fill every travel interval with a scare.

## 5. Natural phenomenon behaviour rules

### 5.1 Deep scattering layer
- Use a depth band / density volume, not a terrain collider.
- Density and depth respond to time of day and local ecological conditions.
- The player can distinguish it from the bottom through repeated pings, depth comparison, and observation over time.
- A fictional persistence error requires its own anomaly event and evidence.

### 5.2 Brine pool
- Confined to a mapped seabed depression.
- Player vessel remains above the water column; the pool does not affect surface buoyancy.
- Danger applies to bottom-contact equipment, divers, or organisms that cross the chemical interface.
- Require a survey clue and visible boundary before any equipment loss.
- No instant player death from mere proximity.

### 5.3 Vents, seeps, and hydrates
- Vent and seep sites have distinct data profiles, temperatures, chemistry, fauna, and visual effects.
- Vent plume and unstable substrate can affect a deployed probe or bottom tool.
- Cold seep chemistry is not a boiling-water hazard.
- Gas hydrate events remain local to sediment and must be supported by geology.
- Do not trigger a large-scale collapse or tsunami from an ordinary player interaction.

### 5.4 Diel vertical migration
- Evaluate on local dawn / dusk and light windows.
- The scattering layer and associated fauna move as a coordinated ecological volume.
- Predators may follow prey; they do not all ascend in a single synchronized monster wave.
- Sounding Rig returns change with layer depth and density.

### 5.5 Whale fall and Osedax
- The whale fall is a persistent ecological site with staged succession.
- Scavenger activity changes with remaining soft tissue, exposed bone, and elapsed time.
- Osedax are small attached bone-associated worms, not hostile enemies.
- Protected whale remains cannot enter ordinary catch / sale inventory.

## 6. Human maritime incident rules

### 6.1 Gear and line damage
- Resolve plausible causes first: load, snag, abrasion, corrosion, knot failure, fatigue, animal bite, or propeller interaction.
- Inspection must show at least one clue before the player commits to repair or replacement.
- A rare impossible result needs corroborating sound, trace, second damaged component, or repeatable location evidence.
- One bent hook never proves a named entity.

### 6.2 Hull knocks
- Ordinary knocks may be generated by hull movement, loose equipment, rope, debris, wave impact, or wildlife.
- A knock alone does not change threat state, panic, hull condition, or navigation.
- A hostile threat uses its own explicit warning event and state machine.
- Repeated knocks may create a journal clue only after a stable event threshold or player investigation.

### 6.3 Drifting vessel / ghost ship
- Simulate drift from wind, current, wave, and hull condition unless a fictional anomaly is explicitly active.
- Boarding is permitted only when relative motion and distance are safe.
- Preserve rescue-first logic when a survivor may be present.
- A vessel may be empty, damaged, occupied, or misleading; boarding is not always a loot reward.
- A phantom vessel is a visual / navigational event until the legendary scheduler authorizes contact.

### 6.4 Debris and contaminated salvage
- Unknown containers require inspection before handling.
- Contamination, rot, sharp edges, entanglement, and unstable load are valid hazards.
- Do not force bare-hand interaction with hazardous contents.
- An anecdotal freezer incident may inspire one authored optional event, never a repeated random punishment.

## 7. Operational Strain and Panic

### 7.1 Separate responsibilities
- **Operational Strain** is a proposed hidden state representing accumulated fatigue, poor rest, long monotony, and workload.
- **Panic** remains the existing fear response and existing HUD value.
- Strain can increase perceptual uncertainty; Panic can increase threat sensitivity.
- Do not add a second visible meter until the game has a meaningful rest / sleep loop.

### 7.2 Strain inputs
Potential contributors:
- Continuous time away from a safe harbour.
- Long periods without rest.
- Repeated high-workload actions such as difficult fishing, salvage, repairs, and navigation in poor visibility.
- Disrupted sleep schedule if rest is implemented.
- Extended exposure to monotonous sound and low-visibility conditions.

Potential recovery:
- Meaningful rest in a safe location.
- Return to a lit, inhabited harbour.
- Completed sleep period if a rest system exists.
- Reduced workload during a safe weather window.

Do not increase strain merely because the player is in darkness. Night is not itself a mental illness trigger.

### 7.3 Presentation bands (proposed)

Use a normalized internal value from 0.0 to 1.0 only if implemented.

| Strain | Permitted effect | Prohibited effect |
|---|---|---|
| 0.00–0.24 | No perceptual penalty; ordinary ambience | No hallucination |
| 0.25–0.49 | Slightly slower confidence recovery after ambiguous signals | No control interference |
| 0.50–0.74 | Rare false-positive audio / light interpretation; clear uncertainty cues | No damage or hidden threat |
| 0.75–1.00 | Brief authored perceptual event; stronger uncertainty in non-critical instrument presentation | No false lethal telegraph, quest lock, or forced steering |

Strain is not a sanity meter and must not label the protagonist as “going insane.” It is a workload and recovery model.

### 7.4 Verification and player agency

Every perceptual event must offer a way to check it:
- Compare compass / bearing against a known landmark.
- Repeat a Sounding Rig ping from a different heading.
- Wait for the sound or light to resolve.
- Inspect a physical trace.
- Check a chart, log, or radio message.
- Return to a safe harbour.

Do not invert controls, delete map knowledge, alter cargo, spawn collision geometry, or hide an actual attack warning because of a perception event.

## 8. Warning and danger rules

- Natural hazards communicate through environmental evidence: depth, current, temperature, visibility, plume, seabed shape, and animal behaviour.
- Human incidents communicate through equipment condition, sound, line tension, drift, and visible damage.
- Fictional threats use at least two independent warning channels before lethal contact unless a specifically authored story scene is non-interactive.
- A warning may be visual + acoustic, acoustic + instrument, or instrument + environmental response.
- Critical warnings require captions and a non-audio cue.
- A player who responds correctly must have a real escape route, not merely a cosmetic choice.
- Ambient anomalies cannot damage the hull or remove cargo unless they are promoted through the correct threat / hazard authority and warning phase.

## 9. Save and load contract

Persist:
- Evaluated opportunity IDs and outcomes.
- Active event IDs and phase.
- Per-event cooldowns.
- Discovered / inspected evidence.
- Persistent site state (for example, whale-fall stage or surveyed brine pool).
- Operational Strain only if implemented as progression-relevant state.
- Damaged or consumed equipment caused by an explicitly resolved incident.

Do not persist cosmetic audio randomization or transient hallucination visuals after their event has expired.

On load:
- Never reroll a resolved or failed opportunity.
- Restore active events only if their time window remains valid.
- If the event window expired, resolve it as expired without punishment.
- Never resume an attack in a state that skips its warning.

## 10. Accessibility and fairness

- All critical audio cues receive captions.
- Directional audio has a text / compass equivalent.
- Flashing light and high-contrast hallucination effects need intensity controls.
- No essential clue depends on hearing a low-frequency sound.
- Do not use prolonged silence to conceal an unavoidable attack.
- Offer reduced motion and reduced audio dynamic range options.
- Keep real danger warnings visually distinguishable from hallucination presentation.

## 11. Validation checklist

- [ ] Same seed + same opportunity ID produces the same gameplay result.
- [ ] Save/load cannot reroll the event.
- [ ] Natural phenomenon follows its real environmental gates.
- [ ] Human incident has a plausible first explanation.
- [ ] Fictional anomaly has an evidence label and separate anomaly ID.
- [ ] No two major attention events overlap.
- [ ] No ambient event silently damages the boat or removes cargo.
- [ ] Any lethal event uses the existing threat / legendary warning and escape contract.
- [ ] Critical information is available without audio.
- [ ] Psychological presentation never changes player controls or hides real threats.
- [ ] Region-inactive simulation cannot kill the player or destroy quest-critical evidence.
- [ ] Unity implementation has Edit Mode tests and Play Mode verification before being marked implemented.

## 12. Related specifications

- [Deep-Ocean Phenomena and Mariner Experiences](../06_GAMEPLAY_SYSTEMS/DEEP-OCEAN-PHENOMENA-AND-MARINER-EXPERIENCES.md)
- [Maritime Horror Director](../06_GAMEPLAY_SYSTEMS/MARITIME-HORROR-DIRECTOR.md)
- [World Simulation and Event Scheduler](../12_AI_AND_SIMULATION/WORLD-SIMULATION.md)
- [Legendary Maritime Encounter Scheduler](../12_AI_AND_SIMULATION/LEGENDARY-ENCOUNTER-SCHEDULER.md)
- [Audio Design](../10_AUDIO_AND_SOUND_DESIGN/AUDIO-DESIGN-DOCUMENT.md)
- [Save and Load](../06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md)

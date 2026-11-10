# Audio Design Document

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## 1. Audio hierarchy

1. Safety and navigation cues.
2. Player interaction feedback.
3. Threat warning and attack cues.
4. Quest / narrative signals.
5. Environment ambience.
6. Music.

Critical gameplay information must not be audio-only. Provide captions, visual direction cues, and optional simplified waveform / instrument feedback.

## 2. SoundEvent contract

Gameplay sounds emit data, not just AudioSource playback:

SoundEvent fields: id, sourceId, category, band, loudness, position, duration, timestamp, eventKey.

The event bus allows acoustic navigation and AI perception to respond to the same sound. Do not have each creature independently scan every AudioSource in the scene.

## 3. Threat audio states

- Dormant: no attention-grabbing cue; normal ambient bed.
- Investigating: distant irregular low-frequency tone or pressure-like resonance.
- Warning: distinct directional cue, short pulse, wake / hull vibration.
- Stalking: intermittent sound that moves relative to the boat; not a constant loop.
- Pursuing: increasing rhythmic density and directional movement.
- Attack telegraph: unique 2.2–3.0 s cue that cannot be confused with ordinary waves.
- Impact: transient hull / water hit, then audio ducking to expose damage and recovery cues.
- Disengaging: movement recedes and rhythm separates from engine noise.

## 4. Acoustic identity

Each anomaly has a distinct pattern:
- Voice Under the Keel: familiar-voice contour with impossible repeated interval; no literal voice actor impersonation of a real character.
- Bells Beneath the Field: distant bell harmonics with water-filtered decay.
- Sleeper: low pressure tone, chain resonance, and a short pre-strike directional swell.
- Reef That Answers: first ping, delayed return, second return with changed bearing.
- Long Call: long animal-like phrase with a natural call layer and one authored unexplained repetition.

## 5. Deep-ocean acoustic catalogue

| Sound family | Real-world reference | Mix / gameplay treatment |
|---|---|---|
| Deep scattering layer | Many organisms reflecting sonar in a migrating water-column layer | Use a broad, layered return with multiple depths; do not play it as a single creature growl |
| Vent field | Fluid discharge, plume turbulence, and biological activity | Low continuous texture with irregular mineral / bubble transients; distinguish vent from cold seep |
| Cold seep | Gas release and seep-associated animals | Sparse bubbles and subtle activity; no boiling-water roar |
| Brine pool boundary | Acoustic / visual instrument response at a density and chemistry interface | Quiet sensor change and camera boundary; not a monster vocalisation |
| Whale fall | Scavenger activity and deep-water ambience | Small, close biological detail; the site should feel active but not hostile |
| Bioluminescent surface event | Water movement and luminous plankton | Audio remains subtle; glow is not accompanied by an arbitrary supernatural sting |
| Hull knock | Hull, rope, debris, wave, or animal contact | Keep source ambiguity; use two or three damped transients, not a constant knocking loop |
| Damaged fishing line | Tension, abrasion, snapping leader, reel / winch load | Give the player a useful mechanical clue before any anomaly cue |
| Drifting vessel | Rigging, loose fittings, engine cooling, hull slap, water ingress | Human-scale, irregular sounds; silence can be more effective than ghostly choir effects |

The names “Merfrog,” “Quackers,” and similar internet labels are not approved scientific sound categories. They may appear as an NPC's unverified nickname in a field note, never as an official classification.

## 6. Isolation, fatigue, and perception

Long exposure to repetitive machinery, poor sleep, monotonous horizons, and uncertain bearings can change attention and perceived sound. Use this carefully:
- A player may become more attentive to a repeated engine rhythm or faint hull vibration.
- A false-positive sound should be short and distinguishable from critical navigation / threat cues.
- A caption should identify the perceptual uncertainty without declaring the source.
- A verification action may remove or reclassify the sound.
- Do not add random voices saying the player's name as a routine fatigue effect.
- Do not use a hallucinated sound to conceal a real attack warning.
- Do not make low-frequency sound the only channel for critical information.

Operational Strain is a proposed future system and is not currently implemented. Panic remains a separate existing gameplay system.

## 7. Mix and player feedback

- Duck ambient bed by 2–4 dB during a critical warning rather than raising every threat sound excessively.
- Keep engine, wave, and threat bands separated enough for the player to recognize their source.
- Use non-spatial caption tags such as [low vibration behind the boat] or [bell, distant north-east].
- Do not use sub-bass alone; speakers and accessibility conditions vary.
- Include mono compatibility checks and adjustable dynamic range.
- Preserve a recognizable ambient baseline before introducing silence or missing-frequency effects.
- Never remove all audio during a lethal warning unless an equivalent visual / caption warning remains.

## 8. Audio implementation

Use Unity AudioSource / AudioMixer for playback, with a shared event bus for gameplay metadata. Audio playback should not own AI state. The threat state machine emits cue events; audio presentation maps them to authored clips / snapshots. Keep spatialization, attenuation, and ducking values in data assets.

## 9. Acceptance tests

- Critical warning is heard and captioned.
- Cue direction works in stereo and has a clear mono caption.
- Audio mixer does not mask the engine / navigation cue.
- Repeated event reload does not duplicate a persistent audio cue.
- Different threat states use distinct cues and timing.
- Natural acoustic ecology does not sound like a threat by default.
- A false-positive perception cue cannot mask a real warning.
- Mono playback retains critical information.

## 10. Greyhaven prototype status — 2026-11-10

The runtime FishermanAtmosphereDirector adds a restrained first layer of maritime unease when a scene contains SeaStateDirector:
- two hollow, spatialized hull knocks;
- a distant synthesized navigation bell;
- a false warm navigation light that fades without a wake;
- a quiet distant animal-like call.

These cues are gated by advancing sea time and night / elevated panic. They are not final audio assets and do not alter panic, vessel condition, cargo, navigation, or creature state. The procedural clips are temporary placeholders pending properly licensed or recorded field recordings.

See [Maritime Horror Director](../06_GAMEPLAY_SYSTEMS/MARITIME-HORROR-DIRECTOR.md), [Deep-Ocean Phenomena and Mariner Experiences](../06_GAMEPLAY_SYSTEMS/DEEP-OCEAN-PHENOMENA-AND-MARINER-EXPERIENCES.md), and [Maritime Anomaly Rules](../07_GAME_RULES_AND_BALANCING/MARITIME-ANOMALY-RULES.md). Authored ambience, a music score, mixer snapshots, radio dialogue and the final threat-audio event bus remain future work.

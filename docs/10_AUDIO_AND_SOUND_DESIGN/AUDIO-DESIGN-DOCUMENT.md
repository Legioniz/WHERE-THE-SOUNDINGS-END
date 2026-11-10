# Audio Design Document

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
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

`SoundEvent { id, sourceId, category, band, loudness, position, duration, timestamp, eventKey }`

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

## 5. Mix and player feedback

- Duck ambient bed by 2–4 dB during a critical warning rather than raising every threat sound excessively.
- Keep engine, wave, and threat bands separated enough for the player to recognize their source.
- Use non-spatial caption tags such as [low vibration behind the boat] or [bell, distant north-east].
- Do not use sub-bass alone; speakers and accessibility conditions vary.
- Include mono compatibility checks and adjustable dynamic range.

## 6. Audio implementation

Use Unity AudioSource / AudioMixer for playback, with a shared event bus for gameplay metadata. Audio playback should not own AI state. The threat state machine emits cue events; audio presentation maps them to authored clips / snapshots. Keep spatialization, attenuation, and ducking values in data assets.

## 7. Acceptance tests

- Critical warning is heard and captioned.
- Cue direction works in stereo and has a clear mono caption.
- Audio mixer does not mask the engine / navigation cue.
- Repeated event reload does not duplicate a persistent audio cue.
- Different threat states use distinct cues and timing.


## 8. Greyhaven prototype status — 2026-11-10

The runtime `FishermanAtmosphereDirector` adds a restrained first layer of maritime unease when a scene contains `SeaStateDirector`:

- two hollow, spatialized hull knocks;
- a distant synthesized navigation bell;
- a false warm navigation light that fades without a wake;
- a quiet distant animal-like call.

These cues are gated by advancing sea time and night / elevated panic. They are not final audio assets and do not alter panic, vessel condition, cargo, navigation, or creature state. The procedural clips are temporary placeholders pending properly licensed or recorded field recordings.

See `docs/06_GAMEPLAY_SYSTEMS/MARITIME-HORROR-DIRECTOR.md` for the full creative rules and acceptance criteria. Authored ambience, a music score, mixer snapshots, radio dialogue and the final threat-audio event bus remain future work.

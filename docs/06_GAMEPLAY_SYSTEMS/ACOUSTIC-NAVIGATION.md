# Acoustic Navigation

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Player-facing fantasy

When fog removes landmarks, the player navigates by interpreting the sounds of the coast: lighthouse bell, buoy bells, surf, engine reflections, radio markers, and the Sounding Rig. Audio is useful but never omniscient.

## 2. Signal sources

| Source | Pattern | Reliability | Gameplay meaning |
|---|---|---|---|
| Lighthouse | Regular, long-range repeating bell | High when audible | Broad bearing toward Greyhaven |
| Bell buoy | Short repeating paired tone | High within authored range | Local hazard / channel marker |
| Surf against rock | Broad, irregular noise | Medium | Coastline and rock proximity |
| Own engine reflection | Changes with terrain and heading | Low–medium | Nearby geometry clue |
| Radio marker | Repeating coded pulse | Medium–high | Quest or navigation marker |
| Sounding Rig | Player-controlled pulse + return | Variable | Depth / object / anomaly hypothesis |
| Creature call | Irregular authored phrase | Unknown | Possible living source / threat cue |

## 3. Bearing and confidence

Every source should provide a bearing estimate and confidence, not an exact world coordinate. The player triangulates by listening from two or more positions or comparing instruments.

[
Confidence = S \, R \, Q
]
- (S): signal strength after attenuation;
- (R): repetition / pattern stability;
- (Q): known-source reliability.

A false or reactive signal can have high strength but low pattern stability. Display confidence with an icon / text label and waveform stability; never show a falsely precise GPS marker for an unknown source.

## 4. Player actions

- Slow or stop engine to improve listening.
- Rotate / reposition the boat to compare bearings.
- Wait for another buoy cycle.
- Use the compass and chart as independent references.
- Deploy the Sounding Rig for depth / echo evidence.
- Mark a bearing with a chart note.

The player should not be forced to use headphones or perfect stereo localization. Provide visual bearing arc and subtitles for authored cues.

## 5. Acoustic event triggers

- Buoy and lighthouse signals run on authored schedules.
- Signal anomalies trigger on explicit opportunity events (Rig deployment, tide transition, quest location entry).
- Threat calls are emitted by the threat state machine.
- Ambient animal calls are schedule / region based and do not automatically imply danger.

A signal event is not itself damage. A signal can mislead the player only when the world also supplies enough evidence to question it.

## 6. Implementation

Create `AcousticSourceDefinition` data assets for ID, band, pattern, repetition interval, nominal loudness, attenuation profile, reliability, region, and accessibility caption. Runtime `AcousticSourceState` owns phase, current position, and enabled state.

Create a shared `SoundEventBus`; systems subscribe by sound category and sensitivity. The first version should use authored source positions, attenuation curves, and noise/masking—not physical wave simulation.

## 7. Tests

- Bearing changes as player position changes.
- Signal strength falls with distance and masking.
- Known buoy patterns remain distinguishable from anomaly patterns.
- A player can triangulate a source with two observations.
- An acoustic signal never bypasses safe-zone or quest gating.

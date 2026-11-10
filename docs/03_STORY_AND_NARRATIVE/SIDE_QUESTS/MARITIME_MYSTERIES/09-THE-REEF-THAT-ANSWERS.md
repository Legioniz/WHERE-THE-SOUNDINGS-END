# The Reef That Answers

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed / Non-canon  
> Version: 0.1  
> Type: Sounding Rig puzzle / acoustic cartography  
> Inspiration: Original fictional concept informed by the project's acoustic-navigation pillar

## Short lore

> “The reef returned every ping twice. The second answer came from a place the chart said was open water. When the player changed the rig's weight, the second answer moved.”

A section of reef produces a delayed echo that seems to come from different directions depending on how the Sounding Rig is deployed.

## Player hook

The lighthouse keeper gives the player a damaged chart with a blank area marked only by three question marks. Several boats have reported false depth readings near that coordinate.

## Player activity

1. Deploy the Sounding Rig at three marked points.
2. Change deployment depth and record the echo delay.
3. Compare the apparent source with the boat's known position.
4. Identify a curved reef wall that redirects the signal.
5. Chart a safe passage using two independent reference points.

## Outcome

The player creates the first reliable acoustic map of the reef. The map becomes a new navigational tool and a future threat-warning location. The reef does not speak or think; the mystery is how the environment changes what the player hears.

## Reward

A permanent chart layer and improved confidence display for one known reef region.

## Dependencies

- Sounding Rig deployment, depth, and echo delay.
- Basic acoustic attenuation / reflection approximation.
- Chart annotation.
- A small test area with known geometry.

## Research and originality note

This is an original fictional quest, not a traditional legend. Its purpose is to showcase the signature Sounding Rig and teach the player that a signal is evidence that needs interpretation.

## Scope note

A deterministic echo table based on hand-authored geometry is sufficient. Do not implement full underwater wave acoustics for the prototype.


## Runtime Behaviour and Implementation

### Event trigger and RNG
- An echo response is evaluated on each valid Rig pulse inside the reef bounds.
- Shifted-bearing anomaly chance: **25% per eligible pulse**. The quest's required two-return pattern is guaranteed by the third valid pulse.
- Use the fixed reef geometry and event seed to select one of three authored echo profiles. Same location + same profile + same pulse configuration must reproduce the same result.

### Behaviour states
`Ordinary Reef Return → Delayed Echo → Bearing Offset → Geometry Hypothesis → Safe Route Charted`.
The reef does not move or target the player. A curved rock wall creates the first return; an authored secondary reflection produces the apparent second source. A confidence indicator falls when the bearing conflicts with compass / known landmarks.

### Perception / FOV
No creature FOV. Reef geometry can be visible from 20–60 m depending on fog and lighting; dangerous rock edges should be marked by surface foam, buoy, chart warning, or Rig depth band. The elevated camera must retain enough near-boat view to steer.

### Sound behaviour
The Rig ping is followed by an echo after 0.4–1.4 seconds. A shifted echo appears slightly lower in level and altered in stereo position. The waveform UI shows the delay and a confidence warning; do not rely on stereo alone.

### Attacks and chase
**None.** No creature attack, pursuit, or hull damage from the sound. Rocks are physical collision hazards with fair visual / instrument warning. Do not let an echo teleport the player's boat or change the actual current vector.

### Lore reveal
The chart-maker had mistaken a repeated reflection for a second reef. The old chart's "open water" gap was an interpretation error. The mystery is how the same sound can create a convincing false map, not whether the reef is alive.

### Acceptance tests
- Echo profile repeats deterministically for a fixed save.
- The player can solve it using waveform and compass, including mono audio.
- No damage or pursuit event is emitted by the echo.
- Required pattern is available by the third valid pulse.

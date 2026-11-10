# Fog and Visibility

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## 1. Design goal

Fog reduces certainty, not fairness. It should conceal distance and shape while preserving enough visual, audio, and UI information for the player to make a decision. The elevated 2.5D camera does not use a first-person player vision cone by default.

## 2. Separate visibility systems

- **Camera visibility:** what is rendered inside the camera frustum.
- **Player awareness:** information the player can reasonably infer from sight, audio, map, and instruments.
- **Entity visibility:** what a creature can visually detect from its own position and orientation.
- **Sound detection:** independent of camera and visual FOV.

Do not confuse camera frustum FOV with creature detection FOV. A threat may be outside the current screen but must provide a directional cue before it can hit the boat.

## 3. Fog bands

Prototype distances (in world metres; 1 Unity unit = 1 metre once scale is locked):

| Weather | Clear visual range | Threat silhouette range | Navigation cue |
|---|---:|---:|---|
| Clear | 220 m | 70 m | Landmarks, lights, buoys |
| Light mist | 140 m | 55 m | Landmarks + acoustic cues |
| Heavy fog | 65 m | 30 m | Buoys, lighthouse, compass, sound |
| Dense fog event | 35 m | 18 m | Short-range cues and instruments |

Values are initial tuning targets. Keep collision hazard readability higher than distant scenic detail: rocks that can damage the boat need a silhouette, buoy, wake, map cue, or advance warning.

## 4. Visibility transmission

For AI sensing and gameplay cue confidence, use a shared fog transmission factor (Fin[0,1]) derived from distance and density:

[
F(d) = e^{-k d}
]

Use authored density per weather state; no physically accurate volumetric-scattering model is needed for AI. Visual score:

[
V = C \, L \, M \, O \, F
]

where (C) is distance falloff, (L) light contrast, (M) wake / movement exposure, (O) occlusion, and (F) fog transmission. See [Entity Perception and Encounter Math](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md).

## 5. Player threat awareness

Because the game uses an elevated camera, threats should be readable through more than direct visibility:
- directional wake / ripple;
- short low-frequency hull vibration;
- Sounding Rig needle / waveform deviation;
- localized fog displacement or silhouette;
- radio / lighthouse warning when story-appropriate.

At least one of these must be available before hostile impact. Avoid simultaneous visual and audio masking of every clue.

## 6. Light

Lanterns increase local player visibility and may also increase the boat's contrast to a hostile entity. Light should be a tactical choice, not a universal detection shield:
- lantern on: larger player view and clearer nearby hazards, higher visual contrast to entities;
- lantern off: less visual range, lower contrast, but no immunity from acoustic sensing;
- flare: brief high-contrast visibility and a loud signal event.

## 7. Accessibility

Every important directional threat cue needs a non-spatial visual equivalent (edge indicator, caption, instrument pulse, or threat-state icon). Subtitles identify the cue without revealing an unknown entity's identity. Provide separate sliders for ambience, threat cues, radio, and effects.

## 8. Acceptance tests

Test clear, mist, heavy fog, and dense fog. Confirm that the player can identify an imminent collision and a threat warning without relying solely on color or stereo audio. Confirm that fog affects entity visual detection but not sound-event intake.

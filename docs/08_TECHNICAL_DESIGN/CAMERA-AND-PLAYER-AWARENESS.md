# Camera Field of View and Player Awareness

> Project: WHERE THE SOUNDINGS END  
> Status: Prototype design specification  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## 1. What "FOV" means in this game

The player is represented by a boat in an elevated-camera 2.5D world. The player does not have a first-person eye position or a biological vision cone. There are three separate concepts:

1. **Camera FOV / frustum:** which part of the 3D world is rendered on screen.
2. **Player awareness:** which events the player can reasonably understand through screen, sound, chart, instruments, and captions.
3. **Threat FOV:** the creature's detection cone used by its AI to acquire the boat.

Do not implement a narrow first-person player vision cone unless the game design changes to a first-person mode.

## 2. Camera prototype

Keep camera projection an adjustable prototype decision. Test a perspective follow camera against an orthographic camera using the same boat route and fog settings. Select based on navigation readability, horizon / island composition, and the ability to judge collision distance.

For a perspective prototype, begin around 50–60° vertical camera FOV, an elevated trailing angle, and a stable follow distance. These are starting values, not locked specifications. If orthographic wins the visual test, FOV is not the correct control; tune orthographic size and camera framing instead.

The camera must:
- keep the boat and near-water hazard zone readable;
- avoid abrupt auto-rotation toward threats;
- reveal enough water ahead for steering;
- avoid placing critical clues behind permanent UI overlays;
- preserve the horizon / fog composition that sells distance.

## 3. Player awareness rules

The player should notice a threat through at least one of:
- threat silhouette / wake inside camera view;
- directional surface disturbance;
- hull vibration or directional audio cue;
- Sounding Rig response;
- chart / compass / HUD warning.

If a hostile is close enough to strike but off-screen, a directional cue is mandatory. A visual edge indicator may point toward a threat without identifying it. Do not show an exact threat location through fog or walls.

## 4. Near-field and far-field presentation

| Range band | Player feedback |
|---|---|
| 0–20 m | Boat wake, collision geometry, surface disturbance, close hull sound |
| 20–60 m | Silhouette / wake in clear or moderate fog; directional cue |
| 60–120 m | Large movement or plume only in clear conditions; sound / instrument bearing |
| Beyond 120 m | No precise target marker; environmental / acoustic clue only |

These are initial presentation bands. Fog may reduce the visual range. A critical collision hazard must receive a cue before it enters the unavoidable collision radius.

## 5. Threat detection remains separate

The Sleeper's AI uses its own 100° total visual cone, clear-water visual range of 70 m, acquisition dwell, line-of-sight test, and acoustic listener. The player's camera FOV does not determine whether the threat detects the boat. See [Entity Perception and Encounter Math](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md).

## 6. Accessibility

Directional audio must have a visual equivalent. Threat awareness must not depend on subtle color alone. Provide screen-space direction, a text caption, and an optional simplified threat indicator. The player should be able to understand the warning without spatial headphones.

## 7. Acceptance tests

- Camera framing does not hide a hazard within unavoidable collision range.
- Threat can be off-screen but its pre-attack cue remains available.
- Player can identify cue direction with mono audio or captions.
- Camera does not auto-lock onto or reveal a hidden source.
- Perspective and orthographic alternatives are tested before the projection is locked.

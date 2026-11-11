# Camera Field of View and Player Awareness

> Project: WHERE THE SOUNDINGS END  
> Status: Greyhaven prototype runtime implementation; Unity verification pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

## 1. What "FOV" means in this game

The player is represented by a boat in an elevated-camera 2.5D world. The player does not have a first-person eye position or a biological vision cone. There are three separate concepts:

1. **Camera FOV / frustum:** which part of the 3D world is rendered on screen.
2. **Player awareness:** which events the player can reasonably understand through screen, sound, chart, instruments, and captions.
3. **Threat FOV:** the creature's detection cone used by its AI to acquire the boat.

Do not implement a narrow first-person player vision cone unless the game design changes to a first-person mode.

## 2. Camera presentation baseline

The Greyhaven prototype uses an elevated, fixed-angle orthographic camera. Keep that projection for the current 2.5D blockout; perspective remains a comparison candidate for a later visual test, not an assumption that the camera should change projection now.

The generated Greyhaven camera currently starts from:
- Orthographic size: 35 world units.
- Elevated pitch: 55 degrees.
- Trailing offset: (0, 48, -36) world units.
- Position smoothing: 0.28 seconds.

These are prototype framing values. Revisit them only through a repeatable route/readability comparison with the same boat, hazards, coastline and fog.

The camera must:
- keep the boat and near-water hazard zone readable;
- reveal enough water ahead for steering;
- avoid abrupt auto-rotation toward threats;
- avoid placing critical clues behind permanent UI overlays;
- preserve the horizon / fog composition that sells distance;
- retain a stable elevated frame instead of rotating with every boat heading change.

## 3. Movement-aware follow behaviour

CameraFollow2_5D owns camera presentation only. Boat position and movement remain owned by the boat Rigidbody/controller.

The camera follows the boat in LateUpdate and uses the Rigidbody's horizontal linear velocity to add a damped look-ahead offset. It follows actual travel direction rather than the boat's nose, so drift and reverse movement remain visible in the framing.

Initial serialized tuning:
- Look-ahead time: 0.72 seconds.
- Maximum look-ahead distance: 6 world units.
- Position damping: 0.28 seconds.
- Turn-bank sensitivity: 1.35 degrees per radian/second of yaw rate.
- Maximum turn bank: 2.5 degrees.
- Rotation damping: 0.34 seconds.

Turning response is a small camera roll derived from Rigidbody yaw angular velocity. The camera does not yaw-lock to the vessel, chase a creature, or reveal a hidden threat. The helper math is isolated in CameraFollowMath for deterministic Edit Mode tests.

## 4. Shared ocean-surface response

The camera samples MaritimeSurfaceBehaviour, the same component that drives the Greyhaven water mesh and the dead-calm / wrong-way-swell incidents.

- The camera follows the boat's actual vertical heave from the Rigidbody buoyancy simulation; it does not add a second wave-height offset.
- Camera pitch and roll follow a restrained, damped, clamped fraction of the sampled surface normal (default influence 0.25; maximum wave tilt 1.5 degrees).
- Dead calm naturally reduces wave response as the surface amplitude settles.
- Wrong-way swell changes the surface pattern that the camera samples; the camera does not invent a second wave model.
- The surface sampler transforms normals correctly for scaled water planes.

The hull responds to the same sampled surface through four-point Rigidbody buoyancy. Camera heave therefore comes from the physical vessel transform; adding another vertical wave offset would double-count the swell and exaggerate motion. The remaining camera tilt is presentation-only and never feeds back into boat physics.

The ocean surface is explicitly created by the Greyhaven Editor builder and passed to the Greyhaven camera. The deep-ocean survey builder now also passes its scene water surface to the survey camera. FishermanAtmosphereDirector continues to own incident timing and calls the existing surface triggers. The camera does not own or schedule maritime incidents.

## 5. Player awareness rules

The player should notice a threat through at least one of:
- threat silhouette / wake inside camera view;
- directional surface disturbance;
- hull vibration or directional audio cue;
- Sounding Rig response;
- chart / compass / HUD warning.

If a hostile is close enough to strike but off-screen, a directional cue is mandatory. A visual edge indicator may point toward a threat without identifying it. Do not show an exact threat location through fog or walls.

## 6. Near-field and far-field presentation

| Range band | Player feedback |
|---|---|
| 0–20 m | Boat wake, collision geometry, surface disturbance, close hull sound |
| 20–60 m | Silhouette / wake in clear or moderate fog; directional cue |
| 60–120 m | Large movement or plume only in clear conditions; sound / instrument bearing |
| Beyond 120 m | No precise target marker; environmental / acoustic clue only |

These are initial presentation bands. Fog may reduce the visual range. A critical collision hazard must receive a cue before it enters the unavoidable collision radius.

## 7. Threat detection remains separate

The Sleeper's AI uses its own 100-degree total visual cone, clear-water visual range of 70 m, acquisition dwell, line-of-sight test, and acoustic listener. The player's camera FOV does not determine whether the threat detects the boat. See [Entity Perception and Encounter Math](../07_GAME_RULES_AND_BALANCING/ENTITY-PERCEPTION-AND-ENCOUNTER-MATH.md).

The camera must never rotate toward a hidden creature merely because its threat system has acquired the boat. Threat visibility and camera framing are separate contracts.

## 8. Accessibility

Directional audio must have a visual equivalent. Threat awareness must not depend on subtle color alone. Provide screen-space direction, a text caption, and an optional simplified threat indicator. The player should be able to understand the warning without spatial headphones.

Camera motion remains intentionally restrained. Keep wave tilt and turn banking clamped and damped; a reduced-motion option should be added before the camera is treated as final accessibility-complete.

## 9. Acceptance tests

### Automated Edit Mode coverage
- Look-ahead follows horizontal travel velocity, ignores vertical velocity, remains zero when stationary, and respects its maximum distance.
- Negative tuning values cannot reverse the look-ahead vector.
- Turn banking follows yaw-rate sign and clamps extreme values.

### Unity scene / Play Mode checks still required
- Camera framing does not hide a hazard within unavoidable collision range.
- Camera smoothly leads in the actual travel direction during acceleration, reverse, and drift.
- Camera banking remains subtle during tight turns and settles when the boat stops turning.
- Camera follows the boat's buoyancy-driven vertical heave without adding an independent wave-height offset.
- Camera tilt uses the same sampled surface normal as the generated water mesh in Greyhaven and both survey sandboxes.
- Dead calm visibly settles the water and reduces camera tilt; wrong-way swell reverses direction/rate continuously without a surface or camera snap.
- Camera does not auto-lock onto or reveal a hidden source.
- When the boat leaves the bounded water mesh, the camera stops sampling wave tilt rather than reacting to an extrapolated surface.
- Confirm boat and hazard readability at the harbour, breakwater, wreck field, open-water edge, and under fog.
- Test reduced-motion needs before final accessibility sign-off.

Unity 6000.3.28f1 compilation, Edit Mode execution, Play Mode review, and a Windows build are not claimed by a repository commit or a passing static check.

# Entity Perception and Encounter Mathematics

> Project: WHERE THE SOUNDINGS END  
> Status: Prototype tuning specification  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## 1. Important distinction: player camera vs entity FOV

The game is 2.5D with an elevated camera. The player does not automatically have a first-person vision cone. The camera defines what is on screen. **FOV in this document primarily means the hostile entity's visual detection cone for the player's boat.** The player also receives a perception/visibility system that determines whether clues and threats are legible.

Do not hide an attack behind the edge of the camera. If the threat is near enough to strike, at least one readable cue (sound, wake, shadow, rig response, or HUD indicator) must be available even when it is off-screen.

## 2. Visual detection

Prototype hostile values:
- Total FOV: 100 degrees.
- Clear-condition detection range: 70 m.
- Strong fog reduces range to 25–40 m.
- Visual evaluation: 4 times per second while nearby.
- Acquisition dwell: 0.8–1.2 s above threshold.
- Loss-of-visual confidence decay: 2–4 s depending on threat.

Cone test:
[
ec{d} = rac{P_{target}-P_{eye}}{|P_{target}-P_{eye}|}
]
[
insideFov iff ec{f}cdotec{d} geq cos(	heta/2)
]

Occlusion is a separate raycast test. A target is not visible through terrain or a fully occluding hull. Use layer masks so rain, spray, UI, and cosmetic particles do not block AI vision. The current Unity Physics.Raycast API supports an explicit maximum distance, layer mask, and trigger-interaction mode; define these per query rather than relying on a project-wide default (https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/raycast).

## 3. Player visibility score

[
V = C \, L \, M \, O \, F
]

- (C = max(0, 1-d/R)): distance falloff.
- (L): contrast / light exposure (0–1.25).
- (M): movement and wake exposure (0.65–1.2).
- (O): line-of-sight factor (0 or 1 initially; later can be 0–1 for partial cover).
- (F): fog transmission (0–1).

The entity acquires the boat when (V geq 0.55) continuously for the dwell time. A fast wake or lantern can improve visibility; fog, distance, and island / rock occlusion reduce it. Values are tuning parameters, not optical measurements.

## 4. Acoustic detection

[
A_r = rac{A_s W B O}{1+(d/D)^2}
]

Sound events are banded (low engine, mid mechanical, high Rig ping, broadband collision). (A_s) is source loudness, (W) propagation, (B) creature sensitivity, (O) masking / occlusion, (d) distance, and (D) attenuation distance. An event is eligible for investigation when (A_r) exceeds the creature threshold.

Prototype bands:
- Low engine hum: persistent, easily localized at close range.
- High Rig ping: short, distinctive, strongly attracts the Sleeper.
- Collision / horn: short, loud, immediate investigation cue.
- Wave / rain: environmental masking; it reduces confidence, not all detection.

## 5. Encounter chance

Use event-boundary probability:
[
P = operatorname{clamp}(P_0 M_r M_t M_w M_q M_n M_c, 0, P_{cap})
]
Roll once per named opportunity. Do not use frame-by-frame rolls. Required quest evidence is guaranteed after a bounded number of failed opportunities.

## 6. Starter tuning table

| System | Initial value | Notes |
|---|---:|---|
| Entity total visual FOV | 100° | Total cone, not half-angle |
| Clear-water visual range | 70 m | Hostile baseline |
| Visual sensing cadence | 4 Hz | Nearby active region |
| Acquisition dwell | 1.0 s | Prevents one-frame acquisition |
| Memory confidence half-life | 3 s | Tuning per threat |
| Rig-ping threat opportunity | 0.12–0.25 | Per eligible deployment, different by event |
| Threat pursuit duration | 12 s | Reassess before continuing |
| Warning telegraph | 2.2–3.0 s | Before first impact |
| Strike damage | 12–18% hull | Starter boat |
| Attack cooldown | 18–24 s | Prevent chain hits |
| Post-attack quiet lock | ≥120 game seconds | Director-level lock |

## 7. Test scenarios

Test at clear noon, night with lantern, rain, dense fog, behind an island, engine off, engine high, one Rig ping, repeated Rig pings, and after collision. Measure false-positive detections, missed detections, time to warning, and time to disengage. Each test should use a fixed event seed.


## 8. Unity implementation notes

- Use Physics.Raycast for direct visual occlusion tests with an explicit max distance, layer mask, and QueryTriggerInteraction. Unity 6.0.6 API reference: https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/raycast
- For shore-based NPCs that move on a NavMesh, NavMeshAgent provides movement, path, steering, and raycast-related capabilities. It is not the default solution for open-water creatures. Reference: https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/ai/navmeshagent
- Re-check Unity's resource hub and release information when selecting the exact Unity 6 LTS editor version: https://unity.com/campaign/unity-6-resources

These references establish API capabilities, not the project's tuning values or a requirement to use a specific Unity release.

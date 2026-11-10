# Sounding Rig Algorithms

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Signal pipeline

`Deploy → Emit Pulse → Receive Returns → Attenuate / Mask / Occlude → Extract Features → Rank Hypotheses → Display Confidence → Emit SoundEvent → Apply Reactive Response → Record Observation`

## Basic return model

[
S_r = rac{S_0 \, O \, M}{1+(d/D)^2}
]

Where (S_0) is emitted pulse strength, (O) is line-of-propagation / occlusion, (M) is environmental masking, (d) is range, and (D) is the authored attenuation distance. Return delay is a gameplay approximation based on distance band, not a full acoustic simulation.

## Signal hypotheses

The rig can return:
- hard surface / reef;
- hollow cavity / structure;
- moving school / animal;
- current boundary;
- wreckage / debris;
- unknown repeated signal;
- reactive anomaly.

The UI shows the top hypothesis, a confidence band, and one or two alternatives. It must not label a creature by name before the story has taught that identity.

## Reactive signal states

| Stage | Trigger | Return |
|---|---|---|
| 0 — Quiet | No anomaly response | Normal return |
| 1 — Acknowledgement | First eligible pulse | Slight delayed echo |
| 2 — Repetition | Repeated pulse within response window | Second return with altered delay or bearing |
| 3 — Escalation | Third pulse / disturbance threshold | Strong distortion and external warning cue |
| Reset | Leave region or wait cooldown | Return to stage 0 or authored story state |

Initial tuning: 8 points disturbance per Rig pulse; response window 90–150 game seconds; anomaly cooldown 5–10 game minutes. Quest-specific overrides must be explicit.

## False-return design

False returns are authored, deterministic at the event boundary, and learnable. They may arise from:
- a curved reef wall;
- a hollow wreck;
- a moving animal;
- an echo delay caused by a second surface;
- an anomalous response.

Do not roll an arbitrary false-positive percentage on every sample. The same location / pulse conditions should reproduce the same class of return for a fixed save state.

## Player safety

A false signal may lead to a hazardous location, but the signal itself never damages the hull. The map, visible water, current, or repeated echo must offer a chance to detect the mismatch.

## Tests

- Range and masking lower confidence.
- Reef return is repeatable at the same point and pulse configuration.
- Reactive anomaly advances only once per pulse event.
- Leaving the zone does not erase quest flags.
- The player can distinguish a known buoy from an unknown signal after evidence is collected.

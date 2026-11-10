# Environmental Reactions

> Project: WHERE THE SOUNDINGS END  
> Status: Implementation-ready design specification  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Purpose

The sea should respond consistently to the player's actions. Reactions are driven by typed gameplay events, not by hidden random punishment.

## Input events

| Player / world event | Immediate response | Longer response |
|---|---|---|
| Engine running | Creates an acoustic signature and wake | Nearby wildlife may move away; hostile entities may investigate |
| High engine throttle | Stronger sound and wake | Increased detection chance within the current region |
| Sounding Rig pulse | Creates a directed ping / return | Some anomalies may answer or change signal |
| Repeated Rig pulses | Builds a local disturbance score | Reactive anomaly may escalate one stage |
| Horn / flare / emergency device | High-intensity signal | Can attract help or danger based on authored event rules |
| Collision | Impact audio, hull damage, debris | Nearby entities investigate the impact point |
| Fishing repeatedly in one zone | Catch success gradually falls | Fish school relocates or local stock recovers later |
| Light / lantern | Increases visual contrast around the boat | Some entities detect the illuminated silhouette more easily |
| Quiet drift | Low engine noise | Natural ambient cues become easier to hear |

## Local disturbance

Maintain a per-region disturbance scalar (D in [0,100]).

[
D_{t+Delta t} = operatorname{clamp}(D_t + sum g_i - lambda Delta t, 0, 100)
]

- (g_i): disturbance gain from engine, ping, collision, horn, or repeated extraction.
- (lambda): authored recovery rate.
- Every source event has a one-time event ID so the same collision cannot add disturbance every frame.

Disturbance influences only defined outcomes: event weights, fish avoidance, and anomaly response. It never directly damages the player.

Prototype gains (tune in playtests):
- Low engine interval: +0.2 / second.
- High throttle interval: +0.5 / second.
- Sounding Rig pulse: +8.
- Collision: +12.
- Horn: +10.
- Quiet recovery: -1.0 / in-game minute.

## Signal reaction stages

For signal-reactive anomalies:
- Stage 0: no response.
- Stage 1: delayed / distorted echo.
- Stage 2: apparent source shifts or repeats.
- Stage 3: strong return; anomaly becomes visible as a surface disturbance or changes the local event state.

Escalation requires a defined number of pulses or disturbance threshold. It does not escalate merely because the player remains in the region.

## Environmental damage rules

Only explicit hazards apply damage:
- rocks / reefs: collision impulse and hull integrity;
- severe current: lateral force and control penalty;
- wave strike: temporary heading / speed disruption;
- hostile creature: its attack component;
- structural debris: localized collision hazard.

A sound, voice, bell, or false reading never deals direct damage. It can lead the player toward a dangerous location, but the map, cues, and environment must provide a fair warning.

## Recovery and reset

- Disturbance decays when the player leaves the area or stops generating noise.
- Fish availability follows its own biomass / recovery model.
- Quest anomalies reset on authored resolution or cooldown, not by deleting story evidence.
- Threat cooldowns persist through save/load.

## Tests

- Each action emits at most one disturbance event per event ID.
- Disturbance is clamped and recovers.
- A Rig pulse changes the intended anomaly response stage only.
- Ambient audio changes are not a hidden damage trigger.
- Repeated player actions produce predictable, learnable escalation.

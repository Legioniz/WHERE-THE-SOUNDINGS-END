# Fish AI and Acoustic Ecology

> Project: WHERE THE SOUNDINGS END  
> Status: First gameplay integration implemented; autonomous schooling remains planned  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-16

## 1. Ownership

The persistent gameplay scene owns authoritative fish populations, habitat stocks, catch selection, migration, and save participation through `FishingSystemDirector`. Addressable fish-school prefabs are presentation only. Releasing a school visual must never create, remove, or reset fish stock.

Regional population records continue to advance through the existing coarse simulation. Do not instantiate one GameObject per simulated fish.

## 2. Acoustic response in active fishing grounds

The fishing director subscribes to `AcousticEventBus.SignalEmitted` while enabled. A signal contributes to local fishing-ground disturbance only when:

- a fishing ground has been selected by a cast;
- its region matches the last cast's region when both IDs are available;
- the signal falls within its authored radius plus a small reaction margin.

The response is bounded by signal loudness and proximity. A cast publishes a `FishingGearNoise` event; securing a catch publishes a stronger gear-noise event. Sounding Rig pulses and other typed acoustic events can also disturb a nearby active ground.

## 3. Gameplay effects

Disturbance is a temporary value in the fishing director, not a replacement population value.

- Higher disturbance reduces candidate selection weights, but never hard-locks a species out.
- Higher disturbance increases the bite wait, bounded to a maximum 1.8x multiplier.
- Disturbance decays exponentially using the simulation clock and a configurable half-life (default 180 game seconds).
- A different region's signal is ignored when both region IDs are known.
- Signal metadata, not AudioSource playback, drives the response.

This creates a tactical choice: repeated fishing and noisy equipment can make a nearby ground less productive, encouraging the player to move, change technique, or allow the water to settle.

## 4. Performance and safety

- Event-driven subscription; no per-frame AudioSource scan.
- One disturbance scalar for the currently tracked ground; no per-fish update loop.
- No extra persistent GameObjects or scene ownership.
- Acoustic noise is published only at meaningful gameplay events, not every rendered frame.
- Visual school pooling remains independent of fish stock and catch outcomes.

## 5. Current limitations

- Disturbance currently models the last selected fishing ground, not a full spatial grid of all habitats.
- Autonomous roaming fish schools, predator-specific responses, feeding, breeding, and detailed per-species schooling are not implemented by this pass.
- Temporary disturbance resets with a new game session; authoritative regional stock and catch records remain in the existing save participant.
- Runtime tuning requires Unity playtesting across nearshore, kelp, open-water, and deep-water grounds.

## 6. Acceptance tests

- Loudness and proximity scale disturbance without exceeding the configured bound.
- Out-of-radius signals contribute zero disturbance.
- Half-life decay is independent of frame partitioning.
- Maximum disturbance still leaves a non-zero candidate availability.
- Maximum bite-wait multiplier is bounded.
- Fish-school visual release does not mutate authoritative population records.
- Unity Play Mode confirms cast and catch signals reach the acoustic listener without duplicate event delivery.

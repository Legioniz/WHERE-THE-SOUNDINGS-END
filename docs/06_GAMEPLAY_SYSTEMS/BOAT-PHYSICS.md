# Boat Physics and Motor Response

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline with visual motor telemetry  
> Last reviewed: 2026-11-12

## Physics authority

The vessel Rigidbody remains the movement authority. `BoatPrototypeController` applies force and torque in `FixedUpdate`; `BoatHydrodynamics` samples the shared analytic water surface and applies buoyancy. The outboard model, propeller animation, particles and cowl vibration are presentation only.

The movement model currently includes:

- forward and reverse propulsion with different acceleration and speed limits;
- opposing-throttle braking before reversing;
- propulsion taper near the speed limit;
- engine-condition and hull-integrity influence on available thrust;
- lateral water resistance, quadratic forward drag, and speed-dependent rudder authority;
- Rigidbody interpolation and a single root collision proxy.

Do not set Rigidbody velocity from the motor-effects component. Do not add collision geometry to the decorative propeller or effect hierarchy.

## Motor telemetry and effect response

There is no full crankshaft, gearbox, propeller torque-curve, cavitation or fluid solver. `BoatMotorTelemetryMath` therefore derives a bounded **presentation proxy** from the current throttle and signed horizontal forward speed. It is not reported as a physically measured RPM or torque value.

- **Engine demand / prop wash:** throttle produces underwater bubbles and turbulent prop wash. A hard launch can produce a strong prop wash before the hull has built speed.
- **Propeller rotation:** the outboard rotor pivots around the shaft axis and spins from normalized motor RPM telemetry. Idle retains a low visual idle rate.
- **Surface wake / spray:** this uses total horizontal vessel speed. It continues while coasting after throttle is released, and grows with speed.
- **Engine vibration:** the visual cowl receives a very small oscillation under load. Poor engine condition increases the visible roughness; the Rigidbody and collision hull remain unaffected.
- **Movement locked for dredging:** throttle telemetry is zero while the action locks the helm, but residual vessel speed can still produce a wake.

Particle rates and maximum particle counts are capped. Particles simulate in world space so the stern leaves a trail instead of carrying old foam along with the boat. The particle texture and material are created once per boat at runtime and released when the effects component is destroyed.

## Tuning principles

1. A stationary boat at zero throttle must show no powered prop wash; the motor can remain visually idling.
2. A stationary or slow boat under high throttle must show stronger prop wash and stronger cowl vibration.
3. A fast boat coasting with zero throttle must retain a speed-driven surface wake, but not powered prop wash or high-load vibration.
4. Reverse throttle must use the reverse speed limit when estimating engine load.
5. Damaged engines may look rougher, but this presentation layer must not apply damage or reduce thrust itself.
6. Visual effects must not add colliders, rigidbodies, forces, torque, or direct velocity writes.

## Validation and scene regeneration

The repository validator checks the telemetry contract, bounded effects, visual rotor hierarchy, both generated scene builders, and regression tests. Edit Mode tests cover the pure telemetry math and propeller visual hierarchy.

After pulling the changes, regenerate scenes in Unity when ready:

- `Tools/WHERE THE SOUNDINGS END/Rebuild Greyhaven Environment`
- `Tools/WHERE THE SOUNDINGS END/Rebuild Deep Ocean Survey Sandboxes`

Back up any manual scene edits first; these commands replace their generated scenes. The effects create their particle objects at runtime when the scene enters Play Mode.

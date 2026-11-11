# Boat Movement and Hydrodynamics

> Project: WHERE THE SOUNDINGS END  
> Status: First Rigidbody hydrodynamics implementation  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

## Design goal

The starter vessel should feel like a small, heavy working boat rather than a character controller with a boat model attached. It accelerates gradually, coasts after throttle release, needs water flow across the rudder to turn, slides sideways only briefly, and responds to the shape and motion of the sea.

The implementation is deliberately a small, stable approximation suitable for the project's 2.5D camera. It is not a computational fluid-dynamics simulation, a naval-architecture-grade hull model, or a realistic wave solver.

## Runtime ownership

| Component | Responsibility |
|---|---|
| `BoatPrototypeController` | Reads steering input and applies propulsion, reverse/braking, rudder torque, lateral resistance and forward drag. |
| `BoatHydrodynamics` | Samples four hull points, applies buoyancy forces, and damps roll/pitch angular motion. |
| `MaritimeSurfaceBehaviour` | Owns the analytic wave field and renders a mesh deformed by the same function used by buoyancy sampling. |
| `VesselConditionController` | Supplies engine/hull condition to reduce acceleration and effective speed. |
| `SeaStateDirector` | Reads actual Rigidbody velocity for movement-driven world time. It does not move the boat. |
| `CameraFollow2_5D` | Follows the vessel transform; camera smoothing is presentation only and never feeds back into physics. |

## Control behaviour

- **Forward throttle:** applies force along the boat's forward axis. Acceleration tapers as the vessel approaches its forward speed limit.
- **Reverse throttle:** uses lower reverse acceleration and a lower reverse speed limit.
- **Opposing throttle:** brakes existing forward/reverse motion first; it only builds motion in the opposite direction after speed falls close to zero.
- **Steering:** applies yaw torque rather than rotating the transform directly. Rudder authority grows with forward speed.
- **Prop wash:** a small steering response remains available at low speed when throttle is applied. No throttle and no water flow means no meaningful rudder authority.
- **Reverse steering:** rudder response changes sign while moving astern.
- **Coasting:** releasing throttle removes propulsion but does not instantly stop the hull.
- **Lateral resistance:** sideways velocity is damped more strongly than forward movement.
- **Forward drag:** quadratic drag increases with speed and limits coasting distance.
- **Damage:** engine condition reduces thrust and effective speed; hull condition also reduces propulsion and speed capability.

## Forces and approximations

Let (v_f) be forward velocity, (v_l) lateral velocity, (u) normalized throttle, and (s) normalized steering.

Propulsion is a bounded acceleration along the hull's forward axis:

[
a_p = u a_0 h_e h_h q(v_f)
]

where (h_e) and (h_h) are engine/hull condition factors and (q) tapers acceleration toward the direction-specific speed limit. When throttle opposes current forward motion, a separate braking acceleration is applied before reverse movement begins.

The main passive water forces are:

[
a_l = -c_l v_l
]

[
a_d = -c_d v_f |v_f|
]

where (c_l) is lateral resistance and (c_d) is quadratic forward drag.

Rudder authority is based on the greater of forward water flow and a small throttle-derived prop-wash term. It is capped at one and multiplies yaw acceleration. A separate yaw-velocity damping term prevents endless rotation.

### Four-point buoyancy

The hull has four sample points: port/starboard at the bow and port/starboard at the stern. Each point queries the water surface for a world-space point, normal, and vertical surface velocity.

For point (i), the depth below the water surface is (d_i), and normal velocity relative to the moving water is (v_i):

[
F_i = operatorname{clamp}(k d_i - c v_i,;0,;F_{max})
]

[
F_{buoyancy} = sum_i F_i hat{n}_i
]

The spring constant is distributed so the four points support the Rigidbody mass at the target draft:

[
k = rac{m g}{N d_{target}}
]

The damping coefficient uses a critically damped spring estimate for the per-point supported mass:

[
c = 2 sqrt{k rac{m}{N}}
]

The forces are applied at the hull sample positions, so wave height differences create heave, pitch and roll naturally. Roll/pitch angular damping keeps the small workboat stable in the prototype's modest sea state.

### Shared visual/physics water surface

`MaritimeSurfaceBehaviour` owns one analytic wave function for both the rendered surface and physics queries. Its simulation state advances in `FixedUpdate` using `Time.fixedDeltaTime`. The component has execution order `-100`, so it advances and snapshots the water state before the default-order `BoatHydrodynamics.FixedUpdate` samples the four hull points.

Each physics step preserves the previous and current phase, direction and amplitude. `TrySampleSurface` evaluates the current physics snapshot, then estimates vertical surface velocity from the two snapshots over that exact fixed-step duration. This keeps the water velocity supplied to buoyancy independent of render frame rate and includes phase progression, direction blending and amplitude ramps during dead-calm / wrong-way-swell incidents.

The visible water mesh is deformed in `LateUpdate`. It interpolates between the previous and current fixed snapshots using the render interpolation fraction, matching the generated boats' Rigidbody interpolation without feeding presentation state back into buoyancy. The first mesh is generated from the same initial wave state before the first frame is rendered.

Wrong-way swell does not reset phase from absolute application time. Its travel direction blends toward the incident direction and its phase rate eases through reversal. This prevents the water pattern from jumping when an incident begins or ends. The solver still models only vertical surface velocity; horizontal orbital water velocity and currents are not implemented.

The water surface currently covers a bounded plane. Outside its local sample bounds, the buoyancy component applies no water support. Future streamed regions must provide their own water-surface component or a shared world water service.

### Hull collision and impact damage

The boat uses one root-level BoxCollider as its simplified waterline collision hull. Greyhaven's hull and raised-bow meshes are visual-only; their primitive colliders are removed so overlapping child boxes cannot create duplicate contacts or seams. The deep-water survey boats also use one root hull proxy. Static island terrain, breakwater rocks, pier decks, piles and wreck pieces retain their authored colliders. The ocean mesh itself remains non-colliding.

Unity's Rigidbody solver owns physical separation and momentum. `VesselConditionController` does not multiply or overwrite post-impact Rigidbody velocity. It aggregates the strongest contact-normal impact reported during a physics step and processes one damage event on the next fixed step.

Damage uses only the component of relative velocity along the contact normal. Tangential scraping therefore does not count as a full-speed collision. The initial gameplay rule is:

- Damage threshold: 4.2 m/s normal impact speed.
- Hull damage: `clamp((normalImpactSpeed - threshold) × 2.2, 2, 28)` integrity points per accepted impact.
- Engine damage: 18% of the hull damage amount.
- Cargo: the contacted local hull position maps to the corresponding 7×9 hold cell. An occupied non-protected item can be destroyed; quest-critical cargo remains protected by the existing hold rule.
- Invalid vectors, zero contact normals, NaN and infinite values produce no impact damage.
- Every accepted damaging strike publishes one `HullImpact` acoustic event with region identity, contact position, scaled loudness/radius and the current action-driven game timestamp. This feeds the existing acoustic listener; it does not add another audio/perception system.

The values are prototype tuning, not measured ship-damage physics. They must be validated and adjusted in Play Mode.

## Initial prototype tuning

| Parameter | Initial value |
|---|---:|
| Rigidbody mass | 1,200 kg (gameplay scale) |
| Target draft at buoyancy probes | 0.18 m |
| Maximum probe submersion used by the spring solver | 0.65 m |
| Buoyancy points | 4 |
| Forward acceleration | 2.8 m/s² |
| Reverse acceleration | 0.95 m/s² |
| Opposing-throttle braking | 2.6 m/s² |
| Forward speed limit | 8.5 m/s |
| Reverse speed limit | 3.2 m/s |
| Full rudder flow speed | 4.5 m/s |
| Lateral resistance coefficient | 2.4 |
| Quadratic forward drag coefficient | 0.018 |

These are gameplay tuning values, not measured properties of a real vessel. Validate handling in Play Mode and tune from recorded stop-distance, acceleration, turning-circle and wave-response observations.

## Required tests and acceptance

Edit Mode tests cover:
- buoyancy is zero above the water surface;
- deeper submersion produces greater upward force;
- probe depth is clamped to the configured maximum submersion;
- damping reduces upward force while the hull rises and increases it while sinking;
- buoyancy force respects the per-point cap;
- forward thrust is stronger than reverse;
- opposing throttle brakes before reverse acceleration;
- speed taper approaches zero propulsion at the speed limit;
- damaged engine/hull reduce propulsion;
- rudder authority increases with flow and prop wash creates a low-speed response;
- wave phase remains continuous through reversal, direction transitions blend gradually, and vertical surface velocity includes amplitude changes.

Play Mode tests are still required in Unity:
- vessel settles to a stable draft without sinking or launching out of the water;
- wave crests and troughs cause small, stable heave/pitch/roll;
- releasing throttle produces a believable coast and stopping distance;
- steering response increases with speed and changes direction in reverse;
- the vessel does not snag on the visible water mesh;
- the same buoyancy response is observed with render caps at 30, 60 and 120 FPS while the fixed timestep remains unchanged;
- the rendered wave mesh interpolates smoothly without changing the physics sampling cadence;
- the sea-time clock follows the actual vessel Rigidbody velocity;
- hard bow and side impacts separate the vessel cleanly from piers, rocks, terrain and wrecks;
- fast tangential scraping does not cause the same damage as an equally fast head-on impact;
- multiple contacts in one physics step produce one strongest-impact damage event;
- quest-critical cargo is never destroyed by impact damage, while ordinary occupied cargo can be lost;
- an accepted hard impact publishes one acoustic HullImpact event and produces a readable damage report;
- the save/load system restores position and linear/angular velocity without an impulse spike.

## Known limits and future work

- No full hull-volume sampling, buoyancy mesh, fluid displacement, wave-current simulation, wind force, or propeller cavitation.
- Surface velocity models vertical wave motion, including incident amplitude/direction ramps; horizontal orbital water velocity and currents are not implemented.
- Water-direction and phase transitions are smoothed, but the surface is still an authored analytic swell approximation rather than a physically solved ocean.
- No wake or bow-wave visual response yet.
- There is no separate collision hull/waterline model; the visible water has no collider and provides force samples only.
- The 2.5D camera remains fixed-angle orthographic. Vessel pitch/roll is physically present but intentionally damped for readability.
- Future improvements should be driven by Play Mode observations, not by adding a heavy simulation package prematurely.

# Testing Strategy

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline with initial deep-ocean survey Edit Mode coverage  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Test layers

### Edit Mode / unit
- Pure time, depth, confidence, deterministic-key and probability calculations.
- Regional profile configuration and habitat eligibility.
- Inventory geometry, blocked cells, item placement and damage.
- Save DTO validation and schema migration.
- Stable opportunity identity and idempotent event evaluation.
- Acoustic signal construction, range/loudness/sensitivity/noise scoring, event deduplication and listener persistence.
- NPC schedule resolution, overnight schedule boundaries, and NPC relationship save/restore.

### Play Mode
- Boat handling, speed gating and Sounding Rig input.
- Hull contact against piers, breakwater rocks, terrain and wrecks; root-hull-only collision ownership; no visual collider seams; normal-impact damage; per-physics-step contact aggregation; collision acoustic event propagation.
- Rigidbody buoyancy, wave following, heave/pitch/roll stability, forward/reverse thrust, opposing-throttle braking, speed taper, rudder flow, and lateral water resistance.
- Water-wave simulation advances on the fixed physics timestep before buoyancy; rendered wave interpolation remains smooth and does not alter physics sampling at 30, 60 and 120 FPS.
- Survey action advances the sea clock for its configured duration.
- Pausing or opening dialogue / inventory still freezes sea time.
- Greyhaven profile returns shallow bathymetry and never reports a deep scattering layer.
- Missing regional profile produces a clear coverage message.
- A site marker can be discovered once and remains discovered after save/load.
- A survey pulse publishes one AcousticSignal.
- The survey layer never creates a collider or modifies seabed collision.

### Integration
- Complete trip, fishing, salvage, survey, recovery and save/load.
- Load schema 0, 1, 2 and 3 saves through schema 5 without inventing survey progress; migrate schema 4 weather labels into safe timer defaults.
- Survey multiple regions and verify opportunity IDs do not collide.
- Confirm repeated pings from the same heading do not promote an uncertain return.
- Confirm a second ping from a different heading in the same region and within the time window can promote the interpretation.
- Confirm cross-region pings cannot confirm one another.
- Confirm disabled / unloaded SoundingRigController releases its action-driven clock flag.
- Confirm keyboard and gamepad HarvestBeat bindings both register timed minigame beats.
- Confirm weather/fog durations, shift cooldowns, hallucination cooldowns and pending seabird warning state survive save/load.
- Confirm maritime incident caption, hush and false-navigation-light state restore without rerolling the event.
- Confirm all Greyhaven NPC IDs are unique, each interaction starts the correct Yarn node, schedule changes use SeaStateDirector.CurrentHour, and NPC movement pauses with the action-driven clock.
- Confirm NPC relationship scores and Yarn first-meeting/trust variables restore independently after a save/load cycle.
- Confirm Outer Shoals and Hollow Trench sandbox scenes contain valid regional profiles and authored site markers.

### Performance
- Profile water mesh, buoyancy sampling, Rigidbody forces, fog, local wildlife, acoustic listeners and physics at representative and worst-case loads.
- Verify acoustic listeners are event-driven / throttled and do not poll all AudioSources.
- Verify survey site lookup remains acceptable as site counts grow; replace linear scans with spatial indexing when needed.

### Accessibility and playtest
- Captions and text equivalent for critical audio / bearing cues.
- Mono playback, adjustable dynamic range, reduced motion and reduced flash.
- Comprehension of sonar confidence and the difference between biological mid-water returns and the true seabed.
- Navigation, fear pacing, workload, recovery, economy and time-to-understand.

## Current automated coverage

The Edit Mode test source includes eight suites, including deterministic maritime-surface transition coverage:
- action-driven clock policy;
- cargo damage / repair / quest-cargo protection;
- save migration to schema v4;
- deep scattering layer day/night depth ordering;
- mid-water return separation from seabed;
- stable survey opportunity hashing;
- Greyhaven shallow profile versus Outer Shoals deep profile.
- Same-region, changed-heading and fresh-time-window requirements for independent scattering-layer observations.
- Separate confidence calculation for seabed and sparse biological-layer returns.
- Site signal confidence from authored signal strength and acoustic attenuation.
- NPC day/overnight schedule boundaries and missing-window behaviour.
- NPC stable-ID relationship persistence.
- Contact-point relative velocity including angular motion; contact-normal impact speed, glancing-impact rejection, threshold/capped hull damage, disabled damage-rate tuning, invalid collision inputs and proportional engine damage.\n- Greyhaven shoreline determinism/envelope, bounded multi-scale heightfield, distinct headland and inland-spine elevations, beach/grass/basalt classification and invalid terrain-sample fallback.
- Four-point buoyancy force and damping response, forward/reverse acceleration, braking behaviour, engine/hull condition effects, and flow-dependent rudder authority.
- Continuous wave phase across reversal, gradual direction blending, vertical surface velocity including amplitude transitions, and maximum probe-submersion clamping.
- Fixed-step surface-velocity estimation and normalized interpolation between previous/current wave snapshots.

The repository static validator checks required project files, pinned Unity / package versions, input action presence, save schema v5, Sounding Rig persistence markers, regional profile IDs and assembly-definition references. A Tree-sitter C# parser checks syntax across all project C# files.

## Verification boundary

The static validator checks repository structure and configuration; Tree-sitter checks C# syntax. Neither resolves C# symbols or compiles against Unity/package assemblies. These checks do not import the Unity project, run Edit Mode or Play Mode tests, or create a player build.

A feature is not considered fully verified until Unity 6000.3.28f1 imports the project without compile errors, the relevant tests pass, and the generated Greyhaven scene has been exercised in Play Mode.

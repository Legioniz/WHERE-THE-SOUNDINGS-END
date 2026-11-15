# Testing Strategy

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline with Edit Mode regression suites and authored Play Mode coverage; Unity execution pending  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## Test layers

### Edit Mode / unit
- Pure time, depth, confidence, deterministic-key and probability calculations.
- Regional profile configuration and habitat eligibility.
- Inventory geometry, blocked cells, item placement and damage.
- Save DTO validation and schema migration, including schema 6-to-7 regional visit migration and schema 7-to-8 economy wallet conversion.
- Economy sale-line rounding, modifier clamps, stock-driven demand bounds, repair/fuel quote rounding, wallet overflow/overspend rejection, transaction idempotency, wallet save/restore and legacy currency migration.
- RegionalChartPolicy unknown-region suppression, idempotent first-visit unlocks, and discovered-site-only marks.
- Stable opportunity identity and idempotent event evaluation.
- Acoustic signal construction, range/loudness/sensitivity/noise scoring, event deduplication and listener persistence.
- NPC schedule resolution, overnight schedule boundaries, and NPC relationship save/restore.
- Dockside access and modal policy: stationary-speed threshold, horizontal approach radius, invalid numeric input, vertical-separation independence, and exclusive ownership across dockside, inventory, dialogue and front-end pause overlays.

### Play Mode
- Bootstrap a fresh checkout in Unity 6000.3.28f1, load the generated Greyhaven persistent scene, and verify the player-controlled vessel, Rigidbody/hydrodynamics, Input System asset, harvest and Sounding Rig components, cargo hold, camera target, world clock, save coordinator, autosave controller and area streamer are present.
- Load the generated Greyhaven persistent scene and verify the three startup areas become resident without replacing the gameplay scene.
- Move the vessel into the lighthouse preload radius, confirm the lighthouse area activates, return to the harbour, and confirm it unloads after the configured grace period while the persistent scene remains loaded.
- Repeat area transitions and check that queued requests are cancelled when out of range, re-entry during unload is safely re-queued, and no area is left in a failed/loading/unloading state.
- Confirm no visible one-frame gap, missing collision surface, duplicate static geometry, or player freeze occurs during repeated transitions.
- Boat handling, speed gating and Sounding Rig input.
- Regional chart: M/button open, Esc close, no overlap with inventory/dialogue/dockside/pause, only visited region names, vessel plot, discovered-site-only marks, no unknown bearings/compass/route markers.
- Dockside hub: access only inside a shore-approach radius at or below 0.85 m/s, no duplicate NPC trigger prompt or shared-input ownership, menu pause/restore, Escape and gamepad Start close, no overlapping inventory/dialogue/pause panels, correct resident Yarn handoff, cargo appraisal read-only behaviour, vessel diagnostics and guest-ledger autosave checkpoint.
- Verify all seven location menus and furniture actions at supported resolutions; confirm no menu opens while fishing, dredging, sounding, dialogue or inventory is active.
- Hull contact against piers, breakwater rocks, terrain and wrecks; root-hull-only collision ownership; no visual collider seams; normal-impact damage; per-physics-step contact aggregation; collision acoustic event propagation.
- Rigidbody buoyancy, wave following, heave/pitch/roll stability, forward/reverse thrust, opposing-throttle braking, speed taper, rudder flow, and lateral water resistance.
- Water-wave simulation advances on the fixed physics timestep before buoyancy; rendered wave interpolation remains smooth and does not alter physics sampling at 30, 60 and 120 FPS.
- Survey action advances the sea clock for its configured duration.
- Interact selects the nearest valid phenomenon site or historical artifact within the existing safe inspection range; empty interactions do not steal an NPC/dockside interaction claim.
- Artifact metadata anchors remain available when static estuary/lighthouse area scenes unload and reload; their physical render props stream with the correct area.
- Pausing or opening dialogue / inventory still freezes sea time.
- Greyhaven profile returns shallow bathymetry and never reports a deep scattering layer.
- Missing regional profile produces a clear coverage message.
- A site marker can be discovered once and remains discovered after save/load.
- A historical artifact can be inspected once, reports previously examined on repeat inspection, and persists independently from inspected phenomenon-site IDs after save/load.
- Archive inspection does not add a chart mark, visited region, survey discovery, quest flag, relationship event or sea disturbance.
- A survey pulse publishes one AcousticSignal.
- The survey layer never creates a collider or modifies seabed collision.

### Integration
- Complete trip, fishing, salvage, survey, recovery and save/load.
- Load supported schema 0–8 fixtures through current schema 9 without inventing survey progress or archive reads; migrate schema 4 weather labels, schema 5-to-6 NPC relationship memory, schema 6-to-7 empty regional visits, schema 7-to-8 whole-dollar currency, and schema 8-to-9 empty historical-artifact inspection ledger.
- Survey multiple regions and verify opportunity IDs do not collide.
- Confirm repeated pings from the same heading do not promote an uncertain return.
- Confirm a second ping from a different heading in the same region and within the time window can promote the interpretation.
- Confirm cross-region pings cannot confirm one another.
- Confirm disabled / unloaded SoundingRigController releases its action-driven clock flag.
- Confirm keyboard and gamepad HarvestBeat bindings both register timed minigame beats.
- Confirm weather/fog durations, shift cooldowns, hallucination cooldowns and pending seabird warning state survive save/load.
- Confirm maritime incident caption, hush and false-navigation-light state restore without rerolling the event.
- Confirm all Greyhaven NPC IDs are unique, each interaction starts the correct Yarn node, schedule changes use SeaStateDirector.CurrentHour, and NPC movement pauses with the action-driven clock.
- Rebuild the Greyhaven village and confirm service signs, gable roof/gable meshes, windows, porch/awning details, repair hoist, net-mender lean-to, inn veranda, and resident furniture clusters are generated.
- Confirm the physical dock chart table is entirely on the east half of the T-pier, the central mooring lane remains clear, the map silhouette matches the generated terrain profile, and map markers match the village, dock, lighthouse and wreck coordinates.
- Confirm the chart legend maps Harbourmaster to the office, Mara to the market, Tomas to repair and Inez to the net shed; signs and TextMesh fonts render at the gameplay camera scale.
- Confirm Harbourmaster's log desk and office records, Mara's sorting/scale/collection props, Tomas's repair bench/radio/engine props, Inez's net loom/knot/spool props and the Inn's shared seating are visible from the gameplay camera.
- Confirm resident furniture is static and collider-free, is assigned to the intended additive area scene, and does not obstruct NPC schedule destinations, the quay walking lane, market service face, repair bay, net-shed approach, road access or boat mooring.
- Stream the harbour and village areas in/out repeatedly and confirm furniture appears/disappears with its owning static area without duplicate props or persistent-scene references.
- Run the Editor generated-area preflight against six valid revision-5 scenes, then against stale revision, wrong stable ID, missing root and missing scene asset cases. Only the valid set should pass. Confirm scenes opened only for validation are closed afterward, previously loaded scenes remain loaded, and the previously active scene is restored.
- Confirm NPC relationship scores and Yarn first-meeting/trust variables restore independently after a save/load cycle.
- Confirm Outer Shoals and Hollow Trench sandbox scenes contain valid regional profiles and authored site markers.
- Enter and leave each DeepOceanSurveyRegion; verify the first visit unlocks exactly once, persists through save/load, and never exposes other region names.
- Discover an authored phenomenon site with the Sounding Rig; verify its mark appears only in its visited region and no unknown site receives a placeholder marker.
- Verify chart local plotting bounds and marker scaling; no compass, GPS, route or world-scale coordinate is implied.

### Performance
- Capture the `[AreaStreaming]` load/unload telemetry in a Windows Development Build. Record per-area load duration, referenced mesh/material/texture counts and estimated asset bytes, global allocated/reserved memory, managed memory and GC memory.
- Profile cold and warm routes at least five times; report median and worst-case load duration and frame-time spikes on a named hardware configuration. Per-area asset estimates are reference-based and are not GPU VRAM measurements.
- Profile water mesh, buoyancy sampling, Rigidbody forces, fog, local wildlife, acoustic listeners and physics at representative and worst-case loads.
- Verify acoustic listeners are event-driven / throttled and do not poll all AudioSources.
- Verify survey site lookup remains acceptable as site counts grow; replace linear scans with spatial indexing when needed.

### Accessibility and playtest
- Captions and text equivalent for critical audio / bearing cues.
- Mono playback, adjustable dynamic range, reduced motion and reduced flash.
- Comprehension of sonar confidence and the difference between biological mid-water returns and the true seabed.
- Navigation, fear pacing, workload, recovery, economy and time-to-understand.

## Current automated coverage

The Edit Mode assembly contains deterministic suites for the following policies and regressions, including dockside access/modal exclusivity, regional chart discovery/save migration and maritime-surface transitions:
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
- NPC stable-ID relationship persistence and schema 5-to-6 memory-ledger migration.
- Dockside menu, inventory and pause-overlay mutual exclusion, non-finite/paused-time rejection, and restoration of the prior time scale if the hub component is disabled while open.
- Contact-point relative velocity including angular motion; contact-normal impact speed, glancing-impact rejection, threshold/capped hull damage, disabled damage-rate tuning, invalid collision inputs and proportional engine damage.
- Greyhaven shoreline determinism/envelope, bounded multi-scale heightfield, distinct headland and inland-spine elevations, beach/grass/basalt classification and invalid terrain-sample fallback.
- Four-point buoyancy force and damping response, forward/reverse acceleration, braking behaviour, engine/hull condition effects, and flow-dependent rudder authority.
- Continuous wave phase across reversal, gradual direction blending, vertical surface velocity including amplitude transitions, and maximum probe-submersion clamping.
- Fixed-step surface-velocity estimation and normalized interpolation between previous/current wave snapshots.
- Exact generated-area identity/revision matching; stale revisions, wrong area IDs and empty identity/version values are rejected.

The repository static validator checks required project files, pinned Unity / package versions, input action presence, save schema v9 and ordered v4-to-v5/v5-to-v6/v6-to-v7/v7-to-v8/v8-to-v9 migrations, dockside modal wiring, Sounding Rig and historical-artifact persistence markers, generated-area revision/preflight wiring, regional profile IDs and assembly-definition references. A Tree-sitter C# parser checks syntax across all project C# files.

## Verification boundary

The static validator checks repository structure and configuration; Tree-sitter checks C# syntax. Neither resolves C# symbols or compiles against Unity/package assemblies. These checks do not import the Unity project, run Edit Mode or Play Mode tests, or create a player build.

The streaming transition Play Mode test and dockside access/modal Edit Mode tests are authored but have not been run in Unity. A feature is not considered fully verified until Unity 6000.3.28f1 imports the project without compile errors, the relevant tests pass, and the generated Greyhaven scene has been exercised in Play Mode.

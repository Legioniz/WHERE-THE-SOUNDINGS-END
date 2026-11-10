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
- Acoustic signal construction and listener scoring when listener adapters exist.

### Play Mode
- Boat handling, speed gating and Sounding Rig input.
- Survey action advances the sea clock for its configured duration.
- Pausing or opening dialogue / inventory still freezes sea time.
- Greyhaven profile returns shallow bathymetry and never reports a deep scattering layer.
- Missing regional profile produces a clear coverage message.
- A site marker can be discovered once and remains discovered after save/load.
- A survey pulse publishes one AcousticSignal.
- The survey layer never creates a collider or modifies seabed collision.

### Integration
- Complete trip, fishing, salvage, survey, recovery and save/load.
- Load schema 0, 1, 2 and 3 saves into schema 4 without inventing survey progress.
- Survey multiple regions and verify opportunity IDs do not collide.
- Confirm repeated pings from the same heading do not promote an uncertain return.
- Confirm a second ping from a different heading in the same region and within the time window can promote the interpretation.
- Confirm cross-region pings cannot confirm one another.
- Confirm disabled / unloaded SoundingRigController releases its action-driven clock flag.

### Performance
- Profile water mesh, fog, local wildlife, acoustic listeners and physics at representative and worst-case loads.
- Verify acoustic listeners are event-driven / throttled and do not poll all AudioSources.
- Verify survey site lookup remains acceptable as site counts grow; replace linear scans with spatial indexing when needed.

### Accessibility and playtest
- Captions and text equivalent for critical audio / bearing cues.
- Mono playback, adjustable dynamic range, reduced motion and reduced flash.
- Comprehension of sonar confidence and the difference between biological mid-water returns and the true seabed.
- Navigation, fear pacing, workload, recovery, economy and time-to-understand.

## Current automated coverage

The Edit Mode test source includes:
- action-driven clock policy;
- cargo damage / repair / quest-cargo protection;
- save migration to schema v4;
- deep scattering layer day/night depth ordering;
- mid-water return separation from seabed;
- stable survey opportunity hashing;
- Greyhaven shallow profile versus Outer Shoals deep profile.\n- Same-region, changed-heading and fresh-time-window requirements for independent scattering-layer observations.\n- Separate confidence calculation for seabed and sparse biological-layer returns.

The repository static validator checks required project files, pinned Unity / package versions, input action presence, save schema v4, Sounding Rig persistence markers and regional profile IDs.

## Verification boundary

The static validator checks repository structure and configuration only. It does not compile C#, import the Unity project, run Edit Mode or Play Mode tests, or create a player build.

A feature is not considered fully verified until Unity 6000.3.28f1 imports the project without compile errors, the relevant tests pass, and the generated Greyhaven scene has been exercised in Play Mode.

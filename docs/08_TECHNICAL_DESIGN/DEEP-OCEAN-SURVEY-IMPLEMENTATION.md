# Deep-Ocean Survey Implementation — Technical Analysis

> Project: WHERE THE SOUNDINGS END  
> Status: Sounding Rig and three survey scenes generated; physical site simulation remains in progress  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## 1. Scope and implementation boundary

This document is the engineering map for the first deep-ocean survey implementation. It explains the responsibilities and coupling of each file, the runtime data flow, persistence contract, test coverage, assumptions, and remaining work.

The current build implements a Sounding Rig survey loop and a region-profile abstraction. It does **not** implement a full sonar propagation model, a complete underwater ecology simulation, all regional levels, or the physical hazards of every geological site in the research catalogue.

The project remains Unity 6 LTS / C# / URP. The Greyhaven environment generator is still the scene authority for the current playable prototype.

## 2. Runtime flow

1. Greyhaven's environment builder attaches a shallow regional survey profile to the sheltered bay water object. DeepOceanSurveySandboxBuilder also generates Outer Shoals and Hollow Trench test scenes with their own profiles and authored phenomenon sites.
2. Each generated survey boat receives SoundingRigController, DeepOceanSiteInteractionController and the shared PlayerControls asset.
3. The player presses Q or gamepad right shoulder while the boat is moving slowly enough to survey.
4. The controller resolves the nearest regional profile containing the boat.
5. A stable opportunity ID is built from region, day and saved pulse sequence.
6. PCG32 seeded from the stable ID generates bounded seabed variation.
7. The controller computes a seabed confidence separately from biological-layer confidence, then computes optional scattering-layer depth from the region's photoperiod profile.
8. It reports the seabed and mid-water returns separately. The first layer detection is explicitly uncertain; a second independent heading in the same region and within ten in-game minutes supports confirmation. Leaving a DSL habitat or receiving no distinct layer return resets that confirmation chain.
9. Nearby authored phenomenon-site markers may be discovered and add a short evidence note.
10. The controller publishes an AcousticSignal. AcousticWorldListener evaluates hearing score, deduplicates the event, adds the small sea-disturbance response and exposes SignalHeard to future creature/navigation adapters.
11. DeepOceanSiteInteractionController allows close-range field-note inspection and saves inspected site IDs.
12. Survey, weather, horror cooldowns, acoustic history and temporary maritime atmosphere state were introduced in SaveGameData schema v5 and remain preserved in current schema v6.

A pulse takes approximately 1.25 real seconds and temporarily counts as an action for the action-driven sea clock. UI pause still prevents the world clock from advancing.

## 3. File-by-file responsibility and analysis

### 3.1 DeepOceanSurveyMath.cs

**Role:** Pure domain calculations. This file should remain free of scene references, input, audio playback and save-file access.

Functions:
- GetNightWeight: smooth 24-hour day/night weighting.
- GetScatteringLayerDepth: interpolates authored day and night depths.
- IsDistinctMidwaterReturn: checks whether the biological return is sufficiently separated from the seabed.
- CalculateReturnConfidence: combines signal quality, biological-layer density and acoustic attenuation.\n- CalculateSiteSignalConfidence: combines authored site signal strength, signal quality and attenuation.
- StableKey: FNV-1a hash for repeatable opportunity seeding.
- NormalizeHour: wraps hour values safely.

**Why separate it:** Layer migration and signal classification can be tested without loading a Unity scene. A change to UI or AudioSource behaviour cannot alter the math.

**Known limitation:** The night weighting is a generic sinusoidal photoperiod. It is not a real astronomical sunrise/sunset calculation and does not yet account for latitude, season, cloud cover or water-column light attenuation.

### 3.2 DeepOceanSurveyRegion.cs

**Role:** Scene-authored survey profile for a region.

Owns:
- Stable region ID.
- Horizontal survey radius.
- Nominal seabed depth and bounded variation.
- Scattering-layer presence, density and day/night depths.
- Acoustic attenuation and background noise.

The baseline configurations cover Greyhaven, The Shallows, Blackrock Reach, The Drowned Parish, The Outer Shoals and The Hollow Trench.

**Why it is a component:** The active scene can author a profile at its world/water root without hard-coding coordinates into the survey controller. A region builder can configure the component when it creates that region.

**Known limitation:** Current profiles are simple circular horizontal bounds with one nominal depth band. Real bathymetry must eventually use chart cells, depth samples or a terrain/bathymetry data asset. The numbers are gameplay placeholders, not navigation data.

### 3.3 DeepOceanPhenomenonSite.cs

**Role:** Stable-ID discovery marker for a specific surveyable site.

Stores site ID, region ID, type, display name, depth, detection radius, signal strength, evidence label and first survey note.

Supported types:
- Biological layer
- Brine pool
- Hydrothermal vent
- Cold seep
- Gas hydrate
- Whale fall
- Bioluminescence
- Water-mass front
- Derelict vessel
- Damaged fishing gear
- Unclassified acoustic return

**Important separation:** A marker represents survey evidence. It is not the hazard itself. A brine pool marker does not poison the boat; a vent marker does not heat the water; a whale-fall marker does not simulate ecological succession. Each requires a dedicated site controller before it can affect physics, wildlife, equipment or player health.

### 3.4 DeepOceanPhenomenonRules.cs

**Role:** Central regional and depth eligibility policy for authored phenomenon markers.

The rules prevent obviously misplaced content from becoming discoverable. Examples: brine pools are restricted to the Hollow Trench; whale falls, hydrates and cold seeps require deep-region profiles; water-mass fronts can appear on the shelf; wrecks and damaged fishing gear can occur in coastal and offshore regions.

**Why separate it:** Scene builders and Edit Mode tests can share one source of truth. The rule is intentionally a setting constraint and can be revised when the island-arc geology is finalized.

**Known limitation:** The allowed depth bands are authored game constraints, not an exhaustive scientific occurrence database. They do not replace a geologist-reviewed placement map.

### 3.5 SoundingRigController.cs

**Role:** Player-facing orchestration.

Owns:
- SoundingPulse input action.
- Vessel speed gate and real-time input cooldown.
- Survey action duration.
- Regional profile selection.
- Stable opportunity sequencing.
- Deterministic seabed variation.
- Mid-water echo interpretation.
- Site discovery presentation.
- Procedural placeholder ping.
- Acoustic event publication.
- Save participation.

**Why the controller does not own all calculations:** The controller orchestrates the interaction. The math, region profile, site identity and acoustic event contract remain separate, so later systems can reuse them.

**Known limitations:**
- UI is currently OnGUI prototype presentation.
- Ping sound is synthesized placeholder audio.
- The survey does not model beam angle, transducer frequency, acoustic ray paths, multipath, target aspect, bottom composition or thermoclines.
- The repeated-heading rule is a gameplay verification mechanic, not a claim about real sonar calibration.
- The current site scan selects the nearest discoverable marker; it does not yet render multiple contacts or a chart overlay. Site markers are checked against DeepOceanPhenomenonRules, and the return reports marker depth plus signal confidence. Greyhaven's generated outer-channel wreck field carries the first valid marker (greyhaven.outer_channel_wreck). Greyhaven's generated outer-channel wreck field now carries the first marker (greyhaven.outer_channel_wreck), so the current playable scene has one real survey-discovery target without inventing deep-water geology in the harbor.

### 3.6 AcousticEventBus.cs

**Role:** Typed gameplay signal metadata.

An AcousticSignal carries event ID, kind, region, position, loudness, nominal radius and game timestamp. Sounding Rig pulses publish through this bus.

**Architectural rule:** AI must subscribe to acoustic gameplay events rather than scan every AudioSource every frame. The bus reports a sound event; each listener decides whether it can hear the event using range, occlusion, environment and its own sensory profile.

**Current limitation:** AcousticWorldListener is now a connected world-level receiver. It applies hearing-score and disturbance response and exposes SignalHeard for species-specific adapters. Creature, radio and acoustic-navigation AI are still not connected. The radius is an authored gameplay reach, not a physical sound-pressure contour.

### 3.7 SeaStateDirector.cs

**Role:** Owns action-driven world time and environmental pressure.

The survey action is included with harvesting as a valid reason for the clock to advance. This preserves the rule that a stationary boat does not progress the world clock unless the player is actively performing a meaningful action.

**Risk to watch:** Every future action that advances time must be explicitly registered and must release its active flag on completion, cancellation, disable and scene unload. A stuck active flag would make the sea clock run indefinitely.

### 3.8 SaveGameData.cs

**Role:** Data-only persistence contract.

Schema v5 adds:
- soundingPulseCount;
- discoveredPhenomenonSiteIds;
- evaluatedSurveyOpportunityIds.

The migration path advances schemas 0–4 to 5. Schema 4 saves receive safe weather defaults, no pending bird event, and empty acoustic/inspection history. Old campaign saves remain loadable without inventing player discoveries.

**Persistence rule:** Discovery and evaluated opportunities are durable campaign state. UI captions, audio clips, temporary echoes and presentation objects are transient and must not be saved.

## 4. Regional content integration

| Region | Survey profile | Intended deep-ocean content |
|---|---|---|
| Greyhaven | Shallow 18 m profile, no DSL | Harbor soundings, wreck/debris evidence, ordinary coastal acoustics |
| The Shallows | 52 m shelf profile, no DSL | Reef edge, seagrass, sand channels, fronts and fish schools |
| Blackrock Reach | 210 m exposed volcanic coast profile | Basalt walls, surge channels, caves, unstable rock echoes |
| The Drowned Parish | 34 m drowned valley profile | Flooded structures, submerged streets, debris, freshwater/saltwater interfaces |
| The Outer Shoals | 1,450 m nominal deep profile, DSL enabled | Diel scattering layer, Lantern Drift migration, pelagic fauna, deep ridges |
| The Hollow Trench | 3,850 m nominal trench profile, DSL enabled | Deep scattering layer, slope/canyon, seep/hydrate/vent/whale-fall sites where geology supports them |

The profile component currently contains baseline values for all six regions, but only Greyhaven is attached to a generated scene. Do not describe the other five profiles as playable level content.

The Outer Shoals and Hollow Trench profiles use illustrative depths and layer targets. When region geometry is authored, replace these with a region-specific bathymetric field and a more carefully researched photoperiod / migration model.

## 5. Scientific and design constraints

- A deep scattering layer is a biological volume in the water column, not a moving seabed.
- Diel vertical migration is a population-level pattern with species-specific timing, not a synchronized monster elevator.
- Brine pools are seabed depressions containing dense, chemically distinct brine. Surface vessels do not fall into them.
- Hydrothermal vents and cold seeps are different systems and need separate thermal/chemical profiles.
- Gas hydrates are pressure/temperature-dependent crystalline deposits; do not portray them as ordinary volcanic gas freezing instantly in open water.
- Whale falls are staged ecosystems. Osedax are bone-associated worms, not free-swimming predators.
- Brinicles are excluded from the current warm-water island-arc setting.
- Reported maritime stories stay labelled as reports unless independently verified.
- Psychological workload is not a sanity meter. Operational Strain remains proposed and is not implemented by this code.

## 6. Save compatibility and deterministic behaviour

The opportunity key includes region, day and pulse sequence. The pulse sequence is persisted; evaluated opportunity IDs provide an additional duplicate guard. A stable hash seeds the PCG32 stream used for small bathymetric variation.

When expanding the system:
1. Never change the meaning of an existing save field.
2. Increment the save schema and add an explicit migration.
3. Persist any new event state that can alter later gameplay.
4. Keep cosmetic RNG separate from gameplay RNG.
5. Test save/load between survey pulses and after a discovery.
6. Do not let loading an old save generate a new discovery or a new anomaly roll.

The current survey opportunity list is bounded to 256 entries because the monotonically increasing pulse count is the primary sequence source. AcousticWorldListener keeps a separate bounded 128-event ledger so the same emitted signal cannot be processed twice across save/load.

## 7. Performance and lifecycle considerations

- Region and site arrays are cached at Awake; dynamic scene streaming must refresh or register/unregister profiles explicitly.
- A survey performs a bounded linear scan over region profiles and site markers. This is acceptable for the current small prototype but should become a spatial query when many sites are active.
- The acoustic bus is event-driven; subscribers should not allocate large per-event collections or run expensive perception for distant inactive regions.
- Runtime-created AudioClip must be destroyed with the component. The current controller does this in OnDestroy.
- Survey action state must always be released. If a future animation, cancellation path or object disable interrupts the timer, ensure SeaStateDirector.SetSurveying(false) is called.
- Scene builders should use stable region/site IDs and avoid relying on hierarchy names as persistent identity.

## 8. Current tests and quality gates

Edit Mode tests cover:
- day/night scattering-layer depth ordering;
- same-region / changed-heading / time-window requirements for independent layer observations;
- seabed confidence versus sparse-layer confidence;
- mid-water versus seabed separation;
- stable opportunity hashing;
- shallow Greyhaven versus deep Outer Shoals profile configuration;
- save schema migration to v4.

Static validation checks required source files, the v4 schema marker, the SoundingPulse input, survey persistence fields and region profile identifiers.

These checks do not compile C# or execute Unity. Unity 6000.3.28f1 must still run Edit Mode tests and a Greyhaven Play Mode smoke test.

## 9. Next implementation sequence

1. Build a real bathymetry data asset and sample it by vessel position.
2. Connect species-specific creature perception adapters to AcousticWorldListener.SignalHeard and the existing AI state machines.
3. Add a water-column ecology volume whose density and movement are driven by regional fish/squid cohorts.
4. Add multi-contact sonar presentation with range, bearing and confidence.
5. Add separate physical controllers for brine pools, vents, seeps, hydrate deposits and whale falls.
7. Add persistent survey chart and journal evidence with interpretation confidence.
8. Add a proper sleep/rest loop before considering Operational Strain.
9. Record and integrate licensed field audio; replace synthesized placeholder pings.

## 10. Definition of done

The subsystem is not complete until:
- every playable region has a validated bathymetry profile;
- every authored phenomenon site has stable identity, evidence provenance and gameplay ownership;
- survey results are reproducible and survive save/load;
- sonar never mistakes a biological layer for a physical collision surface;
- critical warnings remain accessible without audio;
- acoustic threat responses use the existing Threat Director and warning/escape rules;
- Unity Edit Mode and Play Mode tests pass in the pinned editor;
- representative scene performance has been profiled.


## Current save-schema note — 2026-11-13

The survey, weather, horror, acoustic-history and temporary-atmosphere fields described here were introduced in schema v5 and remain part of current schema v6. Schema v6 subsequently adds bounded per-NPC relationship-event memory; it does not change the survey field ownership or stable site/opportunity IDs. See SAVE-DATA-STRUCTURE.md for the current migration contract.

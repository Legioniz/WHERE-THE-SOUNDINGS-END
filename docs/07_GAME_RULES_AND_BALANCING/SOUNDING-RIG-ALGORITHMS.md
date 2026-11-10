# Sounding Rig Algorithms

> Project: WHERE THE SOUNDINGS END  
> Status: First runtime slice implemented; full acoustic model proposed  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## 1. Current runtime pipeline

Input → Eligibility → Stable Opportunity ID → Regional Profile → Seabed Return → Optional Mid-water Return → Site Discovery → Acoustic Event → Save

The current implementation is a deterministic survey abstraction. It is not a physically complete sonar simulation. Do not describe it as ray-traced, frequency-calibrated, or hydrographically accurate.

### Runtime files

- DeepOceanSurveyMath.cs: pure time/depth/confidence/hash calculations.
- DeepOceanSurveyRegion.cs: authored regional depth and acoustic profile.
- DeepOceanPhenomenonSite.cs: discoverable site marker.
- SoundingRigController.cs: input, pulse, return interpretation, UI, event publication and persistence.
- AcousticEventBus.cs: typed signal metadata for future listeners.
- SeaStateDirector.cs: advances the action-driven clock during the short survey action.
- SaveGameData.cs: schema v4 survey state.

## 2. Pulse eligibility and pacing

A pulse is accepted only when:
- the game is not paused;
- a vessel Rigidbody exists;
- vessel speed is at or below the configured survey threshold (default 5 m/s);
- the vessel is inside an authored DeepOceanSurveyRegion;
- the real-time debounce / cooldown has elapsed.

The generated Greyhaven scene binds SoundingPulse to Q and gamepad right shoulder. The region profile is attached to Sheltered_Bay_Surface. A pulse takes approximately 1.25 real seconds; this counts as an action for the action-driven sea clock. Repeated input during the cooldown is ignored.

A pulse adds a small amount of sea disturbance (0.012). It does not directly add Panic, damage the hull, remove cargo, or trigger a threat.

## 3. Stable opportunity identity

The current opportunity key is:

{regionId}:day:{dayNumber}:sounding:{pulseSequence}

The sequence is persisted. A stable FNV-1a hash seeds the PCG32 generator for seabed variation. The evaluated opportunity ID is saved, with the latest 256 IDs retained as a defensive duplicate guard.

Do not use UnityEngine.Random for survey return decisions. Cosmetic ping timbre may use presentation randomness only if it cannot affect return classification.

## 4. Regional bathymetry profile

Each playable region root receives a DeepOceanSurveyRegion component. It supplies:
- stable region ID;
- horizontal survey radius;
- nominal seabed depth and bounded variation;
- whether a scattering layer is present;
- day and night layer depth;
- layer density;
- acoustic attenuation and background noise.

The six starter profile values live in ConfigureForRegion. They are game scale targets, not chart data. A scene builder must attach and configure the profile at the correct geographic root. Do not let an unconfigured profile silently inherit Outer Shoals or trench values.

## 5. Deep scattering layer model

The DSL is a biological volume in the water column. It is not a mesh, collider, reef, floor, or solid navigation obstacle.

Current interpolation:

D(h) = D_day + (D_night - D_day) W_night(h)

W_night(h) = 0.5 + 0.5 cos(2πh/24)

This gives a smooth shallower-at-night / deeper-by-day cycle with gradual dawn and dusk transitions. It is a stylised photoperiod curve, not a local astronomical ephemeris. Future regional profiles should add latitude, season, cloud / light attenuation, species composition and migration lag where required.

A return is treated as a distinct mid-water echo only when:
- the region profile has a DSL;
- layer depth and seabed depth are positive;
- the vertical separation meets the configured minimum (default 90 m).

The first eligible ping reports a *possible* biological layer. A second ping within ten in-game minutes, from a heading changed by at least 25 degrees, supports the repeatable interpretation. The player should compare the reading with charted depth and repeat the ping. The layer never changes the true seabed depth.

## 6. Confidence

The prototype confidence is:

C = clamp01(signalQuality × (0.35 + 0.65 × density) × (1 - attenuation))

The current code simplifies this to signal quality multiplied by a density term and acoustic attenuation. It does not yet model transducer power, frequency, range, beam angle, bottom composition, multipath, thermoclines or target aspect.

Never display confidence as a probability that a monster exists. It is only a quality estimate for the current return classification.

## 7. Discoverable phenomenon sites

Attach DeepOceanPhenomenonSite to an authored site marker. Required data:
- stable site ID;
- region ID;
- phenomenon type;
- player-facing name;
- approximate depth;
- detection radius;
- signal strength;
- first survey note;
- evidence label.

Supported types include biological layer, brine pool, hydrothermal vent, cold seep, gas hydrate, whale fall, bioluminescence, water-mass front, derelict vessel, damaged fishing gear, and unclassified acoustic return.

The current site component only reports and discovers a marker. It does not implement the site's physical hazard. Brine chemistry, vent heat, seep gas, hydrate stability, whale-fall succession and derelict-vessel boarding each require separate environment / interaction controllers with their own tests and warning rules.

## 8. Phenomenon placement and depth eligibility

DeepOceanPhenomenonRules is the runtime authority for whether an authored marker is plausible in its region and depth band. Invalid markers are logged during survey-controller initialization and excluded from discovery.

| Phenomenon | Allowed region(s) | Depth band |
|---|---|---:|
| Biological scattering layer | Outer Shoals, Hollow Trench | 80–1,400 m |
| Brine pool | Hollow Trench only | 1,000–6,000 m |
| Hydrothermal vent | Outer Shoals, Hollow Trench | 200–6,000 m |
| Cold seep | Outer Shoals, Hollow Trench | 200–6,000 m |
| Gas hydrate | Outer Shoals, Hollow Trench | 500–6,000 m |
| Whale fall | Outer Shoals, Hollow Trench | 300–6,000 m |
| Bioluminescence | All marine regions | 0–1,200 m |
| Water-mass front | Shallows, Blackrock Reach, Outer Shoals | 0–1,000 m |
| Derelict vessel | All marine regions | 0–6,000 m |
| Damaged fishing gear | All marine regions | 0–2,000 m |
| Unclassified acoustic return | All marine regions | 0–6,000 m |

These are setting constraints for the current provisional island-arc geography, not universal statements about where such phenomena exist in the real ocean. If the world tectonic model changes, revise the rule table and tests together. Brine pools remain restricted to the Hollow Trench until a specific geologically credible basin is authored elsewhere.

The site return now reports marker depth and signal confidence. Site confidence uses marker signal strength × signal quality × (1 − attenuation). Scattering-layer confidence uses layer density × signal quality × (1 − attenuation), with a baseline density factor. These values are survey-quality estimates, not probabilities that an interpretation is correct or that a hostile entity exists.

## 9. Acoustic event contract

A successful pulse publishes an AcousticSignal with:
- stable event ID;
- event kind;
- region ID;
- world position;
- normalized loudness;
- nominal propagation radius;
- game timestamp.

The event bus is a gameplay metadata channel. AI should subscribe and score audibility using range, occlusion, environment and hearing capability. Do not have every creature poll every AudioSource. Do not interpret the nominal radius as a physically exact sound-pressure boundary.

## 10. Save and load

Schema v4 stores:
- total accepted Sounding Rig pulse count;
- discovered phenomenon site IDs;
- evaluated survey opportunity IDs (bounded to the latest 256).

Old saves migrate with empty survey collections. A future site-state system must persist resolved interactions separately from discovery. Do not save transient UI captions, procedural audio clips, or temporary sonar visual effects.

## 11. Future full acoustic model

When the project moves beyond the prototype, add in this order:
1. Sound speed / depth-band propagation approximation and attenuation curves.
2. Transducer beam width, pulse frequency, range and target aspect.
3. Seabed material response and reef / wreck occlusion.
4. Water-column target volumes driven by fish / squid population cohorts.
5. Repeated-ping bearing and range comparison.
6. Acoustic event listener adapter for creature perception.
7. Persistent survey chart overlays and journal interpretation.
8. Instrument degradation from battery, transducer and hull condition.
9. Frequency-specific interference from vents, engines, rain and cavitation.

Keep simulation and presentation separate. The same AcousticSignal can inform sonar UI, creature perception, and narrative evidence, but no single consumer should own the underlying world event.

## 12. Acceptance tests

- Greyhaven profile never reports a deep scattering layer.
- Outer Shoals and Hollow Trench profiles provide a shallower layer at night than at noon.
- The layer is never a collider or seabed height.
- A single return is labelled uncertain; a second independent heading supports classification.
- Same opportunity ID produces the same seabed variation.
- Save/load preserves pulse count, discoveries and evaluated opportunity IDs.
- Sounding Rig pulses publish one acoustic event and add only the specified disturbance.
- Survey actions advance sea time; UI pause still freezes it.
- Missing region profile gives a clear unavailable-coverage message rather than fabricated depth.
- Site discovery does not itself apply physical hazard or threat damage.

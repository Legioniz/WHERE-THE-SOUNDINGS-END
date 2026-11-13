# Acoustic Navigation

> Project: WHERE THE SOUNDINGS END  
> Status: Acoustic event foundation and local fog-of-war chart prototype implemented; navigation-source presentation remains partial  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## 1. Player-facing fantasy

When fog removes landmarks, the player navigates by interpreting the sounds of the coast: lighthouse bell, buoy bells, surf, engine reflections, radio markers and the Sounding Rig. Audio is useful but never omniscient. A chart records only waters physically visited and evidence actually discovered.

## 2. Signal sources

| Source | Pattern | Reliability | Gameplay meaning |
|---|---|---|---|
| Lighthouse | Regular, long-range repeating bell | High when audible | Broad bearing toward Greyhaven when the signal can be heard |
| Bell buoy | Short repeating paired tone | High within authored range | Local hazard / channel marker |
| Surf against rock | Broad, irregular noise | Medium | Coastline and rock proximity |
| Own engine reflection | Changes with terrain and heading | Low–medium | Nearby geometry clue |
| Radio marker | Repeating coded pulse | Medium–high | Quest or navigation marker only after a reliable source is established |
| Sounding Rig | Player-controlled pulse + return | Variable | Depth / object / anomaly hypothesis |
| Creature call | Irregular authored phrase | Unknown | Possible living source / threat cue |

## 3. Bearing and confidence

Every acoustic source should provide a bearing estimate and confidence, not an exact world coordinate. The player triangulates by listening from two or more positions or comparing instruments. A false or reactive signal can have high strength but low pattern stability.

The regional chart does not turn an unknown acoustic event into a map pin. An unknown signal may be heard and investigated in the world, but no permanent map mark or compass guidance is granted until a reliable source/site is discovered. The chart must never draw a false-precision GPS marker for an unknown source.

## 4. Player actions

- Slow or stop the engine to improve listening.
- Rotate or reposition the boat to compare audible bearings.
- Wait for another buoy cycle.
- Use an already-known chart reference only when the source has been identified.
- Deploy the Sounding Rig for depth / echo evidence.
- Record a site mark only after an authored site is discovered.
- Mark a bearing with a chart note when player-authored notes are implemented; this is not yet a runtime feature.

The player should not be forced to use headphones or perfect stereo localization. Provide visual bearing arcs and subtitles for authored cues, but do not show a compass arrow to a destination that remains unknown.

## 5. Acoustic event triggers

- Buoy and lighthouse signals run on authored schedules.
- Signal anomalies trigger on explicit opportunity events (Rig deployment, tide transition, quest location entry).
- Threat calls are emitted by the threat state machine.
- Ambient animal calls are schedule / region based and do not automatically imply danger.

A signal event is not itself damage. A signal can mislead the player only when the world also supplies enough evidence to question it.

## 6. Current implementation boundary

SoundingRigController owns survey pulse input, regional profile selection, deterministic depth variation, site discovery, survey save state and acoustic event publication. AcousticWorldListener evaluates and persists heard-event history. RegionalChartController records physical region visits and exposes only Sounding Rig-discovered sites to the chart UI.

The chart is a local normalised survey plot based on the current region's authored centre/radius. It has no world-scale projection, compass, GPS, route graph, triangulated acoustic markers, player annotations or connected inter-region travel. See [Regional Chart and Discovery](REGIONAL-CHART-AND-DISCOVERY.md).

## 7. Tests

- Bearing estimates change as player position changes.
- Signal strength falls with distance and masking.
- Known buoy patterns remain distinguishable from anomaly patterns.
- A player can triangulate a source with two observations.
- An acoustic signal never bypasses safe-zone or quest gating.
- Unvisited regions remain absent from the chart.
- Discovered-site marks require both a visited region and the stable discovered site ID.

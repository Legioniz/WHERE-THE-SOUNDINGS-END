# Greyhaven — Environment and First-Hub Layout

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — procedural greybox; geography and final art remain provisional  
> Owner: Project lead  
> Version: 0.5  
> Last reviewed: 2026-11-12

## 1. Purpose

Greyhaven is the opening harbor, service hub, and dependable return point. It introduces the boat, harbor mouth, local trade, coastal landmarks, and the contrast between ordinary maritime life and the sea beyond the breakwater.

The central harbor is a reliable safe zone. Natural wildlife may pass through, but a full hostile chase encounter must not begin inside the protected harbor. The environment should feel inhabited and operational before it feels haunted.

This document defines the first playable-space greybox. It does not lock the archipelago's final global map, climate, tectonic placement, or species roster.

## 2. Spatial baseline

The editor generator uses metres as Unity world units. The first blockout spans approximately 220 m east–west and 330 m north–south including the visible water approach.

| Axis / area | Blockout convention |
|---|---|
| X negative | West headland and old breakwater |
| X positive | East estuary edge and mangrove fringe |
| Z negative | Inland village, ridge, and sheltered shore |
| Z positive | Harbor water, channel, and open-sea approach |
| Y | Height above the local water reference |

The shoreline is irregular rather than a straight wall. It pushes seaward near the middle of the bay, with higher volcanic ground behind the settlement and lower sediment edges toward the estuary. All dimensions are prototype values that must be tested against boat manoeuvring and camera framing.

## 3. District layout

### 3.1 West headland

- Greyhaven Lighthouse sits on the western rocky rise.
- The old breakwater follows the west side of the harbor mouth.
- Basalt outcrops establish the volcanic coast.
- The lighthouse is a visual and future acoustic bearing landmark; its bell/audio implementation is not part of this greybox.

### 3.2 Central harbor

- Main pier with a T-shaped end platform and two finger piers.
- Mooring piles, working decks, crates, and a net rack.
- Three navigation buoys mark the inner channel and outer approach.\n- A fixed local harbor chart stands on a table on the east half of the outer T-pier, offset from the central mooring lane. It marks the dock, village services, lighthouse, outer-channel wreck and the workplace of each named resident.
- The harbor mouth remains open between the west and east breakwater arms.
- The starter boat spawns outside the main dock with a turning lane to test.

### 3.3 Village services

The first pass places blockout buildings for the Greyhaven Fish Market, Harbour Office, Boat Repair Shed, Net Mender Shed, Harbour Inn, and four residential houses.

Buildings are readable by silhouette and position. Four blockout residents are generated: the Harbourmaster beside the quay, Mara Venn at the fish market, Tomas Vale near the repair shed, and Inez Sarmiento near the net-mending shed. Each has a boat-proximity dialogue trigger, a stable NPC ID, a daily activity schedule and a simple waypoint destination. Their dialogue nodes and schedule state are saved through the existing Yarn and NPC persistence paths. Interior rooms, doors, shop transactions, and a full quest objective system remain unimplemented.

### 3.4 East estuary edge

- Sediment flats transition into a mangrove-fringed margin.
- Prop roots, trunks, and canopies are placeholder geometry.
- The mudflat and mangrove edge provide future habitats for burrowing crabs, juvenile fish, shorebirds, and estuary food webs.
- This is not a separate freshwater map; salinity and tidal exchange remain part of the estuary design.

### 3.6 Working buildings and resident wayfinding

The generated service buildings now use distinct static architectural silhouettes rather than identical blank cubes. Shared construction includes a foundation plinth, visible corner posts, pitched gable roof and gable caps, a framed door, divided windows, shutters, steps and service signage. The Fish Market has a public counter and canopy; the Harbour Office has a covered porch and bell frame; Boat Repair has a wide service bay and overhead hoist; the Net Mender Shed has a side lean-to and hanging net lines; and the Inn has a veranda and bench. Four inland houses receive smaller porches and chimneys.

Residents are positioned at the service fronts a player would naturally visit: the Harbourmaster keeps quay watch on the main pier and uses the Harbour Office entrance after hours; Mara works at the Fish Market; Tomas works at Boat Repair; and Inez works at the Net Mender Shed. Their exact generated coordinates and secondary anchors are recorded in [Greyhaven Settlement, Buildings and Wayfinding](GREYHAVEN-SETTLEMENT-AND-WAYFINDING.md). Stable NPC IDs, schedule windows, dialogue nodes and save behavior remain unchanged.

The east half of the T-pier carries a physical chart table. Its schematic land outline is sampled from the same Greyhaven terrain profile as the generated island. The chart marks local services, resident workplaces, dock, lighthouse and wreck field. The "you are here" marker is fixed to the dock chart location; this is not a live minimap or a calibrated nautical chart.

### 3.5 Inland ridge and village lanes

- Irregular terrain rises behind the houses.
- A compact road network connects the market, office, repair shed, inn, and residential lane.
- Street lamps and crates establish scale and routes.
- The ridge and headlands frame the harbor without hiding the lighthouse silhouette.

## 4. Water and navigation

The starter water surface is an opaque greybox plane with a runtime procedural swell mesh. Its waves are visual only; there is no hydrodynamic buoyancy or current simulation. The water object carries a shallow Greyhaven survey profile (nominal 18 m depth) and deliberately has no deep scattering layer.

Navigation readability is built around three cues: the lighthouse silhouette on the west headland, red/green channel markers at the harbor mouth, and a red outer-approach buoy beyond the inner piers.

The current scene does not implement animated tide, wind, wave forces, current fields, calibrated acoustic propagation, buoy sounds, or a working lighthouse beam. The Sounding Rig provides a prototype regional depth return and publishes an acoustic gameplay event; it is not a physical sonar simulation. The action-driven clock, panic pressure, and basic fog/light transition are implemented, but dynamic tide/current simulation and authored audio assets remain future work.

## 5. Environment response and ecology

Greyhaven must retain its protected-harbor rule. The inner harbor is not a place where the Threat Director can begin a full hostile chase.

The regional ecology baseline supports mudflat crabs and shorebirds during suitable low tide, juvenile fish and mullet using shallow channels and sheltered margins, attached filter feeders on pilings, small fish sheltering around roots and dock structures, and a non-hostile Mooring Ray visiting the outer harbor on an incoming tide.

The greybox does not spawn, animate, or simulate these ordinary animals. Do not read prop placement as implemented ecology. A wreck/debris field is placed in the outer channel as the first localized dredging site. It also has a discoverable Sounding Rig site marker with a stable ID, a shallow-depth return and a fictional-site evidence label.

## 6. Action-driven gameplay in Greyhaven

- The sea clock starts at 07:00 and advances at 72 in-game seconds per real second only while the vessel's horizontal speed exceeds 0.25 m/s or a fishing/dredging/survey action is active. This produces a 24-hour cycle in approximately 20 real minutes.
- Bringing the boat to a complete stop freezes time. Opening the cargo hold or entering dialogue also freezes time. The inventory remains a deliberate planning pause.
- Night begins at 19:00 and ends at 05:00. Night and heavy fog increase panic; daylight reduces it, with faster recovery in the sheltered harbor.
- Panic alters fog colour/density and directional-light intensity. At high panic, phantom reef shapes and leviathan-scale shadows can appear. They are visual hallucinations, not collision geometry. At extreme panic, phantom seabirds may steal one ordinary fish from the hold after a warning omen.
- F starts a three-beat fishing timing challenge. Successful catches enter the shared spatial hold. Aberrant variants have unique item IDs and 2.8×–3.8× prototype value multipliers.
- G starts a four-beat dredging challenge only when the boat is stopped within range of the outer-channel wreck field. The site yields timber, salt-stiff canvas and corroded fittings, with three pulls per in-game day.
- Q (or gamepad right shoulder) emits a Sounding Rig pulse when vessel speed is at or below 5 m/s. The survey reports a repeatable shallow seabed return and can discover the outer-channel wreck marker. It does not report a deep scattering layer in Greyhaven.
- Engine, handline rod and deck lantern begin installed in the same 7×9 grid as cargo. Hard collisions reduce hull and engine condition, map the impact point to a hold cell, destroy the item occupying that cell and leave the cell blocked. Damaged cells and vessel condition are saved.

These values are first-pass prototype tuning, not final balance. The current soundscape code adjusts pitch and volume on registered AudioSources, but no authored music/ambience clips or mixer snapshots are connected yet.

## 7. Visual and atmosphere targets

The visual target is restrained, weathered and maritime: dark blue-green water, muted basalt and soil, subdued grass, narrow sand shelves, weathered timber and concrete, and overcast coastal lighting with moderate fog. The environment should read as a lived-in fishing island under a heavy maritime sky, not a bright tropical resort.

The generated island now uses a deterministic multi-scale heightfield with irregular shoreline sampling, distinct west/east headlands, an inland ridge, a rear terrace and smaller ridged surface variation. Its higher-resolution land mesh is split into soil, sand, grass and basalt submeshes using elevation, slope and shoreline distance. The sand band conforms to the mesh edge rather than using disconnected rectangular beach pieces. Static basalt outcrops use faceted, deterministic rock geometry.

These are improved procedural greybox forms, not final production models or texture work. The existing material palette is reused; authored texture sets, erosion decals, shoreline foam, vegetation variety, animated water, particles and final environment assets remain separate work. See [Greyhaven Procedural Island Terrain](../08_TECHNICAL_DESIGN/GREYHAVEN-TERRAIN.md) for the terrain profile and validation requirements.

## 7. Scene generation and controls

The environment is generated in the Unity Editor by:

- Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation on first setup.
- Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment to regenerate the scene after a builder change. This command warns before replacing the generated scene.

Output scene: Assets/_Project/Scenes/Greyhaven_Blockout.unity.

The prototype boat controller is active. W/A/S/D or the left gamepad stick apply throttle and steering. Q or gamepad right shoulder emits a Sounding Rig pulse when the boat is slow enough to survey. When the boat enters a resident's interaction radius, the HUD displays an E prompt naming that resident; press E to start their Yarn Spinner conversation. The initial cast and schedule rules are documented in [Greyhaven NPC System](../12_AI_AND_SIMULATION/GREYHAVEN-NPC-SYSTEM.md). Press Space/Enter to advance lines and click a response or use keys 1–5 for choices. Press I to open the 7×9 cargo hold; select an item, click an empty cell to move it, press R to rotate, and use Esc to close. The interface's catalogue intake buttons are prototype test tools, not fishing. The fixed-angle orthographic camera follows the boat and is tuned to show the pier, village edge, and channel during initial testing.

## 8. Acceptance checks

Before the greybox is promoted beyond working baseline, verify in Unity:

- [ ] The setup menu completes without compile or import errors.
- [ ] The scene opens with the URP pipeline and no missing materials.
- [ ] The water surface, terrain mesh, shoreline, and terrain material-submesh boundaries render without visible gaps.\n- [ ] The west headland, east ridge, inland spine, and low coastal shelf read as distinct landforms from the gameplay camera.\n- [ ] Sand, soil, grass, and basalt surfaces remain legible under the overcast lighting and fog.\n- [ ] Buildings, roads, NPC anchors, the lighthouse, and breakwater props remain seated on the regenerated heightfield.\n- [ ] The single terrain MeshCollider and faceted outcrop colliders do not create boat-catching seams or unstable contacts.
- [ ] The boat moves, reverses, and steers in the open channel.
- [ ] The boat does not collide with the dock at spawn.
- [ ] Piers, breakwater rocks, and terrain have appropriate collision.
- [ ] The harbor opening remains navigable at the prototype boat's turning radius.
- [ ] The boat trigger can enter/exit all four resident interaction radii and each E prompt names the correct NPC.
- [ ] The four Yarn dialogue nodes present the correct resident lines and branches.
- [ ] NPC daily activities update from the action-driven sea clock and residents use only their authored short waypoints.
- [ ] NPC relationship scores and Yarn dialogue variables survive manual save and autosave.
- [ ] Accepting the Harbourmaster's buoy job writes a checkpoint autosave.
- [ ] The cargo hold opens/closes, supports valid move/rotate/reject operations, and restores placements through manual save and autosave.
- [ ] The lighthouse and channel buoys remain identifiable at gameplay camera scale.\n- [ ] Service building silhouettes, roof/gable meshes, front signs, porches, market counter, repair hoist, net-mender work lean-to and inn veranda render without missing meshes.\n- [ ] The dock chart table is fully supported on the east T-pier, remains outside the central mooring lane and presents the correct shoreline, workplaces, landmarks and wreck marker.\n- [ ] The chart text and building signs remain readable from the orthographic gameplay camera.\n- [ ] Harbourmaster, Mara, Tomas and Inez occupy their documented service locations during their day windows and use clear nearby secondary anchors.
- [ ] Q emits one survey pulse while stopped, reports shallow depth and can discover the outer-channel wreck marker.
- [ ] Greyhaven never reports a deep scattering layer.
- [ ] The scene is included in the Windows build scene list.
- [ ] Edit Mode tests and a Windows development build pass.

No Editor rendering, Play Mode, physics, or Windows build result is claimed by repository changes alone. The scene must be generated and checked in the pinned Unity Editor.

## 9. Implementation boundary

Implemented by this pass: deterministic 144 × 96 multi-scale island heightfield, irregular mesh-conforming shoreline and sand band, slope/elevation-based soil/sand/grass/basalt submeshes, faceted basalt outcrops, service-specific building blockout details and signs, resident workplace placement, fixed schematic harbor chart table, visual-only procedural swell surface, blockout district geometry, piers, breakwater, navigation markers, lighthouse silhouette, village service and residential placeholders, mangrove/estuary props, starter boat, configurable 2.5D camera framing, a 7×9 spatial cargo hold with runtime interface, the first four Greyhaven NPC blockouts with schedules and separate Yarn dialogue nodes, a shallow regional survey profile, a Sounding Rig pulse loop, and a discoverable outer-channel wreck marker.

Not implemented: final environment art, optimized production meshes, animated or physically simulated water/tide/current/weather/waves, walkable character controller, interiors, advanced NPC pathfinding, interiors, shops and sale transactions, full quest objective/journal logic, fishing and real catch intake, fish populations, ordinary wildlife, creature AI, hostile threat logic, acoustic navigation, streaming, or region transitions. Cargo UI prototype intake controls are not a substitute for fishing.

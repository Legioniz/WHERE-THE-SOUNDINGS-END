# Greyhaven — Environment and First-Hub Layout

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — generated greybox, boat-operated dockside menus and additive static-area streaming; Unity verification pending  
> Owner: Project lead  
> Version: 0.6  
> Last reviewed: 2026-11-14

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
- Three navigation buoys mark the inner channel and outer approach.
- A fixed local harbor chart stands on a table on the east half of the outer T-pier, offset from the central mooring lane. It marks the dock, village services, lighthouse, outer-channel wreck and the workplace of each named resident.
- The harbor mouth remains open between the west and east breakwater arms.
- The starter boat spawns outside the main dock with a turning lane to test.

### 3.3 Village services

The first pass places blockout buildings for the Greyhaven Fish Market, Harbour Office, Boat Repair Shed, Net Mender Shed, Harbour Inn, and four residential houses.

Buildings are readable by silhouette and position. Four blockout residents are generated: the Harbourmaster beside the quay, Mara Venn at the fish market, Tomas Vale near the repair shed, and Inez Sarmiento near the net-mending shed. Each has a stable NPC ID, a daily activity schedule and a simple waypoint destination. From a stationary vessel at a shore approach, the player opens the dockside hub and selects Harbour Office, Fish Market, Boat Repair Shed or Net Mender Shed to speak with the assigned resident through the existing Yarn and NPC relationship systems. Generated resident triggers are externally managed so they do not compete for the shared Interact action. Menu-based interior presentations and furniture actions exist; walkable interior rooms, doors and on-foot movement do not. Shop transactions and a full quest objective system remain unimplemented.

### 3.4 East estuary edge

- Sediment flats transition into a mangrove-fringed margin.
- Prop roots, trunks, and canopies are placeholder geometry.
- The mudflat and mangrove edge provide future habitats for burrowing crabs, juvenile fish, shorebirds, and estuary food webs.
- This is not a separate freshwater map; salinity and tidal exchange remain part of the estuary design.

### 3.5 Inland ridge and village lanes

- Irregular terrain rises behind the houses.
- A compact road network connects the market, office, repair shed, inn, and residential lane.
- Street lamps and crates establish scale and routes.
- The ridge and headlands frame the harbor without hiding the lighthouse silhouette.

### 3.6 Working buildings and resident wayfinding

The generated service buildings now use distinct static architectural silhouettes rather than identical blank cubes. Shared construction includes a foundation plinth, visible corner posts, pitched gable roof and gable caps, a framed door, divided windows, shutters, steps and service signage. The Fish Market has a public counter and canopy; the Harbour Office has a covered porch and bell frame; Boat Repair has a wide service bay and overhead hoist; the Net Mender Shed has a side lean-to and hanging net lines; and the Inn has a veranda and bench. Four inland houses receive smaller porches and chimneys.

Residents are positioned at the service fronts a player would naturally visit: the Harbourmaster keeps quay watch on the main pier and uses the Harbour Office entrance after hours; Mara works at the Fish Market; Tomas works at Boat Repair; and Inez works at the Net Mender Shed. Their exact generated coordinates and secondary anchors are recorded in [Greyhaven Settlement, Buildings and Wayfinding](GREYHAVEN-SETTLEMENT-AND-WAYFINDING.md). Stable NPC IDs, schedule windows, dialogue nodes and save behavior remain unchanged.

The east half of the T-pier carries a physical chart table. Its schematic land outline is sampled from the same Greyhaven terrain profile as the generated island. The chart marks local services, resident workplaces, dock, lighthouse and wreck field. The "you are here" marker is fixed to the dock chart location; this is not a live minimap or a calibrated nautical chart.

## 4. Water and navigation

The starter water surface is a procedural swell mesh driven by a shared analytic surface sampler. The workboat uses a lightweight four-point spring-buoyancy approximation against that same surface; this is not a full fluid solver and does not provide a current simulation. The water object carries a shallow Greyhaven survey profile (nominal 18 m depth) and deliberately has no deep scattering layer.

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

These are improved procedural greybox forms, not final production models or texture work. The existing material palette is reused; authored texture sets, erosion decals, shoreline foam, vegetation variety, animated water, particles and final environment assets remain separate work.

The latest generated art pass adds a district-aware dressing layer through `GreyhavenMaritimeAssetBuilder`. The harbour now has recognizable mooring bollards, cleats, glass lanterns, anchors, lobster pots, float lines and rope/fender wear. Village work areas use stave barrels, slatted fish crates, a hand winch, propeller, net-drying rack, wash tub and work-worn ground patches. The estuary has four varied mangrove silhouettes with exposed prop roots, reed and salt-grass clumps, storm driftwood, a collapsed crab pot, abandoned float line and tide-line silt. The lighthouse headland receives a keeper weather station, storm lantern and wind-felled debris.

These are assembled, deterministic, project-authored procedural meshes rather than final sculpted models or texture sets. The new details are assigned to the existing harbour, village, estuary and landmark roots; they add no gameplay components or collision. Streamed-area revision 3 requires all six generated Greyhaven area scenes to be rebuilt. See [Environment Assets and Set Dressing](../09_ART_AND_VISUAL_DIRECTION/ENVIRONMENT-ASSETS.md) for the asset families, ownership and verification criteria. See [Greyhaven Procedural Island Terrain](../08_TECHNICAL_DESIGN/GREYHAVEN-TERRAIN.md) for the terrain profile and validation requirements.

## 7. Scene generation and controls

The environment is generated in the Unity Editor by:

- Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation on first setup.
- Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment to regenerate the scene after a builder change. This command warns before replacing the generated scene.

Output scene: Assets/_Project/Scenes/Greyhaven_Blockout.unity.

The prototype boat controller is active. W/A/S/D or the left gamepad stick apply throttle and steering. Q or gamepad right shoulder emits a Sounding Rig pulse when the boat is slow enough to survey. At a named dockside approach, the HUD displays an E prompt; press E while horizontal vessel speed is at or below 0.85 m/s to open the seven-location dockside hub. Choose Main Dock, Harbour Office, Fish Market, Boat Repair Shed, Net Mender Shed, Harbour Inn or Western Lighthouse, then select a furniture action. Escape or gamepad Start closes the hub. The menu pauses simulation and cannot overlap inventory, dialogue or the title/pause/save/settings overlays. Resident choices hand off to the existing Yarn Spinner nodes and relationship gates. Press Space/Enter to advance dialogue and click a response or use keys 1–5 for choices. Press I to open the 7×9 cargo hold; select an item, click an empty cell to move it, press R to rotate, and use Esc to close. The interface's catalogue intake buttons are prototype test tools, not fishing. The fixed-angle orthographic camera follows the boat and is tuned to show the pier, village edge, and channel during initial testing.

## 8. Acceptance checks

Before the greybox is promoted beyond working baseline, verify in Unity:

- [ ] The setup menu completes without compile or import errors.
- [ ] The scene opens with the URP pipeline and no missing materials.
- [ ] The water surface, terrain mesh, shoreline, and terrain material-submesh boundaries render without visible gaps.
- [ ] The west headland, east ridge, inland spine, and low coastal shelf read as distinct landforms from the gameplay camera.
- [ ] Sand, soil, grass, and basalt surfaces remain legible under the overcast lighting and fog.
- [ ] Buildings, roads, NPC anchors, the lighthouse, and breakwater props remain seated on the regenerated heightfield.
- [ ] The single terrain MeshCollider and faceted outcrop colliders do not create boat-catching seams or unstable contacts.
- [ ] The boat moves, reverses, and steers in the open channel.
- [ ] The boat does not collide with the dock at spawn.
- [ ] Piers, breakwater rocks, and terrain have appropriate collision.
- [ ] The harbor opening remains navigable at the prototype boat's turning radius.
- [ ] The dockside hub prompt appears only inside a shore-approach radius while the vessel is stationary (0.85 m/s or below).
- [ ] E opens each of the seven location menus; furniture actions display the correct read-only inspection or authored information.
- [ ] Escape and gamepad Start close the hub and restore the previous time scale; inventory, dialogue and front-end pause cannot overlap it.
- [ ] The four resident buttons hand off to the correct Yarn nodes and relationship gates.
- [ ] NPC daily activities update from the action-driven sea clock and residents use only their authored short waypoints.
- [ ] NPC relationship scores and Yarn dialogue variables survive manual save and autosave.
- [ ] Accepting the Harbourmaster's buoy job writes a checkpoint autosave.
- [ ] The cargo hold opens/closes, supports valid move/rotate/reject operations, and restores placements through manual save and autosave.
- [ ] The lighthouse and channel buoys remain identifiable at gameplay camera scale.
- [ ] Service building silhouettes, roof/gable meshes, front signs, porches, market counter, repair hoist, net-mender work lean-to and inn veranda render without missing meshes.
- [ ] The dock chart table is fully supported on the east T-pier, remains outside the central mooring lane and presents the correct shoreline, workplaces, landmarks and wreck marker.
- [ ] The chart text and building signs remain readable from the orthographic gameplay camera.
- [ ] Harbourmaster, Mara, Tomas and Inez occupy their documented service locations during their day windows and use clear nearby secondary anchors.
- [ ] Q emits one survey pulse while stopped, reports shallow depth and can discover the outer-channel wreck marker.
- [ ] Greyhaven never reports a deep scattering layer.
- [ ] The scene is included in the Windows build scene list.
- [ ] Edit Mode tests and a Windows development build pass.

No Editor rendering, Play Mode, physics, or Windows build result is claimed by repository changes alone. The scene must be generated and checked in the pinned Unity Editor.

## 9. Implementation boundary

Implemented runtime/editor foundations include: deterministic 144 × 96 multi-scale island heightfield, irregular mesh-conforming shoreline and sand band, slope/elevation-based soil/sand/grass/basalt submeshes, faceted basalt outcrops, service-specific building blockout details and signs, resident workplace placement, fixed schematic harbour chart table, procedural swell surface with four-point spring buoyancy, blockout district geometry, piers, breakwater, navigation markers, lighthouse silhouette, village service and residential placeholders, mangrove/estuary props, starter boat, configurable 2.5D camera framing, a 7×9 spatial cargo hold with runtime interface, four Greyhaven NPC blockouts with schedules and separate Yarn dialogue nodes, the persistent seven-location dockside hub and furniture actions, a shallow regional survey profile, a Sounding Rig pulse loop, a discoverable outer-channel wreck marker, and six additive static Greyhaven area scenes with transition telemetry and an authored Play Mode test.

Not implemented: final environment art, optimized production meshes, physical tide/current simulation, walkable on-foot character controller and 3D interiors, advanced NPC pathfinding, shops and sale transactions, paid repairs, net crafting, full quest objective/journal logic, real catch intake, fish populations, ordinary wildlife, creature AI, hostile threat logic, full acoustic navigation, and inter-region travel. Greyhaven static scenery is split across six additive area scenes, but that is local area streaming, not regional travel. The dockside hub is a functional menu presentation with furniture actions, not walkable interior geometry. Cargo UI prototype intake controls are not a substitute for fishing.


## 10. Environmental artifact dressing — 2026-11-13

The Greyhaven Editor builder adds deterministic environmental artifact clusters beneath the harbour, village, estuary, lighthouse and outer-channel wreck ownership roots. Existing details include repaired fishing equipment, work residue, domestic objects, retired machinery, shoreline debris and weathered keeper equipment. The wreck cluster is parent-local so its details remain attached to the offset salvage-site transform.

The 2026-11-14 extension adds a partial lower-inlet landing and low former-settlement foundation on the western mudflat, plus exposed keel/rib/crossmember structure and a sheared spar on the existing wreck. Western and eastern mudflat dressing now uses separate top-level roots at negative and positive world X so each is assigned to its matching streamed area scene.

The original environmental prop clusters and the 2026-11-14 physical-remains extension remain static, collider-free set dressing; those visuals do not become loot, sonar discoveries, chart marks, quest flags or NPC events. Six dedicated HistoricalArtifact metadata anchors are the deliberate interaction exception: they are stored beneath the persistent scene root, use the existing boat Interact inspection controller and persist in a separate schema-v9 ledger. The streamed-area revision is 4 and all six generated area scenes must be rebuilt. See [Environmental Storytelling](../02_LORE_AND_WORLD/ENVIRONMENTAL-STORYTELLING.md), [Regional Physical Remains](REGIONAL-PHYSICAL-REMAINS.md), [Historical Artifacts](HISTORICAL-ARTIFACTS-AND-ARCHIVAL-EVIDENCE.md) and [Environment Assets](../09_ART_AND_VISUAL_DIRECTION/ENVIRONMENT-ASSETS.md).

Unity rebuild and camera-height review remain required. In particular, inspect prop grounding, overlap, readability, pier/service clearance, wreck-site placement and streaming ownership before accepting the visual result.

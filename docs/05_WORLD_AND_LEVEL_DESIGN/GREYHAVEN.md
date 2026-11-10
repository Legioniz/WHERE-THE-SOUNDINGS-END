# Greyhaven — Environment and First-Hub Layout

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline — procedural greybox; geography and final art remain provisional  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

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
- The harbor mouth remains open between the west and east breakwater arms.
- The starter boat spawns outside the main dock with a turning lane to test.

### 3.3 Village services

The first pass places blockout buildings for the Greyhaven Fish Market, Harbour Office, Boat Repair Shed, Net Mender Shed, Harbour Inn, and four residential houses.

Buildings are readable by silhouette and position. Interior rooms, NPC schedules, dialogue, doors, shop transactions, and quest logic are not implemented by this environment pass.

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

## 4. Water and navigation

The starter water surface is a static, opaque material plane. It is a placeholder for later water rendering and environmental simulation.

Navigation readability is built around three cues: the lighthouse silhouette on the west headland, red/green channel markers at the harbor mouth, and a red outer-approach buoy beyond the inner piers.

The current scene does not implement animated tide, wind, wave forces, current fields, acoustic propagation, buoy sounds, or a working lighthouse beam. Those features require their own gameplay and technical specifications.

## 5. Environment response and ecology

Greyhaven must retain its protected-harbor rule. The inner harbor is not a place where the Threat Director can begin a full hostile chase.

The regional ecology baseline supports mudflat crabs and shorebirds during suitable low tide, juvenile fish and mullet using shallow channels and sheltered margins, attached filter feeders on pilings, small fish sheltering around roots and dock structures, and a non-hostile Mooring Ray visiting the outer harbor on an incoming tide.

The greybox does not spawn, animate, or simulate these animals. Do not read prop placement as implemented ecology.

## 6. Visual and atmosphere targets

The first pass uses primitive geometry with restrained coastal colours: dark blue-green water; weathered timber and metal; muted basalt and concrete; sand and mud at the shoreline; subdued vegetation; overcast, low-contrast lighting with moderate coastal fog.

The desired read is a lived-in fishing harbor under a heavy maritime sky—not a bright tropical resort. Production models, surface textures, decals, erosion, shoreline foam, water movement, particles, and audio are separate work.

## 7. Scene generation and controls

The environment is generated in the Unity Editor by:

- Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation on first setup.
- Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment to regenerate the scene after a builder change. This command warns before replacing the generated scene.

Output scene: Assets/_Project/Scenes/Greyhaven_Blockout.unity.

The prototype boat controller is active. W/A/S/D or the left gamepad stick apply throttle and steering. Other mapped actions remain reserved. The fixed-angle orthographic camera follows the boat and is tuned to show the pier, village edge, and channel during initial testing.

## 8. Acceptance checks

Before the greybox is promoted beyond working baseline, verify in Unity:

- [ ] The setup menu completes without compile or import errors.
- [ ] The scene opens with the URP pipeline and no missing materials.
- [ ] The water surface, terrain mesh, and shoreline render without visible gaps.
- [ ] The boat moves, reverses, and steers in the open channel.
- [ ] The boat does not collide with the dock at spawn.
- [ ] Piers, breakwater rocks, and terrain have appropriate collision.
- [ ] The harbor opening remains navigable at the prototype boat's turning radius.
- [ ] The lighthouse and channel buoys remain identifiable at gameplay camera scale.
- [ ] The scene is included in the Windows build scene list.
- [ ] Edit Mode tests and a Windows development build pass.

No Editor rendering, Play Mode, physics, or Windows build result is claimed by repository changes alone. The scene must be generated and checked in the pinned Unity Editor.

## 9. Implementation boundary

Implemented by this pass: procedural terrain mesh and shoreline, static water plane, blockout district geometry, piers, breakwater, navigation markers, lighthouse silhouette, village service and residential placeholders, mangrove/estuary props, starter boat, and configurable 2.5D camera framing.

Not implemented: final environment art, optimized production meshes, animated or physically simulated water/tide/current/weather/waves, walkable character controller, interiors, NPCs, shops, quests, dialogue, fishing, fish populations, ordinary wildlife, creature AI, hostile threat logic, acoustic navigation, persistence, streaming, or region transitions.

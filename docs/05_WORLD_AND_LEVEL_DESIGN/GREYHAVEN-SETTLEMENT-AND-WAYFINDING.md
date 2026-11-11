# Greyhaven Settlement, Buildings and Wayfinding

> Project: WHERE THE SOUNDINGS END  
> Status: Working generated-greybox specification — Editor verification pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-12

## 1. Purpose and limits

Greyhaven is the first working harbor settlement. Its generated buildings should read as practical coastal architecture shaped by fishing, repair work, net making, local administration and hospitality. Distinct silhouettes and readable service fronts take priority over generic asset volume.

This pass improves generated greybox architecture and navigation aids. It does not add interiors, a walkable player controller, shop transactions, repair interactions, resident pathfinding or final production art. The generated chart is a fixed local harbor chart, not a live GPS/minimap system.

## 2. Settlement layout

The existing landform, building footprints, roads and service district coordinates remain the spatial baseline. Service buildings remain grouped around the village lane behind the harbor. Residential structures sit farther inland, while the lighthouse remains the western headland bearing.

| Structure | Centre (X, Z) | Generated treatment |
|---|---:|---|
| Greyhaven Fish Market | (-27, -35) | Timber market hall, pitched roof, public sign, front service counter, canopy and fish/crate work props |
| Harbour Office | (21, -32) | Concrete office, pitched roof, public sign, covered entry porch and bell frame |
| Boat Repair Shed | (1, -46) | Wide timber work shed, public sign, framed service bay, sliding-door panels, overhead hoist and tool bench |
| Net Mender Shed | (-48, -52) | Timber work shed, public sign, side lean-to, outdoor work surface and hanging net lines |
| Harbour Inn | (39, -55) | Larger timber inn, public sign, veranda, railings and bench |
| West House | (-55, -73) | Residential porch, shutters and chimney |
| Centre House | (-19, -78) | Residential porch, shutters and chimney |
| East House | (22, -78) | Residential porch, shutters and chimney |
| Ridge House | (55, -72) | Residential porch, shutters and chimney |
| Greyhaven Lighthouse | (-86, -22) | Existing western headland landmark |

### Architectural grammar

All service and residential buildings use the existing weathered timber, concrete, muted roof, glass and trim materials. Shared elements include a foundation plinth, visible corner posts, a pitched gable roof with end caps and ridge/eave trim, a framed front door, divided windows with shutters, steps, and service signs where appropriate.

Building-specific work areas distinguish the five public-facing buildings:
- Market canopy and counter face the village lane.
- Harbour Office porch and bell frame identify the administrative entrance.
- Repair shed's wider bay, sliding panels and hoist identify mechanical work.
- Net Mender side lean-to and hanging net lines identify net repair.
- Inn veranda and bench make it a social stop rather than another service box.

These remain static blockout meshes. They do not imply working doors, interior spaces, animated equipment, repair transactions or net crafting.

## 3. Residents and findable workplaces

Stable NPC IDs, dialogue node names, occupations, schedule windows and save contracts are unchanged.

| Resident | Daytime location | Off-duty destination | Daily window |
|---|---|---|---|
| Harbourmaster | Quay-watch point on the inner main pier, approximately (-1.6, 12) | Harbour Office entrance, approximately (21, -28.5) | 05:00–18:00 quay; 18:00–05:00 office |
| Mara Venn | Fish Market front, approximately (-27, -28.5) | Market-side secondary anchor, approximately (-23, -29) | 04:00–16:00 market; 16:00–04:00 off duty |
| Tomas Vale | Boat Repair Shed front, approximately (1, -40) | Repair-side secondary anchor, approximately (5, -38) | 06:00–18:00 repair; 18:00–06:00 off duty |
| Inez Sarmiento | Net Mender Shed front, approximately (-48, -46) | Net-shed-side secondary anchor, approximately (-44, -46) | 07:00–17:00 net work; 17:00–07:00 off duty |

The first coordinate is X and the second is Z; Y is sampled from the island profile except for the Harbourmaster's quay-watch position, which is placed on the main pier deck. The short secondary anchors preserve the current direct-waypoint movement model. They must remain clear of walls, props, shoreline edges and other residents.

The chart legend explicitly associates:
- Harbourmaster with the Harbour Office;
- Mara Venn with the Fish Market;
- Tomas Vale with Boat Repair;
- Inez Sarmiento with the Net Mender Shed.

The chart identifies workplaces and known resident names; it does not display live schedule state or guarantee that a resident is currently present at the location.

## 4. East T-pier harbor chart

A fixed physical chart table is generated on the east half of the outer T-pier, centred near (7.35, 33) in world X/Z. It sits within the existing deck footprint and is offset from the central mooring lane.

The chart is built from first-party scene geometry and the same `GreyhavenTerrainProfile` used by the island generator:
- muted sea field and a land silhouette sampled from the procedural shoreline;
- a shoreline outline and schematic village access/road lines;
- map markers for the dock, Fish Market, Harbour Office, Boat Repair, Net Mender, Harbour Inn, lighthouse and Outer Channel Wreck Field;
- a fixed "you are here" marker at the chart table's dock location;
- a compact key that identifies the residents' workplaces.

The map uses a schematic top-down local projection for orientation. It is not a calibrated nautical chart, depth chart, GPS display, route planner or navigational safety instrument. It has no live player-position update and no interactable zoom/pan yet.

## 5. Generation and compatibility

The generated scene remains owned by `GreyhavenEnvironmentBuilder`. Run **Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment** in Unity `6000.3.28f1` after pulling these source changes.

No save schema, stable NPC identifier, dialogue node, market relationship value, input action, sea-clock rule, terrain height profile, survey region or boat physics contract changes in this pass.

## 6. Validation checklist

In the pinned Unity Editor and Play Mode, verify:
- [ ] Every service building has a distinct, readable front and service silhouette.
- [ ] Pitched roof panels and gable caps render with outward-facing normals.
- [ ] Signs are legible from the current orthographic gameplay camera.
- [ ] Door steps, veranda decks, canopies, hoist pieces and net lines do not create unwanted collision blockers.
- [ ] The chart table is fully supported by the east half of the T-pier and leaves a usable central mooring lane.
- [ ] The chart's land silhouette matches the generated Greyhaven shoreline.
- [ ] Chart markers align with the corresponding generated structures and the Outer Channel Wreck Field.
- [ ] The fixed dock marker and chart legend are readable at the intended viewing distance.
- [ ] Harbourmaster, Mara, Tomas and Inez are found at their intended service locations during their day windows and move to the correct nearby off-duty anchors.
- [ ] Resident movement stays clear of buildings and props; their interaction triggers and Yarn nodes remain unchanged.
- [ ] The boat can approach, tie alongside and depart without catching on dock/table geometry.

Static project and C# syntax checks do not verify Unity semantic compilation, font availability, scene generation, mesh rendering, NPC movement, collision or camera legibility. Those remain Editor/Play Mode checks.

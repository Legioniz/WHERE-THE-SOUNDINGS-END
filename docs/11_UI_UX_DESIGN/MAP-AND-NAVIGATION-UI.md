# Map and Navigation UI

> Project: WHERE THE SOUNDINGS END  
> Status: Fixed Greyhaven dock chart implemented; live map/navigation interface not implemented  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## Current player-facing navigation aid

Greyhaven has a physical schematic chart table on the east half of the outer T-pier. Its land outline is generated from the same deterministic terrain profile as the island and labels:
- D — Main Dock.
- H — Harbourmaster / Harbour Office.
- M — Mara / Fish Market.
- T — Tomas / Boat Repair.
- I — Inez / Net Mender.
- L — Western Lighthouse.
- W — Outer Channel Wreck Field.

The chart is static. Its player marker is fixed at the dock and it is not a live minimap, calibrated nautical chart, dynamic NPC schedule display or route planner.

The dockside hub's Harbour Chart furniture action repeats the local bearings and workplace key as a readable menu reference. It does not change map state or reveal undiscovered sites.

## Current boundaries

- No live minimap, GPS marker, waypoint route, compass instrument UI or chart zoom/pan.
- No dynamic bathymetry or real-time depth chart. The Sounding Rig is a separate prototype survey instrument.
- No inter-region travel network or navigable world map.
- The six Greyhaven additive scenes are local static-area streaming, not separate regions or travel destinations.

## Future acceptance

A future navigation UI must preserve uncertainty and avoid presenting fictional depth/sonar returns as calibrated real-world navigation data. It should distinguish known landmarks, player discoveries, uncertain acoustic returns and route warnings. It must be readable at gameplay camera scale and accessible without relying on colour alone.

See GREYHAVEN-SETTLEMENT-AND-WAYFINDING.md and GREYHAVEN-DOCKSIDE-INTERACTION-HUB.md for the fixed chart and dockside location menu.

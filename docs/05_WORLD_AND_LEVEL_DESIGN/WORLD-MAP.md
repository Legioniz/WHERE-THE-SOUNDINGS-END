# World Map

> Project: WHERE THE SOUNDINGS END  
> Status: Six-region planning geography; local fog-of-war chart discovery implemented in source  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

The six-region structure is a planning scaffold, not final geometry. Greyhaven is the hub; The Shallows teach basics; Blackrock Reach introduces exposed waters; The Drowned Parish carries historical evidence; The Outer Shoals test long-range navigation; The Hollow Trench anchors the endgame. Exact inter-region distances, bearings and routes are not yet authored as a connected world coordinate system.

## Regional knowledge is earned

The player does not begin with an omniscient map. A region remains unknown until the vessel physically enters its authored survey bounds. On first entry, the region is added to the visited-region log and its chart entry is unlocked. Until then, its name, map position, marks, route and compass guidance are withheld.

The runtime chart can be opened in uncharted water, where it displays only a blank field with no location identity, marks or bearings. In a visited region it becomes a local survey plot rather than a complete world atlas. It displays the vessel position within the active region and only phenomenon sites already discovered by the Sounding Rig. Unknown sites are not represented by question marks or hidden markers. No compass rose, GPS coordinate, heading arrow or route guidance is drawn for unknown water.

The fixed chart table at Greyhaven is a separate settlement wayfinding prop showing the dock, workplaces and nearby local landmarks. It is not a live map and does not reveal offshore regions.

## Runtime boundary

The regional chart/discovery foundation is implemented in RegionalChartController, RegionalChartPolicy and GreyhavenRuntimeUI. Visits are persisted in SaveGameData schema v7; discovered site IDs remain owned by SoundingRigController. The current Outer Shoals and Hollow Trench scenes are isolated survey sandboxes, not destinations connected by sailing. A region unlock therefore records chart knowledge; it does not currently enable a travel transition.

Design around travel time, landmarks, fuel range, weather exposure, alternate routes, safe returns and discovery density once regional travel and shared world coordinates are implemented.

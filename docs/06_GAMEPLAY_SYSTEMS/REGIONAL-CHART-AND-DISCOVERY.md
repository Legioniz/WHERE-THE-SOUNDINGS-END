# Regional Chart and Discovery

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime source implemented; Unity scene/UI/save verification pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-13

## 1. Player-facing rule

The sea does not arrive pre-labelled. A region is unknown until the vessel physically enters its authored survey bounds. Until that first visit, the player receives no region name, map mark, route line, compass pointer or directional hint from the chart interface.

The chart is a record of what the crew has actually encountered, not an omniscient GPS. Unknown water is omitted rather than shown as a labelled destination. The map must never fabricate a bearing or reveal the position of an undiscovered site.

## 2. Discovery and unlock rules

1. RegionalChartController checks the vessel against active DeepOceanSurveyRegion bounds.
2. Entering a region for the first time records its normalized stable region ID in WorldSaveData.visitedRegionIds.
3. The same first visit adds the region ID to the existing ProgressSaveData.unlockedRegionIds collection. The chart entry is therefore unlocked by physical discovery, not by menu selection or a hidden global list.
4. Re-entering a visited region does not award another unlock or discovery event.
5. A saved region remains known after loading, but a region never visited remains absent from the player's chart.
6. The chart UI lists only visited region names. It does not display locked-region names, silhouettes, question-mark pins, compass bearings or route directions.

The unlock is a chart/progression record. It does not yet connect the isolated Outer Shoals and Hollow Trench sandbox scenes into a regional travel network, nor does it gate scene loading. Inter-region travel remains future work.

## 3. Map marks and fog of war

The current regional chart is a local survey plot:
- The vessel position is plotted inside the active region's authored survey radius.
- A phenomenon site is plotted only if the region has been visited and its stable site ID is already in SoundingRigController.DiscoveredSiteIds.
- An unknown or undiscovered site produces no marker, label, icon, distance, bearing or tooltip.
- A region with no recorded soundings shows a blank field plot and instructs the player to use the Sounding Rig; it does not seed placeholder marks.
- The UI has no compass rose, heading arrow, GPS coordinates, route line or directional marker for unknown content.
- The existing fixed Greyhaven dock chart remains a separate settlement wayfinding prop. It is not the open-water chart and does not reveal offshore unknown regions.

The plot uses the current DeepOceanSurveyRegion centre/radius as a local normalised display extent. It is not a surveyed nautical projection, coastline atlas, bathymetric raster or world-scale coordinate system.

## 4. Controls and modal behaviour

- Open/close the regional chart with M or the CHART [M] HUD button.
- Esc closes the chart.
- The chart pauses simulation through the same UI pause owner as the inventory and dialogue presenter.
- It cannot open over dialogue, inventory, dockside interaction, a front-end overlay, a paused state or an active gameplay action.
- Inventory and dialogue cannot open while the chart owns the modal.
- The chart does not claim the boat's Interact action or modify boat physics.

## 5. Runtime ownership

| File | Responsibility |
|---|---|
| RegionalChartPolicy.cs | Pure visit/unlock/site-reveal rules |
| RegionalChartController.cs | Region-bound detection, visit persistence, known-site filtering and local plot coordinates |
| GreyhavenRuntimeUI.cs | Chart overlay, markers, visited-region log and modal/input handling |
| DeepOceanSurveyRegion.cs | Authored region identity and survey bounds |
| DeepOceanPhenomenonSite.cs | Stable-ID authored site metadata |
| SoundingRigController.cs | Authoritative site-discovery set |
| SaveGameData.cs | Schema v7 visited-region persistence |

The chart controller lives on the persistent vessel, not on a streamed static area root. It refreshes scene-authored region/site references at a throttled interval so additive scene changes can be observed without scanning every frame.

## 6. Save compatibility

Save schema v7 adds WorldSaveData.visitedRegionIds. The v6-to-v7 migration initializes this list empty: legacy saves do not magically know which regions the player has visited. Existing ProgressSaveData.unlockedRegionIds values are preserved. A visit adds the stable ID to both collections.

The controller implements ISaveParticipant and ISaveDataValidator; invalid, blank, oversized region identifiers and oversized chart collections are rejected before state is restored. Existing site discovery remains owned by SoundingRigController and is not duplicated into the chart save record.

## 7. Tests and verification

Edit Mode coverage checks:
- unvisited regions remain unknown;
- first visit unlocks once and repeat visits are idempotent;
- a site mark requires both a visited region and a discovered site ID;
- blank IDs cannot unlock or reveal content;
- schema v6 migrates without inventing visited regions;
- schema v7 preserves visits and normalises a null collection.

Unity verification still required:
- rebuild Greyhaven and both survey sandboxes in Unity 6000.3.28f1;
- enter/leave region bounds and confirm exactly one first-visit discovery;
- open/close chart with M, button and Esc while testing all other modal states;
- discover a site with the Sounding Rig and verify only that site receives a mark;
- save/load after visits and discoveries;
- confirm unknown regions/sites never render labels, markers, bearings or compass guidance;
- review plot scaling, marker overlap, text clipping and performance at supported resolutions.

## 8. Explicitly not implemented

- Inter-region sailing, route graph, unlock quests or region travel transitions.
- World-scale map coordinates or calibrated nautical projection.
- Generated coastline, depth-contour, current, weather or hazard layers.
- Compass, GPS, minimap, automatic waypoint routing or route guidance.
- Player-authored map pins, notes, filters, zoom/pan or chart upgrades.
- Dynamic chart updates from NPC rumours, radio signals or triangulated acoustic sources.

These are future features and must not be implied by the current chart prototype.

# Regional Location Catalog

> Project: WHERE THE SOUNDINGS END  
> Status: Proposed location register — not a list of implemented or chart-discovered sites  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-14

## Purpose and status

This register collects the major places referred to by the World Bible and regional design documents. Names are working labels. A listed location is not necessarily built in Unity, connected by travel, discoverable, safe to enter or already present on the player's chart.

The runtime chart remains fog-of-war: a region is unlocked by physical entry, and only Sounding Rig-discovered sites are marked. This design catalogue does not bypass that rule.

## Greyhaven — Sheltered Bay and Estuary

| Location | Function | Status / notes |
|---|---|---|
| Greyhaven Harbour | Safe return point, moorings and boat services | Current greybox hub; full Unity verification pending |
| West Headland Lighthouse | Visual bearing and bell-history source | Greybox landmark; bell service history is proposed story content |
| Harbour Office | Channel records and Harbourmaster workplace | Generated blockout |
| Fish Market | Catch weighing, accounts and Mara's workplace | Generated blockout |
| Boat Repair Shed | Engine and hull services; Tomas's workplace | Generated blockout |
| Net Mender Shed | Gear repair and Inez's workplace | Generated blockout |
| Harbour Inn | Crew waiting, shelter and rumours | Generated blockout; interior is menu presentation |
| Western Mudflat | Estuary edge and sounding-buoy recovery lead | Opening story location; quest not yet implemented |
| Outer T-pier Chart Table | Fixed local settlement chart | Generated prop; not a live minimap |
| Outer-channel Wreck | Local salvage / environmental evidence site | Greyhaven content; story attribution remains open |

## The Shallows — Reef Flat and Seagrass Shelf

| Location | Function | Status / notes |
|---|---|---|
| The Glass Channel | Ordinary bottom and sounding baseline | Proposed |
| Ribbon Meadow | Grazing habitat and nursery water | Proposed |
| The Split Reef | Shallow navigation cut | Proposed |
| The Old Marker Line | Weathered navigation references | Proposed |
| The Outer Shelf Edge | Transition to deeper water | Proposed |

## Blackrock Reach — Exposed Basalt Coast

| Location | Function | Status / notes |
|---|---|---|
| The Split Face | Major basalt headland | Proposed |
| The Needle Stack | Offshore visual bearing | Proposed |
| The Backwash Cut | Surge / cross-current channel | Proposed |
| The Glass Cave | Acoustic reflection and coastal geology | Proposed |
| The Lee Shelf | Conditional sheltered approach | Proposed |

## The Outer Shoals — Migratory Grounds

| Location | Function | Status / notes |
|---|---|---|
| The Outer Marker | Opening-story navigation boundary | Position/design not final |
| The Long Ridge | Current and migratory-life concentration | Proposed |
| The Blue Gut | Deeper channel between shoals | Proposed |
| The Drift Line | Surface debris and migration corridor | Proposed |
| The Quiet Reach | Long open-water navigation test | Proposed |

## The Drowned Parish — Submerged Settlement

| Location | Function | Status / notes |
|---|---|---|
| The Old Landing | Former lower-settlement landing | Proposed |
| The Bell House | Historic public structure in local accounts | Existence and use require corroboration |
| The Sunken Lane | Partly buried settlement route | Proposed |
| The Record Room | Historical evidence source | Proposed; do not place all answers in one room |
| The Upper Steps | Path from old lowland toward higher ground | Proposed |

## The Hollow Trench — Deep-water Margin

| Location | Function | Status / notes |
|---|---|---|
| The Upper Slope | Transition to deep canyon margin | Survey sandbox context |
| The Seep Field | Chemosynthetic habitat / inspection | Authored evidence-site prototype |
| The Brine Boundary | Dense saline water feature | Authored evidence-site prototype |
| The Fall Site | Whale-fall succession evidence | Authored evidence-site prototype |
| The Last Reliable Return | Endgame acoustic boundary | Proposed narrative location |

## Authoring rules

- Give every implemented location a stable `locationId` before it affects saves, charts, quests or contracts.
- Keep display names separate from IDs so names can change without breaking persistence.
- A discovered location must be physically visited or revealed through the existing authored discovery rule.
- Do not add unknown locations to charts as question marks, compass hints or hidden bearings.
- Every hazardous location needs approach conditions, warning, counterplay and a viable retreat rule.
- Every story-critical record needs more than one acquisition path where practical.
- Distinguish a physical site, a rumour about a site and a named location in a design document.

## Related documents

- [World Map](WORLD-MAP.md)
- [Exploration Design](EXPLORATION-DESIGN.md)
- [World Bible](../../02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [Greyhaven](GREYHAVEN.md)

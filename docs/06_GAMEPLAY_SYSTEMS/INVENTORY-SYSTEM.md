# Spatial Cargo and Inventory System

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime spatial hold, starter gear, fishing catches, salvage and impact damage implemented; economy integration pending  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## Design intent

The boat's hold is a physical planning space, not a weight counter. Fish, shellfish, salvage and selected quest cargo occupy authored cell shapes. The player chooses where to place, rotate, move and remove each catch. Cargo capacity therefore creates route, risk and profit decisions during a voyage.

The system takes inspiration from the broader spatial-inventory design space described by DREDGE developer Joel Mason, but uses project-specific shapes, rules, UI, tuning and progression. No DREDGE code, assets, item layouts or text are copied.

## Baseline hold

- Starter hold: 7 columns × 9 rows.
- Each item definition contains a stable item ID and a list of occupied cells.
- Rotation uses 90-degree quarter turns, normalizes the rotated shape to its minimum x/y, then validates the target cells.
- A placement is rejected if any cell is outside the hold, blocked, or already occupied.
- Runtime item instances have unique IDs; item definitions use stable content IDs.
- Quantity, quality and freshness are stored per placed stack.
- Hold upgrades are authored progression. A save cannot silently resize a live hold.
- Engine, handline rod and deck lantern are installed as equipment placements in the same grid as cargo. They consume physical cells and can be destroyed by a damaging impact.
- A collision can permanently block a specific cell and destroy the non-protected item occupying it. The destroyed placement is not silently moved elsewhere.
- Quest-critical cargo and definitions marked non-damageable are protected from cell-damage destruction.
- A damaged cell is persisted in save data and remains blocked until repaired through the vessel repair API.

## Runtime implementation

Located in `Assets/_Project/Scripts/Runtime/Inventory/`:

- `CargoItemDefinition`: ScriptableObject defining stable ID, category, shape and handling metadata.
- `CargoGridGeometry`: deterministic shape rotation, normalization, blocked-cell map and placement validation.
- `SpatialCargoHold`: runtime placements, fit checks, move/remove/place operations, physical damaged-cell mask, item destruction on impact, repair and `ISaveParticipant` capture/restore.
- `CargoItemDefinition`: equipment and aberrant-catch metadata, including an aberrant sale-value multiplier.
- `VesselConditionController`: collision severity, hull/engine condition and impact-to-grid-cell mapping.
- `TryPlaceFirstFit`: explicit acquisition helper that searches for the first valid location and never rearranges existing cargo.
- Query methods expose item definitions, occupied cells, blocked cells and used-cell count to the UI without making the UI an owner of cargo state.

## Player interface

`GreyhavenRuntimeUI` builds the first runtime UI Toolkit interface in the generated Greyhaven scene.

- Press **I** or the on-screen CARGO HOLD control to open the hold; **Esc** closes it.
- The 7×9 hold is rendered as clickable cells. Each item occupies the exact cells defined by its ScriptableObject shape.
- Click any cell belonging to an item to select it. Click an empty, unblocked cell to move the selected item.
- Press **R** or ROTATE to preview the next 90-degree orientation. Click an empty cell to commit the rotation and move. An invalid placement leaves the original placement unchanged.
- REMOVE deletes selected ordinary cargo. Quest-critical cargo and installed equipment cannot be discarded from the player interface.
- Damaged cells show a cross and cannot accept placements. An item hit by a damaged cell is destroyed as a whole, including every occupied cell it used.
- The selection readout displays quantity, quality, freshness and rotation. The header reports occupied cells.
- The hold pauses the prototype simulation while open. This cadence is provisional and must be play-tested.

The catch-intake column remains as **prototype test intake controls** for layout verification. Actual fishing now runs through `HarvestingMinigameController` and inserts the selected catch through `TryPlaceFirstFit`. Dredging at the Greyhaven wreck field yields timber, canvas or corroded fittings. Aberrant fish have distinct item IDs and elevated sale-value multipliers.

## Save integration

The versioned save DTO stores hold dimensions and placement records:

- instance ID
- stable item ID
- anchor x/y
- rotation quarter-turns
- quantity
- quality
- freshness
- damaged-cell coordinates (persisted separately from cargo placements)

The live hold's dimensions remain authoritative during restore. Unknown item IDs and placements that no longer fit are skipped with a warning rather than corrupting the whole save. Manual slots and the dedicated autosave both capture the same `SpatialCargoHold` participant.

## Remaining work

- Drag-and-drop, controller focus navigation, item hover details and accessibility scaling are not complete. Current placement uses click-select then click-destination, with keyboard rotation.
- Sale transactions, freshness-decay ticking, market demand and regional pricing are not connected yet; aberrant value multipliers are metadata only.
- Equipment is physically shared with cargo, but equipment activation, upgrade replacement and repair-shop transactions remain future work.
- Damage repair has a runtime API but no repair-shop UI or material cost transaction yet.
- Grid geometry and damage/save migration tests are present in Edit Mode, but Unity execution, collision tuning and Play Mode interaction still require validation in Unity 6000.3.28f1.

## Reference

Joel Mason, Black Salt Games, “Deep Dive: The surprising depth of spatial inventories in Dredge,” Game Developer, 2 March 2022: https://www.gamedeveloper.com/design/deep-dive-the-surprising-depth-of-spatial-inventories-in-dredge

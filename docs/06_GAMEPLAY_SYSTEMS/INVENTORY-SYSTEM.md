# Spatial Cargo and Inventory System

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime grid and first player interface implemented; fishing/economy integration pending  
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
- Damaged or inaccessible cells are represented by the blocked-cell mask, not by deleting items.
- Quest-critical items must have explicit acceptance/decline handling before they are granted; do not destroy player cargo to make room without a clear player decision.

## Runtime implementation

Located in `Assets/_Project/Scripts/Runtime/Inventory/`:

- `CargoItemDefinition`: ScriptableObject defining stable ID, category, shape and handling metadata.
- `CargoGridGeometry`: deterministic shape rotation, normalization, blocked-cell map and placement validation.
- `SpatialCargoHold`: runtime placements, fit checks, move/remove/place operations and `ISaveParticipant` capture/restore.
- `TryPlaceFirstFit`: explicit acquisition helper that searches for the first valid location and never rearranges existing cargo.
- Query methods expose item definitions, occupied cells, blocked cells and used-cell count to the UI without making the UI an owner of cargo state.

## Player interface

`GreyhavenRuntimeUI` builds the first runtime UI Toolkit interface in the generated Greyhaven scene.

- Press **I** or the on-screen CARGO HOLD control to open the hold; **Esc** closes it.
- The 7×9 hold is rendered as clickable cells. Each item occupies the exact cells defined by its ScriptableObject shape.
- Click any cell belonging to an item to select it. Click an empty, unblocked cell to move the selected item.
- Press **R** or ROTATE to preview the next 90-degree orientation. Click an empty cell to commit the rotation and move. An invalid placement leaves the original placement unchanged.
- REMOVE deletes the selected non-quest item. Quest-critical definitions are protected from accidental discard.
- The selection readout displays quantity, quality, freshness and rotation. The header reports occupied cells.
- The hold pauses the prototype simulation while open. This cadence is provisional and must be play-tested.

The catch-intake column exposes the Greyhaven catalogue as **prototype test intake controls**. Each control adds one item through `TryPlaceFirstFit` at test quality/freshness values. It exists to exercise the real grid and persistence model before fishing output is connected; it is not a fishing mechanic and should be replaced by actual catch results when fishing is implemented.

## Save integration

The versioned save DTO stores hold dimensions and placement records:

- instance ID
- stable item ID
- anchor x/y
- rotation quarter-turns
- quantity
- quality
- freshness

The live hold's dimensions remain authoritative during restore. Unknown item IDs and placements that no longer fit are skipped with a warning rather than corrupting the whole save. Manual slots and the dedicated autosave both capture the same `SpatialCargoHold` participant.

## Remaining work

- Real fishing results must create cargo through the authoritative hold API; the prototype intake buttons are temporary.
- Drag-and-drop, controller focus navigation, item hover details and accessibility scaling are not complete. Current placement uses click-select then click-destination, with keyboard rotation.
- No sell/buy transaction, freshness-decay tick or market integration is wired yet.
- No equipment mount slots, hull damage, cargo loss or storage upgrade gameplay is implemented.
- Grid geometry tests are present in Edit Mode, but Unity execution and Play Mode interaction still require validation in Unity 6000.3.28f1.

## Reference

Joel Mason, Black Salt Games, “Deep Dive: The surprising depth of spatial inventories in Dredge,” Game Developer, 2 March 2022: https://www.gamedeveloper.com/design/deep-dive-the-surprising-depth-of-spatial-inventories-in-dredge

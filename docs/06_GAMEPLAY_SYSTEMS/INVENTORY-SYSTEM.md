# Spatial Cargo and Inventory System

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime grid foundation implemented; authoring and player UI in progress  
> Owner: Project lead  
> Version: 0.2  
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

## Gameplay implications

- Fish shapes should communicate approximate body length and handling, not just price.
- Large or awkward catches create opportunity cost and can force the player to return early.
- Freshness is time-sensitive and belongs to the catch instance/stack, not to a global species counter.
- Equipment uses dedicated mounting slots in a later equipment pass; it should not be mixed into the fish-placement API without explicit slot rules.
- Hull damage may disable cells or dislodge cargo only after the damage model is implemented and communicated.
- Auto-place may be offered as an accessibility/quick action, but must be opt-in and must never silently rearrange the player's existing hold.
- Inventory pause cadence must be tested. Catch acquisition should not interrupt every few seconds with a mandatory packing screen.

## Runtime implementation

Located in `Assets/_Project/Scripts/Runtime/Inventory/`:

- `CargoItemDefinition`: ScriptableObject defining stable ID, category, shape and handling metadata.
- `CargoGridGeometry`: deterministic shape rotation, normalization, blocked-cell map and placement validation.
- `SpatialCargoHold`: runtime placements, fit checks, move/remove/place operations and `ISaveParticipant` capture/restore.

The grid logic is deliberately separated from presentation. A future UI can render the exact same placement model with mouse, keyboard and controller input.

## Save integration

The versioned save DTO stores hold dimensions and placement records:

- instance ID
- stable item ID
- anchor x/y
- rotation quarter-turns
- quantity
- quality
- freshness

The live hold's dimensions remain authoritative during restore. Unknown item IDs and placements that no longer fit are skipped with a warning rather than corrupting the whole save.

## Current limitations

- No inventory screen, drag/drop interaction, item hover preview, rotation key binding or controller navigation yet.
- No content item assets are authored yet; item definitions must be created through the Cargo Item asset menu and added to the hold's catalogue.
- No fishing system currently creates cargo placements.
- No sell/buy transaction or freshness-decay tick is wired yet.
- No equipment mount slots, hull damage, cargo loss or storage upgrade gameplay is implemented.
- Grid geometry has been implemented; it still needs Unity Edit Mode tests and in-editor validation.

## Reference

Joel Mason, Black Salt Games, “Deep Dive: The surprising depth of spatial inventories in Dredge,” Game Developer, 2 March 2022: https://www.gamedeveloper.com/design/deep-dive-the-surprising-depth-of-spatial-inventories-in-dredge

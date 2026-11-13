# Inventory Interface

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime UI Toolkit hold interface connected to harvesting and vessel damage; controller/accessibility pass pending  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## Purpose

The cargo interface is a functional view over the authoritative `SpatialCargoHold`. It does not keep a separate UI-only inventory model. Placement, movement, rotation, fit validation, removal and save state are all applied to the hold component.

## Layout

The interface has three principal areas:

1. **Hud strip:** game/field-log identity and the CARGO HOLD toggle.
2. **Hold grid:** a 7-column × 9-row cell layout. Occupied cells show a compact item mark; blocked cells are visibly distinct; the selected item is highlighted.
3. **Cargo details and intake:** used-cell count, selected item's quantity/quality/freshness/rotation, rotate/remove actions, and a prototype intake list. Fishing and dredging use separate in-world timed prompts.

The inventory is built with Unity UI Toolkit through `UIDocument` and a generated `PanelSettings` asset. The project targets Unity 6, where UI Toolkit handles runtime pointer input without adding a separate uGUI EventSystem.

## Controls

| Input | Action |
|---|---|
| I | Toggle hold |
| Esc | Close hold |
| Click occupied cell | Select the cargo instance occupying that cell |
| Click empty cell | Move selected item to that anchor |
| R | Preview next 90-degree rotation |
| ROTATE | Same as R |
| REMOVE | Remove selected ordinary cargo; installed gear and quest cargo are protected |
| CARGO HOLD button | Toggle hold |

Rotation is applied only when the player clicks a valid destination. The grid model validates every occupied cell. A rejected move does not mutate the existing placement. Quest-critical cargo and installed equipment cannot be removed through this interface. Impact-damaged cells show a cross; cargo destroyed by an impact is removed from the hold and the cell remains blocked.

## Prototype catch intake

The catalogue panel remains explicitly labelled as prototype test intake. The real harvesting loop is separate: F starts a three-beat fishing timing challenge, while G starts a four-beat salvage challenge only when stopped near the Greyhaven wreck field. Results are placed through `SpatialCargoHold.TryPlaceFirstFit`.

## Pause and input

Opening the hold pauses scaled gameplay time so the boat and action-driven clock stop while cargo is arranged. Dialogue also freezes sea time. The hold cannot open over the dockside hub, front-end overlays or dialogue; the pause overlay cannot open while the hold is active. Outside these pause states, the world clock advances only while the boat is moving or a harvest/survey action is active.

## Current limitations

- Click-select/click-destination is implemented; drag-and-drop previews are not.
- No controller navigation/focus scheme has been tuned.
- No responsive small-screen, text scaling, colour-blind or full remapping/accessibility review has been completed.
- Sale transactions, freshness decay and equipment activation/upgrade transactions are not connected. Equipment placements already occupy the same cargo grid.
- Must be tested in Unity Editor and a Windows build; repository static validation alone does not prove runtime UI interaction.

## Related systems

- `docs/06_GAMEPLAY_SYSTEMS/INVENTORY-SYSTEM.md`
- `Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`

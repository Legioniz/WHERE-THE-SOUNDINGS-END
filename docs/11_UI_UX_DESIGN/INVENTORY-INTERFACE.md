# Inventory Interface

> Project: WHERE THE SOUNDINGS END  
> Status: First runtime UI Toolkit interface implemented; controller and accessibility pass pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Purpose

The cargo interface is a functional view over the authoritative `SpatialCargoHold`. It does not keep a separate UI-only inventory model. Placement, movement, rotation, fit validation, removal and save state are all applied to the hold component.

## Layout

The interface has three principal areas:

1. **Hud strip:** game/field-log identity and the CARGO HOLD toggle.
2. **Hold grid:** a 7-column × 9-row cell layout. Occupied cells show a compact item mark; blocked cells are visibly distinct; the selected item is highlighted.
3. **Cargo details and intake:** used-cell count, selected item's quantity/quality/freshness/rotation, rotate/remove actions, and an explicit prototype catch-intake list.

The inventory is built with Unity UI Toolkit through `UIDocument` and a generated `PanelSettings` asset. The generated scene also receives an Input System UI module and EventSystem for pointer interactions.

## Controls

| Input | Action |
|---|---|
| I | Toggle hold |
| Esc | Close hold |
| Click occupied cell | Select the cargo instance occupying that cell |
| Click empty cell | Move selected item to that anchor |
| R | Preview next 90-degree rotation |
| ROTATE | Same as R |
| REMOVE | Remove selected non-quest cargo |
| CARGO HOLD button | Toggle hold |

Rotation is applied only when the player clicks a valid destination. The grid model validates every occupied cell. A rejected move does not mutate the existing placement. Quest-critical cargo cannot be removed through this interface.

## Prototype catch intake

The catalogue panel is explicitly labelled as prototype test intake. Each item button calls `SpatialCargoHold.TryPlaceFirstFit` with one unit and test quality/freshness values. This proves item definitions, shape placement and hold feedback before a fishing-result system exists. The controls are not part of the intended final player-facing interface.

## Pause and input

Opening the hold pauses scaled gameplay time so the player can arrange cargo without the boat simulation continuing. UI input remains active through the Input System UI module. The inventory cannot be opened during an active conversation.

## Current limitations

- Click-select/click-destination is implemented; drag-and-drop previews are not.
- No controller navigation/focus scheme has been tuned.
- No responsive small-screen, text scaling, colour-blind or full remapping/accessibility review has been completed.
- Fishing, sale transactions, freshness decay, equipment slots and hold upgrades are not connected.
- Must be tested in Unity Editor and a Windows build; repository static validation alone does not prove runtime UI interaction.

## Related systems

- `docs/06_GAMEPLAY_SYSTEMS/INVENTORY-SYSTEM.md`
- `Assets/_Project/Scripts/Runtime/Inventory/SpatialCargoHold.cs`
- `Assets/_Project/Scripts/Runtime/Narrative/GreyhavenRuntimeUI.cs`

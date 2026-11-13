# Player Controls

> Project: WHERE THE SOUNDINGS END  
> Status: Shared Input System boat controls and keyboard/gamepad bindings implemented; remapping and full controller navigation pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## Current bindings

The Boat action map in Assets/_Project/Settings/Input/PlayerControls.inputactions is the shared input source. The README lists the current keyboard and gamepad bindings:

| Action | Keyboard | Gamepad |
|---|---|---|
| Move / steer boat | W A S D | Left stick |
| Interact / open dockside location / inspect site | E | South face button |
| Start fishing | F | West face button |
| Start dredging | G | Right trigger |
| Sounding pulse | Q | Right shoulder |
| Harvest timing beat | Space / Enter | East face button |
| Pause / close dockside hub | Esc | Start |
| Inventory | I | North face button |

## Context rules

- Dockside location access requires a vessel inside the authored horizontal shore radius and at or below 0.85 m/s.
- Interact opens the nearest eligible location; furniture and resident choices are then selected with UI pointer controls.
- Esc / Start closes the dockside hub. The same input must not also open the pause overlay.
- Inventory, dialogue, dockside and front-end pause/save/settings panels are mutually exclusive.
- Fishing, dredging and sounding use the shared action map and their existing action gates. A dockside menu cannot open while a gameplay action is active.
- Dialogue lines advance with Space/Enter; choices use click or keys 1–5.
- Inventory uses I to toggle, Esc to close, R to preview rotation, and pointer selection for cargo movement.

## Ownership and remaining work

BoatPrototypeController, HarvestingMinigameController, SoundingRigController, GreyhavenRuntimeUI, GreyhavenFrontEndUI and DocksideHubController consume the shared Input System actions. Generated Greyhaven resident triggers are externally managed by the dockside hub and must not independently enable/disable the shared Interact action.

Full rebinding UI, controller focus/navigation through all UI Toolkit panels, accessibility remapping and input-device switching prompts are not complete. Verify simultaneous key presses and UI transitions in Unity 6000.3.28f1.

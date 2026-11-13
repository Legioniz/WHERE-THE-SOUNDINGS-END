# UI and UX System Overview

> Project: WHERE THE SOUNDINGS END  
> Status: Connected runtime UI Toolkit prototype; visual, controller and accessibility review pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## Runtime surfaces

| Surface | Runtime owner | Current scope |
|---|---|---|
| Title / pause / save / settings | GreyhavenFrontEndUI | Title, continue, five manual slots, save/load/delete, pause, settings |
| Field HUD | GreyhavenRuntimeUI | Field-log header, cargo toggle, contextual interaction prompt |
| Cargo hold | GreyhavenRuntimeUI + SpatialCargoHold | 7×9 grid, placement, move, rotate, remove, damage display, prototype intake |
| Dialogue | GreyhavenRuntimeUI + Yarn Spinner | Resident lines, choices, continue, relationship tier/meter |
| Dockside location hub | DocksideHubController | Seven locations, interior descriptions, furniture actions, resident handoff, checkpoint |
| Fixed harbour chart | Generated physical prop + dockside menu text | Schematic Greyhaven bearings and workplace key |

## Design direction

The current UI uses a restrained maritime field-journal treatment: dark blue-green surfaces, muted paper text, weathered copper accents, compact labels and clear functional hierarchy. These are working runtime styles, not final visual assets or a locked design-token system.

## Modal and time ownership

Dockside, inventory, dialogue and front-end overlays must not overlap. DocksideModalPolicy is the shared deterministic gate for opening these panels. Each pause owner captures and restores the prior time scale. The dockside hub closes with Esc / gamepad Start; the pause action is ignored while dockside, inventory or dialogue owns the interaction.

## Product boundaries

The interface is a connected prototype, not a complete shipping UI. The live map, market transaction screen, repair purchase flow, full journal/quest UI, controller focus navigation, input remapping, comprehensive accessibility settings and responsive layout certification are not complete. The map/chart currently consists of a fixed physical chart and a dockside text reference.

## Verification

Run the generated Greyhaven scene in Unity 6000.3.28f1. Check panel sorting, 16:9 and 16:10 layout, small-window clipping, input focus, keyboard/gamepad modal transitions, save/load feedback, Yarn choice flow and accessibility. Static source checks do not establish these results.

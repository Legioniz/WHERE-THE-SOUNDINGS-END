# Runtime HUD and Modal Ownership

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime UI Toolkit HUD, inventory, dialogue and fog-of-war chart implemented in source; Unity layout/accessibility verification pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## HUD

GreyhavenRuntimeUI builds the starter Greyhaven HUD with Unity UI Toolkit:
- A compact top strip labelled GREYHAVEN / FIELD LOG.
- A Chart [M] toggle that opens the regional field chart.
- A Cargo Hold [I] toggle.
- A shared interaction prompt beneath the strip. The dockside hub owns the generated Greyhaven location prompt; hand-authored resident/site interactions can use the same presenter API.
- Contextual status and feedback within inventory and dialogue panels.

The chart overlay shows only physically visited regions and Sounding Rig-discovered site marks. Unknown regions and sites receive no names, pins, bearings or compass guidance. It uses a local survey plot, not a calibrated nautical map.

The HUD is a functional greybox. It is not a final production HUD or a complete expedition instrument suite.

## Dockside hub overlay

DocksideHubController owns a separate UIDocument with a higher sorting order. It presents seven location menus, room descriptions and furniture actions while the player remains aboard a stationary vessel. The hub requires horizontal speed at or below 0.85 m/s and an authored shore-approach radius.

The hub's UI includes a location list, location title/subtitle, interior description, furniture list, selected-object detail/status and a resident dialogue button where a resident is assigned. The Inn Guest Ledger requests an existing autosave checkpoint. Market and repair inspection are read-only; the UI does not imply sales, paid repairs, net crafting, sleep or physical interior traversal.

## Dialogue overlay

The Yarn presenter shows speaker, line text, choice buttons, a continue action and the active resident's relationship tier/0–100 meter. It uses the current NpcAgent as the source of truth for trust and conversation eligibility.

## Inventory overlay

The hold interface shows a 7×9 cargo grid, blocked/damaged cells, selected item details, rotation/removal actions and a prototype catch-intake catalogue. The prototype intake is a test tool, not the real fishing/dredging loop.

## Modal ownership contract

Only one of the following gameplay-facing modal states may own the screen at a time:
1. Dockside location/furniture hub.
2. Cargo inventory.
3. Yarn dialogue.
4. Front-end title/pause/save/settings overlay.

DocksideModalPolicy supplies deterministic guards used by the runtime UI:
- Dockside cannot open over inventory, dialogue, an active gameplay action, a front-end overlay or a paused time scale.
- Inventory cannot open over dockside, dialogue, the regional chart or front-end overlays.
- The regional chart cannot open over inventory, dialogue, dockside, front-end overlays, a paused state or an active gameplay action.
- Pause cannot open over dockside, inventory or dialogue.
- Esc / Start closes the active dockside menu; it must not also open the pause page.

Each pause owner restores only the time scale it captured. The UI must never restore gameplay while another modal still owns the pause.

## Verification and accessibility

Unity Play Mode review is still required for 16:9 and 16:10 resolutions, minimum supported window size, safe-area/layout overflow, keyboard and gamepad focus, UI sorting, text scaling, colour contrast, and modal transitions. Full controller navigation, remappable controls, reduced-motion integration and comprehensive accessibility settings remain incomplete.

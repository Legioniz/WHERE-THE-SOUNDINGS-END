# Main Menu, Pause and Save UI

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime UI Toolkit front end implemented; Unity visual and input verification pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-13

## Runtime ownership

GreyhavenFrontEndUI is attached beside GreyhavenRuntimeUI on the Greyhaven dialogue/UI object. It adds a title/pause/save/settings overlay to the existing gameplay UIDocument. It owns the front-end pause state; it does not own campaign data or file operations.

## Title screen

The generated title page provides:
- **Begin / Continue** — load the newest occupied manual slot; if none exists, try the separate autosave. If neither exists, continue into a new voyage.
- **Load Field Record** — open the five manual save slots.
- **Settings** — fullscreen, interface scale, audio preferences and accessibility preferences.
- **Quit to Desktop** — call Application.Quit.

## Pause screen

The in-game pause page provides:
- Resume Voyage.
- Save / Load Field Record.
- Settings.
- Return to Title.

The pause overlay freezes scaled gameplay time and restores the prior time scale when resumed. The title, pause, save and settings pages remain modal; dockside and inventory interactions cannot open behind them.

## Save/load screen

Five manual slots are presented independently. Occupied slots provide Load, Save/overwrite and Delete; empty slots provide Save Here. Delete requires an explicit confirmation page. Save/load actions display the result returned by SaveGameCoordinator, including backup-recovery information where available.

The dedicated autosave remains separate from the five manual slots. Continue can fall back to autosave, and the Harbour Inn Guest Ledger can request a checkpoint through the existing AutosaveController. The current slot list is not a cross-scene restore browser: SaveGameCoordinator still requires the save's target scene to be active.

## Settings

Implemented controls:
- Fullscreen toggle.
- Interface scale slider.
- Master volume slider (AudioListener.volume).

Preference-only controls, not yet connected to runtime systems:
- Music preference.
- Effects preference.
- Reduce camera shake.
- High contrast.

Preferences are stored in PlayerPrefs, separately from campaign save files. The settings panel explicitly reports that only fullscreen, master volume and interface scale currently have direct runtime effects.

## Input and modal rules

- Keyboard Esc / gamepad Start toggles the pause page while gameplay is active.
- The pause action is ignored while dockside, inventory or dialogue UI owns the interaction.
- The dockside menu consumes Esc / Start to close itself; it must not also open the pause page in the same input frame.
- Save/settings subpages return to the pause page through Esc / Start.
- Do not let two overlays independently capture and restore Time.timeScale.

## Verification still required

Run the title → continue/new game → pause → save/load/delete/settings → resume flow in Unity 6000.3.28f1. Test empty/full slots, corrupt-primary backup recovery, autosave fallback, controller Start input, resolution scaling, and modal transitions. Static checks do not prove the rendered UI or save flow.

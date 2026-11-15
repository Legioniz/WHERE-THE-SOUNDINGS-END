# Unity Playable Validation

> Engine: Unity 6.3 LTS — 6000.3.28f1  
> Platform: Windows 64-bit  
> Status: Validation path implemented; Unity execution and manual player verification pending

## Fresh checkout

1. Install Unity Hub and Unity Editor 6000.3.28f1. Include Windows Build Support (Mono).
2. Clone the repository and open its root folder in Unity Hub.
3. Allow the package resolver and asset import to finish. On a fresh checkout without `Greyhaven_Blockout.unity`, the Editor first-open hook creates the project rendering settings, Greyhaven blockout, six additive Greyhaven area scenes and two survey sandboxes.
4. Open `Assets/_Project/Scenes/Greyhaven_Blockout.unity`. The vessel and core systems should be present. Press Play.
5. If bootstrap was interrupted, run **Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation**. Existing stale generated scenes continue to require an explicit rebuild confirmation in the normal Editor.

Do not create fake Unity scene, package-lock or meta files by hand. Unity owns their serialization and GUID generation.

## Licensed automated validation

The repository contains a manually dispatched workflow at **Actions → Unity verification → Run workflow**. Before the first run, add a repository Actions secret named `UNITY_LICENSE` containing a valid Unity license file/activation payload. Never place the license in the repository, workflow arguments, comments or logs.

The workflow runs all Edit Mode and Play Mode tests using the pinned Unity version. Only if that job succeeds does it create and upload a Windows 64-bit Development Build. Test reports and the player artifact are retained as GitHub Actions artifacts.

The Windows build method calls the unattended foundation setup, checks that the persistent Greyhaven scene, all six streamed areas, and both survey test scenes are enabled in Build Settings, and fails when a required scene is missing or Unity's BuildPipeline does not report success.

## Required acceptance checks

- Unity opens the repository without package-resolution failures or C# compilation errors.
- The Greyhaven startup smoke test finds the physics-driven vessel, assigned Input System asset, hydrodynamics, cargo hold, harvesting, Sounding Rig, economy components, Main Camera follow target, sea-state director, save/autosave services, and area streamer.
- All Edit Mode tests pass.
- The additive-area Play Mode test preloads terrain/harbour/village, loads the lighthouse before approach, unloads it after leaving, and retains the persistent gameplay scene without failed transitions.
- A Windows Development Build completes and includes all enabled scenes.
- The downloaded build is extracted to a clean folder on Windows and launches without a missing-scene, missing-package, shader or input error.
- A hands-on expedition confirms steering/throttle, shore access, NPC/dockside interactions, fishing, dredging, Sounding Rig, chart, sales/purchases/repairs/refuelling, manual save/load, autosave and restart recovery.

## Verification language

A passing static validator or C# syntax parser is not a Unity compile. A passing Unity test run is not a Windows build. A successful Windows build is not a manual playtest. Only claim each gate after its corresponding evidence exists.

The present implementation is a connected prototype; full campaign progression, creature AI/population simulation, walkable on-foot interiors, inter-region travel and non-Greyhaven physical markets remain outside this playable Greyhaven prototype gate.

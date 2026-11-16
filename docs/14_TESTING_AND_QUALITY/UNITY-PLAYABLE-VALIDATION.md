# Unity Playable Validation

> Engine: Unity 6.3 LTS — 6000.3.28f1  
> Platform: Windows 64-bit  
> Status: Validation path implemented; Unity execution and manual player verification pending

## Fresh checkout

1. Install Unity Hub and Unity Editor 6000.3.28f1. Include Windows Build Support (Mono).
2. Clone the repository and open its root folder in Unity Hub.
3. Allow the package resolver and asset import to finish. On a fresh checkout without `Greyhaven_Blockout.unity`, an `AssetPostprocessor.OnPostprocessAllAssets` callback detects the completed import and schedules the bootstrap on the Editor main thread. It then creates the project rendering settings, Greyhaven blockout, six additive Greyhaven area scenes and two survey sandboxes. This avoids loading the Input Actions asset from an `InitializeOnLoad` callback before Unity has imported it.
4. Open `Assets/_Project/Scenes/Greyhaven_Blockout.unity`. The vessel and core systems should be present. Press Play.
5. If bootstrap was interrupted, run **Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation**. Existing stale generated scenes continue to require an explicit rebuild confirmation in the normal Editor.

Do not create fake Unity scene, package-lock or meta files by hand. Unity owns their serialization and GUID generation.

## Self-hosted Unity verification

The manual GitHub Actions workflow is **Actions → Unity verification → Run workflow**. It uses a Windows self-hosted runner, not a hosted cloud runner, so the Unity license remains local and no repository secret is passed to the workflow.

Configure a dedicated Windows machine (your development PC is suitable) with:

- GitHub Actions self-hosted runner installed for this repository.
- Runner labels: `self-hosted`, `Windows`, `X64`, and `unity-6000.3.28f1`.
- Unity Editor **6000.3.28f1** installed through Unity Hub with Windows Build Support (Mono).
- Unity Editor activated and licensed locally for the Windows account that runs the GitHub Actions runner service.
- The Unity Editor's `Editor` folder added to the runner account's `PATH`, so PowerShell can resolve `Unity.exe`.
- Enough free disk space for a clean project import, test logs and Windows build output.

For a runner registered as a Windows service, verify the service runs under the same Windows account that owns the activated Unity Editor license and has access to the Unity installation. Do not export, commit or upload the local license file. Only dispatch this workflow against trusted `main` code; do not use an always-on self-hosted runner for untrusted pull-request code.

The workflow performs these steps in order:

1. Initialize the generated Greyhaven and survey scenes in unattended Editor mode.
2. Build local Addressables content for the generated runtime prefabs.
3. Run all Edit Mode tests.
4. Run all Play Mode tests, including the startup smoke test, Addressables pool lifecycle test and additive-area transition test.
5. Build a Windows 64-bit Development Player after both test suites pass; the build method rebuilds Addressables content before compiling the Player.
6. Upload test reports, logs and the Windows build as GitHub Actions artifacts.

Before a Windows build, `ProjectReferenceIntegrityValidator` now performs a Unity Editor preflight. It verifies all nine required scenes are imported and enabled, opens every generated scene, rejects missing MonoBehaviour scripts and unresolved project asset dependencies, validates all six streamed-area roots and their content revisions, and checks the persistent gameplay graph. The graph check confirms the vessel's Input Actions asset and required physics, cargo, fishing, survey, economy, chart, camera-follow, save/autosave, dockside and area-streaming components are connected. The Windows build still fails if Unity's BuildPipeline does not report success. Run **Tools > WHERE THE SOUNDINGS END > Validate Project References** to execute this preflight manually after initialization.

## Required acceptance checks

- Unity opens the repository without package-resolution failures or C# compilation errors. The assembly-definition graph resolves Runtime, Editor, Edit Mode and Play Mode references; the Yarn project source glob resolves to at least one source file.
- **Tools > WHERE THE SOUNDINGS END > Validate Project References** passes, with no missing MonoBehaviour scripts or unresolved project asset dependencies across the nine generated scenes. It also confirms both generated transient prefabs are inactive, registered under their expected Addressables keys, and contain the required visual controller.
- Addressables content builds successfully. The Play Mode pool lifecycle test loads `runtime.panic.shadow`, rents and returns the same instance, enforces the pool cap, rejects a double return, and releases the pool only after all instances are returned.
- The Greyhaven startup smoke test finds the physics-driven vessel, assigned Input System asset, hydrodynamics, cargo hold, harvesting, Sounding Rig, economy components, Main Camera follow target, sea-state director, save/autosave services, and area streamer.
- All Edit Mode tests pass.
- The additive-area Play Mode test preloads terrain/harbour/village, loads the lighthouse before approach, unloads it after leaving, and retains the persistent gameplay scene without failed transitions.
- A Windows Development Build completes and includes all enabled scenes.
- The downloaded build is extracted to a clean folder on Windows and launches without a missing-scene, missing-package, shader or input error.
- A hands-on expedition confirms steering/throttle, shore access, NPC/dockside interactions, fishing, dredging, Sounding Rig, chart, sales/purchases/repairs/refuelling, manual save/load, autosave and restart recovery.

## Verification language

A passing static validator or C# syntax parser is not a Unity compile. A passing Unity test run is not a Windows build. A successful Windows build is not a manual playtest. Only claim each gate after its corresponding evidence exists.

The present implementation is a connected prototype; full campaign progression, creature AI/population simulation, walkable on-foot interiors, inter-region travel and non-Greyhaven physical markets remain outside this playable Greyhaven prototype gate.

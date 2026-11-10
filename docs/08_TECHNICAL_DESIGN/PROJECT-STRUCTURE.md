# Project Structure

> Project: WHERE THE SOUNDINGS END  
> Status: Implemented scaffold; expandable baseline  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-10

## Repository root

- Assets/: Unity-owned game assets and C# assemblies.
- Packages/manifest.json: pinned Unity package dependencies; Unity generates packages-lock.json.
- ProjectSettings/ProjectVersion.txt: pinned Unity Editor version.
- docs/: authoritative game design, world, narrative, technical, production, and research documents.
- tools/: offline validation and simulation utilities.
- .github/workflows/: repository integrity automation.

## Unity asset layout

Assets/_Project is the game's first-party content boundary.

| Directory | Responsibility |
|---|---|
| Art/ | Models, textures, materials, and other first-party art |
| Audio/ | Ambience, music, sound effects, and voice |
| Data/ | ScriptableObject definitions for creatures, items, regions, and quests |
| Prefabs/ | Reusable boat, creature, world, and UI prefabs |
| Scenes/ | Boot, menu, persistent world, harbour, and region scenes |
| Scripts/Runtime/ | Runtime gameplay and simulation code |
| Scripts/Editor/ | Editor-only generation, validation, and authoring tools |
| Settings/Input/ | Input Action assets and control schemes |
| Settings/Rendering/ | URP assets and rendering configuration |
| Shaders/ | Project-owned shaders |
| Tests/ | Edit Mode and Play Mode tests |

Assets/ThirdParty is reserved for externally sourced content. Record origin, license, version, modifications, and attribution before importing third-party assets.

## Current assemblies and code

- WhereTheSoundingsEnd.Runtime: runtime scripts; references Unity Input System.
- WhereTheSoundingsEnd.Editor: editor tooling; excluded from player builds.
- WhereTheSoundingsEnd.Tests.EditMode: unit-level tests.
- Core/WorldClock.cs: engine-independent simulation-time primitive.
- Core/Pcg32Random.cs: PCG-XSH-RR deterministic random generator.
- Boat/BoatPrototypeController.cs: temporary Rigidbody handling prototype.
- Camera/CameraFollow2_5D.cs: fixed-angle orthographic follow camera with configurable framing.
- Editor/ProjectFoundationSetup.cs: creates URP assets, project settings, and initial build-scene list.
- Editor/GreyhavenEnvironmentBuilder.cs: builds procedural Greyhaven terrain, shore, harbor, piers, village, estuary placeholders, navigation markers, starter boat, and camera.

## Greyhaven environment generation

Run Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation on first setup. It creates the initial URP assets and generates Assets/_Project/Scenes/Greyhaven_Blockout.unity.

Use Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment after changing the environment generator. The command warns before replacing the generated scene. Generated Unity assets and their corresponding .meta files should be committed after successful Editor initialization.

Greyhaven is a working technical greybox. Its coordinates and dimensions are prototype scale values; final terrain, water, architecture, collision, and camera framing require in-Editor testing.

## Rules

1. Keep runtime code independent of Editor APIs.
2. Keep domain algorithms testable without scene objects where practical.
3. Store authored, mostly immutable definitions in ScriptableObjects.
4. Keep mutable world state in runtime models with clear ownership.
5. Isolate third-party assets and maintain their license records.
6. Do not commit Library, Temp, Obj, generated IDE projects, or build output.
7. Do not label a feature implemented until it exists in runtime code and has passed its relevant tests.

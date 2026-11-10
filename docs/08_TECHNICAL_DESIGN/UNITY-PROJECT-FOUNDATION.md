# Unity Project Foundation

> Project: WHERE THE SOUNDINGS END  
> Status: Implemented scaffold; Unity Editor validation pending  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Purpose

This document records the first real Unity project scaffold. It replaces the previous assumption that the repository only contains design documentation. The scaffold is intentionally small and does not claim that the full game, vertical slice, or production systems are complete.

## Pinned editor and packages

- Unity Editor: 6000.3.28f1 (Unity 6.3 LTS).
- Universal Render Pipeline: 17.3.0.
- Input System: 1.20.1.
- Unity Test Framework: 1.4.6.
- Target platform baseline: Windows 64-bit.
- Gameplay language: C#.

The editor version is pinned in ProjectSettings/ProjectVersion.txt; package versions are pinned in Packages/manifest.json. Allow Unity to generate and maintain Packages/packages-lock.json on first import. Do not hand-edit the generated lock file.

## First open

1. Install Unity Hub and Unity Editor 6000.3.28f1. Include Windows Build Support for the Windows target.
2. Clone this repository and open its root folder through Unity Hub.
3. Allow Package Manager to resolve the pinned packages and allow the editor to finish the first import.
4. In the Unity Editor, run **Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation**.
5. The setup command creates the URP assets, starter materials, the Greyhaven blockout scene, player settings, and the initial enabled build-scene list.
6. Press Play to test the early boat handling. The scene uses primitive geometry and placeholder materials only.

The setup command is idempotent for generated assets. It does not overwrite an existing Greyhaven blockout scene or existing blockout materials.

## Current executable foundation

### Runtime assemblies

- WhereTheSoundingsEnd.Runtime contains player-facing runtime code and references the Input System package.
- WhereTheSoundingsEnd.Editor contains editor-only project setup and scene generation.
- WhereTheSoundingsEnd.Tests.EditMode contains Edit Mode tests.

Editor-only code must not leak into runtime assemblies. Runtime gameplay rules should remain testable without requiring a scene where practical.

### Initial runtime components

- WorldClock is an engine-independent elapsed-time primitive. The game's day length, calendar, tide schedule, and time scale remain design decisions.
- Pcg32Random implements PCG-XSH-RR with explicit seed and stream values. Treat its algorithm and version as save-compatibility-sensitive once persistent world generation uses it.
- BoatPrototypeController is temporary Rigidbody handling for the first Greyhaven test. Its values are not the final vessel model.
- CameraFollow2_5D provides an orthographic fixed-angle follow camera.

### Input actions

The Boat action map contains Move, Interact, Fish, SoundingPulse, and Pause. Keyboard and gamepad bindings are defined in PlayerControls.inputactions. Only Move is consumed by the current boat prototype; the other actions are reserved for subsequent systems.

### Generated scene and settings

The editor setup command generates:

- Assets/_Project/Scenes/Greyhaven_Blockout.unity
- Assets/_Project/Settings/Rendering/DefaultRenderer.asset
- Assets/_Project/Settings/Rendering/DefaultPipeline.asset
- Assets/_Project/Art/Materials/Blockout_*.mat

It also configures the default render pipeline, Linear color space, Input System backend, Windows application identifier, window dimensions, and initial build-scene list. Generated Unity assets and their .meta files should be committed after the first successful editor setup.

## Integrity checks and current limitations

Run python tools/validate_project.py and python tools/validate_csharp.py from the repository root. GitHub Actions runs both checks on pushes and pull requests to main. The C# check uses Tree-sitter to parse every project source file and catch malformed syntax.

These checks validate repository structure, JSON configuration, assembly-definition references and C# syntax. They do not perform semantic C# compilation against Unity/package assemblies, import assets, execute Edit Mode tests, or produce a Windows build. Those checks must be run in the pinned Unity Editor after first import.

Not yet implemented: fishing, catch inventory, selling, world persistence, save migration, ecosystem simulation, creature AI, threat director, quests, full navigation/acoustics, finished UI/audio, production art, and release packaging.

## First validation gate

Before adding more content, verify all of the following in Unity 6000.3.28f1:

- Project opens without package or compile errors.
- The setup menu completes without errors.
- URP is assigned in Graphics settings.
- Greyhaven blockout opens and the camera follows the boat.
- WASD and left-stick movement work; boat collision and deceleration are observable.
- Edit Mode tests pass.
- A Windows x86-64 development build launches from a clean clone.

Record the actual test result and machine/editor version before marking this gate complete.

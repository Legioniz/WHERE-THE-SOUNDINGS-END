# C# Compile-Risk Audit

> Project: WHERE THE SOUNDINGS END  
> Audit date: 2026-11-10  
> Target editor: Unity 6000.3.28f1

## Scope reviewed

A targeted source and configuration review covered the runtime, editor and Edit Mode assembly definitions; package pins; the boat/input, save, acoustic, atmosphere and survey runtime systems; the Yarn dialogue presenter; and the two generated-scene builders.

## Findings

### Assembly boundaries

The project has distinct runtime, editor and Edit Mode test assemblies. The runtime assembly references Input System and Yarn Spinner Unity. The editor assembly references the runtime assembly, URP runtime, Input System and Yarn Spinner Unity. The Edit Mode test assembly references the runtime assembly and opts into Unity Test Framework assemblies.

The repository validator now checks assembly-definition names, duplicate names, declared references and the intended Editor/Test platform boundaries. The external package assembly allow-list is explicit so unexpected references fail CI rather than silently passing.

### API and version-sensitive areas

The project pins Unity 6000.3.28f1, Input System 1.20.1, URP 17.3.0, Unity Test Framework 1.4.6 and Yarn Spinner Unity 3.2.8. The reviewed Unity 6 API usage includes Rigidbody.linearVelocity and FindObjectsByType. The custom Yarn Spinner DialoguePresenterBase overrides and YarnTask cancellation flow are package-version-sensitive and require confirmation by the Unity compiler.

No additional source-level compiler defect was confirmed during this targeted review. This is not equivalent to a successful semantic compilation.

## Automated checks added

GitHub Actions now runs:
1. Repository/configuration integrity validation.
2. Assembly-definition reference and platform-boundary validation.
3. Tree-sitter parsing of every C# file under Assets/_Project.

Tree-sitter detects malformed syntax and missing syntax nodes. It does not resolve C# symbols, generic overloads, Unity/package APIs, generated code, or asset import dependencies.

## Follow-up source finding — 2026-11-11

A later M0 review of `Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs` found a semantic compile blocker that the earlier targeted review did not identify: the Inez Sarmiento `BuildVillageResident` call passed `trim`, but no local `trim` variable was declared in `Build`. The syntax parser accepted the file because the identifier was syntactically valid; only semantic compilation would resolve the missing name.

The call now passes the existing `concrete` material, matching the other resident placeholder setup. The correction is in commit [`06a7004`](https://github.com/Legioniz/WHERE-THE-SOUNDINGS-END/commit/06a700406bad809e6671af26163e147dbd309811).

This finding narrows the confidence of the earlier source review: no additional semantic defects are claimed absent a Unity compile. Unity `6000.3.28f1` import/compile and Edit Mode execution remain mandatory.

## Verification status

| Gate | Status |
|---|---|
| Repository/configuration checks | Automated in GitHub Actions |
| Assembly-definition checks | Automated in GitHub Actions |
| C# syntax parsing | Automated in GitHub Actions |
| Unity semantic compilation | **Not verified in this environment** |
| Edit Mode tests | **Not run in this environment** |
| Play Mode tests / player build | **Not run in this environment** |

Do not describe the project as compile-clean until Unity 6000.3.28f1 completes a clean import with zero compiler errors. After that, run the Edit Mode suite and exercise the generated Greyhaven scene in Play Mode.

## Required Unity validation

1. Open the repository using Unity 6000.3.28f1.
2. Wait for package resolution and the first asset import to finish.
3. Confirm the Console contains no C# compiler errors.
4. Run all Edit Mode tests from Test Runner.
5. Open Greyhaven and verify boat input, harvesting, save/load, acoustic event publication and atmosphere timing.
6. Record the editor version and actual results in the commit that completes this gate.

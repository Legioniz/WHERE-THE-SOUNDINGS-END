# Performance Testing — Area Streaming and Asset Residency

> Project: WHERE THE SOUNDINGS END  
> Status: Streaming source implemented; Unity profiling and player acceptance pending  
> Owner: Project lead  
> Version: 1.0  
> Last reviewed: 2026-11-12

## 1. Goal

Greyhaven should reveal and unload static scenery around the vessel without a full-scene transition, loading spinner, or intentional pause. The game preloads an area's additive scene while the boat is still outside its visible space. The area root remains inactive until all content integrity checks pass; activation happens only after validation.

This is a performance architecture, not a promise of zero frame-time spikes on every machine. Unity Editor profiling and target-hardware benchmarks remain required.

## 2. Current implementation boundary

The persistent Greyhaven scene keeps the vessel, player camera, water simulation, salvage/survey interaction sites, dialogue runner, UI, NPCs, schedules, NPC anchors, action-driven clock, save coordinator, and all save participants alive.

Six additive scenes contain static scenery only:

| Area | Preload centre (X/Z) | Preload radius | Unload radius | Startup |
|---|---:|---:|---:|---|
| Terrain | (0, -58) | 132 m | 185 m | Yes |
| Harbour | (0, 18) | 112 m | 155 m | Yes |
| Village | (0, -54) | 96 m | 138 m | Yes |
| West estuary | (-80, -36) | 62 m | 88 m | No |
| East estuary | (78, -36) | 62 m | 88 m | No |
| Lighthouse | (-86, -22) | 68 m | 96 m | No |

The preloader checks horizontal vessel distance four times per second by default. It permits one concurrent scene load by default to avoid bursting disk, CPU, and memory bandwidth. Unload distance is intentionally larger than preload distance, and an outside-area grace interval prevents rapid unload/reload oscillation.

## 3. Runtime load sequence

1. The director checks the vessel's horizontal distance against each area's preload radius.
2. It queues a single additive scene load through Unity's asynchronous scene API.
3. The scene root is authored inactive. No static renderers or colliders become visible before validation.
4. The loader requires exactly one root object and exactly one StreamedAreaSceneRoot.
5. The root validates its stable area ID, content revision, inactive state, static-only component policy, mesh references, enabled mesh collider references, renderer materials, submesh coverage, and shaders.
6. A failed or mismatched area is unloaded and marked failed for the current session. It is not activated as a partial or fallback scene.
7. A valid root is activated, then the world atmosphere director refreshes its cached light/audio targets.
8. When the vessel leaves the larger unload radius for the configured grace period, the root is deactivated and the additive scene is unloaded.

No loading overlay is displayed. If a load is not complete before the vessel arrives, the system continues loading without pausing gameplay; the empty space remains preferable to a blocking spinner or partially loaded asset set. The default radius is a starting budget and must be tuned from measured travel speed and load time.

## 4. Asset integrity and mismatch prevention

- Area identity and content revision are explicit. A wrong scene at a valid path is rejected.
- An area root must be inactive in its saved scene, so validation happens before visibility.
- Area scenes must not contain gameplay MonoBehaviour scripts or Rigidbody objects. Dynamic systems and their cross-scene references stay persistent.
- Missing MeshFilter or SkinnedMeshRenderer meshes and enabled MeshCollider meshes are rejected.
- Renderer material lists must be nonempty; null materials, missing or unsupported shaders, and insufficient material slots for mesh submeshes are rejected.
- Area scenes must contain exactly one root object to avoid unvalidated orphan objects.
- Static geometry is moved as a whole hierarchy; parent-child relationships and world transforms are preserved.
- Procedural Greyhaven terrain and basalt outcrop meshes are saved as reusable mesh assets before scene generation, rather than depending on transient in-memory meshes.
- Unity scene references are used for serialized dependencies. No runtime network download, paid service, Addressables package, or paid API is required.

The current blockout uses generated Unity primitives and mesh assets. The authoring builder creates six area scenes and includes them in Build Settings. Existing generated projects receive a destructive-rebuild confirmation before the old one-scene Greyhaven blockout is replaced.

## 5. Memory policy

Unloading an additive scene destroys its scene objects and releases scene-owned references. Shared materials, textures, and meshes remain resident while another loaded scene or object references them. The runtime deliberately does not call Resources.UnloadUnusedAssets after every area exit: a global unused-asset sweep can itself create a frame-time spike. Any future global asset collection should run during a title/safe-harbour transition or another authored quiet window, after profiling proves that it is needed.

For final art, large textures and meshes should be split by area, reused through shared project assets where appropriate, and imported with platform-appropriate compression, mipmaps, LODs, and sensible texture sizes. Streaming scenes is not a substitute for those asset budgets.

## 6. Required acceptance tests

### Functional

- Fresh checkout: Initialize Project Foundation generates Greyhaven plus all six additive scenes and the deep-ocean sandboxes.
- Repeated initialization is safe; no duplicated area roots, objects, or Build Settings entries appear.
- Upgrading an old generated project requires explicit confirmation. Cancelling leaves the existing scene untouched.
- Enter and leave each area's preload and unload bands; exactly one instance is active and no scene is unloaded inside its preload/unload hysteresis band.
- Rapidly cross the same boundary several times; queued requests do not duplicate, and unload grace prevents thrashing.
- A missing scene, wrong area ID, wrong revision, missing mesh, missing material, missing shader, unsupported shader, insufficient submesh materials, unexpected gameplay script, or extra scene root is rejected without activating it.
- Save, load, dialogue, schedules, water movement, and boat controls continue while areas stream.
- Loading or unloading a static scenery scene does not change the active gameplay scene or save participant set.

### Performance

Profile a Windows development build on the minimum supported hardware and a representative mid-range machine. Record:

- Median, 95th, and 99th percentile frame time while stationary and underway.
- Main-thread and render-thread spikes during area load, activation, and unload.
- Scene load duration, queue wait, area activation duration, and number of simultaneous loads.
- Managed heap, native texture/mesh memory, total process memory, and post-unload retained memory.
- GPU time, draw calls, batches, shadow casters, overdraw, and visible renderer count.
- Worst-case sequence: maximum boat speed through two adjacent preload boundaries, with the title UI already closed and the scene under representative weather/light load.

Initial target: no blocking load screen, no forced time-scale pause, no duplicate renderer flashes, and no missing/mismatched assets. Set concrete frame-time and memory budgets only after target hardware is selected and baseline profiling is recorded.

## 7. Known limitations

- Unity Editor compilation, Edit Mode execution, Play Mode travel tests, and Windows build have not yet been performed for this feature.
- The six generated scene assets are outputs of the Editor bootstrap and are not committed as binary scene files in the repository. A clean checkout must run Initialize Project Foundation.
- The current system streams static scenery, not NPCs, water simulation, mission state, dynamic wildlife, or arbitrary audio/addressable groups.
- A late or failed load may leave a visible gap rather than freeze the player. Preload radius and asset budgets must be tuned to avoid reaching an unloaded area.
- Scene unloading does not guarantee immediate physical deallocation of shared assets. Avoid global asset unload sweeps during active navigation.

## 8. Related documents

- [Area Streaming and Asset Residency](../08_TECHNICAL_DESIGN/AREA-STREAMING-AND-ASSET-RESIDENCY.md)
- [Greyhaven](../05_WORLD_AND_LEVEL_DESIGN/GREYHAVEN.md)
- [Save and Load](../06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md)
- [Testing Strategy](TESTING-STRATEGY.md)

# Area Streaming and Asset Residency

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime telemetry and Play Mode transition test authored; Unity execution pending  
> Version: 1.2  
> Last reviewed: 2026-11-15

## 1. Design intent

Use a Resident Evil-style area handoff at the world-architecture level: small, bounded spaces become resident before the player reaches them; distant spaces are retired after the player has left. The player does not see a global level-loading screen. This project uses Unity additive scenes rather than a hard-coded scene swap.

The goal is to avoid loading every static Greyhaven environment object at once and to isolate art dependencies by area. It is not intended to mimic Resident Evil's fixed-camera staging, room locks, or encounter pacing.

## 2. Scene ownership

### Persistent gameplay scene

The generated Greyhaven_Blockout scene owns:

- Player vessel Rigidbody, boat controller, camera, and controls.
- Water mesh simulation and surface sampling.
- Dynamic fishing/salvage/survey sites and the acoustic/world simulation.
- Dialogue runner, UI Toolkit documents, front-end menus, dockside hub and interaction components.
- NPC agents, NPC schedules, externally managed resident trigger components, dockside access points and waypoint anchors.
- World clock, atmosphere director, save coordinator, autosave, and save participants.
- AreaStreamingDirector, which owns area load state and travel-distance checks.

These are not unloaded during ordinary sailing. Keeping NPCs, save participants, and gameplay references in one scene avoids stale object references and incomplete cross-scene saves.

### Additive static area scenes

The Editor bootstrap splits static-only child hierarchies into six scenes:

| Scene asset | Stable ID | Content |
|---|---|---|
| Greyhaven_Terrain.unity | greyhaven.terrain | Island terrain mesh and static basalt outcrops |
| Greyhaven_Harbour.unity | greyhaven.harbour | Breakwaters, piers, racks, crates, and static harbour structures |
| Greyhaven_Village.unity | greyhaven.village | Buildings, roads, street-lamp fixtures, and static village props |
| Greyhaven_Estuary_West.unity | greyhaven.estuary.west | West mudflat and mangrove visuals |
| Greyhaven_Estuary_East.unity | greyhaven.estuary.east | East estuary sediment and mangroves |
| Greyhaven_Lighthouse.unity | greyhaven.lighthouse | Lighthouse tower, lantern room, roof, and bearing light |

The generator refuses to move a hierarchy that contains a custom MonoBehaviour or Rigidbody. NPC agents and their anchors therefore remain in the persistent gameplay scene. The DocksideHubController and its UIDocument are children of the persistent world root, not any extracted static area. They remain available while the village and lighthouse scenery load/unload. Buoys, water, salvage interactions, and other simulation objects remain persistent unless deliberately moved into a future data-driven system.

Each generated area scene contains exactly one StreamedAreaSceneRoot, saved inactive. The loader is the only system that activates this root.

## 3. Load policy

AreaStreamingDirector performs horizontal X/Z distance checks using unscaled time, so preloading continues while the title overlay or pause UI has set the time scale to zero.

- Preload radius: load additive scene and validate it while the vessel is still outside the area.
- Unload radius: larger than preload radius; defines a hysteresis band that prevents boundary flicker.
- Unload grace: the vessel must remain outside the unload radius continuously before unloading starts.
- Concurrency: one scene load by default. It can be raised to at most three after profiling.
- Retry: a missing or invalid scene is marked failed for the current director lifetime. This prevents per-frame load loops and repeated IO/log spam.
- Re-entry: if the vessel returns while an area is already unloading, the director queues a fresh load request. The load queue rechecks the current distance before starting so a stale request cannot load an area the vessel has already left.
- Unload guard: the unload coroutine rechecks the vessel distance immediately before deactivating the area root.
- On-demand access: `RequestArea(areaId)` pins a known area as an explicit request and loads it regardless of vessel distance; `ReleaseArea(areaId)` returns it to automatic distance policy. Explicit requests keep the area active until released. Unknown IDs return false, and a new explicit request retries a previously failed area.
- Standby: after the vessel leaves the preload radius, a loaded static root is deactivated immediately while its scene and assets remain resident. Returning inside the preload radius reactivates it without a scene reload. The wider unload radius and grace period still govern final retirement. Explicitly requested areas do not enter standby.
- Idle cleanup: after at least two area unloads, a quiet window of 12 seconds, and a 90-second cleanup cooldown, the director runs one asynchronous `Resources.UnloadUnusedAssets` sweep. New scene loads wait until the sweep finishes. The cleanup is not run on every transition and never forces a managed-GC collection.
- Explicit-call boundary: `RequestArea` and `ReleaseArea` are public runtime APIs, but the current Greyhaven prototype has no gameplay/quest caller for them. Existing six-area Greyhaven streaming is driven by vessel-distance checks; explicit requests are currently covered by Play Mode tests. Wire future region/mission transitions to these APIs when those features are implemented.
- Retry: explicit `RequestArea` calls clear a prior failed flag and allow a deliberate retry; automatic proximity loading still avoids retry loops.

Areas marked loadAtStartup are terrain, harbour, and village. The estuaries and lighthouse are loaded on approach. The initial radii are conservative starting values and should be tuned from measured vessel speed, scene load duration, and target-device hardware.

## 4. Validation contract

StreamedAreaSceneRoot.TryValidate rejects an area before activation if any of the following is true:

1. Root is active in the serialized scene.
2. Stable area ID or content revision does not match the requested definition.
3. The root contains another gameplay MonoBehaviour or a Rigidbody.
4. A mesh filter, skinned renderer, or enabled mesh collider is missing its mesh.
5. A renderer has no material assignments, a null material, or a missing/unsupported shader.
6. A renderer has fewer material slots than the mesh has submeshes.
7. The scene has an extra root object or does not have exactly one area root.

A rejected scene is unloaded. The streaming layer does not spawn substitute cubes, render magenta materials, or accept partially loaded asset sets. The persistent dockside menu is not a streamed interior scene: it is a UI presentation bound to location coordinates and existing runtime systems. A missing scene/build-list entry is also rejected before load starts.

### Editor preflight and content revisions

The Greyhaven Editor builder owns the current streamed-area content revision (5) and the ordered stable IDs for all six area scenes. The project-initialization command opens each generated area scene additively for a preflight check and verifies that it contains exactly one root with a StreamedAreaSceneRoot, the expected stable ID and current content revision, and valid referenced meshes/materials/shaders. A scene already loaded before preflight is left loaded; scenes opened only for validation are closed, and the previously active scene is restored.

Missing, stale or invalid area scenes produce a clear confirmation prompt before the generated Greyhaven blockout and all six area scenes are rebuilt. Choosing to keep them leaves the existing content untouched and emits a warning; affected areas may still be rejected by the runtime loader. The November 15 revision increment covers the new historical-artifact visual clusters added beneath the streamed harbour, village, western estuary and lighthouse roots. Rebuilding from the current Editor builder is required before those visual additions appear in existing generated area scenes.

## 5. State transitions

1. UNLOADED to QUEUED when the vessel enters the preload radius.
2. QUEUED to LOADING when a concurrency slot is available.
3. LOADING to VALIDATING when Unity additive load completes.
4. VALIDATING to ACTIVE after identity, revision, and asset checks pass and the root activates.
5. VALIDATING to REJECTED if validation fails; the scene unloads without partial activation.
6. ACTIVE to STANDBY when the vessel leaves the preload radius and no explicit request holds the area.
7. STANDBY to ACTIVE when the vessel returns inside preload range; the already-loaded scene is reactivated.
8. ACTIVE or STANDBY to OUTSIDE_GRACE when the vessel exceeds the unload radius.
9. OUTSIDE_GRACE to ACTIVE if the vessel returns before grace expires.
10. OUTSIDE_GRACE to UNLOADING after the grace duration.
11. UNLOADING to UNLOADED when scene unload completes.

A queued proximity request is cancelled when the vessel leaves the preload band before loading begins. Explicit requests are distance-independent and remain pinned until released. An async load already underway is allowed to complete, then standby/unload policy is applied.

## 6. Asset and memory rules

- Prefer separate area-owned meshes, textures, lightmaps, decals, and scene-local static props for future final art.
- Reuse common materials and shared assets across areas where their residency is intentional.
- Use LOD groups, mesh simplification, occlusion culling, static batching where appropriate, texture mipmaps/compression, and sensible shadow distances based on profiling.
- Keep dynamic objects and their references in the persistent world or move them through a dedicated ownership/state-transfer contract; do not mix them into static area scenes.
- Scene unload retires the area's GameObjects and scene references. Unity may retain shared assets still referenced elsewhere.
- Do not call `Resources.UnloadUnusedAssets` after every transition; global sweeps can introduce frame-time spikes. The director performs a throttled asynchronous sweep only after multiple area retirements and a quiet window, and serializes new scene loads against that sweep. Keep the cleanup thresholds configurable and profile them on target hardware.
- No Addressables package, remote download, paid storage, paid API, or runtime account is required for this implementation.

## 7. Runtime profiling and telemetry

In the Editor and Development Builds, each completed area load emits one `[AreaStreaming] LOAD` record and each unload emits `UNLOAD_START` / `UNLOAD_COMPLETE` records. The load record includes:

- additive load duration measured from request start through validated root activation;
- unique mesh, material and texture counts referenced by the area's renderers;
- estimated memory size of those unique referenced assets using Unity's runtime memory-size API;
- global Unity allocated/reserved memory, managed heap use and GC-reported managed memory;
- allocated-memory delta across the load operation.

The unload record captures the area's referenced asset estimate before deactivation and global memory counters after unload. Logs are emitted only in the Editor or Development Builds; release builds do not emit per-transition profiling logs.

**Interpretation limits:** per-area asset bytes are an estimate of assets referenced by that area's renderers, not a measurement of physical GPU VRAM or guaranteed exclusive residency. Shared assets may appear in multiple area's estimates. Global allocated/reserved counters include the rest of the running game and Unity runtime. A scene unload does not guarantee that Unity immediately releases every shared asset. Do not trigger `Resources.UnloadUnusedAssets` on every transition.

For a useful hardware profile, repeat the same route at least five times in a Windows Development Build with the Unity Profiler attached. Record median and worst-case load duration, frame-time spikes, total allocated/reserved memory, managed memory, per-area asset estimates, and the number of simultaneously loaded areas. Capture a cold-start route and a warm repeat route separately. Record the target PC CPU, GPU, RAM, resolution, graphics quality and build configuration with the results.

No performance threshold is claimed until a representative Windows baseline is measured.

## 8. Future expansion

For future islands or larger maritime regions, author additional area definitions and matching additive scenes with unique stable IDs and content revisions. Do not make each island a separate persistent gameplay root until save data, active player ownership, NPC/world clock ownership, lighting/render settings, and cross-region travel have explicit transfer semantics.

## 9. Verification status

The source includes deterministic Edit Mode tests for distance boundaries, non-finite inputs, preload/unload hysteresis, standby boundaries, and exact area-ID/content-revision matching (including stale revision and wrong-area rejection). The Editor preflight also checks generated scene presence, root count, serialized root state and referenced asset integrity before offering a rebuild. Play Mode coverage exercises proximity preload/unload and an explicit request → standby → wake → release → unload cycle while asserting the persistent gameplay scene remains loaded. Static CI validation checks the scene loader, on-demand API, standby lifecycle, quiet-window cleanup, root integrity contract, editor scene generation, build-scene list preservation, telemetry instrumentation and Play Mode coverage.

Unity semantic compilation, Edit Mode test execution, Play Mode travel test execution, frame-time profiling, and Windows player build remain mandatory before treating the feature as production-verified. The telemetry is implemented but no real hardware measurements have been collected in this environment.

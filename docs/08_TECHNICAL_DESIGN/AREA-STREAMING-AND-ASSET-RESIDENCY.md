# Area Streaming and Asset Residency

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime and editor source implemented; Unity validation pending  
> Version: 1.0  
> Last reviewed: 2026-11-12

## 1. Design intent

Use a Resident Evil-style area handoff at the world-architecture level: small, bounded spaces become resident before the player reaches them; distant spaces are retired after the player has left. The player does not see a global level-loading screen. This project uses Unity additive scenes rather than a hard-coded scene swap.

The goal is to avoid loading every static Greyhaven environment object at once and to isolate art dependencies by area. It is not intended to mimic Resident Evil's fixed-camera staging, room locks, or encounter pacing.

## 2. Scene ownership

### Persistent gameplay scene

The generated Greyhaven_Blockout scene owns:

- Player vessel Rigidbody, boat controller, camera, and controls.
- Water mesh simulation and surface sampling.
- Dynamic fishing/salvage/survey sites and the acoustic/world simulation.
- Dialogue runner, UI Toolkit document, front-end menus, and interaction components.
- NPC agents, NPC schedules, interaction triggers, and waypoint anchors.
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

The generator refuses to move a hierarchy that contains a custom MonoBehaviour or Rigidbody. NPC agents and their anchors therefore remain in the persistent gameplay scene. Buoys, water, salvage interactions, and other simulation objects remain persistent unless deliberately moved into a future data-driven system.

Each generated area scene contains exactly one StreamedAreaSceneRoot, saved inactive. The loader is the only system that activates this root.

## 3. Load policy

AreaStreamingDirector performs horizontal X/Z distance checks using unscaled time, so preloading continues while the title overlay or pause UI has set the time scale to zero.

- Preload radius: load additive scene and validate it while the vessel is still outside the area.
- Unload radius: larger than preload radius; defines a hysteresis band that prevents boundary flicker.
- Unload grace: the vessel must remain outside the unload radius continuously before unloading starts.
- Concurrency: one scene load by default. It can be raised to at most three after profiling.
- Retry: a missing or invalid scene is marked failed for the current director lifetime. This prevents per-frame load loops and repeated IO/log spam.

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

A rejected scene is unloaded. The streaming layer does not spawn substitute cubes, render magenta materials, or accept partially loaded asset sets. A missing scene/build-list entry is also rejected before load starts.

## 5. State transitions

1. UNLOADED to QUEUED when the vessel enters the preload radius.
2. QUEUED to LOADING when a concurrency slot is available.
3. LOADING to VALIDATING when Unity additive load completes.
4. VALIDATING to ACTIVE after identity, revision, and asset checks pass and the root activates.
5. VALIDATING to REJECTED if validation fails; the scene unloads without partial activation.
6. ACTIVE to OUTSIDE_GRACE when the vessel exceeds the unload radius.
7. OUTSIDE_GRACE to ACTIVE if the vessel returns before grace expires.
8. OUTSIDE_GRACE to UNLOADING after the grace duration.
9. UNLOADING to UNLOADED when scene unload completes.

A request that is still queued is cancelled when the vessel leaves the preload band before the load begins. A request that has entered Unity's async-load phase is allowed to complete and then follows the normal distance/grace unload policy.

## 6. Asset and memory rules

- Prefer separate area-owned meshes, textures, lightmaps, decals, and scene-local static props for future final art.
- Reuse common materials and shared assets across areas where their residency is intentional.
- Use LOD groups, mesh simplification, occlusion culling, static batching where appropriate, texture mipmaps/compression, and sensible shadow distances based on profiling.
- Keep dynamic objects and their references in the persistent world or move them through a dedicated ownership/state-transfer contract; do not mix them into static area scenes.
- Scene unload retires the area's GameObjects and scene references. Unity may retain shared assets still referenced elsewhere.
- Do not call Resources.UnloadUnusedAssets after every transition; global sweeps can introduce the very frame-time spikes this system is intended to avoid. Consider an explicit quiet-window cleanup only after memory profiling proves it is required.
- No Addressables package, remote download, paid storage, paid API, or runtime account is required for this implementation.

## 7. Future expansion

For future islands or larger maritime regions, author additional area definitions and matching additive scenes with unique stable IDs and content revisions. Do not make each island a separate persistent gameplay root until save data, active player ownership, NPC/world clock ownership, lighting/render settings, and cross-region travel have explicit transfer semantics.

## 8. Verification status

The source includes deterministic tests for distance boundaries, non-finite inputs, and preload/unload hysteresis. Static CI validation checks the scene loader, root integrity contract, editor scene generation, and build-scene list preservation.

Unity semantic compilation, Edit Mode test execution, Play Mode travel tests, frame-time profiling, and Windows player build remain mandatory before treating the feature as production-verified.

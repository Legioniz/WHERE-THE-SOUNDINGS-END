# Performance and Platform Profiling

> Project: WHERE THE SOUNDINGS END  
> Status: Baseline policy  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Target order

The first shipping target is Windows PC. Do not optimize around mobile-only APIs before a mobile build is an approved platform target.

## Tooling clarification

The DREDGE mobile presentation is a Unity Developer Summit case study. It does not establish a generic product suite named “Unity Studio Productions (USP)”. This repository therefore uses documented Unity and platform profiling tools rather than assuming access to an internal Unity studio toolkit.

## Universal Unity profiling

Profile early and repeatedly using:

- Unity Profiler: CPU, GPU, rendering, physics, audio, scripting and allocations.
- Unity Memory Profiler: managed/native memory, textures, meshes, render targets and retained objects.
- Profile Analyzer: compare captures before/after a change.
- Frame Debugger and URP Rendering Debugger: draw calls, overdraw, render passes and shader cost.
- Project Auditor: static project/code findings.
- Development builds on representative target hardware, not only Editor Play Mode.

Capture representative workloads, not an empty harbor only:

1. Greyhaven at dawn with boat and village visible.
2. Dense fog and rain with audio and water effects.
3. Busy fishing ground with multiple schools.
4. Several nearby creature agents plus distant cohort simulation.
5. Major threat encounter with sonar, audio and environmental effects.
6. Long session to identify memory growth and thermal/performance drift.

## Performance budgets

Do not invent benchmark results. Establish budgets after capturing a baseline on the minimum supported PC. Record target hardware, Unity version, render resolution, graphics API, quality preset, scene, workload and capture duration with every result.

Track at minimum:

- CPU main-thread and render-thread frame time.
- GPU frame time.
- 1% low and sustained frame rate.
- Managed allocations per frame and GC pauses.
- Total memory, texture memory and mesh memory.
- Draw calls, batches, triangles and overdraw.
- Physics time and active rigidbody/collider counts.
- Audio voices and DSP load.
- Load/scene-transition duration.

## Optimization rules

- Use spatial partitioning and distance-based simulation LOD for wildlife.
- Pool repeated transient effects and frequently spawned objects after profiling demonstrates allocation pressure.
- Use mesh LODs, occlusion and frustum culling where the scene warrants them.
- Keep URP shader variants and material count controlled.
- Use texture compression and sensible mipmaps.
- Avoid per-frame LINQ, allocations, broad scene searches and repeated string construction in hot paths.
- Use deterministic, bounded update cadences for far-field ecology.
- Do not reduce simulation fidelity blindly; profile the actual bottleneck first.

## Mobile-only branch

If Android becomes an approved target, evaluate Unity Adaptive Performance and the Android Dynamic Performance Framework (ADPF) to react to thermal/power conditions. These are device/runtime adaptation tools, not replacements for profiling.

Android profiling should include Unity Profiler, Memory Profiler, Android Studio Profiler and Android GPU Inspector on representative physical devices. Sustained tests are required to detect thermal throttling.

If iOS is approved, use Xcode Instruments and Metal frame capture. For Windows PC, use PIX for DirectX 12 workloads where relevant, plus vendor GPU tools as needed.

## Source references

- Unity, Profiling and debugging tools: https://unity.com/how-to/profiling-and-debugging-tools
- Android Developers, Get started with game development in Unity: https://developer.android.com/games/engines/unity/start-in-unity
- GDC Vault, Unity Developer Summit: Getting DREDGE to Look Great and Run Cool on Mobile: https://gdcvault.com/play/1035399/Unity-Developer-Summit-Getting-DREDGE

The DREDGE mobile session is a case study, not a target performance specification for this project.

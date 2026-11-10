# Performance and Programming Language Guidance

> Project: WHERE THE SOUNDINGS END  
> Status: Engineering baseline / proposed production standard  
> Primary engine: Unity  
> Primary gameplay language: C#  
> Last reviewed: 2026-11-10

## 1. Executive decision

**Use Unity with C# as the project's foundation.** Do not split the game across several programming languages merely to chase theoretical raw speed. Keep gameplay, orchestration, and most simulation logic in C#. Use Unity's native performance tooling first, then introduce lower-level code only when profiling proves a specific bottleneck.

Recommended stack:

- **Unity** — rendering, scene management, physics, animation, audio, navigation, input, asset pipeline, and platform builds.
- **C#** — primary language for gameplay, AI, world systems, interaction, progression, inventory, save/load, and game-state logic.
- **Unity Profiler and Profile Analyzer** — identify CPU, GPU, memory, rendering, and allocation bottlenecks.
- **C# Jobs + Burst Compiler** — parallelize suitable independent calculations and compile compatible performance-critical C# into optimized native code.
- **Unity Mathematics** — data-oriented math in Burst-compatible jobs.
- **Entities (ECS / DOTS)** — optional; use selectively for large populations or data-heavy simulation where measurements justify the architectural cost.
- **C++ native plugins** — reserved for isolated workloads that demonstrably need native integration or an existing native library.
- **Python** — offline tooling, content validation, probability simulations, balance analysis, and benchmark processing. Do not use Python for per-frame game logic.

This is a performance strategy, not a mandate to use every package or language.

## 2. Language comparison

The table is a qualitative engineering comparison, not a universal benchmark ranking. Actual performance depends on compiler, runtime, algorithms, data layout, memory behavior, engine bindings, workload, and target hardware.

| Language | Raw execution potential | Memory control | Integration fit for this Unity project | Recommended use |
|---|---|---|---|---|
| C | Very high | Manual | Native plugin / library boundary | Specialized native libraries |
| C++ | Very high | Manual, RAII, custom allocators | Strong native-plugin option | Proven low-level hotspots |
| Rust | Very high | Ownership and borrowing model | Requires a native interop layer and deployment testing | Optional native library experiment |
| C# | High; can be very efficient with suitable data structures and Burst-compatible code | Managed by default; explicit low-allocation techniques available | Excellent; Unity's primary scripting language | **Main game language** |
| Go | High | Garbage-collected | No standard Unity gameplay path | External tools or services |
| Java | High | Garbage-collected | No standard Unity gameplay path | External tools or services |
| Swift | High | ARC / runtime-managed | Platform-specific interop | Not recommended for core game systems |
| Zig | Very high | Explicit control | Native plugin integration work required | Experimental native module |
| Lua | Runtime-dependent | Runtime-managed | Requires embedding or a third-party integration | Embedded scripting only |
| JavaScript / TypeScript | Runtime-dependent | Runtime-managed | Not a natural native Unity gameplay path | Web tools or companion interfaces |
| Python | Low for tight real-time CPU loops; excellent for tooling | Runtime-managed | External/offline tooling | Simulation, analysis, pipeline automation |

### Important interpretation

- C++ is not automatically faster for the whole game. Calling native code frequently across a managed/native boundary can erase gains.
- C# is not automatically slow. Data layout, allocation behavior, algorithmic complexity, batching, and Burst compatibility can matter more than the language label.
- Rust can deliver native performance, but its use inside a Unity project adds FFI, build, packaging, and platform-validation work.
- Python is valuable for testing the game's mathematical systems offline, but runtime gameplay should remain in Unity-compatible code.
- Compare complete workloads, not isolated language benchmark charts.

## 3. Unity performance stack

### C# — default gameplay layer

Use C# for:
- Player movement, interaction, and abilities.
- Enemy state machines and behavior coordination.
- Horror director and dynamic event scheduling.
- Quest, objective, progression, and narrative-state systems.
- Inventory, equipment, crafting, and resources.
- World state, doors, switches, traps, and interactables.
- Save/load, settings, and configuration.
- UI, menus, dialogue, and accessibility.
- Audio event orchestration and gameplay feedback.

Engineering rules:
- Prefer event-driven updates over polling every object every frame when practical.
- Cache component and system references; avoid repeated scene-wide searches.
- Avoid LINQ, closures, boxing, string formatting, and temporary collections in hot paths unless profiling shows they are harmless.
- Reuse frequently spawned objects where pooling is appropriate.
- Keep frequently accessed data compact and predictable.
- Avoid expensive work in `Update()` when it can be scheduled less frequently or triggered by events.
- Treat garbage collection as a measurable budget; do not assume that every allocation is harmful, but eliminate repeated avoidable allocations in critical loops.

### C# Jobs + Burst

Use for suitable independent, data-heavy work such as:
- Large batches of distance, visibility, or influence calculations.
- Procedural placement validation.
- Statistical sampling and simulation batches.
- Spatial queries over custom data structures.
- Large-scale environmental or threat-field calculations.
- Other pure or mostly data-oriented calculations that can be safely separated from UnityEngine object access.

Constraints:
- Burst supports a subset of C# and has restrictions. Keep Burst code isolated from managed objects and unsupported APIs.
- Jobs should process batches of data, not tiny individual calls with high scheduling overhead.
- Respect job dependencies and NativeContainer ownership/lifetime rules.
- Most UnityEngine object and scene operations must remain on the main thread unless the specific API documents thread safety.
- Complete jobs only when their results are actually required; blocking too early defeats concurrency.
- Measure scheduling overhead and total frame time, not only the job's inner loop.

### Entities / ECS / DOTS

Consider ECS selectively when:
- Many similar entities require repeated data-oriented processing.
- Entity counts and update costs are demonstrably high.
- The project benefits from contiguous data and controlled system scheduling.
- The team can support the additional authoring, debugging, and architectural complexity.

Do not convert every gameplay object to ECS by default. A hybrid GameObject + ECS approach can be appropriate. Ordinary interactive objects, narrative triggers, unique enemies, and scene-authored set pieces may remain GameObjects if that is simpler and performs adequately.

### C++ native plugins

Use only when:
- A profiler identifies a repeatable, important CPU bottleneck.
- A suitable algorithm cannot be made fast enough using C#, Jobs, Burst, or existing Unity systems.
- An existing native library is needed.
- The expected gain is large enough to justify native build and maintenance cost.

Costs:
- Platform-specific compilation and binaries.
- Native memory ownership and lifetime management.
- Managed/native marshalling and boundary-call overhead.
- Debugging, crash diagnosis, and CI complexity.
- Separate validation for every supported operating system and CPU architecture.

Keep native APIs coarse-grained: pass a batch of inputs, perform substantial work, and return a batch of outputs. Avoid thousands of tiny native calls per frame.

## 4. Game-system implementation guidance

| Game system | Default implementation | Performance strategy |
|---|---|---|
| Player controller | C# | Stable timestep where needed; avoid per-frame allocations |
| Enemy behavior | C# state machines / behavior architecture | Stagger decision ticks; separate sensing from decision-making |
| Enemy perception | C# | Layer masks, spatial partitioning, cached targets, bounded query frequency |
| Navigation | Unity AI Navigation / NavMesh | Avoid redundant path requests; schedule and throttle recalculations |
| Horror director | C# | Event scheduling, cooldowns, weighted choices, seeded random streams |
| Probability and encounter logic | C# runtime; Python offline validation | Test distributions and edge cases; avoid expensive Monte Carlo work every frame |
| Procedural placement | C# orchestration | Burst/Jobs for independent batch calculations if profiling warrants |
| World streaming | C# orchestration + Unity scene/addressable tooling as selected | Async loading, bounded activation work, memory budgets |
| Physics and interaction | Unity physics + C# | Layer collision matrix, query discipline, simplified colliders where appropriate |
| Inventory and resources | C# | Data-driven definitions; avoid unnecessary object churn |
| Save/load | C# | Versioned data schema, asynchronous file I/O where safe, integrity checks |
| Audio and atmosphere | Unity audio system + C# orchestration | Voice limits, spatialization cost, mixer profiling |
| UI and dialogue | Unity UI solution + C# | Avoid rebuilding large UI trees unnecessarily |
| Offline balancing tools | Python | Reproducible seeds, CSV/JSON output, plots and statistical checks |

## 5. Probability, randomness, and simulation rules

The horror director and event systems should be understandable, testable, and tunable.

- Use explicit random-number generator instances rather than relying on hidden global random state for all systems.
- Keep independent random streams for systems such as world generation, enemy decisions, loot, and director events when reproducibility matters.
- Record seeds and relevant inputs in debug logs so problematic runs can be reproduced.
- Use weighted selection only when weights are validated and normalized according to a clearly documented rule.
- Distinguish probability per frame, per second, per encounter, and per eligible event. These are not interchangeable.
- Use cooldowns, eligibility conditions, and anti-repeat rules to prevent mathematically valid but undesirable event sequences.
- Run Monte Carlo simulations offline in Python to validate encounter frequency, resource scarcity, event fairness, and edge cases.
- The runtime implementation must use the same documented rules and should be covered by deterministic unit tests where possible.
- Do not run large statistical simulations in the real-time gameplay loop.

## 6. Performance budgets and profiling

Frame-time reference budgets:

| Target | Total frame budget |
|---|---:|
| 60 FPS | 16.67 ms |
| 50 FPS | 20.00 ms |
| 30 FPS | 33.33 ms |

These are mathematical frame-time conversions, not measured project results. CPU and GPU time must be inspected separately. A game can miss its target because of rendering even when scripts are fast, or because of CPU work even when the GPU is underused.

Track at minimum:
- Main-thread CPU time and worker-thread utilization.
- GPU frame time, draw calls, batches, overdraw, shader and lighting cost.
- Managed allocations per frame and garbage-collection activity.
- Peak resident memory, asset memory, texture and audio memory.
- Scene load, activation, and unload time.
- Physics step cost, active bodies, collision queries, and contacts.
- Navigation path request count and calculation cost.
- Active AI count, perception queries, and decision frequency.
- Worst-case frame spikes, not only average FPS.

### Profiling procedure

1. Define target hardware and a minimum supported hardware profile.
2. Create a repeatable benchmark scene with representative world content.
3. Include stress cases: multiple active enemies, navigation requests, environmental effects, simultaneous audio, and world events.
4. Profile an exported development or release build on target hardware; do not rely exclusively on Editor measurements.
5. Capture CPU, GPU, memory, and allocation data with Unity Profiler and related tools.
6. Identify the dominant bottleneck before changing code.
7. Change one major variable at a time.
8. Repeat the same test and compare median, high-percentile, and worst observed frame times.
9. Keep the optimization only if the improvement is repeatable and worth the maintenance cost.
10. Re-run gameplay, save compatibility, and platform tests.

### Initial performance acceptance criteria

- Choose a primary frame-rate target after target hardware is defined.
- Use 16.67 ms as the total 60 FPS frame budget or 33.33 ms as the 30 FPS fallback reference.
- Establish explicit RAM, loading-time, and asset-memory budgets before content production scales.
- Test long sessions and scene transitions for memory growth and leaks.
- Avoid frequent spikes above the chosen frame budget during representative gameplay.
- Maintain a performance test scene and record results for each meaningful optimization.

## 7. Production architecture recommendation

Use a layered architecture:

1. **Unity engine layer:** rendering, physics, audio, animation, navigation, input, scenes, and platform lifecycle.
2. **Gameplay layer (C#):** player, enemies, interactions, objectives, inventory, survival, and progression.
3. **World simulation layer (C#):** world state, event scheduling, threat systems, procedural orchestration, and persistence.
4. **High-throughput compute layer:** C# Jobs + Burst for measured data-heavy workloads; ECS only where appropriate.
5. **Native extension layer:** C++ only for proven specialized hotspots or required native libraries.
6. **Development and analysis layer:** Python tools for simulation, balancing, validation, and reporting.

Keep dependencies flowing toward stable interfaces. Gameplay systems should not depend directly on Python tools or platform-specific native details. Native code should expose narrow interfaces and be replaceable.

## 8. Engine and language decision

The project has already selected Unity as its engine direction. The language and engine comparison should therefore be used to guide implementation, not to trigger repeated engine changes.

| Option | Strength | Cost / risk | Decision |
|---|---|---|---|
| Unity + C# | Direct engine integration, broad tooling, productive gameplay development | Requires allocation discipline and profiling | **Adopt as baseline** |
| Unity + C# Jobs/Burst | Native-optimized execution for supported data-oriented workloads; parallel work | Restrictions, job scheduling, data conversion, and debugging complexity | **Adopt selectively after profiling** |
| Unity Entities / DOTS | Efficient large-scale data-oriented simulation | More complex architecture and authoring workflow | Evaluate against measured entity workloads |
| Unity + C++ plugin | Native performance and access to native libraries | Build, ABI, memory, packaging, and interop burden | Only for proven hotspots |
| Unity + Rust library | Native performance with Rust safety model | FFI, integration, deployment, and platform validation | Experimental, not default |
| Godot or Unreal migration | Different engine capabilities and workflows | Large rewrite, retraining, asset and tooling migration | Reconsider only for a documented unmet requirement |

## 9. Implementation phases

### Phase A — Establish the baseline
- Pin the Unity Editor version and package versions in the project.
- Define target platforms and minimum hardware.
- Create a profiling scene and performance test procedure.
- Record baseline CPU, GPU, RAM, loading, and allocation metrics.

### Phase B — Build core gameplay in C#
- Implement the player loop, interactions, enemy behavior, game-state flow, and essential survival systems.
- Keep systems modular and data-driven where it improves iteration.
- Add automated tests for probability, state transitions, saving, and important gameplay rules.

### Phase C — Optimize based on evidence
- Remove avoidable work and allocations.
- Improve update cadence, spatial queries, pooling, batching, and asset use.
- Prototype Jobs/Burst for suitable high-throughput calculations.
- Evaluate ECS only if the measured workload supports the additional complexity.

### Phase D — Consider native code only when justified
- Isolate the specific bottleneck.
- Prototype the same workload in optimized C# and the candidate native implementation.
- Include interop overhead and all supported platforms in the benchmark.
- Add a native plugin only when the total-game benefit is substantial and repeatable.

### Phase E — Regression and release gates
- Run benchmark scenes on minimum and recommended hardware.
- Validate long sessions, scene transitions, save/load, and platform builds.
- Compare performance against the recorded baseline.
- Document any accepted performance trade-off.

## 10. Official references

- Unity Manual: https://docs.unity3d.com/Manual/
- Unity Profiler: https://docs.unity3d.com/Manual/Profiler.html
- Unity Profiler overview: https://docs.unity3d.com/Manual/ProfilerWindow.html
- Unity Job System: https://docs.unity3d.com/Manual/job-system.html
- Burst package documentation: https://docs.unity3d.com/Packages/com.unity.burst@latest/
- Unity Mathematics package: https://docs.unity3d.com/Packages/com.unity.mathematics@latest/
- Entities package documentation: https://docs.unity3d.com/Packages/com.unity.entities@latest/
- Unity native plug-ins: https://docs.unity3d.com/Manual/NativePlugins.html
- Microsoft C# documentation: https://learn.microsoft.com/en-us/dotnet/csharp/
- Rust language documentation: https://www.rust-lang.org/learn
- Python documentation: https://docs.python.org/3/

---

**Final engineering principle:** Use Unity + C# for the game. Profile real gameplay. Optimize algorithms and memory behavior before changing languages. Use Jobs/Burst for compatible high-throughput work, ECS selectively, Python for offline analysis, and C++/Rust only when a measured requirement justifies native integration.

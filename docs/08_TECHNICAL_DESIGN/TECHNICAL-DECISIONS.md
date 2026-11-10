# Technical Decisions

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## ADR-001 — Engine and language
Decision: Unity + C# is the production baseline. Status: current project direction. Rationale: the existing repository engineering guide selects Unity/C# and defines a Unity-native performance path. Consequence: architecture and examples use Unity scenes/prefabs, C#, ScriptableObjects, physics/audio/rendering, and Unity profiling. Python is offline tooling only. This supersedes earlier exploratory Godot + GDScript advice.

## ADR-002 — Performance
Profile first; use C# by default, Jobs/Burst selectively, ECS/DOTS only if workloads justify it, and native plugins only for measured bottlenecks. See docs/technical/PERFORMANCE_AND_LANGUAGE_GUIDANCE.md.

## ADR-003 — Simulation fidelity
Use gameplay-oriented approximations for boat motion, fish populations, fog, acoustic signals, and threats.

## ADR-004 — Failure
Failure has consequences without default deletion of long-term campaign progress.

## ADR-005 — World
Six authored regions connected by navigable routes.

## ADR-006 — Documentation
One source of truth per subsystem; substantive changes update dependent docs and tests.

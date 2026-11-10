# Technical Design Document

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

## Stack
Unity (pin Unity 6 LTS/editor version at project initialization), C# gameplay, Windows PC first. Begin with URP unless a tested requirement justifies another pipeline. Use ScriptableObjects for authored definitions and runtime models for mutable state. Python is for offline simulation and content validation.

## Architecture
Separate presentation, gameplay rules, world simulation, authored data, and persistence. Each mutable state has one owner. Prefer small components and explicit interfaces/events. Keep domain calculations testable outside scenes where practical. Use Jobs/Burst only for proven batch workloads; ECS/DOTS and native plugins are optional.

## Boundaries
World/time; vessel; fishing/stock; navigation/acoustics; creature AI/director; inventory/economy; quest/narrative; save/load; UI/audio.

## Scenes
Start with bootstrap, menu, persistent world shell, harbour content, and region scenes. Choose additive loading only after memory, lighting, and transition tests. Reusable prefabs must not assume fixed scene paths.

## Performance
Preserve docs/technical/PERFORMANCE_AND_LANGUAGE_GUIDANCE.md. 60 FPS is 16.67 ms/frame; 30 FPS is 33.33 ms/frame. These are calculations, not measured project performance. Choose target hardware and budgets before production scales.

No playable Unity project is asserted to exist yet.

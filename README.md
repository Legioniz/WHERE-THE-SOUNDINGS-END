# WHERE THE SOUNDINGS END

> **Some waters should never be disturbed.**

A single-player, 2.5D open-world fishing survival-horror game for Windows PC. The player takes up work connected to a missing fisherman in an isolated island village, where the sea's ecology, acoustic signals, and unexplained entities are interconnected.

## Current project baseline

| Area | Decision |
|---|---|
| Engine | Unity |
| Gameplay language | C# |
| First platform | Windows PC |
| Mode | Single-player |
| Presentation | 2.5D with a 3D world and controlled elevated camera |
| World | Open, organized into authored regions |
| Horror | Intense, systemic, atmosphere- and audio-led |
| Playtime goal | 20+ hours; not yet validated |
| Death | Consequences and recovery, not default campaign permadeath |
| Initial cash target | ₱0, assuming existing PC and internet |
| Production | Solo-first |

## Start here
- [Game Design Document](docs/01_GAME_DESIGN/GAME-DESIGN-DOCUMENT.md)
- [World Bible](docs/02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [Core Game Loop](docs/01_GAME_DESIGN/CORE-GAME-LOOP.md)
- [System Mathematics](docs/07_GAME_RULES_AND_BALANCING/SYSTEM-MATHEMATICS.md)
- [Technical Design](docs/08_TECHNICAL_DESIGN/TECHNICAL-DESIGN-DOCUMENT.md)
- [Roadmap](docs/13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md)
- [Documentation Index](docs/README.md)

## Engineering decision

The existing [Performance and Programming Language Guidance](docs/technical/PERFORMANCE_AND_LANGUAGE_GUIDANCE.md) selects Unity + C#. It is preserved and remains authoritative. Earlier Godot + GDScript exploration is superseded. Python is reserved for offline simulation, balancing, and validation tools, not real-time gameplay.

Documents distinguish established theory, engineering approximations, and designer tuning. Placeholder files do not imply implemented features.

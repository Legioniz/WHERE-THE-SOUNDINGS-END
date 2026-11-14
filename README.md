# WHERE THE SOUNDINGS END

> **Some waters should never be disturbed.**

A single-player, 2.5D open-world fishing survival-horror prototype for Windows PC. The player works around an isolated island village tied to a missing fisherman, where ordinary seamanship, marine ecology, acoustic signals, and unexplained events become increasingly difficult to distinguish.

## Current project baseline

| Area | Decision |
|---|---|
| Engine | Unity 6.3 LTS — 6000.3.28f1 |
| Rendering | Universal Render Pipeline 17.3.0 |
| Gameplay language | C# |
| Input | Unity Input System 1.20.1 |
| First platform | Windows 64-bit |
| Mode | Single-player |
| Presentation | 2.5D with a 3D world and elevated camera |
| Horror | Systemic, atmosphere- and audio-led |
| Production | Solo-first, AI-assisted |
| Status | Connected gameplay prototype; not a complete game |

## Getting the project running

1. Install Unity Hub and Unity Editor **6000.3.28f1** with Windows Build Support.
2. Clone this repository and open the repository root in Unity Hub.
3. Let Unity resolve packages and finish importing assets.
4. Run **Tools > WHERE THE SOUNDINGS END > Initialize Project Foundation** in the Editor.
5. Open the generated Greyhaven blockout and press Play.

The setup command creates the starter URP assets and generates three scenes when missing:

- `Greyhaven_Blockout.unity` — harbour, village, starter boat, Harbourmaster, fishing/salvage prototype, and shallow survey profile.
- `OuterShoals_SurveySandbox.unity` — deep bathymetry profile, a diel scattering layer, authored phenomenon sites, Sounding Rig, and site inspection.
- `HollowTrench_SurveySandbox.unity` — trench profile, authored brine-pool/seep/hydrate/whale-fall evidence sites, Sounding Rig, and site inspection.

The two deep-water scenes are isolated test ranges included in Build Settings. There is **no in-game region-travel system yet**. They are functional survey prototypes, not finished levels or complete deep-ocean hazard simulations.

Use **Tools > WHERE THE SOUNDINGS END > Rebuild Greyhaven Environment** after changing the Greyhaven builder. Use **Tools > WHERE THE SOUNDINGS END > Rebuild Deep Ocean Survey Sandboxes** to regenerate the two deep-water test scenes. Rebuild operations replace generated scenes and warn before discarding manual scene edits.

## Current prototype controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move / steer boat | W A S D | Left stick |
| Interact / inspect nearby site or open a dockside location | E | South face button |
| Start fishing | F | West face button |
| Start dredging | G | Right trigger |
| Sounding pulse | Q | Right shoulder |
| Fishing hook/fight / salvage pull | Space / Enter | East face button |
| Pause | Esc | Start |
| Inventory | I | North face button |
| Regional chart | M | HUD button / pointer |

The shared Harvest Beat Input System action handles fishing strikes/fight pulls and dredging beats for keyboard and gamepad. The fishing loadout is selected through the on-screen panel; tiered tackle unlocks after five and twelve landed fish. The current Greyhaven habitat/depth resolver is a local-coordinate prototype approximation, not live bathymetry. At a stationary shore approach, Interact opens the Greyhaven dockside hub: select a location, inspect its furniture, and speak with its assigned resident without leaving the vessel. Escape or gamepad Start closes the dockside menu; inventory, chart, dialogue, pause and dockside panels are mutually exclusive. The regional chart opens with M after the vessel physically visits a region; unknown regions and undiscovered sites have no map marks, labels, bearings or compass guidance. Boat movement now uses inertia and water forces instead of direct velocity changes; its buoyancy is a lightweight four-point approximation, not a full fluid simulation.

## Implemented runtime foundations

- Force-driven Rigidbody workboat movement with forward/reverse thrust, braking, speed-dependent rudder, prop wash, lateral resistance, and quadratic drag.
- Four-point spring buoyancy sampled from the same moving ocean surface used by rendering and the velocity-aware orthographic follow camera.
- Movement/action-driven sea time: the day advances while the boat is genuinely moving or the player is performing a supported harvesting or survey action.
- Multi-stage fishing encounters with selectable rod, line, hook, reel, bait and seven techniques; cast/deployment, variable bite waits, hook windows, multi-beat fish fights, tension/line failure, species-specific resistance and burst behavior, catch grading/size and spatial cargo storage.\n- Greyhaven fish profiles with habitat/depth eligibility, bait and technique preferences, per-habitat aggregate populations, catch depletion, daily recovery and six-hour habitat migration; loadout, lifetime catches and fish stocks use existing save data. Dredging remains a separate reservation-based minigame.
- Cargo hold geometry, blocked/damaged cells, item placement, salvage-site depletion, and vessel hull/engine condition.
- Day/night lighting and fog response, panic pressure, dead-calm and wrong-way-swell incidents, distant hull knocks, false navigation lights, and other restrained maritime omens.
- Rare unexplained warning flashes: a brief, non-directional broken-corner indicator with no text, sound, map mark, journal note, or gameplay effect.
- A Sounding Rig with regional bathymetry approximations, a day/night biological scattering-layer return, signal confidence, independent-ping verification, authored site discovery, and save persistence.
- Four Greyhaven residents with action-clock daily schedules, short waypoint movement, stable NPC IDs, persistent individual trust scores, seven-tier reactive Yarn dialogue, character-specific everyday facts and sayings, and trust-gated personal topics.
- A boat-operated Greyhaven dockside hub with seven location menus, furniture inspections, cargo and vessel diagnostics, existing resident dialogue, and a guest-ledger checkpoint through the current autosave service.
- Six additive static Greyhaven area scenes with distance-based preload/unload, re-entry safeguards, scene-root validation and Editor/Development Build memory/load telemetry. This is local scenery streaming, not inter-region travel.
- An event-driven acoustic listener that scores signals by range, loudness, sensitivity, and background noise, deduplicates heard events, adds environmental disturbance, and exposes a scored event for future creature perception.
- Close-range inspection of authored phenomenon sites with persistent field-note records.
- Greyhaven, Outer Shoals, and Hollow Trench survey scenes generated by Editor tooling.
- A deterministic Greyhaven maritime art pass with assembled harbour fittings, working-yard equipment, mangrove/root and reed variants, shoreline wrack, weathering, and a keeper weather station; generated assets remain static and collider-free.
- Expanded Greyhaven physical-history dressing: a collapsed lower-inlet landing, partial former-settlement foundation and exposed outer-channel wreck framing. The other five regions have a physical-remains design specification but are not yet implemented as playable areas.
- A fog-of-war regional chart that unlocks a region only after physical entry, plots the vessel locally and marks only Sounding Rig-discovered sites; it does not provide GPS, compass or inter-region routes.
- Five manual JSON save slots, backup recovery, a separate autosave file, schema migrations, and autosave lifecycle/checkpoint triggers.

## Save and load foundation

Runtime saves are stored outside the project in `Application.persistentDataPath/Saves/`:

- `slot_01.json` through `slot_05.json`
- `autosave.json`
- corresponding `.bak` recovery files

Save schema v8 preserves boat transform/velocity, cargo (including sale quantity), vessel condition, daily salvage depletion, sea time, weather intensity and remaining duration, weather cooldown, hallucination cooldown, pending seabird events, the latest omen, maritime incident countdown/caption/hush/false-light state, Sounding Rig discoveries and opportunities, acoustic hearing history, inspected site IDs, bounded per-NPC relationship-event memory, physically visited chart regions, and the integer-cent USD wallet with its processed-transaction ledger. Save participants are discovered by the scene coordinator.

The title/pause front end includes a player-facing manual save/load screen for five slots plus the separate autosave. Cross-scene restore, mid-dialogue resume, and persistence adapters for future creature/population/quest/economy systems are not complete.

## Project status and verification boundary

The repository contains a connected prototype, not a complete 20-hour game. Creature AI, population simulation, a full regional travel network, the finished market/economy loop, quest campaign, and site-specific physical hazard simulations remain incomplete. The economy now has an integrated researched design specification that connects the existing USD seafood price schedule and economy mathematics to Greyhaven's current NPCs, spatial cargo, vessel condition and save architecture; runtime transactions, fuel use, services and market UI remain unimplemented. Deep-ocean site markers are survey evidence; they do not yet implement the heat/chemistry/pressure/entanglement effects of vents, seeps, hydrates, brine pools, or wreck hazards.

The static check is:

```bash
python tools/validate_project.py
```

GitHub Actions runs this repository-integrity validator on pushes and pull requests to `main`. It checks file/configuration contracts and key integration wiring. It is **not a Unity compilation or player build**. The Edit Mode tests are in `Assets/_Project/Tests/EditMode/`; they still need to be run in Unity 6000.3.28f1, followed by Play Mode checks and a Windows build.

## Repository safeguards

The repository includes code-owner review routing, a pull-request checklist, least-privilege/pinned GitHub Actions, weekly Dependabot checks for GitHub Actions, a security reporting policy, and a repository safeguard validator.

Run **python tools/validate_safeguards.py --self-test** locally. Read [Repository Safeguards](docs/14_TESTING_AND_QUALITY/REPOSITORY-SAFEGUARDS.md) for the automated coverage, limitations, and required GitHub main-branch ruleset. Branch protection and GitHub secret-scanning settings must still be enabled in repository Settings by an administrator; committing files does not activate those account-level controls.

## Production target and agent instructions

The current production baseline targets a commercial Windows release around **November 2028**, using a 24-month planning horizon and an assumed 25–40 development hours per week. This is an estimate, not a guaranteed ship date. The aggressive scenario is 18–20 months; the conservative contingency is 30–36 months. The game targets 20+ hours of play, subject to validation through a complete vertical slice and playtesting.

- [Repository-wide coding agent instructions](AGENTS.md)
- [GitHub Copilot instructions](.github/copilot-instructions.md)
- [Development Roadmap](docs/13_PRODUCTION_MANAGEMENT/DEVELOPMENT-ROADMAP.md)
- [Production Plan](docs/13_PRODUCTION_MANAGEMENT/PRODUCTION-PLAN.md)
- [Project Milestones](docs/13_PRODUCTION_MANAGEMENT/PROJECT-MILESTONES.md)
- [Implementation Audit](docs/13_PRODUCTION_MANAGEMENT/IMPLEMENTATION-AUDIT.md)
- [Task Backlog](docs/13_PRODUCTION_MANAGEMENT/TASK-BACKLOG.md)
- [Feature Priorities](docs/13_PRODUCTION_MANAGEMENT/FEATURE-PRIORITIES.md)
- [AI-Assisted Development Workflow](docs/13_PRODUCTION_MANAGEMENT/AI-ASSISTED-DEVELOPMENT.md)

AI agents must read `AGENTS.md` before modifying the project. Planned content and documented mechanics are not implementation evidence; report only tests and builds actually executed.

## Design and technical references

- [Game Design Document](docs/01_GAME_DESIGN/GAME-DESIGN-DOCUMENT.md)
- [World Bible](docs/02_LORE_AND_WORLD/WORLD-BIBLE.md)
- [Lore Guide](docs/02_LORE_AND_WORLD/LORE.md)
- [Island History](docs/02_LORE_AND_WORLD/ISLAND-HISTORY.md) · [Timeline](docs/02_LORE_AND_WORLD/TIMELINE.md)
- [Supernatural Lore](docs/02_LORE_AND_WORLD/SUPERNATURAL-LORE.md)
- [Character Arcs](docs/03_STORY_AND_NARRATIVE/CHARACTER-ARCS.md)
- [Maritime Horror Director](docs/06_GAMEPLAY_SYSTEMS/MARITIME-HORROR-DIRECTOR.md)
- [Save and Load](docs/06_GAMEPLAY_SYSTEMS/SAVE-AND-LOAD.md)
- [Boat Movement and Hydrodynamics](docs/06_GAMEPLAY_SYSTEMS/BOAT-MOVEMENT.md)
- [Sounding Rig algorithms](docs/07_GAME_RULES_AND_BALANCING/SOUNDING-RIG-ALGORITHMS.md)
- [Technical Design](docs/08_TECHNICAL_DESIGN/TECHNICAL-DESIGN-DOCUMENT.md)
- [Deep-Ocean Survey Implementation](docs/08_TECHNICAL_DESIGN/DEEP-OCEAN-SURVEY-IMPLEMENTATION.md)
- [Testing Strategy](docs/14_TESTING_AND_QUALITY/TESTING-STRATEGY.md)
- [Documentation Index](docs/README.md)

Documents distinguish established science, engineering approximations, designer tuning, placeholders, and fictional anomalies. A design specification does not mean its system has been implemented.

## Local machine data and generated files

Unity editor output and local machine state must stay outside version control. The repository ignore rules exclude Unity `Library/`, `Temp/`, `Obj/`, `Logs/`, build folders, editor caches, local environment files, and common signing/key files. Keep Unity `.meta` files tracked because their GUIDs are part of asset identity.

Runtime saves are written under Unity's `Application.persistentDataPath/Saves/`, outside the project source tree. Do not commit player saves, local diagnostic logs, real credentials, signing certificates, private keys, or machine-specific path inventories.

If a real credential is accidentally committed, revoke or rotate it and assess all affected history. A current-branch deletion alone does not remove earlier copies. Follow the destructive-operation safeguards in `AGENTS.md`; do not rewrite shared history without an explicit, carefully scoped procedure.

### Greyhaven narrative content update — 2026-11-13

The four resident conversations now contain a connected but deliberately unresolved evidence thread: the silent bell's log history, unusual cold-centred catches, unexplained hull scoring, and an empty but untorn net. Each clue is recorded in Yarn state and can be compared through resident dialogue. Personal details further distinguish the Harbourmaster, Mara, Tomas and Inez. This is authored dialogue content; it does not mean the full quest campaign or the Choir's cause is implemented. Yarn/Unity runtime verification remains pending.

- Deterministic Greyhaven environmental artifact dressing across the harbour, village, estuary, lighthouse and outer-channel wreck; static and non-interactive by design.

- [Regional Histories and Maritime Tales](docs/05_WORLD_AND_LEVEL_DESIGN/REGIONAL-HISTORIES-AND-MARITIME-TALES.md)
# Project Milestones

> Project: WHERE THE SOUNDINGS END  
> Status: Planned milestone baseline — no future milestone is presumed complete  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-15

Milestones are evidence gates, not calendar-only achievements. Dates are approximate and should be reforecast after the repository audit and each major playtest.

| ID | Target window | Milestone | Required evidence |
|---|---|---|---|
| M0 | Nov–Dec 2026 | Production baseline and repository audit | Architecture map, verified implementation inventory, prioritized gaps, clean static checks, agreed vertical-slice path |
| M1 | Feb–Apr 2027 | Complete vertical slice | 45–90 minute playable slice in one region, expedition loop, one NPC arc, one quest, one threat, UI/audio/save integration, performance/accessibility review |
| M2 | May–Jul 2027 | Living ocean and horror systems | Documented encounter rules, creature perception/behaviour, environmental events, warning/counterplay, deterministic tests, persistence where required |
| M3 | Aug–Oct 2027 | NPC and narrative foundation complete | Major NPC/quest/story arc playable, schedule and dialogue integration, stable IDs, persistent state, reliable clue access, save/load regression |
| M4 | Nov 2027–Mar 2028 | Six-region content pass | All six regions represented by playable content, progression/travel rules, regional ecology, landmarks, hazards, and content acceptance checks |
| M5 | Apr–Jul 2028 | Content-complete game | Main campaign and approved endings playable, economy/progression balanced, 20+ hour target evaluated through playtests, full-run saves validated |
| M6 | Aug–Oct 2028 | Release candidate | Feature freeze, external testing, regression, Windows compatibility, accessibility and license review, versioned release package |
| M7 | Target: Nov 2028 | Commercial release decision | Release checklist approved, distribution package verified, storefront and legal/distribution requirements complete |

## Milestone acceptance rules

### M0 — Production baseline and audit
- Inventory existing scripts, scenes, builders, packages, tests, docs, CI workflows, save participants, and incomplete systems.
- Classify each major feature as implemented, partial, planned, deprecated, or unverified.
- Identify architecture risks, duplicated ownership, unverified scene wiring, and save compatibility risks.
- Verify repository static checks and C# syntax checks; record that neither is a Unity build.
- Approve a small vertical-slice backlog.

### M1 — Complete vertical slice
- Start a new game and complete a meaningful 45–90 minute experience.
- Prepare gear, depart, navigate, fish/investigate/survey, face risk, return, trade/repair/recover, and save/load.
- Include one authored NPC arc, one quest, one meaningful threat, environmental/audio identity, and player-facing UI.
- Demonstrate the slice in Unity Play Mode and test a Windows build.
- Record playtest feedback and decide whether the wider production scope is still viable.

### M2 — Living ocean and horror systems
- Verify event eligibility, cooldowns, world state, warnings, counterplay, encounter locks, and consequences.
- Verify wildlife is ecological and threat behaviour is intentional.
- Verify relevant systems interact correctly with action-driven time, weather, acoustic signals, and save/load.
- Test repeated events and edge cases; no required progression may rely on an unrepeatable random event.

### M3 — NPC and narrative foundation
- Verify stable IDs, daily schedules, dialogue, relationships, quest/world state, and save/load.
- Critical NPC clues must remain discoverable without exact random timing.
- Test schedule boundaries, overnight behaviour, conversation locks, pathing, and persistence.
- Do not call dialogue memory complete unless the state is explicitly stored and restored.

### M4 — Six-region content pass
- Each region has a deliberate player purpose, navigational identity, ecology, discoveries, hazards, return routes, and progression contribution.
- Verify inter-region travel and saving while changing regions.
- Check region-specific content against the World Bible and regional ecology specification.
- Separate authored design content from working Unity scenes and verified playability.

### M5 — Content-complete game
- Complete approved main story and endings.
- Test new-game, save, load, autosave, backup recovery, and migration scenarios.
- Evaluate playtime, difficulty, income, upgrade costs, and encounter pacing across multiple playthroughs.
- Fix progression blockers and data-loss defects before content lock.

### M6/M7 — Release candidate and release
- Complete regression, compatibility, accessibility, licensing, privacy/support, and clean-install checks.
- Verify the exact release build and distribution package.
- Storefront, price, rating jurisdictions, localization, and release date are decisions to be recorded; none are assumed by this plan.
- A release date is not approved until the release checklist passes.

## Current checkpoint — M0, 2026-11-13

**State: In progress; do not mark M0 complete yet.**

- Audited the tracked repository, existing runtime foundations, Editor scene-generation path, package/configuration files, seven Edit Mode test suites, GitHub Actions and repository protection constraints.
- Corrected the known undeclared `trim` material argument in the Greyhaven builder (commit `06a7004`). Unity semantic compilation is still required.
- The pre-correction main baseline's repository integrity and safeguard workflows passed. This is static/configuration evidence only.
- M0 blockers remain: clean Unity import and semantic compilation, Unity-generated package lock and stable `.meta` files, generated-scene/bootstrap verification, Edit Mode execution, Play Mode integration tests, save/restore validation and a clean Windows build.
- The repository's rulesets endpoint reports a plan restriction for this private repository. Required remote approvals/status checks are therefore not recorded as active.

- Greyhaven now has a persistent seven-location boat-operated dockside hub, resident Yarn handoff, furniture inspections, vessel/cargo diagnostics and a guest-ledger autosave checkpoint. Menu-based interior presentation is implemented; walkable 3D interiors are not.
- Greyhaven static scenery is split into six additive scenes with load/unload safeguards and development profiling telemetry. A Play Mode transition test is authored but not yet executed.
- Shared modal ownership now prevents dockside, inventory, dialogue and front-end pause overlays from overlapping. Edit Mode regression tests are authored but not yet executed.
- The source now has a revision-aware generated-area preflight: current revision 5, exact stable-ID/revision checks, explicit rebuild confirmation, and Editor tests for stale/wrong-area rejection. This remains source-level until Unity runs valid/stale scene scenarios.
- The historical archive now has six Greyhaven source records and a schema-v9 ledger. Rebuild and inspect all six generated areas; no Unity runtime verification is claimed.
- PR #8 implementation, PR #9 documentation updates and PR #10 modal-ownership/documentation reconciliation merged. GitHub Actions runs #274, #275 and #279 passed repository/C# syntax validation and safeguards. These checks are not Unity compilation or runtime evidence.


See [Implementation Audit](IMPLEMENTATION-AUDIT.md) and [Task Backlog](TASK-BACKLOG.md) for evidence and work order.

## Status updates

Update this document when a milestone changes state. Include:
- Date and commit/release identifier.
- Evidence and test results.
- Known blockers and accepted risks.
- Deferred scope and next owner.
- Explicit items not tested.

Do not mark a milestone complete because its target date arrived or because the associated design document exists.

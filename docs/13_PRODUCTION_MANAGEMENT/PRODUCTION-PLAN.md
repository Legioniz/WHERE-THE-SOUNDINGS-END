# Production Plan

> Project: WHERE THE SOUNDINGS END  
> Status: Working production baseline  
> Owner: Project lead  
> Version: 0.3  
> Last reviewed: 2026-11-13

## 1. Purpose

This document defines how the project will be produced as a commercial, solo-first, AI-assisted Unity game. The [Development Roadmap](DEVELOPMENT-ROADMAP.md) owns the calendar forecast and phase outcomes. This document owns production workflow, capacity assumptions, scope control, and quality gates.

## 2. Production constraints and assumptions

- 25–40 development hours are available in a typical week.
- One human project lead owns product direction, final approval, playtesting, and release decisions.
- AI assistants/agents may support code, architecture analysis, documentation, research synthesis, test design, data authoring, bug triage, and review.
- AI output must be inspected, integrated, tested, and licensed appropriately. Generated code is not automatically correct or production-ready.
- Unity 6.3 LTS (`6000.3.28f1`), URP 17.3.0, C#, Input System 1.20.1, Windows 64-bit first.
- Initial release is single-player. Multiplayer, console ports, mod support, and additional platforms are outside the committed baseline unless separately approved.
- The 24-month/November 2028 target is a planning estimate. The 18–20 month scenario is high risk; a 30–36 month scenario is the schedule contingency.

## 3. Production operating model

### Two-week work cycles

Use rolling two-week cycles. At the stated capacity, a cycle represents roughly 50–80 available hours before interruptions and reserve. Do not fill all theoretical capacity with new feature work.

Every cycle should have:
- One demonstrable player-facing or technical outcome.
- A short task list with dependencies and an explicit owner.
- A test plan and acceptance criteria before implementation begins.
- Documentation and save-data impact identified up front.
- A review of code, data, generated scenes, and integration effects.
- A short completion report: completed, partially completed, blocked, verified, and not verified.

Reserve a meaningful portion of capacity for bug fixing, integration, playtesting, documentation, asset pipeline work, and unexpected problems.

### Weekly allocation guide

| Workstream | Typical hours/week |
|---|---:|
| AI-assisted implementation/refactoring | 12–18 |
| Testing, debugging, and playtest review | 5–7 |
| Design and technical documentation | 3–5 |
| Art, audio, and asset pipeline | 3–5 |
| Integration, Git, and release hygiene | 2–3 |

Adjust the mix according to the current phase. During a test or release period, testing and integration may take more time than feature implementation.

## 4. Work item lifecycle

1. **Discover:** inspect repository, relevant documents, current behaviour, dependencies, and open defects.
2. **Specify:** record player value, trigger/preconditions, state transitions, feedback, failure paths, persistence, accessibility, and acceptance tests.
3. **Plan:** split the work into a smallest testable increment; identify scene, builder, data, package, and save-schema impact.
4. **Implement:** extend existing architecture; avoid duplicate owners and unrelated refactors.
5. **Verify:** run static checks, syntax checks, Edit Mode tests, Play Mode/integration checks, and builds as available and relevant.
6. **Document:** update the owning design/technical documents, docs index when needed, and development log for material progress.
7. **Review:** inspect the final diff for correctness, maintainability, performance, compatibility, licensing, and scope.
8. **Integrate:** commit focused changes and push only when authorized; verify the resulting remote revision.
9. **Report:** distinguish verified outcomes from assumptions, pending Unity checks, and remaining defects.

## 5. Quality gates

### Gate A — Repository and architecture readiness
- Unity and package versions remain pinned and documented.
- Static project validator and C# syntax checker pass.
- Runtime ownership, assembly references, scene builders, and save architecture are understood.
- No critical unresolved architecture or data-loss issue blocks the chosen vertical-slice path.

### Gate B — Vertical-slice readiness
- One region supports a complete expedition loop.
- At least one NPC arc, one quest, and one meaningful threat are playable.
- UI, audio, action-driven time, save/load, and recovery are connected. The Greyhaven title/pause/save/settings interface and dockside location hub are source-implemented; modal ownership and runtime acceptance still require Unity verification.
- Playtests confirm the core loop is understandable and enjoyable.
- Major accessibility and performance risks are identified.

### Gate C — Content production readiness
- Vertical-slice feedback is incorporated.
- Shared systems and data contracts support expansion without per-content code duplication.
- Regional content has a clear design owner, acceptance criteria, and asset/license provenance.
- Content throughput is measured before final scope is locked.

### Gate D — Content-complete readiness
- Main campaign and approved endings are playable from a clean start to completion.
- No critical missing progression, broken save migration, or data corruption is known.
- Economy, difficulty, travel, threats, and playtime have been tested across repeated runs.
- The intended 20+ hour experience is supported by real playtest data rather than an estimate alone.

### Gate E — Release readiness
- Clean Windows install and launch tested on supported hardware.
- Release candidate passes critical regression and compatibility checks.
- Accessibility, captions, audio controls, performance, save recovery, and crash handling reviewed.
- Third-party code and assets have documented licenses/permissions and required credits.
- Storefront, price, ratings, privacy/support information, and release materials are approved.

## 6. Scope control

The GDD content counts are planning ceilings. A feature is not mandatory solely because it appears in a target table. Prioritize:
1. A stable, memorable core expedition loop.
2. A complete and well-tested vertical slice.
3. Distinctive ocean, ecological, and horror systems.
4. Narrative and NPC depth that supports the world.
5. Regional breadth only after the shared systems prove expandable.
6. Polish, accessibility, and reliability before release.

Any addition that threatens the vertical slice, save integrity, or quality gates must be deferred or offset by reducing other scope. The project lead approves material changes to platform, genre, core loop, release window, or major content targets.

## 7. Current implementation and evidence discipline

The root README and owning system documents describe the current prototype. Treat code inspection and test evidence as authoritative for implementation status. This project currently has repository-level static validation and C# syntax validation in GitHub Actions; those checks do not compile Unity assemblies or execute scenes. Unity import, Edit Mode, Play Mode, performance profiling, and Windows builds must be reported separately.

No production document can declare a system complete by itself. Update status only after inspecting the implementation and recording the verification evidence.

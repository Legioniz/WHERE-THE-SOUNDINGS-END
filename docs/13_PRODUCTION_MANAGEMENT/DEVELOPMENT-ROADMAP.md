# Development Roadmap

> Project: WHERE THE SOUNDINGS END  
> Status: Working production baseline — estimate, not a release commitment  
> Owner: Project lead  
> Version: 0.2  
> Baseline date: 2026-11-11  
> Planned review: weekly; reforecast at each milestone

## 1. Production target

The project is targeting a **commercial Windows PC release** of a single-player, 2.5D open-world fishing survival-horror game.

| Planning variable | Baseline |
|---|---|
| Development model | Solo-first, AI-assisted |
| Available development capacity | 25–40 hours/week |
| Recommended planning horizon | 24 months |
| Recommended release target | November 2028 |
| Aggressive scenario | 18–20 months; high schedule and scope risk |
| Conservative scenario | 30–36 months |
| Gameplay target | 20+ hours; plan around 20–30 hours only if playtests support it |
| First platform | Windows 64-bit |
| Engine baseline | Unity 6.3 LTS / 6000.3.28f1 |
| Scope status | Planning baseline; validate through a vertical slice |

At 25–40 hours per week, the project has an approximate capacity of 1,300–2,080 hours per year, or 2,600–4,160 hours over a 24-month plan before holidays, interruptions, and capacity changes. AI assistance is expected to reduce drafting, implementation, documentation, research synthesis, and test-authoring time. It does not remove the need for architecture review, Unity compilation, playtesting, art/audio production, integration, performance work, licensing checks, and release QA.

**Schedule confidence is low until the repository audit and first vertical slice establish actual throughput.** Reforecast after those gates rather than silently compressing testing.

## 2. Product scope baseline

The high concept, core loop, differentiators, four-act story structure, and six-region geography remain defined by the Game Design Document and World Bible. This roadmap does not replace those documents.

The current planning targets from the GDD are:

| Content category | Planning target |
|---|---:|
| Main regions | 6 |
| Ordinary fish | 45–60 |
| Anomalous species | 10–15 |
| Major NPCs | 12–15, plus minor population |
| Main quests | 25–35 |
| Side quests | 30–45 |
| Locations | 35–50 |
| Major threats | 6–8 |
| Boat/equipment upgrades | 20–30 |
| Gameplay duration | 20+ hours; 20–30 hours is a validation target |

These figures are planning ceilings, not a promise that every item ships. The quality and variety of the vertical slice take priority over reaching a spreadsheet count.

The six-region scaffold is Greyhaven, The Shallows, Blackrock Reach, The Drowned Parish, The Outer Shoals, and The Hollow Trench. Greyhaven is the home hub; exact geometry, travel distances, ecological composition, and content density require prototype/playtest validation.

## 3. Twenty-four-month phase plan

The dates below are approximate planning windows counted from 11 November 2026. Existing systems must be audited and completed where appropriate; phases are not instructions to rebuild features that already exist.

| Phase | Window | Primary outcome | Exit gate |
|---|---|---|---|
| 1. Production foundation and audit | Months 1–3; Nov 2026–Jan 2027 | Verified architecture baseline, critical gaps, stable production workflow, NPC foundation and a reliable playable loop skeleton | Audit/backlog accepted; project checks pass; no unresolved blocker in the selected vertical-slice path |
| 2. Complete core gameplay loop | Months 4–6; Feb–Apr 2027 | Preparation → expedition → fishing/investigation → risk → return → sell/repair/recover/save works coherently in one region | 45–90 minute vertical slice with one NPC arc, one threat, one quest, full UI/audio/save loop and first performance/accessibility review |
| 3. Horror and living-ocean simulation | Months 7–9; May–Jul 2027 | Ocean-state reactions, creature perception/behaviour, environmental anomalies, threat scheduling, warning and counterplay rules | Repeatable encounter tests; ecological and horror systems interact without bypassing player feedback or persistence |
| 4. NPC and narrative expansion | Months 10–12; Aug–Oct 2027 | NPC routines, dialogue, relationship/ memory progression, quests, world-state reactions and first major story arc | NPC and quest state survives save/load; required clues are reliably discoverable; first arc is playable end-to-end |
| 5. Full-world content production | Months 13–17; Nov 2027–Mar 2028 | Expand all six regions, regional ecology, locations, fishing opportunities, threats, side content and travel progression | Each region has a defined purpose, safe/unsafe routes, progression, content and validated travel loop |
| 6. Integration, content completion and balance | Months 18–21; Apr–Jul 2028 | Complete campaign/endings, balance economy and progression, integrate content, validate 20+ hour experience, optimize representative worst-case loads | Content-complete build; beginning-to-end playthrough; no release-blocking progression, save, performance or stability defects |
| 7. Release preparation | Months 22–24; Aug–Nov 2028 | External testing, regression, accessibility, platform compatibility, licensing, store materials and release candidate | Release checklist signed off; distribution package and rollback/backup plan verified |

### Phase 1 — Production foundation and audit

- Audit the current repository, Unity project settings, package versions, scenes, generated-scene builders, runtime scripts, documentation, tests, and GitHub Actions.
- Identify implemented, partial, planned, duplicate, and unverified systems. Create a prioritized gap list without rewriting stable systems.
- Confirm the supported Unity editor version and document local setup and repeatable verification steps.
- Close critical integration gaps in boat handling, camera, sea clock, save/load, scene bootstrap, and generated blockouts.
- Advance the existing Greyhaven NPC foundation: stable IDs, schedules, scene interactions, Yarn dialogue, relationship persistence, test coverage, and a clear path for later navigation and quest integration.
- Establish a first end-to-end expedition skeleton and a two-week sprint/review rhythm.

### Phase 2 — Complete core gameplay loop and vertical slice

- Complete the first region's expedition loop: prepare, choose gear, depart, navigate, fish/investigate/survey, manage risk, return, sell, repair, recover, and save.
- Close the gaps between fishing, cargo, market/economy, fuel/repairs, vessel condition, UI, audio, and persistence.
- Build one 45–90 minute vertical slice in one region. It must include an NPC arc, a quest, a meaningful threat, authored sound/atmosphere, a complete UI/save loop, and a clear ending to the slice.
- Conduct playtests before approving broad content expansion.

### Phase 3 — Horror and living-ocean simulation

- Integrate time, weather, sea state, acoustic events, ecological activity, and encounter eligibility through explicit ownership and interfaces.
- Implement creature perception, threat scheduling, approach/withdrawal behaviours, warning windows, escape/counterplay, and encounter locks.
- Implement environmental horror such as sudden sea quiet, wrong-way swell, false navigation lights, hull knocks, and other authored incidents through documented trigger, cooldown, feedback, and persistence rules.
- Keep ordinary wildlife ecological; reserve lethal aggression for authored circumstances.
- Add deterministic tests for eligibility, cooldowns, state transitions, and save/restore.

### Phase 4 — NPC and narrative expansion

- Expand the current Greyhaven resident architecture rather than creating per-character one-off controllers.
- Add obstacle-aware navigation when the world geometry supports it; direct waypoint movement remains a greybox solution.
- Integrate dialogue choices with relationship/quest/world state through explicit typed data.
- Add structured NPC memories only with a documented save migration and tests.
- Build reliable critical-clue discovery so required story progression never depends on a random schedule coincidence.
- Keep human testimony, rumor, observed evidence, and confirmed supernatural fact distinct.

### Phase 5 — Full-world content production

- Build and validate the six-region travel network, region identity, navigation hazards, safe returns, and progression gates.
- Populate regions with ecologically grounded catches, wildlife, resources, salvage, landmarks, mysteries, and authored threats.
- Reuse shared systems and data-driven content definitions; avoid a separate code implementation for every fish, NPC, or event.
- Verify travel time, fuel/range, hazard escalation, economy, and content density through playtests.

### Phase 6 — Integration, completion, and balance

- Complete story acts and all approved endings, side quests, regional content, and player progression.
- Verify the full save/load lifecycle across scenes and supported schema versions.
- Balance the economy, upgrade curve, encounter frequency, recovery costs, fishing yield, and travel demands.
- Run repeated full-game playthroughs, regression suites, accessibility reviews, profiling, and failure recovery tests.
- Lock content only after critical progression and save data are stable.

### Phase 7 — Release preparation

- Freeze features; accept only release blockers and critical quality fixes.
- Run internal and external playtests, platform/driver compatibility checks, clean-install tests, and backup/restore drills.
- Complete asset/code/audio/font license review, credits, store assets, trailer, support/contact plan, and required distribution paperwork.
- Choose storefront, price, regional availability, age-rating path, and localization scope explicitly; these decisions are currently TBD.
- Produce and archive a versioned Windows release candidate and verify the exact package intended for distribution.

## 4. Weekly capacity model

The 25–40 hour weekly capacity is a planning input, not a requirement to work at maximum every week.

| Workstream | Typical hours/week |
|---|---:|
| AI-assisted implementation and refactoring | 12–18 |
| Testing, debugging, and playtest analysis | 5–7 |
| Game design and technical documentation | 3–5 |
| Art, audio, and asset pipeline | 3–5 |
| Integration, version control, and release hygiene | 2–3 |

The ranges are adjustable. Reserve capacity for defects, review, interruptions, and integration. Do not count AI-generated output as completed work until it is integrated and verified.

## 5. Scope and change control

1. The GDD and approved design documents define product intent; this roadmap defines sequence and forecast.
2. A vertical-slice result may reduce, defer, or reshape content targets.
3. Any major feature request must identify player value, dependencies, technical owner, data/save impact, test plan, and schedule cost.
4. Do not add a second system where an existing system can be extended safely.
5. Prefer a smaller coherent release over a larger unfinished world.
6. Do not sacrifice accessibility, persistence integrity, stability, licensing, or testing to preserve a calendar date.
7. Reforecast at each milestone using demonstrated throughput and actual remaining work.

## 6. Related production documents

- [Production Plan](PRODUCTION-PLAN.md)
- [Project Milestones](PROJECT-MILESTONES.md)
- [Feature Priorities](FEATURE-PRIORITIES.md)
- [Development Sprints](DEVELOPMENT-SPRINTS.md)
- [Release Plan](RELEASE-PLAN.md)
- [Risk Register](RISK-REGISTER.md)
- [Task Backlog](TASK-BACKLOG.md)
- [AI-Assisted Development Workflow](AI-ASSISTED-DEVELOPMENT.md)
- [Testing Strategy](../14_TESTING_AND_QUALITY/TESTING-STRATEGY.md)
- [Documentation Standard](../DOCUMENTATION-STANDARD.md)

The roadmap is a forecast. A planned milestone is not complete until its exit criteria are demonstrated in the project.

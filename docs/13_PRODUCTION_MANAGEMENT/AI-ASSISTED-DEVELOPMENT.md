# AI-Assisted Development Workflow

> Project: WHERE THE SOUNDINGS END  
> Status: Working production baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-11

## 1. Purpose and roles

The project uses AI assistants and coding agents as a force multiplier for a solo-first production model. The human project lead retains creative direction, scope approval, final code acceptance, hands-on gameplay evaluation, and release approval.

AI may assist with:
- repository exploration and architecture mapping;
- implementation, refactoring, and code explanation;
- game design and technical documentation;
- test-case generation, static analysis, and bug triage;
- structured content/data drafting;
- research synthesis and source comparison;
- build/release checklist preparation and change summaries.

AI does not independently prove that a feature works, replace Unity compilation, validate game feel, own commercial/legal decisions, or grant rights to third-party assets.

## 2. Project-specific context every agent must preserve

- Unity 6.3 LTS (`6000.3.28f1`), URP 17.3.0, C#, Input System 1.20.1.
- Single-player 2.5D fishing survival horror; Windows 64-bit first.
- Action-driven sea-time progression is an intentional gameplay rule.
- The ocean combines physical conditions, ecology, acoustics, authored anomalies, and player-caused disturbance.
- Horror is systemic and atmosphere/audio-led. Warnings, eligibility, cooldowns, encounter locks, counterplay, and persistence must be authored rather than improvised.
- Ordinary wildlife is ecological, not automatically hostile.
- The six-region scaffold and content counts are planning targets; the GDD and World Bible remain authoritative for product/world intent.
- The project currently contains a connected prototype. Read the README for the current implementation boundary.

## 3. Required AI work loop

### A. Orient
- Read `/AGENTS.md`, the root README, documentation index, production roadmap, priorities, milestones, AI workflow, and documentation standard.
- Inspect the current branch and repository tree.
- Read relevant source files, scene builders, ScriptableObjects/data assets, assembly definitions, save contracts, tests, and owning documents.
- Confirm whether the requested feature already exists partially or is described only in documentation.

### B. Plan
- State intended player-visible behaviour and the smallest testable implementation increment.
- Identify the system that owns each state and event.
- Identify dependencies, update cadence, lifecycle, save data, migration, scene/build effects, and failure modes.
- Define acceptance tests before coding.
- Prefer extension of existing architecture over parallel infrastructure.

### C. Implement
- Keep changes focused and consistent with namespace, assembly, naming, serialization, and folder conventions.
- Use typed data and explicit state transitions; avoid hidden coupling and magic values.
- Protect stable IDs and old save compatibility.
- Avoid unbounded per-frame work, expensive polling, global mutable state, and duplicate clocks/managers.
- Use generated-scene builders as the source when generated scenes need to change.
- Do not add package dependencies or third-party assets without a clear need and license review.

### D. Verify
- Add Edit Mode tests for deterministic logic and edge cases.
- Define Play Mode/integration tests for scene wiring, input, timing, interactions, cross-system state, and persistence.
- Run `python tools/validate_project.py` and `python tools/validate_csharp.py` when a suitable local environment is available.
- Run Unity import/compilation, Edit Mode tests, Play Mode checks, and a Windows build when the environment and task allow.
- Report unavailable verification explicitly. Never convert an unrun test into a passing test.

### E. Document and hand off
- Update the authoritative design/technical document and any dependent documents.
- Update the docs index if a document is added or renamed.
- Update the development log for meaningful implementation changes.
- Summarize files changed, behaviour added, tests run, results, unverified work, and remaining risks.
- Use focused commits and verify remote state when a push is authorized.

## 4. AI-generated research and content

- Research-based claims about marine ecology, geography, seamanship, geology, and animal behaviour require credible sources in the owning research document.
- Distinguish established science, documented practice, reported accounts, folklore, engineering approximation, designer tuning, and fictional anomalies.
- Do not fabricate citations, species ranges, prices, regulations, technical guarantees, or historical facts.
- Do not copy protected game content, dialogue, art, music, or proprietary assets. Use original expression and licensed assets.
- Drafted quests, dialogue, creatures, or lore remain proposed until reviewed and approved for canon.

## 5. Completion and status language

Use precise status terms:
- **Implemented:** code/data/assets exist and are wired into the project.
- **Tested:** a named test or manual scenario was actually run.
- **Verified:** acceptance criteria passed with evidence.
- **Partial:** some behaviour exists but documented gaps remain.
- **Planned/Proposed:** design only; implementation not established.
- **Unverified:** code may exist, but relevant execution evidence is missing.
- **Deprecated:** replaced or intentionally removed; preserve history where appropriate.

Do not say "finished", "production-ready", "bug-free", or "release-ready" based on generated code or documentation alone.

## 6. Human review checkpoints

The human project lead should review:
- changes to core loop, horror identity, world canon, region structure, progression, endings, or major content targets;
- new dependencies, third-party assets, licenses, commercial services, or release commitments;
- save-schema migrations and stable-ID changes;
- architectural changes spanning multiple systems;
- external-facing marketing claims and release decisions.

Routine implementation may proceed within the approved design and architecture. Raise a decision request only when a required product or technical choice cannot be resolved safely from existing documentation.

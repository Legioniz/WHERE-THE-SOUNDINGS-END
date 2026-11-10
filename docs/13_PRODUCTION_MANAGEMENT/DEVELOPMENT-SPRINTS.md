# Development Sprints

> Project: WHERE THE SOUNDINGS END  
> Status: Working sprint-process baseline  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

## Sprint model

Use rolling two-week development cycles. At 25–40 hours/week, the theoretical two-week capacity is approximately 50–80 hours; actual committed work must leave room for defects, integration, review, documentation, and interruptions.

Do not use story points or task counts as a substitute for demonstrated throughput. After several completed cycles, forecast using actual cycle outcomes.

## Required sprint planning fields

Every sprint record should contain:

- Sprint identifier and dates.
- One primary outcome and why it matters to the player or project.
- Scope included and explicitly excluded.
- Tasks, owners, dependencies, and acceptance criteria.
- Code, scene, save-data, documentation, asset, and package impacts.
- Automated and manual verification plan.
- Risks, blockers, and decision requests.

## Suggested cycle structure

### Start of sprint
1. Review milestone and current priority list.
2. Inspect open defects and incomplete work from the previous cycle.
3. Select one demonstrable outcome.
4. Confirm dependencies, save compatibility, scene-generation implications, and test coverage.
5. Reserve capacity for integration and unplanned fixes.

### During sprint
1. Implement in small, reviewable increments.
2. Keep the main branch in a coherent state.
3. Update tests and documentation alongside the feature.
4. Run checks after meaningful integration points.
5. Record new blockers and decisions rather than silently expanding scope.

### End of sprint
1. Run available static and syntax checks.
2. Run relevant Unity Edit Mode/Play Mode tests when Unity is available.
3. Demonstrate the outcome in the editor/build where required.
4. Review the diff, docs, save migrations, and generated scenes.
5. Record completed, partially completed, deferred, blocked, and unverified items.
6. Update the roadmap/backlog only when evidence justifies a change.

## Sprint report template

\`\`\`markdown
# Sprint YYYY-NN

- Dates:
- Primary outcome:
- Milestone supported:
- Planned capacity:
- Actual capacity:

## Completed
- [ ] Item — evidence/commit/test

## Partially completed
- Item — remaining work and blocker

## Verification
- Static validator:
- C# syntax:
- Unity import/compile:
- Edit Mode:
- Play Mode/integration:
- Windows build:

## Risks and decisions
- Risk:
- Decision required:

## Carry-over
- Item:
- Reason:
- Next action:
\`\`\`

A sprint is not complete because its dates elapsed. Report actual evidence and carry unfinished work forward explicitly.

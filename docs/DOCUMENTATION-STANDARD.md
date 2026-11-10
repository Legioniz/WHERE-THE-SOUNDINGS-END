# Documentation Standard

> Project: WHERE THE SOUNDINGS END  
> Status: Working baseline  
> Owner: Project lead  
> Version: 0.1  
> Last reviewed: 2026-11-10

1. Game identity and scope live in the GDD and linked vision/pillars.
2. Canonical setting facts live in the World Bible.
3. Each mechanic has one authoritative gameplay specification.
4. Each implemented system has one authoritative technical specification.
5. Numeric tuning values live in the owning subsystem document or data asset; avoid duplicated constants.
6. Major decisions are recorded with rationale, alternatives, consequences, and status.
7. Research-derived claims cite sources and explain the supported claim.
8. Deprecated designs are archived, never silently overwritten.

Feature specs define player promise, triggers, preconditions, inputs, state changes, outcomes, edge cases, feedback, tuning, failure handling, dependencies, accessibility, and acceptance tests.

Technical specs define responsibility, mutable-state ownership, lifecycle, interfaces, data model, update cadence, persistence, error handling, performance risks, test seams, and dependencies.

Every probability names its unit: per cast, second, trip, day, or eligible event. Separate established theory, engineering approximation, and designer-selected tuning. Use UPPER-KEBAB-CASE filenames. Update dependent documents and tests in the same change.

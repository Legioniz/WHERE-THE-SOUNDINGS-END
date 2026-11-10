# Release Plan

> Project: WHERE THE SOUNDINGS END  
> Status: Working release-planning baseline; storefront and commercial terms TBD  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-11

## 1. Release objective

Prepare a stable commercial Windows 64-bit release of the single-player game. The planning target is November 2028, subject to milestone evidence and release-readiness approval. The date is not a guarantee and must move if critical quality gates are not met.

The intended experience is 20+ hours, with a 20–30 hour planning envelope to be validated through actual playtests. Do not advertise a duration or feature that has not been verified in the release build.

## 2. Decisions still open

The following must be explicitly selected and documented before store publication:

- Storefront/distribution channel.
- Price, discounts, regional pricing, and refund/support policy.
- Age-rating jurisdictions and submission route.
- Localization and supported languages.
- Minimum/recommended PC specifications.
- Privacy/support contact and any required legal notices.
- Demo, early-access, soundtrack, or other edition strategy.

Do not infer a Steam release, price, rating, localization promise, or early-access commitment from this document.

## 3. Release gates

### Content and progression
- Main campaign, approved endings, regional progression, and critical quests are completable from a clean new game.
- No softlocks, missing required clues, broken NPC/quest state, or inaccessible mandatory locations.
- Actual playtime and progression claims are measured from representative playtests.
- Tutorials and feedback explain fishing, navigation, risk, recovery, and the Sounding Rig sufficiently for new players.

### Reliability and saves
- New game, manual save, load, autosave, backup recovery, and supported schema migrations are tested.
- Save operations handle interrupted writes and malformed files safely.
- Clean install does not depend on editor-generated local files that are absent from the release package.
- No known critical data-loss, progression-blocking, or crash defect remains.

### Performance and compatibility
- Test the release build on declared minimum and recommended hardware.
- Profile representative normal and worst-case scenes, including weather/fog, water, wildlife, effects, audio listeners, and high entity counts.
- Validate frame pacing, memory, loading, input devices, window/display modes, and resolution scaling.
- Record known limitations and ensure they do not invalidate advertised compatibility.

### Accessibility and user experience
- Critical audio information has captions or text equivalents where needed.
- Review mono playback, dynamic range, master/music/effects levels, reduced motion/flash where applicable, readable UI, remapping, and controller navigation.
- Verify pause behaviour, settings persistence, onboarding, save feedback, error messages, and recovery from common failures.

### Legal, licensing, and distribution
- Review licenses and permissions for source code, libraries, models, textures, fonts, music, sound effects, voice, and datasets.
- Include required attribution and credits.
- Confirm third-party package terms and redistribution rights.
- Complete storefront disclosures, age-rating requirements, privacy/support information, and distribution-specific requirements.
- Create an archived release manifest containing version, commit, build settings, package list, and known issues.

## 4. Release candidate workflow

1. Freeze feature development; accept only release blockers and approved critical fixes.
2. Create a versioned release-candidate branch/tag according to the approved Git workflow.
3. Build from a clean checkout using the pinned Unity editor and documented settings.
4. Run static project validation and C# syntax validation.
5. Open/import in Unity, resolve no compilation errors, and run all relevant Edit Mode tests.
6. Exercise critical scenarios in Play Mode, including a full expedition, NPC dialogue, threat encounter, save/load, autosave, and recovery.
7. Build the Windows player and perform clean-install, launch, compatibility, and regression checks.
8. Give the candidate to external testers; collect reproducible defects and prioritize them.
9. Rebuild after approved fixes and repeat impacted regression tests.
10. Verify the exact package, checksums/archive, store files, credits, and rollback/backup plan before release approval.

## 5. Marketing and store materials

Prepare only after the game has a stable representative build:
- concise store description and feature list;
- gameplay screenshots and trailer captured from the release candidate or clearly identified current build;
- accurate supported-platform and system-requirement information;
- accessibility and language statements;
- content/age disclosures where required;
- credits, support channel, and troubleshooting information.

Do not present concept art, planned features, placeholder UI, or unimplemented regions as captured gameplay.

## 6. Post-launch operations

- Maintain a known-issues list and reproducible bug-report template.
- Triage crashes, save corruption, progression blockers, and security/privacy concerns first.
- Preserve release tags, build artifacts, source commit, and build metadata.
- Use focused hotfixes and regression checks; avoid unreviewed changes to save schemas.
- Decide patch cadence and any additional content only after observing player feedback and available capacity.

## 7. Definition of release-ready

Release is ready only when all applicable release gates above have evidence, the exact Windows package has been verified, licensing and distribution requirements are cleared, and the project lead approves the release decision. A passing static validator alone is not release approval.

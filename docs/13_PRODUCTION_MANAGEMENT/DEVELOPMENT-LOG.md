## 2026-11-15 — Field-discovery cinematics, transitions and new recovery catalog

**Branch:** `feature/field-discoveries-cinematic-scenes-20261115`.

### Implemented in source
- Expanded the opening archive sequence from four to six beats and the first-voyage prologue from four to six beats, retaining the established Orison chronology and Greyhaven assignment.
- Added a brief exposure-dip transition between cinematic shots, using the existing live-camera director, unscaled timing, skip path, camera snapshot/restore and reduced-motion preference.
- Added first-recovery archival reveals for fish/specimens and salvage cargo. Each reveal contains two beats and only triggers once per stable item ID.
- Persisted discovered item IDs in the existing `ProgressSaveData.discoveredEntryIds` collection using the `field-discovery:` prefix; no schema migration was added.
- Added five fish entries (Pacific sailfish, cobia, red cornetfish, black marlin and giant oarfish) with region availability and market-class habitat/depth defaults.
- Added four archive-grade salvage objects to the existing dredge reward pool. They are not configured as ordinary sale stock.
- Added first-discovery policy tests and static validation for the new runtime policy, tests, item IDs and cinematic/save integration.

### Verification boundary
- Repository validation and C# syntax checks are expected from the pull request workflow.
- Unity semantic compilation, ScriptableObject regeneration, actual fish encounter probabilities, generated-scene presentation, Edit Mode execution, Play Mode discovery/save-load, Windows build and player launch remain unverified until run in Unity 6000.3.28f1.

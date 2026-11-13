# Save and Load

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime foundation hardened; Unity verification pending  
> Owner: Project lead  
> Version: 0.7  
> Last reviewed: 2026-11-13

## Player-facing behaviour

The game uses five independent save slots. Every slot is a separate JSON document; a save in one slot does not overwrite another slot.

The runtime storage directory is:

- Unity: Application.persistentDataPath/Saves/
- Dedicated automatic save: autosave.json
- Automatic-save backup: autosave.json.bak
- Primary file: slot_01.json through slot_05.json
- Previous-save backup: slot_01.json.bak through slot_05.json.bak
- Temporary write: slot_01.json.tmp through slot_05.json.tmp

The exact absolute directory is platform-specific. On Windows, Unity places persistent data under the current user's LocalLow application-data directory.

## Runtime implementation

The implementation lives in Assets/_Project/Scripts/Runtime/Persistence/.

- SaveGameData is the versioned snapshot DTO. It contains stable IDs and data-only records, not references to GameObjects or components.
- SaveFileService validates slots and the save DTO, enforces a 1 MiB file limit, serializes JSON, writes a temporary file, replaces the primary file, rotates the previous primary into a backup, lists slots, loads saves, recovers from a valid backup, and deletes a slot.
- SaveGameCoordinator is the scene-level entry point. It discovers active and inactive MonoBehaviours implementing ISaveParticipant, asks them to capture state, and restores those participants through SaveRestoreTransaction after data and runtime-aware checks pass.
- AutosaveController runs a five-minute unscaled-time interval after a 45-second startup delay, retries failed writes after 30 seconds, and also saves on application pause, focus loss, active-scene unload, application quit, and requested checkpoints. Lifecycle triggers are debounced by 15 seconds to avoid duplicate focus/pause/scene notifications. Explicit checkpoints and application quit bypass this debounce so important progress and the final session state are written.
- BoatPrototypeController records the Greyhaven starter boat transform and Rigidbody velocity.
- SeaStateDirector persists elapsed game time, time of day, panic, sea disturbance, weather intensity/remaining duration/next shift, hallucination cooldown, pending seabird warning and latest omen.
- FishermanAtmosphereDirector persists its incident countdown, caption/hush timers and active false-navigation-light position/remaining duration.
- AcousticWorldListener persists hearing pressure, last heard signal metadata and a bounded event-ID deduplication ledger.
- DeepOceanSiteInteractionController persists inspected phenomenon-site IDs.
- SoundingRigController persists accepted pulse count, discovered phenomenon site IDs and evaluated survey opportunity IDs.
- RegionalChartController persists physically visited region IDs and records corresponding chart unlocks; map marks remain derived from SoundingRigController discoveries.
- VesselConditionController persists hull and engine condition.
- SpatialCargoHold persists cargo placements and damaged-cell coordinates.
- SalvageSite persists committed daily pulls by stable site ID; transient reservations are refunded when actions do not complete.
- Dredging reserves daily stock and commits one pull only after the reward is placed; leaving range, pausing, disabling the controller or failing to place cargo refunds the reservation.
- Save loading validates the full DTO and runtime cargo layout before mutation, then rolls participants back if any restore fails.
- Save files are capped at 1 MiB; physics vectors, collection counts, identifiers, cargo metadata and nested progression records are validated.
- Cargo placement identifiers must be non-empty GUIDs and unique within the hold; the live cargo layout is checked before saving and restoring.
- Dialogue, inventory, harvesting, sounding and survey inspection use exclusive action claims; loading is blocked while a stateful action is active.

Typical runtime use:

1. Obtain the SaveGameCoordinator in the active scene.
2. Call SaveSlot(1) to write to slot one.
3. Call LoadSlot(1) to restore slot one.
4. Inspect SaveOperationResult.Success and Message before displaying feedback to the player.
5. Call GetSlots() to populate a future save/load screen and DeleteSlot(slot) to remove a slot.
6. Call LoadAutosave() to restore the dedicated automatic save, or RequestCheckpointAutosave(checkpointId) on AutosaveController for authored checkpoints.

The generated Greyhaven scene now contains a SaveGameCoordinator. Rebuild the Greyhaven scene in Unity after updating the environment generator so the coordinator GameObject is present.

## Restore transaction and input safety

A load is a transaction rather than a sequence of best-effort component updates:

1. Migrate and validate the complete DTO before touching live participants.
2. Enforce a 1 MiB save-file cap, bounded collection counts, finite world/player/vessel coordinates, and sane velocity/rotation ranges before deserialized physics values can reach a Rigidbody.
3. Run runtime-aware participant validation. The spatial cargo hold rejects unknown item definitions, malformed or duplicate GUID instance IDs, duplicate/out-of-range damaged cells, and placements that do not fit the authored hold.
4. Capture the current live state through the same save-participant contract, then run the runtime-aware validators against that rollback snapshot as well.
5. Apply the requested snapshot. If a participant throws, restore the captured snapshot in reverse participant order. If rollback itself fails, stop using the current session and reload the scene.

Dredging uses a transient pull reservation. Reservations reduce the number of pulls available to other in-progress actions but are not persisted as spent stock. A pull is committed only after the salvage item is successfully placed in cargo; an abandoned action, lost interaction range, full hold, controller disable, or expired day reservation releases the pull.

Dialogue, harvesting, sounding, survey-site inspection, and inventory opening use a shared exclusive interaction claim. Repeated inspection is rate-limited, and only the first discovery of a phenomenon site adds the discovery disturbance.

## Reliability rules

1. Each slot is isolated in its own file.
2. A write is first completed to a temporary file. Existing primary saves are replaced with a backup copy retained.
3. A failed write leaves the last primary save untouched whenever the platform's atomic replace operation is supported. A portable fallback retains a backup before copying the replacement.
4. Loading validates the JSON document and schema before exposing it to gameplay code.
5. If the primary file is missing or invalid, the service attempts the slot's backup and reports that recovery occurred.
6. Save schemas are versioned. Older documents are migrated through explicit migration steps; documents from a newer schema are rejected rather than silently rewritten.
7. Content references use stable string IDs. Do not serialize Unity instance IDs, object references, scene hierarchy paths, or transient array positions as content identity.
8. Save data remains local. No account, cloud sync, or network service is required.

## Current coverage and limits

Implemented now:

- Five separate save slots.
- Versioned JSON data schema.
- Migration entry point for old schema versions.
- Primary/backup/temp file handling for manual slots and the dedicated autosave.
- Timed autosaves using unscaled real time, plus lifecycle and authored-checkpoint triggers.
- Autosaves never overwrite the five manual save slots.
- Corrupt-primary fallback to backup.
- Slot listing and deletion.
- Scene-level capture/restore participant contract.
- Starter boat position, rotation, linear velocity, and angular velocity.
- Spatial cargo placements, damaged hold cells and equipment placements through SpatialCargoHold.
- Action-driven day/time, panic and sea disturbance.
- Hull/engine condition and wreck-site daily depletion.
- Greyhaven NPC stable IDs, last activity IDs, region IDs, individual relationship scores and bounded relationship-event memory IDs through NpcAgent (schema v6 relationship memory, retained in schema v7).
- Yarn Spinner float, string and bool variables through YarnDialogueSaveParticipant (requires a DialogueRunner reference).
- Sounding Rig survey pulse sequence, discovered phenomenon-site IDs, and evaluated opportunity IDs introduced in schema v5 and retained in current schema v7.

Not yet connected because the corresponding gameplay systems do not exist in the current prototype:

- Player character state and player inventory.
- Sale transactions, regional market state and freshness decay (not yet implemented).
- Dialogue presentation cursor and mid-line resume state.
- Dynamic tide/current/weather simulation and authored audio mixer state.
- Full quest, journal evidence and discovery progression; NPC dialogue now changes individual relationship scores and persists a bounded event-memory ledger, while structured world-action memories remain future work.
- Regional fish stocks, creature states, threat state, encounter cooldowns and random stream ownership.
- Persistent physical state for deep-ocean sites (brine-pool disturbance, vent/seep condition, hydrate stability, whale-fall succession); current site markers record discovery only.
- A dedicated save confirmation prompt and clear player-facing autosave status indicator. The title/pause front end now exposes five manual slots, a separate autosave load path, save/load/delete actions and operation feedback; autosave itself is implemented.
- Loading into a different scene automatically. The coordinator currently requires the target scene to be active before restoring its participants.

A field's presence in SaveGameData is a persistence contract, not proof that the related gameplay feature is implemented. Greyhaven's Editor builder generates its starter cargo definitions and configured DialogueRunner; a clean Unity bootstrap and runtime save/restore pass are still required to verify those generated references. Add a focused ISaveParticipant when each new system gains authoritative runtime state.

## Related documents

- Technical field map: docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md
- Verification: docs/14_TESTING_AND_QUALITY/SAVE-LOAD-TESTS.md
- Project architecture: docs/08_TECHNICAL_DESIGN/PROJECT-STRUCTURE.md

## Deep-ocean survey persistence

Schema v5 introduced three survey fields to WorldSaveData; these fields remain part of the current schema v6:
- soundingPulseCount: monotonic accepted-pulse sequence used in stable opportunity IDs;
- discoveredPhenomenonSiteIds: stable site IDs already found by the player;
- evaluatedSurveyOpportunityIds: recent opportunity IDs retained as a duplicate-evaluation guard.

Schema 0–3 saves migrate to v4 with zero pulses and empty discovery/opportunity collections. This preserves older campaigns without granting discoveries.

SoundingRigController is an ISaveParticipant. Its temporary sonar caption, generated AudioClip, current echo display, and temporary confirmation window are intentionally not persisted. A future physical site controller must add its own state only when the site can actually be altered by player actions.


Schema v5 also records:
- weather fog pressure, remaining fog duration and next weather-shift cooldown;
- hallucination cooldown, pending seabird event and remaining warning countdown;
- latest omen text;
- inspected phenomenon-site IDs;
- acoustic pressure, latest heard event/region/position/timestamp, and a bounded 128-event duplicate guard;
- maritime incident countdown, caption/hush durations, hush blend, and active false-navigation-light position/remaining duration.

Schema 4 saves migrate with weather defaults derived from their old weather label, a safe weather-shift delay, no pending bird event, and empty acoustic/inspection history. Existing schema 0–3 migrations remain ordered and preserved. The current v5-to-v6 migration separately initializes NPC relationship-memory ledgers and maps legacy zero relationship values to neutral 50.

Generated scenes include an AcousticWorldListener. It subscribes to AcousticEventBus, scores audibility by range/loudness/sensitivity/background noise, deduplicates heard IDs, applies a small disturbance response, and exposes a scored SignalHeard event for future creature or navigation listeners. It does not poll AudioSources.


The Harbour Inn Guest Ledger uses the existing `AutosaveController.RequestCheckpointAutosave` method and does not introduce a new save kind, file, DTO or schema migration. The dockside UI is modal-exclusive with the inventory, dialogue and front-end pause/save/settings screens; it pauses simulation while open and restores the previous time scale on close.

# Save and Load

> Project: WHERE THE SOUNDINGS END  
> Status: Runtime foundation implemented; gameplay coverage in progress  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Player-facing behaviour

The game uses five independent save slots. Every slot is a separate JSON document; a save in one slot does not overwrite another slot.

The runtime storage directory is:

- Unity: Application.persistentDataPath/Saves/
- Primary file: slot_01.json through slot_05.json
- Previous-save backup: slot_01.json.bak through slot_05.json.bak
- Temporary write: slot_01.json.tmp through slot_05.json.tmp

The exact absolute directory is platform-specific. On Windows, Unity places persistent data under the current user's LocalLow application-data directory.

## Runtime implementation

The implementation lives in Assets/_Project/Scripts/Runtime/Persistence/.

- SaveGameData is the versioned snapshot DTO. It contains stable IDs and data-only records, not references to GameObjects or components.
- SaveFileService validates slots, serializes JSON, writes a temporary file, replaces the primary file, rotates the previous primary into a backup, lists slots, loads saves, recovers from a valid backup, and deletes a slot.
- SaveGameCoordinator is the scene-level entry point. It discovers active and inactive MonoBehaviours implementing ISaveParticipant, asks them to capture state, and restores those participants on load.
- BoatPrototypeController currently implements ISaveParticipant and records the Greyhaven starter boat transform and Rigidbody velocity.

Typical runtime use:

1. Obtain the SaveGameCoordinator in the active scene.
2. Call SaveSlot(1) to write to slot one.
3. Call LoadSlot(1) to restore slot one.
4. Inspect SaveOperationResult.Success and Message before displaying feedback to the player.
5. Call GetSlots() to populate a future save/load screen and DeleteSlot(slot) to remove a slot.

The generated Greyhaven scene now contains a SaveGameCoordinator. Rebuild the Greyhaven scene in Unity after updating the environment generator so the coordinator GameObject is present.

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
- Primary/backup/temp file handling.
- Corrupt-primary fallback to backup.
- Slot listing and deletion.
- Scene-level capture/restore participant contract.
- Starter boat position, rotation, linear velocity, and angular velocity.

Not yet connected because the corresponding gameplay systems do not exist in the current prototype:

- Player character state and player inventory.
- Fishing catches, cargo and equipment runtime models.
- WorldClock instance, day/night and weather runtime state.
- Quests, journal evidence, discoveries and NPC relationships.
- Regional fish stocks, creature states, threat state, encounter cooldowns and random stream ownership.
- A player-facing save/load menu, autosave policy, save confirmation prompt and quit-save flow.
- Loading into a different scene automatically. The coordinator currently requires the target scene to be active before restoring its participants.

A field's presence in SaveGameData is a persistence contract, not proof that the related gameplay feature is implemented. Add a focused ISaveParticipant when each system gains authoritative runtime state.

## Related documents

- Technical field map: docs/08_TECHNICAL_DESIGN/SAVE-DATA-STRUCTURE.md
- Verification: docs/14_TESTING_AND_QUALITY/SAVE-LOAD-TESTS.md
- Project architecture: docs/08_TECHNICAL_DESIGN/PROJECT-STRUCTURE.md
# Save Load Tests

> Project: WHERE THE SOUNDINGS END  
> Status: Edit Mode test coverage added; Unity execution pending  
> Owner: Project lead  
> Version: 0.2  
> Last reviewed: 2026-11-10

## Automated Edit Mode tests

Test file: Assets/_Project/Tests/EditMode/SaveFileServiceTests.cs

| Test | Expected result |
|---|---|
| Save_UsesAnIndependentFileForEachSlot_AndRoundTripsData | Slot 1 and slot 2 produce different files; loaded values match the source DTO |
| Load_RecoversPreviousSaveWhenPrimaryFileIsCorrupted | Invalid primary JSON falls back to the last valid backup and marks recovery |
| Load_MigratesAnUnversionedLegacyDocument | A schema-v0 document is normalized and returned as the current schema |
| Save_RejectsSaveFilesFromANewerSchema | A future schema is rejected without creating or overwriting a file |
| Load_DoesNotRollBackFromANewerPrimaryToAnOlderBackup | A newer primary schema is rejected instead of silently loading an older backup |
| Save_RejectsInvalidCargoRecords | Missing stable item IDs and invalid quantities fail validation |
| Load_MigratesSchemaOneAndInitializesSpatialCargoAndDialogueCollections | Schema-v1 saves gain the schema-v2 cargo and dialogue collections |
| Save_RejectsInvalidSpatialCargoPlacementRecords | Invalid spatial placements fail validation |
| Delete_RemovesPrimaryAndBackupFiles | Deleting a slot removes its primary, backup and temporary files |
| Operations_RejectSlotNumbersOutsideConfiguredRange | Slot values outside 1–5 are rejected |
| Autosave_UsesDedicatedFile_AndDoesNotOverwriteManualSlots | Autosave has its own file and cannot overwrite a manual slot |
| Autosave_LoadRecoversPreviousAutosaveWhenPrimaryIsCorrupted | A corrupted autosave primary recovers from its previous backup |
| DeleteAutosave_DoesNotDeleteManualSlots | Removing autosave leaves manual saves untouched |

Tests inject a temporary directory so they do not read or modify the player's actual save folder. Cargo geometry tests live in Assets/_Project/Tests/EditMode/CargoGridGeometryTests.cs and cover shape rotation, bounds, overlap and blocked cells.

## Manual Unity checks

Run with Unity Editor 6000.3.28f1:

1. Rebuild the Greyhaven environment so the Save Game Coordinator, spatial cargo hold and starter cargo catalogue are included in the scene.
2. Enter Play Mode and call SaveSlot(1) from a temporary test harness or debugger.
3. Move the boat and build up non-zero linear velocity.
4. Call SaveSlot(1) again. Verify slot_01.json and slot_01.json.bak exist in Application.persistentDataPath/Saves/.
5. Change the boat position again and call LoadSlot(1). Verify the boat transform and Rigidbody velocity return to the saved values.
6. Save into slot 2. Verify slot_02.json exists separately and slot 1 remains unchanged.
7. Corrupt slot_02.json manually while retaining slot_02.json.bak. Load slot 2 and verify the result reports backup recovery.
8. Delete slot 1 and verify both its primary and backup are removed.
9. In Play Mode, request a checkpoint autosave and confirm autosave.json updates immediately even when a periodic autosave just occurred.
10. Stop Play Mode or close the application and confirm the final quit save is attempted even when another lifecycle autosave ran recently.
11. Confirm pause and focus-loss notifications that happen together produce no more than one write inside the 15-second debounce window.
12. Create two cargo item definitions with different shapes. Verify valid placement, 90-degree rotation, collision rejection, blocked-cell rejection, moving and removing.
13. Place multiple cargo items, save and reload. Verify instance IDs, coordinates, rotations, quantities, quality and freshness are restored.
14. Configure a Yarn Project and Dialogue Runner, change float/string/bool variables through a branching conversation, save and reload. Verify all stored variables are restored and choices reflect the restored state.

## Acceptance criteria

- A slot cannot overwrite a different slot.
- Failed serialization or file access returns a failed SaveOperationResult.
- A backup is retained when overwriting an existing valid primary save.
- Invalid primary data does not prevent a valid backup from loading.
- Newer schema versions are never rewritten by an older build.
- Restored state is applied only after the required scene is active.
- Tests use temporary storage and leave the user's real save directory untouched.

The repository's static integrity workflow does not compile C# or run Unity tests. The automated Edit Mode suite must be executed in the pinned Unity Editor before calling the system editor-verified.
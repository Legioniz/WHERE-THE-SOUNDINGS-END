# Save Load Tests

> Project: WHERE THE SOUNDINGS END  
> Status: Edit Mode test coverage added; Unity execution pending  
> Owner: Project lead  
> Version: 0.4  
> Last reviewed: 2026-11-12

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
| Load_RejectsOversizedPrimaryFileBeforeDeserialization | Files larger than 1 MiB are rejected before JSON parsing |
| Save_RejectsNonFiniteVesselPhysicsValues | Non-finite or out-of-range saved physics coordinates and velocities are rejected |
| Save_RejectsDuplicateCargoInstanceIdentifiers | Duplicate cargo instance GUIDs are rejected |
| Save_RejectsMalformedCargoInstanceIdentifiers | Cargo instance IDs must be valid GUIDs |
| Save_RejectsEmptyCargoInstanceGuid | The empty GUID is rejected as a non-identity value |
| Save_RejectsOversizedCollectionsBeforeSerialization | Save collections cannot exceed the configured entry cap |
| Save_RejectsMalformedQuestCollectionEntries | Null/malformed quest records are rejected rather than skipped |
| Restore_RollsBackEveryParticipantWhenALaterParticipantThrows | A participant failure restores the captured pre-load state |
| Restore_RejectsAnUnsafeRollbackSnapshotBeforeMutatingParticipants | Loading is refused when the pre-load state cannot be safely restored |
| SalvageReservation_DoesNotSpendStockUntilCommit | An interrupted reservation does not reduce committed daily stock |
| SalvageReservation_CannotReserveMorePullsThanDailyStock | Concurrent reservations cannot exceed the daily pull quota |
| InspectionRateLimit_RejectsRepeatedAttemptsUntilCooldownExpires | Repeated inspections are rejected until the unscaled-time cooldown expires |
| InteractionClaim_AllowsOnlyOneNewInteractionPerFrame | Only one new player interaction can claim a frame |
| InteractionClaim_BlocksWhileHarvestOrSurveyIsActive | Active harvest and survey actions block new interactions |
| SalvageReservation_IsNotPersistedAsSpentBeforeCommit | An in-flight reservation is not serialized as consumed daily stock |
| InspectionDisturbancePolicy_OnlyFirstDiscoveryAddsDisturbance | Repeat inspections add no disturbance |
| CargoValidation_RejectsDuplicateInstanceIdsBeforeRestore | Runtime cargo validation rejects duplicate identifiers before mutation |
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

15. Start dredging, save mid-action, then cancel, disable the controller, leave the wreck radius or allow the action to cross into a new day. Verify the reservation is refunded and no salvage is awarded.
16. Complete dredging with a free cargo slot and verify exactly one item reward and one committed pull. Repeat with a full hold and verify the reserved pull is returned.
17. Rapidly inspect the same site. Verify the interaction cooldown is respected and only the first inspection raises disturbance.
18. Trigger dialogue, fishing/dredging, sounding, inspection and inventory on the same frame. Verify only one interaction begins.
19. Corrupt a save with NaN/Infinity physics, duplicate cargo GUIDs and oversized collections. Verify load is rejected before live state is mutated.
20. Simulate a participant restore exception and verify the pre-load participant snapshot is restored.
21. Attempt to load a save during dialogue, sounding or dredging. Verify the coordinator rejects the load without changing the active snapshot.

## Acceptance criteria

- A slot cannot overwrite a different slot.
- Failed serialization or file access returns a failed SaveOperationResult.
- A backup is retained when overwriting an existing valid primary save.
- Invalid primary data does not prevent a valid backup from loading.
- Newer schema versions are never rewritten by an older build.
- The full save and every runtime-aware participant are validated before any live participant is mutated.
- If a participant fails during restoration, the prior live snapshot is restored; rollback failures are fatal and require a scene reload.
- Oversized files, malformed collections, non-finite physics values and duplicate cargo identities are rejected.
- Restored state is applied only after the required scene is active.
- Tests use temporary storage and leave the user's real save directory untouched.

The repository's static integrity workflow does not compile C# or run Unity tests. The automated Edit Mode suite must be executed in the pinned Unity Editor before calling the system editor-verified.
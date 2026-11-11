using System;
using System.Collections.Generic;

namespace WhereTheSoundingsEnd.Persistence
{
    /// <summary>
    /// Validates all participants before applying a snapshot and restores the previous
    /// captured state if any participant fails during application.
    /// </summary>
    public static class SaveRestoreTransaction
    {
        public static bool TryApply(
            SaveGameData candidate,
            IList<ISaveParticipant> participants,
            out string error)
        {
            error = null;
            if (participants == null)
            {
                error = "The save participants could not be discovered.";
                return false;
            }

            if (!SaveGameDataMigration.TryPrepare(candidate, out error))
                return false;

            for (int index = 0; index < participants.Count; index++)
            {
                ISaveParticipant participant = participants[index];
                if (participant == null) continue;

                ISaveDataValidator validator = participant as ISaveDataValidator;
                if (validator == null) continue;

                try
                {
                    if (!validator.TryValidateSaveData(candidate, out error))
                    {
                        if (string.IsNullOrWhiteSpace(error))
                            error = "A gameplay component rejected the save data.";
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    error = "A gameplay component could not validate the save data: " + exception.Message;
                    return false;
                }
            }

            SaveGameData rollback = SaveGameData.CreateNew(0, candidate.sceneId);
            try
            {
                for (int index = 0; index < participants.Count; index++)
                {
                    if (participants[index] != null)
                        participants[index].CaptureSaveData(rollback);
                }
            }
            catch (Exception exception)
            {
                error = "Loading was cancelled because the current gameplay state could not be protected: " +
                        exception.Message;
                return false;
            }

            if (!SaveGameDataMigration.TryPrepare(rollback, out string rollbackValidationError))
            {
                error = "Loading was cancelled because the current gameplay state is not safe to snapshot: " +
                        rollbackValidationError;
                return false;
            }

            for (int index = 0; index < participants.Count; index++)
            {
                ISaveParticipant participant = participants[index];
                ISaveDataValidator validator = participant as ISaveDataValidator;
                if (validator == null) continue;

                try
                {
                    if (!validator.TryValidateSaveData(rollback, out error))
                    {
                        if (string.IsNullOrWhiteSpace(error))
                            error = "A gameplay component rejected the pre-load rollback snapshot.";
                        error = "Loading was cancelled because the previous gameplay state could not be safely restored: " +
                                error;
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    error = "Loading was cancelled because the previous gameplay state could not be validated: " +
                            exception.Message;
                    return false;
                }
            }

            try
            {
                for (int index = 0; index < participants.Count; index++)
                {
                    if (participants[index] != null)
                        participants[index].RestoreSaveData(candidate);
                }

                return true;
            }
            catch (Exception restoreException)
            {
                List<string> rollbackErrors = new List<string>();
                for (int index = participants.Count - 1; index >= 0; index--)
                {
                    if (participants[index] == null) continue;

                    try
                    {
                        participants[index].RestoreSaveData(rollback);
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackErrors.Add(rollbackException.Message);
                    }
                }

                if (rollbackErrors.Count == 0)
                {
                    error = "A gameplay component failed during restore. The previous gameplay state was restored: " +
                            restoreException.Message;
                }
                else
                {
                    error = "Restore failed and rollback reported additional errors. Do not continue the session; " +
                            "reload the scene before retrying. Restore: " + restoreException.Message +
                            " Rollback: " + string.Join(" | ", rollbackErrors.ToArray());
                }

                return false;
            }
        }
    }
}
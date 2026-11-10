using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhereTheSoundingsEnd.Persistence
{
    /// <summary>
    /// Scene-level entry point for manual saves and the dedicated autosave.
    /// Gameplay components implement ISaveParticipant to contribute their state.
    /// </summary>
    public sealed class SaveGameCoordinator : MonoBehaviour
    {
        private SaveFileService _saveFiles;

        private SaveFileService SaveFiles
        {
            get
            {
                if (_saveFiles == null) _saveFiles = new SaveFileService();
                return _saveFiles;
            }
        }

        public SaveOperationResult SaveSlot(int slotNumber)
        {
            SaveGameData data;
            SaveOperationResult failure;
            if (!TryCaptureSnapshot(slotNumber, "manual", string.Empty, out data, out failure))
                return failure;
            return SaveFiles.Save(slotNumber, data);
        }

        public SaveOperationResult SaveAutosave(string reason)
        {
            SaveGameData data;
            SaveOperationResult failure;
            if (!TryCaptureSnapshot(0, "autosave", reason, out data, out failure))
                return failure;
            return SaveFiles.SaveAutosave(data);
        }

        public SaveOperationResult LoadSlot(int slotNumber)
        {
            SaveGameData data;
            SaveOperationResult result = SaveFiles.TryLoad(slotNumber, out data);
            return result.Success ? RestoreSnapshot(data) : result;
        }

        public SaveOperationResult LoadAutosave()
        {
            SaveGameData data;
            SaveOperationResult result = SaveFiles.TryLoadAutosave(out data);
            return result.Success ? RestoreSnapshot(data) : result;
        }

        public List<SaveSlotInfo> GetSlots() { return SaveFiles.GetSlotInfos(); }
        public SaveOperationResult DeleteSlot(int slotNumber) { return SaveFiles.Delete(slotNumber); }
        public SaveOperationResult DeleteAutosave() { return SaveFiles.DeleteAutosave(); }

        private bool TryCaptureSnapshot(
            int slotNumber,
            string saveKind,
            string reason,
            out SaveGameData data,
            out SaveOperationResult failure)
        {
            data = SaveGameData.CreateNew(slotNumber, SceneManager.GetActiveScene().name);
            data.saveKind = saveKind;
            data.saveReason = reason ?? string.Empty;

            List<ISaveParticipant> participants = FindSaveParticipants();
            for (int index = 0; index < participants.Count; index++)
            {
                try
                {
                    participants[index].CaptureSaveData(data);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    data = null;
                    failure = SaveOperationResult.Failed(
                        "Saving was cancelled because a gameplay component could not provide its state: " +
                        exception.Message);
                    return false;
                }
            }

            failure = SaveOperationResult.Succeeded("Snapshot captured.");
            return true;
        }

        private SaveOperationResult RestoreSnapshot(SaveGameData data)
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(data.sceneId) &&
                !string.Equals(data.sceneId, activeScene, StringComparison.Ordinal))
            {
                return SaveOperationResult.Failed(
                    "This save belongs to scene '" + data.sceneId +
                    "'. Load that scene before restoring this save.");
            }

            List<ISaveParticipant> participants = FindSaveParticipants();
            for (int index = 0; index < participants.Count; index++)
            {
                try
                {
                    participants[index].RestoreSaveData(data);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return SaveOperationResult.Failed(
                        "The save file was read, but a gameplay component could not restore its state: " +
                        exception.Message);
                }
            }

            return SaveOperationResult.Succeeded("Save data restored.");
        }

        private static List<ISaveParticipant> FindSaveParticipants()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            List<ISaveParticipant> participants = new List<ISaveParticipant>();
            for (int index = 0; index < behaviours.Length; index++)
            {
                ISaveParticipant participant = behaviours[index] as ISaveParticipant;
                if (participant != null) participants.Add(participant);
            }

            return participants;
        }
    }

    public interface ISaveParticipant
    {
        void CaptureSaveData(SaveGameData data);
        void RestoreSaveData(SaveGameData data);
    }
}
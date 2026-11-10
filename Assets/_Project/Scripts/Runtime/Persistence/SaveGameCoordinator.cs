using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhereTheSoundingsEnd.Persistence
{
    /// <summary>
    /// Scene-level entry point for save/load. Gameplay components implement
    /// ISaveParticipant to contribute their state to the shared snapshot.
    /// </summary>
    public sealed class SaveGameCoordinator : MonoBehaviour
    {
        private SaveFileService _saveFiles;

        private SaveFileService SaveFiles
        {
            get
            {
                if (_saveFiles == null)
                {
                    _saveFiles = new SaveFileService();
                }

                return _saveFiles;
            }
        }

        public SaveOperationResult SaveSlot(int slotNumber)
        {
            SaveGameData data = SaveGameData.CreateNew(
                slotNumber,
                SceneManager.GetActiveScene().name);

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
                    return SaveOperationResult.Failed(
                        "Saving was cancelled because a gameplay component could not provide its state: " +
                        exception.Message);
                }
            }

            return SaveFiles.Save(slotNumber, data);
        }

        public SaveOperationResult LoadSlot(int slotNumber)
        {
            SaveGameData data;
            SaveOperationResult loadResult = SaveFiles.TryLoad(slotNumber, out data);
            if (!loadResult.Success)
            {
                return loadResult;
            }

            string activeScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(data.sceneId) &&
                !string.Equals(data.sceneId, activeScene, StringComparison.Ordinal))
            {
                return SaveOperationResult.Failed(
                    "This save belongs to scene '" + data.sceneId +
                    "'. Load that scene before restoring this slot.");
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

            return loadResult;
        }

        public List<SaveSlotInfo> GetSlots()
        {
            return SaveFiles.GetSlotInfos();
        }

        public SaveOperationResult DeleteSlot(int slotNumber)
        {
            return SaveFiles.Delete(slotNumber);
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
                if (participant != null)
                {
                    participants.Add(participant);
                }
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
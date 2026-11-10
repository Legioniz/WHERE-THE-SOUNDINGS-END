using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WhereTheSoundingsEnd.Persistence
{
    public struct SaveOperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public bool RecoveredFromBackup { get; private set; }

        private SaveOperationResult(bool success, string message, bool recoveredFromBackup)
        {
            Success = success;
            Message = message;
            RecoveredFromBackup = recoveredFromBackup;
        }

        public static SaveOperationResult Succeeded(string message, bool recoveredFromBackup = false)
        {
            return new SaveOperationResult(true, message, recoveredFromBackup);
        }

        public static SaveOperationResult Failed(string message)
        {
            return new SaveOperationResult(false, message, false);
        }
    }

    public sealed class SaveSlotInfo
    {
        public int SlotNumber { get; private set; }
        public bool IsOccupied { get; private set; }
        public string SaveId { get; private set; }
        public string SavedAtUtc { get; private set; }
        public string SceneId { get; private set; }
        public double TotalPlaytimeSeconds { get; private set; }
        public bool RecoveredFromBackup { get; private set; }

        internal SaveSlotInfo(int slotNumber, SaveGameData data, bool recoveredFromBackup)
        {
            SlotNumber = slotNumber;
            IsOccupied = data != null;
            SaveId = data != null ? data.saveId : string.Empty;
            SavedAtUtc = data != null ? data.savedAtUtc : string.Empty;
            SceneId = data != null ? data.sceneId : string.Empty;
            TotalPlaytimeSeconds = data != null ? data.totalPlaytimeSeconds : 0.0;
            RecoveredFromBackup = recoveredFromBackup;
        }
    }

    /// <summary>
    /// Stores one independent JSON document per save slot under persistentDataPath/Saves.
    /// The previous primary file is retained as a backup when a slot is overwritten.
    /// </summary>
    public sealed class SaveFileService
    {
        public const int MinimumSlot = 1;
        public const int MaximumSlot = 5;

        private readonly string _saveDirectory;
        private readonly object _syncRoot = new object();

        public string SaveDirectory
        {
            get { return _saveDirectory; }
        }

        public SaveFileService()
            : this(Path.Combine(Application.persistentDataPath, "Saves"))
        {
        }

        /// <summary>
        /// Directory injection is intended for automated tests and tooling.
        /// </summary>
        public SaveFileService(string saveDirectory)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
            {
                throw new ArgumentException("A save directory is required.", nameof(saveDirectory));
            }

            _saveDirectory = saveDirectory;
        }

        public string GetSaveFilePath(int slotNumber)
        {
            ValidateSlot(slotNumber);
            return Path.Combine(_saveDirectory, "slot_" + slotNumber.ToString("D2") + ".json");
        }

        public string GetAutosaveFilePath()
        {
            return Path.Combine(_saveDirectory, "autosave.json");
        }

        public SaveOperationResult Save(int slotNumber, SaveGameData data)
        {
            if (!IsValidSlot(slotNumber))
            {
                return SaveOperationResult.Failed("Save slot must be between " + MinimumSlot + " and " + MaximumSlot + ".");
            }

            return SaveToPaths(slotNumber, GetSaveFilePath(slotNumber), "slot " + slotNumber, data, "manual");
        }

        public SaveOperationResult SaveAutosave(SaveGameData data)
        {
            return SaveToPaths(0, GetAutosaveFilePath(), "autosave", data, "autosave");
        }

        private SaveOperationResult SaveToPaths(int slotNumber, string destination, string label, SaveGameData data, string saveKind)
        {
            string validationError;
            if (!SaveGameDataMigration.TryPrepare(data, out validationError))
            {
                return SaveOperationResult.Failed(validationError);
            }

            lock (_syncRoot)
            {
                string temporary = destination + ".tmp";
                string backup = destination + ".bak";

                try
                {
                    data.slotNumber = slotNumber;
                    data.saveKind = saveKind;
                    data.schemaVersion = SaveGameData.CurrentSchemaVersion;
                    data.savedAtUtc = DateTime.UtcNow.ToString("O");
                    if (string.IsNullOrWhiteSpace(data.gameVersion))
                    {
                        data.gameVersion = Application.version;
                    }

                    string json = JsonUtility.ToJson(data, true);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return SaveOperationResult.Failed("Unity could not serialize the " + label + " data.");
                    }

                    Directory.CreateDirectory(_saveDirectory);
                    byte[] bytes = new UTF8Encoding(false).GetBytes(json);

                    using (FileStream stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }

                    if (File.Exists(destination))
                    {
                        if (File.Exists(backup))
                        {
                            File.Delete(backup);
                        }

                        try
                        {
                            File.Replace(temporary, destination, backup);
                        }
                        catch (PlatformNotSupportedException)
                        {
                            ReplaceWithPortableFallback(temporary, destination, backup);
                        }
                    }
                    else
                    {
                        File.Move(temporary, destination);
                    }

                    return SaveOperationResult.Succeeded("Saved " + label + ".");
                }
                catch (Exception exception)
                {
                    TryDelete(temporary);
                    Debug.LogError("Save failed for " + label + ": " + exception);
                    return SaveOperationResult.Failed("Could not write " + label + ": " + exception.Message);
                }
            }
        }

        public SaveOperationResult TryLoad(int slotNumber, out SaveGameData data)
        {
            data = null;
            if (!IsValidSlot(slotNumber))
            {
                return SaveOperationResult.Failed("Save slot must be between " + MinimumSlot + " and " + MaximumSlot + ".");
            }

            return TryLoadFromPaths(GetSaveFilePath(slotNumber), slotNumber, "slot " + slotNumber, out data);
        }

        public SaveOperationResult TryLoadAutosave(out SaveGameData data)
        {
            return TryLoadFromPaths(GetAutosaveFilePath(), 0, "autosave", out data);
        }

        private SaveOperationResult TryLoadFromPaths(string primaryPath, int slotNumber, string label, out SaveGameData data)
        {
            data = null;
            lock (_syncRoot)
            {
                string backupPath = primaryPath + ".bak";
                string primaryError;

                if (TryReadValidFile(primaryPath, slotNumber, out data, out primaryError))
                {
                    return SaveOperationResult.Succeeded("Loaded " + label + ".");
                }

                if (primaryError != null &&
                    primaryError.IndexOf("newer game version", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return SaveOperationResult.Failed(primaryError);
                }

                string backupError;
                SaveGameData backupData;
                if (TryReadValidFile(backupPath, slotNumber, out backupData, out backupError))
                {
                    data = backupData;
                    return SaveOperationResult.Succeeded(
                        "The primary " + label + " was unavailable. Recovered from its backup.",
                        true);
                }

                data = null;
                if (!File.Exists(primaryPath) && !File.Exists(backupPath))
                {
                    return SaveOperationResult.Failed(label + " does not exist.");
                }

                return SaveOperationResult.Failed(
                    label + " could not be loaded. Primary: " + primaryError + " Backup: " + backupError);
            }
        }

        public SaveOperationResult DeleteAutosave()
        {
            lock (_syncRoot)
            {
                try
                {
                    string path = GetAutosaveFilePath();
                    TryDelete(path);
                    TryDelete(path + ".bak");
                    TryDelete(path + ".tmp");
                    return SaveOperationResult.Succeeded("Deleted autosave.");
                }
                catch (Exception exception)
                {
                    return SaveOperationResult.Failed("Could not delete autosave: " + exception.Message);
                }
            }
        }

        public List<SaveSlotInfo> GetSlotInfos()
        {
            List<SaveSlotInfo> slots = new List<SaveSlotInfo>(MaximumSlot);
            for (int slot = MinimumSlot; slot <= MaximumSlot; slot++)
            {
                SaveGameData data;
                SaveOperationResult result = TryLoad(slot, out data);
                slots.Add(new SaveSlotInfo(
                    slot,
                    result.Success ? data : null,
                    result.Success && result.RecoveredFromBackup));
            }

            return slots;
        }

        public SaveOperationResult Delete(int slotNumber)
        {
            if (!IsValidSlot(slotNumber))
            {
                return SaveOperationResult.Failed("Save slot must be between " + MinimumSlot + " and " + MaximumSlot + ".");
            }

            lock (_syncRoot)
            {
                try
                {
                    string primaryPath = GetSaveFilePath(slotNumber);
                    TryDelete(primaryPath);
                    TryDelete(primaryPath + ".bak");
                    TryDelete(primaryPath + ".tmp");

                    string corruptPattern = Path.GetFileName(primaryPath) + ".corrupt.*";
                    if (Directory.Exists(_saveDirectory))
                    {
                        string[] corruptFiles = Directory.GetFiles(_saveDirectory, corruptPattern);
                        for (int index = 0; index < corruptFiles.Length; index++)
                        {
                            TryDelete(corruptFiles[index]);
                        }
                    }

                    return SaveOperationResult.Succeeded("Deleted slot " + slotNumber + ".");
                }
                catch (Exception exception)
                {
                    return SaveOperationResult.Failed("Could not delete slot " + slotNumber + ": " + exception.Message);
                }
            }
        }

        private bool TryReadValidFile(
            string path,
            int slotNumber,
            out SaveGameData data,
            out string error)
        {
            data = null;
            error = "File does not exist.";

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                SaveGameData candidate = JsonUtility.FromJson<SaveGameData>(json);

                if (!SaveGameDataMigration.TryPrepare(candidate, out error))
                {
                    return false;
                }

                candidate.slotNumber = slotNumber;
                data = candidate;
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static void ReplaceWithPortableFallback(
            string temporary,
            string destination,
            string backup)
        {
            File.Copy(destination, backup, true);
            File.Copy(temporary, destination, true);
            File.Delete(temporary);
        }

        private static bool IsValidSlot(int slotNumber)
        {
            return slotNumber >= MinimumSlot && slotNumber <= MaximumSlot;
        }

        private static void ValidateSlot(int slotNumber)
        {
            if (!IsValidSlot(slotNumber))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotNumber),
                    "Save slot must be between " + MinimumSlot + " and " + MaximumSlot + ".");
            }
        }

        private static void TryDelete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
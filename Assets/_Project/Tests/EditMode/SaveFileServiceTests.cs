using System;
using System.IO;
using NUnit.Framework;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class SaveFileServiceTests
    {
        private string _temporaryDirectory;
        private SaveFileService _service;

        [SetUp]
        public void SetUp()
        {
            _temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "WhereTheSoundingsEnd_SaveTests",
                Guid.NewGuid().ToString("N"));
            _service = new SaveFileService(_temporaryDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_temporaryDirectory))
            {
                Directory.Delete(_temporaryDirectory, true);
            }
        }

        [Test]
        public void Save_UsesAnIndependentFileForEachSlot_AndRoundTripsData()
        {
            SaveGameData first = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            first.world.activeRegionId = "greyhaven";
            first.vessel.isPresent = true;
            first.vessel.fuel = 83.5f;

            SaveGameData second = SaveGameData.CreateNew(2, "Outer_Shoals");
            second.world.activeRegionId = "outer_shoals";
            second.totalPlaytimeSeconds = 1250.0;

            Assert.That(_service.Save(1, first).Success, Is.True);
            Assert.That(_service.Save(2, second).Success, Is.True);

            string firstPath = _service.GetSaveFilePath(1);
            string secondPath = _service.GetSaveFilePath(2);
            Assert.That(firstPath, Is.Not.EqualTo(secondPath));
            Assert.That(File.Exists(firstPath), Is.True);
            Assert.That(File.Exists(secondPath), Is.True);

            SaveGameData loaded;
            SaveOperationResult result = _service.TryLoad(1, out loaded);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(loaded.world.activeRegionId, Is.EqualTo("greyhaven"));
            Assert.That(loaded.vessel.fuel, Is.EqualTo(83.5f).Within(0.001f));
            Assert.That(loaded.slotNumber, Is.EqualTo(1));
        }

        [Test]
        public void Load_RecoversPreviousSaveWhenPrimaryFileIsCorrupted()
        {
            SaveGameData first = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            first.world.activeRegionId = "first_saved_state";
            Assert.That(_service.Save(1, first).Success, Is.True);

            SaveGameData second = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            second.world.activeRegionId = "second_saved_state";
            Assert.That(_service.Save(1, second).Success, Is.True);

            string primaryPath = _service.GetSaveFilePath(1);
            Assert.That(File.Exists(primaryPath + ".bak"), Is.True);
            File.WriteAllText(primaryPath, "{ definitely not valid json");

            SaveGameData recovered;
            SaveOperationResult result = _service.TryLoad(1, out recovered);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.RecoveredFromBackup, Is.True);
            Assert.That(recovered.world.activeRegionId, Is.EqualTo("first_saved_state"));
        }

        [Test]
        public void Load_MigratesAnUnversionedLegacyDocument()
        {
            Directory.CreateDirectory(_temporaryDirectory);
            File.WriteAllText(
                _service.GetSaveFilePath(1),
                "{\"saveId\":\"legacy-save\",\"player\":{\"health\":72.0}}");

            SaveGameData loaded;
            SaveOperationResult result = _service.TryLoad(1, out loaded);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(loaded.schemaVersion, Is.EqualTo(SaveGameData.CurrentSchemaVersion));
            Assert.That(loaded.saveId, Is.EqualTo("legacy-save"));
            Assert.That(loaded.player.health, Is.EqualTo(72.0f).Within(0.001f));
            Assert.That(loaded.inventory.cargo, Is.Not.Null);
            Assert.That(loaded.progress.quests, Is.Not.Null);
        }

        [Test]
        public void Save_RejectsSaveFilesFromANewerSchema()
        {
            SaveGameData future = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            future.schemaVersion = SaveGameData.CurrentSchemaVersion + 10;

            SaveOperationResult result = _service.Save(1, future);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("newer"));
            Assert.That(File.Exists(_service.GetSaveFilePath(1)), Is.False);
        }

        [Test]
        public void Load_DoesNotRollBackFromANewerPrimaryToAnOlderBackup()
        {
            SaveGameData first = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            first.world.activeRegionId = "older_backup";
            Assert.That(_service.Save(1, first).Success, Is.True);

            SaveGameData second = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            second.world.activeRegionId = "newer_primary";
            Assert.That(_service.Save(1, second).Success, Is.True);

            File.WriteAllText(
                _service.GetSaveFilePath(1),
                "{\"schemaVersion\":999,\"saveId\":\"future-save\"}");

            SaveGameData loaded;
            SaveOperationResult result = _service.TryLoad(1, out loaded);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("newer game version"));
            Assert.That(result.RecoveredFromBackup, Is.False);
            Assert.That(loaded, Is.Null);
        }


        [Test]
        public void Autosave_UsesDedicatedFile_AndDoesNotOverwriteManualSlots()
        {
            SaveGameData manual = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            manual.world.activeRegionId = "manual_state";
            Assert.That(_service.Save(1, manual).Success, Is.True);

            SaveGameData automatic = SaveGameData.CreateNew(0, "Greyhaven_Blockout");
            automatic.world.activeRegionId = "automatic_state";
            Assert.That(_service.SaveAutosave(automatic).Success, Is.True);

            Assert.That(File.Exists(_service.GetSaveFilePath(1)), Is.True);
            Assert.That(File.Exists(_service.GetAutosaveFilePath()), Is.True);
            Assert.That(_service.GetAutosaveFilePath(), Is.Not.EqualTo(_service.GetSaveFilePath(1)));

            SaveGameData loadedManual;
            SaveGameData loadedAutosave;
            Assert.That(_service.TryLoad(1, out loadedManual).Success, Is.True);
            Assert.That(_service.TryLoadAutosave(out loadedAutosave).Success, Is.True);
            Assert.That(loadedManual.world.activeRegionId, Is.EqualTo("manual_state"));
            Assert.That(loadedAutosave.world.activeRegionId, Is.EqualTo("automatic_state"));
            Assert.That(loadedAutosave.slotNumber, Is.Zero);
            Assert.That(loadedAutosave.saveKind, Is.EqualTo("autosave"));
        }

        [Test]
        public void Autosave_LoadRecoversPreviousAutosaveWhenPrimaryIsCorrupted()
        {
            SaveGameData first = SaveGameData.CreateNew(0, "Greyhaven_Blockout");
            first.world.activeRegionId = "previous_autosave";
            Assert.That(_service.SaveAutosave(first).Success, Is.True);

            SaveGameData second = SaveGameData.CreateNew(0, "Greyhaven_Blockout");
            second.world.activeRegionId = "latest_autosave";
            Assert.That(_service.SaveAutosave(second).Success, Is.True);

            string path = _service.GetAutosaveFilePath();
            Assert.That(File.Exists(path + ".bak"), Is.True);
            File.WriteAllText(path, "{ invalid autosave json");

            SaveGameData recovered;
            SaveOperationResult result = _service.TryLoadAutosave(out recovered);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.RecoveredFromBackup, Is.True);
            Assert.That(recovered.world.activeRegionId, Is.EqualTo("previous_autosave"));
        }

        [Test]
        public void DeleteAutosave_DoesNotDeleteManualSlots()
        {
            SaveGameData manual = SaveGameData.CreateNew(2, "Greyhaven_Blockout");
            SaveGameData automatic = SaveGameData.CreateNew(0, "Greyhaven_Blockout");
            Assert.That(_service.Save(2, manual).Success, Is.True);
            Assert.That(_service.SaveAutosave(automatic).Success, Is.True);

            Assert.That(_service.DeleteAutosave().Success, Is.True);

            SaveGameData loadedManual;
            SaveGameData loadedAutosave;
            Assert.That(_service.TryLoad(2, out loadedManual).Success, Is.True);
            Assert.That(_service.TryLoadAutosave(out loadedAutosave).Success, Is.False);
        }

        [Test]
        public void Save_RejectsInvalidCargoRecords()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.inventory.cargo.Add(new InventoryStackSaveData
            {
                itemId = string.Empty,
                quantity = 1
            });

            SaveOperationResult result = _service.Save(1, data);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("cargo"));
        }

        [Test]
        public void Delete_RemovesPrimaryAndBackupFiles()
        {
            SaveGameData first = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            Assert.That(_service.Save(1, first).Success, Is.True);

            SaveGameData second = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            Assert.That(_service.Save(1, second).Success, Is.True);

            string primaryPath = _service.GetSaveFilePath(1);
            Assert.That(File.Exists(primaryPath), Is.True);
            Assert.That(File.Exists(primaryPath + ".bak"), Is.True);

            SaveOperationResult result = _service.Delete(1);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(File.Exists(primaryPath), Is.False);
            Assert.That(File.Exists(primaryPath + ".bak"), Is.False);
        }

        [Test]
        public void Operations_RejectSlotNumbersOutsideConfiguredRange()
        {
            SaveGameData data = SaveGameData.CreateNew(0, "Greyhaven_Blockout");
            SaveOperationResult saveResult = _service.Save(0, data);

            SaveGameData loaded;
            SaveOperationResult loadResult = _service.TryLoad(6, out loaded);

            Assert.That(saveResult.Success, Is.False);
            Assert.That(loadResult.Success, Is.False);
            Assert.That(loaded, Is.Null);
        }
    }
}
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class HarbourmasterContractSaveTests
    {
        private GameObject owner;
        private HarbourmasterContractController contracts;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Harbourmaster contract save test");
            contracts = owner.AddComponent<HarbourmasterContractController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
        }

        [Test]
        public void CaptureAndRestorePreserveActiveContractAndCompletedHistory()
        {
            SetPrivateField(contracts, "activeContractIndex", 1);
            SetPrivateField(contracts, "completedMask", 1);

            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            contracts.CaptureSaveData(data);

            HarbourmasterContractController restored = owner.AddComponent<HarbourmasterContractController>();
            restored.RestoreSaveData(data);

            Assert.IsTrue(restored.HasActiveContract);
            Assert.AreEqual("Keep the Records Dry", restored.ActiveTitle);
            Assert.IsTrue(restored.IsCompleted(0));
            Assert.IsFalse(restored.IsCompleted(1));
        }

        [Test]
        public void ValidatorRejectsAnActiveContractAlreadyMarkedComplete()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = "harbourmaster.contract-board",
                value = true,
                stringValue = "1|2"
            });

            string error;
            Assert.IsFalse(contracts.TryValidateSaveData(data, out error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void MissingLegacyContractFlagRestoresAnEmptyBoard()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            SetPrivateField(contracts, "activeContractIndex", 2);
            SetPrivateField(contracts, "completedMask", 7);

            contracts.RestoreSaveData(data);

            Assert.IsFalse(contracts.HasActiveContract);
            Assert.IsFalse(contracts.IsCompleted(0));
            Assert.IsFalse(contracts.IsCompleted(1));
            Assert.IsFalse(contracts.IsCompleted(2));
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "Expected serialized field " + fieldName);
            field.SetValue(target, value);
        }
    }
}

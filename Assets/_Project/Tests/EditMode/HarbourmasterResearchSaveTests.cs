using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class HarbourmasterResearchSaveTests
    {
        private GameObject owner;
        private HarbourmasterResearchController research;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Harbourmaster research save test");
            research = owner.AddComponent<HarbourmasterResearchController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
        }

        [Test]
        public void SubmittedResearchAndPermitUnlocksPersistWithoutSchemaChange()
        {
            SetPrivateField(research, "submittedMask", 3);
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            research.CaptureSaveData(data);

            Assert.AreEqual(SaveGameData.CurrentSchemaVersion, data.schemaVersion);
            CollectionAssert.Contains(data.progress.unlockedRegionIds, "the_shallows");
            CollectionAssert.Contains(data.progress.unlockedRegionIds, "blackrock_reach");
            CollectionAssert.Contains(data.progress.unlockedRegionIds, "reef_stewardship");
            CollectionAssert.Contains(data.progress.unlockedRegionIds, "licensed_sea_cucumber");

            GameObject restoredOwner = new GameObject("Restored research board");
            try
            {
                HarbourmasterResearchController restored =
                    restoredOwner.AddComponent<HarbourmasterResearchController>();
                string error;
                Assert.IsTrue(restored.TryValidateSaveData(data, out error), error);
                restored.RestoreSaveData(data);
                Assert.IsTrue(restored.IsSubmitted(0));
                Assert.IsTrue(restored.IsSubmitted(1));
                Assert.IsFalse(restored.IsSubmitted(2));
            }
            finally
            {
                Object.DestroyImmediate(restoredOwner);
            }
        }

        [Test]
        public void ValidatorRejectsUnknownResearchBits()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = "harbourmaster.research-board",
                value = true,
                stringValue = "8"
            });
            string error;
            Assert.IsFalse(research.TryValidateSaveData(data, out error));
            Assert.IsNotEmpty(error);
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

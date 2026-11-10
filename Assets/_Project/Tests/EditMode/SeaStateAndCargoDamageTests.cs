using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Core;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class SeaStateAndCargoDamageTests
    {
        private GameObject root;
        private CargoItemDefinition cargoDefinition;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("CargoDamageTest");
            cargoDefinition = ScriptableObject.CreateInstance<CargoItemDefinition>();
            cargoDefinition.name = "Test Fish";
            var serialized = new UnityEditor.SerializedObject(cargoDefinition);
            serialized.FindProperty("itemId").stringValue = "TEST-FISH";
            serialized.FindProperty("displayName").stringValue = "Test Fish";
            serialized.FindProperty("category").enumValueIndex = (int)CargoItemCategory.Fish;
            serialized.FindProperty("occupiedCells").arraySize = 2;
            serialized.FindProperty("occupiedCells").GetArrayElementAtIndex(0).vector2IntValue = Vector2Int.zero;
            serialized.FindProperty("occupiedCells").GetArrayElementAtIndex(1).vector2IntValue = Vector2Int.right;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (cargoDefinition != null) Object.DestroyImmediate(cargoDefinition);
        }

        [TestCase(0f, false, false, false)]
        [TestCase(0.26f, false, false, true)]
        [TestCase(0f, true, false, true)]
        [TestCase(8f, true, true, false)]
        public void ActionDrivenClockPolicy_OnlyAdvancesForMovementOrActionUnlessPaused(
            float speed, bool actionActive, bool paused, bool expected)
        {
            Assert.AreEqual(expected,
                ActionDrivenClockPolicy.ShouldAdvance(speed, 0.25f, actionActive, paused));
        }

        [Test]
        public void DamageCell_DestroysOccupyingCargoAndBlocksSlot()
        {
            SpatialCargoHold hold = root.AddComponent<SpatialCargoHold>();
            hold.SetItemDefinitions(new[] { cargoDefinition });
            string instanceId;
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 2, 3, 0, 1, 1f, 1f, out instanceId));

            CargoPlacement destroyed;
            Assert.IsTrue(hold.TryDamageCell(3, 3, out destroyed));
            Assert.IsNotNull(destroyed);
            Assert.AreEqual(instanceId, destroyed.instanceId);
            Assert.IsTrue(hold.IsCellBlocked(3, 3));
            Assert.AreEqual(0, hold.Placements.Count);
        }

        [Test]
        public void RepairCell_RestoresTheDamagedSlot()
        {
            SpatialCargoHold hold = root.AddComponent<SpatialCargoHold>();
            hold.SetItemDefinitions(new[] { cargoDefinition });
            CargoPlacement ignored;
            Assert.IsTrue(hold.TryDamageCell(4, 5, out ignored));
            Assert.IsTrue(hold.TryRepairCell(4, 5));
            Assert.IsFalse(hold.IsCellBlocked(4, 5));
        }

        [Test]
        public void DamageCell_CannotDestroyQuestCriticalCargo()
        {
            var serialized = new UnityEditor.SerializedObject(cargoDefinition);
            serialized.FindProperty("isQuestCritical").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SpatialCargoHold hold = root.AddComponent<SpatialCargoHold>();
            hold.SetItemDefinitions(new[] { cargoDefinition });
            string instanceId;
            Assert.IsTrue(hold.TryPlace("TEST-FISH", 1, 1, 0, 1, 1f, 1f, out instanceId));

            CargoPlacement destroyed;
            Assert.IsFalse(hold.TryDamageCell(1, 1, out destroyed));
            Assert.AreEqual(1, hold.Placements.Count);
        }

        [Test]
        public void SaveMigration_VersionTwoAddsPanicAndDamagedCellDefaults()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.schemaVersion = 2;
            data.world.panicLevel = 0.42f;
            data.vessel.hullIntegrity = 63f;
            data.inventory.damagedCells.Add(new GridCellSaveData { x = 2, y = 4 });

            string error;
            Assert.IsTrue(SaveGameDataMigration.TryPrepare(data, out error), error);
            Assert.AreEqual(3, data.schemaVersion);
            Assert.AreEqual(0.42f, data.world.panicLevel, 0.001f);
            Assert.AreEqual(63f, data.vessel.hullIntegrity, 0.001f);
            Assert.AreEqual(1, data.inventory.damagedCells.Count);
        }
    }
}

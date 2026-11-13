using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class VesselEquipmentIntegrationTests
    {
        private GameObject owner;
        private GameObject restoredOwner;
        private SpatialCargoHold hold;
        private CargoItemDefinition engine;
        private CargoItemDefinition rod;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Vessel equipment test");
            hold = owner.AddComponent<SpatialCargoHold>();
            engine = CreateDefinition("GEAR-ENGINE-01", "Outboard Engine", 2);
            rod = CreateDefinition("GEAR-ROD-01", "Handline Rod", 1);
            hold.SetItemDefinitions(new[] { engine, rod });
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (restoredOwner != null) Object.DestroyImmediate(restoredOwner);
            if (engine != null) Object.DestroyImmediate(engine);
            if (rod != null) Object.DestroyImmediate(rod);
        }

        [Test]
        public void InstallingEngineMovesItOutOfCargoAndChangesPropulsionProfile()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit("GEAR-ENGINE-01", 1, 1f, 1f, out instanceId));
            VesselEquipmentController equipment = owner.AddComponent<VesselEquipmentController>();

            Assert.AreEqual(0.72f, equipment.EnginePowerMultiplier, 0.001f);
            string message;
            Assert.IsTrue(equipment.TryEquip("GEAR-ENGINE-01", out message), message);

            Assert.AreEqual("GEAR-ENGINE-01", equipment.EngineItemId);
            Assert.AreEqual(0, hold.GetAvailableQuantity("GEAR-ENGINE-01"));
            Assert.Greater(equipment.EnginePowerMultiplier, 1f);
            Assert.Less(equipment.FuelUseMultiplier, 1f);
        }

        [Test]
        public void EngineWearReducesConditionAndRepairRestoresItThroughWallet()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit("GEAR-ENGINE-01", 1, 1f, 1f, out instanceId));
            VesselEquipmentController equipment = owner.AddComponent<VesselEquipmentController>();
            string message;
            Assert.IsTrue(equipment.TryEquip("GEAR-ENGINE-01", out message), message);

            equipment.WearEngine(3600f);
            Assert.Less(equipment.EngineCondition, 1f);
            long quote = equipment.GetRepairQuoteCents(VesselEquipmentController.EngineSlot);
            Assert.Greater(quote, 0L);

            EconomyWalletController wallet = owner.AddComponent<EconomyWalletController>();
            long charged;
            string error;
            Assert.IsTrue(equipment.TryRepairSlot(VesselEquipmentController.EngineSlot, wallet,
                "test.engine.repair", out charged, out error), error);
            Assert.AreEqual(quote, charged);
            Assert.AreEqual(1f, equipment.EngineCondition, 0.0001f);
            Assert.IsFalse(wallet.TryApplyTransaction("test.engine.repair", -quote));
        }

        [Test]
        public void EquipmentLoadoutRoundTripsInExistingInventorySaveCollection()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit("GEAR-ROD-01", 1, 1f, 1f, out instanceId));
            VesselEquipmentController equipment = owner.AddComponent<VesselEquipmentController>();
            string message;
            Assert.IsTrue(equipment.TryEquip("GEAR-ROD-01", out message), message);
            equipment.DamageSlot(VesselEquipmentController.RodSlot, 0.25f);

            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            equipment.CaptureSaveData(data);
            Assert.AreEqual(1, data.inventory.equipment.Count);
            Assert.AreEqual(VesselEquipmentController.RodSlot, data.inventory.equipment[0].slotId);
            Assert.AreEqual(0.75f, data.inventory.equipment[0].condition, 0.001f);

            restoredOwner = new GameObject("Restored vessel equipment test");
            VesselEquipmentController restored = restoredOwner.AddComponent<VesselEquipmentController>();
            string error;
            Assert.IsTrue(restored.TryValidateSaveData(data, out error), error);
            restored.RestoreSaveData(data);
            Assert.AreEqual("GEAR-ROD-01", restored.RodItemId);
            Assert.AreEqual(0.75f, restored.RodCondition, 0.001f);
        }

        [Test]
        public void EquipmentValidatorRejectsDuplicateSlotRecords()
        {
            VesselEquipmentController equipment = owner.AddComponent<VesselEquipmentController>();
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            data.inventory.equipment.Add(new EquipmentSaveData
            {
                slotId = VesselEquipmentController.EngineSlot,
                itemId = "GEAR-ENGINE-01",
                condition = 1f
            });
            data.inventory.equipment.Add(new EquipmentSaveData
            {
                slotId = VesselEquipmentController.EngineSlot,
                itemId = "GEAR-ENGINE-01",
                condition = 0.5f
            });

            string error;
            Assert.IsFalse(equipment.TryValidateSaveData(data, out error));
            Assert.IsNotEmpty(error);
        }

        private static CargoItemDefinition CreateDefinition(string id, string name, int width)
        {
            CargoItemDefinition definition = ScriptableObject.CreateInstance<CargoItemDefinition>();
            SetPrivateField(definition, "itemId", id);
            SetPrivateField(definition, "displayName", name);
            Vector2Int[] cells = new Vector2Int[width];
            for (int i = 0; i < width; i++) cells[i] = new Vector2Int(i, 0);
            SetPrivateField(definition, "occupiedCells", cells);
            return definition;
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

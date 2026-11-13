using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class EconomyRuntimeIntegrationTests
    {
        private GameObject owner;
        private SpatialCargoHold hold;
        private CargoItemDefinition bait;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Economy runtime integration test");
            hold = owner.AddComponent<SpatialCargoHold>();
            bait = ScriptableObject.CreateInstance<CargoItemDefinition>();
            SetPrivateField(bait, "itemId", "SUP-BAIT-WORM");
            SetPrivateField(bait, "displayName", "Worm Bait");
            SetPrivateField(bait, "occupiedCells", new[] { Vector2Int.zero });
            hold.SetItemDefinitions(new[] { bait });
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (bait != null) Object.DestroyImmediate(bait);
        }

        [Test]
        public void FuelDoesNotDrainAtIdleAndConsumesOnlyUnderPropulsionLoad()
        {
            VesselFuelController fuel = owner.AddComponent<VesselFuelController>();
            float initial = fuel.FuelUnits;

            fuel.ConsumePropulsion(0f, 0f, 10f);
            Assert.AreEqual(initial, fuel.FuelUnits, 0.0001f);

            fuel.ConsumePropulsion(1f, 1f, 10f);
            Assert.Less(fuel.FuelUnits, initial);
        }

        [Test]
        public void EmptyMainTankRetainsEmergencyPropulsionReserve()
        {
            VesselFuelController fuel = owner.AddComponent<VesselFuelController>();
            SetPrivateField(fuel, "fuelUnits", 0f);

            Assert.IsTrue(fuel.CanPropel);
            float reserve = fuel.EmergencyFuelUnits;
            fuel.ConsumePropulsion(0.18f, 0.18f, 10f);
            Assert.Less(fuel.EmergencyFuelUnits, reserve);
        }

        [Test]
        public void BaitStackConsumesOneUnitWithoutRemovingRemainingStack()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit("SUP-BAIT-WORM", 5, 1f, 1f, out instanceId));
            Assert.AreEqual(5, hold.GetAvailableQuantity("SUP-BAIT-WORM"));

            Assert.IsTrue(hold.TryConsumeOne("SUP-BAIT-WORM"));

            Assert.AreEqual(4, hold.GetAvailableQuantity("SUP-BAIT-WORM"));
            Assert.AreEqual(1, hold.Placements.Count);
            Assert.AreEqual(4, hold.Placements[0].quantity);
            Assert.AreEqual(4f, hold.Placements[0].saleQuantity, 0.0001f);
        }

        [Test]
        public void LastBaitUnitRemovesOnlyItsPlacement()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit("SUP-BAIT-WORM", 1, 1f, 1f, out instanceId));
            Assert.IsTrue(hold.TryConsumeOne("SUP-BAIT-WORM"));
            Assert.AreEqual(0, hold.GetAvailableQuantity("SUP-BAIT-WORM"));
            Assert.AreEqual(0, hold.Placements.Count);
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

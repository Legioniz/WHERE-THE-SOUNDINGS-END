using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
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
        public void RefuellingRestoresEmergencyReserve()
        {
            VesselFuelController fuel = owner.AddComponent<VesselFuelController>();
            SetPrivateField(fuel, "fuelUnits", 0f);
            SetPrivateField(fuel, "emergencyFuelUnits", 0f);

            fuel.AddFuel(1f);

            Assert.AreEqual(2f, fuel.EmergencyFuelUnits, 0.0001f);
        }

        [Test]
        public void WalletListenerFailureDoesNotSuppressLaterListenersOrRollbackPayment()
        {
            EconomyWalletController wallet = owner.AddComponent<EconomyWalletController>();
            int laterListenerCalls = 0;
            wallet.TransactionApplied += (id, delta, balance) =>
                throw new InvalidOperationException("deliberate wallet listener failure");
            wallet.TransactionApplied += (id, delta, balance) => laterListenerCalls++;

            LogAssert.Expect(LogType.Warning,
                "Wallet transaction listener failed: deliberate wallet listener failure");

            Assert.IsTrue(wallet.TryApplyTransaction("test.wallet.listener-isolation", -100L));
            Assert.AreEqual(1, laterListenerCalls);
            Assert.AreEqual(2400L, wallet.WalletCents);
            Assert.IsTrue(wallet.HasProcessedTransaction("test.wallet.listener-isolation"));
        }

        [Test]
        public void FuelListenerFailureDoesNotSuppressLaterListenersOrChangeConsumedAmount()
        {
            VesselFuelController fuel = owner.AddComponent<VesselFuelController>();
            int laterListenerCalls = 0;
            float observedFuel = 0f;
            fuel.FuelConsumed += amount =>
                throw new InvalidOperationException("deliberate fuel listener failure");
            fuel.FuelConsumed += amount =>
            {
                laterListenerCalls++;
                observedFuel += amount;
            };

            LogAssert.Expect(LogType.Warning,
                "Fuel-consumption listener failed: deliberate fuel listener failure");

            float consumed = fuel.ConsumePropulsion(1f, 1f, 10f);

            Assert.Greater(consumed, 0f);
            Assert.AreEqual(1, laterListenerCalls);
            Assert.AreEqual(consumed, observedFuel, 0.000001f);
        }

        [Test]
        public void WalletRejectsDuplicateTransactionWithoutEmittingSecondEvent()
        {
            EconomyWalletController wallet = owner.AddComponent<EconomyWalletController>();
            int eventCount = 0;
            wallet.TransactionApplied += (id, delta, balance) => eventCount++;

            Assert.IsTrue(wallet.TryApplyTransaction("test.wallet.once", -100L));
            Assert.IsFalse(wallet.TryApplyTransaction("test.wallet.once", -100L));

            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(2400L, wallet.WalletCents);
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

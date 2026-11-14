using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class ExpeditionEconomyTelemetryTests
    {
        private GameObject owner;
        private GameObject restoredOwner;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Expedition telemetry test");
            owner.AddComponent<EconomyWalletController>();
            owner.AddComponent<VesselFuelController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (restoredOwner != null) Object.DestroyImmediate(restoredOwner);
        }

        [Test]
        public void TripAggregatesOnlySuccessfulWalletTransactions()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            telemetry.ObservePropulsion(1f);
            owner.GetComponent<VesselFuelController>().ConsumePropulsion(1f, 1f, 10f);

            EconomyWalletController wallet = owner.GetComponent<EconomyWalletController>();
            Assert.IsTrue(wallet.TryApplyTransaction("trip.sale", 1250L));
            Assert.IsTrue(wallet.TryApplyTransaction("trip.fuel", -350L));
            Assert.IsFalse(wallet.TryApplyTransaction("trip.fuel", -350L));

            telemetry.CompleteTrip();

            Assert.AreEqual(1, telemetry.CompletedTrips);
            Assert.AreEqual(900L, telemetry.LastTripNetCents);
            StringAssert.Contains("GROSS $12.50", telemetry.BuildSummary());
            StringAssert.Contains("COST $3.50", telemetry.BuildSummary());
            StringAssert.Contains("FUEL 0.1", telemetry.BuildSummary());
        }

        [Test]
        public void TelemetryPersistsLifetimeAndLastTripTotals()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            telemetry.ObservePropulsion(0.5f);
            telemetry.RecordCatch("FSH-001", 1.25f, true);
            telemetry.RecordCatch("FSH-035", 0.20f, false);
            owner.GetComponent<EconomyWalletController>().TryApplyTransaction("trip.reward", 500L);
            telemetry.CompleteTrip();
            StringAssert.Contains("LEGAL CATCH 50%", telemetry.BuildBalanceReport());

            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            telemetry.CaptureSaveData(data);
            ExpeditionEconomyTelemetry restored = new GameObject("Restored telemetry test")
                .AddComponent<ExpeditionEconomyTelemetry>();
            try
            {
                string error;
                Assert.IsTrue(restored.TryValidateSaveData(data, out error), error);
                restored.RestoreSaveData(data);
                Assert.AreEqual(1, restored.CompletedTrips);
                Assert.AreEqual(500L, restored.LastTripNetCents);
                StringAssert.Contains("LEGAL CATCH 50%", restored.BuildBalanceReport());
                StringAssert.Contains("Greyhaven", restored.BuildBalanceReport());
            }
            finally
            {
                Object.DestroyImmediate(restored.gameObject);
            }
        }

        [Test]
        public void InProgressTripSurvivesSaveAndReload()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            telemetry.ObservePropulsion(1f);
            owner.GetComponent<EconomyWalletController>().TryApplyTransaction("active.trip.sale", 700L);

            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            telemetry.CaptureSaveData(data);

            restoredOwner = new GameObject("Restored active trip telemetry");
            EconomyWalletController restoredWallet = restoredOwner.AddComponent<EconomyWalletController>();
            restoredOwner.AddComponent<VesselFuelController>();
            ExpeditionEconomyTelemetry restored = restoredOwner.AddComponent<ExpeditionEconomyTelemetry>();
            string error;
            Assert.IsTrue(restored.TryValidateSaveData(data, out error), error);
            restored.RestoreSaveData(data);
            Assert.IsTrue(restored.TripActive);
            StringAssert.Contains("GROSS $7.00", restored.BuildSummary());

            Assert.IsTrue(restoredWallet.TryApplyTransaction("active.trip.repair", -200L));
            StringAssert.Contains("COST $2.00", restored.BuildSummary());
        }

        [Test]
        public void TelemetryRejectsMalformedSaveRecord()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            data.progress.worldFlags.Add(new WorldFlagSaveData
            {
                flagId = "economy.expedition-telemetry",
                value = true,
                stringValue = "1|not-a-number"
            });
            string error;
            Assert.IsFalse(telemetry.TryValidateSaveData(data, out error));
            Assert.IsNotEmpty(error);
        }
    }
}

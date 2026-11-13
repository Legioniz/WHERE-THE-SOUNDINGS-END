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
        }

        [Test]
        public void TripAggregatesOnlySuccessfulWalletTransactions()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            telemetry.ObservePropulsion(1f);

            EconomyWalletController wallet = owner.GetComponent<EconomyWalletController>();
            Assert.IsTrue(wallet.TryApplyTransaction("trip.sale", 1250L));
            Assert.IsTrue(wallet.TryApplyTransaction("trip.fuel", -350L));
            Assert.IsFalse(wallet.TryApplyTransaction("trip.fuel", -350L));

            telemetry.CompleteTrip();

            Assert.AreEqual(1, telemetry.CompletedTrips);
            Assert.AreEqual(900L, telemetry.LastTripNetCents);
            StringAssert.Contains("GROSS $12.50", telemetry.BuildSummary());
            StringAssert.Contains("COST $3.50", telemetry.BuildSummary());
        }

        [Test]
        public void TelemetryPersistsLifetimeAndLastTripTotals()
        {
            ExpeditionEconomyTelemetry telemetry = owner.AddComponent<ExpeditionEconomyTelemetry>();
            telemetry.ObservePropulsion(0.5f);
            owner.GetComponent<EconomyWalletController>().TryApplyTransaction("trip.reward", 500L);
            telemetry.CompleteTrip();

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
            }
            finally
            {
                Object.DestroyImmediate(restored.gameObject);
            }
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

using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Core;
using WhereTheSoundingsEnd.World;

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
        public void ScatteringLayer_MigratesFromDeepDayDepthToShallowerNightDepth()
        {
            float dayDepth = DeepOceanSurveyMath.GetScatteringLayerDepth(12f, 520f, 190f);
            float nightDepth = DeepOceanSurveyMath.GetScatteringLayerDepth(0f, 520f, 190f);

            Assert.That(dayDepth, Is.GreaterThan(500f));
            Assert.That(nightDepth, Is.LessThan(210f));
            Assert.That(nightDepth, Is.LessThan(dayDepth));
        }

        [Test]
        public void ScatteringLayer_IsASeparateWaterColumnReturnNotTheSeabed()
        {
            Assert.IsTrue(DeepOceanSurveyMath.IsDistinctMidwaterReturn(190f, 1450f, 90f));
            Assert.IsFalse(DeepOceanSurveyMath.IsDistinctMidwaterReturn(1390f, 1450f, 90f));
            Assert.IsFalse(DeepOceanSurveyMath.IsDistinctMidwaterReturn(-1f, 1450f, 90f));
        }


        [Test]
        public void PhenomenonSite_RequiresMatchingRegionAndDetectionRadius()
        {
            GameObject siteObject = new GameObject("SurveySiteTest");
            siteObject.transform.SetParent(root.transform, false);
            DeepOceanPhenomenonSite site = siteObject.AddComponent<DeepOceanPhenomenonSite>();
            site.Configure(
                "test.outer_shoals.whale_fall",
                "the_outer_shoals",
                DeepOceanPhenomenonType.WhaleFall,
                "Test Whale Fall",
                2100f,
                40f,
                0.8f,
                "Bone-associated fauna detected.",
                "Established science");

            Assert.IsTrue(site.IsValidForRegion);
            Assert.IsTrue(site.IsDetectableFrom(new Vector3(20f, 0f, 0f), "the_outer_shoals"));
            Assert.IsFalse(site.IsDetectableFrom(new Vector3(20f, 0f, 0f), "greyhaven"));
            Assert.IsFalse(site.IsDetectableFrom(new Vector3(60f, 0f, 0f), "the_outer_shoals"));
        }



        [Test]
        public void PhenomenonRules_KeepGeologyInPlausibleRegionalDepthBands()
        {
            Assert.IsTrue(DeepOceanPhenomenonRules.IsAllowed(
                DeepOceanPhenomenonType.BrinePool, "the_hollow_trench", 2500f));
            Assert.IsFalse(DeepOceanPhenomenonRules.IsAllowed(
                DeepOceanPhenomenonType.BrinePool, "greyhaven", 18f));
            Assert.IsTrue(DeepOceanPhenomenonRules.IsAllowed(
                DeepOceanPhenomenonType.DerelictVessel, "greyhaven", 18f));
            Assert.IsFalse(DeepOceanPhenomenonRules.IsAllowed(
                DeepOceanPhenomenonType.WhaleFall, "greyhaven", 18f));
            Assert.IsTrue(DeepOceanPhenomenonRules.IsAllowed(
                DeepOceanPhenomenonType.WaterMassFront, "the_shallows", 40f));
        }

        [Test]
        public void ScatteringLayerConfirmation_RequiresSameRegionDifferentHeadingAndFreshEvidence()
        {
            Assert.IsTrue(DeepOceanSurveyMath.IsIndependentLayerObservation(
                "the_outer_shoals", "the_outer_shoals", 10f, 55f, 240.0, 600.0, 25f));
            Assert.IsFalse(DeepOceanSurveyMath.IsIndependentLayerObservation(
                "the_outer_shoals", "the_hollow_trench", 10f, 55f, 240.0, 600.0, 25f));
            Assert.IsFalse(DeepOceanSurveyMath.IsIndependentLayerObservation(
                "the_outer_shoals", "the_outer_shoals", 10f, 20f, 240.0, 600.0, 25f));
            Assert.IsFalse(DeepOceanSurveyMath.IsIndependentLayerObservation(
                "the_outer_shoals", "the_outer_shoals", 10f, 55f, 720.0, 600.0, 25f));
        }

        [Test]
        public void ReturnConfidence_SeparatesStrongSeabedEchoFromSparseLayer()
        {
            float seabed = DeepOceanSurveyMath.CalculateReturnConfidence(1f, 0.86f, 0.16f);
            float sparseLayer = DeepOceanSurveyMath.CalculateReturnConfidence(0.1f, 0.86f, 0.16f);

            Assert.That(seabed, Is.GreaterThan(sparseLayer));
            Assert.That(seabed, Is.InRange(0f, 1f));
            Assert.That(sparseLayer, Is.InRange(0f, 1f));
        }

        [Test]
        public void SurveyOpportunityKey_IsStableAcrossRepeatedEvaluation()
        {
            ulong first = DeepOceanSurveyMath.StableKey("the_outer_shoals:day:3:sounding:14");
            ulong second = DeepOceanSurveyMath.StableKey("the_outer_shoals:day:3:sounding:14");
            ulong different = DeepOceanSurveyMath.StableKey("the_outer_shoals:day:3:sounding:15");

            Assert.AreEqual(first, second);
            Assert.AreNotEqual(first, different);
        }

        [Test]
        public void RegionalSurveyProfiles_KeepDeepLayerOutOfGreyhaven()
        {
            DeepOceanSurveyRegion region = root.AddComponent<DeepOceanSurveyRegion>();

            region.ConfigureForRegion("greyhaven");
            Assert.IsFalse(region.HasScatteringLayer);
            Assert.That(region.NominalSeabedDepthMeters, Is.LessThan(30f));

            region.ConfigureForRegion("the_outer_shoals");
            Assert.IsTrue(region.HasScatteringLayer);
            Assert.That(region.NominalSeabedDepthMeters, Is.GreaterThan(1000f));
            Assert.That(region.GetLayerDepth(0f), Is.LessThan(region.GetLayerDepth(12f)));
        }

        [Test]
        public void SaveMigration_VersionTwoAddsPanicDamageAndSurveyDefaults()
        {
            SaveGameData data = SaveGameData.CreateNew(1, "Greyhaven_Blockout");
            data.schemaVersion = 2;
            data.world.discoveredPhenomenonSiteIds = null;
            data.world.evaluatedSurveyOpportunityIds = null;
            data.world.panicLevel = 0.42f;
            data.vessel.hullIntegrity = 63f;
            data.inventory.damagedCells.Add(new GridCellSaveData { x = 2, y = 4 });

            string error;
            Assert.IsTrue(SaveGameDataMigration.TryPrepare(data, out error), error);
            Assert.AreEqual(4, data.schemaVersion);
            Assert.AreEqual(0.42f, data.world.panicLevel, 0.001f);
            Assert.AreEqual(63f, data.vessel.hullIntegrity, 0.001f);
            Assert.AreEqual(1, data.inventory.damagedCells.Count);
            Assert.AreEqual(0, data.world.soundingPulseCount);
            Assert.IsNotNull(data.world.discoveredPhenomenonSiteIds);
            Assert.IsNotNull(data.world.evaluatedSurveyOpportunityIds);
        }
    }
}

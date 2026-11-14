using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class CatchLegalityCargoSaveTests
    {
        private GameObject owner;
        private GameObject restoredOwner;
        private CargoItemDefinition definition;
        private SpatialCargoHold hold;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Catch provenance cargo");
            hold = owner.AddComponent<SpatialCargoHold>();
            definition = ScriptableObject.CreateInstance<CargoItemDefinition>();
            SetPrivateField(definition, "itemId", "TEST-CATCH-PROVENANCE");
            SetPrivateField(definition, "displayName", "Test Catch");
            SetPrivateField(definition, "occupiedCells", new[] { Vector2Int.zero });
            hold.SetItemDefinitions(new[] { definition });
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (restoredOwner != null) Object.DestroyImmediate(restoredOwner);
            if (definition != null) Object.DestroyImmediate(definition);
        }

        [Test]
        public void IllegalCatchProvenanceSurvivesSaveAndRestore()
        {
            string instanceId;
            Assert.IsTrue(hold.TryPlaceFirstFit(
                "TEST-CATCH-PROVENANCE", 1, 0.8f, 1f, out instanceId, 0.12f));
            Assert.IsTrue(hold.TrySetCatchLegality(instanceId, "greyhaven",
                "StillFishing", 0.12f, false, "Below minimum landing weight."));

            SaveGameData data = SaveGameData.CreateNew(1, "greyhaven");
            hold.CaptureSaveData(data);

            restoredOwner = new GameObject("Restored catch provenance");
            SpatialCargoHold restored = restoredOwner.AddComponent<SpatialCargoHold>();
            restored.SetItemDefinitions(new[] { definition });
            string error;
            Assert.IsTrue(restored.TryValidateSaveData(data, out error), error);
            restored.RestoreSaveData(data);

            Assert.AreEqual(1, restored.Placements.Count);
            CargoPlacement placement = restored.Placements[0];
            Assert.IsFalse(placement.catchLegal);
            Assert.AreEqual("greyhaven", placement.catchRegionId);
            Assert.AreEqual("StillFishing", placement.catchTechniqueId);
            Assert.AreEqual(0.12f, placement.catchSizeKg, 0.0001f);
            Assert.AreEqual("Below minimum landing weight.", placement.catchLegalityReason);
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
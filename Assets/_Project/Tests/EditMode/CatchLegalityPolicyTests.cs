using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class CatchLegalityPolicyTests
    {
        private GameObject owner;
        private RegionalChartController chart;
        private CargoItemDefinition definition;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("Catch legality test");
            owner.AddComponent<Rigidbody>();
            chart = owner.AddComponent<RegionalChartController>();
            definition = ScriptableObject.CreateInstance<CargoItemDefinition>();
            SetPrivateField(definition, "itemId", "TEST-LEGAL-FISH");
            SetPrivateField(definition, "displayName", "Test Legal Fish");
            SetPrivateField(definition, "availableRegionIds", new[] { "greyhaven", "the_hollow_trench" });
            SetPrivateField(definition, "minimumLegalCatchWeightKg", 0.15f);
        }

        [TearDown]
        public void TearDown()
        {
            if (owner != null) Object.DestroyImmediate(owner);
            if (definition != null) Object.DestroyImmediate(definition);
        }

        [Test]
        public void StandardCatchInsideAuthoredRangeIsLegal()
        {
            CatchLegalityResult result = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.StillFishing, 0.4f, chart);

            Assert.IsTrue(result.IsLegal);
            Assert.AreEqual(CatchViolation.None, result.Violation);
            Assert.AreEqual(0L, result.FineCents);
        }

        [Test]
        public void UndersizedCatchIsFlaggedWithAuthoredFine()
        {
            CatchLegalityResult result = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.StillFishing, 0.10f, chart);

            Assert.IsFalse(result.IsLegal);
            Assert.AreEqual(CatchViolation.Undersized, result.Violation);
            Assert.AreEqual(100L, result.FineCents);
        }

        [Test]
        public void SpeciesOutsideItsAuthoredRegionIsFlagged()
        {
            CatchLegalityResult result = CatchLegalityPolicy.Evaluate(
                definition, "blackrock_reach", FishingTechnique.StillFishing, 0.4f, chart);

            Assert.IsFalse(result.IsLegal);
            Assert.AreEqual(CatchViolation.OutsideSpeciesRange, result.Violation);
        }

        [Test]
        public void HollowTrenchRequiresOuterShoalsResearchPermit()
        {
            CatchLegalityResult closed = CatchLegalityPolicy.Evaluate(
                definition, "the_hollow_trench", FishingTechnique.StillFishing, 0.4f, chart);
            Assert.IsFalse(closed.IsLegal);
            Assert.AreEqual(CatchViolation.ClosedArea, closed.Violation);

            chart.GrantChartPermit("the_outer_shoals");
            CatchLegalityResult opened = CatchLegalityPolicy.Evaluate(
                definition, "the_hollow_trench", FishingTechnique.StillFishing, 0.4f, chart);
            Assert.IsTrue(opened.IsLegal);
        }

        [Test]
        public void ProtectedSpeciesRequiresPermitAndAuthorizedTechnique()
        {
            SetPrivateField(definition, "protectedCatch", true);
            SetPrivateField(definition, "requiredPermitId", "reef_stewardship");
            SetPrivateField(definition, "allowedTechniqueMask", 1 << (int)FishingTechnique.Handlining);
            SetPrivateField(definition, "requiredHookId", "circle-hook");

            CatchLegalityResult missingPermit = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.Handlining, 0.4f, chart);
            Assert.AreEqual(CatchViolation.ProtectedSpecies, missingPermit.Violation);

            chart.GrantChartPermit("reef_stewardship");
            CatchLegalityResult wrongGear = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.Handlining, 0.4f, chart, "standard-hook");
            Assert.AreEqual(CatchViolation.ProhibitedGear, wrongGear.Violation);

            CatchLegalityResult wrongTechnique = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.Trolling, 0.4f, chart, "circle-hook");
            Assert.AreEqual(CatchViolation.ProhibitedTechnique, wrongTechnique.Violation);

            CatchLegalityResult authorized = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.Handlining, 0.4f, chart, "circle-hook");
            Assert.IsTrue(authorized.IsLegal);
        }

        [Test]
        public void ContractOnlyHarvestRequiresItsSpecificPermit()
        {
            SetPrivateField(definition, "requiredPermitId", "licensed_sea_cucumber");
            CatchLegalityResult missing = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.BottomFishing, 0.4f, chart);
            Assert.AreEqual(CatchViolation.MissingHarvestPermit, missing.Violation);

            chart.GrantChartPermit("licensed_sea_cucumber");
            CatchLegalityResult permitted = CatchLegalityPolicy.Evaluate(
                definition, "greyhaven", FishingTechnique.BottomFishing, 0.4f, chart);
            Assert.IsTrue(permitted.IsLegal);
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
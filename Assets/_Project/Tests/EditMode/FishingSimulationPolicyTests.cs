using NUnit.Framework;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class FishingSimulationPolicyTests
    {
        [TestCase(FishPreferredTechnique.Any, FishingTechnique.Handlining, true)]
        [TestCase(FishPreferredTechnique.BottomFishing, FishingTechnique.BottomFishing, true)]
        [TestCase(FishPreferredTechnique.BottomFishing, FishingTechnique.Trolling, false)]
        [TestCase(FishPreferredTechnique.FloatFishing, FishingTechnique.FloatFishing, true)]
        public void TechniqueCompatibility_RespectsSpeciesPreference(
            FishPreferredTechnique preferred, FishingTechnique actual, bool expected)
        {
            Assert.That(FishingSimulationPolicy.IsTechniqueCompatible(preferred, actual), Is.EqualTo(expected));
        }

        [TestCase(0.12f, 0.5f, 1f, 0.1075f)]
        [TestCase(0.12f, 2f, 1f, 0.07f)]
        [TestCase(0.12f, 0.5f, 1.5f, 0.125f)]
        public void TimingWindow_ShrinksForStrongerFishAndWidensWithBetterControl(
            float baseWindow, float resistance, float control, float expected)
        {
            Assert.That(FishingSimulationPolicy.CalculateTimingWindow(baseWindow, resistance, control),
                Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void TimingWindow_AlwaysHasAFairMinimumAndMaximum()
        {
            Assert.That(FishingSimulationPolicy.CalculateTimingWindow(0.02f, 3f, 0.5f), Is.EqualTo(0.055f));
            Assert.That(FishingSimulationPolicy.CalculateTimingWindow(0.4f, 0f, 2f), Is.EqualTo(0.18f));
        }

        [TestCase(0, 0f, 0)]
        [TestCase(20, 0.1f, 2)]
        [TestCase(8, 0.01f, 1)]
        [TestCase(5, 0.5f, 1)]
        public void MigrationAmount_IsBoundedAndPopulationPreserving(int population, float rate, int expected)
        {
            Assert.That(FishingSimulationPolicy.CalculateMigrationAmount(population, rate), Is.EqualTo(expected));
            Assert.That(FishingSimulationPolicy.CalculateMigrationAmount(population, rate), Is.LessThanOrEqualTo(population / 3));
        }

        [Test]
        public void CatchQuality_IncreasesWithSuccessfulBeats()
        {
            float poor = FishingSimulationPolicy.CalculateCatchQuality(1, 0.4f);
            float good = FishingSimulationPolicy.CalculateCatchQuality(7, 0.4f);
            Assert.That(good, Is.GreaterThan(poor));
            Assert.That(good, Is.InRange(0f, 1f));
        }

        [Test]
        public void ExcessiveLineTension_ReducesCatchQuality()
        {
            float controlled = FishingSimulationPolicy.CalculateCatchQuality(6, 0.4f);
            float overloaded = FishingSimulationPolicy.CalculateCatchQuality(6, 1.2f);
            Assert.That(overloaded, Is.LessThan(controlled));
        }

        [TestCase(4f, 0f, 10f, true)]
        [TestCase(12f, 0f, 10f, false)]
        [TestCase(0f, 0f, 10f, true)]
        public void DepthEligibility_UsesAuthoredSpeciesRange(
            float depth, float minimum, float maximum, bool expected)
        {
            Assert.That(FishingSimulationPolicy.IsWithinDepth(depth, minimum, maximum), Is.EqualTo(expected));
        }

        [Test]
        public void CatchSize_StaysWithinSpeciesSpecificBounds()
        {
            float size = FishingSimulationPolicy.CalculateCatchSize(0.5f, 4f, 0.8f, 0.9f);
            Assert.That(size, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(size, Is.LessThanOrEqualTo(4f));
        }
    }
}

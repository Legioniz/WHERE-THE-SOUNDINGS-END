using NUnit.Framework;
using UnityEngine;
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
        [TestCase(FishPreferredTechnique.SeaweedHarvesting, FishingTechnique.SeaweedHarvesting, true)]
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

        [TestCase(0, 0f, 0f, 0)]
        [TestCase(20, 0.1f, 0.5f, 2)]
        [TestCase(8, 0.01f, 0.05f, 1)]
        [TestCase(8, 0.01f, 0.5f, 0)]
        [TestCase(2, 0.03f, 0.02f, 1)]
        [TestCase(2, 0.03f, 0.5f, 0)]
        public void MigrationAmount_UsesFractionalProbabilityAndPreservesPopulation(
            int population, float rate, float roll, int expected)
        {
            int amount = FishingSimulationPolicy.CalculateMigrationAmount(population, rate, roll);
            Assert.That(amount, Is.EqualTo(expected));
            Assert.That(amount, Is.LessThanOrEqualTo(Mathf.Max(1, population / 3)));
            Assert.That(amount, Is.LessThan(population));
        }

        [TestCase(0f, 0f, 0.025f)]
        [TestCase(1f, 0f, 0.245f)]
        [TestCase(0f, 1f, 0.105f)]
        [TestCase(1f, 1f, 0.325f)]
        [TestCase(2f, 2f, 0.325f)]
        public void AberrationChance_IsBoundedAndMatchesItsActualMaximum(
            float panic, float disturbance, float expected)
        {
            Assert.That(FishingSimulationPolicy.CalculateAberrationChance(panic, disturbance),
                Is.EqualTo(expected).Within(0.00001f));
        }

        [Test]
        public void EventChance_IsFrameRateIndependent()
        {
            float oneSecond = FishingSimulationPolicy.CalculateEventChance(0.8f, 1f);
            float halfSecond = FishingSimulationPolicy.CalculateEventChance(0.8f, 0.5f);
            float twoHalfSeconds = 1f - (1f - halfSecond) * (1f - halfSecond);
            Assert.That(twoHalfSeconds, Is.EqualTo(oneSecond).Within(0.00001f));
            Assert.That(FishingSimulationPolicy.CalculateEventChance(0.8f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void CandidateWeights_AreRelativeModifiersNotAbsolutePercentages()
        {
            float matched = FishingSimulationPolicy.CalculateCandidateWeight(100f, true, true, false);
            float techniqueMismatch = FishingSimulationPolicy.CalculateCandidateWeight(100f, false, true, false);
            float baitMismatch = FishingSimulationPolicy.CalculateCandidateWeight(100f, true, false, false);
            float bothMismatch = FishingSimulationPolicy.CalculateCandidateWeight(100f, false, false, false);
            float aberrant = FishingSimulationPolicy.CalculateCandidateWeight(100f, true, true, true);

            Assert.That(techniqueMismatch / matched, Is.EqualTo(0.35f).Within(0.00001f));
            Assert.That(baitMismatch / matched, Is.EqualTo(0.28f).Within(0.00001f));
            Assert.That(bothMismatch / matched, Is.EqualTo(0.098f).Within(0.00001f));
            Assert.That(aberrant / matched, Is.EqualTo(0.15f).Within(0.00001f));
        }

        [TestCase(1f, 0f, 20f, 0.22f)]
        [TestCase(1f, 10f, 20f, 0.11f)]
        [TestCase(0.5f, 10f, 20f, 0.055f)]
        [TestCase(1f, 21f, 20f, 0f)]
        public void AcousticDisturbanceGain_IsBoundedByLoudnessAndDistance(
            float loudness, float distance, float radius, float expected)
        {
            Assert.That(FishingSimulationPolicy.CalculateAcousticDisturbanceGain(loudness, distance, radius),
                Is.EqualTo(expected).Within(0.00001f));
        }

        [Test]
        public void DisturbanceDecay_UsesHalfLifeAndIsFrameRateIndependent()
        {
            float fullStep = FishingSimulationPolicy.DecayDisturbance(1f, 180f, 180f);
            float halfStep = FishingSimulationPolicy.DecayDisturbance(1f, 90f, 180f);
            float twoHalfSteps = FishingSimulationPolicy.DecayDisturbance(halfStep, 90f, 180f);
            Assert.That(fullStep, Is.EqualTo(0.5f).Within(0.00001f));
            Assert.That(twoHalfSteps, Is.EqualTo(fullStep).Within(0.00001f));
            Assert.That(FishingSimulationPolicy.DecayDisturbance(0.7f, 0f, 180f), Is.EqualTo(0.7f));
            Assert.That(FishingSimulationPolicy.DecayDisturbance(0.7f, 10f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void DisturbedGround_ReducesAvailabilityAndIncreasesBiteWaitWithoutHardLockout()
        {
            Assert.That(FishingSimulationPolicy.CalculateFishAvailabilityAfterDisturbance(0f), Is.EqualTo(1f));
            Assert.That(FishingSimulationPolicy.CalculateFishAvailabilityAfterDisturbance(1f), Is.EqualTo(0.35f));
            Assert.That(FishingSimulationPolicy.CalculateDisturbedBiteWaitMultiplier(0f), Is.EqualTo(1f));
            Assert.That(FishingSimulationPolicy.CalculateDisturbedBiteWaitMultiplier(1f), Is.EqualTo(1.8f));
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

using NUnit.Framework;
using WhereTheSoundingsEnd.Boat;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class BoatPhysicsTests
    {
        [Test]
        public void Buoyancy_ProducesNoForceAboveWater()
        {
            float force = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                -0.01f, 0f, 1000f, 100f, 1200f);
            Assert.AreEqual(0f, force);
        }

        [Test]
        public void Buoyancy_IncreasesWithSubmersionDepth()
        {
            float shallow = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.1f, 0f, 1000f, 100f, 1200f);
            float deeper = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.2f, 0f, 1000f, 100f, 1200f);

            Assert.That(deeper, Is.GreaterThan(shallow));
            Assert.AreEqual(100f, shallow, 0.001f);
            Assert.AreEqual(200f, deeper, 0.001f);
        }

        [Test]
        public void Buoyancy_DampingResistsVerticalMotion()
        {
            float stationary = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.2f, 0f, 1000f, 100f, 1200f);
            float rising = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.2f, 1f, 1000f, 100f, 1200f);
            float sinking = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.2f, -1f, 1000f, 100f, 1200f);

            Assert.That(rising, Is.LessThan(stationary));
            Assert.That(sinking, Is.GreaterThan(stationary));
        }

        [Test]
        public void Buoyancy_RespectsMaximumPointForce()
        {
            float force = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                2f, -10f, 10000f, 10000f, 500f);
            Assert.AreEqual(500f, force);
        }

        [Test]
        public void Propulsion_AcceleratesForwardAndLimitsAtMaximumSpeed()
        {
            float fromRest = BoatMovementMath.CalculatePropulsionAcceleration(
                1f, 0f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);
            float atLimit = BoatMovementMath.CalculatePropulsionAcceleration(
                1f, 8.5f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);

            Assert.That(fromRest, Is.EqualTo(2.8f).Within(0.001f));
            Assert.AreEqual(0f, atLimit, 0.001f);
        }

        [Test]
        public void Propulsion_ReverseIsSlowerThanForward()
        {
            float forward = BoatMovementMath.CalculatePropulsionAcceleration(
                1f, 0f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);
            float reverse = BoatMovementMath.CalculatePropulsionAcceleration(
                -1f, 0f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);

            Assert.That(forward, Is.GreaterThan(System.Math.Abs(reverse)));
        }

        [Test]
        public void Propulsion_OpposingThrottleAppliesBrakingBeforeReversing()
        {
            float braking = BoatMovementMath.CalculatePropulsionAcceleration(
                -1f, 4f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);

            Assert.AreEqual(-2.6f, braking, 0.001f);
        }

        [Test]
        public void Propulsion_DamagedEngineAndHullReduceAcceleration()
        {
            float healthy = BoatMovementMath.CalculatePropulsionAcceleration(
                1f, 0f, 100f, 100f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);
            float damaged = BoatMovementMath.CalculatePropulsionAcceleration(
                1f, 0f, 30f, 40f, 8.5f, 3.2f, 2.8f, 0.95f, 2.6f);

            Assert.That(damaged, Is.LessThan(healthy));
            Assert.That(damaged, Is.GreaterThan(0f));
        }

        [Test]
        public void RudderAuthority_RequiresWaterFlowAndScalesWithSpeed()
        {
            float stopped = BoatMovementMath.CalculateRudderAuthority(0f, 0f, 1.1f, 4.5f);
            float slow = BoatMovementMath.CalculateRudderAuthority(1f, 0f, 1.1f, 4.5f);
            float fast = BoatMovementMath.CalculateRudderAuthority(6f, 0f, 1.1f, 4.5f);
            float propWash = BoatMovementMath.CalculateRudderAuthority(0f, 1f, 1.1f, 4.5f);

            Assert.AreEqual(0f, stopped);
            Assert.That(slow, Is.GreaterThan(stopped));
            Assert.That(fast, Is.GreaterThan(slow));
            Assert.That(propWash, Is.GreaterThan(stopped));
            Assert.That(fast, Is.LessThanOrEqualTo(1f));
        }
    }
}
using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class MaritimeSurfaceMathTests
    {
        [Test]
        public void WavePhase_ReversesRateWithoutResettingPhase()
        {
            float phase = MaritimeSurfaceMath.AdvancePhase(2.4f, 1.15f, 0.2f);
            float reversed = MaritimeSurfaceMath.AdvancePhase(phase, -1.55f, 0.1f);

            Assert.That(phase, Is.EqualTo(2.63f).Within(0.0001f));
            Assert.That(reversed, Is.EqualTo(2.475f).Within(0.0001f));
            Assert.That(reversed, Is.GreaterThan(0f));
        }

        [Test]
        public void WaveDirection_BendsGraduallyTowardWrongWaySwell()
        {
            Vector2 normal = new Vector2(0.82f, 0.34f).normalized;
            Vector2 wrongWay = new Vector2(-0.48f, 0.88f).normalized;

            Vector2 transitioned = MaritimeSurfaceMath.BlendDirection(
                normal, wrongWay, 1.8f, 0.02f);

            Assert.That(transitioned.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Vector2.Angle(normal, transitioned),
                Is.GreaterThan(0f).And.LessThan(Vector2.Angle(normal, wrongWay)));
            Assert.That(Vector2.Distance(transitioned, wrongWay), Is.GreaterThan(0f));
        }

        [Test]
        public void WaveDirection_HandlesZeroInputWithoutProducingNaN()
        {
            Vector2 result = MaritimeSurfaceMath.BlendDirection(
                Vector2.zero, Vector2.zero, 1.8f, 0.02f);

            Assert.That(result.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(float.IsNaN(result.x), Is.False);
            Assert.That(float.IsNaN(result.y), Is.False);
        }

        [Test]
        public void SurfaceVelocity_IncludesAmplitudeAndDirectionHeightChanges()
        {
            float velocity = MaritimeSurfaceMath.EstimateVerticalVelocity(
                previousHeight: 0.10f,
                currentHeight: 0.34f,
                elapsedSeconds: 0.12f);

            Assert.That(velocity, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void SurfaceVelocity_ReturnsZeroWhenElapsedTimeIsInvalid()
        {
            Assert.AreEqual(0f, MaritimeSurfaceMath.EstimateVerticalVelocity(0.1f, 0.2f, 0f));
            Assert.AreEqual(0f, MaritimeSurfaceMath.EstimateVerticalVelocity(0.1f, 0.2f, -1f));
        }

        [Test]
        public void BuoyancyDepth_IsCappedAtConfiguredMaximumSubmersion()
        {
            float depth = BoatHydrodynamicsMath.ClampSubmersionDepth(1.2f, 0.65f);
            float expectedForce = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                0.65f, 0f, 1000f, 100f, 1200f);

            float actualForce = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                depth, 0f, 1000f, 100f, 1200f);

            Assert.AreEqual(0.65f, depth, 0.0001f);
            Assert.AreEqual(expectedForce, actualForce, 0.0001f);
        }

        [Test]
        public void BuoyancyDepth_RejectsInvalidValues()
        {
            Assert.AreEqual(0f, BoatHydrodynamicsMath.ClampSubmersionDepth(float.NaN, 0.65f));
            Assert.AreEqual(0f, BoatHydrodynamicsMath.ClampSubmersionDepth(0.5f, float.PositiveInfinity));
            Assert.AreEqual(0f, BoatHydrodynamicsMath.ClampSubmersionDepth(-0.5f, 0.65f));
        }
    }
}
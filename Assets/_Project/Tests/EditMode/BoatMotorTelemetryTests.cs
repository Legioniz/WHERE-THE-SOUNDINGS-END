using NUnit.Framework;
using WhereTheSoundingsEnd.Boat;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class BoatMotorTelemetryTests
    {
        [Test]
        public void EngineLoad_IsZeroWhenThrottleIsReleasedEvenAtHighSpeed()
        {
            float load = BoatMotorTelemetryMath.CalculateEngineLoad(
                0f, 8.5f, 8.5f, 3.2f);
            Assert.AreEqual(0f, load, 0.0001f);
        }

        [Test]
        public void EngineLoad_IncreasesAsTheBoatBuildsSpeedUnderThrottle()
        {
            float lowSpeedLoad = BoatMotorTelemetryMath.CalculateEngineLoad(1f, 0f, 8.5f, 3.2f);
            float cruiseLoad = BoatMotorTelemetryMath.CalculateEngineLoad(1f, 4.25f, 8.5f, 3.2f);
            float maximumSpeedLoad = BoatMotorTelemetryMath.CalculateEngineLoad(1f, 8.5f, 8.5f, 3.2f);

            Assert.AreEqual(0.68f, lowSpeedLoad, 0.0001f);
            Assert.That(cruiseLoad, Is.GreaterThan(lowSpeedLoad));
            Assert.AreEqual(1f, maximumSpeedLoad, 0.0001f);
        }

        [Test]
        public void EngineLoad_ScalesWithThrottleDemand()
        {
            float halfThrottle = BoatMotorTelemetryMath.CalculateEngineLoad(0.5f, 0f, 8.5f, 3.2f);
            float fullThrottle = BoatMotorTelemetryMath.CalculateEngineLoad(1f, 0f, 8.5f, 3.2f);
            Assert.AreEqual(fullThrottle * 0.5f, halfThrottle, 0.0001f);
        }

        [Test]
        public void ReverseSpeed_UsesTheLowerReverseSpeedLimit()
        {
            float speedRatio = BoatMotorTelemetryMath.CalculateSpeedRatio(-1.6f, 8.5f, 3.2f);
            float reverseLoad = BoatMotorTelemetryMath.CalculateEngineLoad(-1f, -3.2f, 8.5f, 3.2f);
            Assert.AreEqual(0.5f, speedRatio, 0.0001f);
            Assert.AreEqual(1f, reverseLoad, 0.0001f);
        }

        [Test]
        public void EngineRpmProxy_RetainsIdleAndReachesMaximumAtFullThrottle()
        {
            float idle = BoatMotorTelemetryMath.CalculateEngineRpmNormalized(0f, 7f, 8.5f, 3.2f);
            float maximum = BoatMotorTelemetryMath.CalculateEngineRpmNormalized(1f, 8.5f, 8.5f, 3.2f);
            Assert.AreEqual(BoatMotorTelemetryMath.IdleRpmNormalized, idle, 0.0001f);
            Assert.AreEqual(1f, maximum, 0.0001f);
        }

        [Test]
        public void EngineTelemetry_RejectsInvalidInputsAndRemainsBounded()
        {
            Assert.AreEqual(0f, BoatMotorTelemetryMath.CalculateEngineLoad(float.NaN, 4f, 8.5f, 3.2f));
            Assert.AreEqual(0f, BoatMotorTelemetryMath.CalculateEngineLoad(1f, 4f, 0f, 3.2f));
            Assert.AreEqual(0f, BoatMotorTelemetryMath.CalculateSpeedRatio(
                float.PositiveInfinity, 8.5f, 3.2f));
            Assert.AreEqual(BoatMotorTelemetryMath.IdleRpmNormalized,
                BoatMotorTelemetryMath.CalculateEngineRpmNormalized(float.NaN, 0f, 8.5f, 3.2f), 0.0001f);
        }
    }
}

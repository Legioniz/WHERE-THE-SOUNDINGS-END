using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.CameraSystem;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class CameraFollowMathTests
    {
        [Test]
        public void CalculateLookAhead_UsesHorizontalTravelVelocity()
        {
            Vector3 lead = CameraFollowMath.CalculateLookAhead(
                new Vector3(0f, 12f, 8f),
                0.5f,
                6f);

            Assert.That(lead.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(lead.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(lead.z, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void CalculateLookAhead_ReturnsZeroWhenVesselIsStationary()
        {
            Vector3 lead = CameraFollowMath.CalculateLookAhead(
                new Vector3(0f, 3f, 0f),
                1.0f,
                6f);

            Assert.That(lead, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void CalculateLookAhead_ClampsLeadToMaximumDistance()
        {
            Vector3 lead = CameraFollowMath.CalculateLookAhead(
                new Vector3(0f, 0f, 20f),
                2.0f,
                5f);

            Assert.That(lead.magnitude, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(lead.z, Is.GreaterThan(0f));
        }

        [Test]
        public void CalculateLookAhead_NegativeTuningCannotReverseTheLead()
        {
            Vector3 lead = CameraFollowMath.CalculateLookAhead(
                new Vector3(0f, 0f, 8f),
                -1.0f,
                -5f);

            Assert.That(lead, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void CalculateTurnBank_BanksOppositeTheTurnDirection()
        {
            float leftTurn = CameraFollowMath.CalculateTurnBank(1.0f, 1.5f, 4f);
            float rightTurn = CameraFollowMath.CalculateTurnBank(-1.0f, 1.5f, 4f);

            Assert.That(leftTurn, Is.EqualTo(-1.5f).Within(0.0001f));
            Assert.That(rightTurn, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void CalculateTurnBank_ClampsExtremeAngularVelocity()
        {
            float bank = CameraFollowMath.CalculateTurnBank(12.0f, 1.5f, 2.5f);

            Assert.That(bank, Is.EqualTo(-2.5f).Within(0.0001f));
        }
    }
}
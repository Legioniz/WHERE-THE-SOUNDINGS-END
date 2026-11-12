using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Interaction;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class DocksideAccessPolicyTests
    {
        [Test]
        public void StationaryVesselInsideShoreApproachCanOpenLocation()
        {
            Assert.IsTrue(DocksideAccessPolicy.IsAccessible(
                new Vector3(4f, 0f, 8f),
                new Vector3(0f, 50f, 8f),
                0.5f,
                10f,
                0.85f));
        }

        [Test]
        public void VesselOutsideHorizontalApproachCannotOpenLocation()
        {
            Assert.IsFalse(DocksideAccessPolicy.IsAccessible(
                new Vector3(10.01f, 0f, 0f),
                Vector3.zero,
                0.1f,
                10f,
                0.85f));
        }

        [Test]
        public void MovingVesselCannotOpenLocationEvenWhenNearby()
        {
            Assert.IsFalse(DocksideAccessPolicy.IsAccessible(
                new Vector3(1f, 0f, 1f),
                Vector3.zero,
                0.851f,
                10f,
                0.85f));
        }

        [Test]
        public void VerticalSeparationDoesNotChangeShoreApproachDistance()
        {
            Assert.IsTrue(DocksideAccessPolicy.IsAccessible(
                new Vector3(0f, 500f, 0f),
                new Vector3(0f, -500f, 0f),
                0f,
                1f,
                0.85f));
        }

        [Test]
        public void InvalidOrNonFiniteInputsAreRejected()
        {
            Assert.IsFalse(DocksideAccessPolicy.IsAccessible(
                new Vector3(float.NaN, 0f, 0f),
                Vector3.zero,
                0f,
                10f,
                0.85f));
            Assert.IsFalse(DocksideAccessPolicy.IsAccessible(
                Vector3.zero,
                Vector3.zero,
                0f,
                0f,
                0.85f));
            Assert.IsFalse(DocksideAccessPolicy.IsAccessible(
                Vector3.zero,
                Vector3.zero,
                float.PositiveInfinity,
                10f,
                0.85f));
        }
    }
}

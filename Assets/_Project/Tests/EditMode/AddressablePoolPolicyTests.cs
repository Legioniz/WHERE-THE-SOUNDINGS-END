using System;
using NUnit.Framework;
using WhereTheSoundingsEnd.Streaming;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class AddressablePoolPolicyTests
    {
        [TestCase(-5, 8, 0)]
        [TestCase(0, 8, 0)]
        [TestCase(3, 8, 3)]
        [TestCase(20, 8, 8)]
        [TestCase(3, 0, 1)]
        public void NormalizePrewarmCount_ClampsToSafePoolCapacity(int requested, int maximum, int expected)
        {
            Assert.That(AddressablePoolPolicy.NormalizePrewarmCount(requested, maximum), Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReleaseUnused_RequiresZeroReferencesAndElapsedGrace()
        {
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(1, 100f, 1f), Is.False);
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(0, 0.99f, 1f), Is.False);
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(0, 1f, 1f), Is.True);
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(-1, 0f, 0f), Is.True);
        }

        [Test]
        public void ShouldReleaseUnused_RejectsNonFiniteTimingValues()
        {
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(0, float.NaN, 1f), Is.False);
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(0, 1f, float.PositiveInfinity), Is.False);
            Assert.That(AddressablePoolPolicy.ShouldReleaseUnused(0, float.NegativeInfinity, 0f), Is.False);
        }
    }
}

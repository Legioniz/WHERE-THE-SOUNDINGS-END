using System;
using System.Collections.Generic;
using NUnit.Framework;
using WhereTheSoundingsEnd.Narrative;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class OpeningCinematicPolicyTests
    {
        [Test]
        public void TryRegisterFirstContact_AcceptsStableIdOnlyOnce()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            Assert.IsTrue(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.harbourmaster"));
            Assert.IsFalse(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.harbourmaster"));
            Assert.AreEqual(1, seen.Count);
        }

        [Test]
        public void TryRegisterFirstContact_AcceptsDistinctResidents()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            Assert.IsTrue(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.harbourmaster"));
            Assert.IsTrue(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.mara_venn"));
            Assert.IsTrue(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.tomas_vale"));
            Assert.IsTrue(CinematicSequencePolicy.TryRegisterFirstContact(
                seen, "greyhaven.inez_sarmiento"));
            Assert.AreEqual(4, seen.Count);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(" greyhaven.harbourmaster")]
        [TestCase("greyhaven.harbourmaster ")]
        public void TryRegisterFirstContact_RejectsBlankOrUntrimmedId(string npcId)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            Assert.IsFalse(CinematicSequencePolicy.TryRegisterFirstContact(seen, npcId));
            Assert.IsEmpty(seen);
        }

        [Test]
        public void TryRegisterFirstContact_RejectsOversizedId()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            string npcId = new string('x', CinematicSequencePolicy.MaximumNpcIdLength + 1);

            Assert.IsFalse(CinematicSequencePolicy.TryRegisterFirstContact(seen, npcId));
            Assert.IsEmpty(seen);
        }

        [Test]
        public void TryRegisterFirstContact_RejectsNullSet()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CinematicSequencePolicy.TryRegisterFirstContact(null, "greyhaven.harbourmaster"));
        }

        [TestCase(0f, 12, 30f, 0)]
        [TestCase(0.1f, 12, 30f, 3)]
        [TestCase(0.5f, 12, 30f, 12)]
        [TestCase(-1f, 12, 30f, 0)]
        [TestCase(1f, 0, 30f, 0)]
        [TestCase(1f, 12, 0f, 0)]
        public void GetVisibleCharacterCount_ClampsRevealProgress(
            float elapsed, int length, float rate, int expected)
        {
            Assert.AreEqual(expected,
                CinematicSequencePolicy.GetVisibleCharacterCount(elapsed, length, rate));
        }

        [Test]
        public void GetVisibleCharacterCount_HandlesInvalidTimingValues()
        {
            Assert.AreEqual(0, CinematicSequencePolicy.GetVisibleCharacterCount(
                float.NaN, 10, 30f));
            Assert.AreEqual(0, CinematicSequencePolicy.GetVisibleCharacterCount(
                1f, 10, float.PositiveInfinity));
        }
    }
}

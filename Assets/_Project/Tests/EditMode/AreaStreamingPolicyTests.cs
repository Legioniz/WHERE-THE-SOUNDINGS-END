using NUnit.Framework;
using WhereTheSoundingsEnd.Streaming;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class AreaStreamingPolicyTests
    {
        [Test]
        public void ValidRadii_RequireUnloadRadiusBeyondPreloadRadius()
        {
            Assert.That(AreaStreamingPolicy.IsValidRadii(60f, 90f), Is.True);
            Assert.That(AreaStreamingPolicy.IsValidRadii(60f, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.IsValidRadii(90f, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.IsValidRadii(0f, 10f), Is.False);
        }

        [Test]
        public void ShouldPreload_UsesInclusiveInnerBoundary()
        {
            Assert.That(AreaStreamingPolicy.ShouldPreload(60f, 60f), Is.True);
            Assert.That(AreaStreamingPolicy.ShouldPreload(60.01f, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldPreload(-1f, 60f), Is.False);
        }

        [Test]
        public void ShouldUnload_UsesOuterBoundaryAndPreservesHysteresisBand()
        {
            Assert.That(AreaStreamingPolicy.ShouldUnload(90f, 90f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldUnload(90.01f, 90f), Is.True);
            Assert.That(AreaStreamingPolicy.ShouldUnload(75f, 90f), Is.False);
        }

        [Test]
        public void ShouldStandby_OnlyOutsidePreloadRadiusAndRejectsInvalidDistances()
        {
            Assert.That(AreaStreamingPolicy.ShouldStandby(60.01f, 60f), Is.True);
            Assert.That(AreaStreamingPolicy.ShouldStandby(60f, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldStandby(20f, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldStandby(float.NaN, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldStandby(80f, float.PositiveInfinity), Is.False);
        }

        [Test]
        public void DistanceRules_RejectNonFiniteValues()
        {
            Assert.That(AreaStreamingPolicy.ShouldPreload(float.NaN, 60f), Is.False);
            Assert.That(AreaStreamingPolicy.ShouldUnload(float.PositiveInfinity, 90f), Is.False);
            Assert.That(AreaStreamingPolicy.IsValidRadii(float.NaN, 90f), Is.False);
        }
        [Test]
        public void StreamedSceneIdentityAndRevision_AcceptsExactCurrentContent()
        {
            Assert.That(StreamedAreaSceneRoot.MatchesIdentityAndRevision(
                "greyhaven.harbour", "5", "greyhaven.harbour", "5"), Is.True);
        }

        [Test]
        public void StreamedSceneIdentityAndRevision_RejectsStaleRevisionOrWrongArea()
        {
            Assert.That(StreamedAreaSceneRoot.MatchesIdentityAndRevision(
                "greyhaven.harbour", "4", "greyhaven.harbour", "5"), Is.False);
            Assert.That(StreamedAreaSceneRoot.MatchesIdentityAndRevision(
                "greyhaven.village", "5", "greyhaven.harbour", "5"), Is.False);
            Assert.That(StreamedAreaSceneRoot.MatchesIdentityAndRevision(
                "", "5", "greyhaven.harbour", "5"), Is.False);
            Assert.That(StreamedAreaSceneRoot.MatchesIdentityAndRevision(
                "greyhaven.harbour", "5", "greyhaven.harbour", ""), Is.False);
        }
    }
}

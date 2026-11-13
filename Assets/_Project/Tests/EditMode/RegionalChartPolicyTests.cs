using System.Collections.Generic;
using NUnit.Framework;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class RegionalChartPolicyTests
    {
        [Test]
        public void UnvisitedRegionRemainsUnknown()
        {
            var visited = new HashSet<string>();
            Assert.That(RegionalChartPolicy.IsRegionKnown(visited, "the_outer_shoals"), Is.False);
        }

        [Test]
        public void FirstPhysicalVisitUnlocksAndRecordsRegion()
        {
            var visited = new HashSet<string>();
            var unlocked = new HashSet<string>();
            Assert.That(RegionalChartPolicy.TryRecordVisit(visited, unlocked, " The_Outer_Shoals "), Is.True);
            Assert.That(visited.Contains("the_outer_shoals"), Is.True);
            Assert.That(unlocked.Contains("the_outer_shoals"), Is.True);
            Assert.That(RegionalChartPolicy.TryRecordVisit(visited, unlocked, "the_outer_shoals"), Is.False);
        }

        [Test]
        public void SiteMarkRequiresBothVisitedRegionAndDiscoveredSite()
        {
            var visited = new HashSet<string>();
            var discovered = new HashSet<string> { "site.wreck.01" };
            Assert.That(RegionalChartPolicy.ShouldRevealSite(visited, discovered, "greyhaven", "site.wreck.01"), Is.False);
            visited.Add("greyhaven");
            Assert.That(RegionalChartPolicy.ShouldRevealSite(visited, discovered, "greyhaven", "site.wreck.01"), Is.True);
            Assert.That(RegionalChartPolicy.ShouldRevealSite(visited, discovered, "greyhaven", "site.unknown"), Is.False);
        }

        [Test]
        public void BlankIdentifiersCannotUnlockOrRevealAnything()
        {
            var visited = new HashSet<string>();
            var unlocked = new HashSet<string>();
            Assert.That(RegionalChartPolicy.TryRecordVisit(visited, unlocked, " "), Is.False);
            Assert.That(RegionalChartPolicy.IsRegionKnown(visited, " "), Is.False);
            Assert.That(RegionalChartPolicy.ShouldRevealSite(visited, unlocked, "greyhaven", " "), Is.False);
        }
    }
}

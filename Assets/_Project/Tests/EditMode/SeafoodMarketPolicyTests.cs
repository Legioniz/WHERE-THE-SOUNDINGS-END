using NUnit.Framework;
using WhereTheSoundingsEnd.Economy;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class SeafoodMarketPolicyTests
    {
        [TestCase("greyhaven", SeafoodMarketClass.Inshore, 1.08)]
        [TestCase("greyhaven", SeafoodMarketClass.Reef, 0.92)]
        [TestCase("the_shallows", SeafoodMarketClass.Reef, 1.20)]
        [TestCase("blackrock_reach", SeafoodMarketClass.Pelagic, 1.18)]
        [TestCase("the_drowned_parish", SeafoodMarketClass.Inshore, 1.20)]
        [TestCase("the_outer_shoals", SeafoodMarketClass.Pelagic, 1.25)]
        [TestCase("the_hollow_trench", SeafoodMarketClass.Deepwater, 1.40)]
        [TestCase("the_hollow_trench", SeafoodMarketClass.Seaweed, 0.50)]
        public void DestinationFactorMatchesAuthoredMarketMatrix(
            string regionId, SeafoodMarketClass marketClass, double expected)
        {
            Assert.AreEqual((decimal)expected,
                SeafoodMarketPolicy.GetDestinationFactor(regionId, marketClass));
        }

        [Test]
        public void EveryEstablishedRegionHasAMarketAndUnknownRegionHasNoPrice()
        {
            string[] regions =
            {
                "greyhaven", "the_shallows", "blackrock_reach",
                "the_drowned_parish", "the_outer_shoals", "the_hollow_trench"
            };
            for (int i = 0; i < regions.Length; i++)
                Assert.IsTrue(SeafoodMarketPolicy.IsSupportedRegion(regions[i]), regions[i]);

            Assert.IsFalse(SeafoodMarketPolicy.IsSupportedRegion("unknown_island"));
            Assert.AreEqual(0m, SeafoodMarketPolicy.GetDestinationFactor(
                "unknown_island", SeafoodMarketClass.Inshore));
        }
    }
}
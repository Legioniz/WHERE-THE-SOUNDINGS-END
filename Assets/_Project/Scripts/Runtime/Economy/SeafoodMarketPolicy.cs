using System;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>Authored destination pricing matrix. Factors apply only where a sale occurs.</summary>
    public static class SeafoodMarketPolicy
    {
        public static bool IsSupportedRegion(string regionId)
        {
            switch (Normalize(regionId))
            {
                case "greyhaven":
                case "the_shallows":
                case "blackrock_reach":
                case "the_drowned_parish":
                case "the_outer_shoals":
                case "the_hollow_trench":
                    return true;
                default:
                    return false;
            }
        }

        public static string GetMarketName(string regionId)
        {
            switch (Normalize(regionId))
            {
                case "greyhaven": return "Greyhaven";
                case "the_shallows": return "The Shallows";
                case "blackrock_reach": return "Blackrock Reach";
                case "the_drowned_parish": return "The Drowned Parish";
                case "the_outer_shoals": return "The Outer Shoals";
                case "the_hollow_trench": return "The Hollow Trench";
                default: return "Uncharted Market";
            }
        }

        public static decimal GetDestinationFactor(string regionId, SeafoodMarketClass marketClass)
        {
            string region = Normalize(regionId);
            switch (region)
            {
                case "greyhaven":
                    return Factor(marketClass, 1.08m, 0.92m, 0.90m, 0.70m, 1.10m, 1.05m);
                case "the_shallows":
                    return Factor(marketClass, 0.95m, 1.20m, 0.95m, 0.75m, 1.15m, 1.15m);
                case "blackrock_reach":
                    return Factor(marketClass, 0.90m, 1.10m, 1.18m, 1.00m, 0.95m, 0.85m);
                case "the_drowned_parish":
                    return Factor(marketClass, 1.20m, 0.85m, 0.75m, 0.55m, 1.15m, 1.10m);
                case "the_outer_shoals":
                    return Factor(marketClass, 0.75m, 0.90m, 1.25m, 1.15m, 0.85m, 0.75m);
                case "the_hollow_trench":
                    return Factor(marketClass, 0.55m, 0.75m, 1.10m, 1.40m, 0.70m, 0.50m);
                default:
                    return 0m;
            }
        }

        private static decimal Factor(SeafoodMarketClass marketClass,
            decimal inshore, decimal reef, decimal pelagic, decimal deepwater,
            decimal invertebrate, decimal seaweed)
        {
            switch (marketClass)
            {
                case SeafoodMarketClass.Inshore: return inshore;
                case SeafoodMarketClass.Reef: return reef;
                case SeafoodMarketClass.Pelagic: return pelagic;
                case SeafoodMarketClass.Deepwater: return deepwater;
                case SeafoodMarketClass.Invertebrate: return invertebrate;
                case SeafoodMarketClass.Seaweed: return seaweed;
                default: return 1m;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
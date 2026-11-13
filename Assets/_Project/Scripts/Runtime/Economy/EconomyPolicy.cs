using System;

namespace WhereTheSoundingsEnd.Economy
{
    /// <summary>
    /// Deterministic, side-effect-free economy calculations.
    /// All prices are USD; monetary results are integer cents.
    /// Runtime systems must pass authored item prices and destination factors.
    /// </summary>
    public static class EconomyPolicy
    {
        public const decimal MinimumQualityFactor = 0.60m;
        public const decimal MaximumQualityFactor = 1.20m;
        public const decimal MinimumFreshnessFactor = 0.35m;
        public const decimal MaximumFreshnessFactor = 1.00m;
        public const decimal MinimumDemandFactor = 0.75m;
        public const decimal MaximumDemandFactor = 1.35m;
        public const decimal MinimumLiveConditionFactor = 1.00m;
        public const decimal MaximumLiveConditionFactor = 1.15m;

        /// <summary>
        /// Returns a line total in cents. The sale unit is supplied by the caller:
        /// kilograms for ordinary seafood, or the authored individual/unit for special goods.
        /// Ineligible or unsupported cargo always returns zero.
        /// </summary>
        public static long CalculateSaleLineCents(
            decimal saleQuantity,
            decimal basePriceUsdPerUnit,
            decimal destinationFactor,
            decimal qualityFactor,
            decimal freshnessFactor,
            decimal demandFactor,
            decimal liveConditionFactor,
            bool ordinarySaleEligible)
        {
            if (!ordinarySaleEligible || saleQuantity <= 0m || basePriceUsdPerUnit <= 0m ||
                destinationFactor <= 0m)
                return 0L;

            decimal quality = Clamp(qualityFactor, MinimumQualityFactor, MaximumQualityFactor);
            decimal freshness = Clamp(freshnessFactor, MinimumFreshnessFactor, MaximumFreshnessFactor);
            decimal demand = Clamp(demandFactor, MinimumDemandFactor, MaximumDemandFactor);
            decimal live = Clamp(liveConditionFactor, MinimumLiveConditionFactor, MaximumLiveConditionFactor);

            decimal revenueUsd = saleQuantity * basePriceUsdPerUnit * destinationFactor *
                                 quality * freshness * demand * live;
            decimal cents = decimal.Round(revenueUsd * 100m, 0, MidpointRounding.AwayFromZero);
            if (cents >= long.MaxValue) return long.MaxValue;
            return Math.Max(0L, (long)cents);
        }

        /// <summary>
        /// Buyer-specific demand from target stock and current stock.
        /// A fully stocked buyer trends below neutral; a depleted buyer trends above it.
        /// </summary>
        public static decimal CalculateDemandFactor(
            int targetStock,
            int currentStock,
            decimal scarcityResponse,
            int minimumStock = 1)
        {
            if (targetStock < 1 || currentStock < 0 || minimumStock < 1)
                return 1.00m;

            decimal denominator = Math.Max(targetStock, minimumStock);
            decimal shortageRatio = (targetStock - (decimal)currentStock) / denominator;
            decimal response = Math.Max(0m, scarcityResponse);
            return Clamp(1m + response * shortageRatio, MinimumDemandFactor, MaximumDemandFactor);
        }

        public static long CalculateRepairCostCents(decimal missingIntegrity, decimal costUsdPerIntegrityPoint)
        {
            if (missingIntegrity <= 0m || costUsdPerIntegrityPoint <= 0m) return 0L;
            return ToCents(missingIntegrity * costUsdPerIntegrityPoint);
        }

        public static long CalculateFuelCostCents(decimal fuelUnits, decimal costUsdPerFuelUnit)
        {
            if (fuelUnits <= 0m || costUsdPerFuelUnit <= 0m) return 0L;
            return ToCents(fuelUnits * costUsdPerFuelUnit);
        }

        /// <summary>
        /// Computes a wallet result without mutating state. Callers commit only after
        /// every other transaction precondition has passed.
        /// </summary>
        public static bool TryApplyWalletDelta(long currentCents, long deltaCents, out long resultCents)
        {
            resultCents = currentCents;
            if (currentCents < 0L) return false;

            if (deltaCents < 0L)
            {
                if (deltaCents == long.MinValue || currentCents < -deltaCents) return false;
                resultCents = currentCents + deltaCents;
                return true;
            }

            if (long.MaxValue - currentCents < deltaCents) return false;
            resultCents = currentCents + deltaCents;
            return true;
        }

        private static long ToCents(decimal amountUsd)
        {
            if (amountUsd <= 0m) return 0L;
            decimal cents = decimal.Round(amountUsd * 100m, 0, MidpointRounding.AwayFromZero);
            return cents >= long.MaxValue ? long.MaxValue : (long)cents;
        }

        private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}

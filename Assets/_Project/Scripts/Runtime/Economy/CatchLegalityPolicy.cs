using System;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Economy
{
    public enum CatchViolation
    {
        None,
        ClosedArea,
        OutsideSpeciesRange,
        Undersized,
        ProtectedSpecies,
        MissingHarvestPermit,
        ProhibitedGear,
        ProhibitedTechnique
    }

    [Serializable]
    public sealed class CatchLegalityResult
    {
        public bool IsLegal;
        public CatchViolation Violation;
        public string Reason;
        public long FineCents;
    }

    /// <summary>
    /// Fictional game rules for catch eligibility. These are authored gameplay thresholds,
    /// not statements of real-world fisheries law or biological minimum landing sizes.
    /// </summary>
    public static class CatchLegalityPolicy
    {
        public static CatchLegalityResult Evaluate(CargoItemDefinition definition,
            string regionId, FishingTechnique technique, float catchWeightKg,
            RegionalChartController chart, string selectedHookId = null)
        {
            if (definition == null)
                return Illegal(CatchViolation.OutsideSpeciesRange, "Unknown species record.", 300L);

            string region = string.IsNullOrWhiteSpace(regionId) ? string.Empty : regionId.Trim().ToLowerInvariant();
            if (region == "the_hollow_trench" && (chart == null || !chart.HasPermit("the_outer_shoals")))
                return Illegal(CatchViolation.ClosedArea,
                    "The Hollow Trench is closed to unpermitted harvesting.", 1000L);

            if (region.Length == 0 || !SeafoodMarketPolicy.IsSupportedRegion(region))
                return Illegal(CatchViolation.ClosedArea, "The catch was taken outside a recognized fishing ground.", 500L);

            if (!definition.IsAvailableInRegion(region))
                return Illegal(CatchViolation.OutsideSpeciesRange,
                    "This species is outside its authored regional range.", 300L);

            if (!IsFinite(catchWeightKg) || catchWeightKg < definition.MinimumLegalCatchWeightKg)
                return Illegal(CatchViolation.Undersized,
                    "The landed specimen is below the game's minimum landing weight.", 100L);

            if (definition.ProtectedCatch)
            {
                if (string.IsNullOrWhiteSpace(definition.RequiredPermitId) ||
                    chart == null || !chart.HasPermit(definition.RequiredPermitId))
                    return Illegal(CatchViolation.ProtectedSpecies,
                        "This species is protected in the current campaign rules and requires a harvest permit.", 500L);
            }

            if (!string.IsNullOrWhiteSpace(definition.RequiredPermitId) &&
                (chart == null || !chart.HasPermit(definition.RequiredPermitId)))
                return Illegal(CatchViolation.MissingHarvestPermit,
                    "A specific harvest permit is required for this catch.", 500L);

            if (!string.IsNullOrWhiteSpace(definition.RequiredHookId) &&
                !string.Equals(definition.RequiredHookId, selectedHookId, StringComparison.OrdinalIgnoreCase))
                return Illegal(CatchViolation.ProhibitedGear,
                    "This species requires the approved hook configuration.", 200L);

            int techniqueMask = definition.AllowedTechniqueMask;
            if (techniqueMask != 0 && (techniqueMask & (1 << (int)technique)) == 0)
                return Illegal(CatchViolation.ProhibitedTechnique,
                    "The technique used is not permitted for this species.", 200L);

            return new CatchLegalityResult
            {
                IsLegal = true,
                Violation = CatchViolation.None,
                Reason = "Legal landing.",
                FineCents = 0L
            };
        }

        private static CatchLegalityResult Illegal(CatchViolation violation, string reason, long fine)
        {
            return new CatchLegalityResult
            {
                IsLegal = false,
                Violation = violation,
                Reason = reason,
                FineCents = fine
            };
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
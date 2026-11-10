namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Authored placement constraints for surveyable phenomena.
    /// These are game-world plausibility gates, not a universal oceanographic database.
    /// Update them when the setting's tectonic and regional geology is locked.
    /// </summary>
    public static class DeepOceanPhenomenonRules
    {
        public static bool IsAllowed(
            DeepOceanPhenomenonType type,
            string regionId,
            float depthMeters)
        {
            if (string.IsNullOrWhiteSpace(regionId) ||
                float.IsNaN(depthMeters) || float.IsInfinity(depthMeters) || depthMeters < 0f)
                return false;

            string region = regionId.Trim().ToLowerInvariant();
            switch (type)
            {
                case DeepOceanPhenomenonType.BiologicalLayer:
                    return IsDeepRegion(region) && Between(depthMeters, 80f, 1400f);

                case DeepOceanPhenomenonType.BrinePool:
                    return region == "the_hollow_trench" && Between(depthMeters, 1000f, 6000f);

                case DeepOceanPhenomenonType.HydrothermalVent:
                    return IsDeepRegion(region) && Between(depthMeters, 200f, 6000f);

                case DeepOceanPhenomenonType.ColdSeep:
                    return IsDeepRegion(region) && Between(depthMeters, 200f, 6000f);

                case DeepOceanPhenomenonType.GasHydrate:
                    return IsDeepRegion(region) && Between(depthMeters, 500f, 6000f);

                case DeepOceanPhenomenonType.WhaleFall:
                    return IsDeepRegion(region) && Between(depthMeters, 300f, 6000f);

                case DeepOceanPhenomenonType.Bioluminescence:
                    return IsMarineRegion(region) && Between(depthMeters, 0f, 1200f);

                case DeepOceanPhenomenonType.WaterMassFront:
                    return (region == "the_shallows" || region == "blackrock_reach" ||
                            region == "the_outer_shoals") && Between(depthMeters, 0f, 1000f);

                case DeepOceanPhenomenonType.DerelictVessel:
                    return IsMarineRegion(region) && Between(depthMeters, 0f, 6000f);

                case DeepOceanPhenomenonType.DamagedFishingGear:
                    return IsMarineRegion(region) && Between(depthMeters, 0f, 2000f);

                case DeepOceanPhenomenonType.UnclassifiedAcousticReturn:
                    return IsMarineRegion(region) && Between(depthMeters, 0f, 6000f);

                default:
                    return false;
            }
        }

        private static bool IsDeepRegion(string region)
        {
            return region == "the_outer_shoals" || region == "the_hollow_trench";
        }

        private static bool IsMarineRegion(string region)
        {
            return region == "greyhaven" || region == "the_shallows" ||
                   region == "blackrock_reach" || region == "the_drowned_parish" ||
                   region == "the_outer_shoals" || region == "the_hollow_trench";
        }

        private static bool Between(float value, float minimum, float maximum)
        {
            return value >= minimum && value <= maximum;
        }
    }
}
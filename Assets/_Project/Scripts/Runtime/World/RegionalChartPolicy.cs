using System;
using System.Collections.Generic;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>Pure rules for what a paper chart is allowed to reveal.</summary>
    public static class RegionalChartPolicy
    {
        public static bool IsRegionKnown(ICollection<string> visitedRegionIds, string regionId)
        {
            return visitedRegionIds != null && !string.IsNullOrWhiteSpace(regionId) &&
                visitedRegionIds.Contains(NormalizeId(regionId));
        }

        public static bool ShouldRevealSite(
            ICollection<string> visitedRegionIds,
            ICollection<string> discoveredSiteIds,
            string regionId,
            string siteId)
        {
            return IsRegionKnown(visitedRegionIds, regionId) &&
                discoveredSiteIds != null &&
                !string.IsNullOrWhiteSpace(siteId) &&
                discoveredSiteIds.Contains(siteId.Trim());
        }

        public static bool TryRecordVisit(
            ICollection<string> visitedRegionIds,
            ICollection<string> unlockedRegionIds,
            string regionId)
        {
            if (visitedRegionIds == null || unlockedRegionIds == null ||
                string.IsNullOrWhiteSpace(regionId))
                return false;

            string id = NormalizeId(regionId);
            bool firstVisit = !visitedRegionIds.Contains(id);
            visitedRegionIds.Add(id);
            unlockedRegionIds.Add(id);
            return firstVisit;
        }

        public static string NormalizeId(string regionId)
        {
            return string.IsNullOrWhiteSpace(regionId)
                ? string.Empty
                : regionId.Trim().ToLowerInvariant();
        }
    }
}

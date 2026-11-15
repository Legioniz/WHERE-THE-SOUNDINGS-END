using System;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>Stable namespace and validation rules for campaign field-journal entries.</summary>
    public static class FieldJournalPolicy
    {
        public const string EntryPrefix = "field-discovery:";
        public const string FindingPrefix = "field-finding:";

        public static string CreateEntryId(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) ? string.Empty : EntryPrefix + itemId.Trim();
        }

        public static bool TryGetItemId(string entryId, out string itemId)
        {
            itemId = string.Empty;
            if (string.IsNullOrWhiteSpace(entryId) ||
                !entryId.StartsWith(EntryPrefix, StringComparison.Ordinal))
                return false;

            string candidate = entryId.Substring(EntryPrefix.Length).Trim();
            if (candidate.Length == 0) return false;
            itemId = candidate;
            return true;
        }

        public static string CreateFindingEntryId(string siteId)
        {
            return string.IsNullOrWhiteSpace(siteId) ? string.Empty : FindingPrefix + siteId.Trim();
        }

        public static bool TryGetSiteId(string entryId, out string siteId)
        {
            siteId = string.Empty;
            if (string.IsNullOrWhiteSpace(entryId) ||
                !entryId.StartsWith(FindingPrefix, StringComparison.Ordinal))
                return false;

            string candidate = entryId.Substring(FindingPrefix.Length).Trim();
            if (candidate.Length == 0) return false;
            siteId = candidate;
            return true;
        }

        public static string NormalizeRegionId(string regionId)
        {
            return string.IsNullOrWhiteSpace(regionId) ? "unknown" : regionId.Trim().ToLowerInvariant();
        }
    }
}

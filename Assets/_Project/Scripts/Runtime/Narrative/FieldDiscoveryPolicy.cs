using System;
using System.Collections.Generic;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>Guards first-time field discoveries against blank IDs and duplicate reveals.</summary>
    public static class FieldDiscoveryPolicy
    {
        public static bool TryRegister(ISet<string> discoveredIds, string itemId)
        {
            if (discoveredIds == null || string.IsNullOrWhiteSpace(itemId))
                return false;

            return discoveredIds.Add(itemId.Trim());
        }

        public static bool IsRegistered(IEnumerable<string> discoveredIds, string itemId)
        {
            if (discoveredIds == null || string.IsNullOrWhiteSpace(itemId))
                return false;

            foreach (string discoveredId in discoveredIds)
                if (string.Equals(discoveredId, itemId.Trim(), StringComparison.Ordinal))
                    return true;
            return false;
        }
    }
}

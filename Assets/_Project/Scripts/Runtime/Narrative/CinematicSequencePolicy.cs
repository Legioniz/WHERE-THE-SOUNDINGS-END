using System;
using System.Collections.Generic;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>
    /// Session-level presentation rules for one-time resident introductions.
    /// This state is deliberately cosmetic and is not campaign progression.
    /// </summary>
    public static class CinematicSequencePolicy
    {
        public const int MaximumNpcIdLength = 128;

        public static bool TryRegisterFirstContact(ISet<string> seenNpcIds, string npcId)
        {
            if (seenNpcIds == null)
                throw new ArgumentNullException(nameof(seenNpcIds));

            if (string.IsNullOrWhiteSpace(npcId) ||
                !string.Equals(npcId, npcId.Trim(), StringComparison.Ordinal) ||
                npcId.Length > MaximumNpcIdLength)
                return false;

            return seenNpcIds.Add(npcId);
        }
    }
}

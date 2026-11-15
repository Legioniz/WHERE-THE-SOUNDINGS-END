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

        public static int GetVisibleCharacterCount(float elapsedSeconds, int textLength, float charactersPerSecond)
        {
            if (textLength <= 0)
                return 0;

            if (float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                elapsedSeconds = 0f;
            if (float.IsNaN(charactersPerSecond) || float.IsInfinity(charactersPerSecond))
                charactersPerSecond = 0f;

            double elapsed = Math.Max(0d, elapsedSeconds);
            double rate = Math.Max(0d, charactersPerSecond);
            double reveal = elapsed * rate;
            if (double.IsNaN(reveal) || reveal <= 0d)
                return 0;
            if (double.IsInfinity(reveal) || reveal >= textLength)
                return textLength;

            return (int)Math.Floor(reveal);
        }
    }
}

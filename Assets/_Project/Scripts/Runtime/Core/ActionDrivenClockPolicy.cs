namespace WhereTheSoundingsEnd.Core
{
    /// <summary>
    /// Pure movement/action gate for the world clock. UI pause always wins.
    /// </summary>
    public static class ActionDrivenClockPolicy
    {
        public static bool ShouldAdvance(
            float horizontalSpeed,
            float movementThreshold,
            bool actionActive,
            bool paused)
        {
            if (paused) return false;
            return horizontalSpeed >= movementThreshold || actionActive;
        }
    }
}
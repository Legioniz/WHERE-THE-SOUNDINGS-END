using UnityEngine;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Derives presentation-only motor telemetry from the boat's existing throttle and
    /// signed horizontal speed. This is a tuned proxy, not a simulated combustion engine,
    /// gearbox, propeller torque curve or hydrodynamic load solver.
    /// </summary>
    public static class BoatMotorTelemetryMath
    {
        public const float IdleRpmNormalized = 0.14f;
        private const float MinimumLoadAtLowSpeed = 0.68f;

        public static float CalculateSpeedRatio(
            float forwardSpeed, float maximumForwardSpeed, float maximumReverseSpeed)
        {
            if (!IsFinite(forwardSpeed) ||
                !IsFinite(maximumForwardSpeed) ||
                !IsFinite(maximumReverseSpeed) ||
                maximumForwardSpeed <= 0f ||
                maximumReverseSpeed <= 0f)
                return 0f;

            float speedLimit = forwardSpeed < 0f ? maximumReverseSpeed : maximumForwardSpeed;
            return Mathf.Clamp01(Mathf.Abs(forwardSpeed) / speedLimit);
        }

        public static float CalculateEngineLoad(
            float throttle, float forwardSpeed, float maximumForwardSpeed, float maximumReverseSpeed)
        {
            if (!IsFinite(throttle) ||
                !IsFinite(maximumForwardSpeed) ||
                !IsFinite(maximumReverseSpeed) ||
                maximumForwardSpeed <= 0f ||
                maximumReverseSpeed <= 0f)
                return 0f;

            float throttleDemand = Mathf.Clamp01(Mathf.Abs(throttle));
            if (throttleDemand <= 0.0001f) return 0f;

            float speedRatio = CalculateSpeedRatio(
                forwardSpeed, maximumForwardSpeed, maximumReverseSpeed);
            return Mathf.Clamp01(throttleDemand *
                Mathf.Lerp(MinimumLoadAtLowSpeed, 1f, speedRatio));
        }

        public static float CalculateEngineRpmNormalized(
            float throttle, float forwardSpeed, float maximumForwardSpeed, float maximumReverseSpeed)
        {
            if (!IsFinite(throttle)) return IdleRpmNormalized;

            float throttleDemand = Mathf.Clamp01(Mathf.Abs(throttle));
            float speedRatio = CalculateSpeedRatio(
                forwardSpeed, maximumForwardSpeed, maximumReverseSpeed);
            float rpmDemand = throttleDemand * (0.9f + 0.1f * speedRatio);
            return Mathf.Lerp(IdleRpmNormalized, 1f, Mathf.Clamp01(rpmDemand));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

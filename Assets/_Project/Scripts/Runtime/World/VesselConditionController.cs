using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Converts meaningful hull-normal impacts into vessel/cargo damage and acoustic events.
    /// Multiple contacts in one physics step are coalesced into the strongest impact.
    /// Unity's Rigidbody solver remains responsible for physical separation and momentum.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VesselConditionController : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private DeepOceanSurveyRegion regionProfile;
        [SerializeField, Range(0f, 100f)] private float hullIntegrity = 100f;
        [SerializeField, Range(0f, 100f)] private float engineCondition = 100f;
        [SerializeField, Min(0f)] private float impactThreshold = 4.2f;
        [SerializeField, Min(0f)] private float damagePerImpactSpeed = 2.2f;

        private const float MinimumImpactDamage = 2f;
        private const float MaximumImpactDamage = 28f;
        private const float EngineDamageRatio = 0.18f;

        private Rigidbody body;
        private string lastDamageReport = "Hull condition stable.";
        private float pendingImpactSpeed;
        private Vector3 pendingImpactPoint;
        private bool hasPendingImpact;

        public float HullIntegrity => hullIntegrity;
        public float EngineCondition => engineCondition;
        public string LastDamageReport => lastDamageReport;
        public float ImpactThreshold => impactThreshold;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (cargoHold == null) cargoHold = GetComponent<SpatialCargoHold>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (regionProfile == null) regionProfile = FindFirstObjectByType<DeepOceanSurveyRegion>();
        }

        private void OnValidate()
        {
            impactThreshold = Mathf.Max(0f, impactThreshold);
            damagePerImpactSpeed = Mathf.Max(0f, damagePerImpactSpeed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null || collision.contactCount <= 0)
                return;

            Rigidbody otherBody = collision.rigidbody;
            float strongestImpactSpeed = 0f;
            Vector3 strongestContactPoint = transform.position;

            for (int index = 0; index < collision.contactCount; index++)
            {
                ContactPoint contact = collision.GetContact(index);
                Vector3 vesselPointVelocity = body != null
                    ? body.GetPointVelocity(contact.point)
                    : Vector3.zero;
                Vector3 otherPointVelocity = otherBody != null
                    ? otherBody.GetPointVelocity(contact.point)
                    : Vector3.zero;
                Vector3 relativeContactVelocity = vesselPointVelocity - otherPointVelocity;
                float normalImpactSpeed = VesselImpactMath.CalculateNormalImpactSpeed(
                    relativeContactVelocity, contact.normal);

                if (normalImpactSpeed <= strongestImpactSpeed)
                    continue;

                strongestImpactSpeed = normalImpactSpeed;
                strongestContactPoint = contact.point;
            }

            if (strongestImpactSpeed < impactThreshold)
                return;

            // Keep only the strongest reported contact before processing one damage event.
            if (!hasPendingImpact || strongestImpactSpeed > pendingImpactSpeed)
            {
                pendingImpactSpeed = strongestImpactSpeed;
                pendingImpactPoint = strongestContactPoint;
            }
            hasPendingImpact = true;
        }

        private void FixedUpdate()
        {
            if (!hasPendingImpact)
                return;

            float impactSpeed = pendingImpactSpeed;
            Vector3 contactPoint = pendingImpactPoint;
            hasPendingImpact = false;
            pendingImpactSpeed = 0f;
            pendingImpactPoint = Vector3.zero;

            ApplyImpact(impactSpeed, contactPoint);
        }

        public void ApplyImpact(float impactSpeed, Vector3 worldContact)
        {
            float severity = VesselImpactMath.CalculateHullDamage(
                impactSpeed, impactThreshold, damagePerImpactSpeed,
                MinimumImpactDamage, MaximumImpactDamage);
            if (severity <= 0f)
                return;

            hullIntegrity = Mathf.Clamp(hullIntegrity - severity, 0f, 100f);
            engineCondition = Mathf.Clamp(
                engineCondition - VesselImpactMath.CalculateEngineDamage(severity, EngineDamageRatio),
                0f, 100f);

            if (!IsFinite(worldContact))
                worldContact = transform.position;

            Vector3 local = transform.InverseTransformPoint(worldContact);
            int x = Mathf.Clamp(Mathf.RoundToInt((local.x + 1.05f) / 2.1f * 6f), 0, 6);
            int y = Mathf.Clamp(Mathf.RoundToInt((local.z + 2.8f) / 5.6f * 8f), 0, 8);

            CargoPlacement destroyed;
            bool cellDamaged = cargoHold != null && cargoHold.TryDamageCell(x, y, out destroyed);
            if (cellDamaged)
            {
                lastDamageReport = destroyed == null
                    ? "Impact breached hold cell " + (x + 1) + "-" + (y + 1) + "."
                    : "Impact destroyed " + destroyed.itemId + " in hold cell " + (x + 1) + "-" + (y + 1) + ".";
            }
            else
            {
                lastDamageReport = "Hull struck at " + impactSpeed.ToString("0.0") +
                    " m/s. Hull integrity -" + Mathf.RoundToInt(severity) + "%.";
            }

            PublishHullImpact(impactSpeed, worldContact);
            Debug.LogWarning(lastDamageReport, this);
        }

        private void PublishHullImpact(float impactSpeed, Vector3 worldContact)
        {
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (regionProfile == null) regionProfile = FindFirstObjectByType<DeepOceanSurveyRegion>();

            float intensity = Mathf.InverseLerp(impactThreshold, impactThreshold + 10f, impactSpeed);
            float loudness = Mathf.Lerp(0.2f, 1f, intensity);
            float radius = Mathf.Lerp(180f, 900f, loudness);
            string regionId = regionProfile != null ? regionProfile.RegionId : "unknown_region";
            double gameTimestamp = seaState != null ? seaState.ElapsedGameSeconds : 0.0;
            string eventId = "vessel.hull-impact." + System.Guid.NewGuid().ToString("N");

            AcousticEventBus.Publish(new AcousticSignal(
                eventId,
                AcousticEventKind.HullImpact,
                regionId,
                worldContact,
                loudness,
                radius,
                gameTimestamp));
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public bool RepairCell(int x, int y)
        {
            return cargoHold != null && cargoHold.TryRepairCell(x, y);
        }

        public void RepairHull(float amount)
        {
            hullIntegrity = Mathf.Clamp(hullIntegrity + Mathf.Max(0f, amount), 0f, 100f);
        }

        public void RepairEngine(float amount)
        {
            engineCondition = Mathf.Clamp(engineCondition + Mathf.Max(0f, amount), 0f, 100f);
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = hullIntegrity < 35f ? new Color(1f, 0.42f, 0.32f) : new Color(0.82f, 0.86f, 0.79f) }
            };
            GUI.Box(new Rect(18, 78, 390, 68), GUIContent.none);
            GUI.Label(new Rect(29, 81, 172, 20), "HULL  " + Mathf.RoundToInt(hullIntegrity) + "%", style);
            GUI.Label(new Rect(210, 81, 172, 20), "ENGINE  " + Mathf.RoundToInt(engineCondition) + "%", style);
            GUI.Label(new Rect(29, 105, 370, 34), lastDamageReport, style);
        }

        public void CaptureSaveData(SaveGameData data)
        {
            data.vessel.hullIntegrity = hullIntegrity;
            data.vessel.engineCondition = engineCondition;
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null) return;
            hullIntegrity = Mathf.Clamp(data.vessel.hullIntegrity, 0f, 100f);
            engineCondition = Mathf.Clamp(data.vessel.engineCondition, 0f, 100f);
        }
    }

    /// <summary>Pure collision math; normal speed excludes tangential scraping velocity.</summary>
    public static class VesselImpactMath
    {
        public static float CalculateNormalImpactSpeed(Vector3 relativeVelocity, Vector3 contactNormal)
        {
            if (!IsFinite(relativeVelocity) || !IsFinite(contactNormal) ||
                contactNormal.sqrMagnitude <= 0.0001f)
                return 0f;

            return Mathf.Abs(Vector3.Dot(relativeVelocity, contactNormal.normalized));
        }

        public static float CalculateHullDamage(
            float normalImpactSpeed,
            float impactThreshold,
            float damagePerImpactSpeed,
            float minimumDamage = 2f,
            float maximumDamage = 28f)
        {
            if (!IsFinite(normalImpactSpeed) || !IsFinite(impactThreshold) ||
                !IsFinite(damagePerImpactSpeed) || !IsFinite(minimumDamage) ||
                !IsFinite(maximumDamage) || normalImpactSpeed < 0f)
                return 0f;

            float safeThreshold = Mathf.Max(0f, impactThreshold);
            if (normalImpactSpeed < safeThreshold || damagePerImpactSpeed <= 0f)
                return 0f;

            float lower = Mathf.Max(0f, minimumDamage);
            float upper = Mathf.Max(lower, maximumDamage);
            float rawDamage = (normalImpactSpeed - safeThreshold) * Mathf.Max(0f, damagePerImpactSpeed);
            return Mathf.Clamp(rawDamage, lower, upper);
        }

        public static float CalculateEngineDamage(float hullDamage, float engineDamageRatio)
        {
            if (!IsFinite(hullDamage) || !IsFinite(engineDamageRatio))
                return 0f;

            return Mathf.Max(0f, hullDamage) * Mathf.Clamp01(engineDamageRatio);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
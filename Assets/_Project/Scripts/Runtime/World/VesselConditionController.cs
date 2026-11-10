using UnityEngine;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Converts hard impacts into hull/engine damage and physical cargo-grid failures.
    /// A damaged cell destroys its occupying non-protected item and remains blocked until repaired.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VesselConditionController : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField, Range(0f, 100f)] private float hullIntegrity = 100f;
        [SerializeField, Range(0f, 100f)] private float engineCondition = 100f;
        [SerializeField, Min(0f)] private float impactThreshold = 4.2f;
        [SerializeField, Min(0f)] private float damagePerImpactSpeed = 2.2f;

        private Rigidbody body;
        private string lastDamageReport = "Hull condition stable.";

        public float HullIntegrity => hullIntegrity;
        public float EngineCondition => engineCondition;
        public string LastDamageReport => lastDamageReport;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (cargoHold == null) cargoHold = GetComponent<SpatialCargoHold>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null ||
                collision.collider.CompareTag("SafeHarbourStructure"))
                return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < impactThreshold) return;

            Vector3 contact = collision.contactCount > 0
                ? collision.GetContact(0).point
                : transform.position;
            ApplyImpact(impactSpeed, contact);
        }

        public void ApplyImpact(float impactSpeed, Vector3 worldContact)
        {
            if (impactSpeed < impactThreshold) return;

            float severity = Mathf.Clamp((impactSpeed - impactThreshold) * damagePerImpactSpeed, 2f, 28f);
            hullIntegrity = Mathf.Clamp(hullIntegrity - severity, 0f, 100f);
            engineCondition = Mathf.Clamp(engineCondition - severity * 0.18f, 0f, 100f);

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
                lastDamageReport = "Hull struck hard. Hull integrity -" + Mathf.RoundToInt(severity) + "%.";
            }

            if (body != null)
            {
                body.linearVelocity *= 0.72f;
                body.angularVelocity *= 0.55f;
            }

            Debug.LogWarning(lastDamageReport, this);
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
            GUI.Box(new Rect(18, 78, 190, 48), GUIContent.none);
            GUI.Label(new Rect(29, 81, 172, 20), "HULL  " + Mathf.RoundToInt(hullIntegrity) + "%", style);
            GUI.Label(new Rect(29, 101, 172, 20), "ENGINE  " + Mathf.RoundToInt(engineCondition) + "%", style);
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
}
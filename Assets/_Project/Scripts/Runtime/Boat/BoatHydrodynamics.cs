using UnityEngine;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Four-point spring buoyancy driven by the same analytic water surface as the visual waves.
    /// Each point applies force at its hull position, allowing heave, pitch and roll to emerge
    /// from Rigidbody physics without a full fluid solver.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatHydrodynamics : MonoBehaviour
    {
        [SerializeField] private MaritimeSurfaceBehaviour waterSurface;
        [SerializeField] private Vector3[] buoyancyOffsets =
        {
            new Vector3(-0.72f, -0.22f, -1.72f),
            new Vector3(0.72f, -0.22f, -1.72f),
            new Vector3(-0.72f, -0.22f, 1.72f),
            new Vector3(0.72f, -0.22f, 1.72f)
        };
        [SerializeField, Min(0.05f)] private float targetDraft = 0.18f;
        [SerializeField, Min(0f)] private float maximumSubmersion = 0.65f;
        [SerializeField, Min(0.1f)] private float buoyancySafetyMultiplier = 1.65f;
        [SerializeField, Min(0f)] private float rollDamping = 2.1f;
        [SerializeField, Min(0f)] private float pitchDamping = 1.8f;

        private Rigidbody body;
        private float springStrength;
        private float springDamping;
        private float maximumPointForce;

        public MaritimeSurfaceBehaviour WaterSurface => waterSurface;

        public void Configure(MaritimeSurfaceBehaviour surface)
        {
            waterSurface = surface;
            RecalculateSpringSettings();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            RecalculateSpringSettings();
        }

        private void Start()
        {
            if (waterSurface == null)
                waterSurface = FindFirstObjectByType<MaritimeSurfaceBehaviour>();
            if (waterSurface == null)
                Debug.LogError("BoatHydrodynamics requires a MaritimeSurfaceBehaviour in the active scene.", this);
            RecalculateSpringSettings();
        }

        private void OnValidate()
        {
            if (buoyancyOffsets == null || buoyancyOffsets.Length == 0)
            {
                buoyancyOffsets = new[]
                {
                    new Vector3(-0.72f, -0.22f, -1.72f),
                    new Vector3(0.72f, -0.22f, -1.72f),
                    new Vector3(-0.72f, -0.22f, 1.72f),
                    new Vector3(0.72f, -0.22f, 1.72f)
                };
            }
            targetDraft = Mathf.Max(0.05f, targetDraft);
            maximumSubmersion = Mathf.Max(targetDraft, maximumSubmersion);
            RecalculateSpringSettings();
        }

        private void FixedUpdate()
        {
            if (body == null || waterSurface == null || buoyancyOffsets == null || buoyancyOffsets.Length == 0)
                return;

            for (int index = 0; index < buoyancyOffsets.Length; index++)
            {
                Vector3 point = transform.TransformPoint(buoyancyOffsets[index]);
                if (!waterSurface.TrySampleSurface(point, out Vector3 surfacePoint,
                    out Vector3 surfaceNormal, out Vector3 surfaceVelocity))
                    continue;

                float depth = Vector3.Dot(surfacePoint - point, surfaceNormal);
                depth = BoatHydrodynamicsMath.ClampSubmersionDepth(depth, maximumSubmersion);
                if (depth <= 0f) continue;

                Vector3 pointVelocity = body.GetPointVelocity(point);
                float relativeNormalVelocity = Vector3.Dot(pointVelocity - surfaceVelocity, surfaceNormal);
                float force = BoatHydrodynamicsMath.CalculateBuoyancyForce(
                    depth, relativeNormalVelocity, springStrength, springDamping, maximumPointForce);
                if (force > 0f)
                    body.AddForceAtPosition(surfaceNormal * force, point, ForceMode.Force);
            }

            // The hull can respond to waves, but the workboat should not tumble from small chop.
            Vector3 localAngularVelocity = transform.InverseTransformDirection(body.angularVelocity);
            Vector3 stabilizingTorque =
                -transform.right * (localAngularVelocity.x * rollDamping) -
                transform.forward * (localAngularVelocity.z * pitchDamping);
            body.AddTorque(stabilizingTorque, ForceMode.Acceleration);
        }

        private void RecalculateSpringSettings()
        {
            if (body == null || buoyancyOffsets == null || buoyancyOffsets.Length == 0) return;
            float pointMass = body.mass / buoyancyOffsets.Length;
            springStrength = (body.mass * Physics.gravity.magnitude) /
                             (buoyancyOffsets.Length * targetDraft);
            springDamping = 2f * Mathf.Sqrt(springStrength * pointMass);
            maximumPointForce = (body.mass * Physics.gravity.magnitude * buoyancySafetyMultiplier) /
                                buoyancyOffsets.Length;
        }
    }

    public static class BoatHydrodynamicsMath
    {
        public static float ClampSubmersionDepth(float depth, float maximumSubmersion)
        {
            if (float.IsNaN(depth) || float.IsInfinity(depth) ||
                float.IsNaN(maximumSubmersion) || float.IsInfinity(maximumSubmersion))
                return 0f;

            return Mathf.Clamp(depth, 0f, Mathf.Max(0f, maximumSubmersion));
        }

        public static float CalculateBuoyancyForce(
            float depth, float relativeNormalVelocity,
            float springStrength, float springDamping, float maximumPointForce)
        {
            if (float.IsNaN(depth) || float.IsInfinity(depth) ||
                float.IsNaN(relativeNormalVelocity) || float.IsInfinity(relativeNormalVelocity) ||
                float.IsNaN(springStrength) || float.IsInfinity(springStrength) || springStrength < 0f ||
                float.IsNaN(springDamping) || float.IsInfinity(springDamping) || springDamping < 0f ||
                float.IsNaN(maximumPointForce) || float.IsInfinity(maximumPointForce) || maximumPointForce < 0f)
                return 0f;
            if (depth <= 0f || maximumPointForce <= 0f) return 0f;
            return Mathf.Clamp(depth * springStrength - relativeNormalVelocity * springDamping, 0f, maximumPointForce);
        }
    }
}
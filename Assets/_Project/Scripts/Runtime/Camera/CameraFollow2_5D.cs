using UnityEngine;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.CameraSystem
{
    /// <summary>
    /// Elevated orthographic chase camera for the 2.5D boat prototype.
    /// Anticipates the vessel's actual travel vector and reacts gently to the
    /// shared maritime surface without rotating toward threats.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraFollow2_5D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody targetBody;
        [SerializeField] private MaritimeSurfaceBehaviour seaSurface;

        [Header("Framing")]
        [SerializeField] private Vector3 offset = new Vector3(0.0f, 48.0f, -36.0f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.28f;
        [SerializeField] private Vector3 cameraEulerAngles = new Vector3(55.0f, 0.0f, 0.0f);

        [Header("Movement anticipation")]
        [SerializeField, Min(0.0f)] private float lookAheadSeconds = 0.72f;
        [SerializeField, Min(0.0f)] private float maximumLookAheadDistance = 6.0f;

        [Header("Turning response")]
        [SerializeField, Min(0.0f)] private float turnBankSensitivity = 1.35f;
        [SerializeField, Range(0.0f, 6.0f)] private float maximumTurnBank = 2.5f;

        [Header("Sea response")]
        [SerializeField, Range(0.0f, 1.0f)] private float waveBobInfluence = 0.45f;
        [SerializeField, Range(0.0f, 1.0f)] private float waveTiltInfluence = 0.40f;
        [SerializeField, Range(0.0f, 5.0f)] private float maximumWaveTilt = 1.5f;
        [SerializeField, Min(0.01f)] private float rotationSmoothTime = 0.34f;

        private Vector3 smoothVelocity;
        private Vector3 rotationSmoothVelocity;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                targetBody = target != null ? target.GetComponent<Rigidbody>() : null;
            }
        }

        public Vector3 Offset { get => offset; set => offset = value; }
        public Vector3 CameraEulerAngles { get => cameraEulerAngles; set => cameraEulerAngles = value; }
        public float SmoothTime { get => smoothTime; set => smoothTime = Mathf.Max(0.01f, value); }
        public MaritimeSurfaceBehaviour SeaSurface { get => seaSurface; set => seaSurface = value; }

        private void Awake()
        {
            ResolveTargetBody();
        }

        private void OnValidate()
        {
            smoothTime = Mathf.Max(0.01f, smoothTime);
            rotationSmoothTime = Mathf.Max(0.01f, rotationSmoothTime);
            lookAheadSeconds = Mathf.Max(0.0f, lookAheadSeconds);
            maximumLookAheadDistance = Mathf.Max(0.0f, maximumLookAheadDistance);
            turnBankSensitivity = Mathf.Max(0.0f, turnBankSensitivity);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            ResolveTargetBody();

            Vector3 vesselVelocity = targetBody != null
                ? targetBody.linearVelocity
                : Vector3.zero;

            Vector3 movementLead = CameraFollowMath.CalculateLookAhead(
                vesselVelocity,
                lookAheadSeconds,
                maximumLookAheadDistance);

            float waveHeight = 0.0f;
            Vector3 waveNormal = Vector3.up;

            if (seaSurface != null)
            {
                Vector3 sampledSurfacePosition;
                seaSurface.SampleSurface(target.position, out sampledSurfacePosition, out waveNormal);
                waveHeight = sampledSurfacePosition.y - seaSurface.transform.position.y;
            }

            Vector3 desiredPosition = target.position + offset + movementLead;
            desiredPosition.y += waveHeight * waveBobInfluence;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref smoothVelocity,
                smoothTime);

            float turnBank = targetBody != null
                ? CameraFollowMath.CalculateTurnBank(
                    targetBody.angularVelocity.y,
                    turnBankSensitivity,
                    maximumTurnBank)
                : 0.0f;

            float wavePitch = 0.0f;
            float waveRoll = 0.0f;
            if (seaSurface != null)
            {
                wavePitch = Mathf.Atan2(-waveNormal.z, waveNormal.y) * Mathf.Rad2Deg * waveTiltInfluence;
                waveRoll = Mathf.Atan2(waveNormal.x, waveNormal.y) * Mathf.Rad2Deg * waveTiltInfluence;
                wavePitch = Mathf.Clamp(wavePitch, -maximumWaveTilt, maximumWaveTilt);
                waveRoll = Mathf.Clamp(waveRoll, -maximumWaveTilt, maximumWaveTilt);
            }

            Vector3 desiredEuler = cameraEulerAngles + new Vector3(wavePitch, 0.0f, waveRoll + turnBank);
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.x = Mathf.SmoothDampAngle(
                currentEuler.x, desiredEuler.x, ref rotationSmoothVelocity.x, rotationSmoothTime);
            currentEuler.y = Mathf.SmoothDampAngle(
                currentEuler.y, desiredEuler.y, ref rotationSmoothVelocity.y, rotationSmoothTime);
            currentEuler.z = Mathf.SmoothDampAngle(
                currentEuler.z, desiredEuler.z, ref rotationSmoothVelocity.z, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(currentEuler);
        }

        private void ResolveTargetBody()
        {
            if (target != null && targetBody == null)
                targetBody = target.GetComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Pure camera framing helpers kept separate so movement response can be
    /// regression-tested without constructing a scene or running a camera.
    /// </summary>
    public static class CameraFollowMath
    {
        public static Vector3 CalculateLookAhead(Vector3 velocity, float leadTime, float maximumDistance)
        {
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            float safeLeadTime = Mathf.Max(0.0f, leadTime);
            float safeMaximumDistance = Mathf.Max(0.0f, maximumDistance);

            return Vector3.ClampMagnitude(horizontalVelocity * safeLeadTime, safeMaximumDistance);
        }

        public static float CalculateTurnBank(
            float angularVelocityY,
            float sensitivity,
            float maximumBankDegrees)
        {
            float safeSensitivity = Mathf.Max(0.0f, sensitivity);
            float safeMaximum = Mathf.Max(0.0f, maximumBankDegrees);
            return Mathf.Clamp(-angularVelocityY * safeSensitivity, -safeMaximum, safeMaximum);
        }
    }
}
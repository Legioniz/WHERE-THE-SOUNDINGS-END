using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;
using WhereTheSoundingsEnd.Economy;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Player propulsion and rudder control for the Greyhaven workboat.
    /// It applies forces and torque to a Rigidbody; velocity is never assigned directly.
    /// Buoyancy and hull-water contact are handled by BoatHydrodynamics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoatHydrodynamics))]
    public sealed class BoatPrototypeController : MonoBehaviour, ISaveParticipant
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Propulsion")]
        [SerializeField, Min(0.1f)] private float forwardAcceleration = 2.8f;
        [SerializeField, Min(0.1f)] private float reverseAcceleration = 0.95f;
        [SerializeField, Min(0.1f)] private float brakingAcceleration = 2.6f;
        [SerializeField, Min(0.5f)] private float maximumForwardSpeed = 8.5f;
        [SerializeField, Min(0.5f)] private float maximumReverseSpeed = 3.2f;

        [Header("Rudder")]
        [SerializeField, Min(0.1f)] private float rudderAcceleration = 1.35f;
        [SerializeField, Min(0.1f)] private float rudderFullFlowSpeed = 4.5f;
        [SerializeField, Min(0f)] private float propWashFlow = 1.1f;
        [SerializeField, Min(0f)] private float yawDamping = 1.25f;

        [Header("Water resistance")]
        [SerializeField, Min(0f)] private float lateralResistance = 2.4f;
        [SerializeField, Min(0f)] private float quadraticForwardDrag = 0.018f;

        private Rigidbody body;
        private InputActionMap boatMap;
        private InputAction moveAction;
        private VesselConditionController vesselCondition;
        private VesselFuelController fuelController;
        private VesselEquipmentController equipmentController;
        private ExpeditionEconomyTelemetry economyTelemetry;
        private HarvestingMinigameController harvestingController;
        private Vector2 moveInput;

        // Read-only presentation telemetry. The Rigidbody and force model remain authoritative.
        public float CurrentThrottle { get; private set; }
        public float CurrentForwardSpeedMetersPerSecond { get; private set; }
        public float CurrentSpeedMetersPerSecond { get; private set; }
        public float CurrentSpeedRatio { get; private set; }
        public float CurrentEngineLoad { get; private set; }
        public float NormalizedEngineRpm { get; private set; } = 0.14f;
        public float EngineConditionNormalized { get; private set; } = 1f;

        public InputActionAsset InputActions
        {
            get => inputActions;
            set
            {
                inputActions = value;
                if (isActiveAndEnabled) BindInputActions();
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            vesselCondition = GetComponent<VesselConditionController>();
            fuelController = GetComponent<VesselFuelController>();
            equipmentController = GetComponent<VesselEquipmentController>();
            economyTelemetry = GetComponent<ExpeditionEconomyTelemetry>();
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.None;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0.04f;
            body.angularDamping = 0.18f;
            body.centerOfMass = new Vector3(0f, -0.10f, -0.12f);
        }

        private void OnEnable()
        {
            BindInputActions();
        }

        private void BindInputActions()
        {
            if (inputActions == null)
            {
                // The generated-scene builder assigns the shared asset immediately after AddComponent.
                Debug.LogWarning("BoatPrototypeController is waiting for PlayerControls.inputactions.", this);
                return;
            }

            if (boatMap != null) boatMap.Disable();
            boatMap = inputActions.FindActionMap("Boat", true);
            moveAction = boatMap.FindAction("Move", true);
            boatMap.Enable();
        }

        private void OnDisable()
        {
            boatMap?.Disable();
            boatMap = null;
            moveAction = null;
            moveInput = Vector2.zero;
        }

        private void Update()
        {
            if (moveAction != null)
                moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            // Generated-scene builders may attach vessel condition after this controller.
            // Resolve lazily so damage still affects thrust in every generated scene.
            if (vesselCondition == null)
                vesselCondition = GetComponent<VesselConditionController>();
            if (fuelController == null)
                fuelController = GetComponent<VesselFuelController>();
            if (equipmentController == null)
                equipmentController = GetComponent<VesselEquipmentController>();
            if (economyTelemetry == null)
                economyTelemetry = GetComponent<ExpeditionEconomyTelemetry>();
            if (harvestingController == null)
                harvestingController = GetComponent<HarvestingMinigameController>();

            // Dredging is a stationary deck action; do not let fresh helm input carry the vessel
            // away from the site while the reserved pull is in progress.
            bool movementLocked = harvestingController != null && harvestingController.IsDredging;
            float throttle = movementLocked ? 0f : Mathf.Clamp(moveInput.y, -1f, 1f);
            if (fuelController != null)
            {
                if (!fuelController.CanPropel) throttle = 0f;
                else if (fuelController.FuelUnits <= 0.0001f) throttle = Mathf.Clamp(throttle, -0.18f, 0.18f);
            }
            float steering = movementLocked ? 0f : Mathf.Clamp(moveInput.x, -1f, 1f);
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            float forwardSpeed = Vector3.Dot(body.linearVelocity, forward);
            float engineCondition = vesselCondition != null ? vesselCondition.EngineCondition : 100f;
            float hullIntegrity = vesselCondition != null ? vesselCondition.HullIntegrity : 100f;

            // Presentation telemetry uses horizontal, boat-relative motion. It is deliberately
            // separate from forwardSpeed below so the existing force and steering model is untouched.
            Vector3 motorForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (motorForward.sqrMagnitude > 0.0001f) motorForward.Normalize();
            else motorForward = Vector3.forward;
            Vector3 motorVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            float motorForwardSpeed = Vector3.Dot(motorVelocity, motorForward);
            CurrentThrottle = throttle;
            CurrentForwardSpeedMetersPerSecond = motorForwardSpeed;
            CurrentSpeedMetersPerSecond = motorVelocity.magnitude;
            float speedLimit = motorForwardSpeed < 0f ? maximumReverseSpeed : maximumForwardSpeed;
            CurrentSpeedRatio = Mathf.Clamp01(CurrentSpeedMetersPerSecond / Mathf.Max(0.1f, speedLimit));
            EngineConditionNormalized = Mathf.Clamp01(engineCondition / 100f);
            CurrentEngineLoad = BoatMotorTelemetryMath.CalculateEngineLoad(
                throttle, motorForwardSpeed, maximumForwardSpeed, maximumReverseSpeed);
            NormalizedEngineRpm = BoatMotorTelemetryMath.CalculateEngineRpmNormalized(
                throttle, motorForwardSpeed, maximumForwardSpeed, maximumReverseSpeed);

            if (economyTelemetry != null && (fuelController == null || fuelController.CanPropel))
                economyTelemetry.ObservePropulsion(throttle);

            if (fuelController != null && Mathf.Abs(throttle) >= 0.05f)
            {
                fuelController.ConsumePropulsion(throttle, CurrentEngineLoad, Time.fixedDeltaTime);
                if (!fuelController.CanPropel)
                {
                    throttle = 0f;
                    CurrentThrottle = 0f;
                    CurrentEngineLoad = 0f;
                    NormalizedEngineRpm = BoatMotorTelemetryMath.CalculateEngineRpmNormalized(
                        0f, motorForwardSpeed, maximumForwardSpeed, maximumReverseSpeed);
                }
            }

            float propulsion = BoatMovementMath.CalculatePropulsionAcceleration(
                throttle, forwardSpeed, engineCondition, hullIntegrity,
                maximumForwardSpeed, maximumReverseSpeed,
                forwardAcceleration, reverseAcceleration, brakingAcceleration);
            if (equipmentController != null)
                propulsion *= equipmentController.EnginePowerMultiplier;
            if (Mathf.Abs(propulsion) > 0.0001f)
                body.AddForce(forward * propulsion, ForceMode.Acceleration);

            // A displacement hull resists sideways motion much more strongly than forward travel.
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            float lateralSpeed = Vector3.Dot(horizontalVelocity, right);
            body.AddForce(-right * lateralSpeed * lateralResistance, ForceMode.Acceleration);

            // Quadratic drag rises with speed and naturally limits coasting distance.
            body.AddForce(
                -forward * (forwardSpeed * Mathf.Abs(forwardSpeed) * quadraticForwardDrag),
                ForceMode.Acceleration);

            // The rudder needs water flowing past it. Prop wash gives a small response at low speed,
            // while reverse travel reverses the vessel's steering response.
            float rudderAuthority = BoatMovementMath.CalculateRudderAuthority(
                forwardSpeed, throttle, propWashFlow, rudderFullFlowSpeed);
            float travelDirection = Mathf.Abs(forwardSpeed) > 0.2f
                ? Mathf.Sign(forwardSpeed)
                : Mathf.Abs(throttle) > 0.05f ? Mathf.Sign(throttle) : 1f;
            float yawAcceleration = steering * rudderAcceleration * rudderAuthority * travelDirection;
            float yawVelocity = Vector3.Dot(body.angularVelocity, Vector3.up);
            body.AddTorque(
                Vector3.up * (yawAcceleration - yawVelocity * yawDamping),
                ForceMode.Acceleration);
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null)
                throw new System.ArgumentNullException(nameof(data), "Save data must contain a vessel record.");

            if (body == null) body = GetComponent<Rigidbody>();
            data.vessel.isPresent = true;
            data.vessel.vesselId = "greyhaven.starter_vessel";
            CopyVector(transform.position, data.vessel.position);
            CopyVector(transform.eulerAngles, data.vessel.rotationEuler);
            CopyVector(body.linearVelocity, data.vessel.linearVelocity);
            CopyVector(body.angularVelocity, data.vessel.angularVelocity);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null || !data.vessel.isPresent) return;
            if (body == null) body = GetComponent<Rigidbody>();

            Vector3 position = ToVector(data.vessel.position);
            Quaternion rotation = Quaternion.Euler(ToVector(data.vessel.rotationEuler));
            body.position = position;
            body.rotation = rotation;
            body.linearVelocity = ToVector(data.vessel.linearVelocity);
            body.angularVelocity = ToVector(data.vessel.angularVelocity);
            transform.SetPositionAndRotation(position, rotation);
        }

        private static void CopyVector(Vector3 source, Vector3SaveData destination)
        {
            if (destination == null) throw new System.ArgumentNullException(nameof(destination));
            destination.x = source.x;
            destination.y = source.y;
            destination.z = source.z;
        }

        private static Vector3 ToVector(Vector3SaveData value)
        {
            return value == null ? Vector3.zero : new Vector3(value.x, value.y, value.z);
        }
    }

    /// <summary>Pure helpers keep boat tuning and limits testable without a physics scene.</summary>
    public static class BoatMovementMath
    {
        public static float CalculatePropulsionAcceleration(
            float throttle, float forwardSpeed, float engineCondition, float hullIntegrity,
            float maximumForwardSpeed, float maximumReverseSpeed,
            float forwardAcceleration, float reverseAcceleration, float brakingAcceleration)
        {
            throttle = Mathf.Clamp(throttle, -1f, 1f);
            if (Mathf.Abs(throttle) <= 0.0001f) return 0f;

            float engineFactor = Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(engineCondition / 100f));
            float hullFactor = Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(hullIntegrity / 100f));
            float conditionFactor = engineFactor * hullFactor;
            float forwardLimit = maximumForwardSpeed * Mathf.Lerp(0.65f, 1f, engineFactor) *
                                 Mathf.Lerp(0.55f, 1f, hullFactor);
            float reverseLimit = maximumReverseSpeed * Mathf.Lerp(0.65f, 1f, engineFactor) *
                                 Mathf.Lerp(0.55f, 1f, hullFactor);

            // Opposing throttle brakes first. It only begins reversing once forward speed is near zero.
            if (Mathf.Abs(forwardSpeed) > 0.25f && Mathf.Sign(throttle) != Mathf.Sign(forwardSpeed))
                return throttle * brakingAcceleration * conditionFactor;

            if (throttle > 0f)
            {
                float speedFactor = 1f - Mathf.Clamp01(Mathf.Max(0f, forwardSpeed) / Mathf.Max(0.1f, forwardLimit));
                return throttle * forwardAcceleration * speedFactor * conditionFactor;
            }

            float reverseSpeedFactor = 1f -
                Mathf.Clamp01(Mathf.Max(0f, -forwardSpeed) / Mathf.Max(0.1f, reverseLimit));
            return throttle * reverseAcceleration * reverseSpeedFactor * conditionFactor;
        }

        public static float CalculateRudderAuthority(
            float forwardSpeed, float throttle, float propWashFlow, float fullFlowSpeed)
        {
            float effectiveFlow = Mathf.Max(Mathf.Abs(forwardSpeed), Mathf.Abs(throttle) * propWashFlow);
            return Mathf.Clamp01(effectiveFlow / Mathf.Max(0.1f, fullFlowSpeed));
        }
    }
}
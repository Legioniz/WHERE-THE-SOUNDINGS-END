using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Greyhaven blockout controller only. Values are prototype tuning, not final boat physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatPrototypeController : MonoBehaviour, ISaveParticipant
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Prototype handling")]
        [SerializeField, Min(0.1f)] private float acceleration = 5.0f;
        [SerializeField, Min(0.1f)] private float maximumSpeed = 10.0f;
        [SerializeField, Min(0.1f)] private float turnAcceleration = 2.0f;
        [SerializeField, Min(0.1f)] private float steeringReferenceSpeed = 2.0f;

        private Rigidbody _body;
        private InputActionMap _boatMap;
        private InputAction _moveAction;
        private Vector2 _moveInput;

        public InputActionAsset InputActions
        {
            get => inputActions;
            set => inputActions = value;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.useGravity = false;
            _body.constraints |= RigidbodyConstraints.FreezePositionY |
                                 RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationZ;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.linearDamping = 1.6f;
            _body.angularDamping = 2.0f;
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError("BoatPrototypeController is missing PlayerControls.inputactions.", this);
                enabled = false;
                return;
            }

            _boatMap = inputActions.FindActionMap("Boat", true);
            _moveAction = _boatMap.FindAction("Move", true);
            _boatMap.Enable();
        }

        private void OnDisable()
        {
            _boatMap?.Disable();
            _moveInput = Vector2.zero;
        }

        private void Update()
        {
            if (_moveAction != null)
            {
                _moveInput = Vector2.ClampMagnitude(_moveAction.ReadValue<Vector2>(), 1.0f);
            }
        }

        private void FixedUpdate()
        {
            float throttle = Mathf.Clamp(_moveInput.y, -1.0f, 1.0f);
            float steering = Mathf.Clamp(_moveInput.x, -1.0f, 1.0f);
            _body.AddForce(transform.forward * (throttle * acceleration), ForceMode.Acceleration);

            Vector3 horizontalVelocity = new Vector3(
                _body.linearVelocity.x,
                0.0f,
                _body.linearVelocity.z);

            float speed = horizontalVelocity.magnitude;
            VesselConditionController condition = GetComponent<VesselConditionController>();
            float engineFactor = condition == null ? 1f : Mathf.Lerp(0.15f, 1f, condition.EngineCondition / 100f);
            float effectiveMaximumSpeed = maximumSpeed * engineFactor;
            if (speed > effectiveMaximumSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * effectiveMaximumSpeed;
                _body.linearVelocity = new Vector3(
                    horizontalVelocity.x,
                    _body.linearVelocity.y,
                    horizontalVelocity.z);
            }

            float forwardSpeed = Vector3.Dot(horizontalVelocity, transform.forward);
            float travelDirection = Mathf.Abs(forwardSpeed) > 0.15f
                ? Mathf.Sign(forwardSpeed)
                : 1.0f;
            float steeringAuthority = Mathf.Clamp01(speed / steeringReferenceSpeed);

            _body.AddTorque(
                Vector3.up * (steering * turnAcceleration * steeringAuthority * travelDirection),
                ForceMode.Acceleration);
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null)
            {
                throw new System.ArgumentNullException(nameof(data), "Save data must contain a vessel record.");
            }

            if (_body == null)
            {
                _body = GetComponent<Rigidbody>();
            }

            data.vessel.isPresent = true;
            data.vessel.vesselId = "greyhaven.starter_vessel";
            CopyVector(transform.position, data.vessel.position);
            CopyVector(transform.eulerAngles, data.vessel.rotationEuler);
            CopyVector(_body.linearVelocity, data.vessel.linearVelocity);
            CopyVector(_body.angularVelocity, data.vessel.angularVelocity);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.vessel == null || !data.vessel.isPresent)
            {
                return;
            }

            if (_body == null)
            {
                _body = GetComponent<Rigidbody>();
            }

            Vector3 position = ToVector(data.vessel.position);
            Quaternion rotation = Quaternion.Euler(ToVector(data.vessel.rotationEuler));

            _body.position = position;
            _body.rotation = rotation;
            _body.linearVelocity = ToVector(data.vessel.linearVelocity);
            _body.angularVelocity = ToVector(data.vessel.angularVelocity);
            transform.SetPositionAndRotation(position, rotation);
        }

        private static void CopyVector(Vector3 source, Vector3SaveData destination)
        {
            if (destination == null)
            {
                throw new System.ArgumentNullException(nameof(destination));
            }

            destination.x = source.x;
            destination.y = source.y;
            destination.z = source.z;
        }

        private static Vector3 ToVector(Vector3SaveData value)
        {
            return value == null
                ? Vector3.zero
                : new Vector3(value.x, value.y, value.z);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Greyhaven blockout controller only. Values are prototype tuning, not final boat physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatPrototypeController : MonoBehaviour
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
            if (speed > maximumSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maximumSpeed;
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
                Vector3.up * (-steering * turnAcceleration * steeringAuthority * travelDirection),
                ForceMode.Acceleration);
        }
    }
}

using UnityEngine;

namespace WhereTheSoundingsEnd.CameraSystem
{
    /// <summary>
    /// Fixed-angle orthographic follow camera for the initial 2.5D blockout.
    /// </summary>
    public sealed class CameraFollow2_5D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0.0f, 28.0f, -22.0f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.25f;
        [SerializeField] private Vector3 cameraEulerAngles = new Vector3(55.0f, 0.0f, 0.0f);

        private Vector3 _smoothVelocity;

        public Transform Target { get => target; set => target = value; }
        public Vector3 Offset { get => offset; set => offset = value; }
        public Vector3 CameraEulerAngles { get => cameraEulerAngles; set => cameraEulerAngles = value; }
        public float SmoothTime { get => smoothTime; set => smoothTime = Mathf.Max(0.01f, value); }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _smoothVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(cameraEulerAngles);
        }
    }
}
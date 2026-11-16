using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Lightweight non-physical orbit for panic-only seabird hallucinations.
    /// </summary>
    public sealed class PanicBirdSwarmVisual : MonoBehaviour, WhereTheSoundingsEnd.Streaming.IPooledObject
    {
        [SerializeField] private float orbitDegreesPerSecond = 115f;
        [SerializeField] private float bobAmplitude = 0.35f;
        [SerializeField] private float bobFrequency = 4f;
        private Vector3 origin;

        private void Awake()
        {
            origin = transform.localPosition;
        }

        public void OnPoolRent()
        {
            origin = transform.localPosition;
        }

        public void OnPoolReturn()
        {
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, orbitDegreesPerSecond * Time.deltaTime, Space.Self);
            Vector3 position = origin;
            position.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.localPosition = position;
        }
    }
}
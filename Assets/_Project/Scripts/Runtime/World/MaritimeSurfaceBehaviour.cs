using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Greyhaven prototype ocean surface. A runtime mesh provides readable swell;
    /// rare incidents can flatten it or reverse its travel direction.
    /// Camera sampling uses this same wave function so presentation cannot drift
    /// away from the surface that the player sees.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaritimeSurfaceBehaviour : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] private float normalAmplitude = 0.16f;
        [SerializeField, Min(0.05f)] private float waveFrequency = 0.72f;
        [SerializeField, Min(0.05f)] private float waveSpeed = 1.15f;

        private Mesh generatedMesh;
        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private float calmRemaining;
        private float wrongWayRemaining;
        private float amplitudeBlend;
        private Vector2 travelDirection = new Vector2(0.82f, 0.34f);
        private Vector2 wrongWayDirection = new Vector2(-0.48f, 0.88f);

        private void Awake()
        {
            amplitudeBlend = normalAmplitude;

            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

            generatedMesh = BuildGrid(64);
            filter.sharedMesh = generatedMesh;
            baseVertices = generatedMesh.vertices;
            vertices = new Vector3[baseVertices.Length];
        }

        public void TriggerDeadCalm(float durationSeconds)
        {
            calmRemaining = Mathf.Max(calmRemaining, durationSeconds);
            wrongWayRemaining = 0f;
        }

        public void TriggerWrongWaySwell(float durationSeconds)
        {
            wrongWayRemaining = Mathf.Max(wrongWayRemaining, durationSeconds);
            calmRemaining = 0f;
        }

        /// <summary>
        /// Samples the exact runtime wave equation used by the generated mesh.
        /// The normal is transformed with the inverse-transpose matrix so scaled
        /// water planes retain a correct world-space slope.
        /// </summary>
        public void SampleSurface(
            Vector3 worldPosition,
            out Vector3 worldSurfacePosition,
            out Vector3 worldSurfaceNormal)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            Vector2 direction;
            float phase;
            GetWaveState(out direction, out phase);

            float centerHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z, direction, phase, amplitudeBlend);

            const float sampleStep = 0.05f;
            float leftHeight = EvaluateWaveHeight(
                localPosition.x - sampleStep, localPosition.z, direction, phase, amplitudeBlend);
            float rightHeight = EvaluateWaveHeight(
                localPosition.x + sampleStep, localPosition.z, direction, phase, amplitudeBlend);
            float backHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z - sampleStep, direction, phase, amplitudeBlend);
            float forwardHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z + sampleStep, direction, phase, amplitudeBlend);

            float slopeX = (rightHeight - leftHeight) / (sampleStep * 2.0f);
            float slopeZ = (forwardHeight - backHeight) / (sampleStep * 2.0f);
            Vector3 localNormal = new Vector3(-slopeX, 1.0f, -slopeZ);

            Matrix4x4 normalMatrix = transform.localToWorldMatrix.inverse.transpose;
            worldSurfaceNormal = normalMatrix.MultiplyVector(localNormal).normalized;
            worldSurfacePosition = transform.TransformPoint(
                new Vector3(localPosition.x, centerHeight, localPosition.z));
        }

        /// <summary>
        /// Physics query for the same rendered wave field, including the vertical motion
        /// of the surface. The buoyancy solver uses this rather than a separate flat waterline.
        /// </summary>
        public bool TrySampleSurface(
            Vector3 worldPosition,
            out Vector3 worldSurfacePosition,
            out Vector3 worldSurfaceNormal,
            out Vector3 worldSurfaceVelocity)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            if (Mathf.Abs(localPosition.x) > 5f || Mathf.Abs(localPosition.z) > 5f)
            {
                worldSurfacePosition = Vector3.zero;
                worldSurfaceNormal = Vector3.up;
                worldSurfaceVelocity = Vector3.zero;
                return false;
            }

            SampleSurface(worldPosition, out worldSurfacePosition, out worldSurfaceNormal);

            Vector2 direction;
            float phase;
            GetWaveState(out direction, out phase);
            float phaseRate = waveSpeed * (wrongWayRemaining > 0f ? -1.35f : 1f);
            const float sampleInterval = 0.025f;
            float previousHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z,
                direction, phase - phaseRate * sampleInterval, amplitudeBlend);
            float nextHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z,
                direction, phase + phaseRate * sampleInterval, amplitudeBlend);
            float verticalVelocity = (nextHeight - previousHeight) / (2f * sampleInterval);
            worldSurfaceVelocity = transform.TransformVector(Vector3.up * verticalVelocity);
            return true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (calmRemaining > 0f) calmRemaining -= dt;
            if (wrongWayRemaining > 0f) wrongWayRemaining -= dt;

            float targetAmplitude = calmRemaining > 0f
                ? 0.004f
                : wrongWayRemaining > 0f ? 0.30f : normalAmplitude;

            float blendSpeed = calmRemaining > 0f ? 0.10f : 0.035f;
            amplitudeBlend = Mathf.MoveTowards(amplitudeBlend, targetAmplitude, dt * blendSpeed);

            Vector2 direction;
            float phase;
            GetWaveState(out direction, out phase);

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 point = baseVertices[i];
                float height = EvaluateWaveHeight(
                    point.x, point.z, direction, phase, amplitudeBlend);
                vertices[i] = new Vector3(point.x, height, point.z);
            }

            generatedMesh.vertices = vertices;
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateBounds();
        }

        private void GetWaveState(out Vector2 direction, out float phase)
        {
            bool wrongWay = wrongWayRemaining > 0f;
            direction = wrongWay ? wrongWayDirection : travelDirection;
            phase = Time.time * waveSpeed * (wrongWay ? -1.35f : 1.0f);
        }

        private float EvaluateWaveHeight(
            float x,
            float z,
            Vector2 direction,
            float phase,
            float amplitude)
        {
            float along = (x * direction.x + z * direction.y) * waveFrequency;
            float cross = (x * -direction.y + z * direction.x) * 0.34f;
            float primary = Mathf.Sin(along + phase);
            float secondary = Mathf.Sin(along * 0.53f + cross + phase * 0.68f) * 0.34f;
            float longSwell = Mathf.Sin(cross * 0.19f - phase * 0.24f) * 0.22f;
            return (primary + secondary + longSwell) * amplitude;
        }

        private static Mesh BuildGrid(int resolution)
        {
            Mesh mesh = new Mesh { name = "Greyhaven_Procedural_Swell_Surface" };
            mesh.MarkDynamic();

            Vector3[] meshVertices = new Vector3[(resolution + 1) * (resolution + 1)];
            Vector2[] uv = new Vector2[meshVertices.Length];
            int[] triangles = new int[resolution * resolution * 6];

            for (int z = 0; z <= resolution; z++)
            for (int x = 0; x <= resolution; x++)
            {
                int index = z * (resolution + 1) + x;
                float u = x / (float)resolution;
                float v = z / (float)resolution;
                meshVertices[index] = new Vector3(
                    Mathf.Lerp(-5f, 5f, u), 0f, Mathf.Lerp(-5f, 5f, v));
                uv[index] = new Vector2(u, v);
            }

            int triangleIndex = 0;
            for (int z = 0; z < resolution; z++)
            for (int x = 0; x < resolution; x++)
            {
                int index = z * (resolution + 1) + x;
                triangles[triangleIndex++] = index;
                triangles[triangleIndex++] = index + resolution + 1;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index + resolution + 1;
                triangles[triangleIndex++] = index + resolution + 2;
            }

            mesh.vertices = meshVertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private void OnDestroy()
        {
            if (generatedMesh == null) return;

            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
        }
    }
}
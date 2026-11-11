using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Greyhaven prototype ocean surface. Rendering, buoyancy and camera awareness sample
    /// one continuous analytic wave field. Incident direction and phase changes are eased
    /// so a dead-calm or wrong-way-swell transition cannot teleport the visible wave pattern.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaritimeSurfaceBehaviour : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] private float normalAmplitude = 0.16f;
        [SerializeField, Min(0.05f)] private float waveFrequency = 0.72f;
        [SerializeField, Min(0.05f)] private float waveSpeed = 1.15f;
        [SerializeField, Min(0f)] private float directionBlendSharpness = 1.8f;
        [SerializeField, Min(0.1f)] private float phaseRateAcceleration = 2.5f;
        [SerializeField, Min(0.1f)] private float wrongWayPhaseRateMultiplier = 1.35f;

        private Mesh generatedMesh;
        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private float calmRemaining;
        private float wrongWayRemaining;
        private float amplitudeBlend;
        private float previousAmplitudeBlend;
        private float wavePhase;
        private float previousWavePhase;
        private float wavePhaseRate;
        private float surfaceSampleDeltaTime;
        private Vector2 travelDirection = new Vector2(0.82f, 0.34f);
        private Vector2 wrongWayDirection = new Vector2(-0.48f, 0.88f);
        private Vector2 currentWaveDirection;
        private Vector2 previousWaveDirection;

        private void Awake()
        {
            normalAmplitude = Mathf.Clamp(normalAmplitude, 0f, 0.5f);
            waveFrequency = Mathf.Max(0.05f, waveFrequency);
            waveSpeed = Mathf.Max(0.05f, waveSpeed);
            travelDirection = NormalizeDirection(travelDirection, Vector2.right);
            wrongWayDirection = NormalizeDirection(wrongWayDirection, Vector2.up);
            currentWaveDirection = travelDirection;
            previousWaveDirection = currentWaveDirection;
            wavePhaseRate = waveSpeed;
            amplitudeBlend = normalAmplitude;
            previousAmplitudeBlend = amplitudeBlend;

            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

            generatedMesh = BuildGrid(64);
            filter.sharedMesh = generatedMesh;
            baseVertices = generatedMesh.vertices;
            vertices = new Vector3[baseVertices.Length];

            // The first rendered frame must use the same wave state that physics queries.
            ApplyWaveMesh();
        }

        private void OnValidate()
        {
            normalAmplitude = Mathf.Clamp(normalAmplitude, 0f, 0.5f);
            waveFrequency = Mathf.Max(0.05f, waveFrequency);
            waveSpeed = Mathf.Max(0.05f, waveSpeed);
            directionBlendSharpness = Mathf.Max(0f, directionBlendSharpness);
            phaseRateAcceleration = Mathf.Max(0.1f, phaseRateAcceleration);
            wrongWayPhaseRateMultiplier = Mathf.Max(0.1f, wrongWayPhaseRateMultiplier);
            travelDirection = NormalizeDirection(travelDirection, Vector2.right);
            wrongWayDirection = NormalizeDirection(wrongWayDirection, Vector2.up);
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
        /// Samples the current analytic wave height and world-space normal. The normal is
        /// transformed with the inverse-transpose matrix so scaled/rotated water planes work.
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
        /// Physics/camera query for the rendered wave field. The temporal difference includes
        /// phase, direction and amplitude transitions, so incident ramps affect buoyancy velocity.
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
            float currentHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z, direction, phase, amplitudeBlend);
            float previousHeight = EvaluateWaveHeight(
                localPosition.x, localPosition.z,
                previousWaveDirection, previousWavePhase, previousAmplitudeBlend);

            float verticalVelocity = MaritimeSurfaceMath.EstimateVerticalVelocity(
                previousHeight, currentHeight, surfaceSampleDeltaTime);
            worldSurfaceVelocity = transform.TransformVector(Vector3.up * verticalVelocity);
            return true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Preserve the exact previous rendered state for a full-state velocity estimate.
            previousAmplitudeBlend = amplitudeBlend;
            previousWavePhase = wavePhase;
            previousWaveDirection = currentWaveDirection;
            surfaceSampleDeltaTime = dt;

            if (calmRemaining > 0f) calmRemaining = Mathf.Max(0f, calmRemaining - dt);
            if (wrongWayRemaining > 0f) wrongWayRemaining = Mathf.Max(0f, wrongWayRemaining - dt);

            float targetAmplitude = calmRemaining > 0f
                ? 0.004f
                : wrongWayRemaining > 0f ? 0.30f : normalAmplitude;
            float blendSpeed = calmRemaining > 0f ? 0.10f : 0.035f;
            amplitudeBlend = Mathf.MoveTowards(amplitudeBlend, targetAmplitude, dt * blendSpeed);

            Vector2 targetDirection = wrongWayRemaining > 0f ? wrongWayDirection : travelDirection;
            currentWaveDirection = MaritimeSurfaceMath.BlendDirection(
                currentWaveDirection, targetDirection, directionBlendSharpness, dt);

            float targetPhaseRate = waveSpeed *
                (wrongWayRemaining > 0f ? -wrongWayPhaseRateMultiplier : 1f);
            wavePhaseRate = Mathf.MoveTowards(
                wavePhaseRate, targetPhaseRate, phaseRateAcceleration * dt);
            wavePhase = MaritimeSurfaceMath.AdvancePhase(wavePhase, wavePhaseRate, dt);

            ApplyWaveMesh();
        }

        private void ApplyWaveMesh()
        {
            if (generatedMesh == null || baseVertices == null || vertices == null) return;

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
            direction = currentWaveDirection;
            phase = wavePhase;
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

        private static Vector2 NormalizeDirection(Vector2 direction, Vector2 fallback)
        {
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : fallback.normalized;
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

    /// <summary>Small deterministic helpers for continuous wave-state transitions.</summary>
    public static class MaritimeSurfaceMath
    {
        public static float AdvancePhase(float currentPhase, float phaseRate, float deltaTime)
        {
            return currentPhase + phaseRate * Mathf.Max(0f, deltaTime);
        }

        public static Vector2 BlendDirection(
            Vector2 currentDirection,
            Vector2 targetDirection,
            float sharpness,
            float deltaTime)
        {
            Vector2 fallback = targetDirection.sqrMagnitude > 0.0001f
                ? targetDirection.normalized
                : Vector2.right;
            Vector2 current = currentDirection.sqrMagnitude > 0.0001f
                ? currentDirection.normalized
                : fallback;
            Vector2 target = fallback;
            float blend = 1f - Mathf.Exp(-Mathf.Max(0f, sharpness) * Mathf.Max(0f, deltaTime));
            Vector2 result = Vector2.Lerp(current, target, blend);
            return result.sqrMagnitude > 0.0001f ? result.normalized : target;
        }

        public static float EstimateVerticalVelocity(
            float previousHeight,
            float currentHeight,
            float elapsedSeconds)
        {
            if (float.IsNaN(previousHeight) || float.IsInfinity(previousHeight) ||
                float.IsNaN(currentHeight) || float.IsInfinity(currentHeight) ||
                float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds) ||
                elapsedSeconds <= 0.000001f)
                return 0f;

            return (currentHeight - previousHeight) / elapsedSeconds;
        }
    }
}
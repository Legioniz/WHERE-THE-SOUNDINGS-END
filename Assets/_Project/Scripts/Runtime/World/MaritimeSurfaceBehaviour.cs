using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Greyhaven prototype ocean surface. A dense runtime mesh provides readable swell;
    /// rare incidents can flatten it or reverse its travel direction.
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
        private float amplitudeBlend = 1f;
        private Vector2 travelDirection = new Vector2(0.82f, 0.34f);
        private Vector2 wrongWayDirection = new Vector2(-0.48f, 0.88f);

        private void Awake()
        {
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

        private void Update()
        {
            float dt = Time.deltaTime;
            if (calmRemaining > 0f) calmRemaining -= dt;
            if (wrongWayRemaining > 0f) wrongWayRemaining -= dt;

            float targetAmplitude = calmRemaining > 0f ? 0.004f : wrongWayRemaining > 0f ? 0.30f : normalAmplitude;
            amplitudeBlend = Mathf.MoveTowards(amplitudeBlend, targetAmplitude, dt * (calmRemaining > 0f ? 0.10f : 0.035f));

            Vector2 direction = wrongWayRemaining > 0f ? wrongWayDirection : travelDirection;
            float phase = Time.time * waveSpeed * (wrongWayRemaining > 0f ? -1.35f : 1f);

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 p = baseVertices[i];
                float along = (p.x * direction.x + p.z * direction.y) * waveFrequency;
                float cross = (p.x * -direction.y + p.z * direction.x) * 0.34f;
                float primary = Mathf.Sin(along + phase);
                float secondary = Mathf.Sin(along * 0.53f + cross + phase * 0.68f) * 0.34f;
                float longSwell = Mathf.Sin(cross * 0.19f - phase * 0.24f) * 0.22f;
                vertices[i] = new Vector3(p.x, (primary + secondary + longSwell) * amplitudeBlend, p.z);
            }

            generatedMesh.vertices = vertices;
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateBounds();
        }

        private static Mesh BuildGrid(int resolution)
        {
            Mesh mesh = new Mesh { name = "Greyhaven_Procedural_Swell_Surface" };
            mesh.MarkDynamic();
            Vector3[] verts = new Vector3[(resolution + 1) * (resolution + 1)];
            Vector2[] uv = new Vector2[verts.Length];
            int[] triangles = new int[resolution * resolution * 6];

            for (int z = 0; z <= resolution; z++)
            for (int x = 0; x <= resolution; x++)
            {
                int index = z * (resolution + 1) + x;
                float u = x / (float)resolution;
                float v = z / (float)resolution;
                verts[index] = new Vector3(Mathf.Lerp(-5f, 5f, u), 0f, Mathf.Lerp(-5f, 5f, v));
                uv[index] = new Vector2(u, v);
            }

            int t = 0;
            for (int z = 0; z < resolution; z++)
            for (int x = 0; x < resolution; x++)
            {
                int i = z * (resolution + 1) + x;
                triangles[t++] = i;
                triangles[t++] = i + resolution + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + resolution + 1;
                triangles[t++] = i + resolution + 2;
            }

            mesh.vertices = verts;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private void OnDestroy()
        {
            if (generatedMesh != null)
            {
                if (Application.isPlaying) Destroy(generatedMesh);
                else DestroyImmediate(generatedMesh);
            }
        }
    }
}
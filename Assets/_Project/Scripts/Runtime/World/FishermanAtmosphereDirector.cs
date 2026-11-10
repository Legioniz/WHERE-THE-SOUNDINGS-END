using UnityEngine;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Adds restrained, non-combat maritime incidents to the Greyhaven prototype.
    /// Incidents borrow their language from ordinary seamanship: uncertain bearings,
    /// buoy signals, hull sounds and lights seen through poor visibility.
    /// They never create damage, collisions or guaranteed supernatural explanations.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FishermanAtmosphereDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private Rigidbody vesselBody;
        [SerializeField] private Transform worldRoot;

        [Header("Incident pacing (in-game seconds)")]
        [SerializeField, Min(60f)] private float firstIncidentDelay = 420f;
        [SerializeField, Min(120f)] private float minimumIncidentInterval = 1080f;
        [SerializeField, Min(120f)] private float maximumIncidentInterval = 2100f;
        [SerializeField, Range(0f, 1f)] private float daytimePressureThreshold = 0.32f;

        private AudioSource incidentAudio;
        private AudioClip buoyBellClip;
        private AudioClip hullKnockClip;
        private AudioClip distantCallClip;
        private float incidentCountdown;
        private float captionRemaining;
        private string currentCaption = string.Empty;
        private GameObject falseNavigationLight;
        private Light falseNavigationLightSource;
        private float falseLightRemaining;
        private Material falseLightMaterial;

        private void Awake()
        {
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (vesselBody == null)
            {
                Boat.BoatPrototypeController boat = FindFirstObjectByType<Boat.BoatPrototypeController>();
                if (boat != null) vesselBody = boat.GetComponent<Rigidbody>();
            }
            if (worldRoot == null) worldRoot = transform;

            incidentAudio = gameObject.AddComponent<AudioSource>();
            incidentAudio.playOnAwake = false;
            incidentAudio.loop = false;
            incidentAudio.spatialBlend = 1f;
            incidentAudio.minDistance = 5f;
            incidentAudio.maxDistance = 95f;
            incidentAudio.rolloffMode = AudioRolloffMode.Linear;
            incidentAudio.dopplerLevel = 0f;

            buoyBellClip = BuildBellClip();
            hullKnockClip = BuildKnockClip();
            distantCallClip = BuildDistantCallClip();
            incidentCountdown = firstIncidentDelay;
        }

        private void Update()
        {
            float realDelta = Time.deltaTime;
            if (captionRemaining > 0f)
            {
                captionRemaining -= realDelta;
                if (captionRemaining <= 0f) currentCaption = string.Empty;
            }

            UpdateFalseLight(realDelta);

            if (seaState == null || vesselBody == null || !seaState.IsAdvancing)
                return;

            bool pressured = seaState.IsNight || seaState.PanicLevel >= daytimePressureThreshold;
            if (!pressured) return;

            incidentCountdown -= realDelta * 72f;
            if (incidentCountdown > 0f) return;

            incidentCountdown = Random.Range(minimumIncidentInterval, maximumIncidentInterval);
            RollIncident();
        }

        private void RollIncident()
        {
            float roll = Random.value;
            if (roll < 0.36f)
            {
                PlayAtBoat(hullKnockClip, vesselBody.position + vesselBody.transform.right * Random.Range(-0.8f, 0.8f), 0.42f);
                ShowCaption("[two hollow knocks travel through the hull]");
            }
            else if (roll < 0.69f)
            {
                Vector3 bellPosition = vesselBody.position + vesselBody.transform.forward * Random.Range(28f, 52f)
                    + vesselBody.transform.right * Random.Range(-24f, 24f);
                PlayAtBoat(buoyBellClip, bellPosition, 0.31f);
                ShowCaption("[a navigation bell rings somewhere beyond the fog]");
            }
            else if (roll < 0.91f)
            {
                SpawnFalseNavigationLight();
                ShowCaption("[a warm light drifts across the water. No wake follows it]");
            }
            else
            {
                PlayAtBoat(distantCallClip, vesselBody.position + vesselBody.transform.forward * Random.Range(35f, 65f), 0.2f);
                ShowCaption("[a long animal call carries over the water]");
            }
        }

        private void PlayAtBoat(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null || incidentAudio == null) return;
            incidentAudio.transform.position = position;
            incidentAudio.PlayOneShot(clip, volume);
        }

        private void ShowCaption(string caption)
        {
            currentCaption = caption;
            captionRemaining = 4.5f;
        }

        private void SpawnFalseNavigationLight()
        {
            if (falseNavigationLight != null) return;

            Vector3 position = vesselBody.position
                + vesselBody.transform.forward * Random.Range(42f, 68f)
                + vesselBody.transform.right * Random.Range(-22f, 22f);
            position.y = 0.8f;

            falseNavigationLight = new GameObject("Atmosphere_False_Navigation_Light");
            falseNavigationLight.transform.SetParent(worldRoot, true);
            falseNavigationLight.transform.position = position;

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "Distant_Warm_Glow";
            glow.transform.SetParent(falseNavigationLight.transform, false);
            glow.transform.localScale = new Vector3(0.55f, 0.32f, 0.55f);
            Collider glowCollider = glow.GetComponent<Collider>();
            if (glowCollider != null) Destroy(glowCollider);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                falseLightMaterial = new Material(shader);
                falseLightMaterial.color = new Color(1f, 0.62f, 0.29f, 1f);
                glow.GetComponent<Renderer>().sharedMaterial = falseLightMaterial;
            }

            falseNavigationLightSource = falseNavigationLight.AddComponent<Light>();
            falseNavigationLightSource.type = LightType.Point;
            falseNavigationLightSource.color = new Color(1f, 0.53f, 0.25f);
            falseNavigationLightSource.intensity = 2.1f;
            falseNavigationLightSource.range = 13f;
            falseNavigationLightSource.shadows = LightShadows.None;

            falseLightRemaining = Random.Range(7f, 12f);
        }

        private void UpdateFalseLight(float deltaTime)
        {
            if (falseNavigationLight == null) return;
            falseLightRemaining -= deltaTime;

            // A slight lateral drift makes the light hard to classify, not a chase target.
            falseNavigationLight.transform.position += Vector3.right * (Mathf.Sin(Time.time * 0.7f) * deltaTime * 0.28f);
            if (falseNavigationLightSource != null)
                falseNavigationLightSource.intensity = Mathf.Lerp(0f, 2.1f, Mathf.Clamp01(falseLightRemaining / 2.5f));

            if (falseLightRemaining > 0f) return;
            Destroy(falseNavigationLight);
            falseNavigationLight = null;
            falseNavigationLightSource = null;
            if (falseLightMaterial != null)
            {
                Destroy(falseLightMaterial);
                falseLightMaterial = null;
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(currentCaption) || captionRemaining <= 0f) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };
            style.normal.textColor = new Color(0.82f, 0.84f, 0.81f);
            GUI.Label(new Rect(Screen.width * 0.5f - 300f, Screen.height - 92f, 600f, 34f), currentCaption, style);
        }

        private void OnDestroy()
        {
            if (buoyBellClip != null) Destroy(buoyBellClip);
            if (hullKnockClip != null) Destroy(hullKnockClip);
            if (distantCallClip != null) Destroy(distantCallClip);
            if (falseLightMaterial != null) Destroy(falseLightMaterial);
            if (falseNavigationLight != null) Destroy(falseNavigationLight);
        }

        private static AudioClip BuildBellClip()
        {
            const int sampleRate = 22050;
            const float duration = 2.6f;
            int count = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 1.8f) * Mathf.Clamp01(t * 45f);
                float tone = Mathf.Sin(2f * Mathf.PI * 392f * t)
                    + 0.42f * Mathf.Sin(2f * Mathf.PI * 587f * t)
                    + 0.18f * Mathf.Sin(2f * Mathf.PI * 784f * t);
                samples[i] = tone * envelope * 0.22f;
            }
            return CreateClip("Procedural_Distant_Buoy_Bell", samples, sampleRate);
        }

        private static AudioClip BuildKnockClip()
        {
            const int sampleRate = 22050;
            const float duration = 1.15f;
            int count = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float first = DampedKnock(t, 0.04f, 0.16f);
                float second = DampedKnock(t, 0.39f, 0.20f);
                samples[i] = (first + second) * 0.34f;
            }
            return CreateClip("Procedural_Hollow_Hull_Knocks", samples, sampleRate);
        }

        private static float DampedKnock(float time, float start, float length)
        {
            float local = time - start;
            if (local < 0f || local > length) return 0f;
            float envelope = Mathf.Exp(-local * 23f);
            float low = Mathf.Sin(2f * Mathf.PI * 74f * local);
            float rattle = Mathf.Sin(2f * Mathf.PI * 191f * local) * 0.22f;
            return (low + rattle) * envelope;
        }

        private static AudioClip BuildDistantCallClip()
        {
            const int sampleRate = 22050;
            const float duration = 3.4f;
            int count = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float frequency = 170f + Mathf.Sin(t * 2.2f) * 24f;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.12f;
            }
            return CreateClip("Procedural_Distant_Animal_Call", samples, sampleRate);
        }

        private static AudioClip CreateClip(string clipName, float[] samples, int sampleRate)
        {
            AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
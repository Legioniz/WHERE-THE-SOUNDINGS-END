using UnityEngine;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Adds restrained, non-combat maritime incidents to the Greyhaven prototype.
    /// Incidents borrow their language from ordinary seamanship: uncertain bearings,
    /// buoy signals, hull sounds and lights seen through poor visibility.
    /// They never create damage, collisions or guaranteed supernatural explanations.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FishermanAtmosphereDirector : MonoBehaviour, ISaveParticipant
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
        private MaritimeSurfaceBehaviour seaSurface;
        private AudioSource[] hushSources = System.Array.Empty<AudioSource>();
        private float[] hushBaseVolumes = System.Array.Empty<float>();
        private float hushRemaining;
        private float hushBlend = 1f;

        private void Awake()
        {
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            if (vesselBody == null)
            {
                WhereTheSoundingsEnd.Boat.BoatPrototypeController boat = FindFirstObjectByType<Boat.BoatPrototypeController>();
                if (boat != null) vesselBody = boat.GetComponent<Rigidbody>();
            }
            if (worldRoot == null) worldRoot = transform;

            GameObject waterObject = GameObject.Find("Sheltered_Bay_Surface");
            if (waterObject != null)
            {
                seaSurface = waterObject.GetComponent<MaritimeSurfaceBehaviour>();
                if (seaSurface == null) seaSurface = waterObject.AddComponent<MaritimeSurfaceBehaviour>();
            }

            hushSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            hushBaseVolumes = new float[hushSources.Length];
            for (int i = 0; i < hushSources.Length; i++)
                hushBaseVolumes[i] = hushSources[i] != null ? hushSources[i].volume : 0f;

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
            UpdateHush(realDelta);

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
            if (roll < 0.25f) TriggerDeadCalm();
            else if (roll < 0.48f) TriggerWrongWaySwell();
            else if (roll < 0.62f)
            {
                PlayAtBoat(hullKnockClip, vesselBody.position + vesselBody.transform.right * Random.Range(-0.8f, 0.8f), 0.42f);
                ShowCaption("[two hollow knocks travel through the hull]");
            }
            else if (roll < 0.77f)
            {
                Vector3 bellPosition = vesselBody.position + vesselBody.transform.forward * Random.Range(28f, 52f)
                    + vesselBody.transform.right * Random.Range(-24f, 24f);
                PlayAtBoat(buoyBellClip, bellPosition, 0.31f);
                ShowCaption("[a navigation bell rings somewhere beyond the fog]");
            }
            else if (roll < 0.92f)
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

        private void TriggerDeadCalm()
        {
            if (seaSurface != null) seaSurface.TriggerDeadCalm(Random.Range(12f, 20f));
            hushRemaining = Random.Range(8f, 14f);
            ShowCaption("[the wind stops. The waves flatten. Even the water against the hull goes quiet]");
        }

        private void TriggerWrongWaySwell()
        {
            if (seaSurface != null) seaSurface.TriggerWrongWaySwell(Random.Range(14f, 22f));
            hushRemaining = Random.Range(3f, 6f);
            ShowCaption("[the swell begins moving against the wind]");
        }

        private void UpdateHush(float deltaTime)
        {
            if (hushRemaining > 0f) hushRemaining -= deltaTime;
            float target = hushRemaining > 0f ? 0f : 1f;
            hushBlend = Mathf.MoveTowards(hushBlend, target, deltaTime * (target < hushBlend ? 1.8f : 0.32f));

            for (int i = 0; i < hushSources.Length; i++)
            {
                if (hushSources[i] == null || hushSources[i] == incidentAudio) continue;
                hushSources[i].volume = hushBaseVolumes[i] * Mathf.Lerp(0.025f, 1f, hushBlend);
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

        private void SpawnFalseNavigationLight(Vector3? restoredPosition = null, float restoredRemainingSeconds = -1f)
        {
            if (falseNavigationLight != null) return;

            Vector3 position = restoredPosition.HasValue
                ? restoredPosition.Value
                : vesselBody.position
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

            falseLightRemaining = restoredRemainingSeconds >= 0f ? restoredRemainingSeconds : Random.Range(7f, 12f);
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

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null)
                throw new System.ArgumentNullException(nameof(data), "Save data must contain world state.");

            data.world.atmosphereIncidentCountdownGameSeconds = System.Math.Max(0.0, incidentCountdown);
            data.world.atmosphereCaption = currentCaption ?? string.Empty;
            data.world.atmosphereCaptionRemainingRealSeconds = System.Math.Max(0.0, captionRemaining);
            data.world.atmosphereHushRemainingRealSeconds = System.Math.Max(0.0, hushRemaining);
            data.world.atmosphereHushBlend = Mathf.Clamp01(hushBlend);
            data.world.atmosphereFalseLightActive = falseNavigationLight != null;
            data.world.atmosphereFalseLightRemainingRealSeconds =
                falseNavigationLight != null ? System.Math.Max(0.0, falseLightRemaining) : 0.0;
            CopyVector(falseNavigationLight != null ? falseNavigationLight.transform.position : Vector3.zero,
                data.world.atmosphereFalseLightPosition);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;

            incidentCountdown = (float)System.Math.Max(0.0, data.world.atmosphereIncidentCountdownGameSeconds);
            currentCaption = data.world.atmosphereCaption ?? string.Empty;
            captionRemaining = (float)System.Math.Max(0.0, data.world.atmosphereCaptionRemainingRealSeconds);
            hushRemaining = (float)System.Math.Max(0.0, data.world.atmosphereHushRemainingRealSeconds);
            hushBlend = Mathf.Clamp01(data.world.atmosphereHushBlend);

            if (data.world.atmosphereFalseLightActive &&
                data.world.atmosphereFalseLightRemainingRealSeconds > 0.0)
            {
                SpawnFalseNavigationLight(
                    ToVector(data.world.atmosphereFalseLightPosition),
                    (float)data.world.atmosphereFalseLightRemainingRealSeconds);
            }
        }

        private static void CopyVector(Vector3 source, Vector3SaveData destination)
        {
            if (destination == null) throw new System.ArgumentNullException(nameof(destination));
            destination.x = source.x;
            destination.y = source.y;
            destination.z = source.z;
        }

        private static Vector3 ToVector(Vector3SaveData source)
        {
            return source == null ? Vector3.zero : new Vector3(source.x, source.y, source.z);
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

namespace WhereTheSoundingsEnd.World
{
    /// <summary>Attaches the atmosphere layer to scenes that already contain the sea-state system.</summary>
    internal static class FishermanAtmosphereBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToGreyhaven()
        {
            SeaStateDirector seaState = Object.FindFirstObjectByType<SeaStateDirector>();
            if (seaState == null || seaState.GetComponent<FishermanAtmosphereDirector>() != null)
                return;

            seaState.gameObject.AddComponent<FishermanAtmosphereDirector>();
        }
    }
}
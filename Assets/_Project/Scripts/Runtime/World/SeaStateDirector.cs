using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Core;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// Greyhaven action-driven clock and psychological pressure director.
    /// Time advances only while the vessel is genuinely moving or a harvest action is active.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SeaStateDirector : MonoBehaviour, ISaveParticipant
    {
        [Header("References")]
        [SerializeField] private Rigidbody vesselBody;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private HarvestingMinigameController harvesting;
        [SerializeField] private SpatialCargoHold cargoHold;

        [Header("Clock")]
        [SerializeField, Min(1f)] private float gameSecondsPerRealSecond = 72f;
        [SerializeField, Min(0f)] private float movementThreshold = 0.25f;
        [SerializeField, Range(0f, 23f)] private float startingHour = 7f;
        [SerializeField] private float nightStartsAtHour = 19f;
        [SerializeField] private float dayStartsAtHour = 5f;

        [Header("Panic")]
        [SerializeField, Range(0f, 1f)] private float panicLevel;
        [SerializeField, Min(0f)] private float nightPanicPerGameMinute = 0.018f;
        [SerializeField, Min(0f)] private float fogPanicPerGameMinute = 0.009f;
        [SerializeField, Min(0f)] private float daylightRecoveryPerGameMinute = 0.012f;
        [SerializeField, Min(0f)] private float safeHarbourRecoveryPerGameMinute = 0.04f;

        [Header("Atmosphere")]
        [SerializeField] private float baseFogDensity = 0.0018f;
        [SerializeField] private Color dayFog = new Color(0.15f, 0.20f, 0.21f);
        [SerializeField] private Color nightFog = new Color(0.025f, 0.045f, 0.065f);

        private double elapsedGameSeconds;
        private double timeOfDaySeconds;
        private float seaDisturbance;
        private float weatherFogPressure;
        private float fogRemainingGameSeconds;
        private float weatherShiftCooldown;
        private Material birdMaterial;
        private bool harvestingActive;
        private float hallucinationCooldown;
        private bool birdEventPending;
        private float birdEventCountdown;
        private float[] sourceBasePitch = System.Array.Empty<float>();
        private float[] sourceBaseVolume = System.Array.Empty<float>();
        private string latestOmen = string.Empty;
        private Light[] sceneLights = System.Array.Empty<Light>();
        private AudioSource[] soundscapeSources = System.Array.Empty<AudioSource>();

        public double ElapsedGameSeconds => elapsedGameSeconds;
        public double TimeOfDaySeconds => timeOfDaySeconds;
        public int DayNumber => Mathf.FloorToInt((float)(elapsedGameSeconds / 86400.0)) + 1;
        public float PanicLevel => panicLevel;
        public float SeaDisturbance => seaDisturbance;
        public bool IsNight => CurrentHour >= nightStartsAtHour || CurrentHour < dayStartsAtHour;
        public float CurrentHour => (float)(timeOfDaySeconds / 3600.0);
        public bool IsAdvancing { get; private set; }
        public string LatestOmen => latestOmen;

        private void Awake()
        {
            sceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            soundscapeSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            sourceBasePitch = new float[soundscapeSources.Length];
            sourceBaseVolume = new float[soundscapeSources.Length];
            for (int i = 0; i < soundscapeSources.Length; i++)
            {
                sourceBasePitch[i] = soundscapeSources[i] != null ? soundscapeSources[i].pitch : 1f;
                sourceBaseVolume[i] = soundscapeSources[i] != null ? soundscapeSources[i].volume : 1f;
            }
            if (timeOfDaySeconds <= 0.0)
                timeOfDaySeconds = startingHour * 3600.0;
            weatherShiftCooldown = Random.Range(900f, 1500f);
        }

        public void Configure(Rigidbody boat, DialogueRunner runner, HarvestingMinigameController harvest, SpatialCargoHold hold)
        {
            vesselBody = boat;
            dialogueRunner = runner;
            harvesting = harvest;
            cargoHold = hold;
        }

        public void SetHarvesting(bool active)
        {
            harvestingActive = active;
        }

        public void AddDisturbance(float amount)
        {
            seaDisturbance = Mathf.Clamp01(seaDisturbance + amount);
        }

        public void SetWeatherFogPressure(float normalizedPressure)
        {
            weatherFogPressure = Mathf.Clamp01(normalizedPressure);
        }

        public void AddPanic(float amount)
        {
            panicLevel = Mathf.Clamp01(panicLevel + amount);
        }

        public bool RollAberration()
        {
            float chance = Mathf.Clamp(0.025f + panicLevel * 0.22f + seaDisturbance * 0.08f, 0.025f, 0.38f);
            return Random.value < chance;
        }

        private void Update()
        {
            bool pausedByUi = Time.timeScale <= 0.0001f ||
                              (dialogueRunner != null && dialogueRunner.IsDialogueRunning);
            float speed = vesselBody == null ? 0f : HorizontalSpeed(vesselBody.linearVelocity);
            IsAdvancing = ActionDrivenClockPolicy.ShouldAdvance(speed, movementThreshold, harvestingActive, pausedByUi);

            if (!IsAdvancing)
            {
                UpdateAtmosphere(0f);
                return;
            }

            float gameDelta = Time.deltaTime * gameSecondsPerRealSecond;
            elapsedGameSeconds += gameDelta;
            timeOfDaySeconds = (timeOfDaySeconds + gameDelta) % 86400.0;
            UpdateWeather(gameDelta);
            UpdatePanic(gameDelta);
            seaDisturbance = Mathf.MoveTowards(seaDisturbance, 0f, gameDelta / 60f * 0.0008f);
            UpdateAtmosphere(gameDelta);
            UpdateHallucination(gameDelta);
        }

        private void UpdateWeather(float gameDelta)
        {
            if (fogRemainingGameSeconds > 0f)
            {
                fogRemainingGameSeconds -= gameDelta;
                if (fogRemainingGameSeconds <= 0f)
                {
                    fogRemainingGameSeconds = 0f;
                    weatherFogPressure = 0f;
                }
                return;
            }

            weatherShiftCooldown -= gameDelta;
            if (weatherShiftCooldown > 0f) return;

            weatherShiftCooldown = Random.Range(1800f, 3600f);
            if (Random.value < 0.34f)
            {
                weatherFogPressure = Random.Range(0.55f, 0.95f);
                fogRemainingGameSeconds = Random.Range(720f, 1200f);
            }
            else
            {
                weatherFogPressure = 0f;
            }
        }

        private void UpdatePanic(float gameDelta)
        {
            float minutes = gameDelta / 60f;
            bool foggy = weatherFogPressure > 0.1f;

            if (IsNight)
                panicLevel += nightPanicPerGameMinute * minutes;
            else
                panicLevel -= daylightRecoveryPerGameMinute * minutes;

            if (foggy)
                panicLevel += fogPanicPerGameMinute * weatherFogPressure * minutes;

            if (!IsNight && IsNearHarbour())
                panicLevel -= safeHarbourRecoveryPerGameMinute * minutes;

            panicLevel = Mathf.Clamp01(panicLevel);
        }

        private bool IsNearHarbour()
        {
            if (vesselBody == null) return false;
            Vector3 p = vesselBody.position;
            return p.x > -42f && p.x < 42f && p.z > -18f && p.z < 48f;
        }

        private void UpdateAtmosphere(float gameDelta)
        {
            float nightBlend = IsNight ? 1f : 0f;
            float panicFog = panicLevel * 0.0014f;
            RenderSettings.fog = true;
            RenderSettings.fogDensity = baseFogDensity + nightBlend * 0.0011f + panicFog + weatherFogPressure * 0.0024f;
            RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, Mathf.Clamp01(nightBlend + panicLevel * 0.35f + weatherFogPressure * 0.25f));

            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] == null || sceneLights[i].type != LightType.Directional) continue;
                float target = Mathf.Lerp(0.92f, 0.22f, nightBlend);
                target *= 1f - panicLevel * 0.16f;
                sceneLights[i].intensity = Mathf.MoveTowards(sceneLights[i].intensity, target, Time.unscaledDeltaTime * 0.2f);
            }

            float soundWarp = Mathf.Clamp01(nightBlend * 0.55f + panicLevel * 0.45f);
            for (int i = 0; i < soundscapeSources.Length; i++)
            {
                if (soundscapeSources[i] == null) continue;
                soundscapeSources[i].pitch = sourceBasePitch[i] * Mathf.Lerp(1f, 0.88f, soundWarp);
                soundscapeSources[i].volume = sourceBaseVolume[i] * Mathf.Lerp(1f, 0.72f, panicLevel * 0.5f);
            }
        }

        private void UpdateHallucination(float gameDelta)
        {
            // Once the omen has appeared, its countdown must continue independently of the next event cooldown.
            if (birdEventPending)
            {
                birdEventCountdown -= gameDelta;
                if (birdEventCountdown <= 0f)
                {
                    birdEventPending = false;
                    if (TryStealFish())
                    {
                        latestOmen = "The flock tears away one of the catches.";
                        SpawnBirdSwarm();
                    }
                    else
                    {
                        latestOmen = "The birds circle, but find nothing to take.";
                    }
                }
                return;
            }

            hallucinationCooldown -= gameDelta;
            if (panicLevel < 0.72f || hallucinationCooldown > 0f || vesselBody == null || !IsNight)
                return;

            hallucinationCooldown = Random.Range(55f, 105f);
            if (panicLevel > 0.9f && Random.value < 0.34f)
            {
                birdEventPending = true;
                birdEventCountdown = 105f;
                latestOmen = "Black wings gather above the fog. Keep the catch covered.";
                return;
            }

            bool leviathan = Random.value < 0.38f;
            latestOmen = leviathan
                ? "A vast shadow passes beneath the swell."
                : "A reef shape moves against the swell.";

            // Visual-only phantom obstruction. It deliberately has no collider and cannot damage the boat.
            Vector3 forward = vesselBody.transform.forward;
            Vector3 side = vesselBody.transform.right;
            Vector3 position = vesselBody.position + forward * Random.Range(22f, 36f) +
                               side * Random.Range(-13f, 13f);
            GameObject phantom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            phantom.name = leviathan ? "Panic_Hallucination_Leviathan_Shadow" : "Panic_Hallucination_Phantom_Reef";
            phantom.transform.position = position;
            phantom.transform.localScale = leviathan ? new Vector3(22f, 1.8f, 46f) : new Vector3(5f, 1.4f, 8f);
            Renderer renderer = phantom.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                Material phantomMaterial = new Material(shader);
                phantomMaterial.color = new Color(0.015f, 0.025f, 0.03f, 0.7f);
                if (phantomMaterial.HasProperty("_Surface")) phantomMaterial.SetFloat("_Surface", 1f);
                if (phantomMaterial.HasProperty("_SrcBlend")) phantomMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (phantomMaterial.HasProperty("_DstBlend")) phantomMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (phantomMaterial.HasProperty("_ZWrite")) phantomMaterial.SetFloat("_ZWrite", 0f);
                phantomMaterial.renderQueue = 3000;
                renderer.material = phantomMaterial;
                Destroy(phantomMaterial, 6f);
            }
            Collider collider = phantom.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Destroy(phantom, 6f);
        }

        private bool TryStealFish()
        {
            if (cargoHold == null) return false;
            IReadOnlyList<CargoPlacement> placements = cargoHold.Placements;
            List<CargoPlacement> exposedFish = new List<CargoPlacement>();
            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement placement = placements[i];
                CargoItemDefinition definition = placement == null ? null : cargoHold.GetItemDefinition(placement.itemId);
                if (definition != null && definition.Category == CargoItemCategory.Fish && !definition.IsQuestCritical)
                    exposedFish.Add(placement);
            }
            if (exposedFish.Count == 0) return false;
            CargoPlacement stolen = exposedFish[Random.Range(0, exposedFish.Count)];
            CargoPlacement removed;
            return cargoHold.TryRemove(stolen.instanceId, out removed);
        }

        private void SpawnBirdSwarm()
        {
            if (vesselBody == null) return;
            GameObject swarm = new GameObject("Panic_Hallucination_Bird_Swarm");
            swarm.transform.SetParent(vesselBody.transform, false);
            swarm.transform.localPosition = Vector3.up * 7f;
            swarm.AddComponent<PanicBirdSwarmVisual>();
            if (birdMaterial == null)
            {
                Shader birdShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (birdShader != null)
                {
                    birdMaterial = new Material(birdShader);
                    birdMaterial.color = new Color(0.015f, 0.02f, 0.025f);
                }
            }
            for (int i = 0; i < 7; i++)
            {
                GameObject bird = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bird.name = "Phantom_Seabird";
                bird.transform.SetParent(swarm.transform, false);
                bird.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-1f, 2f), Random.Range(-5f, 5f));
                bird.transform.localScale = new Vector3(0.45f, 0.18f, 0.8f);
                Collider birdCollider = bird.GetComponent<Collider>();
                if (birdCollider != null) Destroy(birdCollider);
                Renderer birdRenderer = bird.GetComponent<Renderer>();
                if (birdMaterial != null) birdRenderer.sharedMaterial = birdMaterial;
            }
            Destroy(swarm, 5f);
        }

        private void OnDestroy()
        {
            if (birdMaterial != null) Destroy(birdMaterial);
        }

        private void OnGUI()
        {
            float hour = CurrentHour;
            int displayHour = Mathf.FloorToInt(hour);
            int minute = Mathf.FloorToInt((hour - displayHour) * 60f);
            string period = displayHour >= 12 ? "PM" : "AM";
            int twelveHour = displayHour % 12;
            if (twelveHour == 0) twelveHour = 12;

            GUIStyle text = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.88f, 0.86f, 0.76f) }
            };
            GUI.Box(new Rect(18, 18, 190, 54), GUIContent.none);
            GUI.Label(new Rect(30, 23, 170, 24),
                "DAY " + (Mathf.FloorToInt((float)(elapsedGameSeconds / 86400.0)) + 1) +
                "   " + twelveHour.ToString("00") + ":" + minute.ToString("00") + " " + period, text);
            GUI.Label(new Rect(30, 47, 170, 20), IsAdvancing ? "SEA TIME: RUNNING" : "SEA TIME: HELD", text);

            GUI.Box(new Rect(Screen.width * 0.5f - 125, 18, 250, 44), GUIContent.none);
            GUIUtility.RotateAroundPivot(panicLevel * 720f, new Vector2(Screen.width * 0.5f - 94f, 34f));
            GUI.Label(new Rect(Screen.width * 0.5f - 110, 21, 28, 26), "◉", text);
            GUIUtility.RotateAroundPivot(-panicLevel * 720f, new Vector2(Screen.width * 0.5f - 94f, 34f));
            GUI.Label(new Rect(Screen.width * 0.5f - 78, 21, 192, 18),
                "PANIC  " + Mathf.RoundToInt(panicLevel * 100f) + "%", text);
            GUI.HorizontalSlider(new Rect(Screen.width * 0.5f - 110, 44, 220, 12), panicLevel, 0f, 1f);

            if (!string.IsNullOrEmpty(latestOmen) && panicLevel >= 0.72f)
            {
                GUIStyle omen = new GUIStyle(text) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                omen.normal.textColor = new Color(0.88f, 0.72f, 0.64f);
                GUI.Label(new Rect(Screen.width * 0.5f - 240, 72, 480, 36), latestOmen, omen);
            }
        }

        public void CaptureSaveData(SaveGameData data)
        {
            data.world.elapsedGameSeconds = elapsedGameSeconds;
            data.world.timeOfDaySeconds = timeOfDaySeconds;
            data.world.dayNumber = Mathf.FloorToInt((float)(elapsedGameSeconds / 86400.0)) + 1;
            data.world.panicLevel = panicLevel;
            data.world.seaDisturbance = seaDisturbance;
            data.world.weatherId = weatherFogPressure >= 0.65f ? "dense_fog" : weatherFogPressure > 0.1f ? "mist" : "clear";
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            elapsedGameSeconds = System.Math.Max(0.0, data.world.elapsedGameSeconds);
            timeOfDaySeconds = System.Math.Max(0.0, System.Math.Min(86399.0, data.world.timeOfDaySeconds));
            panicLevel = Mathf.Clamp01(data.world.panicLevel);
            seaDisturbance = Mathf.Clamp01(data.world.seaDisturbance);
            weatherFogPressure = data.world.weatherId == "dense_fog" ? 0.85f : data.world.weatherId == "mist" ? 0.4f : 0f;
            fogRemainingGameSeconds = weatherFogPressure > 0f ? 600f : 0f;
        }

        private static float HorizontalSpeed(Vector3 velocity)
        {
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }
}
using UnityEngine;
using Yarn.Unity;
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
        private bool harvestingActive;
        private float hallucinationCooldown;
        private string latestOmen = string.Empty;

        public double ElapsedGameSeconds => elapsedGameSeconds;
        public double TimeOfDaySeconds => timeOfDaySeconds;
        public float PanicLevel => panicLevel;
        public float SeaDisturbance => seaDisturbance;
        public bool IsNight => CurrentHour >= nightStartsAtHour || CurrentHour < dayStartsAtHour;
        public float CurrentHour => (float)(timeOfDaySeconds / 3600.0);
        public bool IsAdvancing { get; private set; }
        public string LatestOmen => latestOmen;

        private void Awake()
        {
            if (timeOfDaySeconds <= 0.0)
                timeOfDaySeconds = startingHour * 3600.0;
        }

        public void Configure(Rigidbody boat, DialogueRunner runner, HarvestingMinigameController harvest)
        {
            vesselBody = boat;
            dialogueRunner = runner;
            harvesting = harvest;
        }

        public void SetHarvesting(bool active)
        {
            harvestingActive = active;
        }

        public void AddDisturbance(float amount)
        {
            seaDisturbance = Mathf.Clamp01(seaDisturbance + amount);
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
            bool moving = speed >= movementThreshold;
            IsAdvancing = !pausedByUi && (moving || harvestingActive);

            if (!IsAdvancing)
            {
                UpdateAtmosphere(0f);
                return;
            }

            float gameDelta = Time.deltaTime * gameSecondsPerRealSecond;
            elapsedGameSeconds += gameDelta;
            timeOfDaySeconds = (timeOfDaySeconds + gameDelta) % 86400.0;
            UpdatePanic(gameDelta);
            seaDisturbance = Mathf.MoveTowards(seaDisturbance, 0f, gameDelta / 60f * 0.0008f);
            UpdateAtmosphere(gameDelta);
            UpdateHallucination(gameDelta);
        }

        private void UpdatePanic(float gameDelta)
        {
            float minutes = gameDelta / 60f;
            bool foggy = RenderSettings.fog && RenderSettings.fogDensity > baseFogDensity * 1.35f;

            if (IsNight)
                panicLevel += nightPanicPerGameMinute * minutes;
            else
                panicLevel -= daylightRecoveryPerGameMinute * minutes;

            if (foggy)
                panicLevel += fogPanicPerGameMinute * minutes;

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
            RenderSettings.fogDensity = baseFogDensity + nightBlend * 0.0011f + panicFog;
            RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, Mathf.Clamp01(nightBlend + panicLevel * 0.35f));

            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type != LightType.Directional) continue;
                float target = Mathf.Lerp(0.92f, 0.22f, nightBlend);
                target *= 1f - panicLevel * 0.16f;
                lights[i].intensity = Mathf.MoveTowards(lights[i].intensity, target, Time.unscaledDeltaTime * 0.2f);
            }
        }

        private void UpdateHallucination(float gameDelta)
        {
            hallucinationCooldown -= gameDelta;
            if (panicLevel < 0.72f || hallucinationCooldown > 0f || vesselBody == null || !IsNight)
                return;

            hallucinationCooldown = Random.Range(55f, 105f);
            latestOmen = Random.value < 0.5f
                ? "A reef shape moves against the swell."
                : "Something large turns beneath the boat.";

            // Visual-only phantom obstruction. It deliberately has no collider and cannot damage the boat.
            Vector3 forward = vesselBody.transform.forward;
            Vector3 side = vesselBody.transform.right;
            Vector3 position = vesselBody.position + forward * Random.Range(22f, 36f) +
                               side * Random.Range(-13f, 13f);
            GameObject phantom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            phantom.name = "Panic_Hallucination_Phantom_Reef";
            phantom.transform.position = position;
            phantom.transform.localScale = new Vector3(5f, 1.4f, 8f);
            Renderer renderer = phantom.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                             Shader.Find("Standard"));
            renderer.material.color = new Color(0.015f, 0.025f, 0.03f, 0.7f);
            renderer.material.SetFloat("_Surface", 1f);
            renderer.material.SetFloat("_AlphaClip", 0f);
            renderer.material.renderQueue = 3000;
            Collider collider = phantom.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Destroy(phantom, 6f);
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
            GUI.Label(new Rect(Screen.width * 0.5f - 112, 21, 226, 18),
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
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            elapsedGameSeconds = System.Math.Max(0.0, data.world.elapsedGameSeconds);
            timeOfDaySeconds = System.Math.Max(0.0, System.Math.Min(86399.0, data.world.timeOfDaySeconds));
            panicLevel = Mathf.Clamp01(data.world.panicLevel);
            seaDisturbance = Mathf.Clamp01(data.world.seaDisturbance);
        }

        private static float HorizontalSpeed(Vector3 velocity)
        {
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }
}
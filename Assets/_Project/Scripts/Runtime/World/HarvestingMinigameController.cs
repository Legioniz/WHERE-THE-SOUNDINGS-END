using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.World
{
    public enum HarvestActionType { None, Fishing, Dredging }

    /// <summary>
    /// First playable harvesting loop: timed fishing strikes and circular precision salvage beats.
    /// Successful catches are placed into the same spatial hold used by equipment and damaged cargo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HarvestingMinigameController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField, Min(2)] private int fishingBeats = 3;
        [SerializeField, Min(2)] private int dredgingBeats = 4;
        [SerializeField, Min(0.3f)] private float beatDuration = 1.15f;
        [SerializeField, Range(0.05f, 0.3f)] private float perfectWindow = 0.13f;

        private InputAction fishAction;
        private InputAction dredgeAction;
        private HarvestActionType action;
        private float beatTimer;
        private int beatIndex;
        private int successfulBeats;
        private float targetPhase;
        private float needlePhase;
        private string status = "F: fish   G: dredge";
        private Texture2D ringTexture;
        private Texture2D targetTexture;
        private SalvageSite activeSalvageSite;

        public bool IsActionActive => action != HarvestActionType.None;
        public HarvestActionType ActiveAction => action;

        public void Configure(InputActionAsset input, SpatialCargoHold hold, SeaStateDirector director)
        {
            controls = input;
            cargoHold = hold;
            seaState = director;
        }

        private void Awake()
        {
            if (cargoHold == null) cargoHold = GetComponent<SpatialCargoHold>();
            ringTexture = CreateRingTexture(128, new Color(0.78f, 0.83f, 0.77f, 0.9f));
            targetTexture = CreateArcTexture(128, 0.22f, new Color(0.86f, 0.37f, 0.22f, 0.95f));
        }

        private void Start()
        {
            if (controls == null) return;
            fishAction = controls.FindAction("Fish", false);
            dredgeAction = controls.FindAction("Dredge", false);
        }

        private void OnDestroy()
        {
            if (ringTexture != null) Destroy(ringTexture);
            if (targetTexture != null) Destroy(targetTexture);
        }

        private void Update()
        {
            if (action == HarvestActionType.None)
            {
                if (fishAction != null && fishAction.WasPressedThisFrame())
                    Begin(HarvestActionType.Fishing);
                else if (dredgeAction != null && dredgeAction.WasPressedThisFrame())
                    Begin(HarvestActionType.Dredging);
                return;
            }

            beatTimer += Time.deltaTime;
            needlePhase = Mathf.Repeat(beatTimer / beatDuration, 1f);
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                RegisterBeat();

            if (beatTimer >= beatDuration)
                MissBeat();
        }

        private void Begin(HarvestActionType type)
        {
            if (Time.timeScale <= 0.0001f || (seaState != null && seaState.PanicLevel > 0.98f))
                return;

            Rigidbody vessel = GetComponent<Rigidbody>();
            if (vessel != null && vessel.linearVelocity.magnitude > 1.5f)
            {
                status = "Slow the vessel before attempting a careful harvest.";
                return;
            }

            if (type == HarvestActionType.Dredging)
            {
                float distance;
                activeSalvageSite = SalvageSite.FindNearest(transform.position, out distance);
                if (activeSalvageSite == null)
                {
                    status = "No wreck or debris field close enough to dredge.";
                    return;
                }
                if (seaState != null && !activeSalvageSite.TryConsumePull(seaState.DayNumber))
                {
                    status = "This debris field has been picked clean for today.";
                    return;
                }
            }

            action = type;
            beatIndex = 0;
            successfulBeats = 0;
            beatTimer = 0f;
            targetPhase = Random.Range(0.18f, 0.82f);
            needlePhase = 0f;
            status = type == HarvestActionType.Fishing
                ? "FISHING: strike when the needle enters the marked arc."
                : "DREDGING: time each pull to the marked arc.";
            if (seaState != null) seaState.SetHarvesting(true);
        }

        private void RegisterBeat()
        {
            if (Mathf.Abs(Mathf.DeltaAngle(needlePhase * 360f, targetPhase * 360f)) <= perfectWindow * 360f)
                successfulBeats++;
            beatIndex++;
            AdvanceBeat();
        }

        private void MissBeat()
        {
            beatIndex++;
            AdvanceBeat();
        }

        private void AdvanceBeat()
        {
            int required = action == HarvestActionType.Fishing ? fishingBeats : dredgingBeats;
            if (beatIndex >= required)
            {
                CompleteAction();
                return;
            }

            beatTimer = 0f;
            targetPhase = Random.Range(0.12f, 0.88f);
        }

        private void CompleteAction()
        {
            HarvestActionType completed = action;
            int required = completed == HarvestActionType.Fishing ? fishingBeats : dredgingBeats;
            float quality = Mathf.Clamp01(0.35f + (successfulBeats / (float)required) * 0.65f);
            string result = completed == HarvestActionType.Fishing
                ? ResolveCatch(quality)
                : ResolveSalvage(quality);

            status = result;
            action = HarvestActionType.None;
            activeSalvageSite = null;
            if (seaState != null)
            {
                seaState.SetHarvesting(false);
                seaState.AddDisturbance(completed == HarvestActionType.Fishing ? 0.035f : 0.065f);
                seaState.AddPanic(completed == HarvestActionType.Fishing ? 0.004f : 0.012f);
            }
        }

        private string ResolveCatch(float quality)
        {
            List<CargoItemDefinition> candidates = new List<CargoItemDefinition>();
            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;
            bool aberrant = seaState != null && seaState.RollAberration();

            for (int i = 0; i < definitions.Count; i++)
            {
                CargoItemDefinition definition = definitions[i];
                if (definition == null) continue;
                bool isFish = definition.Category == CargoItemCategory.Fish;
                if (!isFish) continue;
                if (definition.IsAberrant == aberrant)
                    candidates.Add(definition);
            }

            if (candidates.Count == 0)
            {
                for (int i = 0; i < definitions.Count; i++)
                    if (definitions[i] != null && definitions[i].Category == CargoItemCategory.Fish &&
                        !definitions[i].IsAberrant) candidates.Add(definitions[i]);
            }

            if (candidates.Count == 0) return "No fish catalogue entries are available here.";

            CargoItemDefinition catchDefinition = candidates[Random.Range(0, candidates.Count)];
            string instanceId;
            if (!cargoHold.TryPlaceFirstFit(catchDefinition.ItemId, 1, quality, 1f, out instanceId))
                return "The hold is full or damaged. The catch slips away.";

            return (catchDefinition.IsAberrant ? "ABERRANT CATCH: " : "CAUGHT: ") +
                   catchDefinition.DisplayName + "  ·  quality " + Mathf.RoundToInt(quality * 100f) + "%";
        }

        private string ResolveSalvage(float quality)
        {
            List<CargoItemDefinition> candidates = new List<CargoItemDefinition>();
            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;
            for (int i = 0; i < definitions.Count; i++)
                if (definitions[i] != null && definitions[i].Category == CargoItemCategory.Salvage)
                    candidates.Add(definitions[i]);

            if (candidates.Count == 0) return "No salvage definitions are available.";
            CargoItemDefinition item = candidates[Random.Range(0, candidates.Count)];
            int quantity = quality >= 0.82f ? 2 : 1;
            string instanceId;
            if (!cargoHold.TryPlaceFirstFit(item.ItemId, quantity, quality, 1f, out instanceId))
                return "The hold is full or damaged. Salvage lost to the water.";

            return "SALVAGED: " + item.DisplayName + " ×" + quantity;
        }

        private void OnGUI()
        {
            if (action == HarvestActionType.None)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    normal = { textColor = new Color(0.86f, 0.86f, 0.78f) }
                };
                GUI.Label(new Rect(20, Screen.height - 42, 520, 24), status, style);
                return;
            }

            float size = 116f;
            Rect rect = new Rect(Screen.width * 0.5f - size * 0.5f,
                Screen.height * 0.72f - size * 0.5f, size, size);
            GUI.Box(new Rect(rect.x - 72, rect.y - 54, size + 144, size + 100), GUIContent.none);
            GUIStyle heading = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(rect.x - 72, rect.y - 47, size + 144, 22),
                action == HarvestActionType.Fishing ? "FISHING — STRIKE" : "DREDGING — PULL", heading);

            GUI.DrawTexture(rect, ringTexture);
            GUIUtility.RotateAroundPivot(-targetPhase * 360f, rect.center);
            GUI.DrawTexture(rect, targetTexture);
            GUIUtility.RotateAroundPivot(targetPhase * 360f, rect.center);
            GUIUtility.RotateAroundPivot(-needlePhase * 360f, rect.center);
            GUI.Box(new Rect(rect.center.x - 2, rect.y + 5, 4, size * 0.43f), GUIContent.none);
            GUIUtility.RotateAroundPivot(needlePhase * 360f, rect.center);

            GUI.Label(new Rect(rect.x - 72, rect.y + size + 7, size + 144, 22),
                "SPACE / ENTER   ·   " + (beatIndex + 1) + " / " +
                (action == HarvestActionType.Fishing ? fishingBeats : dredgingBeats), heading);
        }

        private static Texture2D CreateRingTexture(int size, Color colour)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.46f;
            float inner = size * 0.38f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= outer && distance >= inner ? colour : Color.clear);
                }
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateArcTexture(int size, float arc, Color colour)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.46f;
            float inner = size * 0.38f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 delta = new Vector2(x, y) - center;
                    float distance = delta.magnitude;
                    float angle = Mathf.Repeat(Mathf.Atan2(delta.y, delta.x) / (Mathf.PI * 2f), 1f);
                    bool inArc = angle <= arc;
                    texture.SetPixel(x, y, distance <= outer && distance >= inner && inArc ? colour : Color.clear);
                }
            texture.Apply();
            return texture;
        }
    }
}
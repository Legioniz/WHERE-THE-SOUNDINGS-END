using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.World
{
    /// <summary>
    /// First physical interaction layer for authored deep-ocean sites.
    /// Players can approach a marker and inspect its field note; physical hazard effects
    /// remain owned by future type-specific controllers.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DeepOceanSiteInteractionController : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField, Min(1f)] private float interactionDistance = 16f;

        private Rigidbody vesselBody;
        private InputAction interactAction;
        private DeepOceanPhenomenonSite[] sites = Array.Empty<DeepOceanPhenomenonSite>();
        private readonly HashSet<string> inspectedSiteIds = new HashSet<string>(StringComparer.Ordinal);
        private string latestInspection = "APPROACH A SURVEY SITE AND PRESS E TO INSPECT.";

        public IReadOnlyCollection<string> InspectedSiteIds => inspectedSiteIds;
        public string LatestInspection => latestInspection;

        public void Configure(InputActionAsset input, SeaStateDirector director)
        {
            controls = input;
            seaState = director;
        }

        private void Awake()
        {
            vesselBody = GetComponent<Rigidbody>();
            if (seaState == null) seaState = FindFirstObjectByType<SeaStateDirector>();
            sites = FindObjectsByType<DeepOceanPhenomenonSite>(FindObjectsSortMode.None);
        }

        private void Start()
        {
            if (controls != null) interactAction = controls.FindAction("Interact", false);
        }

        private void Update()
        {
            if (interactAction != null && interactAction.WasPressedThisFrame())
                TryInspectNearestSite();
        }

        private void TryInspectNearestSite()
        {
            if (Time.timeScale <= 0.0001f || (seaState != null && seaState.IsDialogueActive))
                return;

            DeepOceanPhenomenonSite nearest = null;
            float bestDistance = interactionDistance * interactionDistance;
            Vector2 vesselPosition = new Vector2(vesselBody.position.x, vesselBody.position.z);
            for (int index = 0; index < sites.Length; index++)
            {
                DeepOceanPhenomenonSite site = sites[index];
                if (site == null || !site.IsValidForRegion) continue;
                Vector2 sitePosition = new Vector2(site.transform.position.x, site.transform.position.z);
                float distance = (sitePosition - vesselPosition).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    nearest = site;
                    bestDistance = distance;
                }
            }

            if (nearest == null)
            {
                latestInspection = "NO SURVEY SITE WITHIN SAFE INSPECTION RANGE.";
                return;
            }

            bool firstInspection = inspectedSiteIds.Add(nearest.SiteId);
            latestInspection = (firstInspection ? "FIELD NOTE RECORDED: " : "KNOWN SITE: ") +
                nearest.DisplayName + " [" + nearest.EvidenceLabel + "]\n" + nearest.FirstSurveyNote;
            if (seaState != null) seaState.AddDisturbance(firstInspection ? 0.008f : 0.002f);
        }

        public void CaptureSaveData(SaveGameData data)
        {
            if (data == null || data.world == null)
                throw new ArgumentNullException(nameof(data), "Save data must contain world state.");
            data.world.inspectedPhenomenonSiteIds.Clear();
            data.world.inspectedPhenomenonSiteIds.AddRange(inspectedSiteIds);
        }

        public void RestoreSaveData(SaveGameData data)
        {
            if (data == null || data.world == null) return;
            inspectedSiteIds.Clear();
            if (data.world.inspectedPhenomenonSiteIds == null) return;
            for (int index = 0; index < data.world.inspectedPhenomenonSiteIds.Count; index++)
            {
                string id = data.world.inspectedPhenomenonSiteIds[index];
                if (!string.IsNullOrWhiteSpace(id)) inspectedSiteIds.Add(id);
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(latestInspection)) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, wordWrap = true,
                normal = { textColor = new Color(0.82f, 0.86f, 0.84f) }
            };
            GUI.Label(new Rect(20f, Screen.height - 210f, 520f, 54f), latestInspection, style);
        }
    }
}

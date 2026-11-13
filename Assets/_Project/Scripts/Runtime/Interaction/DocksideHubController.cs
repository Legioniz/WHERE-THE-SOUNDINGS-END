using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using WhereTheSoundingsEnd.Boat;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Narrative;
using WhereTheSoundingsEnd.NPC;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Interaction
{
    public enum DocksideFurnitureAction
    {
        ReadTideLog,
        ReadHarbourChart,
        ReadNoticeboard,
        InspectCatch,
        ReadCatchNotes,
        InspectVessel,
        ReadEngineNotes,
        ReadNetNotes,
        ReadKnotGuide,
        InspectNetSupplies,
        SaveHarbourLog,
        ReadRumours,
        ReadNightWatch,
        InspectBeacon,
        ReadLighthouseLog,
        ReadResidenceNotice
    }

    [Serializable]
    public sealed class DocksideFurnitureOption
    {
        [SerializeField] private string furnitureId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private DocksideFurnitureAction action;

        public string FurnitureId => furnitureId;
        public string DisplayName => displayName;
        public string Description => description;
        public DocksideFurnitureAction Action => action;

        public DocksideFurnitureOption(string id, string name, string detail, DocksideFurnitureAction furnitureAction)
        {
            furnitureId = id;
            displayName = name;
            description = detail;
            action = furnitureAction;
        }
    }

    [Serializable]
    public sealed class DocksideLocationDefinition
    {
        [SerializeField] private string locationId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string subtitle = string.Empty;
        [SerializeField, TextArea] private string interiorDescription = string.Empty;
        [SerializeField] private Vector3 accessPoint;
        [SerializeField, Min(1f)] private float accessRadius = 24f;
        [SerializeField] private string dialogueNode = string.Empty;
        [SerializeField] private NpcAgent resident;
        [SerializeField] private List<DocksideFurnitureOption> furniture =
            new List<DocksideFurnitureOption>();

        public string LocationId => locationId;
        public string DisplayName => displayName;
        public string Subtitle => subtitle;
        public string InteriorDescription => interiorDescription;
        public Vector3 AccessPoint => accessPoint;
        public float AccessRadius => accessRadius;
        public string DialogueNode => dialogueNode;
        public NpcAgent Resident => resident;
        public IReadOnlyList<DocksideFurnitureOption> Furniture => furniture;

        public DocksideLocationDefinition(
            string id,
            string name,
            string locationSubtitle,
            string description,
            Vector3 point,
            float radius,
            string node,
            NpcAgent npc,
            IList<DocksideFurnitureOption> options)
        {
            locationId = id;
            displayName = name;
            subtitle = locationSubtitle;
            interiorDescription = description;
            accessPoint = point;
            accessRadius = radius;
            dialogueNode = node;
            resident = npc;
            furniture = options == null
                ? new List<DocksideFurnitureOption>()
                : new List<DocksideFurnitureOption>(options);
        }
    }

    public static class DocksideAccessPolicy
    {
        public static bool IsAccessible(
            Vector3 vesselPosition,
            Vector3 accessPoint,
            float horizontalSpeed,
            float accessRadius,
            float maximumDockingSpeed)
        {
            if (!IsFinite(vesselPosition.x) || !IsFinite(vesselPosition.y) ||
                !IsFinite(vesselPosition.z) || !IsFinite(accessPoint.x) ||
                !IsFinite(accessPoint.y) || !IsFinite(accessPoint.z) ||
                !IsFinite(horizontalSpeed) || !IsFinite(accessRadius) ||
                !IsFinite(maximumDockingSpeed) || accessRadius <= 0f ||
                maximumDockingSpeed < 0f || horizontalSpeed < 0f)
                return false;

            float dx = vesselPosition.x - accessPoint.x;
            float dz = vesselPosition.z - accessPoint.z;
            return dx * dx + dz * dz <= accessRadius * accessRadius &&
                   horizontalSpeed <= maximumDockingSpeed;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    public static class DocksideModalPolicy
    {
        public static bool CanOpenDocksideMenu(
            bool inventoryOpen,
            bool dialogueOpen,
            bool gameplayActionBusy,
            bool frontEndOverlayOpen,
            float timeScale)
        {
            return !inventoryOpen && !dialogueOpen && !gameplayActionBusy &&
                   !frontEndOverlayOpen && IsFinite(timeScale) && timeScale > 0.0001f;
        }

        public static bool CanOpenInventory(
            bool docksideMenuOpen,
            bool frontEndOverlayOpen,
            bool dialogueOpen)
        {
            return !docksideMenuOpen && !frontEndOverlayOpen && !dialogueOpen;
        }

        public static bool CanOpenPauseMenu(
            bool docksideMenuOpen,
            bool inventoryOpen,
            bool dialogueOpen)
        {
            return !docksideMenuOpen && !inventoryOpen && !dialogueOpen;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Boat-only location hub. It presents authored interior/furniture menus while keeping
    /// streamed buildings static and all gameplay ownership in persistent runtime systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DocksideHubController : MonoBehaviour
    {
        private const float MaximumDockingSpeed = 0.85f;

        [Header("Presentation")]
        [SerializeField] private UIDocument document;
        [SerializeField] private GreyhavenRuntimeUI dialogueUI;
        [SerializeField] private List<DocksideLocationDefinition> locations =
            new List<DocksideLocationDefinition>();

        [Header("Vessel and gameplay")]
        [SerializeField] private Rigidbody vesselBody;
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private VesselConditionController vesselCondition;
        [SerializeField] private SeaStateDirector seaState;
        [SerializeField] private SaveGameCoordinator saveCoordinator;

        private VisualElement overlay;
        private VisualElement locationList;
        private VisualElement furnitureList;
        private Label locationTitle;
        private Label locationSubtitle;
        private Label interiorDescription;
        private Label furnitureTitle;
        private Label furnitureDetail;
        private Label statusLabel;
        private Button residentButton;
        private InputAction interactAction;
        private InputAction pauseAction;
        private bool ownsInteractAction;
        private bool menuOpen;
        private bool pauseOwned;
        private bool promptOwned;
        private float previousTimeScale = 1f;
        private DocksideLocationDefinition selectedLocation;

        public bool IsMenuOpen => menuOpen;
        public string ActiveLocationId => selectedLocation != null ? selectedLocation.LocationId : string.Empty;

        public void Configure(
            UIDocument uiDocument,
            GreyhavenRuntimeUI dialoguePresenter,
            Rigidbody boatBody,
            InputActionAsset input,
            SpatialCargoHold hold,
            VesselConditionController condition,
            SeaStateDirector clock,
            SaveGameCoordinator saves,
            IList<DocksideLocationDefinition> definitions)
        {
            document = uiDocument;
            dialogueUI = dialoguePresenter;
            vesselBody = boatBody;
            controls = input;
            cargoHold = hold;
            vesselCondition = condition;
            seaState = clock;
            saveCoordinator = saves;
            locations = definitions == null
                ? new List<DocksideLocationDefinition>()
                : new List<DocksideLocationDefinition>(definitions);
            if (dialogueUI != null)
                dialogueUI.DocksideHub = this;
        }

        private void Start()
        {
            if (document == null || document.rootVisualElement == null)
            {
                Debug.LogError("DocksideHubController requires a UIDocument with PanelSettings.", this);
                enabled = false;
                return;
            }

            if (vesselBody == null || dialogueUI == null || controls == null)
            {
                Debug.LogError("DocksideHubController is missing its vessel, input, or dialogue UI reference.", this);
                enabled = false;
                return;
            }

            if (saveCoordinator == null)
                saveCoordinator = FindFirstObjectByType<SaveGameCoordinator>();
            if (seaState == null)
                seaState = FindFirstObjectByType<SeaStateDirector>();

            InputActionMap map = controls.FindActionMap("Boat", false);
            interactAction = map != null ? map.FindAction("Interact", false) : null;
            pauseAction = map != null ? map.FindAction("Pause", false) : null;
            if (interactAction != null && !interactAction.enabled)
            {
                interactAction.Enable();
                ownsInteractAction = true;
            }

            BuildInterface();
        }

        private void OnDestroy()
        {
            if (ownsInteractAction && interactAction != null)
                interactAction.Disable();
            if (dialogueUI != null && dialogueUI.DocksideHub == this)
                dialogueUI.DocksideHub = null;
            RestoreTimeScale();
        }

        private void OnDisable()
        {
            if (menuOpen)
                CloseMenu();
            else
            {
                RestoreTimeScale();
                ClearOwnedPrompt();
            }
        }

        private void Update()
        {
            if (vesselBody == null) return;

            if (menuOpen)
            {
                Keyboard keyboard = Keyboard.current;
                bool keyboardClose = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
                bool gamepadClose = pauseAction != null && pauseAction.WasPressedThisFrame();
                if (keyboardClose || gamepadClose)
                    CloseMenu();
                return;
            }

            if (dialogueUI == null || !DocksideModalPolicy.CanOpenDocksideMenu(
                dialogueUI.IsInventoryOpen,
                dialogueUI.IsDialogueOpen,
                seaState != null && seaState.IsGameplayActionBusy,
                dialogueUI.FrontEnd != null && dialogueUI.FrontEnd.IsOverlayOpen,
                Time.timeScale))
            {
                ClearOwnedPrompt();
                return;
            }

            DocksideLocationDefinition nearest = FindAccessibleLocation();
            if (nearest == null)
            {
                ClearOwnedPrompt();
                return;
            }

            dialogueUI.SetInteractionPrompt("E  /  OPEN " + nearest.DisplayName.ToUpperInvariant());
            promptOwned = true;
            if (interactAction != null && interactAction.WasPressedThisFrame())
                OpenMenu(nearest);
        }

        private DocksideLocationDefinition FindAccessibleLocation()
        {
            Vector3 velocity = vesselBody.linearVelocity;
            float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            DocksideLocationDefinition nearest = null;
            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < locations.Count; i++)
            {
                DocksideLocationDefinition location = locations[i];
                if (location == null || string.IsNullOrWhiteSpace(location.LocationId))
                    continue;

                if (!DocksideAccessPolicy.IsAccessible(
                    vesselBody.position, location.AccessPoint, horizontalSpeed,
                    location.AccessRadius, MaximumDockingSpeed))
                    continue;

                float dx = vesselBody.position.x - location.AccessPoint.x;
                float dz = vesselBody.position.z - location.AccessPoint.z;
                float distance = dx * dx + dz * dz;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = location;
                }
            }

            return nearest;
        }

        private void BuildInterface()
        {
            VisualElement root = document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1f;
            root.pickingMode = PickingMode.Ignore;

            overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.right = 0;
            overlay.style.top = 0;
            overlay.style.bottom = 0;
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.backgroundColor = new Color(0.012f, 0.025f, 0.029f, 0.96f);
            overlay.style.display = DisplayStyle.None;
            overlay.pickingMode = PickingMode.Position;
            root.Add(overlay);

            VisualElement frame = new VisualElement();
            frame.style.width = new Length(92f, LengthUnit.Percent);
            frame.style.maxWidth = 1180;
            frame.style.height = new Length(88f, LengthUnit.Percent);
            frame.style.minHeight = 440;
            frame.style.flexDirection = FlexDirection.Column;
            frame.style.backgroundColor = new Color(0.045f, 0.075f, 0.078f, 0.99f);
            frame.style.borderTopWidth = 2;
            frame.style.borderTopColor = new Color(0.66f, 0.34f, 0.22f);
            frame.style.borderBottomWidth = 1;
            frame.style.borderBottomColor = new Color(0.30f, 0.39f, 0.37f);
            frame.style.paddingLeft = 22;
            frame.style.paddingRight = 22;
            frame.style.paddingTop = 16;
            frame.style.paddingBottom = 18;
            overlay.Add(frame);

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 14;
            frame.Add(header);

            Label brand = new Label("GREYHAVEN  /  DOCKSIDE SERVICES");
            brand.style.color = new Color(0.82f, 0.78f, 0.66f);
            brand.style.fontSize = 13;
            brand.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(brand);

            Button close = MakeButton("CLOSE  [ESC]", CloseMenu);
            close.style.minWidth = 120;
            header.Add(close);

            VisualElement body = new VisualElement();
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1f;
            body.style.minHeight = 0;
            frame.Add(body);

            locationList = new ScrollView(ScrollViewMode.Vertical);
            locationList.style.width = new Length(27f, LengthUnit.Percent);
            locationList.style.minWidth = 190;
            locationList.style.paddingRight = 12;
            locationList.style.borderRightWidth = 1;
            locationList.style.borderRightColor = new Color(0.24f, 0.32f, 0.31f);
            body.Add(locationList);

            VisualElement interior = new VisualElement();
            interior.style.flexGrow = 1f;
            interior.style.minWidth = 0;
            interior.style.paddingLeft = 20;
            interior.style.paddingRight = 4;
            interior.style.flexDirection = FlexDirection.Column;
            body.Add(interior);

            locationTitle = new Label("SELECT A LOCATION");
            locationTitle.style.color = new Color(0.91f, 0.88f, 0.77f);
            locationTitle.style.fontSize = 24;
            locationTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            interior.Add(locationTitle);

            locationSubtitle = new Label(string.Empty);
            locationSubtitle.style.color = new Color(0.73f, 0.59f, 0.43f);
            locationSubtitle.style.fontSize = 11;
            locationSubtitle.style.marginTop = 4;
            interior.Add(locationSubtitle);

            interiorDescription = new Label(string.Empty);
            interiorDescription.style.color = new Color(0.77f, 0.81f, 0.76f);
            interiorDescription.style.whiteSpace = WhiteSpace.Normal;
            interiorDescription.style.marginTop = 12;
            interiorDescription.style.marginBottom = 16;
            interior.Add(interiorDescription);

            Label furnitureHeading = new Label("ROOM DETAILS  /  INTERACTABLE FURNITURE");
            furnitureHeading.style.color = new Color(0.82f, 0.78f, 0.66f);
            furnitureHeading.style.fontSize = 11;
            furnitureHeading.style.unityFontStyleAndWeight = FontStyle.Bold;
            furnitureHeading.style.marginBottom = 8;
            interior.Add(furnitureHeading);

            furnitureList = new ScrollView(ScrollViewMode.Vertical);
            furnitureList.style.flexGrow = 1f;
            furnitureList.style.minHeight = 110;
            interior.Add(furnitureList);

            VisualElement detail = new VisualElement();
            detail.style.marginTop = 12;
            detail.style.paddingLeft = 14;
            detail.style.paddingRight = 14;
            detail.style.paddingTop = 10;
            detail.style.paddingBottom = 10;
            detail.style.backgroundColor = new Color(0.025f, 0.047f, 0.049f, 0.98f);
            detail.style.borderLeftWidth = 2;
            detail.style.borderLeftColor = new Color(0.62f, 0.29f, 0.20f);
            interior.Add(detail);

            furnitureTitle = new Label("SELECT AN OBJECT");
            furnitureTitle.style.color = new Color(0.91f, 0.84f, 0.69f);
            furnitureTitle.style.fontSize = 14;
            furnitureTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            detail.Add(furnitureTitle);

            furnitureDetail = new Label("Choose a piece of furniture to inspect or use.");
            furnitureDetail.style.color = new Color(0.78f, 0.82f, 0.78f);
            furnitureDetail.style.whiteSpace = WhiteSpace.Normal;
            furnitureDetail.style.marginTop = 6;
            detail.Add(furnitureDetail);

            statusLabel = new Label(string.Empty);
            statusLabel.style.color = new Color(0.73f, 0.68f, 0.54f);
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            statusLabel.style.marginTop = 8;
            detail.Add(statusLabel);

            residentButton = MakeButton("SPEAK WITH RESIDENT", StartResidentDialogue);
            residentButton.style.marginTop = 12;
            residentButton.style.minHeight = 38;
            interior.Add(residentButton);

            RenderLocationList();
        }

        private void RenderLocationList()
        {
            locationList.Clear();
            Label heading = new Label("NEARBY LOCATIONS");
            heading.style.color = new Color(0.73f, 0.77f, 0.70f);
            heading.style.fontSize = 11;
            heading.style.marginBottom = 8;
            locationList.Add(heading);

            for (int i = 0; i < locations.Count; i++)
            {
                DocksideLocationDefinition location = locations[i];
                if (location == null) continue;
                DocksideLocationDefinition captured = location;
                Button button = MakeButton(location.DisplayName.ToUpperInvariant(),
                    () => SelectLocation(captured));
                button.style.marginBottom = 6;
                button.style.minHeight = 42;
                locationList.Add(button);
            }

            Label note = new Label("ACCESS IS AVAILABLE WHILE THE VESSEL IS STATIONARY WITHIN THE LOCATION'S SHORE APPROACH.");
            note.style.color = new Color(0.55f, 0.63f, 0.59f);
            note.style.fontSize = 10;
            note.style.whiteSpace = WhiteSpace.Normal;
            note.style.marginTop = 14;
            locationList.Add(note);
        }

        private void OpenMenu(DocksideLocationDefinition location)
        {
            if (location == null || menuOpen || dialogueUI == null ||
                !DocksideModalPolicy.CanOpenDocksideMenu(
                    dialogueUI.IsInventoryOpen,
                    dialogueUI.IsDialogueOpen,
                    seaState != null && seaState.IsGameplayActionBusy,
                    dialogueUI.FrontEnd != null && dialogueUI.FrontEnd.IsOverlayOpen,
                    Time.timeScale)) return;
            if (seaState != null && !seaState.TryClaimInteraction()) return;

            previousTimeScale = Time.timeScale;
            pauseOwned = true;
            Time.timeScale = 0f;
            menuOpen = true;
            selectedLocation = location;
            overlay.style.display = DisplayStyle.Flex;
            ClearOwnedPrompt();
            SelectLocation(location);
        }

        private void CloseMenu()
        {
            if (!menuOpen) return;
            overlay.style.display = DisplayStyle.None;
            menuOpen = false;
            selectedLocation = null;
            RestoreTimeScale();
            ClearOwnedPrompt();
        }

        private void RestoreTimeScale()
        {
            if (!pauseOwned) return;
            Time.timeScale = previousTimeScale;
            pauseOwned = false;
        }

        private void ClearOwnedPrompt()
        {
            if (!promptOwned) return;
            dialogueUI?.SetInteractionPrompt(string.Empty);
            promptOwned = false;
        }

        private void SelectLocation(DocksideLocationDefinition location)
        {
            selectedLocation = location;
            locationTitle.text = location.DisplayName.ToUpperInvariant();
            locationSubtitle.text = location.Subtitle.ToUpperInvariant();
            interiorDescription.text = location.InteriorDescription;
            furnitureTitle.text = "SELECT AN OBJECT";
            furnitureDetail.text = "Choose a piece of furniture to inspect or use.";
            statusLabel.text = string.Empty;
            furnitureList.Clear();

            for (int i = 0; i < location.Furniture.Count; i++)
            {
                DocksideFurnitureOption option = location.Furniture[i];
                if (option == null) continue;
                DocksideFurnitureOption captured = option;
                Button button = MakeButton(option.DisplayName.ToUpperInvariant(),
                    () => SelectFurniture(captured));
                button.style.marginBottom = 6;
                button.style.minHeight = 38;
                furnitureList.Add(button);
            }

            bool hasResident = location.Resident != null &&
                               !string.IsNullOrWhiteSpace(location.DialogueNode);
            residentButton.style.display = hasResident ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasResident)
            {
                residentButton.text = location.Resident.CanTalk
                    ? "SPEAK WITH " + location.Resident.DisplayName.ToUpperInvariant()
                    : location.Resident.DisplayName.ToUpperInvariant() + "  /  REFUSES TO SPEAK";
                residentButton.SetEnabled(location.Resident.CanTalk);
            }
        }

        private void SelectFurniture(DocksideFurnitureOption option)
        {
            if (option == null) return;
            furnitureTitle.text = option.DisplayName.ToUpperInvariant();
            furnitureDetail.text = option.Description;
            statusLabel.text = ResolveFurnitureAction(option.Action);
        }

        private string ResolveFurnitureAction(DocksideFurnitureAction action)
        {
            switch (action)
            {
                case DocksideFurnitureAction.ReadTideLog:
                    if (seaState == null) return "The tide log cannot be read because the sea-state record is unavailable.";
                    return "DAY " + seaState.DayNumber.ToString("00") +
                           "  /  " + FormatHour(seaState.CurrentHour) +
                           "  /  SEA DISTURBANCE " + Mathf.RoundToInt(seaState.SeaDisturbance * 100f) +
                           "%  /  PANIC " + Mathf.RoundToInt(seaState.PanicLevel * 100f) +
                           "%\n" + (string.IsNullOrWhiteSpace(seaState.LatestOmen)
                               ? "No unusual report has been entered."
                               : seaState.LatestOmen);

                case DocksideFurnitureAction.ReadHarbourChart:
                    return "LOCAL BEARINGS\nD — Main dock\nH — Harbour Office / Harbourmaster\nM — Fish Market / Mara Venn\nT — Boat Repair / Tomas Vale\nI — Net Mender / Inez Sarmiento\nL — Western lighthouse\nW — Outer Channel Wreck Field\n\nThis is a schematic settlement reference, not a calibrated navigation chart.";

                case DocksideFurnitureAction.ReadNoticeboard:
                    return "HARBOUR NOTICE\nKeep the inner quay clear during the morning catch. Report damaged channel markers to the Harbourmaster. Unusual sounds offshore should be logged with their time and bearing.";

                case DocksideFurnitureAction.InspectCatch:
                    return BuildCatchInspection();

                case DocksideFurnitureAction.ReadCatchNotes:
                    return "MARA'S GRADING NOTES\nKeep delicate fish away from heavy salvage. Record freshness before unloading. Strange specimens should be kept separate until their condition is understood.\n\nThis board provides inspection guidance only. A market sale and currency transaction are not implemented yet.";

                case DocksideFurnitureAction.InspectVessel:
                    return BuildVesselInspection();

                case DocksideFurnitureAction.ReadEngineNotes:
                    return "TOMAS'S ENGINE NOTES\nA heavy throttle under poor engine condition is a warning, not a repair. Listen for changes in load and vibration. Check the propeller after a hard impact.\n\nThe workbench currently diagnoses condition; paid repair and upgrade transactions are not implemented yet.";

                case DocksideFurnitureAction.ReadNetNotes:
                    return "INEZ'S WORK NOTES\nA clean, even mesh is easier to mend than a stretched one. Check the lead line and every join before the next trip. Never trust a knot you cannot identify by touch.";

                case DocksideFurnitureAction.ReadKnotGuide:
                    return "KNOT BOARD\n1. Check the standing part before loading a line.\n2. Keep bends clear of sharp metal edges.\n3. Replace frayed sections instead of hiding them in a splice.\n\nNet crafting and repair are not yet connected to a player equipment system.";

                case DocksideFurnitureAction.InspectNetSupplies:
                    return "THREAD AND ROPE STOCK\nThe visible spools are Inez's work supplies. A persistent net-material inventory has not been implemented, so this inspection does not consume or grant items.";

                case DocksideFurnitureAction.SaveHarbourLog:
                    return SaveHarbourLog();

                case DocksideFurnitureAction.ReadRumours:
                    return "TABLE TALK\nThe outer marker bell rang once on a night with no wind. Tomas says the old propeller shaft still turns in the shed when the tide is wrong. Mara refuses to buy anything brought up warm from deep water.";

                case DocksideFurnitureAction.ReadNightWatch:
                    return seaState != null && seaState.IsNight
                        ? "The harbour lanterns are lit. The water beyond the breakwater is difficult to read tonight."
                        : "The lantern is maintained for the evening watch. It is not yet a player-controlled light source.";

                case DocksideFurnitureAction.InspectBeacon:
                    return "BEACON STATUS\nThe lighthouse is a fixed navigation landmark. Its beam and keeper routines are currently presentation-only; there is no player-operated beacon control.";

                case DocksideFurnitureAction.ReadLighthouseLog:
                    return "LIGHTHOUSE LOG\nThe western headland catches the first hard weather. Keep the outer channel marker in sight when the fog settles. The keeper's full routine and story are not yet implemented.";

                case DocksideFurnitureAction.ReadResidenceNotice:
                    return "PRIVATE RESIDENCE\nThis house is part of the settlement blockout. It has no assigned resident or visitable interior yet.";

                default:
                    return "No interaction is configured for this object.";
            }
        }

        private string BuildCatchInspection()
        {
            if (cargoHold == null || cargoHold.Placements == null)
                return "The cargo hold is unavailable.";

            if (cargoHold.Placements.Count == 0)
                return "The hold is empty. No catch or salvage is ready for inspection.";

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("CARGO APPRAISAL  /  ");
            builder.Append(cargoHold.Placements.Count);
            builder.Append(" PLACEMENTS  /  ");
            builder.Append(cargoHold.GetUsedCellCount());
            builder.Append(" CELLS USED\n");

            for (int i = 0; i < cargoHold.Placements.Count; i++)
            {
                CargoPlacement placement = cargoHold.Placements[i];
                CargoItemDefinition definition = cargoHold.GetItemDefinition(placement.itemId);
                string itemName = definition != null ? definition.DisplayName : placement.itemId;
                builder.Append("\n");
                builder.Append(itemName);
                builder.Append("  /  x");
                builder.Append(Mathf.Max(1, placement.quantity));
                builder.Append("  /  FRESHNESS ");
                builder.Append(Mathf.RoundToInt(Mathf.Clamp01(placement.freshness) * 100f));
                builder.Append("%");
                if (definition != null && definition.IsAberrant)
                    builder.Append("  /  ABERRANT");
                if (definition != null && definition.IsQuestCritical)
                    builder.Append("  /  QUEST ITEM");
            }

            builder.Append("\n\nInspection only. No goods are removed and no sale is performed.");
            return builder.ToString();
        }

        private string BuildVesselInspection()
        {
            if (vesselCondition == null)
                return "Vessel condition telemetry is unavailable.";

            return "HULL INTEGRITY  /  " +
                   Mathf.RoundToInt(Mathf.Clamp01(vesselCondition.HullIntegrity) * 100f) +
                   "%\nENGINE CONDITION  /  " +
                   Mathf.RoundToInt(Mathf.Clamp01(vesselCondition.EngineCondition) * 100f) +
                   "%\nLAST DAMAGE REPORT  /  " +
                   (string.IsNullOrWhiteSpace(vesselCondition.LastDamageReport)
                       ? "No recent damage report."
                       : vesselCondition.LastDamageReport) +
                   "\n\nDiagnosis only. No repair materials or currency are consumed.";
        }

        private string SaveHarbourLog()
        {
            if (saveCoordinator == null)
                return "The save coordinator is unavailable. No checkpoint was written.";

            AutosaveController autosave = saveCoordinator.GetComponent<AutosaveController>();
            if (autosave == null)
                return "The autosave service is unavailable. No checkpoint was written.";

            SaveOperationResult result = autosave.RequestCheckpointAutosave("greyhaven.inn_guest_ledger");
            return result.Success
                ? "HARBOUR LOG SAVED\n" + result.Message
                : "SAVE FAILED\n" + result.Message;
        }

        private void StartResidentDialogue()
        {
            DocksideLocationDefinition location = selectedLocation;
            if (location == null || location.Resident == null ||
                string.IsNullOrWhiteSpace(location.DialogueNode) || !location.Resident.CanTalk)
                return;

            NpcAgent resident = location.Resident;
            string node = location.DialogueNode;
            CloseMenu();
            dialogueUI.BeginDialogue(node, resident);
        }

        private static string FormatHour(float hour)
        {
            int totalMinutes = Mathf.RoundToInt(Mathf.Repeat(hour, 24f) * 60f);
            int hours = (totalMinutes / 60) % 24;
            int minutes = totalMinutes % 60;
            return hours.ToString("00") + ":" + minutes.ToString("00");
        }

        private static Button MakeButton(string title, Action onClick)
        {
            Button button = new Button(onClick);
            button.text = title;
            button.style.color = new Color(0.87f, 0.88f, 0.81f);
            button.style.backgroundColor = new Color(0.085f, 0.12f, 0.12f, 0.98f);
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = new Color(0.24f, 0.32f, 0.30f);
            button.style.borderBottomColor = new Color(0.24f, 0.32f, 0.30f);
            button.style.borderLeftColor = new Color(0.24f, 0.32f, 0.30f);
            button.style.borderRightColor = new Color(0.24f, 0.32f, 0.30f);
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.paddingLeft = 12;
            button.style.paddingRight = 10;
            return button;
        }
    }
}

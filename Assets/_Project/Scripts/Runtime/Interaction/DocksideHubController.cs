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
using WhereTheSoundingsEnd.Economy;

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
        private VisualElement vendorActions;
        private MaraFishMarketController maraMarket;
        private GreyhavenVendorServices vendorServices;
        private HarbourmasterContractController harbourmasterContracts;
        private HarbourmasterResearchController harbourmasterResearch;
        private VesselFuelController fuelController;
        private VesselEquipmentController equipmentController;
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
            maraMarket = boatBody != null ? boatBody.GetComponent<MaraFishMarketController>() : null;
            vendorServices = boatBody != null ? boatBody.GetComponent<GreyhavenVendorServices>() : null;
            harbourmasterContracts = boatBody != null ? boatBody.GetComponent<HarbourmasterContractController>() : null;
            if (boatBody != null && harbourmasterContracts == null)
                harbourmasterContracts = boatBody.gameObject.AddComponent<HarbourmasterContractController>();
            harbourmasterResearch = boatBody != null ? boatBody.GetComponent<HarbourmasterResearchController>() : null;
            if (boatBody != null && harbourmasterResearch == null)
                harbourmasterResearch = boatBody.gameObject.AddComponent<HarbourmasterResearchController>();
            fuelController = boatBody != null ? boatBody.GetComponent<VesselFuelController>() : null;
            equipmentController = boatBody != null ? boatBody.GetComponent<VesselEquipmentController>() : null;
            if (boatBody != null && equipmentController == null)
                equipmentController = boatBody.gameObject.AddComponent<VesselEquipmentController>();
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

            Label servicesHeading = new Label("RESIDENT SERVICES");
            servicesHeading.style.color = new Color(0.82f, 0.78f, 0.66f);
            servicesHeading.style.fontSize = 11;
            servicesHeading.style.unityFontStyleAndWeight = FontStyle.Bold;
            servicesHeading.style.marginTop = 10;
            servicesHeading.style.marginBottom = 5;
            interior.Add(servicesHeading);

            vendorActions = new ScrollView(ScrollViewMode.Vertical);
            vendorActions.style.maxHeight = 170;
            vendorActions.style.minHeight = 42;
            vendorActions.style.flexShrink = 0f;
            interior.Add(vendorActions);

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

            RenderVendorServices(location);

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

        private void RenderVendorServices(DocksideLocationDefinition location)
        {
            if (vendorActions == null) return;
            vendorActions.Clear();
            if (location == null || location.Resident == null) return;

            string residentId = location.Resident.NpcId;
            string residentName = location.Resident.DisplayName ?? string.Empty;
            if (residentName.IndexOf("Harbourmaster", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                RenderHarbourmasterContracts(location.Resident);
                return;
            }
            if (residentId == MaraFishMarketController.VendorId)
            {
                RenderMaraServices();
                return;
            }
            if (residentId == GreyhavenVendorServices.TomasId)
            {
                RenderTomasServices();
                return;
            }
            if (residentId == GreyhavenVendorServices.InezId)
                RenderInezServices();
        }

        private void RenderHarbourmasterContracts(NpcAgent harbourmaster)
        {
            if (vendorActions == null) return;
            vendorActions.Clear();
            AddWalletBalanceLabel();
            if (harbourmasterContracts == null)
            {
                vendorActions.Add(new Label("The Harbourmaster's contract ledger is unavailable."));
                return;
            }

            Label board = new Label(harbourmasterContracts.GetBoardText(harbourmaster));
            board.style.whiteSpace = WhiteSpace.Normal;
            board.style.color = new Color(0.82f, 0.78f, 0.66f);
            board.style.marginBottom = 8;
            vendorActions.Add(board);

            if (!harbourmasterContracts.IsAvailableTo(harbourmaster)) return;

            if (harbourmasterContracts.HasActiveContract)
            {
                Button deliver = MakeButton("SUBMIT REQUIRED SALVAGE", () =>
                {
                    long reward;
                    string message;
                    bool success = harbourmasterContracts.TryCompleteActive(harbourmaster, out reward, out message);
                    statusLabel.text = message;
                    furnitureTitle.text = "HARBOURMASTER / CONTRACT DESK";
                    furnitureDetail.text = message;
                    RenderHarbourmasterContracts(harbourmaster);
                });
                vendorActions.Add(deliver);
            }
            else
            {
                Button accept = MakeButton("ACCEPT NEXT HARBOUR CONTRACT", () =>
                {
                    string message;
                    bool success = harbourmasterContracts.TryAcceptNext(harbourmaster, out message);
                    statusLabel.text = message;
                    furnitureTitle.text = "HARBOURMASTER / CONTRACT DESK";
                    furnitureDetail.text = message;
                    RenderHarbourmasterContracts(harbourmaster);
                });
                vendorActions.Add(accept);
            }

            vendorActions.Add(new Label("RESEARCH AND CHART PERMITS"));
            if (harbourmasterResearch == null)
            {
                vendorActions.Add(new Label("The research ledger is unavailable."));
                return;
            }
            Label researchText = new Label(harbourmasterResearch.GetBoardText(harbourmaster));
            researchText.style.whiteSpace = WhiteSpace.Normal;
            researchText.style.color = new Color(0.78f, 0.82f, 0.75f);
            vendorActions.Add(researchText);
            Button submitResearch = MakeButton("SUBMIT RESEARCH EVIDENCE", () =>
            {
                string message;
                harbourmasterResearch.TrySubmitNext(harbourmaster, out message);
                statusLabel.text = message;
                furnitureTitle.text = "HARBOURMASTER / RESEARCH DESK";
                furnitureDetail.text = message;
                RenderHarbourmasterContracts(harbourmaster);
            });
            submitResearch.SetEnabled(harbourmasterResearch.CanSubmitNext);
            vendorActions.Add(submitResearch);
        }

        private void RenderMaraServices()
        {
            if (vendorActions == null) return;
            vendorActions.Clear();
            AddWalletBalanceLabel();
            if (maraMarket == null || cargoHold == null)
            {
                vendorActions.Add(new Label("The fish market service is not connected to this vessel."));
                return;
            }

            List<string> eligibleIds = new List<string>();
            for (int i = 0; i < cargoHold.Placements.Count; i++)
            {
                CargoPlacement placement = cargoHold.Placements[i];
                CargoItemDefinition definition = placement != null
                    ? cargoHold.GetItemDefinition(placement.itemId) : null;
                if (definition != null && definition.OrdinarySaleEligible && !definition.IsAberrant &&
                    (definition.Category == CargoItemCategory.Fish ||
                     definition.Category == CargoItemCategory.Shellfish ||
                     definition.Category == CargoItemCategory.Cephalopod))
                    eligibleIds.Add(placement.instanceId);
            }

            List<MarketSaleLine> offer = null;
            long total = 0L;
            string error = null;
            bool canQuote = eligibleIds.Count > 0 &&
                maraMarket.TryBuildOffer(eligibleIds, out offer, out total, out error);
            Label summary = new Label(canQuote
                ? "SEAFOOD OFFER  /  " + FormatMoney(total) + " USD  /  " + offer.Count + " CARGO LOTS"
                : (eligibleIds.Count == 0 ? "No eligible seafood in the hold." : (error ?? "Market offer unavailable.")));
            summary.style.whiteSpace = WhiteSpace.Normal;
            summary.style.color = new Color(0.82f, 0.78f, 0.66f);
            vendorActions.Add(summary);
            if (canQuote)
            {
                System.Text.StringBuilder itemized = new System.Text.StringBuilder();
                for (int lineIndex = 0; lineIndex < offer.Count; lineIndex++)
                {
                    MarketSaleLine line = offer[lineIndex];
                    itemized.Append(line.displayName);
                    itemized.Append("  /  ");
                    itemized.Append(line.saleQuantity.ToString("0.00"));
                    itemized.Append(" ");
                    itemized.Append(line.unitLabel);
                    itemized.Append("  /  ");
                    itemized.Append(FormatMoney(line.lineTotalCents));
                    itemized.Append(" USD  /  DEMAND ");
                    itemized.Append(line.demandFactor.ToString("0.00"));
                    if (lineIndex < offer.Count - 1) itemized.Append("\\n");
                }
                Label details = new Label(itemized.ToString());
                details.style.whiteSpace = WhiteSpace.Normal;
                details.style.color = new Color(0.73f, 0.79f, 0.73f);
                details.style.fontSize = 11;
                vendorActions.Add(details);
            }

            if (canQuote)
            {
                Button sell = MakeButton("SELL ALL ELIGIBLE SEAFOOD  /  " + FormatMoney(total), () =>
                {
                    List<MarketSaleLine> receipt;
                    long paid;
                    string failure;
                    bool success = maraMarket.TrySell(eligibleIds,
                        "dockside.mara.sale." + Guid.NewGuid().ToString("N"),
                        out receipt, out paid, out failure);
                    statusLabel.text = success
                        ? "SALE COMPLETE  /  +" + FormatMoney(paid) + " USD  /  " + receipt.Count + " LOTS SOLD."
                        : "SALE NOT COMPLETED  /  " + failure;
                    furnitureTitle.text = "MARA'S FISH MARKET";
                    furnitureDetail.text = statusLabel.text;
                    RenderMaraServices();
                });
                vendorActions.Add(sell);
            }
            else
            {
                Button refresh = MakeButton("RECHECK MARKET OFFER", RenderMaraServices);
                vendorActions.Add(refresh);
            }
        }

        private void RenderTomasServices()
        {
            if (vendorActions == null) return;
            vendorActions.Clear();
            AddWalletBalanceLabel();
            if (vendorServices == null)
            {
                vendorActions.Add(new Label("Tomas's service counter is not connected to this vessel."));
                return;
            }

            long repairQuote = vendorServices.GetRepairQuoteCents();
            Button repair = MakeButton("REPAIR HULL + ENGINE  /  " + FormatMoney(repairQuote), () =>
            {
                long charged;
                string error;
                bool success = vendorServices.TryRepairAll(
                    "dockside.tomas.repair." + Guid.NewGuid().ToString("N"), out charged, out error);
                statusLabel.text = success
                    ? "REPAIRS COMPLETE  /  -" + FormatMoney(charged) + " USD."
                    : "REPAIR DECLINED  /  " + error;
                furnitureTitle.text = "TOMAS'S REPAIR SHED";
                furnitureDetail.text = statusLabel.text;
                RenderTomasServices();
            });
            repair.SetEnabled(repairQuote > 0L && vendorServices.WalletCents >= repairQuote);
            vendorActions.Add(repair);

            long fuelQuote = vendorServices.GetRefuelQuoteCents(25f);
            Button refuel = MakeButton("REFUEL UP TO 25 UNITS  /  " + FormatMoney(fuelQuote), () =>
            {
                float added;
                long charged;
                string error;
                bool success = vendorServices.TryRefuel(25f,
                    "dockside.tomas.fuel." + Guid.NewGuid().ToString("N"),
                    out added, out charged, out error);
                statusLabel.text = success
                    ? "REFUELLED  /  +" + added.ToString("0.0") + " UNITS  /  -" + FormatMoney(charged) + " USD."
                    : "REFUEL DECLINED  /  " + error;
                furnitureTitle.text = "TOMAS'S FUEL PUMP";
                furnitureDetail.text = statusLabel.text;
                RenderTomasServices();
            });
            refuel.SetEnabled(fuelQuote > 0L && vendorServices.WalletCents >= fuelQuote);
            vendorActions.Add(refuel);

            if (cargoHold != null)
            {
                for (int y = 0; y < cargoHold.Height; y++)
                {
                    for (int x = 0; x < cargoHold.Width; x++)
                    {
                        if (!cargoHold.IsCellBlocked(x, y)) continue;
                        int cellX = x;
                        int cellY = y;
                        long cellQuote = vendorServices.GetHoldRepairQuoteCents();
                        Button cellRepair = MakeButton("REPAIR HOLD CELL " + (x + 1) + "-" + (y + 1) +
                            "  /  " + FormatMoney(cellQuote), () =>
                            {
                                long charged;
                                string error;
                                bool success = vendorServices.TryRepairHoldCell(cellX, cellY,
                                    "dockside.tomas.hold." + Guid.NewGuid().ToString("N"),
                                    out charged, out error);
                                statusLabel.text = success
                                    ? "HOLD CELL REPAIRED  /  -" + FormatMoney(charged) + " USD."
                                    : "HOLD REPAIR DECLINED  /  " + error;
                                RenderTomasServices();
                            });
                        cellRepair.SetEnabled(cellQuote > 0L && vendorServices.WalletCents >= cellQuote);
                        vendorActions.Add(cellRepair);
                    }
                }
            }
            if (equipmentController != null && vendorServices != null)
            {
                AddEquipmentRepairAction(VesselEquipmentController.EngineSlot, "ENGINE");
                AddEquipmentRepairAction(VesselEquipmentController.RodSlot, "FISHING ROD");
                AddEquipmentRepairAction(VesselEquipmentController.LanternSlot, "DECK LANTERN");
            }
        }

        private void AddEquipmentRepairAction(string slotId, string label)
        {
            if (vendorActions == null || vendorServices == null || equipmentController == null) return;
            long quote = vendorServices.GetEquipmentRepairQuoteCents(slotId);
            if (quote <= 0L) return;
            Button repair = MakeButton("SERVICE " + label + "  /  " + FormatMoney(quote) + " USD", () =>
            {
                long charged;
                string error;
                bool success = vendorServices.TryRepairEquipmentSlot(slotId,
                    "dockside.tomas.equipment." + slotId + "." + Guid.NewGuid().ToString("N"),
                    out charged, out error);
                statusLabel.text = success ? label + " SERVICED  /  -" + FormatMoney(charged) + " USD." :
                    "EQUIPMENT SERVICE DECLINED  /  " + error;
                RenderTomasServices();
            });
            repair.SetEnabled(vendorServices.WalletCents >= quote);
            vendorActions.Add(repair);
        }

        private void RenderInezServices()
        {
            if (vendorActions == null) return;
            vendorActions.Clear();
            AddWalletBalanceLabel();
            if (vendorServices == null)
            {
                vendorActions.Add(new Label("Inez's supply counter is not connected to this vessel."));
                return;
            }

            List<string> salvageIds = new List<string>();
            if (cargoHold != null)
            {
                for (int i = 0; i < cargoHold.Placements.Count; i++)
                {
                    CargoPlacement placement = cargoHold.Placements[i];
                    CargoItemDefinition definition = placement != null
                        ? cargoHold.GetItemDefinition(placement.itemId) : null;
                    if (definition != null && definition.Category == CargoItemCategory.Salvage &&
                        definition.OrdinarySaleEligible)
                        salvageIds.Add(placement.instanceId);
                }
            }

            List<MarketSaleLine> salvageOffer = null;
            long salvageTotal = 0L;
            string salvageError = null;
            bool canSellSalvage = vendorServices.TryBuildSalvageOffer(
                salvageIds, out salvageOffer, out salvageTotal, out salvageError);
            Label salvageSummary = new Label(canSellSalvage
                ? "SALVAGE APPRAISAL  /  " + FormatMoney(salvageTotal) + " USD  /  " + salvageOffer.Count + " ITEMS"
                : (salvageIds.Count == 0 ? "No eligible salvage aboard." : (salvageError ?? "Salvage offer unavailable.")));
            salvageSummary.style.whiteSpace = WhiteSpace.Normal;
            salvageSummary.style.color = new Color(0.82f, 0.78f, 0.66f);
            vendorActions.Add(salvageSummary);

            if (canSellSalvage)
            {
                System.Text.StringBuilder salvageLines = new System.Text.StringBuilder();
                for (int i = 0; i < salvageOffer.Count; i++)
                {
                    MarketSaleLine line = salvageOffer[i];
                    salvageLines.Append(line.displayName);
                    salvageLines.Append("  /  ");
                    salvageLines.Append(line.saleQuantity.ToString("0.00"));
                    salvageLines.Append(" ");
                    salvageLines.Append(line.unitLabel);
                    salvageLines.Append("  /  ");
                    salvageLines.Append(FormatMoney(line.lineTotalCents));
                    salvageLines.Append(" USD  /  DEMAND ");
                    salvageLines.Append(line.demandFactor.ToString("0.00"));
                    salvageLines.Append("  /  STOCK ");
                    salvageLines.Append(line.buyerStock);
                    salvageLines.Append("/");
                    salvageLines.Append(line.buyerTargetStock);
                    if (i < salvageOffer.Count - 1) salvageLines.Append("\\n");
                }
                Label linesLabel = new Label(salvageLines.ToString());
                linesLabel.style.whiteSpace = WhiteSpace.Normal;
                linesLabel.style.fontSize = 11;
                vendorActions.Add(linesLabel);

                Button sellSalvage = MakeButton("SELL ELIGIBLE SALVAGE  /  " + FormatMoney(salvageTotal), () =>
                {
                    List<MarketSaleLine> receipt;
                    long paid;
                    string error;
                    bool success = vendorServices.TrySellSalvage(salvageIds,
                        "dockside.inez.salvage." + Guid.NewGuid().ToString("N"),
                        out receipt, out paid, out error);
                    statusLabel.text = success
                        ? "SALVAGE SOLD  /  +" + FormatMoney(paid) + " USD."
                        : "SALVAGE SALE DECLINED  /  " + error;
                    furnitureTitle.text = "INEZ'S SALVAGE APPRAISAL";
                    furnitureDetail.text = statusLabel.text;
                    RenderInezServices();
                });
                vendorActions.Add(sellSalvage);
            }

            string[] itemIds =
            {
                "SUP-BAIT-WORM", "SUP-BAIT-FISH", "SUP-BAIT-SHELLFISH", "SUP-BAIT-LURE",
                "GEAR-ROD-01", "GEAR-LAMP-01", "GEAR-ENGINE-01"
            };
            for (int i = 0; i < itemIds.Length; i++)
            {
                string itemId = itemIds[i];
                CargoItemDefinition definition = cargoHold != null ? cargoHold.GetItemDefinition(itemId) : null;
                if (definition == null) continue;
                int quantity = itemId.StartsWith("SUP-BAIT-", StringComparison.Ordinal) ? 5 : 1;
                long quote = vendorServices.GetPurchaseQuoteCents(itemId, quantity);
                int stock = vendorServices.GetStock(itemId);
                Button purchase = MakeButton("BUY " + definition.DisplayName.ToUpperInvariant() +
                    " x" + quantity + "  /  " + FormatMoney(quote) + "  /  STOCK " + stock, () =>
                {
                    long charged;
                    string error;
                    bool success = vendorServices.TryPurchase(itemId, quantity,
                        "dockside.inez.purchase." + itemId + "." + Guid.NewGuid().ToString("N"),
                        out charged, out error);
                    statusLabel.text = success
                        ? "PURCHASE COMPLETE  /  " + definition.DisplayName + " x" + quantity +
                          "  /  -" + FormatMoney(charged) + " USD."
                        : "PURCHASE DECLINED  /  " + error;
                    furnitureTitle.text = "INEZ'S NET AND SUPPLY SHED";
                    furnitureDetail.text = statusLabel.text;
                    RenderInezServices();
                });
                purchase.SetEnabled(quote > 0L && vendorServices.WalletCents >= quote);
                vendorActions.Add(purchase);

                string slotId = VesselEquipmentController.GetSlotForItem(itemId);
                if (!string.IsNullOrEmpty(slotId) && equipmentController != null)
                {
                    Button equip = MakeButton("INSTALL " + definition.DisplayName.ToUpperInvariant() +
                        (itemId == equipmentController.EngineItemId || itemId == equipmentController.RodItemId ||
                         itemId == equipmentController.LanternItemId ? "  /  CURRENTLY INSTALLED" : "  /  EQUIP"),
                        () =>
                        {
                            string message;
                            bool success = equipmentController.TryEquip(itemId, out message);
                            statusLabel.text = message;
                            furnitureTitle.text = "INEZ'S EQUIPMENT FITTING";
                            furnitureDetail.text = message;
                            RenderInezServices();
                        });
                    equip.SetEnabled(cargoHold != null && cargoHold.GetAvailableQuantity(itemId) > 0 &&
                        itemId != equipmentController.EngineItemId &&
                        itemId != equipmentController.RodItemId &&
                        itemId != equipmentController.LanternItemId);
                    vendorActions.Add(equip);
                }
            }
            if (equipmentController != null)
            {
                AddInstalledEquipmentAction(VesselEquipmentController.EngineSlot, "ENGINE");
                AddInstalledEquipmentAction(VesselEquipmentController.RodSlot, "FISHING ROD");
                AddInstalledEquipmentAction(VesselEquipmentController.LanternSlot, "DECK LANTERN");
            }
        }

        private void AddInstalledEquipmentAction(string slotId, string label)
        {
            if (equipmentController == null || vendorActions == null) return;
            string itemId = slotId == VesselEquipmentController.EngineSlot ? equipmentController.EngineItemId :
                slotId == VesselEquipmentController.RodSlot ? equipmentController.RodItemId :
                equipmentController.LanternItemId;
            if (string.IsNullOrEmpty(itemId)) return;
            float condition = slotId == VesselEquipmentController.EngineSlot ? equipmentController.EngineCondition :
                slotId == VesselEquipmentController.RodSlot ? equipmentController.RodCondition :
                equipmentController.LanternCondition;
            Button unequip = MakeButton("STORE " + label + "  /  " + Mathf.RoundToInt(condition * 100f) + "% CONDITION", () =>
            {
                string message;
                equipmentController.TryUnequip(slotId, out message);
                statusLabel.text = message;
                RenderInezServices();
            });
            vendorActions.Add(unequip);
        }

        private void AddWalletBalanceLabel()
        {
            long balance = vendorServices != null ? vendorServices.WalletCents :
                (maraMarket != null ? maraMarket.WalletCents : 0L);
            Label walletLabel = new Label("AVAILABLE WALLET  /  " + FormatMoney(balance) + " USD");
            walletLabel.style.color = new Color(0.78f, 0.84f, 0.75f);
            walletLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            walletLabel.style.marginBottom = 5;
            vendorActions.Add(walletLabel);
            ExpeditionEconomyTelemetry telemetry = vesselBody != null
                ? vesselBody.GetComponent<ExpeditionEconomyTelemetry>() : null;
            if (telemetry != null)
            {
                Label expedition = new Label(telemetry.BuildSummary());
                expedition.style.whiteSpace = WhiteSpace.Normal;
                expedition.style.fontSize = 10;
                expedition.style.color = new Color(0.72f, 0.78f, 0.73f);
                expedition.style.marginBottom = 8;
                vendorActions.Add(expedition);
            }
        }

        private static string FormatMoney(long cents)
        {
            return (cents / 100m).ToString("0.00");
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
                   "%\nFUEL  /  " +
                   (fuelController != null
                       ? fuelController.FuelUnits.ToString("0.0") + " / " + fuelController.Capacity.ToString("0.0") +
                         " UNITS  /  EMERGENCY RESERVE " + fuelController.EmergencyFuelUnits.ToString("0.0") +
                         "  /  FULL-LOAD ESTIMATE " + fuelController.EstimatedFullLoadMinutes.ToString("0") + " MIN"
                       : "UNAVAILABLE") +
                   "\nLAST DAMAGE REPORT  /  " +
                   (string.IsNullOrWhiteSpace(vesselCondition.LastDamageReport)
                       ? "No recent damage report."
                       : vesselCondition.LastDamageReport) +
                   "\n\nUse Tomas's service counter for paid repairs and refuelling.";
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

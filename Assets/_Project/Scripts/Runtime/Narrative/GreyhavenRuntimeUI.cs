using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Yarn.Unity;
using WhereTheSoundingsEnd.Inventory;
using WhereTheSoundingsEnd.Persistence;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>
    /// Runtime UI Toolkit presenter for Yarn dialogue and the starter vessel's spatial cargo hold.
    /// This is the Greyhaven prototype interface; it intentionally owns presentation, not game rules.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GreyhavenRuntimeUI : DialoguePresenterBase
    {
        [Header("Scene references")]
        [SerializeField] private UIDocument document;
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private SpatialCargoHold cargoHold;
        [SerializeField] private InputActionAsset controls;

        private VisualElement _root;
        private VisualElement _dialoguePanel;
        private VisualElement _optionsContainer;
        private VisualElement _inventoryPanel;
        private VisualElement _gridContainer;
        private VisualElement _catalogueContainer;
        private Label _promptLabel;
        private Label _statusLabel;
        private Label _usedCellsLabel;
        private Label _selectedItemLabel;
        private Label _speakerLabel;
        private Label _dialogueText;
        private Button _continueButton;
        private InputAction _inventoryAction;
        private bool _ownsInventoryAction;
        private bool _inventoryOpen;
        private bool _lineWaiting;
        private bool _pauseOwned;
        private float _timeScaleBeforeUi;
        private string _selectedInstanceId;
        private int _selectedRotation;
        private readonly List<Action> _optionActions = new List<Action>();
        private readonly List<Button> _optionButtons = new List<Button>();

        public UIDocument Document { get => document; set => document = value; }
        public DialogueRunner Runner { get => dialogueRunner; set => dialogueRunner = value; }
        public SpatialCargoHold CargoHold { get => cargoHold; set => cargoHold = value; }
        public InputActionAsset Controls { get => controls; set => controls = value; }
        public bool IsInventoryOpen => _inventoryOpen;

        private void Start()
        {
            InitializeUi();
            BindInventoryInput();
        }

        private void OnDestroy()
        {
            if (_ownsInventoryAction && _inventoryAction != null)
                _inventoryAction.Disable();
            ReleasePause();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (_inventoryAction != null && _inventoryAction.WasPressedThisFrame())
                ToggleInventory();

            if (keyboard == null) return;

            if (_inventoryOpen)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                    SetInventoryVisible(false);
                if (keyboard.rKey.wasPressedThisFrame)
                    RotateSelected();
                return;
            }

            if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
            {
                if (_lineWaiting &&
                    (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                {
                    dialogueRunner.RequestNextLine();
                }

                if (_optionActions.Count > 0)
                {
                    if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) ChooseOption(0);
                    else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) ChooseOption(1);
                    else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) ChooseOption(2);
                    else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) ChooseOption(3);
                    else if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame) ChooseOption(4);
                }
            }
        }

        private void InitializeUi()
        {
            if (document == null || document.rootVisualElement == null)
            {
                Debug.LogError("GreyhavenRuntimeUI requires a UIDocument with PanelSettings.", this);
                return;
            }

            _root = document.rootVisualElement;
            _root.Clear();
            _root.style.flexGrow = 1f;
            _root.style.paddingLeft = 22;
            _root.style.paddingRight = 22;
            _root.style.paddingTop = 18;
            _root.style.paddingBottom = 18;
            _root.style.flexDirection = FlexDirection.Column;
            _root.pickingMode = PickingMode.Ignore;

            BuildHud();
            BuildDialoguePanel();
            BuildInventoryPanel();
            RefreshInventory();
        }

        private void BindInventoryInput()
        {
            if (controls == null) return;

            InputActionMap map = controls.FindActionMap("Boat", false);
            _inventoryAction = map?.FindAction("Inventory", false);
            if (_inventoryAction != null && !_inventoryAction.enabled)
            {
                _inventoryAction.Enable();
                _ownsInventoryAction = true;
            }
        }

        private void BuildHud()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.paddingLeft = 14;
            row.style.paddingRight = 14;
            row.style.paddingTop = 9;
            row.style.paddingBottom = 9;
            row.style.backgroundColor = new Color(0.035f, 0.075f, 0.085f, 0.94f);
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.40f, 0.48f, 0.45f, 0.85f);
            _root.Add(row);

            Label title = new Label("GREYHAVEN  /  FIELD LOG");
            title.style.color = new Color(0.83f, 0.85f, 0.79f);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 13;
            row.Add(title);

            Button holdButton = MakeButton("CARGO HOLD  [I]", () => ToggleInventory());
            holdButton.style.minWidth = 150;
            row.Add(holdButton);

            _promptLabel = new Label(string.Empty);
            _promptLabel.style.color = new Color(0.88f, 0.78f, 0.55f);
            _promptLabel.style.fontSize = 13;
            _promptLabel.style.marginTop = 8;
            _promptLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _root.Add(_promptLabel);
        }

        private void BuildDialoguePanel()
        {
            _dialoguePanel = new VisualElement();
            _dialoguePanel.style.position = Position.Absolute;
            _dialoguePanel.style.left = 0;
            _dialoguePanel.style.right = 0;
            _dialoguePanel.style.bottom = 18;
            _dialoguePanel.style.alignSelf = Align.Center;
            _dialoguePanel.style.width = new Length(78, LengthUnit.Percent);
            _dialoguePanel.style.maxWidth = 900;
            _dialoguePanel.style.paddingLeft = 24;
            _dialoguePanel.style.paddingRight = 24;
            _dialoguePanel.style.paddingTop = 18;
            _dialoguePanel.style.paddingBottom = 18;
            _dialoguePanel.style.backgroundColor = new Color(0.025f, 0.055f, 0.065f, 0.97f);
            _dialoguePanel.style.borderTopWidth = 2;
            _dialoguePanel.style.borderTopColor = new Color(0.66f, 0.30f, 0.20f);
            _dialoguePanel.style.borderBottomWidth = 1;
            _dialoguePanel.style.borderBottomColor = new Color(0.35f, 0.43f, 0.42f);
            _dialoguePanel.style.display = DisplayStyle.None;
            _dialoguePanel.pickingMode = PickingMode.Position;
            _root.Add(_dialoguePanel);

            _speakerLabel = new Label("GREYHAVEN");
            _speakerLabel.style.color = new Color(0.85f, 0.54f, 0.36f);
            _speakerLabel.style.fontSize = 12;
            _speakerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _speakerLabel.style.marginBottom = 8;
            _dialoguePanel.Add(_speakerLabel);

            _dialogueText = new Label(string.Empty);
            _dialogueText.style.color = new Color(0.91f, 0.91f, 0.84f);
            _dialogueText.style.fontSize = 17;
            _dialogueText.style.whiteSpace = WhiteSpace.Normal;
            _dialogueText.style.marginBottom = 14;
            _dialoguePanel.Add(_dialogueText);

            _optionsContainer = new VisualElement();
            _optionsContainer.style.flexDirection = FlexDirection.Column;
            _dialoguePanel.Add(_optionsContainer);

            _continueButton = MakeButton("CONTINUE  [SPACE / ENTER]", () =>
            {
                if (dialogueRunner != null && _lineWaiting)
                    dialogueRunner.RequestNextLine();
            });
            _continueButton.style.alignSelf = Align.FlexEnd;
            _continueButton.style.minWidth = 210;
            _dialoguePanel.Add(_continueButton);
        }

        private void BuildInventoryPanel()
        {
            _inventoryPanel = new VisualElement();
            _inventoryPanel.style.position = Position.Absolute;
            _inventoryPanel.style.left = 0;
            _inventoryPanel.style.right = 0;
            _inventoryPanel.style.top = 0;
            _inventoryPanel.style.bottom = 0;
            _inventoryPanel.style.justifyContent = Justify.Center;
            _inventoryPanel.style.alignItems = Align.Center;
            _inventoryPanel.style.backgroundColor = new Color(0.008f, 0.02f, 0.024f, 0.88f);
            _inventoryPanel.style.display = DisplayStyle.None;
            _inventoryPanel.pickingMode = PickingMode.Position;
            _root.Add(_inventoryPanel);

            VisualElement window = new VisualElement();
            window.style.width = new Length(94, LengthUnit.Percent);
            window.style.maxWidth = 1040;
            window.style.maxHeight = new Length(92, LengthUnit.Percent);
            window.style.paddingLeft = 20;
            window.style.paddingRight = 20;
            window.style.paddingTop = 16;
            window.style.paddingBottom = 16;
            window.style.backgroundColor = new Color(0.045f, 0.085f, 0.09f, 0.99f);
            window.style.borderTopWidth = 2;
            window.style.borderTopColor = new Color(0.65f, 0.31f, 0.21f);
            window.style.borderBottomWidth = 1;
            window.style.borderBottomColor = new Color(0.33f, 0.43f, 0.42f);
            window.style.flexDirection = FlexDirection.Column;
            _inventoryPanel.Add(window);

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 12;
            window.Add(header);

            Label heading = new Label("VESSEL HOLD");
            heading.style.color = new Color(0.91f, 0.86f, 0.72f);
            heading.style.fontSize = 20;
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(heading);

            Button close = MakeButton("CLOSE  [ESC]", () => SetInventoryVisible(false));
            header.Add(close);

            _usedCellsLabel = new Label("0 / 63 CELLS");
            _usedCellsLabel.style.color = new Color(0.66f, 0.76f, 0.72f);
            _usedCellsLabel.style.fontSize = 12;
            _usedCellsLabel.style.marginBottom = 10;
            window.Add(_usedCellsLabel);

            VisualElement body = new VisualElement();
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1;
            body.style.minHeight = 0;
            window.Add(body);

            VisualElement holdSide = new VisualElement();
            holdSide.style.flexGrow = 1;
            holdSide.style.marginRight = 18;
            holdSide.style.minWidth = 0;
            body.Add(holdSide);

            VisualElement gridFrame = new VisualElement();
            gridFrame.style.paddingLeft = 10;
            gridFrame.style.paddingRight = 10;
            gridFrame.style.paddingTop = 10;
            gridFrame.style.paddingBottom = 10;
            gridFrame.style.backgroundColor = new Color(0.018f, 0.04f, 0.046f);
            gridFrame.style.borderTopWidth = 1;
            gridFrame.style.borderTopColor = new Color(0.29f, 0.39f, 0.38f);
            holdSide.Add(gridFrame);

            _gridContainer = new VisualElement();
            _gridContainer.style.flexDirection = FlexDirection.Column;
            _gridContainer.style.alignItems = Align.FlexStart;
            gridFrame.Add(_gridContainer);

            _selectedItemLabel = new Label("Select cargo to move or inspect.");
            _selectedItemLabel.style.color = new Color(0.77f, 0.79f, 0.72f);
            _selectedItemLabel.style.whiteSpace = WhiteSpace.Normal;
            _selectedItemLabel.style.marginTop = 10;
            holdSide.Add(_selectedItemLabel);

            VisualElement actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.flexWrap = Wrap.Wrap;
            actions.style.marginTop = 9;
            holdSide.Add(actions);
            actions.Add(MakeButton("ROTATE  [R]", RotateSelected));
            actions.Add(MakeButton("REMOVE", RemoveSelected));

            VisualElement catalogue = new VisualElement();
            catalogue.style.width = 270;
            catalogue.style.minWidth = 220;
            catalogue.style.paddingLeft = 12;
            catalogue.style.paddingRight = 12;
            catalogue.style.paddingTop = 10;
            catalogue.style.paddingBottom = 10;
            catalogue.style.backgroundColor = new Color(0.025f, 0.06f, 0.067f);
            body.Add(catalogue);

            Label catalogueTitle = new Label("CATCH INTAKE");
            catalogueTitle.style.color = new Color(0.86f, 0.83f, 0.72f);
            catalogueTitle.style.fontSize = 14;
            catalogueTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            catalogue.Add(catalogueTitle);

            Label catalogueNote = new Label("Prototype test intake. Real fishing output is not connected yet.");
            catalogueNote.style.color = new Color(0.62f, 0.70f, 0.66f);
            catalogueNote.style.fontSize = 11;
            catalogueNote.style.whiteSpace = WhiteSpace.Normal;
            catalogueNote.style.marginTop = 5;
            catalogueNote.style.marginBottom = 10;
            catalogue.Add(catalogueNote);

            _catalogueContainer = new VisualElement();
            _catalogueContainer.style.flexDirection = FlexDirection.Column;
            catalogue.Add(_catalogueContainer);

            _statusLabel = new Label("The hold is clear.");
            _statusLabel.style.color = new Color(0.80f, 0.72f, 0.57f);
            _statusLabel.style.fontSize = 12;
            _statusLabel.style.whiteSpace = WhiteSpace.Normal;
            _statusLabel.style.marginTop = 12;
            window.Add(_statusLabel);

            Label controlsHint = new Label("Select a packed item, then click an empty cell to move it. R rotates. Quest cargo cannot be discarded.");
            controlsHint.style.color = new Color(0.56f, 0.65f, 0.62f);
            controlsHint.style.fontSize = 11;
            controlsHint.style.whiteSpace = WhiteSpace.Normal;
            controlsHint.style.marginTop = 6;
            window.Add(controlsHint);
        }

        private void RefreshInventory()
        {
            if (_gridContainer == null || cargoHold == null) return;

            _gridContainer.Clear();
            for (int y = cargoHold.Height - 1; y >= 0; y--)
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 3;
                _gridContainer.Add(row);

                for (int x = 0; x < cargoHold.Width; x++)
                {
                    int cellX = x;
                    int cellY = y;
                    CargoPlacement placement;
                    bool hasCargo = cargoHold.TryGetPlacementAtCell(x, y, out placement);
                    bool blocked = cargoHold.IsCellBlocked(x, y);

                    string label = blocked ? "×" : string.Empty;
                    Color background = blocked
                        ? new Color(0.16f, 0.19f, 0.18f)
                        : new Color(0.11f, 0.17f, 0.17f);

                    if (hasCargo)
                    {
                        CargoItemDefinition definition = cargoHold.GetItemDefinition(placement.itemId);
                        label = definition == null
                            ? "?"
                            : (x == placement.anchorX && y == placement.anchorY
                                ? ShortName(definition.DisplayName)
                                : "·");
                        background = string.Equals(placement.instanceId, _selectedInstanceId, StringComparison.Ordinal)
                            ? new Color(0.63f, 0.28f, 0.18f)
                            : new Color(0.25f, 0.39f, 0.35f);
                    }

                    Button cellButton = MakeButton(label, () => HandleCellPressed(cellX, cellY));
                    cellButton.style.width = 43;
                    cellButton.style.height = 41;
                    cellButton.style.minWidth = 43;
                    cellButton.style.paddingLeft = 1;
                    cellButton.style.paddingRight = 1;
                    cellButton.style.paddingTop = 1;
                    cellButton.style.paddingBottom = 1;
                    cellButton.style.marginRight = 3;
                    cellButton.style.backgroundColor = background;
                    cellButton.style.color = new Color(0.92f, 0.91f, 0.83f);
                    cellButton.style.fontSize = 11;
                    cellButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                    row.Add(cellButton);
                }
            }

            int used = cargoHold.GetUsedCellCount();
            _usedCellsLabel.text = used + " / " + (cargoHold.Width * cargoHold.Height) + " CELLS OCCUPIED";

            CargoPlacement selected = FindSelectedPlacement();
            if (selected == null)
            {
                _selectedInstanceId = null;
                _selectedItemLabel.text = "Select cargo to inspect or move it.";
            }
            else
            {
                CargoItemDefinition definition = cargoHold.GetItemDefinition(selected.itemId);
                string name = definition != null ? definition.DisplayName : selected.itemId;
                _selectedRotation = selected.rotationQuarterTurns;
                _selectedItemLabel.text = name + "  ·  Qty " + selected.quantity +
                    "  ·  Quality " + selected.quality.ToString("P0") +
                    "  ·  Freshness " + selected.freshness.ToString("P0") +
                    "  ·  Rotation " + (selected.rotationQuarterTurns * 90) + "°";
            }

            RefreshCatalogue();
        }

        private void RefreshCatalogue()
        {
            if (_catalogueContainer == null || cargoHold == null) return;
            _catalogueContainer.Clear();

            IReadOnlyList<CargoItemDefinition> definitions = cargoHold.ItemDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CargoItemDefinition definition = definitions[i];
                if (definition == null || definition.Category == CargoItemCategory.Quest ||
                    definition.Category == CargoItemCategory.Equipment)
                    continue;

                string itemId = definition.ItemId;
                string itemName = definition.DisplayName;
                Button intake = MakeButton("+  " + itemName, () => IntakePrototypeCatch(itemId));
                intake.style.width = new Length(100, LengthUnit.Percent);
                intake.style.marginBottom = 6;
                intake.style.unityTextAlign = TextAnchor.MiddleLeft;
                _catalogueContainer.Add(intake);
            }
        }

        private void HandleCellPressed(int x, int y)
        {
            if (cargoHold == null) return;

            CargoPlacement clicked;
            if (cargoHold.TryGetPlacementAtCell(x, y, out clicked))
            {
                if (string.Equals(clicked.instanceId, _selectedInstanceId, StringComparison.Ordinal))
                {
                    SetStatus("That cell belongs to the selected cargo.");
                }
                else
                {
                    _selectedInstanceId = clicked.instanceId;
                    _selectedRotation = clicked.rotationQuarterTurns;
                    SetStatus("Selected " + clicked.itemId + ".");
                }
                RefreshInventory();
                return;
            }

            if (cargoHold.IsCellBlocked(x, y))
            {
                SetStatus("That part of the hold is obstructed.");
                return;
            }

            CargoPlacement selected = FindSelectedPlacement();
            if (selected == null)
            {
                SetStatus("Choose a cargo item before moving it.");
                return;
            }

            if (cargoHold.TryMove(selected.instanceId, x, y, _selectedRotation))
            {
                SetStatus("Cargo repositioned.");
                RefreshInventory();
            }
            else
            {
                SetStatus("That shape will not fit there. Existing cargo was left untouched.");
            }
        }

        private void IntakePrototypeCatch(string itemId)
        {
            if (cargoHold == null) return;

            string instanceId;
            if (cargoHold.TryPlaceFirstFit(itemId, 1, 0.92f, 1f, out instanceId))
            {
                _selectedInstanceId = instanceId;
                SetStatus("Catch packed into the first available space.");
            }
            else
            {
                SetStatus("No space for that catch. Rearrange the hold or return to harbour.");
            }

            RefreshInventory();
        }

        private void RotateSelected()
        {
            CargoPlacement selected = FindSelectedPlacement();
            if (selected == null)
            {
                SetStatus("Select cargo before rotating it.");
                return;
            }

            _selectedRotation = (_selectedRotation + 1) % 4;
            _selectedItemLabel.text = "Rotation preview: " + (_selectedRotation * 90) +
                                      "°. Click an empty cell to apply it.";
            SetStatus("Rotation preview changed. Placement is committed only when it fits.");
        }

        private void RemoveSelected()
        {
            CargoPlacement selected = FindSelectedPlacement();
            if (selected == null)
            {
                SetStatus("Select cargo before removing it.");
                return;
            }

            CargoItemDefinition definition = cargoHold.GetItemDefinition(selected.itemId);
            if (definition != null && definition.IsQuestCritical)
            {
                SetStatus("Quest-critical cargo cannot be discarded.");
                return;
            }

            CargoPlacement removed;
            if (cargoHold.TryRemove(selected.instanceId, out removed))
            {
                _selectedInstanceId = null;
                SetStatus("Removed " + (definition != null ? definition.DisplayName : removed.itemId) + ".");
            }

            RefreshInventory();
        }

        private CargoPlacement FindSelectedPlacement()
        {
            if (cargoHold == null || string.IsNullOrEmpty(_selectedInstanceId)) return null;

            IReadOnlyList<CargoPlacement> placements = cargoHold.Placements;
            for (int i = 0; i < placements.Count; i++)
            {
                CargoPlacement placement = placements[i];
                if (placement != null &&
                    string.Equals(placement.instanceId, _selectedInstanceId, StringComparison.Ordinal))
                    return placement;
            }

            return null;
        }

        private void ToggleInventory()
        {
            if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return;
            SetInventoryVisible(!_inventoryOpen);
        }

        private void SetInventoryVisible(bool visible)
        {
            if (_inventoryPanel == null) return;
            _inventoryOpen = visible;
            _inventoryPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (visible)
            {
                RefreshInventory();
                AcquirePause();
                SetStatus("Plan the hold before leaving harbour.");
            }
            else
            {
                ReleasePauseWhenUiClosed();
            }
        }

        private void AcquirePause()
        {
            if (_pauseOwned) return;
            _timeScaleBeforeUi = Time.timeScale;
            _pauseOwned = true;
            Time.timeScale = 0f;
        }

        private void ReleasePauseWhenUiClosed()
        {
            bool dialogueVisible = _dialoguePanel != null && _dialoguePanel.style.display == DisplayStyle.Flex;
            if (!dialogueVisible) ReleasePause();
        }

        private void ReleasePause()
        {
            if (!_pauseOwned) return;
            Time.timeScale = _timeScaleBeforeUi;
            _pauseOwned = false;
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null) _statusLabel.text = message;
        }

        public void SetInteractionPrompt(string message)
        {
            if (_promptLabel != null) _promptLabel.text = message ?? string.Empty;
        }

        public void BeginDialogue(string nodeName)
        {
            if (dialogueRunner == null)
            {
                Debug.LogError("No DialogueRunner is assigned to GreyhavenRuntimeUI.", this);
                return;
            }

            if (dialogueRunner.IsDialogueRunning) return;
            dialogueRunner.StartDialogue(string.IsNullOrWhiteSpace(nodeName)
                ? "Greyhaven_Harbourmaster"
                : nodeName);
        }

        public void RequestCheckpointAutosave(string checkpointId)
        {
            AutosaveController autosave = FindFirstObjectByType<AutosaveController>();
            if (autosave == null)
            {
                Debug.LogWarning("Dialogue checkpoint requested but no AutosaveController is present.", this);
                return;
            }

            SaveOperationResult result = autosave.RequestCheckpointAutosave(checkpointId);
            if (!result.Success) Debug.LogWarning("Dialogue checkpoint autosave failed: " + result.Message, this);
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            if (_dialoguePanel != null)
            {
                _dialoguePanel.style.display = DisplayStyle.Flex;
                _speakerLabel.text = "GREYHAVEN";
                _dialogueText.text = string.Empty;
                _continueButton.style.display = DisplayStyle.None;
                ClearOptions();
                AcquirePause();
            }
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            _lineWaiting = false;
            ClearOptions();
            if (_dialoguePanel != null)
            {
                _dialoguePanel.style.display = DisplayStyle.None;
                _dialogueText.text = string.Empty;
                _continueButton.style.display = DisplayStyle.Flex;
            }
            ReleasePauseWhenUiClosed();
            return YarnTask.CompletedTask;
        }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            if (_dialoguePanel == null) return;

            string speaker = line.CharacterName;
            _speakerLabel.text = string.IsNullOrWhiteSpace(speaker) ? "GREYHAVEN" : speaker.ToUpperInvariant();
            _dialogueText.text = line.TextWithoutCharacterName.Text;
            _continueButton.style.display = DisplayStyle.Flex;
            _lineWaiting = true;

            await YarnTask.WaitUntilCanceled(token.NextContentToken);

            _lineWaiting = false;
            if (_continueButton != null) _continueButton.style.display = DisplayStyle.None;
        }

        public override async YarnTask<DialogueOption> RunOptionsAsync(
            DialogueOption[] dialogueOptions,
            LineCancellationToken token)
        {
            if (_optionsContainer == null || dialogueOptions == null)
                return null;

            ClearOptions();
            _continueButton.style.display = DisplayStyle.None;

            YarnTaskCompletionSource<DialogueOption> chosen = new YarnTaskCompletionSource<DialogueOption>();
            int visibleIndex = 0;

            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                DialogueOption option = dialogueOptions[i];
                if (!option.IsAvailable) continue;

                int choiceIndex = visibleIndex;
                Button button = MakeButton((choiceIndex + 1) + ".  " + option.Line.TextWithoutCharacterName.Text,
                    () => chosen.TrySetResult(option));
                button.style.width = new Length(100, LengthUnit.Percent);
                button.style.marginBottom = 6;
                button.style.unityTextAlign = TextAnchor.MiddleLeft;
                _optionsContainer.Add(button);
                _optionButtons.Add(button);
                _optionActions.Add(() => chosen.TrySetResult(option));
                visibleIndex++;
            }

            if (visibleIndex == 0) return null;

            DialogueOption result;
            using (token.NextContentToken.Register(() => chosen.TrySetResult(null)))
            {
                result = await chosen.Task;
            }

            ClearOptions();
            return result;
        }

        private void ChooseOption(int index)
        {
            if (index < 0 || index >= _optionActions.Count) return;
            _optionActions[index]?.Invoke();
        }

        private void ClearOptions()
        {
            _optionActions.Clear();
            _optionButtons.Clear();
            if (_optionsContainer != null) _optionsContainer.Clear();
        }

        private static string ShortName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "C";
            string value = displayName.Trim();
            return value.Length <= 3 ? value.ToUpperInvariant() : value.Substring(0, 2).ToUpperInvariant();
        }

        private static Button MakeButton(string text, Action callback)
        {
            Button button = new Button(callback) { text = text };
            button.style.minHeight = 34;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            button.style.paddingLeft = 10;
            button.style.paddingRight = 10;
            button.style.paddingTop = 5;
            button.style.paddingBottom = 5;
            button.style.backgroundColor = new Color(0.12f, 0.20f, 0.20f);
            button.style.color = new Color(0.89f, 0.89f, 0.82f);
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = new Color(0.31f, 0.42f, 0.40f);
            button.style.borderBottomColor = new Color(0.31f, 0.42f, 0.40f);
            button.style.borderLeftColor = new Color(0.31f, 0.42f, 0.40f);
            button.style.borderRightColor = new Color(0.31f, 0.42f, 0.40f);
            return button;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using WhereTheSoundingsEnd.Persistence;
using WhereTheSoundingsEnd.Interaction;
using WhereTheSoundingsEnd.CameraSystem;
using WhereTheSoundingsEnd.NPC;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>Title, pause, save/load and settings overlay for Greyhaven.</summary>
    [DisallowMultipleComponent]
    public sealed class GreyhavenFrontEndUI : MonoBehaviour
    {
        private UIDocument document;
        private InputActionAsset controls;
        private SaveGameCoordinator saves;
        public GreyhavenRuntimeUI GameplayUI { get; set; }
        public bool IsOverlayOpen => page != "game" || cinematicActive;
        public bool IsCinematicPlaying => cinematicActive;
        private VisualElement overlay, content, root;
        private Label feedback;
        private InputAction pauseAction;
        private bool paused, firstLaunch = true;
        private float previousTimeScale = 1f;
        private string page = "title";
        private VisualElement cinematicOverlay;
        private VisualElement cinematicCard;
        private Label cinematicEyebrow;
        private Label cinematicTitle;
        private Label cinematicCaption;
        private Label cinematicBeatCounter;
        private VisualElement cinematicProgressTrack;
        private VisualElement cinematicProgressFill;
        private bool reduceCinematicMotion;
        private CameraFollow2_5D cinematicCameraFollow;
        private Camera cinematicCamera;
        private readonly HashSet<string> firstContactNpcIds = new HashSet<string>(StringComparer.Ordinal);
        private bool cinematicActive;
        private bool skipCinematic;
        private bool hasCameraSnapshot;
        private bool cameraFollowWasEnabled;
        private Vector3 cameraSnapshotPosition;
        private Quaternion cameraSnapshotRotation;
        private float cameraSnapshotOrthographicSize;
        private float cameraSnapshotFieldOfView;
        private static readonly Color Ink = new Color(.035f,.065f,.067f,.98f);
        private static readonly Color Panel = new Color(.075f,.12f,.12f,1f);
        private static readonly Color Paper = new Color(.88f,.86f,.78f,1f);
        private static readonly Color Muted = new Color(.62f,.69f,.65f,1f);
        private static readonly Color Accent = new Color(.72f,.34f,.23f,1f);

        public void Initialize(UIDocument uiDocument, InputActionAsset input, SaveGameCoordinator coordinator)
        {
            document = uiDocument; controls = input; saves = coordinator;
            if (document == null || document.rootVisualElement == null) return;
            root = document.rootVisualElement;
            Screen.fullScreen = PlayerPrefs.GetInt("wse.fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            AudioListener.volume = PlayerPrefs.GetFloat("wse.masterVolume", 1f);
            root.style.scale = new Scale(Vector2.one * PlayerPrefs.GetFloat("wse.uiScale", 1f));
            BuildShell();
            BuildCinematicShell();
            InputActionMap map = controls != null ? controls.FindActionMap("Boat", false) : null;
            pauseAction = map != null ? map.FindAction("Pause", false) : null;
            if (pauseAction != null) pauseAction.performed += OnPause;

            if (PlayerPrefs.GetInt("wse.openingCinematicSeen", 0) == 0)
                StartCoroutine(PlayOpeningCinematic());
            else
                OpenTitle();
        }

        private void OnDestroy()
        {
            if (pauseAction != null) pauseAction.performed -= OnPause;
            RestoreCinematicCamera();
            Unpause();
        }

        private void Update()
        {
            if (!cinematicActive) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.escapeKey.wasPressedThisFrame ||
                 keyboard.spaceKey.wasPressedThisFrame ||
                 keyboard.enterKey.wasPressedThisFrame))
                skipCinematic = true;

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.startButton.wasPressedThisFrame)
                skipCinematic = true;
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            if (cinematicActive)
            {
                skipCinematic = true;
                return;
            }

            if (page == "title") return;
            if (page == "pause")
            {
                Resume();
                return;
            }

            if (page != "game")
            {
                OpenPause();
                return;
            }

            if (!DocksideModalPolicy.CanOpenPauseMenu(
                GameplayUI != null && ((GameplayUI.DocksideHub != null && GameplayUI.DocksideHub.IsMenuOpen) || GameplayUI.IsChartOpen),
                GameplayUI != null && GameplayUI.IsInventoryOpen,
                GameplayUI != null && GameplayUI.IsDialogueOpen))
                return;

            OpenPause();
        }

        private void BuildShell()
        {
            overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0; overlay.style.right = 0; overlay.style.top = 0; overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(.01f,.025f,.03f,.93f);
            overlay.style.justifyContent = Justify.Center; overlay.style.alignItems = Align.Center;
            overlay.pickingMode = PickingMode.Position;
            root.Add(overlay);
            VisualElement frame = new VisualElement();
            frame.style.width = new Length(88,LengthUnit.Percent); frame.style.maxWidth = 900;
            frame.style.maxHeight = new Length(92,LengthUnit.Percent);
            frame.style.backgroundColor = Ink; frame.style.paddingLeft = 26; frame.style.paddingRight = 26;
            frame.style.paddingTop = 22; frame.style.paddingBottom = 20;
            frame.style.borderTopWidth = 3; frame.style.borderTopColor = Accent;
            overlay.Add(frame);
            Label eyebrow = new Label("A GREYHAVEN FIELD JOURNAL");
            eyebrow.style.color = Accent; eyebrow.style.fontSize = 10; frame.Add(eyebrow);
            Label title = new Label("WHERE THE SOUNDINGS END");
            title.style.color = Paper; title.style.fontSize = 24; title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 14; frame.Add(title);
            VisualElement line = new VisualElement(); line.style.height = 1;
            line.style.backgroundColor = new Color(.29f,.40f,.38f,1f); line.style.marginBottom = 14; frame.Add(line);
            content = new VisualElement(); content.style.flexGrow = 1; content.style.minHeight = 0; frame.Add(content);
            feedback = new Label(""); feedback.style.color = new Color(.9f,.72f,.53f);
            feedback.style.fontSize = 11; feedback.style.whiteSpace = WhiteSpace.Normal;
            feedback.style.marginTop = 12; frame.Add(feedback);
            Label footer = new Label("ESC / START  PAUSE    ·    SOME WATERS SHOULD NEVER BE DISTURBED");
            footer.style.color = Muted; footer.style.fontSize = 9; footer.style.marginTop = 12; frame.Add(footer);
        }

        private void BuildCinematicShell()
        {
            cinematicOverlay = new VisualElement();
            cinematicOverlay.style.position = Position.Absolute;
            cinematicOverlay.style.left = 0;
            cinematicOverlay.style.right = 0;
            cinematicOverlay.style.top = 0;
            cinematicOverlay.style.bottom = 0;
            cinematicOverlay.style.backgroundColor = new Color(0.005f, 0.012f, 0.018f, 0.64f);
            cinematicOverlay.style.opacity = 0f;
            cinematicOverlay.style.display = DisplayStyle.None;
            cinematicOverlay.pickingMode = PickingMode.Position;

            VisualElement topBar = new VisualElement();
            topBar.style.position = Position.Absolute;
            topBar.style.left = 0;
            topBar.style.right = 0;
            topBar.style.top = 0;
            topBar.style.height = new Length(7.5f, LengthUnit.Percent);
            topBar.style.backgroundColor = new Color(0.005f, 0.008f, 0.01f, 0.98f);
            cinematicOverlay.Add(topBar);

            VisualElement bottomBar = new VisualElement();
            bottomBar.style.position = Position.Absolute;
            bottomBar.style.left = 0;
            bottomBar.style.right = 0;
            bottomBar.style.bottom = 0;
            bottomBar.style.height = new Length(7.5f, LengthUnit.Percent);
            bottomBar.style.backgroundColor = new Color(0.005f, 0.008f, 0.01f, 0.98f);
            cinematicOverlay.Add(bottomBar);

            for (int index = 0; index < 7; index++)
            {
                VisualElement scanline = new VisualElement();
                scanline.style.position = Position.Absolute;
                scanline.style.left = 0;
                scanline.style.right = 0;
                scanline.style.top = new Length(18f + index * 10f, LengthUnit.Percent);
                scanline.style.height = 1;
                scanline.style.backgroundColor = new Color(0.76f, 0.83f, 0.78f, 0.035f);
                scanline.pickingMode = PickingMode.Ignore;
                cinematicOverlay.Add(scanline);
            }

            cinematicCard = new VisualElement();
            cinematicCard.style.position = Position.Absolute;
            cinematicCard.style.left = new Length(7f, LengthUnit.Percent);
            cinematicCard.style.right = new Length(7f, LengthUnit.Percent);
            cinematicCard.style.bottom = new Length(12f, LengthUnit.Percent);
            cinematicCard.style.maxWidth = 980;
            cinematicCard.style.alignSelf = Align.Center;
            cinematicCard.style.paddingLeft = 24;
            cinematicCard.style.paddingRight = 24;
            cinematicCard.style.paddingTop = 18;
            cinematicCard.style.paddingBottom = 16;
            cinematicCard.style.backgroundColor = new Color(0.012f, 0.035f, 0.039f, 0.92f);
            cinematicCard.style.borderLeftWidth = 2;
            cinematicCard.style.borderLeftColor = Accent;
            cinematicCard.style.opacity = 0f;
            cinematicOverlay.Add(cinematicCard);

            cinematicEyebrow = new Label("FIELD RECORD  /  GREYHAVEN");
            cinematicEyebrow.style.color = Accent;
            cinematicEyebrow.style.fontSize = 10;
            cinematicEyebrow.style.unityFontStyleAndWeight = FontStyle.Bold;
            cinematicCard.Add(cinematicEyebrow);

            cinematicTitle = new Label("");
            cinematicTitle.style.color = Paper;
            cinematicTitle.style.fontSize = 26;
            cinematicTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            cinematicTitle.style.whiteSpace = WhiteSpace.Normal;
            cinematicTitle.style.marginTop = 5;
            cinematicTitle.style.marginBottom = 8;
            cinematicCard.Add(cinematicTitle);

            cinematicCaption = new Label("");
            cinematicCaption.style.color = new Color(0.78f, 0.82f, 0.77f, 1f);
            cinematicCaption.style.fontSize = 14;
            cinematicCaption.style.whiteSpace = WhiteSpace.Normal;
            cinematicCaption.style.maxWidth = 760;
            cinematicCaption.style.minHeight = 42;
            cinematicCard.Add(cinematicCaption);

            cinematicBeatCounter = new Label("RECORD 01 / 04");
            cinematicBeatCounter.style.color = Muted;
            cinematicBeatCounter.style.fontSize = 9;
            cinematicBeatCounter.style.marginTop = 10;
            cinematicCard.Add(cinematicBeatCounter);

            cinematicProgressTrack = new VisualElement();
            cinematicProgressTrack.style.height = 2;
            cinematicProgressTrack.style.marginTop = 5;
            cinematicProgressTrack.style.backgroundColor = new Color(0.24f, 0.32f, 0.31f, 1f);
            cinematicCard.Add(cinematicProgressTrack);

            cinematicProgressFill = new VisualElement();
            cinematicProgressFill.style.height = 2;
            cinematicProgressFill.style.width = Length.Percent(0f);
            cinematicProgressFill.style.backgroundColor = Accent;
            cinematicProgressTrack.Add(cinematicProgressFill);

            VisualElement actionRow = new VisualElement();
            actionRow.style.flexDirection = FlexDirection.Row;
            actionRow.style.justifyContent = Justify.FlexEnd;
            actionRow.style.marginTop = 12;
            cinematicCard.Add(actionRow);

            Button skip = new Button(() => skipCinematic = true) { text = "SKIP  /  CONTINUE" };
            skip.style.height = 28;
            skip.style.paddingLeft = 12;
            skip.style.paddingRight = 12;
            skip.style.backgroundColor = Panel;
            skip.style.color = Paper;
            skip.style.borderTopColor = new Color(0.31f, 0.43f, 0.40f, 1f);
            skip.style.borderBottomColor = new Color(0.31f, 0.43f, 0.40f, 1f);
            actionRow.Add(skip);

            root.Add(cinematicOverlay);
        }

        private IEnumerator PlayOpeningCinematic()
        {
            yield return StartCoroutine(RunCinematic(
                BuildOpeningBeats(),
                null,
                false,
                () =>
                {
                    PlayerPrefs.SetInt("wse.openingCinematicSeen", 1);
                    PlayerPrefs.Save();
                    OpenTitle();
                }));
        }

        private static CinematicBeat[] BuildOpeningBeats()
        {
            return new[]
            {
                new CinematicBeat(
                    "ARCHIVE 08  /  ORISON SURVEY",
                    "THE SECOND RETURN",
                    "The first sounding found the seabed. The second came back eleven seconds late.",
                    new Vector3(0f, 1f, 0f), new Vector3(0f, 48f, -36f), 3.4f, 1.12f),
                new CinematicBeat(
                    "RADIO TRANSCRIPT  /  INCOMPLETE",
                    "A VOICE ON THE RADIO",
                    "A familiar voice crossed an empty channel. The crew stopped answering.",
                    new Vector3(12f, 1f, -12f), new Vector3(0f, 42f, -32f), 3.2f, 1.2f),
                new CinematicBeat(
                    "LAST KNOWN POSITION  /  UNCONFIRMED",
                    "THE ORISON WAS LOST",
                    "One apprentice was recovered. The vessel and the rest of its crew were never found.",
                    new Vector3(-16f, 1f, -28f), new Vector3(0f, 40f, -30f), 3.2f, 1.16f),
                new CinematicBeat(
                    "PRESENT DAY  /  GREYHAVEN",
                    "THE COAST IS WAITING",
                    "Eight years later, a new hand takes work on an old boat. There is a harbour to learn and a living to make.",
                    new Vector3(0f, 1f, 12f), new Vector3(0f, 46f, -35f), 3.6f, 1.08f)
            };
        }

        private static CinematicBeat[] BuildFirstVoyageBeats()
        {
            return new[]
            {
                new CinematicBeat(
                    "GREYHAVEN HARBOUR LOG",
                    "THREE DEPARTURES",
                    "Three boats passed beyond the outer marker. Only one came back.",
                    new Vector3(-10f, 1f, -8f), new Vector3(0f, 42f, -31f), 2.8f, 1.12f),
                new CinematicBeat(
                    "LIGHTHOUSE WATCH  /  ELEVEN YEARS",
                    "A SILENT BELL",
                    "The returned skipper kept asking whether the lighthouse bell was ringing. It had been silent for eleven years.",
                    new Vector3(20f, 1f, -34f), new Vector3(0f, 47f, -35f), 3f, 1.08f),
                new CinematicBeat(
                    "HARBOURMASTER'S INSTRUCTION",
                    "THE FIRST JOB",
                    "Recover the sounding buoy intact. Do not switch it on.",
                    new Vector3(0f, 1f, -15f), new Vector3(0f, 40f, -30f), 3f, 1.18f),
                new CinematicBeat(
                    "FIELD NOTE  /  DAY ONE",
                    "LEARN THE WATER",
                    "Keep the old boat running. Learn the channel. Bring back what the sea gives you.",
                    new Vector3(0f, 0f, 6f), new Vector3(0f, 45f, -34f), 2.8f, 1.1f)
            };
        }

        public bool TryPlayFirstContact(NpcAgent npc, Action afterSequence)
        {
            if (npc == null || cinematicActive ||
                !CinematicSequencePolicy.TryRegisterFirstContact(firstContactNpcIds, npc.NpcId))
                return false;

            string title = npc.DisplayName.ToUpperInvariant();
            string eyebrow = "FIRST CONTACT  /  GREYHAVEN";
            string caption = ResolveFirstContactCaption(npc);
            CinematicBeat beat = new CinematicBeat(
                eyebrow,
                title,
                caption,
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 5f, -6f),
                2.65f,
                0.12f);
            StartCoroutine(RunCinematic(new[] { beat }, npc.transform, true, afterSequence));
            return true;
        }

        private static string ResolveFirstContactCaption(NpcAgent npc)
        {
            string identity = (npc.NpcId + " " + npc.OccupationId).ToLowerInvariant();
            if (identity.Contains("harbourmaster"))
                return "The Harbourmaster keeps the channel clear—and remembers the boats that do not return.";
            if (identity.Contains("mara") || identity.Contains("fishmonger") || identity.Contains("market"))
                return "Mara Venn can read a catch by its weight, colour and the marks left by a rough landing.";
            if (identity.Contains("tomas") || identity.Contains("mechanic") || identity.Contains("repair"))
                return "Tomas Vale keeps Greyhaven's engines alive and listens for faults others overlook.";
            if (identity.Contains("inez") || identity.Contains("net_mender") || identity.Contains("net mender"))
                return "Inez Sarmiento knows every knot, worn line and empty net that returns to shore.";
            return "Another life tied to this harbour. Listen carefully; everyone remembers the sea differently.";
        }

        private IEnumerator RunCinematic(
            CinematicBeat[] beats,
            Transform focusOverride,
            bool resumeAfter,
            Action onComplete)
        {
            if (cinematicActive || beats == null || beats.Length == 0)
            {
                if (onComplete != null) onComplete();
                yield break;
            }

            cinematicActive = true;
            skipCinematic = false;
            reduceCinematicMotion = PlayerPrefs.GetInt("wse.reduceCinematicMotion", 0) == 1;
            Pause();
            overlay.style.display = DisplayStyle.None;
            cinematicOverlay.style.display = DisplayStyle.Flex;
            cinematicOverlay.style.opacity = 0f;
            cinematicCard.style.opacity = 0f;

            cinematicCameraFollow = FindFirstObjectByType<CameraFollow2_5D>();
            cinematicCamera = cinematicCameraFollow != null
                ? cinematicCameraFollow.GetComponent<Camera>()
                : Camera.main;
            if (cinematicCamera == null)
                cinematicCamera = FindFirstObjectByType<Camera>();

            Transform focusAnchor = focusOverride != null
                ? focusOverride
                : cinematicCameraFollow != null ? cinematicCameraFollow.Target : null;

            hasCameraSnapshot = cinematicCamera != null;
            if (hasCameraSnapshot)
            {
                Transform cameraTransform = cinematicCamera.transform;
                cameraSnapshotPosition = cameraTransform.position;
                cameraSnapshotRotation = cameraTransform.rotation;
                cameraSnapshotOrthographicSize = cinematicCamera.orthographicSize;
                cameraSnapshotFieldOfView = cinematicCamera.fieldOfView;
            }

            cameraFollowWasEnabled = cinematicCameraFollow != null && cinematicCameraFollow.enabled;
            if (cinematicCameraFollow != null)
                cinematicCameraFollow.enabled = false;

            yield return StartCoroutine(FadeCinematicOverlay(0f, 1f, 0.55f));

            for (int beatIndex = 0; beatIndex < beats.Length && !skipCinematic; beatIndex++)
            {
                CinematicBeat beat = beats[beatIndex];
                cinematicEyebrow.text = beat.Eyebrow;
                cinematicTitle.text = beat.Title;
                cinematicCaption.text = string.Empty;
                cinematicBeatCounter.text = "RECORD " + (beatIndex + 1).ToString("00") +
                    " / " + beats.Length.ToString("00");
                cinematicProgressFill.style.width = Length.Percent(
                    ((beatIndex + 1f) / beats.Length) * 100f);
                cinematicCard.style.opacity = 0f;

                Vector3 startPosition = cinematicCamera != null
                    ? cinematicCamera.transform.position
                    : Vector3.zero;
                Quaternion startRotation = cinematicCamera != null
                    ? cinematicCamera.transform.rotation
                    : Quaternion.identity;
                Vector3 focusPoint = focusAnchor != null
                    ? focusAnchor.position + beat.FocusOffset
                    : Vector3.zero;
                Vector3 destination = focusAnchor != null
                    ? focusPoint + beat.CameraOffset
                    : startPosition;
                Quaternion destinationRotation = focusAnchor != null
                    ? Quaternion.LookRotation(focusPoint - destination, Vector3.up)
                    : startRotation;

                float initialZoom = cinematicCamera != null
                    ? (cinematicCamera.orthographic ? cameraSnapshotOrthographicSize : cameraSnapshotFieldOfView)
                    : 0f;
                float targetZoom = initialZoom * beat.FrameScale;
                if (reduceCinematicMotion) targetZoom = initialZoom;
                float elapsed = 0f;
                float duration = Mathf.Max(0.5f, beat.Duration);

                while (elapsed < duration && !skipCinematic)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    float eased = reduceCinematicMotion
                        ? 1f
                        : progress * progress * (3f - 2f * progress);

                    if (cinematicCamera != null && focusAnchor != null)
                    {
                        cinematicCamera.transform.position = Vector3.Lerp(
                            startPosition, reduceCinematicMotion ? startPosition : destination, eased);
                        cinematicCamera.transform.rotation = Quaternion.Slerp(
                            startRotation, reduceCinematicMotion ? startRotation : destinationRotation, eased);
                        if (cinematicCamera.orthographic)
                            cinematicCamera.orthographicSize = Mathf.Lerp(initialZoom, targetZoom, eased);
                        else
                            cinematicCamera.fieldOfView = Mathf.Lerp(
                                initialZoom, Mathf.Clamp(targetZoom, 25f, 80f), eased);
                    }

                    int visibleCharacters = reduceCinematicMotion
                        ? beat.Caption.Length
                        : Mathf.Clamp(Mathf.FloorToInt(elapsed * 34f), 0, beat.Caption.Length);
                    if (cinematicCaption.text.Length != visibleCharacters)
                        cinematicCaption.text = beat.Caption.Substring(0, visibleCharacters);

                    float fadeIn = Mathf.Clamp01(elapsed / (reduceCinematicMotion ? 0.01f : 0.38f));
                    float fadeOut = Mathf.Clamp01((duration - elapsed) / 0.32f);
                    cinematicCard.style.opacity = Mathf.Min(fadeIn, fadeOut);
                    yield return null;
                }

                cinematicCard.style.opacity = 0f;
            }

            cinematicCard.style.opacity = 0f;
            yield return StartCoroutine(FadeCinematicOverlay(1f, 0f, reduceCinematicMotion ? 0.08f : 0.42f));
            RestoreCinematicCamera();
            cinematicOverlay.style.display = DisplayStyle.None;
            cinematicActive = false;
            skipCinematic = false;

            if (resumeAfter)
                Unpause();

            if (onComplete != null)
                onComplete();
        }

        private IEnumerator FadeCinematicOverlay(float from, float to, float duration)
        {
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, duration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cinematicOverlay.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / safeDuration));
                yield return null;
            }
            cinematicOverlay.style.opacity = to;
        }

        private void RestoreCinematicCamera()
        {
            if (hasCameraSnapshot && cinematicCamera != null)
            {
                cinematicCamera.transform.position = cameraSnapshotPosition;
                cinematicCamera.transform.rotation = cameraSnapshotRotation;
                if (cinematicCamera.orthographic)
                    cinematicCamera.orthographicSize = cameraSnapshotOrthographicSize;
                else
                    cinematicCamera.fieldOfView = cameraSnapshotFieldOfView;
            }

            if (cinematicCameraFollow != null)
                cinematicCameraFollow.enabled = cameraFollowWasEnabled;

            hasCameraSnapshot = false;
            cinematicCamera = null;
            cinematicCameraFollow = null;
        }

        private struct CinematicBeat
        {
            public readonly string Eyebrow;
            public readonly string Title;
            public readonly string Caption;
            public readonly Vector3 FocusOffset;
            public readonly Vector3 CameraOffset;
            public readonly float Duration;
            public readonly float FrameScale;

            public CinematicBeat(
                string eyebrow,
                string title,
                string caption,
                Vector3 focusOffset,
                Vector3 cameraOffset,
                float duration,
                float frameScale)
            {
                Eyebrow = eyebrow;
                Title = title;
                Caption = caption;
                FocusOffset = focusOffset;
                CameraOffset = cameraOffset;
                Duration = duration;
                FrameScale = frameScale;
            }
        }

        private void OpenTitle()
        {
            page = "title"; Pause(); Show(); content.Clear();
            Heading("THE COAST IS WAITING.");
            Body("A quiet harbour. A missing fisherman. A sea that does not always return what it takes.");
            AddButton("BEGIN / CONTINUE", Continue, true);
            AddButton("REPLAY OPENING RECORD", () => StartCoroutine(PlayOpeningCinematic()));
            AddButton("LOAD FIELD RECORD", OpenSaves);
            AddButton("SETTINGS", OpenSettings);
            AddButton("QUIT TO DESKTOP", Application.Quit);
            Hint("Continue restores the latest manual record or separate autosave. If none exists, begin a new voyage.");
            feedback.text = "Your field records remain stored on this device.";
        }

        private void OpenPause()
        {
            page = "pause"; Pause(); Show(); content.Clear();
            Heading("FIELD PAUSED"); Body("Take a moment. The coast will still be here.");
            AddButton("RESUME VOYAGE", Resume, true);
            AddButton("SAVE / LOAD FIELD RECORD", OpenSaves);
            AddButton("SETTINGS", OpenSettings);
            AddButton("RETURN TO TITLE", OpenTitle);
            feedback.text = "The vessel and world simulation are paused.";
        }

        private void OpenSaves()
        {
            page = "saves"; Pause(); Show(); content.Clear();
            Heading("FIELD RECORDS");
            Body("Five manual slots. Autosave is separate. Cross-scene restore is not yet available.");
            List<SaveSlotInfo> slots = saves != null ? saves.GetSlots() : new List<SaveSlotInfo>();
            for (int i = 0; i < SaveFileService.MaximumSlot; i++)
            {
                int n = i + 1; SaveSlotInfo slot = i < slots.Count ? slots[i] : null;
                VisualElement row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center; row.style.backgroundColor = Panel;
                row.style.paddingLeft = 8; row.style.paddingRight = 5; row.style.paddingTop = 6;
                row.style.paddingBottom = 6; row.style.marginBottom = 5; content.Add(row);
                VisualElement info = new VisualElement(); info.style.flexGrow = 1; row.Add(info);
                Label label = new Label("RECORD " + n.ToString("00"));
                label.style.color = Paper; label.style.fontSize = 12; info.Add(label);
                Label detail = new Label(slot != null && slot.IsOccupied ? SlotDetails(slot) : "EMPTY / NO FIELD RECORD");
                detail.style.color = Muted; detail.style.fontSize = 9; detail.style.whiteSpace = WhiteSpace.Normal; info.Add(detail);
                if (slot != null && slot.IsOccupied)
                {
                    SmallButton(row,"LOAD",()=>Run(saves.LoadSlot(n),"Record restored."));
                    SmallButton(row,"SAVE",()=>Run(saves.SaveSlot(n),"Record overwritten."));
                    SmallButton(row,"DELETE",()=>ConfirmDelete(n));
                }
                else SmallButton(row,"SAVE HERE",()=>Run(saves != null ? saves.SaveSlot(n) : SaveOperationResult.Failed("Save service unavailable."),"Record saved."));
            }
            AddButton("SAVE TO NEXT EMPTY SLOT", SaveNext);
            AddButton("BACK", Back);
            feedback.text = "Save and load actions report success or failure here.";
        }

        private void OpenSettings()
        {
            page = "settings"; Pause(); Show(); content.Clear();
            Heading("VESSEL & DISPLAY SETTINGS");
            Body("Preferences are stored locally and remain separate from campaign saves.");
            Section("DISPLAY");
            AddToggle("Fullscreen",Screen.fullScreen,v=>{Screen.fullScreen=v;PlayerPrefs.SetInt("wse.fullscreen",v?1:0);});
            AddSlider("Interface scale",.85f,1.25f,PlayerPrefs.GetFloat("wse.uiScale",1f),v=>{root.style.scale=new Scale(new Vector2(v,v));PlayerPrefs.SetFloat("wse.uiScale",v);});
            Section("AUDIO");
            AddSlider("Master volume",0f,1f,PlayerPrefs.GetFloat("wse.masterVolume",1f),v=>{AudioListener.volume=v;PlayerPrefs.SetFloat("wse.masterVolume",v);});
            AddSlider("Music preference",0f,1f,PlayerPrefs.GetFloat("wse.musicVolume",.8f),v=>PlayerPrefs.SetFloat("wse.musicVolume",v));
            AddSlider("Effects preference",0f,1f,PlayerPrefs.GetFloat("wse.effectsVolume",1f),v=>PlayerPrefs.SetFloat("wse.effectsVolume",v));
            Section("ACCESSIBILITY");
            AddToggle("Reduce camera shake (preference)",PlayerPrefs.GetInt("wse.reduceCameraShake",0)==1,v=>PlayerPrefs.SetInt("wse.reduceCameraShake",v?1:0));
            AddToggle("Reduce cinematic camera movement",PlayerPrefs.GetInt("wse.reduceCinematicMotion",0)==1,v=>PlayerPrefs.SetInt("wse.reduceCinematicMotion",v?1:0));
            AddToggle("High contrast (preference)",PlayerPrefs.GetInt("wse.highContrast",0)==1,v=>PlayerPrefs.SetInt("wse.highContrast",v?1:0));
            AddButton("RESTORE DEFAULTS",ResetSettings); AddButton("BACK",Back);
            feedback.text = "Only master volume, fullscreen, and interface scale currently have direct runtime effects.";
        }

        private void Continue()
        {
            if (saves == null)
            {
                StartNewVoyage();
                return;
            }

            List<SaveSlotInfo> slots = saves.GetSlots();
            SaveSlotInfo newest = null;
            DateTime latest = DateTime.MinValue;
            foreach (SaveSlotInfo slot in slots)
            {
                DateTime parsed;
                if (slot != null && slot.IsOccupied &&
                    DateTime.TryParse(slot.SavedAtUtc, out parsed) && parsed > latest)
                {
                    newest = slot;
                    latest = parsed;
                }
            }

            if (newest != null)
            {
                SaveOperationResult manual = saves.LoadSlot(newest.SlotNumber);
                if (manual.Success)
                {
                    feedback.text = manual.Message;
                    firstLaunch = false;
                    Resume();
                }
                else
                {
                    feedback.text = "LOAD FAILED: " + manual.Message;
                }
                return;
            }

            SaveOperationResult autosave = saves.LoadAutosave();
            if (autosave.Success)
            {
                feedback.text = autosave.Message;
                firstLaunch = false;
                Resume();
                return;
            }

            StartNewVoyage();
        }

        private void StartNewVoyage()
        {
            if (cinematicActive) return;
            StartCoroutine(RunCinematic(
                BuildFirstVoyageBeats(),
                null,
                false,
                () =>
                {
                    firstLaunch = false;
                    Resume();
                }));
        }

        private void SaveNext()
        {
            if(saves==null){feedback.text="Save service unavailable.";return;}
            List<SaveSlotInfo> slots=saves.GetSlots();
            for(int i=0;i<slots.Count;i++)if(!slots[i].IsOccupied){Run(saves.SaveSlot(i+1),"Record saved.");return;}
            feedback.text="All five slots are occupied. Choose SAVE on a record to overwrite it.";
        }

        private void ConfirmDelete(int slot)
        {
            page="confirm";content.Clear();Heading("DELETE RECORD "+slot.ToString("00")+"?");
            Body("This permanently removes the save and its recovery backup. This cannot be undone.");
            AddButton("YES, DELETE RECORD",()=>{SaveOperationResult r=saves.DeleteSlot(slot);OpenSaves();feedback.text=r.Message;},true);
            AddButton("CANCEL",OpenSaves);
        }

        private void ResetSettings()
        {
            foreach(string key in new[]{"wse.fullscreen","wse.uiScale","wse.masterVolume","wse.musicVolume","wse.effectsVolume","wse.reduceCameraShake","wse.highContrast"})PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();Screen.fullScreen=true;AudioListener.volume=1f;root.style.scale=new Scale(Vector2.one);
            OpenSettings();feedback.text="Default preferences restored.";
        }

        private void Run(SaveOperationResult result,string success)
        {
            feedback.text=result.Success?success+" "+result.Message:"ACTION FAILED: "+result.Message;
            if(result.Success&&result.RecoveredFromBackup)feedback.text+=" Recovery backup was used.";
            if(result.Success&&page=="saves"){OpenSaves();feedback.text=success+" "+result.Message+(result.RecoveredFromBackup?" Recovery backup was used.":"");}
        }

        private void Back(){if(page=="confirm")OpenSaves();else if(firstLaunch)OpenTitle();else OpenPause();}
        private void Resume(){firstLaunch=false;page="game";overlay.style.display=DisplayStyle.None;Unpause();}
        private void Pause(){if(paused)return;previousTimeScale=Time.timeScale;Time.timeScale=0f;paused=true;}
        private void Unpause(){if(!paused)return;Time.timeScale=previousTimeScale>0f?previousTimeScale:1f;paused=false;}
        private void Show(){overlay.style.display=DisplayStyle.Flex;}

        private static string SlotDetails(SaveSlotInfo s)
        {
            DateTime date;string stamp=DateTime.TryParse(s.SavedAtUtc,out date)?date.ToLocalTime().ToString("yyyy-MM-dd HH:mm"):"DATE UNKNOWN";
            TimeSpan play=TimeSpan.FromSeconds(Math.Max(0,s.TotalPlaytimeSeconds));
            return (string.IsNullOrEmpty(s.SceneId)?"UNKNOWN AREA":s.SceneId)+" · "+stamp+" · "+play.ToString(@"hh\:mm")+(s.RecoveredFromBackup?" · BACKUP":"");
        }
        private void Heading(string t){Label l=new Label(t);l.style.color=Paper;l.style.fontSize=19;l.style.unityFontStyleAndWeight=FontStyle.Bold;l.style.marginBottom=6;content.Add(l);}
        private void Body(string t){Label l=new Label(t);l.style.color=Muted;l.style.fontSize=11;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginBottom=12;content.Add(l);}
        private void Hint(string t){Label l=new Label(t);l.style.color=Muted;l.style.fontSize=9;l.style.whiteSpace=WhiteSpace.Normal;l.style.marginTop=8;content.Add(l);}
        private void Section(string t){Label l=new Label(t);l.style.color=Accent;l.style.fontSize=10;l.style.marginTop=7;content.Add(l);}
        private void AddButton(string t,Action a,bool primary=false){Button b=new Button(a){text=t};b.style.height=34;b.style.marginBottom=5;b.style.paddingLeft=11;b.style.backgroundColor=primary?Accent:Panel;b.style.color=Paper;b.style.unityTextAlign=TextAnchor.MiddleLeft;content.Add(b);}
        private void SmallButton(VisualElement p,string t,Action a){Button b=new Button(a){text=t};b.style.minWidth=48;b.style.height=26;b.style.marginLeft=3;b.style.fontSize=9;b.style.backgroundColor=Panel;b.style.color=Paper;p.Add(b);}
        private void AddToggle(string t,bool initial,Action<bool> changed){Toggle x=new Toggle(t){value=initial};x.style.color=Paper;x.RegisterValueChangedCallback(e=>changed(e.newValue));content.Add(x);}
        private void AddSlider(string t,float min,float max,float initial,Action<float> changed){VisualElement row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.alignItems=Align.Center;content.Add(row);Label l=new Label(t);l.style.color=Paper;l.style.width=155;l.style.fontSize=10;row.Add(l);Slider s=new Slider(min,max){value=Mathf.Clamp(initial,min,max)};s.style.flexGrow=1;s.RegisterValueChangedCallback(e=>changed(e.newValue));row.Add(s);}
    }
}

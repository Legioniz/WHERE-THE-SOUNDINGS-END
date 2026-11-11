using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WhereTheSoundingsEnd.Boat;
using Yarn.Unity;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Narrative
{
    /// <summary>
    /// Starts the configured resident conversation when the player vessel enters the interaction radius.
    /// The player is still in the boat in this Greyhaven prototype; a walkable character is later work.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class HarbourmasterInteraction : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private GreyhavenRuntimeUI runtimeUI;
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private string dialogueNode = "Greyhaven_Harbourmaster";
        [SerializeField] private string displayName = "Harbourmaster";

        private readonly HashSet<Collider> _vesselColliders = new HashSet<Collider>();
        private InputAction _interactAction;
        private bool _ownsInteractAction;
        private SeaStateDirector _seaState;

        public DialogueRunner Runner { get => dialogueRunner; set => dialogueRunner = value; }
        public GreyhavenRuntimeUI Interface { get => runtimeUI; set => runtimeUI = value; }
        public InputActionAsset Controls { get => controls; set => controls = value; }
        public string DialogueNode { get => dialogueNode; set => dialogueNode = value; }
        public string DisplayName { get => displayName; set => displayName = value; }

        private void Awake()
        {
            SphereCollider trigger = GetComponent<SphereCollider>();
            trigger.isTrigger = true;
            _seaState = FindFirstObjectByType<SeaStateDirector>();
        }

        private void Start()
        {
            if (controls == null) return;
            InputActionMap map = controls.FindActionMap("Boat", false);
            _interactAction = map?.FindAction("Interact", false);
            if (_interactAction != null && !_interactAction.enabled)
            {
                _interactAction.Enable();
                _ownsInteractAction = true;
            }
        }

        private void OnDestroy()
        {
            if (_ownsInteractAction && _interactAction != null)
                _interactAction.Disable();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || other.GetComponentInParent<BoatPrototypeController>() == null)
                return;

            _vesselColliders.Add(other);
            UpdatePrompt();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null) return;
            _vesselColliders.Remove(other);
            UpdatePrompt();
        }

        private void Update()
        {
            if (_vesselColliders.Count == 0 || runtimeUI == null)
                return;

            if (runtimeUI.IsInventoryOpen ||
                (dialogueRunner != null && dialogueRunner.IsDialogueRunning) ||
                (_seaState != null && (_seaState.IsHarvesting || _seaState.IsSurveying)))
            {
                runtimeUI.SetInteractionPrompt(string.Empty);
                return;
            }

            UpdatePrompt();
            if (dialogueRunner == null || _interactAction == null)
                return;

            if (_interactAction.WasPressedThisFrame())
            {
                runtimeUI.SetInteractionPrompt(string.Empty);
                runtimeUI.BeginDialogue(dialogueNode);
            }
        }

        private void UpdatePrompt()
        {
            if (runtimeUI == null) return;
            runtimeUI.SetInteractionPrompt(_vesselColliders.Count > 0
                ? "E  /  SPEAK WITH " + displayName.ToUpperInvariant()
                : string.Empty);
        }
    }
}

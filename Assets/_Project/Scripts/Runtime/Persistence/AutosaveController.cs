using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhereTheSoundingsEnd.Persistence
{
    /// <summary>
    /// Runs time-based and lifecycle autosaves into a dedicated file.
    /// It never writes to the five player-managed manual save slots.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SaveGameCoordinator))]
    public sealed class AutosaveController : MonoBehaviour
    {
        [Header("Schedule")]
        [SerializeField, Min(30f)] private float intervalSeconds = 300f;
        [SerializeField, Min(0f)] private float initialDelaySeconds = 45f;
        [SerializeField, Min(1f)] private float retryDelaySeconds = 30f;
        [SerializeField, Min(0f)] private float lifecycleCooldownSeconds = 15f;

        [Header("Lifecycle triggers")]
        [SerializeField] private bool saveOnApplicationPause = true;
        [SerializeField] private bool saveOnFocusLost = true;
        [SerializeField] private bool saveOnSceneUnload = true;
        [SerializeField] private bool saveOnApplicationQuit = true;

        [Header("Diagnostics")]
        [SerializeField] private bool logSuccessfulAutosaves;

        private SaveGameCoordinator _coordinator;
        private double _elapsedRealtime;
        private double _nextAttemptAt;
        private double _lastAttemptAt = double.NegativeInfinity;
        private bool _isSaving;

        public event Action<string, SaveOperationResult> AutosaveCompleted;

        public float IntervalSeconds
        {
            get { return intervalSeconds; }
            set { intervalSeconds = Mathf.Max(30f, value); }
        }

        public bool IsSaving { get { return _isSaving; } }

        private void Awake()
        {
            _coordinator = GetComponent<SaveGameCoordinator>();
        }

        private void OnEnable()
        {
            _elapsedRealtime = 0.0;
            _lastAttemptAt = double.NegativeInfinity;
            _nextAttemptAt = Mathf.Max(0f, initialDelaySeconds);
            SceneManager.sceneUnloading += HandleSceneUnloading;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloading -= HandleSceneUnloading;
        }

        private void Update()
        {
            _elapsedRealtime += Time.unscaledDeltaTime;
            if (_elapsedRealtime >= _nextAttemptAt)
                AttemptAutosave("periodic", false);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && saveOnApplicationPause) AttemptAutosave("application_pause", false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && saveOnFocusLost) AttemptAutosave("focus_lost", false);
        }

        private void OnApplicationQuit()
        {
            if (saveOnApplicationQuit) AttemptAutosave("application_quit", true);
        }

        private void HandleSceneUnloading(Scene scene)
        {
            if (!saveOnSceneUnload) return;

            Scene activeScene = SceneManager.GetActiveScene();
            if (scene.handle == activeScene.handle && scene.handle == gameObject.scene.handle)
                AttemptAutosave("scene_unload", false);
        }

        /// <summary>
        /// Call at authored checkpoints, such as quest milestones or a safe-harbour return.
        /// </summary>
        public SaveOperationResult RequestCheckpointAutosave(string checkpointId)
        {
            string id = string.IsNullOrWhiteSpace(checkpointId) ? "unnamed" : checkpointId;
            return AttemptAutosave("checkpoint:" + id, true);
        }

        private SaveOperationResult AttemptAutosave(string reason, bool force)
        {
            if (_isSaving)
                return SaveOperationResult.Failed("An autosave is already in progress.");

            if (_coordinator == null) _coordinator = GetComponent<SaveGameCoordinator>();
            if (_coordinator == null)
                return SaveOperationResult.Failed("Autosave requires a SaveGameCoordinator on the same GameObject.");

            if (!force && _elapsedRealtime - _lastAttemptAt < lifecycleCooldownSeconds)
            {
                _nextAttemptAt = Math.Max(_nextAttemptAt, _lastAttemptAt + lifecycleCooldownSeconds);
                return SaveOperationResult.Failed("Autosave skipped because a recent save already ran.");
            }

            _isSaving = true;
            _lastAttemptAt = _elapsedRealtime;
            SaveOperationResult result;

            try
            {
                result = _coordinator.SaveAutosave(reason);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                result = SaveOperationResult.Failed("Autosave threw an exception: " + exception.Message);
            }
            finally
            {
                _isSaving = false;
            }

            if (result.Success)
            {
                _nextAttemptAt = _elapsedRealtime + Mathf.Max(30f, intervalSeconds);
                if (logSuccessfulAutosaves)
                    Debug.Log("Autosave complete (" + reason + "). " + result.Message, this);
            }
            else
            {
                _nextAttemptAt = _elapsedRealtime + Mathf.Max(1f, retryDelaySeconds);
                Debug.LogWarning("Autosave failed (" + reason + "). " + result.Message, this);
            }

            AutosaveCompleted?.Invoke(reason, result);
            return result;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhereTheSoundingsEnd.World;

namespace WhereTheSoundingsEnd.Streaming
{
    [Serializable]
    public sealed class StreamedAreaDefinition
    {
        [SerializeField] private string areaId = string.Empty;
        [SerializeField] private string scenePath = string.Empty;
        [SerializeField] private string contentRevision = "1";
        [SerializeField] private Vector3 center;
        [SerializeField, Min(1f)] private float preloadRadius = 80f;
        [SerializeField, Min(1f)] private float unloadRadius = 110f;
        [SerializeField, Min(0f)] private float unloadGraceSeconds = 4f;
        [SerializeField] private bool loadAtStartup;

        public string AreaId => areaId;
        public string ScenePath => scenePath;
        public string ContentRevision => contentRevision;
        public Vector3 Center => center;
        public float PreloadRadius => preloadRadius;
        public float UnloadRadius => unloadRadius;
        public float UnloadGraceSeconds => unloadGraceSeconds;
        public bool LoadAtStartup => loadAtStartup;

        public StreamedAreaDefinition(
            string stableAreaId,
            string additiveScenePath,
            string revision,
            Vector3 areaCenter,
            float preloadDistance,
            float unloadDistance,
            float unloadGrace,
            bool preloadAtStartup)
        {
            areaId = stableAreaId;
            scenePath = additiveScenePath;
            contentRevision = revision;
            center = areaCenter;
            preloadRadius = preloadDistance;
            unloadRadius = unloadDistance;
            unloadGraceSeconds = unloadGrace;
            loadAtStartup = preloadAtStartup;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(areaId) &&
                   !string.IsNullOrWhiteSpace(scenePath) &&
                   !string.IsNullOrWhiteSpace(contentRevision) &&
                   AreaStreamingPolicy.IsValidRadii(preloadRadius, unloadRadius) &&
                   !float.IsNaN(unloadGraceSeconds) &&
                   !float.IsInfinity(unloadGraceSeconds) &&
                   unloadGraceSeconds >= 0f &&
                   IsFinite(center.x) && IsFinite(center.y) && IsFinite(center.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Preloads static area scenes before the player reaches them, activates their validated
    /// roots without a loading overlay, and unloads distant areas after a grace period.
    /// Gameplay controllers, NPCs, save participants, the player, water simulation and camera
    /// stay in the persistent scene to avoid broken cross-scene references and lost state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AreaStreamingDirector : MonoBehaviour
    {
        [SerializeField] private Transform trackedPlayer;
        [SerializeField] private List<StreamedAreaDefinition> areas =
            new List<StreamedAreaDefinition>();
        [SerializeField, Range(1, 3)] private int maximumConcurrentLoads = 1;
        [SerializeField, Min(0.05f)] private float distanceCheckInterval = 0.25f;

        private sealed class AreaState
        {
            public StreamedAreaDefinition Definition;
            public Scene Scene;
            public StreamedAreaSceneRoot Root;
            public bool IsLoading;
            public bool IsLoaded;
            public bool IsUnloading;
            public bool PendingLoad;
            public bool Failed;
            public float OutsideSince = -1f;
        }

        private readonly List<AreaState> states = new List<AreaState>();
        private float nextDistanceCheck;
        private int activeLoadCount;

        public int LoadedAreaCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < states.Count; i++)
                    if (states[i].IsLoaded) count++;
                return count;
            }
        }

        public void Configure(Transform player, IList<StreamedAreaDefinition> definitions)
        {
            trackedPlayer = player;
            areas = definitions == null
                ? new List<StreamedAreaDefinition>()
                : new List<StreamedAreaDefinition>(definitions);
        }

        private void Awake()
        {
            states.Clear();
            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < areas.Count; i++)
            {
                StreamedAreaDefinition definition = areas[i];
                if (definition == null || !definition.IsValid())
                {
                    Debug.LogError("Area streaming contains an invalid area definition at index " + i + ".", this);
                    continue;
                }

                if (!seenIds.Add(definition.AreaId))
                {
                    Debug.LogError("Area streaming contains duplicate area ID '" + definition.AreaId + "'.", this);
                    continue;
                }

                states.Add(new AreaState { Definition = definition });
            }
        }

        private void Start()
        {
            if (trackedPlayer == null)
            {
                Rigidbody vessel = FindFirstObjectByType<Rigidbody>();
                if (vessel != null) trackedPlayer = vessel.transform;
            }

            if (trackedPlayer == null)
            {
                Debug.LogError("Area streaming has no tracked player/vessel Transform. No areas will be loaded.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].Definition.LoadAtStartup)
                    RequestLoad(states[i]);
            }

            ProcessLoadQueue();
        }

        private void Update()
        {
            if (trackedPlayer == null) return;

            if (Time.unscaledTime >= nextDistanceCheck)
            {
                nextDistanceCheck = Time.unscaledTime + Mathf.Max(0.05f, distanceCheckInterval);
                EvaluateAreas();
            }

            ProcessLoadQueue();
        }

        private void EvaluateAreas()
        {
            Vector3 playerPosition = trackedPlayer.position;

            for (int i = 0; i < states.Count; i++)
            {
                AreaState state = states[i];
                StreamedAreaDefinition definition = state.Definition;
                float distance = HorizontalDistance(playerPosition, definition.Center);

                if (AreaStreamingPolicy.ShouldPreload(distance, definition.PreloadRadius))
                {
                    state.OutsideSince = -1f;
                    RequestLoad(state);
                    continue;
                }

                if (!state.IsLoaded || state.IsLoading || state.IsUnloading)
                {
                    if (!definition.LoadAtStartup && distance > definition.PreloadRadius)
                        state.PendingLoad = false;
                    continue;
                }

                if (!AreaStreamingPolicy.ShouldUnload(distance, definition.UnloadRadius))
                {
                    state.OutsideSince = -1f;
                    continue;
                }

                if (state.OutsideSince < 0f)
                {
                    state.OutsideSince = Time.unscaledTime;
                    continue;
                }

                if (Time.unscaledTime - state.OutsideSince >= definition.UnloadGraceSeconds)
                    StartCoroutine(UnloadAreaRoutine(state));
            }
        }

        private void RequestLoad(AreaState state)
        {
            if (state == null || state.IsLoaded || state.IsLoading ||
                state.IsUnloading || state.PendingLoad || state.Failed)
                return;

            state.PendingLoad = true;
        }

        private void ProcessLoadQueue()
        {
            int concurrencyLimit = Mathf.Clamp(maximumConcurrentLoads, 1, 3);
            while (activeLoadCount < concurrencyLimit)
            {
                AreaState next = null;
                for (int i = 0; i < states.Count; i++)
                {
                    if (states[i].PendingLoad && !states[i].IsLoading &&
                        !states[i].IsLoaded && !states[i].IsUnloading && !states[i].Failed)
                    {
                        next = states[i];
                        break;
                    }
                }

                if (next == null) return;
                next.PendingLoad = false;
                next.IsLoading = true;
                activeLoadCount++;
                StartCoroutine(LoadAreaRoutine(next));
            }
        }

        private IEnumerator LoadAreaRoutine(AreaState state)
        {
            string scenePath = state.Definition.ScenePath;
            if (!Application.CanStreamedLevelBeLoaded(scenePath))
            {
                FailLoad(state, "Scene '" + scenePath + "' is not included in Build Settings.");
                yield break;
            }

            AsyncOperation operation;
            try
            {
                operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            }
            catch (Exception exception)
            {
                FailLoad(state, "Could not begin loading scene '" + scenePath + "': " + exception.Message);
                yield break;
            }

            if (operation == null)
            {
                FailLoad(state, "Unity returned no load operation for scene '" + scenePath + "'.");
                yield break;
            }

            yield return operation;

            Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                FailLoad(state, "Scene '" + scenePath + "' reported completion but is not loaded.");
                yield break;
            }

            GameObject[] roots = loadedScene.GetRootGameObjects();
            StreamedAreaSceneRoot areaRoot = null;
            int rootCount = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                StreamedAreaSceneRoot candidate = roots[i].GetComponent<StreamedAreaSceneRoot>();
                if (candidate == null) continue;
                areaRoot = candidate;
                rootCount++;
            }

            string validationError = string.Empty;
            if (roots.Length != 1 || rootCount != 1 || areaRoot == null ||
                !areaRoot.TryValidate(
                    state.Definition.AreaId,
                    state.Definition.ContentRevision,
                    out validationError))
            {
                if (rootCount != 1)
                    validationError = "Expected one root object with exactly one StreamedAreaSceneRoot; found " +
                                      roots.Length + " root objects and " + rootCount + " area roots.";
                Debug.LogError("Rejected streamed area '" + state.Definition.AreaId + "': " +
                               validationError, this);
                AsyncOperation cleanup = SceneManager.UnloadSceneAsync(loadedScene);
                if (cleanup != null) yield return cleanup;
                state.Failed = true;
                FinishLoad(state);
                yield break;
            }

            state.Scene = loadedScene;
            state.Root = areaRoot;
            state.Root.gameObject.SetActive(true);
            state.IsLoaded = true;
            state.OutsideSince = -1f;
            RefreshWorldSystems();

            if (trackedPlayer != null &&
                HorizontalDistance(trackedPlayer.position, state.Definition.Center) >
                state.Definition.UnloadRadius)
            {
                state.OutsideSince = Time.unscaledTime;
            }

            FinishLoad(state);
        }

        private void FailLoad(AreaState state, string reason)
        {
            Debug.LogError("Area streaming failed for '" + state.Definition.AreaId + "': " + reason, this);
            state.Failed = true;
            FinishLoad(state);
        }

        private void FinishLoad(AreaState state)
        {
            state.IsLoading = false;
            activeLoadCount = Mathf.Max(0, activeLoadCount - 1);
        }

        private IEnumerator UnloadAreaRoutine(AreaState state)
        {
            if (state == null || !state.IsLoaded || state.IsUnloading)
                yield break;

            state.IsUnloading = true;
            if (state.Root != null)
                state.Root.gameObject.SetActive(false);

            RefreshWorldSystems();

            if (state.Scene.IsValid() && state.Scene.isLoaded)
            {
                if (SceneManager.GetActiveScene() == state.Scene)
                    SceneManager.SetActiveScene(gameObject.scene);

                AsyncOperation operation = SceneManager.UnloadSceneAsync(state.Scene);
                if (operation != null) yield return operation;
            }

            state.Scene = default(Scene);
            state.Root = null;
            state.IsLoaded = false;
            state.IsUnloading = false;
            state.OutsideSince = -1f;
        }

        private void RefreshWorldSystems()
        {
            SeaStateDirector seaState = FindFirstObjectByType<SeaStateDirector>();
            if (seaState != null)
                seaState.RefreshSceneTargets();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < states.Count; i++)
            {
                AreaState state = states[i];
                if (!state.IsLoaded || !state.Scene.IsValid() || !state.Scene.isLoaded)
                    continue;

                if (state.Root != null)
                    state.Root.gameObject.SetActive(false);
                if (SceneManager.GetActiveScene() == state.Scene && gameObject.scene.IsValid())
                    SceneManager.SetActiveScene(gameObject.scene);
                SceneManager.UnloadSceneAsync(state.Scene);
            }
        }

        private static float HorizontalDistance(Vector3 first, Vector3 second)
        {
            float x = first.x - second.x;
            float z = first.z - second.z;
            return Mathf.Sqrt(x * x + z * z);
        }
    }
}

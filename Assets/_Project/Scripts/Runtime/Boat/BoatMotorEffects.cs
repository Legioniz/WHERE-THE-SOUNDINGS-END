using UnityEngine;

namespace WhereTheSoundingsEnd.Boat
{
    /// <summary>
    /// Visual-only outboard response. It reads physics telemetry but never applies forces,
    /// changes Rigidbody velocity, or adds colliders to the vessel.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoatPrototypeController))]
    public sealed class BoatMotorEffects : MonoBehaviour
    {
        [Header("Particle limits")]
        [SerializeField, Min(0f)] private float maximumPropWashRate = 32f;
        [SerializeField, Min(0f)] private float maximumSurfaceSprayRate = 38f;
        [SerializeField, Min(0f)] private float maximumPropellerDegreesPerSecond = 5400f;

        [Header("Effect placement")]
        [SerializeField] private Vector3 propWashLocalPosition = new Vector3(0f, -0.58f, -3.18f);
        [SerializeField] private Vector3 surfaceSprayLocalPosition = new Vector3(0f, -0.15f, -2.78f);

        private BoatPrototypeController controller;
        private Transform propellerRotor;
        private Transform engineCowl;
        private Vector3 engineCowlRestPosition;
        private Quaternion engineCowlRestRotation;
        private Quaternion propellerRestRotation;
        private Transform effectsRoot;
        private ParticleSystem propWash;
        private ParticleSystem surfaceSpray;
        private Material particleMaterial;
        private Texture2D particleTexture;
        private float propellerAngle;

        private void Awake()
        {
            controller = GetComponent<BoatPrototypeController>();
            ResolveVisualParts();
            BuildParticleEffects();
        }

        private void OnEnable()
        {
            if (propWash != null) propWash.Play(true);
            if (surfaceSpray != null) surfaceSpray.Play(true);
        }

        private void LateUpdate()
        {
            if (controller == null) return;

            float load = Mathf.Clamp01(controller.CurrentEngineLoad);
            float speed = Mathf.Clamp01(controller.CurrentSpeedRatio);
            float rpm = Mathf.Clamp01(controller.NormalizedEngineRpm);
            float wear = 1f - Mathf.Clamp01(controller.EngineConditionNormalized);

            // Prop wash follows engine demand, including a hard launch from low vessel speed.
            SetEmissionRate(propWash, maximumPropWashRate * load);
            SetStartSpeed(propWash, 1.3f + load * 0.9f, 2.3f + load * 2.3f);

            // Surface wake follows actual horizontal speed, so it persists while coasting.
            float speedWake = Mathf.SmoothStep(0.12f, 0.82f, speed);
            float surfaceIntensity = Mathf.Clamp01(Mathf.Max(speedWake, load * 0.68f));
            SetEmissionRate(surfaceSpray, maximumSurfaceSprayRate * surfaceIntensity);
            SetStartSpeed(surfaceSpray, 1.1f + surfaceIntensity * 0.8f,
                2.0f + surfaceIntensity * 2.0f);

            if (propellerRotor != null)
            {
                propellerAngle = Mathf.Repeat(
                    propellerAngle + maximumPropellerDegreesPerSecond * rpm * Time.deltaTime,
                    360f);
                propellerRotor.localRotation =
                    propellerRestRotation * Quaternion.AngleAxis(propellerAngle, Vector3.forward);
            }

            if (engineCowl != null)
            {
                float vibration = load * (0.55f + wear * 1.2f);
                float phase = Time.time * (18f + rpm * 36f);
                Vector3 offset = new Vector3(
                    Mathf.Sin(phase * 1.07f) * 0.0022f,
                    Mathf.Sin(phase * 1.73f) * 0.0016f,
                    Mathf.Cos(phase * 1.31f) * 0.0020f) * vibration;
                Vector3 angularOffset = new Vector3(
                    Mathf.Sin(phase) * 0.28f,
                    Mathf.Cos(phase * 0.83f) * 0.35f,
                    Mathf.Sin(phase * 1.21f) * 0.18f) * vibration;

                engineCowl.localPosition = engineCowlRestPosition + offset;
                engineCowl.localRotation =
                    engineCowlRestRotation * Quaternion.Euler(angularOffset);
            }
        }

        private void OnDisable()
        {
            if (propWash != null) propWash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (surfaceSpray != null) surfaceSpray.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            RestoreEngineCowl();
        }

        private void OnDestroy()
        {
            if (particleMaterial != null) Destroy(particleMaterial);
            if (particleTexture != null) Destroy(particleTexture);
        }

        private void ResolveVisualParts()
        {
            Transform visualRoot = transform.Find("Boat_Visual_Model");
            if (visualRoot == null) return;

            propellerRotor = visualRoot.Find("Outboard_Propeller_Rotor");
            if (propellerRotor != null)
                propellerRestRotation = propellerRotor.localRotation;

            engineCowl = visualRoot.Find("Outboard_Engine_Cowl");
            if (engineCowl != null)
            {
                engineCowlRestPosition = engineCowl.localPosition;
                engineCowlRestRotation = engineCowl.localRotation;
            }
        }

        private void BuildParticleEffects()
        {
            effectsRoot = new GameObject("Boat_Motor_Effects").transform;
            effectsRoot.SetParent(transform, false);

            particleTexture = CreateSoftParticleTexture();
            particleMaterial = CreateParticleMaterial(particleTexture);

            propWash = CreateEffect(
                "Outboard_Prop_Wash", propWashLocalPosition,
                Quaternion.Euler(-5f, 180f, 0f),
                new Color(0.70f, 0.91f, 0.96f, 0.58f),
                new Vector2(0.10f, 0.22f), new Vector2(0.55f, 1.05f), -0.04f,
                0.10f, 17f);

            surfaceSpray = CreateEffect(
                "Outboard_Surface_Spray", surfaceSprayLocalPosition,
                Quaternion.Euler(-18f, 180f, 0f),
                new Color(0.82f, 0.94f, 1f, 0.52f),
                new Vector2(0.07f, 0.17f), new Vector2(0.24f, 0.62f), 0.42f,
                0.16f, 24f);
        }

        private ParticleSystem CreateEffect(
            string effectName,
            Vector3 localPosition,
            Quaternion localRotation,
            Color startColor,
            Vector2 startSize,
            Vector2 lifetime,
            float gravity,
            float coneRadius,
            float coneAngle)
        {
            GameObject effectObject = new GameObject(effectName);
            effectObject.transform.SetParent(effectsRoot, false);
            effectObject.transform.localPosition = localPosition;
            effectObject.transform.localRotation = localRotation;

            ParticleSystem system = effectObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 160;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(startSize.x, startSize.y);
            main.startColor = startColor;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(gravity);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = coneRadius;
            shape.angle = coneAngle;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.80f, 0f),
                    new GradientAlphaKey(0.42f, 0.68f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

            ParticleSystem.InheritVelocityModule inheritVelocity = system.inheritVelocity;
            inheritVelocity.enabled = true;
            inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
            inheritVelocity.curve = new ParticleSystem.MinMaxCurve(0.85f);

            ParticleSystemRenderer particleRenderer = system.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.sharedMaterial = particleMaterial;
            particleRenderer.sortingFudge = -0.1f;

            system.cullingMode = ParticleSystemCullingMode.PauseAndCatchup;
            system.Play(true);
            return system;
        }

        private static void SetEmissionRate(ParticleSystem system, float rate)
        {
            if (system == null) return;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = Mathf.Max(0f, rate);
        }

        private static void SetStartSpeed(ParticleSystem system, float minimum, float maximum)
        {
            if (system == null) return;
            ParticleSystem.MainModule main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(minimum, maximum);
        }

        private void RestoreEngineCowl()
        {
            if (engineCowl == null) return;
            engineCowl.localPosition = engineCowlRestPosition;
            engineCowl.localRotation = engineCowlRestRotation;
        }

        private static Texture2D CreateSoftParticleTexture()
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "Runtime_Boat_Motor_Soft_Particle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f;
                    float v = (y + 0.5f) / size * 2f - 1f;
                    float radius = Mathf.Sqrt(u * u + v * v);
                    float alpha = 1f - Mathf.SmoothStep(0.28f, 1f, radius);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Material CreateParticleMaterial(Texture2D texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return null;

            Material material = new Material(shader)
            {
                name = "Runtime_Boat_Motor_Particles",
                hideFlags = HideFlags.DontSave
            };
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            material.renderQueue = 3000;
            return material;
        }
    }
}

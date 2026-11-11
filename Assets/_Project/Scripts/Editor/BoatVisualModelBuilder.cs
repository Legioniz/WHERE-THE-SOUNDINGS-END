using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Shared editor-generated visual model for the Greyhaven coastal workboat.
    /// Generated geometry is visual-only; the vessel root remains the physics authority.
    /// </summary>
    public static class BoatVisualModelBuilder
    {
        private const string MaterialsPath = "Assets/_Project/Art/Materials";
        private const string MeshesPath = "Assets/_Project/Art/Meshes";

        private struct HullStation
        {
            public readonly float Z;
            public readonly float HalfBeam;
            public readonly float KeelY;
            public readonly float ChineY;
            public readonly float RailY;

            public HullStation(float z, float halfBeam, float keelY, float chineY, float railY)
            {
                Z = z;
                HalfBeam = halfBeam;
                KeelY = keelY;
                ChineY = chineY;
                RailY = railY;
            }
        }

        // Stern to bow. Positive Z is the vessel's forward axis.
        private static readonly HullStation[] Stations =
        {
            new HullStation(-2.78f, 0.70f, -0.34f, -0.14f, 0.10f),
            new HullStation(-2.48f, 0.86f, -0.40f, -0.20f, 0.12f),
            new HullStation(-1.85f, 0.98f, -0.44f, -0.23f, 0.14f),
            new HullStation(-0.80f, 1.04f, -0.45f, -0.25f, 0.15f),
            new HullStation( 0.45f, 1.00f, -0.43f, -0.23f, 0.18f),
            new HullStation( 1.48f, 0.82f, -0.35f, -0.18f, 0.25f),
            new HullStation( 2.28f, 0.53f, -0.22f, -0.10f, 0.38f),
            new HullStation( 2.76f, 0.025f, -0.055f, -0.025f, 0.53f)
        };

        public static Transform Build(Transform vesselRoot)
        {
            if (vesselRoot == null) throw new ArgumentNullException(nameof(vesselRoot));

            EnsureFolder("Assets/_Project/Art");
            EnsureFolder(MaterialsPath);
            EnsureFolder(MeshesPath);

            Material hull = GetMaterial("Workboat_Hull.mat", new Color(0.045f, 0.105f, 0.108f), 0.28f, 0.34f);
            Material deck = GetMaterial("Workboat_Deck_WornTimber.mat", new Color(0.29f, 0.235f, 0.17f), 0.02f, 0.22f);
            Material cabin = GetMaterial("Workboat_Cabin_PaleSage.mat", new Color(0.48f, 0.49f, 0.41f), 0.08f, 0.25f);
            Material roof = GetMaterial("Workboat_Roof_Charcoal.mat", new Color(0.065f, 0.09f, 0.095f), 0.18f, 0.38f);
            Material glass = GetMaterial("Workboat_Glass_DeepTeal.mat", new Color(0.025f, 0.105f, 0.125f), 0.42f, 0.82f);
            Material trim = GetMaterial("Workboat_Trim_FadedIvory.mat", new Color(0.62f, 0.56f, 0.42f), 0.08f, 0.28f);
            Material signalRed = GetMaterial("Workboat_Signal_Red.mat", new Color(0.62f, 0.16f, 0.11f), 0.12f, 0.3f);
            Material signalGreen = GetMaterial("Workboat_Signal_Green.mat", new Color(0.12f, 0.38f, 0.24f), 0.05f, 0.28f);
            SetEmission(signalGreen, new Color(0.08f, 0.23f, 0.08f));
            Material metal = GetMaterial("Workboat_Metal_Oxidized.mat", new Color(0.19f, 0.235f, 0.22f), 0.62f, 0.48f);
            Material rubber = GetMaterial("Workboat_Fenders_Rubber.mat", new Color(0.025f, 0.032f, 0.032f), 0.0f, 0.14f);
            Material rope = GetMaterial("Workboat_Rope_SaltWorn.mat", new Color(0.43f, 0.34f, 0.22f), 0.0f, 0.18f);
            Material lamp = GetMaterial("Workboat_Lamp_WarmAmber.mat", new Color(0.95f, 0.58f, 0.24f), 0.05f, 0.4f);
            SetEmission(lamp, new Color(0.95f, 0.36f, 0.10f));

            Transform visuals = vesselRoot.Find("Boat_Visual_Model");
            if (visuals == null)
            {
                GameObject visualObject = new GameObject("Boat_Visual_Model");
                visuals = visualObject.transform;
                visuals.SetParent(vesselRoot, false);
            }

            ClearChildren(visuals);

            Mesh hullMesh = SaveMeshAsset("Workboat_Hull_Shell.asset", BuildHullMesh());
            Mesh deckMesh = SaveMeshAsset("Workboat_Deck_Surface.asset", BuildDeckMesh());
            Mesh cabinMesh = SaveMeshAsset("Workboat_Wheelhouse_Shell.asset", BuildCabinMesh());

            AddMesh(visuals, "Hull_Shell", hullMesh, hull);
            AddMesh(visuals, "Deck_Surface", deckMesh, deck);
            AddMesh(visuals, "Wheelhouse_Shell", cabinMesh, cabin);

            BuildHullTrim(visuals, trim, signalRed, metal);
            BuildDeckDetail(visuals, deck, metal, rope, rubber);
            BuildBowRails(visuals, metal);
            BuildWheelhouse(visuals, roof, glass, trim, metal, lamp, signalRed);
            BuildOutboardAndStern(visuals, metal, rubber, signalRed);
            BuildBowFittings(visuals, metal, rope, trim);
            BuildNavigationDetails(visuals, metal, signalRed, signalGreen, lamp);

            AssetDatabase.SaveAssets();
            return visuals;
        }

        private static Mesh BuildHullMesh()
        {
            const int sectionPoints = 7;
            List<Vector3> vertices = new List<Vector3>(Stations.Length * sectionPoints + 2);
            List<Vector2> uv = new List<Vector2>(Stations.Length * sectionPoints + 2);
            List<int> triangles = new List<int>();

            for (int stationIndex = 0; stationIndex < Stations.Length; stationIndex++)
            {
                HullStation s = Stations[stationIndex];
                float[] x =
                {
                    -s.HalfBeam, -s.HalfBeam * 0.84f, -s.HalfBeam * 0.40f,
                    0f,
                    s.HalfBeam * 0.40f, s.HalfBeam * 0.84f, s.HalfBeam
                };
                float[] y =
                {
                    s.RailY, s.ChineY, s.KeelY + 0.06f, s.KeelY,
                    s.KeelY + 0.06f, s.ChineY, s.RailY
                };

                for (int point = 0; point < sectionPoints; point++)
                {
                    vertices.Add(new Vector3(x[point], y[point], s.Z));
                    uv.Add(new Vector2(point / (float)(sectionPoints - 1), stationIndex / 2.5f));
                }
            }

            for (int station = 0; station < Stations.Length - 1; station++)
            {
                for (int point = 0; point < sectionPoints - 1; point++)
                {
                    int a = station * sectionPoints + point;
                    int b = (station + 1) * sectionPoints + point;
                    int c = (station + 1) * sectionPoints + point + 1;
                    int d = station * sectionPoints + point + 1;
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(a); triangles.Add(d); triangles.Add(c);
                }
            }

            AddEndCap(vertices, uv, triangles, 0, sectionPoints, true);
            AddEndCap(vertices, uv, triangles, Stations.Length - 1, sectionPoints, false);

            Mesh mesh = new Mesh { name = "Workboat_Hull_Shell" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddEndCap(
            List<Vector3> vertices, List<Vector2> uv, List<int> triangles,
            int station, int sectionPoints, bool stern)
        {
            int start = station * sectionPoints;
            HullStation s = Stations[station];
            int center = vertices.Count;
            vertices.Add(new Vector3(0f, s.KeelY + 0.055f, s.Z));
            uv.Add(new Vector2(0.5f, 0.5f));

            for (int point = 0; point < sectionPoints - 1; point++)
            {
                if (stern)
                {
                    triangles.Add(center); triangles.Add(start + point + 1); triangles.Add(start + point);
                }
                else
                {
                    triangles.Add(center); triangles.Add(start + point); triangles.Add(start + point + 1);
                }
            }
        }

        private static Mesh BuildDeckMesh()
        {
            List<Vector3> vertices = new List<Vector3>(Stations.Length * 3);
            List<Vector2> uv = new List<Vector2>(Stations.Length * 3);
            List<int> triangles = new List<int>();

            for (int i = 0; i < Stations.Length; i++)
            {
                HullStation s = Stations[i];
                float y = s.RailY - 0.035f;
                vertices.Add(new Vector3(-s.HalfBeam + 0.025f, y, s.Z));
                vertices.Add(new Vector3(0f, y + 0.004f, s.Z));
                vertices.Add(new Vector3(s.HalfBeam - 0.025f, y, s.Z));
                uv.Add(new Vector2(0f, i / 2.5f));
                uv.Add(new Vector2(0.5f, i / 2.5f));
                uv.Add(new Vector2(1f, i / 2.5f));
            }

            for (int i = 0; i < Stations.Length - 1; i++)
            {
                int a = i * 3;
                int b = (i + 1) * 3;
                int c = b + 1;
                int d = a + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(d);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);

                a = i * 3 + 1;
                b = (i + 1) * 3 + 1;
                c = b + 1;
                d = a + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(d);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }

            Mesh mesh = new Mesh { name = "Workboat_Deck_Surface" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCabinMesh()
        {
            Vector3[] v =
            {
                new Vector3(-0.69f, 0.13f, 0.88f), new Vector3(0.69f, 0.13f, 0.88f),
                new Vector3(0.69f, 0.13f, -0.79f), new Vector3(-0.69f, 0.13f, -0.79f),
                new Vector3(-0.58f, 1.12f, 0.65f), new Vector3(0.58f, 1.12f, 0.65f),
                new Vector3(0.58f, 1.12f, -0.66f), new Vector3(-0.58f, 1.12f, -0.66f)
            };
            int[] t =
            {
                0,1,5, 0,5,4,
                1,2,6, 1,6,5,
                2,3,7, 2,7,6,
                3,0,4, 3,4,7
            };
            Mesh mesh = new Mesh { name = "Workboat_Wheelhouse_Shell" };
            mesh.vertices = v;
            mesh.triangles = t;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildHullTrim(Transform root, Material trim, Material accent, Material metal)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 1; i < Stations.Length; i++)
                {
                    HullStation previous = Stations[i - 1];
                    HullStation current = Stations[i];
                    Vector3 a = new Vector3(side * previous.HalfBeam * 0.94f, previous.ChineY + 0.025f, previous.Z);
                    Vector3 b = new Vector3(side * current.HalfBeam * 0.94f, current.ChineY + 0.025f, current.Z);
                    AddBoxBetween(root, "Hull_Sheer_Paint_Stripe", a, b, 0.035f, 0.045f, trim);
                    Vector3 rubA = new Vector3(side * previous.HalfBeam * 0.98f, previous.RailY - 0.055f, previous.Z);
                    Vector3 rubB = new Vector3(side * current.HalfBeam * 0.98f, current.RailY - 0.055f, current.Z);
                    AddBoxBetween(root, "Gunwale_Rub_Rail", rubA, rubB, 0.075f, 0.075f, metal);
                }
            }
            AddBox(root, "Transom_Safety_Mark", new Vector3(0f, 0.015f, -2.765f),
                new Vector3(0.62f, 0.055f, 0.035f), Quaternion.identity, accent);
        }

        private static void BuildDeckDetail(Transform root, Material deck, Material metal, Material rope, Material rubber)
        {
            for (int i = 0; i < 7; i++)
            {
                float z = -2.34f + i * 0.145f;
                AddBox(root, "Aft_Deck_Plank", new Vector3(0f, DeckHeightAt(z) + 0.014f, z),
                    new Vector3(1.38f, 0.026f, 0.11f), Quaternion.identity, deck);
            }

            AddBox(root, "Port_Fish_Crate", new Vector3(-0.46f, DeckHeightAt(-1.83f) + 0.13f, -1.83f),
                new Vector3(0.43f, 0.26f, 0.48f), Quaternion.identity, deck);
            AddBox(root, "Starboard_Fish_Crate", new Vector3(0.45f, DeckHeightAt(-1.87f) + 0.13f, -1.87f),
                new Vector3(0.42f, 0.26f, 0.44f), Quaternion.identity, deck);
            AddBox(root, "Crate_Port_Rim", new Vector3(-0.46f, DeckHeightAt(-1.83f) + 0.275f, -1.83f),
                new Vector3(0.47f, 0.035f, 0.52f), Quaternion.identity, metal);
            AddBox(root, "Crate_Starboard_Rim", new Vector3(0.45f, DeckHeightAt(-1.87f) + 0.275f, -1.87f),
                new Vector3(0.46f, 0.035f, 0.48f), Quaternion.identity, metal);

            AddCylinderBetween(root, "Deck_Winch_Spool", new Vector3(0.45f, DeckHeightAt(-1.18f) + 0.14f, -1.30f),
                new Vector3(0.45f, DeckHeightAt(-1.18f) + 0.14f, -1.06f), 0.10f, metal);
            AddCylinderBetween(root, "Winch_Handle", new Vector3(0.58f, DeckHeightAt(-1.18f) + 0.14f, -1.18f),
                new Vector3(0.72f, DeckHeightAt(-1.18f) + 0.14f, -1.18f), 0.025f, metal);
            AddRopeCoil(root, new Vector3(-0.42f, DeckHeightAt(-1.28f) + 0.03f, -1.28f), 0.16f, rope);

            AddSphere(root, "Port_Rubber_Fender", new Vector3(-1.01f, -0.015f, -0.95f),
                new Vector3(0.22f, 0.31f, 0.42f), rubber);
            AddSphere(root, "Starboard_Rubber_Fender", new Vector3(1.01f, -0.015f, -0.95f),
                new Vector3(0.22f, 0.31f, 0.42f), rubber);
            AddSphere(root, "Port_Aft_Fender", new Vector3(-0.80f, -0.015f, -2.15f),
                new Vector3(0.19f, 0.27f, 0.34f), rubber);
            AddSphere(root, "Starboard_Aft_Fender", new Vector3(0.80f, -0.015f, -2.15f),
                new Vector3(0.19f, 0.27f, 0.34f), rubber);

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 0.81f;
                float[] z = { -2.40f, -1.92f, -1.42f, -1.02f };
                for (int i = 0; i < z.Length; i++)
                {
                    float deckY = DeckHeightAt(z[i]);
                    AddCylinderBetween(root, "Aft_Rail_Stanchion",
                        new Vector3(x, deckY, z[i]), new Vector3(x, deckY + 0.46f, z[i]), 0.045f, metal);
                }
                for (int i = 0; i < z.Length - 1; i++)
                {
                    float railY = DeckHeightAt((z[i] + z[i + 1]) * 0.5f) + 0.45f;
                    AddCylinderBetween(root, "Aft_Deck_Handrail",
                        new Vector3(x, railY, z[i]), new Vector3(x, railY, z[i + 1]), 0.034f, metal);
                }
            }

            AddBox(root, "Aft_Work_Bench", new Vector3(0f, DeckHeightAt(-1.04f) + 0.12f, -1.04f),
                new Vector3(0.90f, 0.12f, 0.20f), Quaternion.identity, deck);
        }

        private static void BuildBowRails(Transform root, Material metal)
        {
            float[] z = { 1.16f, 1.70f, 2.22f };
            float[] halfBeam = { 0.87f, 0.70f, 0.55f };
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < z.Length; i++)
                {
                    float deckY = DeckHeightAt(z[i]);
                    AddCylinderBetween(root, "Bow_Rail_Stanchion",
                        new Vector3(side * halfBeam[i], deckY, z[i]),
                        new Vector3(side * halfBeam[i], deckY + 0.32f, z[i]), 0.035f, metal);
                    if (i < z.Length - 1)
                    {
                        float nextDeckY = DeckHeightAt(z[i + 1]);
                        AddCylinderBetween(root, "Bow_Handrail",
                            new Vector3(side * halfBeam[i], deckY + 0.31f, z[i]),
                            new Vector3(side * halfBeam[i + 1], nextDeckY + 0.31f, z[i + 1]), 0.03f, metal);
                    }
                }
            }

            float bowRailY = DeckHeightAt(2.22f) + 0.31f;
            AddCylinderBetween(root, "Bow_Front_Handrail",
                new Vector3(-halfBeam[2], bowRailY, z[2]),
                new Vector3(halfBeam[2], bowRailY, z[2]), 0.03f, metal);
        }

        private static void BuildWheelhouse(
            Transform root, Material roof, Material glass, Material trim,
            Material metal, Material lamp, Material accent)
        {
            AddBox(root, "Forward_Windscreen", new Vector3(0f, 0.86f, 0.70f),
                new Vector3(1.02f, 0.35f, 0.035f), Quaternion.Euler(8f, 0f, 0f), glass);
            AddBox(root, "Port_Side_Window", new Vector3(-0.625f, 0.87f, -0.005f),
                new Vector3(0.035f, 0.34f, 0.82f), Quaternion.identity, glass);
            AddBox(root, "Starboard_Side_Window", new Vector3(0.625f, 0.87f, -0.005f),
                new Vector3(0.035f, 0.34f, 0.82f), Quaternion.identity, glass);
            AddBox(root, "Aft_Cabin_Door", new Vector3(0f, 0.66f, -0.775f),
                new Vector3(0.38f, 0.72f, 0.04f), Quaternion.identity, roof);

            AddBox(root, "Wheelhouse_Roof", new Vector3(0f, 1.18f, 0f),
                new Vector3(1.48f, 0.13f, 1.58f), Quaternion.identity, roof);
            AddBox(root, "Wheelhouse_Roof_Edge", new Vector3(0f, 1.115f, 0.03f),
                new Vector3(1.55f, 0.045f, 1.65f), Quaternion.identity, trim);
            AddBox(root, "Windscreen_Mullion", new Vector3(0f, 0.86f, 0.728f),
                new Vector3(0.025f, 0.36f, 0.025f), Quaternion.identity, trim);
            AddBox(root, "Windscreen_Wiper", new Vector3(-0.19f, 0.72f, 0.752f),
                new Vector3(0.36f, 0.018f, 0.018f), Quaternion.Euler(0f, 0f, -8f), metal);
            AddBox(root, "Cabin_Port_Handle", new Vector3(-0.73f, 0.60f, -0.50f),
                new Vector3(0.045f, 0.16f, 0.055f), Quaternion.identity, metal);
            AddBox(root, "Cabin_Starboard_Handle", new Vector3(0.73f, 0.60f, -0.50f),
                new Vector3(0.045f, 0.16f, 0.055f), Quaternion.identity, metal);
            AddBox(root, "Cabin_Side_Safety_Stripe", new Vector3(0f, 0.43f, -0.02f),
                new Vector3(1.30f, 0.045f, 0.035f), Quaternion.identity, accent);
            AddBox(root, "Roof_Front_Lamp_Housing", new Vector3(0f, 1.24f, 0.73f),
                new Vector3(0.22f, 0.10f, 0.10f), Quaternion.identity, metal);
            AddSphere(root, "Roof_Front_Lamp", new Vector3(0f, 1.25f, 0.79f),
                new Vector3(0.11f, 0.08f, 0.065f), lamp);
        }

        private static void BuildOutboardAndStern(Transform root, Material metal, Material rubber, Material accent)
        {
            AddBox(root, "Outboard_Transom_Bracket", new Vector3(0f, 0.07f, -2.69f),
                new Vector3(0.48f, 0.24f, 0.18f), Quaternion.identity, metal);
            AddBox(root, "Outboard_Engine_Cowl", new Vector3(0f, -0.015f, -2.88f),
                new Vector3(0.44f, 0.50f, 0.40f), Quaternion.identity, rubber);
            AddBox(root, "Outboard_Engine_Cowl_Accent", new Vector3(0f, 0.08f, -3.085f),
                new Vector3(0.29f, 0.07f, 0.025f), Quaternion.identity, accent);
            AddCylinderBetween(root, "Outboard_Leg", new Vector3(0f, -0.24f, -2.91f),
                new Vector3(0f, -0.65f, -2.91f), 0.075f, metal);
            AddCylinderBetween(root, "Outboard_Propeller_Hub", new Vector3(0f, -0.63f, -2.91f),
                new Vector3(0f, -0.75f, -2.91f), 0.065f, metal);
            AddBox(root, "Outboard_Propeller_Blade_A", new Vector3(0f, -0.76f, -2.91f),
                new Vector3(0.34f, 0.055f, 0.07f), Quaternion.identity, metal);
            AddBox(root, "Outboard_Propeller_Blade_B", new Vector3(0f, -0.76f, -2.91f),
                new Vector3(0.07f, 0.055f, 0.34f), Quaternion.identity, metal);
        }

        private static void BuildBowFittings(Transform root, Material metal, Material rope, Material trim)
        {
            float bowDeck = DeckHeightAt(2.55f);
            AddBox(root, "Bow_Anchor_Roller", new Vector3(0f, bowDeck + 0.07f, 2.55f),
                new Vector3(0.25f, 0.12f, 0.38f), Quaternion.identity, metal);
            AddCylinderBetween(root, "Bow_Anchor_Pin", new Vector3(-0.10f, bowDeck + 0.09f, 2.58f),
                new Vector3(0.10f, bowDeck + 0.09f, 2.58f), 0.035f, metal);
            AddBox(root, "Bow_Anchor_Stock", new Vector3(0f, bowDeck + 0.04f, 2.59f),
                new Vector3(0.38f, 0.06f, 0.06f), Quaternion.identity, metal);
            AddCylinderBetween(root, "Port_Bow_Cleat", new Vector3(-0.39f, bowDeck + 0.03f, 2.13f),
                new Vector3(-0.39f, bowDeck + 0.10f, 2.13f), 0.045f, metal);
            AddCylinderBetween(root, "Starboard_Bow_Cleat", new Vector3(0.39f, bowDeck + 0.03f, 2.13f),
                new Vector3(0.39f, bowDeck + 0.10f, 2.13f), 0.045f, metal);
            AddBox(root, "Bow_Deck_Paint_Mark", new Vector3(0f, bowDeck + 0.02f, 2.26f),
                new Vector3(0.14f, 0.025f, 0.34f), Quaternion.identity, trim);
            AddRopeCoil(root, new Vector3(0.42f, DeckHeightAt(1.68f) + 0.03f, 1.68f), 0.12f, rope);
        }

        private static void BuildNavigationDetails(
            Transform root, Material metal, Material signalRed, Material signalGreen, Material lamp)
        {
            AddCylinderBetween(root, "Roof_Antenna_Mast", new Vector3(0f, 1.24f, -0.38f),
                new Vector3(0f, 1.82f, -0.38f), 0.025f, metal);
            AddCylinderBetween(root, "Roof_Antenna_Crossbar", new Vector3(-0.19f, 1.67f, -0.38f),
                new Vector3(0.19f, 1.67f, -0.38f), 0.018f, metal);
            AddBox(root, "Antenna_Base", new Vector3(0f, 1.25f, -0.38f),
                new Vector3(0.12f, 0.06f, 0.12f), Quaternion.identity, metal);
            AddSphere(root, "Port_Navigation_Light", new Vector3(-0.70f, 1.22f, -0.46f),
                new Vector3(0.085f, 0.11f, 0.085f), signalRed);
            AddSphere(root, "Starboard_Navigation_Light", new Vector3(0.70f, 1.22f, -0.46f),
                new Vector3(0.085f, 0.11f, 0.085f), signalGreen);
            AddSphere(root, "Aft_Utility_Light", new Vector3(0f, 0.57f, -2.50f),
                new Vector3(0.10f, 0.09f, 0.08f), lamp);
        }

        private static float DeckHeightAt(float z)
        {
            if (z <= Stations[0].Z) return Stations[0].RailY - 0.035f;
            for (int i = 1; i < Stations.Length; i++)
            {
                if (z <= Stations[i].Z)
                {
                    float blend = Mathf.InverseLerp(Stations[i - 1].Z, Stations[i].Z, z);
                    float a = Stations[i - 1].RailY - 0.035f;
                    float b = Stations[i].RailY - 0.035f;
                    return Mathf.Lerp(a, b, blend);
                }
            }
            return Stations[Stations.Length - 1].RailY - 0.035f;
        }

        private static void AddRopeCoil(Transform root, Vector3 center, float radius, Material material)
        {
            const int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Vector3 p0 = center + new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
                AddCylinderBetween(root, "Coiled_Deck_Rope", p0, p1, 0.025f, material);
            }
        }

        private static void AddMesh(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static GameObject AddBox(
            Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(part);
            return part;
        }

        private static GameObject AddSphere(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(part);
            return part;
        }

        private static void AddBoxBetween(
            Transform parent, string name, Vector3 start, Vector3 end, float width, float height, Material material)
        {
            Vector3 delta = end - start;
            AddBox(parent, name, (start + end) * 0.5f,
                new Vector3(width, height, delta.magnitude), Quaternion.LookRotation(delta.normalized), material);
        }

        private static void AddCylinderBetween(
            Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material)
        {
            Vector3 delta = end - start;
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = (start + end) * 0.5f;
            part.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            part.transform.localScale = new Vector3(radius * 2f, delta.magnitude * 0.5f, radius * 2f);
            part.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(part);
        }

        private static void RemoveCollider(GameObject part)
        {
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }

        private static Mesh SaveMeshAsset(string fileName, Mesh generated)
        {
            string path = MeshesPath + "/" + fileName;
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            existing.name = generated.name;
            UnityEngine.Object.DestroyImmediate(generated);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Material GetMaterial(string fileName, Color color, float metallic, float smoothness)
        {
            string path = MaterialsPath + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null)
                    throw new InvalidOperationException("No supported shader was found for the workboat model.");
                material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(fileName) };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetEmission(Material material, Color emission)
        {
            if (!material.HasProperty("_EmissionColor")) return;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            EditorUtility.SetDirty(material);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                throw new InvalidOperationException("Invalid workboat asset folder: " + path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

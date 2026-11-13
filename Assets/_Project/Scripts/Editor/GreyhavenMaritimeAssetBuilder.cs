using System;
using UnityEngine;

namespace WhereTheSoundingsEnd.Editor
{
    /// <summary>
    /// Deterministic, static maritime art pass for Greyhaven. The assembled silhouettes
    /// replace repeated blockout language without introducing runtime or collision costs.
    /// </summary>
    public static class GreyhavenMaritimeAssetBuilder
    {
        public static void Build(
            Transform harbour, Transform village, Transform estuary, Transform landmarks,
            Material timber, Material iron, Material rust, Material ceramic, Material canvas,
            Material rope, Material foliage, Material basalt, Material glass)
        {
            if (harbour == null || village == null || estuary == null || landmarks == null)
                throw new ArgumentNullException("Greyhaven asset roots must be assigned.");

            BuildHarbourAssets(harbour, timber, iron, rust, rope, glass);
            BuildVillageAssets(village, timber, iron, rust, canvas, ceramic);
            BuildEstuaryAssets(estuary, timber, iron, rust, rope, foliage, basalt, glass);
            BuildHeadlandAssets(landmarks, timber, iron, rust, canvas, basalt, glass);
        }

        private static void BuildHarbourAssets(Transform parent, Material timber, Material iron,
            Material rust, Material rope, Material glass)
        {
            Transform root = Group("Harbour_Bespoke_Maritime_Assets", parent);
            BuildBollard(root, "West_Mooring_Bollard", new Vector3(-3.4f, 0.84f, 29.5f), iron, rust, rope);
            BuildBollard(root, "East_Mooring_Bollard", new Vector3(3.4f, 0.84f, 29.5f), iron, rust, rope);
            BuildCleat(root, "Pier_Mooring_Cleat_A", new Vector3(-2.8f, 0.91f, 17f), iron);
            BuildCleat(root, "Pier_Mooring_Cleat_B", new Vector3(2.8f, 0.91f, 25f), iron);
            BuildLantern(root, "Quay_Glass_Lantern", new Vector3(-4.1f, 1.05f, 36.2f), iron, rust, glass);
            BuildAnchor(root, "Retired_Admiralty_Anchor", new Vector3(4.6f, 0.96f, 36.5f), iron, rust, rope);
            BuildLobsterPot(root, "Harbour_Lobster_Pot_A", new Vector3(-5.4f, 0.95f, 31f), timber, rope, rust, 0.78f);
            BuildLobsterPot(root, "Harbour_Lobster_Pot_B", new Vector3(-6.1f, 0.95f, 32.3f), timber, rope, rust, 0.62f);
            BuildNetFloatLine(root, "Harbour_Float_Line", new Vector3(5.1f, 1.1f, 19.2f), rope, glass, rust);
            // Salt streaks and tar patches are sparse, broad, and confined to the working deck edge.
            WearPatch(root, "Quay_Salt_Wear_A", new Vector3(-3.55f, 0.84f, 21f), new Vector2(1.7f, 4.8f), canvas, -8f);
            WearPatch(root, "Quay_Tar_Wear_B", new Vector3(3.45f, 0.84f, 31f), new Vector2(1.2f, 3.2f), rust, 14f);
            RopeCoil(root, "Quay_Heavy_Rope_Coil", new Vector3(5.2f, 0.94f, 35.4f), rope, 0.48f);
            Log(root, "Quay_Split_Fender_Log", new Vector3(-4.2f, 0.91f, 12.5f),
                new Vector3(-3.65f, 1.0f, 15.2f), 0.16f, timber);
        }

        private static void BuildVillageAssets(Transform parent, Material timber, Material iron,
            Material rust, Material canvas, Material ceramic)
        {
            Transform root = Group("Village_Working_Life_Assets", parent);
            float market = Ground(-34f, -35.5f);
            BuildBarrel(root, "Fish_Market_Brine_Barrel", new Vector3(-38.2f, market + 0.55f, -36.4f), timber, rust, ceramic);
            BuildFishCrate(root, "Market_Vented_Fish_Crate", new Vector3(-32.9f, market + 0.28f, -37.1f), timber, iron);
            BuildFishCrate(root, "Market_Stacked_Fish_Crate", new Vector3(-32.05f, market + 0.28f, -37.0f), timber, iron);
            WearPatch(root, "Market_Mud_And_Salt_Scuff", new Vector3(-35.5f, market + 0.015f, -37.6f),
                new Vector2(4.5f, 2.2f), canvas, -6f);

            float repair = Ground(13f, -47f);
            BuildHandWinch(root, "Repair_Shed_Hand_Winch", new Vector3(13f, repair + 0.12f, -47f), timber, iron, rust);
            BuildPropeller(root, "Workshop_Three_Blade_Propeller", new Vector3(10.2f, repair + 0.16f, -49.5f), iron, rust);
            BuildBarrel(root, "Workshop_Oil_Drum", new Vector3(14.8f, repair + 0.48f, -49.2f), iron, rust, ceramic);
            WearPatch(root, "Workshop_Oil_And_Grit", new Vector3(13.7f, repair + 0.02f, -49.6f),
                new Vector2(3.8f, 1.9f), rust, 18f);

            float nets = Ground(-55f, -53f);
            BuildNetRack(root, "Net_Mender_Drying_Rack", new Vector3(-58f, nets, -55f), timber, rope, canvas);
            RopeCoil(root, "Net_Mender_Spare_Hawser", new Vector3(-52.7f, nets + 0.08f, -54.2f), rope, 0.36f);
            BuildFishCrate(root, "Net_Mender_Float_Crate", new Vector3(-57.5f, nets + 0.28f, -52.8f), timber, iron);

            float inn = Ground(45f, -59f);
            BuildBarrel(root, "Inn_Rainwater_Cask", new Vector3(47.1f, inn + 0.55f, -59.8f), timber, rust, ceramic);
            BuildWashTub(root, "Inn_Boot_Wash_Tub", new Vector3(43.5f, inn + 0.16f, -58.4f), ceramic, iron);
            WearPatch(root, "Inn_Doorstep_Worn_Stone", new Vector3(45f, inn + 0.02f, -57.7f),
                new Vector2(2.6f, 1.1f), canvas, 0f);
        }

        private static void BuildEstuaryAssets(Transform parent, Material timber, Material iron,
            Material rust, Material rope, Material foliage, Material basalt, Material glass)
        {
            Transform root = Group("Estuary_Ecological_And_Shoreline_Assets", parent);
            Transform mangrove = Group("Mangrove_Roots_And_Canopy_Variants", root);
            // Trees are deliberately irregular: exposed prop roots, bent trunks, and layered crowns.
            BuildMangrove(mangrove, "East_Mangrove_01", new Vector3(81f, Ground(81f, -33f), -33f), timber, foliage, 1.15f);
            BuildMangrove(mangrove, "East_Mangrove_02", new Vector3(88f, Ground(88f, -43f), -43f), timber, foliage, 0.88f);
            BuildMangrove(mangrove, "East_Mangrove_03", new Vector3(73f, Ground(73f, -49f), -49f), timber, foliage, 1.28f);
            BuildMangrove(mangrove, "East_Mangrove_04", new Vector3(94f, Ground(94f, -29f), -29f), timber, foliage, 0.76f);
            BuildReedClump(root, "Eastern_Tidal_Reeds_A", new Vector3(64f, Ground(64f, -29f), -29f), foliage, 1.1f);
            BuildReedClump(root, "Eastern_Tidal_Reeds_B", new Vector3(70f, Ground(70f, -42f), -42f), foliage, 0.82f);
            BuildReedClump(root, "Western_Salt_Grass", new Vector3(-78f, Ground(-78f, -30f), -30f), foliage, 0.72f);

            Transform wrack = Group("Tide_Line_Wrack_And_Debris", root);
            Log(wrack, "Storm_Tossed_Driftwood_A", new Vector3(61f, Ground(61f, -25f) + 0.1f, -25f),
                new Vector3(64.4f, Ground(64.4f, -25.8f) + 0.12f, -25.8f), 0.22f, timber);
            Log(wrack, "Storm_Tossed_Driftwood_B", new Vector3(74f, Ground(74f, -31f) + 0.08f, -31f),
                new Vector3(72.1f, Ground(72.1f, -32.5f) + 0.1f, -32.5f), 0.14f, timber);
            BuildLobsterPot(wrack, "Tide_Line_Collapsed_Crab_Pot", new Vector3(67f, Ground(67f, -29f) + 0.05f, -29f), timber, rope, rust, 0.68f);
            BuildNetFloatLine(wrack, "Mudflat_Abandoned_Float_Line", new Vector3(75f, Ground(75f, -28f) + 0.1f, -28f), rope, glass, rust);
            BuildAnchor(wrack, "Half_Buried_Small_Anchor", new Vector3(-73f, Ground(-73f, -29f) + 0.08f, -29f), iron, rust, rope);
            WearPatch(wrack, "Tidal_Silt_Sheen_A", new Vector3(68f, Ground(68f, -31f) + 0.012f, -31f),
                new Vector2(5.8f, 2.1f), basalt, 23f);
            WearPatch(wrack, "Tidal_Silt_Sheen_B", new Vector3(-74f, Ground(-74f, -30f) + 0.012f, -30f),
                new Vector2(3.4f, 1.6f), canvas, -18f);
        }

        private static void BuildHeadlandAssets(Transform parent, Material timber, Material iron,
            Material rust, Material canvas, Material basalt, Material glass)
        {
            Transform root = Group("Lighthouse_Headland_Maritime_Assets", parent);
            float ground = Ground(-86f, -75f);
            BuildWeatherStation(root, "Keepers_Weather_Station", new Vector3(-86f, ground, -75f), timber, iron, rust, glass);
            BuildLantern(root, "Keepers_Storm_Lantern", new Vector3(-82.5f, ground + 0.12f, -72.2f), iron, rust, glass);
            BuildNetFloatLine(root, "Headland_Rope_And_Float_Remnants", new Vector3(-89f, ground + 0.08f, -69f), canvas, glass, rust);
            Log(root, "Headland_Wind_Felled_Branch", new Vector3(-91f, ground + 0.06f, -77f),
                new Vector3(-88.8f, ground + 0.08f, -79f), 0.13f, timber);
            WearPatch(root, "Lighthouse_Stone_Salt_Stain", new Vector3(-86f, ground + 0.018f, -71f),
                new Vector2(4.2f, 2.8f), basalt, 9f);
        }

        private static void BuildBollard(Transform p, string name, Vector3 pos, Material iron, Material rust, Material rope)
        {
            Cylinder(p, name + "_Pedestal", pos + Vector3.up * 0.16f, new Vector3(0.42f, 0.16f, 0.42f), iron);
            Cylinder(p, name + "_Shaft", pos + Vector3.up * 0.52f, new Vector3(0.18f, 0.38f, 0.18f), rust);
            Cylinder(p, name + "_Cap", pos + Vector3.up * 0.9f, new Vector3(0.34f, 0.12f, 0.34f), iron);
            Cylinder(p, name + "_Horn", pos + new Vector3(0f, 0.78f, 0f), new Vector3(0.43f, 0.1f, 0.43f), iron);
            RopeCoil(p, name + "_Rope_Lashing", pos + new Vector3(0.28f, 0.58f, 0f), rope, 0.16f);
            for (int i = 0; i < 3; i++)
                Cube(p, name + "_Rust_Scar_" + i, pos + new Vector3(0.096f, 0.36f + i * 0.17f, -0.01f),
                    new Vector3(0.012f, 0.06f, 0.055f), rust, i * 17f);
        }

        private static void BuildCleat(Transform p, string name, Vector3 pos, Material iron)
        {
            Cylinder(p, name + "_Base", pos, new Vector3(0.42f, 0.08f, 0.28f), iron);
            Between(p, name + "_Stem", pos + Vector3.up * 0.06f, pos + Vector3.up * 0.28f, 0.11f, iron);
            Between(p, name + "_Horn_Left", pos + new Vector3(-0.26f, 0.27f, 0f), pos + new Vector3(-0.18f, 0.36f, 0f), 0.09f, iron);
            Between(p, name + "_Horn_Right", pos + new Vector3(0.26f, 0.27f, 0f), pos + new Vector3(0.18f, 0.36f, 0f), 0.09f, iron);
            Between(p, name + "_Crossbar", pos + new Vector3(-0.2f, 0.31f, 0f), pos + new Vector3(0.2f, 0.31f, 0f), 0.09f, iron);
        }

        private static void BuildLantern(Transform p, string name, Vector3 pos, Material iron, Material rust, Material glass)
        {
            Cylinder(p, name + "_Foot", pos + Vector3.up * 0.08f, new Vector3(0.34f, 0.08f, 0.34f), iron);
            Cube(p, name + "_Glass_Core", pos + Vector3.up * 0.42f, new Vector3(0.27f, 0.52f, 0.27f), glass, 0f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                Vector3 offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.17f;
                Between(p, name + "_Frame_" + i, pos + offset + Vector3.up * 0.16f,
                    pos + offset + Vector3.up * 0.69f, 0.045f, rust);
            }
            Cylinder(p, name + "_Roof", pos + Vector3.up * 0.74f, new Vector3(0.4f, 0.08f, 0.4f), iron);
            Cylinder(p, name + "_Vent", pos + Vector3.up * 0.84f, new Vector3(0.12f, 0.1f, 0.12f), rust);
            Between(p, name + "_Carry_Handle_Left", pos + new Vector3(-0.13f, 0.86f, 0f),
                pos + new Vector3(0f, 1.05f, 0f), 0.035f, iron);
            Between(p, name + "_Carry_Handle_Right", pos + new Vector3(0f, 1.05f, 0f),
                pos + new Vector3(0.13f, 0.86f, 0f), 0.035f, iron);
        }

        private static void BuildAnchor(Transform p, string name, Vector3 pos, Material iron, Material rust, Material rope)
        {
            Between(p, name + "_Shank", pos + Vector3.up * 0.08f, pos + Vector3.up * 1.05f, 0.13f, iron);
            Between(p, name + "_Stock", pos + new Vector3(-0.62f, 0.82f, 0f), pos + new Vector3(0.62f, 0.82f, 0f), 0.11f, rust);
            Between(p, name + "_Fluke_Left", pos + new Vector3(-0.58f, 0.12f, 0f), pos + new Vector3(-0.94f, 0.52f, 0f), 0.14f, iron);
            Between(p, name + "_Fluke_Right", pos + new Vector3(0.58f, 0.12f, 0f), pos + new Vector3(0.94f, 0.52f, 0f), 0.14f, iron);
            Cylinder(p, name + "_Ring", pos + Vector3.up * 1.18f, new Vector3(0.3f, 0.07f, 0.3f), rust);
            RopeCoil(p, name + "_Short_Rode", pos + new Vector3(0.55f, 0.13f, 0.35f), rope, 0.22f);
        }

        private static void BuildLobsterPot(Transform p, string name, Vector3 pos, Material timber, Material rope, Material rust, float scale)
        {
            Transform g = Group(name, p);
            g.localPosition = pos;
            g.localScale = Vector3.one * scale;
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                    Between(g, name + "_Corner_" + i + "_" + j,
                        new Vector3(i * 0.52f, 0f, j * 0.38f), new Vector3(i * 0.52f, 0.76f, j * 0.38f), 0.075f, timber);
            Between(g, name + "_Top_Front", new Vector3(-0.52f, 0.76f, -0.38f), new Vector3(0.52f, 0.76f, -0.38f), 0.07f, timber);
            Between(g, name + "_Top_Back", new Vector3(-0.52f, 0.76f, 0.38f), new Vector3(0.52f, 0.76f, 0.38f), 0.07f, timber);
            Between(g, name + "_Top_Left", new Vector3(-0.52f, 0.76f, -0.38f), new Vector3(-0.52f, 0.76f, 0.38f), 0.07f, timber);
            Between(g, name + "_Top_Right", new Vector3(0.52f, 0.76f, -0.38f), new Vector3(0.52f, 0.76f, 0.38f), 0.07f, timber);
            for (int y = 1; y <= 5; y++)
            {
                float h = y * 0.12f;
                Between(g, name + "_Net_Rib_A_" + y, new Vector3(-0.5f, h, -0.39f), new Vector3(0.5f, h, 0.39f), 0.018f, rope);
                Between(g, name + "_Net_Rib_B_" + y, new Vector3(-0.5f, h, 0.39f), new Vector3(0.5f, h, -0.39f), 0.018f, rope);
            }
            Between(g, name + "_Entrance_Funnel_Left", new Vector3(-0.22f, 0.25f, -0.39f), new Vector3(0f, 0.46f, -0.18f), 0.035f, rust);
            Between(g, name + "_Entrance_Funnel_Right", new Vector3(0.22f, 0.25f, -0.39f), new Vector3(0f, 0.46f, -0.18f), 0.035f, rust);
            Cube(g, name + "_Bait_Bag", new Vector3(0f, 0.12f, 0.12f), new Vector3(0.24f, 0.16f, 0.22f), rust, 12f);
        }

        private static void BuildBarrel(Transform p, string name, Vector3 pos, Material body, Material bands, Material lid)
        {
            Cylinder(p, name + "_Stave_Body", pos + Vector3.up * 0.48f, new Vector3(0.54f, 0.48f, 0.54f), body);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.27f;
                Cube(p, name + "_Stave_Seam_" + i, pos + offset + Vector3.up * 0.48f,
                    new Vector3(0.025f, 0.78f, 0.018f), bands, -i * 72f);
            }
            Cylinder(p, name + "_Lower_Hoop", pos + Vector3.up * 0.22f, new Vector3(0.57f, 0.045f, 0.57f), bands);
            Cylinder(p, name + "_Upper_Hoop", pos + Vector3.up * 0.72f, new Vector3(0.57f, 0.045f, 0.57f), bands);
            Cylinder(p, name + "_Top_Lid", pos + Vector3.up * 0.97f, new Vector3(0.5f, 0.035f, 0.5f), lid);
            Cube(p, name + "_Chalk_Mark", pos + new Vector3(0f, 0.51f, -0.276f),
                new Vector3(0.13f, 0.06f, 0.012f), lid, -8f);
        }

        private static void BuildFishCrate(Transform p, string name, Vector3 pos, Material timber, Material iron)
        {
            Cube(p, name + "_Base", pos + Vector3.up * 0.08f, new Vector3(0.88f, 0.12f, 0.64f), timber, 0f);
            for (int side = -1; side <= 1; side += 2)
                for (int row = 0; row < 3; row++)
                    Cube(p, name + "_Slat_" + side + "_" + row,
                        pos + new Vector3(0f, 0.18f + row * 0.12f, side * 0.29f),
                        new Vector3(0.88f, 0.075f, 0.055f), timber, row % 2 == 0 ? -2f : 2f);
            for (int side = -1; side <= 1; side += 2)
                for (int x = -1; x <= 1; x += 2)
                    Cube(p, name + "_Corner_" + side + "_" + x,
                        pos + new Vector3(x * 0.41f, 0.24f, side * 0.29f),
                        new Vector3(0.06f, 0.48f, 0.06f), iron, 0f);
            Cube(p, name + "_Drain_Gap", pos + new Vector3(0f, 0.12f, 0.0f),
                new Vector3(0.42f, 0.012f, 0.025f), iron, 0f);
        }

        private static void BuildHandWinch(Transform p, string name, Vector3 pos, Material timber, Material iron, Material rust)
        {
            Cube(p, name + "_Bed", pos + Vector3.up * 0.12f, new Vector3(1.45f, 0.18f, 0.72f), timber, -8f);
            for (int x = -1; x <= 1; x += 2)
                Cube(p, name + "_Upright_" + x, pos + new Vector3(x * 0.48f, 0.62f, 0f),
                    new Vector3(0.12f, 0.9f, 0.58f), timber, 0f);
            Between(p, name + "_Axle", pos + new Vector3(-0.62f, 0.78f, 0f),
                pos + new Vector3(0.62f, 0.78f, 0f), 0.12f, iron);
            Cylinder(p, name + "_Drum", pos + new Vector3(0f, 0.78f, 0f), new Vector3(0.4f, 0.28f, 0.4f), rust);
            Between(p, name + "_Crank_Arm", pos + new Vector3(0.62f, 0.78f, 0f),
                pos + new Vector3(0.78f, 1.2f, 0f), 0.07f, iron);
            Between(p, name + "_Crank_Grip", pos + new Vector3(0.78f, 1.2f, -0.18f),
                pos + new Vector3(0.78f, 1.2f, 0.18f), 0.09f, timber);
        }

        private static void BuildPropeller(Transform p, string name, Vector3 pos, Material iron, Material rust)
        {
            Cylinder(p, name + "_Hub", pos, new Vector3(0.24f, 0.18f, 0.24f), rust);
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2f / 3f;
                Vector3 direction = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 end = pos + direction * 0.92f;
                Between(p, name + "_Blade_" + i, pos, end, 0.16f, iron);
                Cube(p, name + "_Blade_Tip_" + i, end, new Vector3(0.22f, 0.06f, 0.18f), rust, -i * 120f);
            }
        }

        private static void BuildNetRack(Transform p, string name, Vector3 pos, Material timber, Material rope, Material canvas)
        {
            for (int x = -1; x <= 1; x += 2)
                Between(p, name + "_Post_" + x, pos + new Vector3(x * 1.2f, 0f, 0f),
                    pos + new Vector3(x * 1.2f, 2.1f, 0f), 0.13f, timber);
            Between(p, name + "_Top_Beam", pos + new Vector3(-1.3f, 2.1f, 0f),
                pos + new Vector3(1.3f, 2.1f, 0f), 0.14f, timber);
            for (int i = 0; i < 8; i++)
                Between(p, name + "_Hanging_Net_Strands_" + i,
                    pos + new Vector3(-1.05f + i * 0.3f, 0.15f, 0.06f),
                    pos + new Vector3(-0.85f + i * 0.3f, 1.9f, 0.06f), 0.018f, rope);
            Cube(p, name + "_Folded_Mending_Canvas", pos + new Vector3(0.3f, 0.18f, 0.34f),
                new Vector3(0.85f, 0.12f, 0.48f), canvas, 9f);
        }

        private static void BuildWashTub(Transform p, string name, Vector3 pos, Material ceramic, Material iron)
        {
            Cylinder(p, name + "_Basin", pos + Vector3.up * 0.18f, new Vector3(0.68f, 0.18f, 0.68f), ceramic);
            Cylinder(p, name + "_Rim", pos + Vector3.up * 0.36f, new Vector3(0.74f, 0.055f, 0.74f), iron);
            Cube(p, name + "_Waterline", pos + new Vector3(0f, 0.27f, 0f),
                new Vector3(0.44f, 0.012f, 0.44f), ceramic, 0f);
        }

        private static void BuildMangrove(Transform p, string name, Vector3 pos, Material timber, Material leaves, float scale)
        {
            Transform tree = Group(name, p);
            tree.localPosition = pos;
            tree.localScale = Vector3.one * scale;
            Between(tree, name + "_Bent_Trunk_Lower", Vector3.zero, new Vector3(0.18f, 2.2f, 0.05f), 0.32f, timber);
            Between(tree, name + "_Bent_Trunk_Upper", new Vector3(0.18f, 2.2f, 0.05f), new Vector3(-0.22f, 3.7f, 0.2f), 0.22f, timber);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 0.4f;
                Vector3 rootStart = new Vector3(Mathf.Cos(a) * 0.12f, 0.2f, Mathf.Sin(a) * 0.12f);
                Vector3 rootEnd = new Vector3(Mathf.Cos(a) * (1.05f + (i % 2) * 0.25f), 0.03f,
                    Mathf.Sin(a) * (0.85f + (i % 3) * 0.12f));
                Between(tree, name + "_Prop_Root_" + i, rootStart, rootEnd, 0.13f - i * 0.009f, timber);
            }
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.35f;
                Vector3 start = new Vector3(-0.12f, 2.7f, 0.14f);
                Vector3 end = start + new Vector3(Mathf.Cos(a) * 1.1f, 0.55f + (i % 2) * 0.3f, Mathf.Sin(a) * 0.9f);
                Between(tree, name + "_Canopy_Branch_" + i, start, end, 0.09f, timber);
                Sphere(tree, name + "_Canopy_Cluster_" + i, end + Vector3.up * 0.15f,
                    new Vector3(1.05f, 0.68f, 0.9f), leaves);
            }
            Sphere(tree, name + "_Canopy_Crown", new Vector3(-0.12f, 3.55f, 0.18f),
                new Vector3(1.4f, 0.82f, 1.2f), leaves);
        }

        private static void BuildReedClump(Transform p, string name, Vector3 pos, Material foliage, float scale)
        {
            Transform clump = Group(name, p);
            clump.localPosition = pos;
            clump.localScale = Vector3.one * scale;
            for (int i = 0; i < 13; i++)
            {
                float a = i * 2.39996323f;
                float radius = 0.12f + (i % 4) * 0.09f;
                Vector3 start = new Vector3(Mathf.Cos(a) * radius, 0.02f, Mathf.Sin(a) * radius);
                Vector3 end = start + new Vector3(Mathf.Cos(a) * (0.18f + (i % 3) * 0.12f),
                    0.55f + (i % 5) * 0.18f, Mathf.Sin(a) * (0.18f + (i % 2) * 0.1f));
                Between(clump, name + "_Blade_" + i, start, end, 0.025f + (i % 3) * 0.006f, foliage);
            }
        }

        private static void BuildWeatherStation(Transform p, string name, Vector3 pos, Material timber, Material iron, Material rust, Material glass)
        {
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2f / 3f;
                Vector3 foot = pos + new Vector3(Mathf.Cos(a) * 0.72f, 0.08f, Mathf.Sin(a) * 0.72f);
                Vector3 top = pos + new Vector3(0f, 1.65f, 0f);
                Between(p, name + "_Tripod_Leg_" + i, foot, top, 0.07f, timber);
            }
            Cylinder(p, name + "_Pivot", pos + Vector3.up * 1.7f, new Vector3(0.2f, 0.09f, 0.2f), iron);
            Between(p, name + "_Wind_Vane_Shaft", pos + Vector3.up * 1.72f, pos + Vector3.up * 2.35f, 0.035f, rust);
            Between(p, name + "_Vane_Arrow", pos + new Vector3(-0.45f, 2.25f, 0f),
                pos + new Vector3(0.45f, 2.25f, 0f), 0.045f, iron);
            Between(p, name + "_Rain_Gauge", pos + new Vector3(0.55f, 0.1f, 0.45f),
                pos + new Vector3(0.55f, 1.1f, 0.45f), 0.08f, glass);
        }


        private static void BuildNetFloatLine(Transform p, string name, Vector3 pos, Material rope, Material floatMaterial, Material weight)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 point = pos + new Vector3(i * 0.34f, Mathf.Sin(i * 0.8f) * 0.06f, Mathf.Cos(i * 0.55f) * 0.12f);
                if (i > 0)
                {
                    Vector3 previous = pos + new Vector3((i - 1) * 0.34f,
                        Mathf.Sin((i - 1) * 0.8f) * 0.06f, Mathf.Cos((i - 1) * 0.55f) * 0.12f);
                    Between(p, name + "_Rope_" + i, previous, point, 0.025f, rope);
                }
                Sphere(p, name + "_Float_" + i, point, new Vector3(0.14f, 0.12f, 0.14f), i % 2 == 0 ? floatMaterial : weight);
            }
        }

        private static void RopeCoil(Transform p, string name, Vector3 pos, Material rope, float radius)
        {
            const int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 a = pos + new Vector3(Mathf.Cos(a0) * radius, 0.04f + Mathf.Sin(a0 * 2f) * 0.012f, Mathf.Sin(a0) * radius);
                Vector3 b = pos + new Vector3(Mathf.Cos(a1) * radius, 0.04f + Mathf.Sin(a1 * 2f) * 0.012f, Mathf.Sin(a1) * radius);
                Between(p, name + "_Segment_" + i, a, b, 0.045f, rope);
            }
            Cylinder(p, name + "_Coil_Core", pos + Vector3.up * 0.025f, new Vector3(radius * 0.62f, 0.025f, radius * 0.62f), rope);
        }

        private static void Log(Transform p, string name, Vector3 start, Vector3 end, float radius, Material wood)
        {
            Between(p, name + "_Trunk", start, end, radius, wood);
            Sphere(p, name + "_Broken_End_A", start, Vector3.one * radius * 1.7f, wood);
            Sphere(p, name + "_Broken_End_B", end, Vector3.one * radius * 1.5f, wood);
        }

        private static void WearPatch(Transform p, string name, Vector3 pos, Vector2 size, Material material, float yaw)
        {
            // A low, irregular, shallow patch reads as salt, mud or oil staining without a decal shader.
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            patch.name = name;
            patch.transform.SetParent(p, false);
            patch.transform.localPosition = pos;
            patch.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            patch.transform.localScale = new Vector3(size.x, 0.018f, size.y);
            patch.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(patch);
        }

        private static float Ground(float x, float z)
        {
            return GreyhavenTerrainProfile.HeightAt(x, z);
        }

        private static Transform Group(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 scale, Material material, float yaw)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(go);
            return go;
        }

        private static GameObject Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(go);
            return go;
        }

        private static GameObject Sphere(Transform parent, string name, Vector3 pos, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(go);
            return go;
        }

        private static void Between(Transform parent, string name, Vector3 start, Vector3 end, float diameter, Material material)
        {
            Vector3 delta = end - start;
            GameObject go = Cylinder(parent, name, (start + end) * 0.5f,
                new Vector3(diameter, delta.magnitude * 0.5f, diameter), material);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }
    }
}

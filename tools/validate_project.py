#!/usr/bin/env python3
"""Static integrity checks for the Unity scaffold; this is not a Unity build."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_EDITOR = "6000.3.28f1"
EXPECTED_PACKAGES = {
    "com.unity.inputsystem": "1.20.1",
    "com.unity.render-pipelines.universal": "17.3.0",
    "com.unity.test-framework": "1.4.6",
}

REQUIRED_FILES = [
    "ProjectSettings/ProjectVersion.txt",
    "Packages/manifest.json",
    "Assets/_Project/Settings/Input/PlayerControls.inputactions",
    "Assets/_Project/Scripts/Runtime/WhereTheSoundingsEnd.Runtime.asmdef",
    "Assets/_Project/Scripts/Runtime/Core/WorldClock.cs",
    "Assets/_Project/Scripts/Runtime/Core/Pcg32Random.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameData.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveFileService.cs",
    "Assets/_Project/Scripts/Runtime/Persistence/SaveGameCoordinator.cs",
    "Assets/_Project/Scripts/Runtime/Boat/BoatPrototypeController.cs",
    "Assets/_Project/Scripts/Runtime/Camera/CameraFollow2_5D.cs",
    "Assets/_Project/Scripts/Editor/WhereTheSoundingsEnd.Editor.asmdef",
    "Assets/_Project/Scripts/Editor/ProjectFoundationSetup.cs",
    "Assets/_Project/Scripts/Editor/GreyhavenEnvironmentBuilder.cs",
    "Assets/_Project/Tests/EditMode/WhereTheSoundingsEnd.Tests.EditMode.asmdef",
    "Assets/_Project/Tests/EditMode/CoreFoundationTests.cs",
    "Assets/_Project/Tests/EditMode/SaveFileServiceTests.cs",
    "docs/05_WORLD_AND_LEVEL_DESIGN/GREYHAVEN.md",
]


def load_json(relative_path: str) -> dict:
    path = ROOT / relative_path
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        raise ValueError(f"{relative_path}: {error}") from error


def main() -> int:
    failures: list[str] = []
    for relative_path in REQUIRED_FILES:
        if not (ROOT / relative_path).is_file():
            failures.append(f"Missing required file: {relative_path}")

    version_path = ROOT / "ProjectSettings/ProjectVersion.txt"
    if version_path.exists():
        version_text = version_path.read_text(encoding="utf-8")
        if f"m_EditorVersion: {EXPECTED_EDITOR}" not in version_text:
            failures.append(f"Project editor version is not pinned to {EXPECTED_EDITOR}.")

    try:
        manifest = load_json("Packages/manifest.json")
        dependencies = manifest.get("dependencies", {})
        for package, expected in EXPECTED_PACKAGES.items():
            actual = dependencies.get(package)
            if actual != expected:
                failures.append(f"{package}: expected {expected}, found {actual!r}")

        actions = load_json("Assets/_Project/Settings/Input/PlayerControls.inputactions")
        maps = {item["name"]: item for item in actions.get("maps", [])}
        boat_map = maps.get("Boat")
        if boat_map is None:
            failures.append("PlayerControls.inputactions has no Boat action map.")
        else:
            action_names = {action["name"] for action in boat_map.get("actions", [])}
            expected_actions = {"Move", "Interact", "Fish", "SoundingPulse", "Pause"}
            if not expected_actions.issubset(action_names):
                failures.append("Boat action map is missing one or more required actions.")
            identifiers = [
                item.get("id")
                for item in boat_map.get("actions", []) + boat_map.get("bindings", [])
            ]
            if len(identifiers) != len(set(identifiers)):
                failures.append("Input action/binding identifiers must be unique.")

        for path in ROOT.glob("Assets/_Project/**/*.asmdef"):
            definition = json.loads(path.read_text(encoding="utf-8"))
            if not definition.get("name"):
                failures.append(f"Assembly definition has no name: {path.relative_to(ROOT)}")
    except (ValueError, KeyError, TypeError, json.JSONDecodeError) as error:
        failures.append(str(error))

    gitignore = ROOT / ".gitignore"
    if gitignore.exists():
        text = gitignore.read_text(encoding="utf-8")
        for pattern in ("[Ll]ibrary/", "[Tt]emp/", "[Oo]bj/"):
            if pattern not in text:
                failures.append(f".gitignore is missing {pattern}")

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print("PASS: Unity project scaffold integrity checks completed.")
    print("NOTE: This validates files and configuration only; it does not compile or build with Unity.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

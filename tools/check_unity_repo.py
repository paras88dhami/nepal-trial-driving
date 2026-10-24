# Unity 6 game verification — static source/scene checks only.
# This does NOT compile C# or launch Unity.
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
GAME = ASSETS / "Game"
CAR = ["figure-eight", "u-turn", "hill-start", "traffic-light", "speed-breaker", "garage"]
BIKE = ["figure-eight", "narrow-plank", "u-turn", "traffic-light", "speed-breaker", "ramp", "behavior"]
SCENES = (
    ["NepalTrialMainMenu"]
    + ["CarTrial_" + s for s in CAR]
    + ["BikeTrial_" + v + "_" + s for v in ("motorcycle", "scooter") for s in BIKE]
)
CORE = [
    "Flow/TrialCatalog.cs",
    "Flow/TrialSessionService.cs",
    "Flow/TrialSceneRelay.cs",
    "Flow/TrialMainMenuController.cs",
    "Trial/TrialGameplayRuntime.cs",
    "Vehicles/Car/CarDriveController.cs",
    "Vehicles/TwoWheeler/TwoWheelerRigidbodyController.cs",
    "Editor/Phase4GameSetup.cs",
    "Editor/Phase5BuildPipeline.cs",
    "Tests/EditMode/NepalTrial.EditModeTests.asmdef",
    "Tests/PlayMode/NepalTrial.PlayModeTests.asmdef",
]
ASSEMBLIES = {
    GAME / "NepalTrial.Runtime.asmdef": "NepalTrial.Runtime",
    GAME / "Editor/NepalTrial.Editor.asmdef": "NepalTrial.Editor",
    GAME / "Tests/EditMode/NepalTrial.EditModeTests.asmdef": "NepalTrial.EditModeTests",
    GAME / "Tests/PlayMode/NepalTrial.PlayModeTests.asmdef": "NepalTrial.PlayModeTests",
}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--release-gate", action="store_true",
                        help="Require all Unity-generated scenes and tracked .meta sidecars.")
    args = parser.parse_args()
    errors: list[str] = []
    warnings: list[str] = []

    for rel in CORE:
        if not (GAME / rel).is_file():
            errors.append(f"Required source file missing: Assets/Game/{rel}")

    for path, expected in ASSEMBLIES.items():
        try:
            assembly = json.loads(path.read_text(encoding="utf-8-sig"))
            if assembly.get("name") != expected:
                errors.append(f"{path.relative_to(ROOT)}: unexpected assembly name")
        except (OSError, ValueError) as ex:
            errors.append(f"{path.relative_to(ROOT)}: invalid assembly JSON ({ex})")

    try:
        manifest = json.loads((ROOT / "Packages/manifest.json").read_text(encoding="utf-8"))
        deps = manifest["dependencies"]
        if "com.unity.test-framework" not in deps:
            errors.append("Unity Test Framework package is not declared.")
        if "com.unity.inputsystem" not in deps:
            errors.append("Unity Input System package is not declared.")
    except (OSError, ValueError, KeyError) as ex:
        errors.append(f"Invalid Packages/manifest.json: {ex}")

    cs_files = list(GAME.rglob("*.cs"))
    if len(cs_files) < 60:
        errors.append("Expected Unity gameplay sources appear incomplete.")

    for file in cs_files:
        text = file.read_text(encoding="utf-8-sig")
        # Known unsupported Unity pointer API that prevented compilation previously.
        if re.search(r"\bIPointerCancelHandler\b", text):
            errors.append(f"Unsupported Unity IPointerCancelHandler in {file.relative_to(ROOT)}")
        if text.count("{") != text.count("}"):
            errors.append(f"Brace imbalance in {file.relative_to(ROOT)}")
        meta = file.with_name(file.name + ".meta")
        if not meta.is_file():
            (errors if args.release_gate else warnings).append(
                f"Unity GUID sidecar not committed: {meta.relative_to(ROOT)}"
            )

    absent = [name for name in SCENES if not (ASSETS / "Scenes" / (name + ".unity")).is_file()]
    if absent:
        detail = (f"{len(absent)}/{len(SCENES)} required scene files missing. "
                  "Run Nepal Trial > Phase 4 > Prepare Playable Game locally, commit .unity/.meta files.")
        (errors if args.release_gate else warnings).append(detail)

    if args.release_gate and not absent:
        build = (ROOT / "ProjectSettings/EditorBuildSettings.asset")
        if not build.is_file():
            errors.append("Build Settings asset not committed.")
        else:
            contents = build.read_text(encoding="utf-8")
            for name in SCENES:
                if f"Assets/Scenes/{name}.unity" not in contents:
                    errors.append(f"Scene not registered in Build Settings: {name}")

    print(f"Static source inventory: {len(cs_files)} C# files; {len(SCENES)} expected game scenes.")
    for warning in warnings:
        print("WARNING:", warning)
    for error in errors:
        print("ERROR:", error)
    if errors:
        print(f"FAIL: {len(errors)} source/release gate errors. Unity runtime tests have NOT been run.")
        return 1
    if warnings:
        print(f"PASS: source structure only ({len(warnings)} release-readiness warnings). "
              "Unity tests and Android build are NOT verified.")
    else:
        print("PASS: static source/asset prerequisites only. "
              "Unity tests and Android build require separate evidence.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

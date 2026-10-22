# Phase 1 — Working status and setup

**Status: CODED only — UNITY VALIDATION PENDING.** There is no verified Unity 6 project or playable APK yet.

## Committed prototype
- Assets/Game/Vehicles/Car/CarInputCommand.cs: normalized commands
- CarDriveController.cs: four WheelCollider drivetrain (requires inspector assignment)
- PrototypeCarInput.cs: desktop keyboard fallback and methods for mobile UI bindings
- VehicleFollowCamera.cs: basic chase camera
- ProceduralFigureEight.cs: illustrative circle guide lines; official figure-8 shape and sizes are NOT implemented
- EditMode test source: input clamping

## Steps to make a genuine Unity project
1. Install the Unity 6 LTS Editor, Android Build Support and Unity Personal license.
2. Create a **Universal 3D (URP)** project with Unity Hub; preserve generated ProjectSettings, Packages, Assets and all .meta files.
3. Integrate the committed Assets/Game scripts and reconcile the template's actual Packages/manifest.json with the starter file in this branch; do not blindly replace compatible packages.
4. Configure the Input System in Player Settings; verify compatible URP, Input System and Test Framework versions in Package Manager.
5. Build a simple car prefab with Rigidbody, four oriented WheelColliders, low-poly body and inspector assignments.
6. Attach PrototypeCarInput and VehicleFollowCamera. Add a floor collider. Generate approximate guide lines using ProceduralFigureEight.
7. Add real checkpoint/boundary track meshes and validator logic; the current lines have no colliders and are not a completed trial.
8. Run a Unity editor import/compile and execute EditMode tests; fix errors, commit generated metadata.
9. Configure Android landscape orientation and test an actual APK on hardware.

## Current blockers
No verified Unity editor version, scene/prefab, URP pipeline asset, android build, mobile UI, real checkpoints, official measurements or real device tests. The package versions listed are provisional until checked against the chosen Unity Editor.

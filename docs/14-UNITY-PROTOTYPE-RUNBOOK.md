# Prototype Build Runbook

## Current status
**CODED ONLY. Unity import, C# compilation, gameplay validation and Android build HAVE NOT been performed.** PR #1 stays draft. This branch has scripts and an Editor scene generator, not a committed Unity-generated scene or project settings.

## Unity setup
1. Install a supported Unity 6 LTS Editor via Unity Hub with Android Build Support, SDK/NDK and OpenJDK. Activate Unity Personal or other valid license.
2. Create a **Universal 3D (URP)** project. Record the actual Unity version in `ProjectSettings/ProjectVersion.txt`.
3. Merge this feature branch's `Assets/Game` into the Unity project. Keep the URP template's verified `Packages/manifest.json` and project settings; compare rather than replacing it with an unverified starter manifest.
4. Ensure Input System and uGUI modules are installed/compatible; use active input handling Input System (or Both). Unity's Input System UI Input Module is required.
5. Let Unity generate all missing `.meta` files and scene YAML. Save and commit those files along with the genuine Unity project.

## Create playable prototype scene
1. Wait for Unity scripts to compile; fix reported errors.
2. Menu: **Nepal Trial → Create Prototype Scene**.
3. Open `Assets/Scenes/PrototypeCar.unity` if needed and press Play.
4. On desktop, W accelerates, S brakes, A/D steer, R toggles reverse while held, Space handbrake. On mobile use Left/Right/Gas/Brake touch surfaces.
5. The initial guide comprises two circles, NOT a verified Nepal figure-8. There are no assigned boundary colliders or official scoring; prototype boundary components exist but require trigger setup.
6. Review the car collider, four wheel positions, suspension, physical stability and camera and tune in Unity.

## Acceptance checklist
- [ ] Clean Unity import and zero compiler errors.
- [ ] Menu generator creates scene and valid prefab/component references.
- [ ] Vehicle drives forward, steers, brakes, reverses and stops without unstable bouncing.
- [ ] Follow camera tracks smoothly.
- [ ] Touch controls work simultaneously and reliably on device (multi-touch steering requires further work).
- [ ] Generate validated trial boundary geometry, wheel-contact triggers, checkpoints and results UI.
- [ ] Unity EditMode tests pass.
- [ ] Android build succeeds and runs on device with measured FPS.
- [ ] Verify official track dimensions, scoring and local jurisdiction before marking exam-accurate.

## Known limitations
The starter `Packages/manifest.json` is provisional and incomplete for an actual URP template. WheelColliders may need stiffness/suspension adjustments. Prototype boundary detector uses a wheel-collider trigger tag which is not integrated with physical WheelCollider tyre contact, so it does NOT yet score line crossings. Touch left/right buttons share one steering axis and need multi-touch reconciliation. The UI module and built-in font availability must be verified with the exact Unity Editor version.

Update `docs/06-IMPLEMENTATION-PLAN.md` and `docs/11-DEVELOPMENT-LOG.md` with real evidence at each validation step; do not mark as completed without a passing run.

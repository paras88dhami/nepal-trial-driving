# 02 — Technical Requirements Document (TRD)

**Status:** Proposed technical baseline; exact package versions to be pinned after Unity verification.

## Technology
- Unity 6 LTS editor, exact version recorded in ProjectSettings/ProjectVersion.txt.
- C# gameplay and tooling; Universal Render Pipeline (URP) for mobile 3D.
- Unity Rigidbody/colliders; WheelCollider or reviewed free car controller; custom motorcycle lean and balance.
- Unity Input System for touch and desktop debug controls.
- Cinemachine compatible version for chase camera.
- uGUI Canvas for responsive HUD; avoid inconsistent mixing of UI frameworks.
- Procedural meshes or ProBuilder for trial geometry in real-world meters.
- Local JSON in Application.persistentDataPath; PlayerPrefs only for simple preferences.
- GitHub + Unity .gitignore, visible .meta files and YAML text serialization.
- Unity Test Framework edit-mode unit tests and play-mode tests; Android smoke tests.
- No backend, login, analytics, network permissions or paid assets without approval.

## Initial performance budgets
Landscape UI with safe-area handling; target 30 FPS on defined lower-end phone and 60 FPS on defined mid-range phone (targets only). Optimize mobile shadows, meshes, textures and draw calls after profiling. Start physics timestep around 0.02 seconds, tune with evidence.

## Build environment
Unity Hub, Unity Editor, Android Build Support, SDK/NDK/OpenJDK, Git, optional Blender and a C# IDE. Build debug APK for tests and AAB for Play release. Unity Cloud ID is optional for local development.

## Package intake
For every package log source URL, license, version, Unity compatibility, commercial use, redistribution rights and fallback. Avoid incompatible or unverified package versions. No purchased packages without the owner's permission.

## Verification gates
1. File/config validation.
2. Real Unity import, compile and edit-mode tests.
3. Scene play-mode tests.
4. APK build and hardware smoke tests.
5. Device FPS, memory, thermals and control validation.

Missing Unity license/editor/toolchain must be reported as a blocker; never fabricate test results.

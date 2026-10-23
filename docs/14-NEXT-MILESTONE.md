# Next milestone — Generated car practice scene

**2026-10-22 · CODED / UNCOMPILED**

Continues draft PR #1 on `feature/unity-foundation-car`.

Added:
- `Assets/Editor/TrialPrototypeBuilder.cs`: Unity menu item **Nepal Trial → Build Car Practice Scene**; generates a gray floor, placeholder vehicle body with assigned WheelColliders, basic lighting, camera, keyboard input, and approximate figure-8 markings; saves `Assets/Scenes/CarPractice.unity` **when actually run in Unity**.
- `Assets/Game/Input/MobileHoldPedal.cs`: touch press/release bridge for accelerator, brake, steering and handbrake. Touch Canvas buttons/panels and EventSystem wiring remain required.

### How to run
1. Open the repository as a genuine Unity 6 LTS URP project and resolve packages in Package Manager. Unity project creation/version has NOT been validated.
2. Ensure **Input System** is installed and Active Input Handling includes new Input System; run **Nepal Trial → Build Car Practice Scene** from the Unity editor menu.
3. Press Play; keyboard W/S (throttle/brake), A/D (steering), R (reverse), Space (handbrake). Attach UI graphics with MobileHoldPedal for touch inputs.
4. Test wheel tuning, collider interactions, camera, track alignment, Android build. Commit Unity-generated `.meta`, scene and project settings after successful import.

### Known limitations
The course is only two guide circles, not a verified official trial figure-8 and not a scored track. No validated checkpoints, pass/fail, touch Canvas, polished assets, debug build, or verified official dimensions yet. Tool-generated Unity assets require actual editor validation before merging PR #1.

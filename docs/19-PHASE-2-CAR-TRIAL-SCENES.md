# Phase 2 — Car trials: generated playable practice scenes (2026-10-24)

> **IMPLEMENTED IN CODE, NOT YET VERIFIED IN UNITY.** Track sizes and penalty values are prototype choices, **not confirmed official Nepal licensing trial requirements**.

## Build the six scenes

1. Update your local project: `git switch dev` and `git pull --ff-only origin dev` (only with a clean worktree).
2. Open the project in **Unity 6000.6.7f1**; let it import code/packages and inspect the **Console** first.
3. Open the Unity top menu **Nepal Trial → Phase 2 → Create Car Trial Scenes**.
4. The command creates the following scene files under `Assets/Scenes` **only if each does not already exist**:
   - `CarTrial_figure-eight.unity`
   - `CarTrial_u-turn.unity`
   - `CarTrial_hill-start.unity`
   - `CarTrial_traffic-light.unity`
   - `CarTrial_speed-breaker.unity`
   - `CarTrial_garage.unity`
5. Open each scene and press **Play**. The generated car has a Rigidbody/WheelColliders, touch input, following camera, course sensor, score, countdown, results, and replay action.
6. Run **Nepal Trial → Phase 2 → Audit Car Trial Scenes** for a static missing-reference check. Passing this audit does **not** mean the physics or exams work.
7. Use **Window → General → Test Runner → EditMode → Run All** to run NUnit logic tests. Confirm PlayMode and Android touch behavior manually.

The builder saves reusable URP materials to `Assets/Game/Generated/Materials`. After creating scenes, review and commit the resulting `.unity`, `.mat` and `.meta` files before expecting the same generated layouts on another machine. Scene creation is intentionally local (code committed; generated Unity scene assets are not yet committed remotely).

## Prototype stage behavior and course goals

| Scene | What physically triggers completion | Failure/violation observation |
|---|---|---|
| Figure-eight | Drive through **10 sequential forward-facing** loop gates | Wheel outside both marked rings after beginning course |
| U-turn | Cross entry volume, accumulate **net** 140° heading change, cross return exit | Leaves driving-region bounds, fails turn or times out |
| Hill start | Cross start, drive up incline and cross exit | Rolls back more than 0.5m from highest point or times out |
| Traffic light | Cross sensor's stop line while light is green | Crossing on red or yellow |
| Speed breaker | Enter breaker volume at or under provisional 12km/h | Cross too fast |
| Garage | Hold whole car within parking volume for 2 seconds at near-zero speed | Hits obstacle, or never fully parks before timer |

**Scoring defaults:** 100 points, 70 needed to pass, 180s total, provisional penalties configured on each trial component. Thresholds can be adjusted in the inspector. No claim that these match Nepal's official point allocation, vehicle categories, measurements or legal licensing standards.

## Controls

- **W / Up** throttle; **S / Down** brake; **A/D** or arrows steer.
- **R** toggles reverse; **G** Drive; **N** Neutral; **P** Park; **Space** parking brake.
- **T** resets the car; **F5** resets the full trial; **Escape** toggles pause.
- Touch buttons: steer, throttle, brake, reverse, Drive, Neutral, Park, handbrake, Pause and Restart.

Reverse/Park/Drive shifts require near-zero speed and brake interlock according to the code. The HUD displays the currently engaged gear (separate from selected gear).

## Tests and known limitations

- NUnit test cases authored for ordered directional gates, boundaries, U-turn net rotation, hill rollback and six independent trial-result sequences; **not run**.
- Generated tracks use colored Unity primitives and approximate geometry, not high-quality models or an official CAD layout.
- Trigger order, WheelCollider suspension/grip, ramp transition and UI scaling **must be checked in Unity PlayMode**.
- Inspect actual car dimensions; the garage checks BoxCollider corners, which only approximates rendered body geometry.
- Check scene and collision-mask interactions, reverse behavior, and score persistence after returning to Play.
- The generated six scenes are separate practice sessions rather than a single continuous official exam circuit. Unifying them is part of Phase 4.
- Any real licensing exam rulebook should be externally verified before an official-realism claim or release.

## Exit criterion for Phase 2

**Not met yet**: the six generated scenes must exist as versioned Unity assets, pass compile/static scene audit/NUnit tests, and complete/fail correctly in PlayMode with documented QA evidence. Until then Phase 2 is **code-integrated, unverified**.

# Nepal Driving Trial 3D — Five-phase development and acceptance plan

Updated 2026-10-24. Source of truth: Unity 6.6 URP project on `dev`.

**Policy:** Code present is not the same as tested. Merge of PR #5 was requested before local validation. All code on `dev` remains **unverified** until Unity compiles, EditMode/PlayMode tests run, and an Android build is exercised.

## Phase 1 — Car physics + input integration
- Drive/Reverse/Park/Neutral state and safe shifts; accelerator and brake, rear parking brake.
- Editor keyboard + multitouch; on-screen gear toggle and handbrake; reset of position/velocity and wheel torques.
- Steering speed sensitivity, suspension defaults, tire grip tuning, collision feedback.
- NUnit calculation/state cases; Unity PlayMode driving test on flat road and slope.
- Acceptance: Drive, reverse after stop, brake, park, reset, and steering work in Android touch and Unity keyboard modes without console errors.

## Phase 2 — All car trial courses
- Figure-8 and U-turn: start/finish gates, travel direction, proper boundary incidents.
- Hill start: rollback, safe stop, stall/timeout (if applicable); traffic signal: stop-line crossing; breaker: crossing speed; garage: full body inside, hold time, obstacle collision.
- All validators driven by real Unity sensor/trigger observations, not placeholder checks.
- Course stages connected to score/penalties, result flow and restart.
- Acceptance: each course has scene/prefab wiring, clear completion and failure cases; official dimensions/rules must be verified externally and versioned rather than guessed.

## Phase 3 — Motorcycle, scooter and all two-wheeler trials
- Physical wheel contact and suspension, yaw/lean stability, ground braking, engine, shifting/clutch for motorcycle, automatic scooter.
- Bike/scooter figure-8, narrow plank, fall/foot-down, direction and boundary checks; any additional official course stages after confirmation.
- Acceptance: both vehicles controllable on target Android hardware and finish playable trial sequences.

## Phase 4 — End-to-end game flow and persistence
- Vehicle catalog/spawn, garage selections, mode/course selection; spawn/respawn.
- Home, practice, exam, HUD, pause/retry and results scenes; connect progress/scores and settings to Unity UI.
- Offline save migration, error handling and reliable restore/retry.
- Acceptance: user can enter a vehicle, complete a trial, see a result, restart, quit and reopen with progress intact.

## Phase 5 — Test, fix and release gate
- EditMode deterministic logic tests, PlayMode integration tests (Unity physics + trigger rules), scene/prefab verification, Android touch smoke tests.
- Performance profiling on low-/mid-range Android phones; URP material/lighting and asset licensing audit; build and packaging.
- Acceptance: successful Unity compilation, no blocking test failures, repeatable on-device run; record actual commands, devices, outcomes and known defects. Do **not** mark a phase verified without evidence.

## Immediate work log
- PR #5 merged into `dev` at commit `653c395184f91d0207a7618b3286b1359c97323f` (unverified foundation).
- Phase 1 implementation continues on `dev` at the user's request. Manual Unity testing still pending.

## Phase statuses (2026-10-24)
- Phase 1: IMPLEMENTATION IN PROGRESS — physical Drive/Reverse/Neutral/Park commands, Park brake on all wheels, hold/release handbrake, touchscreen and keyboard, vehicle reset, speed-adaptive steering, motor torque safety, gear indicator and NUnit cases on dev. Actual suspension tuning, physics validation and real asset wheel setup remain.
- Phase 2: SCENE GENERATION IMPLEMENTED / UNITY VALIDATION PENDING — six independent practice car trials can be created through Unity's Nepal Trial > Phase 2 menu, with trial controller, car, checkpoints, scoring, pause/retry/results, reset-aware sensors, traffic light cycle, prototype geometry, and a static scene audit. See docs/19-PHASE-2-CAR-TRIAL-SCENES.md. Generated .unity and material assets still need creation/commit in Unity; code has NOT been compiled or played, and official track rules remain unverified.
- Phase 3: FULL PRACTICE-SCENE IMPLEMENTATION CODED / UNITY VALIDATION PENDING — motorcycle/scooter front+rear WheelColliders, independent brake/steering and balance assistance, clutch/gear engine stalls and ignition, keyboard/touch UI, 7 A/K course sensors, a generator for 14 independent Unity scenes, scene audit and NUnit cases. The generated .unity/.mat/.meta assets have not been created or committed, nor have Unity tests or mobile ride tests run. See docs/20-PHASE-3-TWO-WHEELERS.md. This is not a verified or physically realistic production simulation.
- Phase 4: SCENE-ROUTING AND UI IMPLEMENTATION CODED / UNITY VALIDATION PENDING — generated menu/garage UI, 3 starter vehicles, Practice/Exam/Free Drive, stage-by-stage scene routing for 6 car or 7 A/K trials, offline session resume, aggregated exam results with recovery-safe run IDs, gear-specific scene vehicle selection, settings integration and a one-click Editor command for registering 21 scenes. NUnit cases and Build Settings audit authored. Actual Unity-generated scene assets and interactive QA remain outstanding. See docs/21-PHASE-4-GAME-FLOW.md.
- Phase 5: NOT RUN — Unity 6.6 Test Framework 1.8.0 dependency added to manifest; automated and physical Android tests still must run in Unity.

## Local validation steps (when ready)
1. Back up or commit local changes. `git switch dev && git pull --ff-only origin dev` from `C:\\Users\\kapil\\nepal-trial-driving`.
2. Open that folder in Unity Hub with editor `6000.6.7f1`; allow Package Manager to resolve `com.unity.test-framework` and update `Packages/packages-lock.json`.
3. Open the existing prototype scene, click `Nepal Trial > Upgrade Current Car Controls` (not Create) and save. Check the Console for compile/asset errors first.
4. Test keyboard W/S/A/D or arrows; R reverse, P park, N neutral, G drive, Space handbrake toggle, T vehicle reset, plus mobile touch controls. A shift may be pending until the car stops.
5. Open `Window > General > Test Runner`, run EditMode tests and record the actual pass/fail report. Complete PlayMode physics and Android touchscreen tests on a device.
6. Commit Unity-generated `.meta` and package lock changes only after inspecting them. Never copy `Library`, `Temp`, `Logs` into GitHub.

**Do not mark any phase completed/verified on the strength of code commits alone.**

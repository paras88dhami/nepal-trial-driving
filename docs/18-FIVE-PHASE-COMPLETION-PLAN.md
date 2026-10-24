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

## Phase statuses
- Phase 1: IN PROGRESS — adding working controls, reset and tests.
- Phases 2–4: PARTIALLY CODED — validators and game state foundations exist, full physical scene integration pending.
- Phase 5: NOT RUN — Unity environment resides on the user's PC.

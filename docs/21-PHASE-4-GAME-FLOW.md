# Phase 4 — Menu, garage, game modes, exam campaign, results and offline progress

**Code status: implemented on `dev`, NOT compiled or PlayMode/Android tested yet.**

## Prepare all 21 scenes with one Editor command

1. Back up any Unity scene you edited and commit/stash local changes before pulling.
2. In a terminal, open the repo and run `git switch dev` then `git pull --ff-only origin dev`.
3. Open the project in Unity Hub, using **Unity 6000.6.7f1**. Wait for Package Manager and script import, and fix Console compile errors **before** generating scenes.
4. Choose **Nepal Trial → Phase 4 → Prepare Playable Game**.
   - Preserves existing `.unity` files if present.
   - Runs the Phase 2 six-car-course generator and the Phase 3 fourteen-two-wheeler-course generator.
   - Creates `Assets/Scenes/NepalTrialMainMenu.unity`, if missing.
   - Registers Main Menu first and up to **20** trial scenes in **File → Build Profiles → Scene List** / Build Settings, preserving other registered scene entries.
5. Choose **Nepal Trial → Phase 4 → Audit Menu and Build Settings**. Resolve any missing/disabled scenes. The Phase 2 and Phase 3 audit menu items can additionally check individual scene wiring.
6. Open `Assets/Scenes/NepalTrialMainMenu.unity` and enter Play Mode.

**GitHub does not yet contain the generated binary/text scene assets**. You must generate them in Unity and commit reviewed `.unity`, `.mat`, `.meta`, and serialized editor scene/build configuration changes before another computer can open this exact assembled game. Never push `Library/`, `Temp/`, or `Logs/`.

## End-to-end player flow

Home → Garage (Car, Motorcycle, Scooter; three starter vehicle IDs unlocked) → Mode selection → Course selection for Practice/Free Drive OR the first trial for Exam.

| Mode | Behavior | Offline record |
|---|---|---|
| **Practice** | Exactly one selected course with its existing timer, course validator and in-scene results; retry and Home | Individual trial runtime records one finished attempt |
| **Exam** | Car: 6 stages; motorcycle/scooter: 7 stages in order; when a stage passes tap **Next Stage** to load the next; failing a stage or accumulating >30 total penalty points ends the session | One aggregate record under `exam-starter-<vehicle>` per finished exam (using a run ID to avoid repeated persistence during crash recovery) |
| **Free Drive** | Selected course scene used for unrestricted vehicle practice; exam runtime remains in Ready, stage scoring and time penalties disabled | No attempt/score record |

The above pass/fail thresholds and scores are **demonstration gameplay values**, not verified official Nepal licensing rules.

The `TrialSceneRelay` attaches automatically through `SceneManager.sceneLoaded` to car/two-wheeler trial scenes, and adds **Home**, **Next Stage**, plus a status label to the existing HUD. Existing scene scripts do not require manually adding relay references.

## Progress / resume / results

- `TrialSessionService` serializes vehicle ID, game mode, selected courses, next stage index, total penalty points, per-stage pending result, score and a run ID into `PlayerPrefs`.
- An unfinished session offers **Resume** on the Home screen, including after app restart.
- Finishing an exam aggregates the stage penalties and stores one final run record in `LocalProgressData`. Practice results are saved by their existing scene runtime. Free Drive never adds scores.
- Restarting an unfinished stage discards its **uncommitted** stage result and reruns that stage; restarting a completed exam begins a new run at stage one. Returning Home from an unfinished run keeps the Resume option.
- All storage is **device-local and editable**, not secure, and may be cleared when uninstalling the application. No cloud sync or login exists.
- The Home screen shows total saved attempts/passes and the last session score.

## Settings and their integration

- Steering sensitivity loads into car and motorcycle/scooter input components whenever the gameplay scene starts.
- Sound volume is applied to `AudioListener.volume`.
- Graphics tier is mapped to an available Unity quality level.
- Vibration is applied on supported Android devices when a violation is accepted and the setting is enabled.
- Values are stored locally with `LocalSettingsStore`; changes apply when scenes next load.

## Tests and required checks

New NUnit EditMode tests cover vehicle/course catalog, starter unlocks, menu state transitions, practice/free-drive/exam modes, exam ordering, aggregate penalties, failed stage, retry, and avoiding duplicate saved exam attempts. **These tests have not been run.**

When Unity compiles, run **Window → General → Test Runner → EditMode → Run All**, then perform PlayMode/Android checks:

1. Generate and audit 21 scenes; launch **Main Menu** from build profile.
2. Choose all 3 vehicle categories; test Practice and Free Drive (score timer inactive in Free Drive).
3. Test a full car exam (6 scenes) and motorcycle/scooter exam (7 scenes), Next Stage, failures, and exact total penalty behavior.
4. Tap Home in mid-exam; confirm Resume brings you back to the correct stage. Quit/relaunch the app and repeat.
5. Finish an exam; return Home; verify exactly one new exam record, and that retry starts at first stage.
6. Change steering/graphics/audio/vibration settings, restart, and test the actual effect.
7. Run Android build with touch controls and inspect performance/camera/UI sizing for mobile.

**Phase 4 does not pass verification until every course scene loads and gameplay, persistence, menu, and UI behavior have been observed to work with no blocking errors.** Phase 5 will include this full Unity test/fix cycle.

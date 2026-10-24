# Phase 5 — Unity verification, bug fixes, Android build and release gate

Updated 2026-10-24. **Status: phase tooling and test cases authored; full testing NOT run.**

## Current evidence (do not confuse these with gameplay tests)

| Gate | Actual evidence | Verdict |
|---|---|---|
| GitHub repository source checks | Workflow `Unity Source Gate (not runtime tests)` on `dev` | Passing source structure checks as of phase 5 setup |
| Unity 6000.6.7f1 script compilation | Unity Editor not available in GitHub connector execution environment | **NOT RUN** |
| EditMode NUnit tests | Test code written and batchmode script supplied | **NOT RUN** |
| PlayMode tests / physics | Added `Assets/Game/Tests/PlayMode` including car/scooter smoke tests | **NOT RUN** |
| 21 assembled game scenes | Only 3 older scenes currently tracked in GitHub; Phase 2/3/4 generators produce the rest when run locally | **BLOCKED** |
| Android development APK | Editor build method and Windows script supplied; no Android Build Support in this runtime | **NOT RUN** |
| Real Android touch/controller QA | Requires Android device and a built APK | **NOT RUN** |
| Official trial rules and geometry | All dimensions and penalties remain provisional | **UNVERIFIED** |

## First compile + scene preparation (Windows)

1. Ensure no work is lost in your local Unity scene. Commit or stash local changes.
2. In PowerShell, from `C:\Users\kapil\nepal-trial-driving`, run:
   ```powershell
   git switch dev
   git pull --ff-only origin dev
   python tools/check_unity_repo.py
   ```
3. Open the project in Unity Hub **6000.6.7f1**, allow Package Manager import, and check **Window > General > Console**. Fix every compiler error before trying PlayMode or builds. The old `.meta` sidecars for new GitHub-created scripts are currently missing; Unity should generate them on import.
4. In Unity top menu, choose **Nepal Trial > Phase 4 > Prepare Playable Game**. This will generate the 6 car + 14 bike/scooter trial scenes plus 1 main menu, preserving any existing scene files.
5. Then run:
   - **Nepal Trial > Phase 2 > Audit Car Trial Scenes**
   - **Nepal Trial > Phase 3 > Audit Motorcycle and Scooter Trials**
   - **Nepal Trial > Phase 4 > Audit Menu and Build Settings**
   - **Nepal Trial > Phase 5 > Validate Build Prerequisites**
6. **Save/commit the `.unity`, `.mat`, `.meta` assets and updated ProjectSettings scene list**. GitHub cannot access locally generated Unity scene files until pushed.
7. Run strict prerequisite check:
   ```powershell
   python tools/check_unity_repo.py --release-gate
   ```
   This intentionally **fails before generated scenes and tracked GUID sidecars exist**.

## EditMode and PlayMode testing

Use Unity Test Runner: **Window > General > Test Runner > EditMode > Run All** and then **PlayMode > Run All**.

Or in PowerShell at the repository root:

```powershell
.\scripts\run-unity-tests.ps1
```

If Unity Hub installed in another location:

```powershell
.\scripts\run-unity-tests.ps1 -UnityExe "D:\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
```

The script creates logs and NUnit XML under `Build\QA\`, rejects missing/empty test reports, fails for test failures/inconclusive tests, and stops if the Unity CLI exits unsuccessfully. PlayMode tests include **trial lifecycle pause/restart**, **car rear-wheel motor torque and reset**, and **automatic scooter drive torque**.

**Do not report tests as passed until you have inspected actual Test Runner results or the XML reports and Unity logs.** Attach the relevant error output to the development chat when a test fails.

## Android development build

Install **Android Build Support**, **Android SDK/NDK Tools**, and **OpenJDK** for the matching Unity 6 Editor through Unity Hub. Verify menu scene at build index 0 and generated trial scenes included.

Then run (this refuses to build if prerequisites aren't met):

```powershell
.\scripts\build-android-dev.ps1
```

APK target: `Build\Android\NepalDrivingTrial-dev.apk`. It is a **development build**, not a signed Play Store release, and will not be written into the repository (`Build/` is ignored).

## Manual regression matrix — mark only after observation

| ID | Test procedure | Expected result |
|---|---|---|
| QA-01 | Launch menu, enter each starter vehicle | All 3 spawn with corresponding controls and correct camera |
| QA-02 | Car Drive/Park/Neutral/Reverse, hold brake then shift | Correct gear state, Park holds, reverse works after stopping |
| QA-03 | Car accelerate, steer, brake, handbrake, reset | Predictable response, no crash; reset position and speed |
| QA-04 | Figure-eight both directions and leave/reenter lines | Correct ordered gates and one penalty per excursion |
| QA-05 | U-turn enter, turn, leave boundaries, exit | Completion only for valid direction/angle; penalties accurate |
| QA-06 | Hill start: stop, roll back, proceed | Slope stability, rollback threshold; stage completes |
| QA-07 | Red/green signal, slow/fast breaker, garage | Correct stage completion/failure, no double-penalties |
| QA-08 | Motorcycle clutch/first gear, engine stall, ignition | Gear and engine drive behavior; correct handbrake equivalents |
| QA-09 | Scooter throttle, separate brakes, stability | Automatic motion, no manual gears required |
| QA-10 | Bike plank foot-down and fall, U-turn signal | Invalid events penalized once; valid attempts pass |
| QA-11 | Bike stop on ramp, crest and descend | Physics handles connected slopes; no false rollback |
| QA-12 | Seven A/K behavioral and traffic/bump stages | Trigger UI matches exam rules and scorer |
| QA-13 | Exam 6 car stages and 7 bike/scooter stages | One aggregate result; Next Stage/Replay/Home work |
| QA-14 | Force close/reopen mid-exam, resume after completed stage | Resumes the *next unplayed* stage and no double save |
| QA-15 | Settings sound, steering, vibration, graphics; relaunch | Values persist and affect actual game behavior |
| QA-16 | Android landscape touchscreen and low-midrange FPS | Buttons accept simultaneous touches; no blocking UI overlap |

Test on at least one low-end and one midrange Android device, with touch input, battery/performance/thermals and application lifecycle (minimize, rotate, reboot).

## Release exit criteria

Do not call Phase 5 or the game complete until:
- All required assets are committed and Unity compilation succeeds.
- Every EditMode and PlayMode test passes; logs are reviewed.
- All 21 scenes audited and manually exercised.
- All QA-01 through QA-16 cases have repeatable PASS evidence or explicit fixes.
- Android APK builds and installs, and the Play Store signing/art/permission checklist is addressed.
- Provincial official licensing specs are verified before claiming legal exam accuracy.

GitHub source gate being green is **not** a release approval. Physics, mobile QA, scene generation and publish readiness remain user-editor/device dependent.

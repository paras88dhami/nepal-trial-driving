# Nepal Connected Trial Grounds — Car, Motorcycle and Scooter

**Code status:** committed to `dev`. **Unity compilation, generated-scene audits,
PlayMode physics and Android testing have NOT run for the latest changes.**

Practice and Free Drive still use separate course scenes. **Exam** uses one
connected scene, one player vehicle, one 100-point score and seven stages.

## Included exam stages

| Car B | Motorcycle/Scooter A/K |
|---|---|
| 1 Figure-8 | 1 Figure-8 |
| 2 Speed breakers | 2 Narrow plank |
| 3 U-turn | 3 U-turn |
| 4 Traffic lights | 4 Traffic lights |
| 5 Uphill/downhill ramp | 5 Speed breakers |
| 6 Garage L-back | 6 Uphill/downhill ramp |
| 7 Behavioral assessment | 7 Behavioral assessment |

**Marking** follows a configurable 100-point, 70-to-pass model; violations
have point deductions OR immediate failure. Repeated line touches/foot-down
and missed ramp stops are handled separately. Behavioral check is a manual
teaching checklist; it is not auto-detected through physics.

## Ground geometry currently implemented in the connected-scene generator

| Module | Generator reference geometry |
|---|---|
| Car Figure-8 | 12m outer diameter, 6m inner diameter, 9m between centers |
| Bike/scooter Figure-8 | 3m outer radius, 1.5m inner radius, 4.5m between centers |
| Bike narrow plank | 10m long, 0.5m wide; wheel-contact validator uses a 7.5m travel target |
| Bike curved U-turn | 10.4m outside span, 5.6m inside diameter; curved boundary detection |
| Bike ramp | 6m up, 3m flat, 6m down, 3m wide; uphill/downhill stops |
| Car ramp | 8.5m up, 8m flat, 8.5m down, 5m wide; uphill/downhill stops |
| Car L-back | 20m by 3m approach and 8.5m by 3m perpendicular lane, reverse requirement, tire limits and forward-correction scoring |
| Speed breakers | Separate multi-bump generated lanes for car and two-wheeler |

A **1.5m curved L-back junction and exact official adjacency are not yet
modeled**. The connected generator spaces stages 70m apart along a straight
connecting pavement. Therefore **this is not a replica of an entire
government examination ground**. Final geometry, vehicle wheelbase and physical
drivability require local editing and verification.

Car connected Exam has **opt-in manual clutch/stall training**: hold Shift to
disengage clutch, use W to accelerate, press I to restart engine. Touch-screen
Clutch and Ignition buttons are added by the car indicator script. The clutch
simulation is intentionally simplified. Separate Practice car handling remains
automatic. Car turn signals: Z = left, X = right. The score catalog handles
Figure-8 stalls, first/repeated speed-breaker stalls, first/repeated ramp stalls,
and wrong/missing U-turn indicators.

## Get changes into Unity (Windows)

**Save your local scene changes and stop Play Mode before pulling.**

1. Close Unity Editor (Unity Hub may remain open).
2. In PowerShell from `C:\Users\kapil\nepal-trial-driving`:
   ```powershell
   git status
   git pull origin dev
   ```
   If Git refuses because your local Unity-generated scenes are modified, do
   **not** discard them; preserve them in a commit or backup before merging.
3. Reopen Unity 6000.6.7f1 and wait for all scripts to compile. If red Console
   errors remain, stop and share **the first red error**.
4. In Unity choose **Nepal Trial → Connected Exam →
   Rebuild Connected Grounds (Back Up Existing)**.
   - This regenerates `NepalTrial_FullCar.unity`,
     `NepalTrial_FullMotorcycle.unity` and `NepalTrial_FullScooter.unity`.
   - It first copies previous connected scene files to
     `Build/ConnectedSceneBackups/<timestamp>/`.
   - It does not rebuild or delete the original 21 Practice scenes.
   - Do not run this while you have unsaved edits to a connected scene; save them
     first. Review generated files before committing them.
5. Choose **Nepal Trial → Connected Exam →
   Audit All Three Connected Grounds**.
6. Open `Assets/Scenes/NepalTrialMainMenu.unity` and press **Play**.
   Test **Car → Exam**, then Motorcycle → Exam, Scooter → Exam.
7. Run **Window → General → Test Runner**; run EditMode, then PlayMode,
   and inspect failures. Or use `./scripts/run-unity-tests.ps1`.
8. When everything is verified, commit the generated `.unity`, `.meta`,
   `.mat` and changed `ProjectSettings/EditorBuildSettings.asset`.

## Current verification/limitations

- **Source structure GitHub workflow:** may pass; does **not** compile scripts.
- **Automated NUnit tests:** written, not yet run in Unity for new changes.
- **Generated connected scenes:** must be rebuilt locally to include changes.
- **Vehicle handling:** WheelColliders, turn detection, garage reverse paths,
  throttle/clutch response, downhill ramp joins and all markers must be driven
  in PlayMode and on Android.
- **Time limit:** currently a provisional 1,200-second whole-exam limit, not a
  confirmed licensing rule.
- **Marks:** category-specific rules are defined and partially sensor-connected;
  physical detection still needs tuning at the game site.
- **Exam resume:** checkpoints save after a completed stage. On resume,
  the vehicle is repositioned at the next station (rather than restoring
  arbitrary mid-stage positions), and stage-local sensor counters reset.
  This recovery needs actual PlayMode test evidence.
- **Official accuracy:** compare government spot dimensions and the latest
  province-specific directive before publishing as a realistic licensed-exam
  simulation.

Government references:
- https://hetaudayatayat.bagamati.gov.np/np/download-traffic-law/19
- The published 2074/2077 trial training spot annex drawings.

**Do not label this game production-ready or officially certified** based on
successful generation, static source checks or old Practice screenshots.

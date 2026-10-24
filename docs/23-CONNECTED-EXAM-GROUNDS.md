# Connected Nepal Trial Exam Grounds — implementation checkpoint

**Status: new generator and scoring flow committed to `dev`. Not compiled or played in Unity yet.**
Existing single-stage Practice / Free Drive scenes remain unchanged and are intentionally retained.

## What has been implemented

- Dedicated **one-scene connected exam** per category: `NepalTrial_FullCar`,
  `NepalTrial_FullMotorcycle` and `NepalTrial_FullScooter`.
- A Unity Editor generator imports the six existing obstacle/course scene modules
  into one large ground, retains one player vehicle/camera/HUD/TrialGameplayRuntime,
  remaps scene sensors to that one trial runtime, and adds connecting pavement.
  **The links, road geometry and car L-back shape are PROVISIONAL**, not government CAD.
- Figure-8 loops in the connected course are rebuilt using published Bagmati
  reference measurements. Car B: outer diameter 12m, inner diameter 6m,
  center separation 9m. A/K: outer radius 3.5m, inner radius 1.5m,
  center separation 4.5m. **Trigger gate approach/vehicle path still requires PlayMode testing.**
- Seven ordered score stages for both categories, including B behavioral assessment.
- One 100-point exam score shared across the entire scene, not a fresh 100 per
  obstacle. A/K and B use category-specific violation rule catalogs.
- Explicit immediate-failure and repeated-violation thresholds.
- Car garage in the connected exam requires selecting Reverse, and reports forward
  corrections (-10 then disqualification); it is **NOT YET** a geometrically correct
  L-back maneuver, and the marking lines/path require more work.
- Connected car and bike ramp sensors use 0.1524m (six-inch) rollback tolerance.
- A manual behavior checklist provides three checks, deductions 3/4/3, for learning.
- Menu **Exam** mode enters the connected scene. Practice / Free Drive still use
  the original one-obstacle scenes. A finished connected exam saves one overall
  score and result; an interrupted exam currently cannot restore its checkpoint.

## How to generate and try the new grounds in Unity

**Back up or commit your current generated scenes and any unsaved edits first.**
The generator does not overwrite existing connected scenes.

1. Exit Unity Play Mode and close the Unity Editor.
2. Pull the updated `dev` branch. Keep your locally edited Unity scene assets
   safe: if Git refuses the pull, do NOT discard them.
3. Reopen the project in Unity `6000.6.7f1`; wait for import. Resolve any red
   Console compile errors first.
4. Choose **Nepal Trial → Connected Exam → Generate All Three Connected Grounds**.
   This imports existing `Assets/Scenes/CarTrial_*.unity` and
   `Assets/Scenes/BikeTrial_*.unity`, then registers the three new scenes
   in Unity Build Settings.
5. Open `Assets/Scenes/NepalTrial_FullCar.unity` or the connected Motorcycle/Scooter
   scene to inspect the full top-down ground and Inspector references.
6. Press Play, or start from `NepalTrialMainMenu.unity` and select
   **Start Driving → Car / Motorcycle / Scooter → Exam**.
7. Test the entire course in sequence. Watch for:
   - missing references and Unity Console errors
   - Figure-8 reachability/gate directions and boundary sensors
   - stage entry/exit sequence and transfer distances/road continuity
   - controls / reverse shifts / L-back corrections
   - behavior checklist/score/pass/fail/Restart/Home buttons
8. Run Unity EditMode and PlayMode tests. Commit `NepalTrial_Full*.unity`,
   associated `.meta`, material and ProjectSettings changes once reviewed.

## Important gaps before calling this an authentic Nepal trial

1. **Vehicle-specific geometry**: final road positions, widths and obstacle
   connections must be checked against each official government layout.
2. **Car L-back**: replace the existing rectangular parking sensor/course with
   a measured L-shaped reverse lane, approach and exit gates, collision/line
   detection, steering correction and mandatory completion checks.
3. **Fine boundary events**: distinguish a permissible single painted-line
   touch from tyre crossing or departing the road. Existing figure-eight
   sensors currently emit only whole-wheel/annulus departure events.
4. **Car traffic/bumps/ramp**: align actual sensor output and 3/4/3 behaviors
   with the official A/K and B marking tables, especially engine stalling
   and stops at both ends of the ramp.
5. **Exam checkpoint persistence**: complete safe restoration of a stopped
   connected exam. For now unfinished connected exams must be restarted.
6. **Tests**: Unity EditMode/PlayMode, manual route crossing and Android build,
   touch multi-input, performance and official-rule sign-off are still required.

References: Bagmati Province's published A/K and B trial spot drawings and
annexes: https://hetaudayatayat.bagamati.gov.np/np/download-traffic-law/19

**Important:** No Unity compile, Game view driving test, GitHub runtime CI job,
or Android build has been executed for this new generator.

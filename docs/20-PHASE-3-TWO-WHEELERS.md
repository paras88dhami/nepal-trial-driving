# Phase 3 — Motorcycle and scooter gameplay / seven A–K trial stages

**Status: code integrated on dev; NOT compiled, scene-generated, or PlayMode/Android tested.**

## Scope and regulatory accuracy

The Nepal Ministry of Physical Infrastructure and Transport's published A/K practical trial annex lists seven categories: **figure-eight, narrow plank, U-turn, traffic signal, speed breaker, ramp (ascending/descending), and behavioral checks**. Reference: [Ministry A/K practical trial annex](https://www.mopit.gov.np/rules/%E0%A4%B8%E0%A4%B5%E0%A4%BE%E0%A4%B0%E0%A5%80%E0%A4%9A%E0%A4%BE%E0%A4%B2%E0%A4%95%E0%A4%85%E0%A4%A8%E0%A5%81%E0%A4%AE%E0%A4%A4%E0%A4%BF%E0%A4%AA%E0%A4%A4%E0%A5%8D%E0%A4%B0%E0%A4%AA%E0%A4%B0%E0%A5%80%E0%A4%95%E0%A5%8D%E0%A4%B7%E0%A4%BE%E0%A4%B8%E0%A4%9E%E0%A5%8D%E0%A4%9A%E0%A4%BE%E0%A4%B2%E0%A4%A8%E0%A4%95%E0%A4%BE%E0%A4%B0%E0%A5%8D%E0%A4%AF%E0%A4%B5%E0%A4%BF%E0%A4%A7%E0%A4%BF%E0%A4%A8%E0%A4%BF%E0%A4%B0%E0%A5%8D%E0%A4%A6%E0%A5%87%E0%A4%B6%E0%A4%BF%E0%A4%95%E0%A4%BE%E0%A4%95%E0%A5%8B%E0%A4%85%E0%A4%A8%E0%A5%8D%E0%A4%A4%E0%A4%BF%E0%A4%AE%E0%A4%B0%E0%A5%81%E0%A4%AA2%281%29%E0%A4%B8%E0%A4%82%E0%A4%B6%E0%A4%BE%E0%A5%8B%E0%A4%A7%E0%A4%BF%E0%A4%A42021-07-26_01-09-54-347_1676367030.pdf).

This implementation uses **PROVISIONAL** geometry, arcade physics, score deductions and thresholds. Verify the **current rulebook for the province and testing office** before publishing the game as official or exact.

## Generate 14 separate trial scenes

1. With your project working tree clean or backed up, update: `git switch dev && git pull --ff-only origin dev`.
2. Open Unity Hub, project `nepal-trial-driving`, Editor `6000.6.7f1`.
3. Resolve package imports and check the **Console** for compile errors before proceeding.
4. Choose **Nepal Trial → Phase 3 → Create Motorcycle and Scooter Trials**.
5. Open a generated scene from `Assets/Scenes/BikeTrial_motorcycle_figure-eight.unity` or `Assets/Scenes/BikeTrial_scooter_narrow-plank.unity`. The full names always use lowercase `motorcycle` or `scooter`. (The menu creates 7 per type, 14 total.)
6. Press **Play** and inspect wheel contact, motor torque, balance, HUD, penalty application, results, restarts and saved record.
7. Run **Nepal Trial → Phase 3 → Audit Motorcycle and Scooter Trials** for a **static wiring audit**.
8. Run **Window → General → Test Runner → EditMode → Run All**, then add actual PlayMode and Android touchscreen observations to the QA log.
9. Review and commit generated `.unity`, `.mat` and `.meta` files. GitHub currently stores the **generator** source, not the Unity-generated scene assets.

Generated scenes already bearing those names are **preserved**, not replaced automatically.

## Seven separately playable trial stages per vehicle

| Stage | Scene logic | Sample failure triggers |
|---|---|---|
| Figure-eight | Ordered direction-only gates, two-wheel ring boundaries | Out-of-ring incident, foot down, fall, manual engine stall |
| Narrow plank | Both wheel ground hits in volume, forward distance before timeout | Wheel off plank, foot down, timeout |
| U-turn | Entry/exit volumes, net 140° turn, current left/right indicator | Boundary, incomplete turn, timeout, missing/counter-direction indicator |
| Traffic light | Trigger stop-line and timed red/green light | Cross on red/yellow |
| Speed breaker | Cross 5 physical bumps inside trigger at controlled speed | Over-speed, foot down, manual engine stall |
| Ramp | Physical uphill slope, stop zone, crest, downhill slope, rollback guard, exit | Missed stop, rollback, foot down, timeout, repeated stall |
| Behavioral checks | Interactive helmet/mirror/brake/tire checklist | Incomplete checklist when submitted |

The per-scene TrialGameplayRuntime stores passed/failed results via existing LocalProgressStore. **These are independent practice scenes**, not yet a single seven-stage continuous exam course. Integrating menus and sequential scene selection is Phase 4.

## Controls

**Keyboard** (during Play): W/Up acceleration, S/Down front brake, Space rear brake; A/D or Left/Right steer; Shift hold clutch, E gear up, Q gear down; F foot down, Z left indicator, X right indicator, I restart engine, T reset bike/scooter, Esc pause/resume trial, F5 restart attempt.

**Mobile touch**: on-screen steer, throttle, front/rear brake, clutch and gear buttons, indicators, foot-down, ignition, pause/restart. Clutch and manual gear are ignored by the automatic scooter. The motorcycle **starts in Neutral**: hold clutch then tap Gear + to engage First, release clutch and apply throttle. It may stall if left engaged, stationary, with clutch released and no throttle for a sustained period.

## Technical limitations and verification required

- Two-wheelers use front/rear WheelColliders, a Rigidbody, separate brake torques and assisted roll stabilizing torque. **Do not describe this as validated real-world bike dynamics**.
- Front/rear tire contact, yaw turning, balance, mass/suspension, lean, slope starts and collision behavior must be tuned and verified in Unity physics, especially on Android.
- Narrow plank foot-down is a user control rather than detection of an animated physical rider's shoe.
- Behavioral safety checks are interactive UI confirmations, not detection of helmet strap/mirrors/tires in real camera images.
- Ramp joins and stop-zone collider positions are prototype geometry and require PlayMode QA.
- Stage thresholds, penalty marks and immediate failure behavior are configurable prototypes, not official calculations.
- Auto-generated asset files and corresponding Unity `.meta` sidecars must be committed after generating them locally.
- CI status and NUnit test cases are not evidence that actual Unity EditMode tests or PlayMode tests have run.

## Exit criteria

Phase 3 is **NOT VERIFIED / NOT CLOSED** until:
- Unity imports and compiles the updated project without blockers.
- All 14 generated Unity scenes pass static reference audits and controlled PlayMode exercise.
- Motorcycle first gear/clutch/restart engine, scooter automatic drive, brake forces, stable tire contacts, balance, fall, ramp descent, foot down, signal and all seven completion/failure paths behave repeatably.
- EditMode/PlayMode tests and an actual Android build are executed and passing, with outcome evidence.

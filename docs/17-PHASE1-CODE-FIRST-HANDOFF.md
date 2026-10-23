# Phase 1 — Code-first integration handoff

**Owner decision:** Finish the GitHub coding backlog first, then perform Unity Editor and Android verification as a distinct quality gate. This does not waive verification or permit marking untested features as done.

## New integrated code in PR #3
- Multi-pointer mobile button state with separate left/right/throttle/brake/handbrake; release events handled.
- Prototype car input supports keyboard fallback and touch buttons.
- Track math, ordered checkpoints and wheel-boundary deduplication, plus EditMode test source.
- PracticeTrialRuntime tracks wheel positions, warnings and progress on approximate annulus geometry.
- Prototype scene generator now creates checkpoint transforms, provisional HUD labels and wires wheel components to PracticeTrialRuntime.

## Known engineering gaps before 'code complete'
- Actual figure-8 path should use verified official Nepal geometry, gates and direction-aware wheel-line rules.
- Checkpoint direction, trial-start gating and reset/reposition remain basic.
- Generated Unity scene is created *only when an Editor script runs*; no saved scene or complete generated ProjectSettings/.meta files exist yet.
- Current car visual body is a cube; no wheel mesh animation, gears/clutch UX, audio or polished HUD.
- Runtime UI and wheel dynamics may require adjustments based on Editor compile feedback.
- Package manifest and Unity version remain provisional; resolve on actual URP template.
- No Unity compilation, test run, APK or device inspection has occurred.

## Handoff verification checklist
1. Install/activate Unity 6 LTS with Android modules and create valid URP project.
2. Import feature branch and confirm code compiles; fix errors and preserve Unity-generated metadata.
3. Run Nepal Trial > Create Prototype Scene; save the scene.
4. Play: test steering, brake, reverse, simultaneous touch inputs, checkpoints and warnings.
5. Execute Unity EditMode tests and add PlayMode vehicle tests.
6. Build Android APK and measure gameplay performance on a phone.
7. Only then mark Phase 1 verified/complete and consider release branch promotion.

Do not claim official exam accuracy: dimensions, course geometry and deductions are still subject to official-source validation.

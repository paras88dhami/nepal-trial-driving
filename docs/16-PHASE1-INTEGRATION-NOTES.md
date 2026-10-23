# Phase 1 — Integration update

## Committed source
- Separate mobile button state for left/right steering, gas, brake and handbrake; multiple controls can be held at once.
- Per-button pointer ID ownership and release-on-up.
- `PracticeTrialRuntime` samples WheelCollider contact positions; provisional two-circle annulus boundaries, deduplicated warnings, ordered proximity checkpoints, optional UI text fields and reset.
- Additional EditMode checkpoint test.

## Wiring required in Unity
Attach `PracticeTrialRuntime` to the generated scene. Set the CarDriveController, all 4 WheelColliders, checkpoint transforms and UI Texts in Inspector. Tune lane width and course positions to actual generated geometry. Assign UI button components and enable Input System UI Input Module. Existing scene builder does NOT yet auto-wire trial runner/checkpoints.

## Known limitations
- Current two-loop centerline is only a geometric placeholder, not a physically faithful figure-8.
- Boundary logic is wheel-position based; outside-lane events can fire before entering the course, so start placement/checkpoint gating must be configured.
- Off-ground wheels use a projection estimate, not accurate tyre-ground contact.
- Checkpoints use radius checks rather than directional gates; require course direction checks later.
- Mobile UI buttons are not yet verified on-device.
- Unity Editor import/compile, automated tests and Android APK remain outstanding.
- No official scoring deductions are implemented.

**Status: CODED, not Unity-compiled or Android tested.** Do not mark Phase 1 complete.

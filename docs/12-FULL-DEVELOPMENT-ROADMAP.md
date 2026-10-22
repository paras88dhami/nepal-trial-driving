# 12 — Full Development Roadmap (2026-10-22)
Status: APPROVED TO START; implementation details remain subject to technical and rules verification. Read with [PRD](01-PRD.md), [TRD](02-TRD.md), [Implementation Plan](06-IMPLEMENTATION-PLAN.md) and [Codex Contract](../AGENTS.md).

## Product
Offline Android-first Nepal driving license trial **practice simulator**, not an official government examination. Car (B) and motorcycle/scooter (A/K), individual practice and full trial mode, configurable verified rules, feedback and progress. No backend, multiplayer, ads or purchases in initial version.

## Proposed technical stack
Unity 6 LTS (pin exact Editor after install), C#, URP, Input System, Cinemachine, Unity UI, Rigidbody/WheelCollider, custom motorcycle balance, JSON local persistence, Unity Test Framework, GitHub + Codex. Prefer self-made geometry and verified free assets. No paid purchases without approval.

## Phase schedule (planning targets, not commitments)
| Phase | Approx. timeline | Deliverable and exit gate |
|---|---|---|
| 0: Unity foundation | Week 1 | Verified Editor project, packages, dev/feature branches, empty-scene compile, test Android build |
| 1: Car core | Weeks 2–3 | Throttle/brake/reverse/steering, wheel physics, camera, mobile inputs |
| 2: Figure-8 vertical slice | Week 4 | Procedural course, checkpoints, violations, practice retry, Android demo |
| 3: Full car track | Weeks 5–7 | U-turn, bumps, lights, ramp, reverse parking and appropriate safety checks |
| 4: Car exam/scoring | Week 8 | Versioned ruleset, mock exam, mistake report, saves |
| 5: Motorcycle physics | Weeks 9–11 | Lean, low-speed stability, braking, rider controls, device-tested |
| 6: Motorcycle trial | Weeks 12–13 | Figure-8, plank, U-turn, lights, bumps, ramp, exam |
| 7: Full UI/progression | Weeks 14–15 | Screens, responsive HUD, accessibility, English UI, history, settings |
| 8: Visuals/performance | Weeks 16–17 | Licensed models, textures, lighting, audio, device profiling |
| 9: Release QA | Weeks 18–20 | Automated tests, APK/AAB, device testing, privacy/store readiness |

Actual times may vary significantly, particularly bike physics and low-end Android optimization.

## Detailed phase 0
- Verify appropriate official trial rules/dimensions (do not pretend unverified values are official).
- Pin and record Unity 6 LTS Editor and compatible package versions.
- Create genuine Unity URP project (Assets, Packages, ProjectSettings and metadata).
- Configure Android, Input System, Cinemachine, UI, assembly definitions and test runner.
- Commit a minimal scene and run successful Unity import/compilation and Android build.
- Create dev; implement on feature branches; PR after validation.

## Phase 1 car controller deliverables
- Independent input provider and normalized throttle, steering and brake commands.
- Rigidbody and four-wheel WheelColliders, suspension/traction and configurable car setup.
- Steering, forward/reverse, handbrake, gear UI only after underlying functionality exists.
- Smooth chase camera and touch-oriented controls plus editor keyboard fallback.
- Reproducible tests for command clamping, brake, low-speed maneuvers and scene setup.
- No claim of 'playable' without real Unity scene/device testing.

## Phase 2 acceptance
Third-person car completes recognizable figure-8 on Android, reliably detects wheel/line violations and checkpoints, shows useful feedback/retry, and remains smooth on a defined test device.

## Later phases
Use reusable section validators, ruleset configuration, violation deduplication, persistence schema migrations and UI presentation. Build the car exam before commencing motorcycle physics; then add rider lean and balance, bike-specific sections, art/audio, optimization and publishing.

## Quality gates and branches
main=approved stable; dev=integration; feature/*=work. PRs must list requirement IDs, scope, tests, licenses, blockers. Status vocabulary: TODO, CODED, COMPILED, TESTED, ANDROID VERIFIED. A generated C# file or Unity scene is not evidence of a successful Unity build. Keep docs/11-DEVELOPMENT-LOG.md and docs/06-IMPLEMENTATION-PLAN.md updated.

## Owner vs automation
Codex can author and commit scripts, project settings, scene generators and tests when granted access. The owner may need to activate a Unity license, approve third-party licenses, review visual/gameplay feel, test Android hardware and authorize Play publishing.

## Known dependencies and risks
- Genuine Unity Editor and compatible build toolchain are necessary to validate generated project/scene YAML.
- Trial rules and dimensions vary; verify applicable jurisdiction with official notices and version rulesets.
- Detailed vehicle controllers are hard to tune; default to testable simple controllers and evolve using play tests.
- Only licensed assets; provenance recorded.
- Performance targets: 30 FPS on agreed low-end Android, 60 FPS on agreed midrange Android, never guaranteed until measured.

## Next immediate action
Work on dev → feature/unity-foundation-car. Initialize basic repo-ready scripts, input and procedural car scene generation; when a Unity Editor is available, generate editor assets and test. Do not merge unvalidated gameplay into main.

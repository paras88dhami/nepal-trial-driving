# 10 — Unity Architecture and Coding Standards

## Responsibilities
**Core:** lifecycle, scene navigation, events and configuration.
**Vehicles:** physics, motor, wheels, steering, bike balancing; no awareness of score.
**Input:** touch and debug devices mapped to normalized control commands.
**Trial:** section definitions, checkpoints, validators, score accumulation and ruleset versions.
**Persistence:** versioned saves, migrations and corruption recovery.
**UI:** reads state, raises intent, never implements scoring math.
**Editor:** track generators and developer-only tooling.
**Tests:** edit-mode rules tests and play-mode physics/scene smoke tests.

## Target project structure
```text
Assets/
  Game/
    Core/
    Vehicles/Car/
    Vehicles/Bike/
    Input/
    Trial/Definitions/
    Trial/Validators/
    Trial/Scoring/
    Persistence/
    UI/
    Camera/
    Environment/
    Editor/
    Tests/EditMode/
    Tests/PlayMode/
  Scenes/
  Art/
  Audio/
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
```
Preserve .meta files produced by Unity. Do not guess GUIDs without checking editor-generated references.

## Data flow
InputAction → VehicleCommand → VehicleController → PhysicalState → TrialValidator → ViolationEvent → ScoringService → HUD / Results → SaveService.
Use dependency boundaries and explicit interfaces; validation must be deterministic given state and rules.

## Coding practices
PascalCase public types, camelCase locals, explicit serialized private fields, single-purpose components, minimal allocations in Update/FixedUpdate, no FindObjectOfType in hot loops, no magic thresholds, no unexplained singleton global state. Separate Update input sampling from FixedUpdate physics application.

## Test and review rules
Each validator needs threshold tests, rearm/deduplication tests and scene-level trials. Car controller requires tests for input clamping and safe stop. Every new package needs license/version review. CI checks can validate text/config but cannot replace actual Unity Editor compile/import results.

## Project initialization
Create a genuine Unity 6 LTS project in a supported Unity Editor and record exact version and pinned packages; placeholder directory structure is not sufficient evidence the project is runnable.

## Git workflow
Initialize main once, branch dev, changes on feature/*, PR to dev after validation, release from validated dev to main. Preserve runnable dev once Unity baseline established.

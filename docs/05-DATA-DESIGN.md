# 05 — Offline Data and Save System

**Architecture:** No backend, login, account or online sync in v1. Store local structured data with schema versions.

## Storage
Use Application.persistentDataPath for JSON records. PlayerPrefs only for trivial preferences (such as selected quality). Save atomically (write temp and replace), validate on load and retain a recoverable backup where practical.

## Proposed models
```text
PlayerSettings
 schemaVersion:int
 language:string
 quality:string
 steeringMode:string
 steeringSensitivity:float
 soundVolume:float

TrialAttempt
 attemptId:string
 startedAtUtc:string
 completedAtUtc:string?
 vehicleCategory:string
 mode:practice|exam
 rulesetId:string
 sectionResults:SectionResult[]
 totalScore:int?
 passed:bool?
 verifiedOfficialRules:bool

SectionResult
 sectionId:string
 completed:bool
 violations:Violation[]
 pointsLost:int

Violation
 ruleId:string
 timestampSeconds:float
 details:string
 penaltyPoints:int
 instantFail:bool
```

## Integrity and migration
- Store explicit ruleset identifiers and version so past scores remain interpretable if rules change.
- Never overwrite a mock-exam score with a practice score.
- Debounce saves; handle interrupted writes and corrupted JSON gracefully.
- Schema migrations retain old progress when feasible; test migration using archived sample files.
- Do not store personal identity information or collect unnecessary device telemetry.
- A UI score is only meaningful if the selected trial ruleset is defined and valid.

## Planned tests
Round-trip serialization; absent files; corrupt files; schema upgrade; duplicate violation handling; interrupted saves; no network dependency.

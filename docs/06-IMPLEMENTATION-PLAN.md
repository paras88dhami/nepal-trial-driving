# 06 — Implementation Plan

**Status:** Documentation phase; Unity installation and actual code/build status are NOT confirmed.

## Phase 0 — Documentation and repository
- [x] Establish requirements and development rules documents.
- [ ] Review and approve proposed scope.
- [ ] Verify official Nepal trial dimensions and rules for the target transport office.
- [ ] Establish Unity version, git branches and tested project baseline.
**Exit gate:** approved requirements, clear outstanding assumptions.

## Phase 1 — Car vertical slice (PRD-002/006/007/008)
- [ ] Initialize Unity 6 LTS URP project with correct packages and .meta files.
- [ ] Implement car driving, brake, steering and follow camera using a free/approved approach.
- [ ] Generate figure-8 geometry with visible boundaries.
- [ ] Trigger checkpoints, boundary collisions and configurable mistakes.
- [ ] Add Android landscape touch HUD and practice retry.
- [ ] Add edit-mode tests and real Unity compilation.
**Exit gate:** playable figure-8 on real device; report measured performance.

## Phase 2 — Car trial and mock examination (PRD-002/004/005/006/009)
- [ ] U-turn, speed breakers, traffic light, ramp/hill start and reverse garage.
- [ ] Practice section selection, section briefing, results and local save.
- [ ] Verified versioned exam rules and pass/fail conditions.
- [ ] Play-mode validation and Android QA.

## Phase 3 — Motorcycle/scooter (PRD-001/003/013)
- [ ] Bike physics with low-speed lean and balance, braking and foot-contact logic.
- [ ] Narrow plank and remaining bike sections.
- [ ] Adapt safety checklist and scoring to verified rules.
- [ ] Repeated controlled input tests on a physical device.

## Phase 4 — Polish and launch (PRD-010/011/012)
- [ ] Replace placeholders with license-checked optimized assets.
- [ ] Add audio, settings, UI polish and accessibility.
- [ ] Localization, crash/performance tests, store policy and privacy review.
- [ ] APK/AAB builds, hands-on QA, release candidate and launch.

## Tracking convention
For every task record: PRD ID | owner | branch/commit | status (TODO/CODED/COMPILED/TESTED/ANDROID VERIFIED) | dependencies | evidence | blockers. Keep main releasable, dev for integration and feature branches for changes.

## Definition of done
A feature is not marked tested merely because it has code. Compilation, automated tests, device builds and performance must be backed by actual logs or observations.

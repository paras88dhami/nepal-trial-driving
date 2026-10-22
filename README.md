# Nepal Driving Trial 3D — Project Blueprint

**Status:** Documentation baseline v1.0 • **Date:** 2026-10-22 • **Repository:** `paras88dhami/nepal-trial-driving`

A mobile-first, offline, single-player 3D **practice simulator** for Nepal's practical driving-license trial, initially covering Category B cars and Category A/K motorcycles/scooters. The game is a training aid, **not an official government exam or a guaranteed replica of a particular testing center**.

## Quick start for contributors and Codex
1. Read [`AGENTS.md`](AGENTS.md) and [`docs/01-PRD.md`](docs/01-PRD.md).
2. Review the rules and unverified assumptions in [`docs/08-NEPAL-TRIAL-RULES.md`](docs/08-NEPAL-TRIAL-RULES.md).
3. Follow the dependency order in [`docs/06-IMPLEMENTATION-PLAN.md`](docs/06-IMPLEMENTATION-PLAN.md).
4. Do not claim an APK or a Unity project is playable until actually imported, run and tested.

## Documentation index
1. `01-PRD.md` — product scope and acceptance criteria
2. `02-TRD.md` — technology choices and constraints
3. `03-GAME-FLOW.md` — game states, screens, navigation
4. `04-UI-UX-DESIGN.md` — visual design and controls
5. `05-DATA-DESIGN.md` — offline schema and save evolution
6. `06-IMPLEMENTATION-PLAN.md` — phases and quality gates
7. `07-GAME-DESIGN.md` — game mechanics and progression
8. `08-NEPAL-TRIAL-RULES.md` — evidence and rule verification
9. `09-ASSET-PLAN.md` — asset list, budget, licensing
10. `10-ARCHITECTURE.md` — Unity module layout and boundaries
11. `AGENTS.md` — non-negotiable Codex development instructions

## Decisions requiring approval before gameplay work
- Exact Unity 6 **LTS editor version** and URP package version after testing availability.
- Trial ground geometry/dimensions from applicable government diagrams.
- Province / transport office-specific rule variant when needed.
- Which free vehicle controllers/models to use, after license and performance review.
- Android application ID before store release (working placeholder: `com.parasdhami.nepaldrivingtrial`).

## Working assumptions
- Android first; landscape orientation; 3rd-person camera; free playable offline practice and exam modes.
- Car is built and validated before bike; final target includes both.
- No paid purchases, online auth, ads or backend in v1 without explicit approval.

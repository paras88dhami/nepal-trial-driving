# Codex / AI Development Contract

Applies to the entire repository. The documentation is the source of truth.

## Read before coding
1. docs/01-PRD.md
2. docs/08-NEPAL-TRIAL-RULES.md
3. docs/02-TRD.md and docs/10-ARCHITECTURE.md
4. docs/03-GAME-FLOW.md, docs/04-UI-UX-DESIGN.md, docs/07-GAME-DESIGN.md
5. docs/06-IMPLEMENTATION-PLAN.md, docs/09-ASSET-PLAN.md, docs/05-DATA-DESIGN.md

## Scope rules
- Implement only approved PRD requirements in the current implementation phase.
- PROPOSED, TBD and UNVERIFIED are not approvals or verified facts.
- No ads, accounts, backend, multiplayer, cloud integration, paid assets or scope changes without explicit owner approval.
- Never invent Nepal trial dimensions or point deductions. Cite and date official sources; keep rule variants configurable.
- Label game as an unofficial practice simulator, not a government exam.

## Development rules
- Unity 6 LTS, C#, URP, Android first, offline.
- Separate vehicle physics, input, trial validation, scoring, saves and UI.
- Prefer testable C# components; avoid monolithic MonoBehaviours and magic scoring numbers.
- Pin verified package versions; keep asset license provenance.
- Commit Unity Assets, Packages, ProjectSettings and .meta files; ignore Library, Temp, Obj, Logs and builds.
- Never commit credentials or paid third-party assets without redistribution permission.
- For an empty repo initialize main, then create dev; develop on feature branches and merge via reviewed PR.
- Add unit tests for rules and play-mode tests for physics/scenes where possible.

## Honest verification
- CODED = code committed.
- COMPILED = Unity compiler actually succeeded.
- TESTED = tests really executed.
- ANDROID VERIFIED = successful build and device smoke test.
- Without Unity, explicitly report UNITY VALIDATION PENDING.
- After each feature update docs/06-IMPLEMENTATION-PLAN.md and docs/11-DEVELOPMENT-LOG.md with commit, tests, blockers and next steps.
- PR summaries must include requirement IDs, changed files, dependencies/assets, license evidence, tests, assumptions and risks.

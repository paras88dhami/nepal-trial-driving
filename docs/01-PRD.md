# 01 — Product Requirements Document (PRD)

**Version:** 1.0 | **Status:** Proposed baseline awaiting owner approval.

## Vision
Create a smooth, beautiful offline 3D Android practice simulator for Nepal's car (Category B) and motorcycle/scooter (A/K) driving-license trials. Help learners practice individual obstacles, understand mistakes and attempt a mock examination. Unofficial educational product, not a guaranteed replica.

## Audience and platform
Nepali beginner drivers; Android phones including budget devices; landscape, touchscreen; playable offline without login.

## Requirements
| ID | Feature | Acceptance |
|---|---|---|
| PRD-001 | Vehicle selection | Select car, motorcycle or scooter with different handling |
| PRD-002 | Car sections | Figure-8, speed bumps, U-turn, signals, hill/ramp and reverse garage |
| PRD-003 | Bike sections | Figure-8, narrow plank, U-turn, signals, bumps and hill |
| PRD-004 | Practice | Choose/retry each obstacle and view mistake feedback |
| PRD-005 | Mock exam | Sequential sections, configurable penalty and disqualification rules |
| PRD-006 | Results | Explain per-section mistakes, points and outcome |
| PRD-007 | Controls | Mobile steering, throttle, brake; driving aids as options |
| PRD-008 | Camera | Smooth follow and camera reset/cycle |
| PRD-009 | Storage | Save settings, scores and progress locally |
| PRD-010 | Language | English at initial release; Nepali localization planned |
| PRD-011 | Performance | Aim 60 FPS midrange / acceptable 30 FPS low-end, verify on devices |
| PRD-012 | Disclaimer | Identify unofficial practice simulator; local trial rules may vary |
| PRD-013 | Safety checks | Helmet, seatbelt, signals only as supported by verified rules |

## Delivery milestones
A. Car figure-8 playable vertical slice.
B. All car sections with practice and exam.
C. Motorcycle/scooter physics and sections.
D. Graphics polish, accessibility, localization and Android release QA.

## Out of scope v1
Open world, racing, police chases, multiplayer, online leaderboards, login, payments, ads, written-theory questions, AI traffic, VR and user-generated maps.

## Definition of complete
Interactive menus; reliable collision/scoring; retriable practice; reproducible mock-exam outcome; locally saved progress; successful Android on-device test; verified applicable trial rules. Never claim official certification.

## Change control
New requests must be described, estimated, approved and reflected here before implementation.

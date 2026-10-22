# 08 — Nepal Trial Rules and Source Verification

**Status: UNVERIFIED RULE DETAILS.** This document is the authoritative *project register for evidence*, not a claim that the game's scoring matches government requirements. Never invent dimensions, section order, penalties or pass marks.

## Scope
Identify applicable regulations for Category B (car) and A/K (motorcycle/scooter). National guidance, provincial implementation and individual transport-office layouts may differ. A legal source and a site diagram are not interchangeable.

## Candidate source register (verify access, currency and interpretation)
- Ministry of Physical Infrastructure and Transport: https://mopit.gov.np/ — search published driver's-license trial examination procedure and amendments.
- Department of Transport Management: https://dotm.gov.np/ — license information and official trial documentation.
- Provincial transport offices (for example Hetauda, Bagmati) — local track layout and implementation notices.
- Transport-office officials or official public notices for center-specific differences.
Record document title, URL, issuer, date, jurisdiction, page/section, and last verification date for every implemented rule.

## Provisional obstacle inventory — NOT a verified scoring specification
Car practice: figure-8, speed breakers, U-turn, traffic signals, hill/ramp, garage/reverse parking, safety behavior.
Motorcycle practice: figure-8, narrow plank, U-turn, traffic signals, speed breakers, slope, safety behavior.
Obstacle list and order require validation against currently applicable rules.

## Required rule matrix
| Field | Meaning |
|---|---|
| rulesetId | Distinct version and jurisdiction |
| category | B / A / K as applicable |
| sectionId / order | Verified obstacle and exam order |
| geometry | Dimensions, clearances, incline, marker positions, units |
| condition | Trigger predicate / prohibited action |
| points | Verified score or deduction |
| instantFail | Verified immediate disqualification |
| cooldown | When one violation can trigger again |
| evidence | Official title, URL and exact page/section |
| verifiedOn | Date reviewed, reviewer |
| status | UNVERIFIED / VERIFIED / SUPERSEDED |

## High-priority open questions
- Official scoring and minimum passing criteria per category and current jurisdiction.
- Exact dimensions of figure-8, plank, U-turn, garage, speed breaker and ramp.
- Legal definitions of tyre touching line, foot touching ground, bike fall, engine stall, rollback, signal violation and retakes.
- Whether required safety checks and trial section sequence vary by office.
- Whether old notices have been superseded.

## Implementation constraint
Prototype geometry may be clearly labelled approximate. Do NOT label practice or exam mode officially accurate until every deployed scoring rule and dimension is sourced and verified. Use separate configurable rulesets for regional variants.

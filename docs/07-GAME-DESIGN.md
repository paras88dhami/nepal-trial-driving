# 07 — Game Design Document (GDD)

## Core fantasy
A learner enters a recognizable Nepal trial ground, controls a vehicle and improves through targeted feedback. This is a training simulation, not a racing game.

## Modes
**Practice:** freely choose a section; read instructions; replay quickly; receive explicit feedback; optional accessible driving aids.
**Mock exam:** ordered trial sections, one attempt, configured rule deductions and immediate-fail conditions; final detailed score report.

## Vehicle models
Car: speed, steering, braking, inertia, wheel grip, slope response, optional handbrake and manual gears. Motorcycle/scooter: slow-speed stability, steering/lean coordination, fall detection and foot-placement penalty simulation as needed. Vehicle tuning remains independent from rules evaluation.

## Sections
Car: figure-8, bumps, U-turn, traffic signal, hill/ramp, garage/reverse.
Motorcycle/scooter: figure-8, narrow plank, U-turn, traffic signal, bumps, hill/ramp.
Safety checks, sequencing and scoring depend on verified local ruleset.

## Level structure
Briefing → countdown → checkpoints → monitored drive → section result → next/retry. Each level declares entrance, boundaries, waypoints, obstacles, success condition, violations and failure triggers.

## Rules engine
Rule definitions include code, section, category, penalty, cooldown/rearm condition, instant-fail flag and evidence version. Store violations as events. Never use visual collision alone as evidence if the trial condition concerns wheel position, balance or traffic timing.

## Progression
Practice remains accessible; players can practice difficult sections without grind. Preserve attempts, personal best and mistake history locally. Progress is optional; never block essential practice behind payment.

## Control feel and tuning
Start with accessible mobile steering and gradual acceleration. Offer sensitivity controls. Slow-speed figure-8 and motorcycle balance are primary physics acceptance cases, not top-speed driving.

## Quality criteria
Player can understand mistakes, repeat challenges without crashes, pause safely, and complete mock exams without duplicated deductions. Maintain an always-visible unofficial practice disclaimer in relevant help/about screens.

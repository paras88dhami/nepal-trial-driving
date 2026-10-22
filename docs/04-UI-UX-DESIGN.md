# 04 — UI/UX Design Brief

**Style:** Polished, stylized-realistic Nepal driving test ground; practical and understandable rather than cartoonish or expensive photorealism. Mobile-first landscape.

## Visual direction
- Asphalt, realistic lane boundaries, cones, traffic equipment, safety signs, training-school environment and clear vehicle silhouette.
- URP lighting, baked environment light and selective real-time shadows. Graphics scalability is essential.
- Distinguish safe paths, hazards, targets and results by geometry/labels as well as color.
- Consistent typography, limited color palette, accessible controls and readable feedback in direct sunlight.

## Key screens
Splash, Home, Vehicle Select, Mode Select, Section Select, Trial Briefing, Driving HUD, Pause, Results/Mistakes, Progress and Settings.

## Driving HUD
- Left: adjustable steering wheel or left/right buttons; optional tilt control only after validation.
- Right: accelerator and brake; gear, clutch/handbrake if corresponding simulation exists.
- Top: score/remaining points, speed, section and timer where appropriate.
- Camera and pause accessible but outside steering input areas.
- Large touch targets, safe-area support, no inadvertent taps over driving controls.

## Camera and accessibility
Default chase camera showing front wheel path/road edges; optional alternate camera; smooth-follow without disorienting lag. Offer steering sensitivity and control layouts. English first, Nepali localization after strings validated.

## Feedback
Immediate but not obstructive violation cue. After each practice attempt explain where and why points were lost, plus retry. In exams do not distract with verbose messages; show final breakdown.

## UI acceptance criteria
No overlapping controls on target screens, input responsive during turns, labels readable at representative phone resolutions, result screen explains each deduction, and back/pause never unintentionally restarts an exam.

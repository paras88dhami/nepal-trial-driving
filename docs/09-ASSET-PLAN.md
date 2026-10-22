# 09 — Asset Management Plan

## Goals
Attractive, mobile-friendly 3D presentation and legal, reproducible assets. Budget starts at NPR 0 for paid assets.

## Inventory by category
- Vehicles: one car, one motorcycle, one scooter, detachable wheels/rider as needed.
- Trial: asphalt, markings, cones, plank, U-turn geometry, bumps, ramps, garage, traffic-light posts.
- Environment: ground, fence, small buildings, trees, sky, signs, safety barriers.
- Audio: engine idle/rev, tire roll, brake, horn, collision, UI, success/failure feedback.
- UI: control icons, speedometer, pause/camera, result indicators, menus.
- Code packages: input, camera, car/bike vehicle systems, deterministic trial geometry.

## Sourcing hierarchy
1. Built-in Unity geometry and self-authored C# controllers.
2. Original Blender models/procedural modelling.
3. Freely licensed assets with verified commercial-use and redistribution terms.
4. Paid assets only after explicit user approval.

## Asset register template
| Name | Category | Source URL | Author | License | Redistribution allowed? | Version | Unity compatibility | Attribution required | Approval |
|---|---|---|---|---|---|---|---|---|---|
| Placeholder cube car | Prototype | Original | Project | Project-owned | Yes | v1 | To test | No | Proposed |

## Engineering and performance
Use consistent meter scale and collision shapes. Track texture memory, polygon count, draw calls, compression, shader variant and prefab ownership. Prefer shared materials/atlases, baked static lighting, optimized meshes and LODs where useful; profile on physical Android phones.

## Third-party risks
A free-to-download asset is not automatically free to redistribute in a public GitHub repo. Preserve attribution and license evidence. Do not commit copyrighted models from unrelated games, pirated Asset Store packages, watermarked meshes or unknown-origin content.

## Phase-specific needs
Phase 1 only needs placeholder car/wheels, track, markers, UI and camera. Add vehicles/audio/decoration after gameplay works. Avoid large asset downloads before establishing the Unity project baseline.

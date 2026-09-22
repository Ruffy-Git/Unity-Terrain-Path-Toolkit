# Roadmap

## 0.1 — Functional pre-release candidate
- [x] Unity package manifest
- [x] Runtime path component
- [x] Scene-view control-point editing
- [x] Path sampling
- [x] Terrain projection
- [x] Basic strip mesh generation
- [x] Mesh fingerprint/change detection
- [x] Safe regeneration checks
- [x] Adopt-current-mesh-as-baseline workflow
- [x] Explicit destructive regeneration action
- [x] Core EditMode tests
- [x] Installation and workflow documentation

## Required validation before stable release
- [ ] Install from Git URL in Unity
- [ ] Confirm clean compilation in Unity 2022.3 LTS or newer
- [ ] Run all EditMode tests
- [ ] Generate a path on flat terrain
- [ ] Generate a path across hilly terrain
- [ ] Modify generated mesh and verify regeneration is blocked
- [ ] Adopt modified mesh as baseline and verify warning clears
- [ ] Add real Unity screenshots/GIF to README

## 0.2 — Better authoring
- [ ] Bezier/spline interpolation
- [ ] Point insertion/removal controls
- [ ] Branch/junction authoring
- [ ] Per-point width
- [ ] Advanced UV controls
- [ ] Edge/falloff options
- [ ] Sample scene

## Later exploration
- Terrain painting beneath paths
- Embankment/cut handling
- Prefab decoration along paths
- Intersections
- Runtime generation where practical

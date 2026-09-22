# Changelog

All notable changes to this project will be documented here.

## [0.1.0] - 2026-09-22

### Added
- Unity Package Manager structure and assembly definitions.
- Editable TerrainPath component and Scene-view control-point handles.
- Polyline sampling and optional Unity Terrain projection.
- Basic strip-mesh generation with UVs and normals.
- Configurable width, sample spacing, vertical offset, Terrain, and material.
- Deterministic mesh fingerprinting for external-edit detection.
- Safe regeneration blocking when generated geometry has changed.
- Adopt Current Mesh as Baseline workflow.
- Explicit Replace Manual Edits and Regenerate workflow.
- EditMode tests for mesh generation, path sampling, and fingerprint change detection.
- Architecture, getting-started, manual-edit-protection, security, contribution, and issue/PR documentation.

### Known limitations
- v0.1 has not yet been validated inside the user's local Unity Editor.
- Paths currently use polyline interpolation rather than Bezier curves.
- Baseline adoption records accepted output; it does not reverse mesh edits into source control points.

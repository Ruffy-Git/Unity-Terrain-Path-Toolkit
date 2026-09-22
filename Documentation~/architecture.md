# Architecture

## Core principle

Editable source data is the authority. Generated geometry is an output, not the only representation of the path.

## Planned layers

### Path data
Stores control points and authoring parameters independently of generated mesh data.

### Sampling
Converts the editable path into evenly spaced samples suitable for terrain projection and mesh construction.

### Terrain projection
Projects samples against a selected Unity Terrain while retaining configurable vertical offset and smoothing.

### Mesh generation
Builds vertices, triangles, UVs, normals, and optional edge information from sampled path data.

### Baseline tracking
Records a fingerprint of generated output. If an artist edits the generated mesh outside the authoring workflow, the editor can warn before regeneration.

### Manual-baseline adoption
An explicit operation will allow the current output to become the accepted baseline. Regeneration must never silently overwrite detected external edits.

## Non-goals

The first releases are not intended to replace full road-network, traffic, or city-generation systems. The initial scope is environment path authoring.

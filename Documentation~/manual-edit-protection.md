# Manual Edit Protection

Terrain Path Toolkit treats path control points and settings as editable source data and the generated mesh as an output.

After generation, the toolkit records a deterministic fingerprint of mesh vertices, triangle indices, and UVs. Before normal regeneration it calculates the fingerprint again. If those values differ, normal regeneration stops and nothing is overwritten.

## Adopt Current Mesh as Baseline

Use this when a direct mesh change was intentional. The toolkit records the current output as accepted.

Baseline adoption protects intent; it does not convert arbitrary mesh edits back into path control points. Future regeneration still derives geometry from the editable path source, so shape changes that must survive regeneration should be represented in source controls whenever possible.

## Replace Manual Edits and Regenerate

Use this when you intentionally want to discard direct changes to the generated mesh and rebuild it from the current editable path source.

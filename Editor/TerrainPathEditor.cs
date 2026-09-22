using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TerrainPathToolkit.Editor
{
    [CustomEditor(typeof(TerrainPath))]
    public sealed class TerrainPathEditor : UnityEditor.Editor
    {
        private TerrainPath Path => (TerrainPath)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var modified = HasExternalMeshEdits();
            if (modified)
                EditorGUILayout.HelpBox("The generated mesh differs from its recorded baseline. Regeneration is blocked until you adopt the current mesh or intentionally replace it.", MessageType.Warning);

            if (GUILayout.Button("Add Point")) AddPoint();
            if (GUILayout.Button("Generate / Regenerate Path")) Generate(false);

            using (new EditorGUI.DisabledScope(Path.GeneratedObject == null))
            {
                if (GUILayout.Button("Adopt Current Mesh as Baseline")) AdoptBaseline();
            }

            if (modified && GUILayout.Button("Replace Manual Edits and Regenerate"))
                Generate(true);
        }

        private void AddPoint()
        {
            Undo.RecordObject(Path, "Add Terrain Path Point");
            var count = Path.ControlPoints.Count;
            var position = count > 0 ? Path.GetWorldPoint(count - 1) + Path.transform.forward * 5f : Path.transform.position;
            Path.AddWorldPoint(position);
            EditorUtility.SetDirty(Path);
        }

        private bool HasExternalMeshEdits()
        {
            if (Path.GeneratedObject == null || string.IsNullOrEmpty(Path.BaselineFingerprint)) return false;
            var filter = Path.GeneratedObject.GetComponent<MeshFilter>();
            return filter == null || MeshFingerprint.Calculate(filter.sharedMesh) != Path.BaselineFingerprint;
        }

        private void Generate(bool replaceManualEdits)
        {
            if (HasExternalMeshEdits() && !replaceManualEdits)
            {
                EditorUtility.DisplayDialog("Manual Mesh Edits Detected",
                    "This output has mesh edits outside the authoring tool. Adopt the current mesh as a manual baseline, or use Replace Manual Edits and Regenerate if you intentionally want to discard them.",
                    "OK");
                return;
            }

            var points = PathSampling.SamplePolyline(Path);
            if (Path.ConformToTerrain && Path.TargetTerrain != null)
                PathSampling.ProjectToTerrain(points, Path.TargetTerrain, Path.VerticalOffset);

            if (points.Count < 2)
            {
                EditorUtility.DisplayDialog("Terrain Path Toolkit", "At least two valid path points are required.", "OK");
                return;
            }

            Undo.RecordObject(Path, "Generate Terrain Path");
            var output = Path.GeneratedObject;
            if (output == null)
            {
                output = new GameObject("Generated Path");
                Undo.RegisterCreatedObjectUndo(output, "Create Generated Path");
                output.transform.SetParent(Path.transform, false);
                Path.GeneratedObject = output;
            }

            var filter = output.GetComponent<MeshFilter>() ?? Undo.AddComponent<MeshFilter>(output);
            var renderer = output.GetComponent<MeshRenderer>() ?? Undo.AddComponent<MeshRenderer>(output);
            if (Path.PathMaterial != null) renderer.sharedMaterial = Path.PathMaterial;

            var oldMesh = filter.sharedMesh;
            var mesh = PathMeshBuilder.BuildStrip(points, Path.Width, output.transform);
            filter.sharedMesh = mesh;
            Path.BaselineFingerprint = MeshFingerprint.Calculate(mesh);

            if (oldMesh != null && oldMesh != mesh && !AssetDatabase.Contains(oldMesh))
                DestroyImmediate(oldMesh);

            EditorUtility.SetDirty(Path);
            EditorUtility.SetDirty(output);
        }

        private void AdoptBaseline()
        {
            var filter = Path.GeneratedObject != null ? Path.GeneratedObject.GetComponent<MeshFilter>() : null;
            if (filter == null || filter.sharedMesh == null) return;
            Undo.RecordObject(Path, "Adopt Terrain Path Mesh Baseline");
            Path.BaselineFingerprint = MeshFingerprint.Calculate(filter.sharedMesh);
            EditorUtility.SetDirty(Path);
        }

        private void OnSceneGUI()
        {
            for (var i = 0; i < Path.ControlPoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                var next = Handles.PositionHandle(Path.GetWorldPoint(i), Quaternion.identity);
                if (!EditorGUI.EndChangeCheck()) continue;
                Undo.RecordObject(Path, "Move Terrain Path Point");
                Path.SetWorldPoint(i, next);
                EditorUtility.SetDirty(Path);
            }

            var preview = PathSampling.SamplePolyline(Path);
            if (Path.ConformToTerrain && Path.TargetTerrain != null)
                PathSampling.ProjectToTerrain(preview, Path.TargetTerrain, Path.VerticalOffset);
            Handles.color = Color.white;
            for (var i = 0; i < preview.Count - 1; i++) Handles.DrawLine(preview[i], preview[i + 1], 3f);
        }
    }
}

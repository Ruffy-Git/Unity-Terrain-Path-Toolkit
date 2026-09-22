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

            var points = PathSampling.Sample(Path);
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

            var filter = output.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = output.AddComponent<MeshFilter>();
                Undo.RegisterCreatedObjectUndo(filter, "Add Terrain Path Mesh Filter");
            }

            var renderer = output.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = output.AddComponent<MeshRenderer>();
                Undo.RegisterCreatedObjectUndo(renderer, "Add Terrain Path Mesh Renderer");
            }

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
            var changed = false;

            for (var i = 0; i < Path.ControlPoints.Count; i++)
            {
                var point = Path.GetWorldPoint(i);
                var size = HandleUtility.GetHandleSize(point) * 0.09f;

                Handles.color = i == 0 ? new Color(0.2f, 1f, 0.3f) :
                    i == Path.ControlPoints.Count - 1 ? new Color(1f, 0.35f, 0.2f) :
                    new Color(0.2f, 0.7f, 1f);

                Handles.SphereHandleCap(0, point, Quaternion.identity, size, EventType.Repaint);
                Handles.Label(point + Vector3.up * size * 1.5f, "P" + i);

                EditorGUI.BeginChangeCheck();
                var next = Handles.PositionHandle(point, Quaternion.identity);
                if (!EditorGUI.EndChangeCheck()) continue;

                Undo.RecordObject(Path, "Move Terrain Path Point");
                Path.SetWorldPoint(i, next);
                EditorUtility.SetDirty(Path);
                changed = true;
            }

            var preview = PathSampling.Sample(Path);
            if (Path.ConformToTerrain && Path.TargetTerrain != null)
                PathSampling.ProjectToTerrain(preview, Path.TargetTerrain, Path.VerticalOffset);

            Handles.color = new Color(1f, 0.85f, 0.15f);
            for (var i = 0; i < preview.Count - 1; i++)
                Handles.DrawAAPolyLine(4f, preview[i], preview[i + 1]);

            if (changed && Path.LivePreview && !HasExternalMeshEdits())
                Generate(false);

            SceneView.RepaintAll();
        }
    }
}

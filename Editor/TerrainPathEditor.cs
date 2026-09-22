using UnityEditor;
using UnityEngine;

namespace TerrainPathToolkit.Editor
{
    [CustomEditor(typeof(TerrainPath))]
    public sealed class TerrainPathEditor : UnityEditor.Editor
    {
        private TerrainPath Path => (TerrainPath)target;
        private int selectedPoint = -1;
        private bool tightTurnDetected;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (tightTurnDetected)
                EditorGUILayout.HelpBox("A tight turn may cause the path edges to overlap. Move the nearby control points farther apart, reduce Width, or add an intermediate point.", MessageType.Warning);

            var modified = HasExternalMeshEdits();
            if (modified)
                EditorGUILayout.HelpBox("The generated mesh differs from its recorded baseline. Regeneration is blocked until you adopt the current mesh or intentionally replace it.", MessageType.Warning);

            EditorGUILayout.LabelField("Scene Authoring", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(selectedPoint >= 0 ? "Selected Point: P" + selectedPoint : "Selected Point: None");

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Point")) AddPoint();
                using (new EditorGUI.DisabledScope(selectedPoint < 0))
                    if (GUILayout.Button("Insert After")) InsertAfterSelected();
                using (new EditorGUI.DisabledScope(selectedPoint < 0 || Path.ControlPoints.Count <= 2))
                    if (GUILayout.Button("Delete Selected")) DeleteSelected();
            }

            if (GUILayout.Button("Generate / Regenerate Path")) Generate(false);

            using (new EditorGUI.DisabledScope(Path.GeneratedObject == null))
                if (GUILayout.Button("Adopt Current Mesh as Baseline")) AdoptBaseline();

            if (modified && GUILayout.Button("Replace Manual Edits and Regenerate"))
                Generate(true);
        }

        private void AddPoint()
        {
            Undo.RecordObject(Path, "Add Terrain Path Point");
            var count = Path.ControlPoints.Count;
            var position = count > 0 ? Path.GetWorldPoint(count - 1) + Path.transform.forward * 5f : Path.transform.position;
            Path.AddWorldPoint(position);
            selectedPoint = Path.ControlPoints.Count - 1;
            EditorUtility.SetDirty(Path);
            RegenerateIfLive();
        }

        private void InsertAfterSelected()
        {
            if (selectedPoint < 0) return;
            var a = Path.GetWorldPoint(selectedPoint);
            Vector3 position;
            if (selectedPoint < Path.ControlPoints.Count - 1)
                position = Vector3.Lerp(a, Path.GetWorldPoint(selectedPoint + 1), 0.5f);
            else
                position = a + Path.transform.forward * 5f;

            Undo.RecordObject(Path, "Insert Terrain Path Point");
            Path.InsertWorldPoint(selectedPoint + 1, position);
            selectedPoint++;
            EditorUtility.SetDirty(Path);
            RegenerateIfLive();
        }

        private void DeleteSelected()
        {
            if (selectedPoint < 0 || Path.ControlPoints.Count <= 2) return;
            Undo.RecordObject(Path, "Delete Terrain Path Point");
            Path.RemovePoint(selectedPoint);
            selectedPoint = Mathf.Clamp(selectedPoint, 0, Path.ControlPoints.Count - 1);
            EditorUtility.SetDirty(Path);
            RegenerateIfLive();
        }

        private void RegenerateIfLive()
        {
            if (Path.LivePreview && !HasExternalMeshEdits()) Generate(false);
            SceneView.RepaintAll();
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
                    "This output has mesh edits outside the authoring tool. Adopt the current mesh as a manual baseline, or use Replace Manual Edits and Regenerate if you intentionally want to discard them.", "OK");
                return;
            }

            var points = PathSampling.Sample(Path);
            if (Path.ConformToTerrain && Path.TargetTerrain != null)
                PathSampling.ProjectToTerrain(points, Path.TargetTerrain, Path.VerticalOffset);
            if (points.Count < 2) return;

            tightTurnDetected = DetectTightTurn(points, Path.Width);

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
            var mesh = PathMeshBuilder.BuildStrip(points, Path.Width, output.transform,\n                Path.ConformToTerrain ? Path.TargetTerrain : null, Path.VerticalOffset);
            filter.sharedMesh = mesh;
            Path.BaselineFingerprint = MeshFingerprint.Calculate(mesh);
            if (oldMesh != null && oldMesh != mesh && !AssetDatabase.Contains(oldMesh)) DestroyImmediate(oldMesh);

            EditorUtility.SetDirty(Path);
            EditorUtility.SetDirty(output);
        }

        private static bool DetectTightTurn(System.Collections.Generic.IReadOnlyList<Vector3> points, float width)
        {
            if (points == null || points.Count < 3) return false;
            for (var i = 1; i < points.Count - 1; i++)
            {
                var a = points[i] - points[i - 1];
                var b = points[i + 1] - points[i];
                if (a.sqrMagnitude < 0.0001f || b.sqrMagnitude < 0.0001f) continue;
                var angle = Vector3.Angle(a, b);
                var localLength = Mathf.Min(a.magnitude, b.magnitude);
                if (angle > 35f && localLength < width * 1.5f) return true;
            }
            return false;
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
                var size = HandleUtility.GetHandleSize(point) * (i == selectedPoint ? 0.16f : 0.12f);
                Handles.color = i == selectedPoint ? Color.yellow :
                    i == 0 ? new Color(0.2f, 1f, 0.3f) :
                    i == Path.ControlPoints.Count - 1 ? new Color(1f, 0.35f, 0.2f) :
                    new Color(0.2f, 0.7f, 1f);

                if (Handles.Button(point, Quaternion.identity, size, size * 1.25f, Handles.SphereHandleCap))
                {
                    selectedPoint = i;
                    Repaint();
                }
                Handles.Label(point + Vector3.up * size * 1.6f, "P" + i);

                if (i != selectedPoint) continue;
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

            tightTurnDetected = DetectTightTurn(preview, Path.Width);
            Handles.color = tightTurnDetected ? new Color(1f, 0.35f, 0.15f) : new Color(1f, 0.85f, 0.15f);
            for (var i = 0; i < preview.Count - 1; i++)
                Handles.DrawAAPolyLine(4f, preview[i], preview[i + 1]);

            if (changed && Path.LivePreview && !HasExternalMeshEdits()) Generate(false);
        }
    }
}

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
            EditorGUILayout.HelpBox(
                "Move the scene handles to edit the path. Generated-mesh and terrain-conforming workflows are under active development.",
                MessageType.Info);

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(Path, "Add Terrain Path Point");
                var count = Path.ControlPoints.Count;
                var position = count > 0
                    ? Path.GetWorldPoint(count - 1) + Path.transform.forward * 5f
                    : Path.transform.position;
                Path.AddWorldPoint(position);
                EditorUtility.SetDirty(Path);
            }
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

            Handles.color = Color.white;
            for (var i = 0; i < Path.ControlPoints.Count - 1; i++)
                Handles.DrawLine(Path.GetWorldPoint(i), Path.GetWorldPoint(i + 1), 3f);
        }
    }
}

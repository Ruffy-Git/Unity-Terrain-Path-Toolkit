using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    /// <summary>
    /// Editable source data for a terrain path.
    /// Control points remain the source of truth; generated meshes are outputs.
    /// </summary>
    [ExecuteAlways]
    public sealed class TerrainPath : MonoBehaviour
    {
        [SerializeField] private List<Vector3> controlPoints = new()
        {
            new Vector3(-5f, 0f, 0f),
            new Vector3(5f, 0f, 0f)
        };

        [Min(0.1f)]
        [SerializeField] private float width = 3f;

        [Min(0.05f)]
        [SerializeField] private float sampleSpacing = 1f;

        [SerializeField] private bool conformToTerrain = true;

        public IReadOnlyList<Vector3> ControlPoints => controlPoints;
        public float Width => width;
        public float SampleSpacing => sampleSpacing;
        public bool ConformToTerrain => conformToTerrain;

        public Vector3 GetWorldPoint(int index) => transform.TransformPoint(controlPoints[index]);

        public void SetWorldPoint(int index, Vector3 worldPoint)
        {
            controlPoints[index] = transform.InverseTransformPoint(worldPoint);
        }

        public void AddWorldPoint(Vector3 worldPoint)
        {
            controlPoints.Add(transform.InverseTransformPoint(worldPoint));
        }

        public void RemovePoint(int index)
        {
            if (controlPoints.Count <= 2) return;
            controlPoints.RemoveAt(index);
        }
    }
}

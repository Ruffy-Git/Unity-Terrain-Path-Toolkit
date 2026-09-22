using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TerrainPathToolkit.Tests
{
    public class PathMeshBuilderTests
    {
        [Test]
        public void BuildStrip_TwoPoints_CreatesQuad()
        {
            var go = new GameObject("Path Test");
            try
            {
                var points = new List<Vector3> { Vector3.zero, Vector3.forward * 5f };
                var mesh = PathMeshBuilder.BuildStrip(points, 2f, go.transform);

                Assert.AreEqual(4, mesh.vertexCount);
                Assert.AreEqual(6, mesh.triangles.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

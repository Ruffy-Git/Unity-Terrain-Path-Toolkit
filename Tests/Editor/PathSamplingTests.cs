using NUnit.Framework;
using UnityEngine;

namespace TerrainPathToolkit.Tests
{
    public class PathSamplingTests
    {
        [Test]
        public void SamplePolyline_DefaultPath_ReturnsMultipleSamples()
        {
            var go = new GameObject("Path Sampling Test");
            try
            {
                var path = go.AddComponent<TerrainPath>();
                var samples = PathSampling.SamplePolyline(path);
                Assert.Greater(samples.Count, 2);
                Assert.AreEqual(path.GetWorldPoint(0), samples[0]);
                Assert.AreEqual(path.GetWorldPoint(path.ControlPoints.Count - 1), samples[samples.Count - 1]);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

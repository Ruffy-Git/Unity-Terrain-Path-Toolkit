using System.Collections.Generic;
using UnityEngine;

namespace TerrainPathToolkit
{
    public static class TerrainCarver
    {
        public static float[,] BuildCarvedHeights(TerrainPath path, IReadOnlyList<Vector3> centerline, float[,] source)
        {
            var terrain = path.TargetTerrain;
            var data = terrain.terrainData;
            var result = (float[,])source.Clone();
            var resolution = data.heightmapResolution;
            var origin = terrain.transform.position;
            var size = data.size;
            var halfPath = path.Width * 0.5f + path.CarveExtraWidth;
            var influence = halfPath + path.CarveBlendWidth;

            for (var z = 0; z < resolution; z++)
            {
                var worldZ = origin.z + z / (float)(resolution - 1) * size.z;
                for (var x = 0; x < resolution; x++)
                {
                    var worldX = origin.x + x / (float)(resolution - 1) * size.x;
                    var p = new Vector2(worldX, worldZ);
                    var bestDistance = float.MaxValue;
                    var targetY = 0f;

                    for (var i = 0; i < centerline.Count - 1; i++)
                    {
                        var a3 = centerline[i];
                        var b3 = centerline[i + 1];
                        var a = new Vector2(a3.x, a3.z);
                        var b = new Vector2(b3.x, b3.z);
                        var ab = b - a;
                        var denominator = ab.sqrMagnitude;
                        if (denominator < 0.000001f) continue;
                        var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denominator);
                        var nearest = a + ab * t;
                        var distance = Vector2.Distance(p, nearest);
                        if (distance >= bestDistance) continue;
                        bestDistance = distance;
                        targetY = Mathf.Lerp(a3.y, b3.y, t);
                    }

                    if (bestDistance > influence) continue;
                    var weight = bestDistance <= halfPath ? 1f :
                        1f - Mathf.SmoothStep(0f, 1f, (bestDistance - halfPath) / Mathf.Max(0.01f, path.CarveBlendWidth));
                    weight *= path.CarveStrength;
                    var currentY = origin.y + result[z, x] * size.y;
                    var blendedY = Mathf.Lerp(currentY, targetY, weight);
                    result[z, x] = Mathf.Clamp01((blendedY - origin.y) / size.y);
                }
            }
            return result;
        }
    }
}

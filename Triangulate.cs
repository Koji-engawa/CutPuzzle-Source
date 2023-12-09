using System.Collections.Generic;
using UnityEngine;

//頂点群が成す図形内部を重複のない三角形に分割するクラス
public class Triangulate
    {
        public static void EarCut(
            IList<Vector3> vertices,
            IList<int> resultIndices,
            int vertexStart = 0,
            int vertexCount = -1)
        {
            vertexCount = vertexCount < 0 ? vertices.Count : vertexCount;

            if (vertexCount < 3)
            {
                return;
            }

            List<int> candidateIndices = new List<int>();

            if (IsClockWise(vertices, vertexStart, vertexCount))
            {
                for (var i = 0; i < vertexCount; i++)
                {
                    candidateIndices.Add(vertexStart + i);
                }
            }
            else
            {
                for (var i = 0; i < vertexCount; i++)
                {
                    candidateIndices.Add(vertexStart + vertexCount - 1 - i);
                }
            }

            while (candidateIndices.Count >= 3)
            {
                var success = GetEar(vertices, candidateIndices, ref resultIndices, vertexStart, vertexCount);
                if (!success)
                {
                    break;
                }
            }
        }

        private static bool IsClockWise(IList<Vector3> vertices, int vertexStart, int vertexCount)
        {
            float sum = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 va = vertices[vertexStart + i];
                Vector3 vb = (i == vertexCount - 1)
                    ? vertices[vertexStart]
                    : vertices[vertexStart + i + 1];

                sum += va.x * vb.y - va.y * vb.x;
            }

            return sum < 0;
        }

        private static bool GetEar(
            IList<Vector3> vertices,
            List<int> candidateIndices,
            ref IList<int> resultIndices,
            int vertexStart,
            int vertexCount)
        {
            var hasEar = false;
            for (int i = 0; i < candidateIndices.Count; i++)
            {
                if (candidateIndices.Count <= 3)
                {
                    resultIndices.Add(candidateIndices[0]);
                    resultIndices.Add(candidateIndices[1]);
                    resultIndices.Add(candidateIndices[2]);
                    candidateIndices.RemoveRange(0, 3);
                    return true;
                }

                var length = candidateIndices.Count;
                var indexA = candidateIndices[(i + length - 1) % length];
                var indexB = candidateIndices[i];
                var indexC = candidateIndices[(i + 1) % length];
                var vertexA = vertices[indexA];
                var vertexB = vertices[indexB];
                var vertexC = vertices[indexC];

                if (!IsAngleLessThan180(vertexB, vertexA, vertexC))
                {
                    continue;
                }

                var isEar = true;
                for (var j = 0; j < vertexCount; j++)
                {
                    if (vertexStart + j == indexA
                        || vertexStart + j == indexB
                        || vertexStart + j == indexC)
                    {
                        continue;
                    }

                    var vp = vertices[vertexStart + j];

                    if (IsContain(vertexA, vertexB, vertexC, vp))
                    {
                        isEar = false;
                        break;
                    }
                }

                if (isEar)
                {
                    hasEar = true;
                    resultIndices.Add(indexA);
                    resultIndices.Add(indexB);
                    resultIndices.Add(indexC);
                    candidateIndices.RemoveAt(i);
                    i -= 1;
                }
            }

            if (!hasEar)
            {
                return false;
            }

            return true;
        }

        private static bool IsContain(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            var c1 = (b.x - a.x) * (p.y - b.y) - (b.y - a.y) * (p.x - b.x);
            var c2 = (c.x - b.x) * (p.y - c.y) - (c.y - b.y) * (p.x - c.x);
            var c3 = (a.x - c.x) * (p.y - a.y) - (a.y - c.y) * (p.x - a.x);

            return c1 > 0f && c2 > 0f && c3 > 0f || c1 < 0f && c2 < 0f && c3 < 0f;
        }

        private static bool IsAngleLessThan180(
            Vector3 o,
            Vector3 a,
            Vector3 b)
        {
            return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x) > 0;
        }
    }

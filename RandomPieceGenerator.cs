using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//ランダムな形状のピースを生成する
public class RandomPieceGenerator : MonoBehaviour
{
    private Material NGMaterial;

    public int maxVertices;
    public float minX = -1.3f;
    public float maxX = 1.3f;
    public float minY = -1.3f;
    public float maxY = 1.3f;

    private List<Vector2> points = new List<Vector2>();

    void Awake()
    {
        NGMaterial = Resources.Load<Material>("NGMaterial");
        Mesh mesh = GenerateRandomMesh();
        DisplayMesh(mesh);
    }

    //ランダムな形状のメッシュを生成する
    Mesh GenerateRandomMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        List<Vector2> uv;
        List<int> resultIndices = new List<int>();
        int vertexCount = UnityEngine.Random.Range(5, maxVertices + 1);
        vertices = new Vector3[vertexCount];
        
        //一番初めの点を生成
        Vector2 firstPoint = GenerateRandomPoint();
        points.Add(firstPoint);

        //2つ目〜最後の一つ前までの点を生成
        for (int i = 1; i < vertexCount; i++)
        {
            Vector2 newPoint = GenerateRandomPoint();

            //直前の点と新しい点を結ぶ線分が他の線分と交差しないか確認（交差しなくなるまでnewPointを作り直し）
            while (!IsValidSegment(points[points.Count - 1], newPoint))
            {
                newPoint = GenerateRandomPoint();
            }

            points.Add(newPoint);
        }

        //閉じた形状となるように、最後に一番初めと同じ座標の点を追加
        points.Add(firstPoint);

        //EarCutを使うために、一旦pointsリストを3次元配列に変換
        vertices = Array.ConvertAll(points.ToArray(), i => (Vector3)i);

        //生成した頂点群が成す図形内部を重複のない三角形に分割する
        Triangulate.EarCut(vertices, resultIndices);

        //メッシュに頂点とresultIndicesを設定
        mesh.SetVertices(vertices);
        mesh.SetIndices(resultIndices.ToArray(), MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        //uvを頂点数と同じ数だけ生成（値は適当でOK）
        uv = new List<Vector2>(vertices.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            uv.Add(new Vector2(0, 0));
        }

        mesh.uv = uv.ToArray();

        return mesh;
    }

    //ランダムな座標の点を生成
    Vector2 GenerateRandomPoint()
    {
        float x = UnityEngine.Random.Range(minX, maxX);
        float y = UnityEngine.Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

    //追加しようとする線分が2つの条件を満たすかどうか確認
    bool IsValidSegment(Vector2 startPoint, Vector2 endPoint)
    {
        //最後の点と最初の点を結ぶ線分が既存の線分と交差しないか確認
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (IsCrossing(endPoint, points[0], points[i], points[i + 1]))
                return false;
        }

        //全ての既存の線分と交差しないか確認
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (IsCrossing(startPoint, endPoint, points[i], points[i + 1]))
                return false;
        }

        return true;
    }

    //2つの線分が交差するかどうかを確認
    bool IsCrossing(Vector2 startPoint1, Vector2 endPoint1, Vector2 startPoint2, Vector2 endPoint2)
    {
        var vector1 = endPoint1 - startPoint1;
        var vector2 = endPoint2 - startPoint2;

        return Cross(vector1, startPoint2 - startPoint1) * Cross(vector1, endPoint2 - startPoint1) < 0 &&
               Cross(vector2, startPoint1 - startPoint2) * Cross(vector2, endPoint1 - startPoint2) < 0;
    }

    float Cross(Vector2 vector1, Vector2 vector2)
    {
        return vector1.x * vector2.y - vector1.y * vector2.x;
    }

    //生成したメッシュを使用してピースを生成する
    void DisplayMesh(Mesh mesh)
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        meshRenderer.material = NGMaterial;

        //新しいゲームオブジェクトにPolygonCollider2Dを追加し、メッシュから形状を設定
        PolygonCollider2D polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        polygonCollider.points = mesh.vertices.Select(v => new Vector2(v.x, v.y)).ToArray();
        polygonCollider.isTrigger = true;

        gameObject.AddComponent<Fit>();
        gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
    }
}
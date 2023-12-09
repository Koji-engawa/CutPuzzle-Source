using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//ピースをカットするクラス
public class Cutter : MonoBehaviour
{
    private List<Vector3> vertices;
    private List<Color32> colors;
    private List<Vector2> uv;
    private List<int> indices;

    private CutResult _resultsA = new CutResult();
    private CutResult _resultsB = new CutResult();

    private Color fillColor = Color.white; // 塗りつぶす色

    private LineRenderer lineRenderer;
    private Vector2 mouseStart;
    private Vector2 mouseEnd;

    private AudioSource cutSE;

    private GameObject[] frameObjects;
    private GameObject frameObject;
    private ClearChecker clearChecker;

    private GameController gameController;

    private Material NGmaterial;

    private bool isHitPiece = false; //カットラインの始点がピース内部かどうか

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        cutSE = GetComponent<AudioSource>();
        frameObjects = GameObject.FindGameObjectsWithTag("Frame");
        frameObject = frameObjects[0];
        clearChecker = frameObject.GetComponent<ClearChecker>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        NGmaterial = Resources.Load<Material>("NGmaterial");
    }

    //ドラッグ&ドロップまたは一本指スワイプでカットラインを指定してカットを実行
    void Update()
    {
    #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 1 && Input.GetMouseButtonDown(0))
    #else
        if (Input.GetMouseButtonDown(0))
    #endif
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.tag == "Piece")
            {
                isHitPiece = true;
                return;
            }
            else
                isHitPiece = false;

            mouseStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        //カットラインの始点がピース内部かどうかでカットラインの表示・非表示を切り替える
    #if UNITY_ANDROID || UNITY_IOS
        if (!isHitPiece && Input.touchCount == 1 && Input.GetMouseButton(0))
    #else
        if (Input.GetMouseButton(0) && !isHitPiece)
    #endif
        {
            lineRenderer.enabled = true;
            mouseEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.SetPosition(0, mouseStart);
            lineRenderer.SetPosition(1, mouseEnd);
        }
        else
        {
            lineRenderer.enabled = false;
        }

        //カットラインがピースに触れていればカット実行
    #if UNITY_ANDROID || UNITY_IOS
        if (!isHitPiece && Input.GetMouseButtonUp(0))
    #else
        if (Input.GetMouseButtonUp(0) && !isHitPiece)
    #endif
        {
            RaycastHit2D hit = Physics2D.Linecast(mouseStart, mouseEnd);
            if (hit.collider != null && hit.collider.tag == "Piece")
            {
                //残り回数のチェックと更新
                if (gameController.restCount > 0)
                    gameController.UpdateCount();
                else return;

                //カットの実行とカット前のピースを削除（clearCheckerの監視対象からも削除）
                if (gameController.restCount >= 0)
                {
                    Mesh mesh = hit.collider.GetComponent<MeshFilter>().mesh;
                    vertices = new List<Vector3>(mesh.vertices);
                    colors = mesh.colors32.Length > 0 ? new List<Color32>(mesh.colors32) : new List<Color32>(new Color32[mesh.vertexCount]);
                    uv = new List<Vector2>(mesh.uv);
                    indices = new List<int>(mesh.GetIndices(0));

                    Transform meshTransform = hit.collider.transform;
                    Destroy(hit.collider.gameObject);

                    Cut(meshTransform, vertices, colors, uv, indices, indices.Count, mouseStart.x, mouseStart.y, mouseEnd.x, mouseEnd.y, _resultsA, _resultsB);
                    clearChecker.pieces.Remove(hit.collider.gameObject);
                    clearChecker.fits.Remove(hit.collider.gameObject.GetComponent<Fit>());
                }
            }
        }
    }

    //メッシュをカットするメソッド
    void Cut(
        Transform meshTransform,
        IList<Vector3> vertices,
        IList<Color32> colors,
        IList<Vector2> uv,
        IList<int> indices,
        int indexCount,
        float x1,
        float y1,
        float x2,
        float y2,
        CutResult _resultsA,
        CutResult _resultsB)
    {
        _resultsA.Clear();
        _resultsB.Clear();

        //メッシュの頂点をローカル座標からワールド座標に変換
        List<Vector3> worldVertices = vertices.Select(v => meshTransform.TransformPoint(v)).ToList();

        for (int i = 0; i < indexCount; i += 3)
        {
            int indexA = indices[i + 0];
            int indexB = indices[i + 1];
            int indexC = indices[i + 2];
            Vector3 a = worldVertices[indexA];
            Vector3 b = worldVertices[indexB];
            Vector3 c = worldVertices[indexC];
            Color color = colors[indexA];
            float uvA_X = uv[indexA].x;
            float uvA_Y = uv[indexA].y;
            float uvB_X = uv[indexB].x;
            float uvB_Y = uv[indexB].y;
            float uvC_X = uv[indexC].x;
            float uvC_Y = uv[indexC].y;

            bool aSide = IsClockWise(x1, y1, x2, y2, a.x, a.y);
            bool bSide = IsClockWise(x1, y1, x2, y2, b.x, b.y);
            bool cSide = IsClockWise(x1, y1, x2, y2, c.x, c.y);
            if (aSide == bSide && aSide == cSide)
            {
                var triangleResult = aSide ? _resultsA : _resultsB;
                triangleResult.AddTriangle(
                    a.x, a.y, b.x, b.y, c.x, c.y,
                    uvA_X, uvA_Y, uvB_X, uvB_Y, uvC_X, uvC_Y,
                    color);
            }
            else if (aSide != bSide && aSide != cSide)
            {
                float abX, abY, caX, caY, uvAB_X, uvAB_Y, uvCA_X, uvCA_Y;
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    a.x, a.y,
                    b.x, b.y,
                    uvA_X, uvA_Y,
                    uvB_X, uvB_Y,
                    out abX, out abY,
                    out uvAB_X, out uvAB_Y);
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    c.x, c.y,
                    a.x, a.y,
                    uvC_X, uvC_Y,
                    uvA_X, uvA_Y,
                    out caX, out caY,
                    out uvCA_X, out uvCA_Y);
                var triangleResult = aSide ? _resultsA : _resultsB;
                var rectangleResult = aSide ? _resultsB : _resultsA;
                triangleResult.AddTriangle(
                    a.x, a.y,
                    abX, abY,
                    caX, caY,
                    uvA_X, uvA_Y,
                    uvAB_X, uvAB_Y,
                    uvCA_X, uvCA_Y,
                    color);
                rectangleResult.AddRectangle(
                    b.x, b.y,
                    c.x, c.y,
                    caX, caY,
                    abX, abY,
                    uvB_X, uvB_Y,
                    uvC_X, uvC_Y,
                    uvCA_X, uvCA_Y,
                    uvAB_X, uvAB_Y,
                    color);
            }
            else if (bSide != aSide && bSide != cSide)
            {
                float abX, abY, bcX, bcY, uvAB_X, uvAB_Y, uvBC_X, uvBC_Y;
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    a.x, a.y,
                    b.x, b.y,
                    uvA_X, uvA_Y,
                    uvB_X, uvB_Y,
                    out abX, out abY,
                    out uvAB_X, out uvAB_Y);
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    b.x, b.y,
                    c.x, c.y,
                    uvB_X, uvB_Y,
                    uvC_X, uvC_Y,
                    out bcX, out bcY,
                    out uvBC_X, out uvBC_Y);
                var triangleResult = bSide ? _resultsA : _resultsB;
                var rectangleResult = bSide ? _resultsB : _resultsA;
                triangleResult.AddTriangle(
                    b.x, b.y,
                    bcX, bcY,
                    abX, abY,
                    uvB_X, uvB_Y,
                    uvBC_X, uvBC_Y,
                    uvAB_X, uvAB_Y,
                    color);
                rectangleResult.AddRectangle(
                    c.x, c.y,
                    a.x, a.y,
                    abX, abY,
                    bcX, bcY,
                    uvC_X, uvC_Y,
                    uvA_X, uvA_Y,
                    uvAB_X, uvAB_Y,
                    uvBC_X, uvBC_Y,
                    color);
            }
            else if (cSide != aSide && cSide != bSide)
            {
                float bcX, bcY, caX, caY, uvBC_X, uvBC_Y, uvCA_X, uvCA_Y;
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    b.x, b.y,
                    c.x, c.y,
                    uvB_X, uvB_Y,
                    uvC_X, uvC_Y,
                    out bcX, out bcY,
                    out uvBC_X, out uvBC_Y);
                GetIntersectionLineAndLineStrip(
                    x1, y1,
                    x2, y2,
                    c.x, c.y,
                    a.x, a.y,
                    uvC_X, uvC_Y,
                    uvA_X, uvA_Y,
                    out caX, out caY,
                    out uvCA_X, out uvCA_Y);
                var triangleResult = cSide ? _resultsA : _resultsB;
                var rectangleResult = cSide ? _resultsB : _resultsA;
                triangleResult.AddTriangle(
                    c.x, c.y,
                    caX, caY,
                    bcX, bcY,
                    uvC_X, uvC_Y,
                    uvCA_X, uvCA_Y,
                    uvBC_X, uvBC_Y,
                    color);
                rectangleResult.AddRectangle(
                    a.x, a.y,
                    b.x, b.y,
                    bcX, bcY,
                    caX, caY,
                    uvA_X, uvA_Y,
                    uvB_X, uvB_Y,
                    uvBC_X, uvBC_Y,
                    uvCA_X, uvCA_Y,
                    color);
            }
        }

        //メッシュの中心を計算
        Vector3 centerA = new Vector3(0,0,0);

        for (int i = 0; i < _resultsA.vertices.Count; i++)
        {
            centerA += _resultsA.vertices[i];
        }
        centerA /= _resultsA.vertices.Count;

        Vector3 centerB = new Vector3(0, 0, 0);

        for (int i = 0; i < _resultsB.vertices.Count; i++)
        {
            centerB += _resultsB.vertices[i];
        }
        centerB /= _resultsB.vertices.Count;

        //原点を中心としたメッシュに変換
        for (int i = 0; i < _resultsA.vertices.Count; i++)
        {
            _resultsA.vertices[i] -= centerA;
        }

        for (int i = 0; i < _resultsB.vertices.Count; i++)
        {
            _resultsB.vertices[i] -= centerB;
        }

        CreateCutObject(_resultsA, "CutObjectA", centerA);
        CreateCutObject(_resultsB, "CutObjectB", centerB);
        cutSE.Play();
    }

    //カット結果を使用して2つのピースを生成
    GameObject CreateCutObject(CutResult result, string name, Vector3 center)
    {
        //結果データの取得
        List<Vector3> cutVertices = result.vertices;
        List<Color32> cutColors = result.colors;
        List<Vector2> cutUv = result.uv;
        List<int> cutIndices = result.indices;

        //取得したデータを使用してメッシュを生成
        GameObject cutObject = new GameObject(name);
        Mesh cutMesh = new Mesh();
        cutMesh.SetVertices(cutVertices);
        cutMesh.SetColors(cutColors);
        cutMesh.SetUVs(0, cutUv);
        cutMesh.SetIndices(cutIndices.ToArray(), MeshTopology.Triangles, 0);
        cutMesh.RecalculateNormals();
        MeshFilter meshFilter = cutObject.AddComponent<MeshFilter>();
        meshFilter.mesh = cutMesh;
        MeshRenderer meshRenderer = cutObject.AddComponent<MeshRenderer>();
        meshRenderer.material = NGmaterial;
        MeshColoring.Coloring(cutMesh, fillColor);

        //生成したメッシュを使用してピースを生成
        PolygonCollider2D polygonCollider = cutObject.AddComponent<PolygonCollider2D>();
        polygonCollider.points = cutVertices.Select(v => new Vector2(v.x, v.y)).ToArray();
        cutObject.AddComponent<Rigidbody2D>().isKinematic = true;
        cutObject.AddComponent<Fit>();
        cutObject.tag = "Piece";

        //カットメッシュの中心座標までpositionを移動
        cutObject.transform.position += center;

        //カットラインの方向ベクトル
        Vector2 cutLineDir = (mouseEnd - mouseStart).normalized;

        //方向ベクトルに対して垂直なベクトル
        Vector2 perp = new Vector2(cutLineDir.y, -cutLineDir.x);

        //カットしたことが視覚的にわかりやすいように、それぞれ反対方向に小さく移動
        if (name == "CutObjectA")
        {
            cutObject.transform.Translate(-perp * 0.08f);
        }
        else if (name == "CutObjectB")
        {
            cutObject.transform.Translate(perp * 0.08f);
        }

        //clearCheckerの監視対象に追加
        clearChecker.pieces.Add(cutObject);
        clearChecker.fits.Add(cutObject.GetComponent<Fit>());

        return cutObject;
    }

    void GetIntersectionLineAndLineStrip(
        float x1, float y1, // Line Point
        float x2, float y2, // Line Point
        float x3, float y3, // Line Strip Point
        float x4, float y4, // Line Strip Point
        float uv3_X, float uv3_Y,
        float uv4_X, float uv4_Y,
        out float x, out float y,
        out float uvX, out float uvY)
    {
        float s1 = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
        float s2 = (x2 - x1) * (y1 - y4) - (y2 - y1) * (x1 - x4);

        float c = s1 / (s1 + s2);

        x = x3 + (x4 - x3) * c;
        y = y3 + (y4 - y3) * c;

        uvX = uv3_X + (uv4_X - uv3_X) * c;
        uvY = uv3_Y + (uv4_Y - uv3_Y) * c;
    }

    bool IsClockWise(
        float x1, float y1,
        float x2, float y2,
        float x3, float y3)
    {
        return (x2 - x1) * (y3 - y2) - (y2 - y1) * (x3 - x2) > 0;
    }
}

//Cutterの結果を管理するクラス
public class CutResult
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<Color32> colors = new List<Color32>();
    public List<int> indices = new List<int>();
    public List<Vector2> uv = new List<Vector2>();

    public void Clear()
    {
        vertices.Clear();
        colors.Clear();
        uv.Clear();
        indices.Clear();
    }

    public void AddTriangle(
        float x1, float y1,
        float x2, float y2,
        float x3, float y3,
        float uv1X, float uv1Y,
        float uv2X, float uv2Y,
        float uv3X, float uv3Y,
        Color color)
    {
        int vertexCount = vertices.Count;
        vertices.Add(new Vector3(x1, y1, 0));
        vertices.Add(new Vector3(x2, y2, 0));
        vertices.Add(new Vector3(x3, y3, 0));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        uv.Add(new Vector2(uv1X, uv1Y));
        uv.Add(new Vector2(uv2X, uv2Y));
        uv.Add(new Vector2(uv3X, uv3Y));
        indices.Add(vertexCount + 2);
        indices.Add(vertexCount + 1);
        indices.Add(vertexCount + 0);
    }

    public void AddRectangle(
        float x1, float y1,
        float x2, float y2,
        float x3, float y3,
        float x4, float y4,
        float uv1_X, float uv1_Y,
        float uv2_X, float uv2_Y,
        float uv3_X, float uv3_Y,
        float uv4_X, float uv4_Y,
        Color color)
    {
        int vertexCount = vertices.Count;
        vertices.Add(new Vector3(x1, y1, 0));
        vertices.Add(new Vector3(x2, y2, 0));
        vertices.Add(new Vector3(x3, y3, 0));
        vertices.Add(new Vector3(x4, y4, 0));
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        uv.Add(new Vector2(uv1_X, uv1_Y));
        uv.Add(new Vector2(uv2_X, uv2_Y));
        uv.Add(new Vector2(uv3_X, uv3_Y));
        uv.Add(new Vector2(uv4_X, uv4_Y));
        indices.Add(vertexCount + 2);
        indices.Add(vertexCount + 1);
        indices.Add(vertexCount + 0);
        indices.Add(vertexCount + 0);
        indices.Add(vertexCount + 3);
        indices.Add(vertexCount + 2);
    }
}
using UnityEngine;

//メッシュの内部を単色で塗りつぶす
public class MeshColoring : MonoBehaviour
{
    Color fillColor = Color.white; // 塗りつぶす色

    void Awake()
    {
        Coloring(this.gameObject.GetComponent<MeshFilter>().mesh,fillColor);
    }

    public static void Coloring(Mesh cutMesh, Color fillColor)
    {
        int vertexCount = cutMesh.vertexCount;

        // メッシュの頂点カラーを初期化（塗りつぶす前に必要）
        Color[] colors = new Color[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            colors[i] = Color.white; // すべての頂点を白色で初期化（変更前のカラーを指定）
        }

        int[] triangles = cutMesh.triangles;

        for (int i = 0; i < triangles.Length; i++)
        {
            int vertexIndex = triangles[i];
            colors[vertexIndex] = fillColor;
        }

        cutMesh.colors = colors;
    }
}

using UnityEngine;

//個別のピースがフレームに収まっているかをチェックする
public class Fit : MonoBehaviour
{
    private GameObject[] frameObjects;
    private GameObject frameObject;
    private EdgeCollider2D frameEdge;
    private PolygonCollider2D framePolygon;
    private ClearChecker clearChecker;

    private bool inPolygon = false;
    private bool inEdge = false;
    public bool fitted = false;

    private Material OKmaterial;
    private Material NGmaterial;

    private SoundManager soundManager;

    //ピースに持たせるスクリプトなので、ピースとして必要な設定をする
    void Awake()
    {
        SortingLayer sortingLayer = gameObject.AddComponent<SortingLayer>();
        sortingLayer.LayerName = "Default";
        sortingLayer.OrderInLayer = 0;
        gameObject.tag = "Piece";
    }

    void Start()
    {
        //現在はフレームはステージごとに一つだが、後々複数にする可能性を考慮してタグで管理
        frameObjects = GameObject.FindGameObjectsWithTag("Frame");
        frameObject = frameObjects[0];
        clearChecker = frameObject.GetComponent<ClearChecker>();
        frameEdge = frameObject.GetComponent<EdgeCollider2D>();
        framePolygon = frameObject.GetComponent<PolygonCollider2D>();
        OKmaterial = Resources.Load<Material>("OKmaterial");
        NGmaterial = Resources.Load<Material>("NGmaterial");
        soundManager = GameObject.FindObjectOfType<SoundManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UpdateState(other, false);
        //誤判定防止のため時間差でチェック（稀にframePolygonとframeEdgeの検出順序が逆転することがある）
        Invoke("CheckState", 0.01f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        UpdateState(other, true);
        //誤判定防止のため時間差でチェック（稀にframePolygonとframeEdgeの検出順序が逆転することがある）
        Invoke("CheckState", 0.01f);
    }

    void UpdateState(Collider2D other, bool state)
    {
        if (other == framePolygon)
            inPolygon = !state;

        if (other == frameEdge)
            inEdge = state;
    }

    void CheckState()
    {
        if (inEdge && inPolygon)
        {
            fitted = true;
            GetComponent<MeshRenderer>().material = OKmaterial;
            clearChecker.ClearCheck();
            soundManager.Play(soundManager.OKSE);
        }
        else
        {
            fitted = false;
            if (GetComponent<MeshRenderer>().sharedMaterial == OKmaterial)
                GetComponent<MeshRenderer>().material = NGmaterial;
            soundManager.Play(soundManager.NGSE);
        }
    }
}

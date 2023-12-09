using UnityEngine;

//ピースの選択・移動・回転に関するクラス
public class PieceMovement : MonoBehaviour
{
    private GameObject targetObject;
    private Rigidbody2D rb;

    public float moveSpeed = 5f;
    public float rotationSpeed = 70f;

    private bool RightbuttonDownFlag = false;
    private bool LeftbuttonDownFlag = false;

    public LayerMask layerMask;
    public SoundManager soundManager;

    private Material ActiveMaterial;
    private Material NGmaterial;

    private float elapsedTime = 0.0f;
    public float duration = 0.5f; // 色の変化にかかる時間

    public Color startColor; // 開始色
    public Color endColor; // 終了色

    private bool isChangingColor = false;
    private float t;

    private bool isDragging = false;
    private Vector3 touchOffset;

    void Start()
    {
        ActiveMaterial = Resources.Load<Material>("ActiveMaterial");
        NGmaterial = Resources.Load<Material>("NGmaterial");
    }

    void Update()
    {
        //ピースを掴んだことが視覚的にわかるように、一瞬色を変化させる（マテリアルを変更するだけでは、既に選択中のピースを再度掴むときに視覚的な変化がなくわかりづらいので）
        if (isChangingColor)
        {
            elapsedTime += Time.deltaTime;
            t = Mathf.Clamp01(elapsedTime / duration);
            ActiveMaterial.color = Color.Lerp(startColor, endColor, t);

            if (elapsedTime >= duration)
            {
                isChangingColor = false;
                elapsedTime = 0.0f;
            }
        }

        //左クリックでピースの選択
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 0, layerMask);

            if (hit.collider != null)
            {
                //直前に選択していたピースはKinematicにする
                if (rb != null)
                {
                    if (rb.gameObject.GetComponent<MeshRenderer>().sharedMaterial == ActiveMaterial)
                        rb.gameObject.GetComponent<MeshRenderer>().material = NGmaterial;

                    rb.isKinematic = true;
                }

                //新しく選択したピースはDynamicにする
                targetObject = hit.collider.gameObject;
                rb = targetObject.GetComponent<Rigidbody2D>();
                rb.isKinematic = false;
                rb.gravityScale = 0;
                rb.centerOfMass = new Vector2(0, 0);
                rb.gameObject.GetComponent<MeshRenderer>().material = ActiveMaterial;
                isChangingColor = true;

                //クリックした座標とピースの相対位置を取得
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                touchOffset = targetObject.transform.position - touchPosition;
                isDragging = true;
            }

            else
            {
                if (rb != null)
                    rb.isKinematic = true;
            }
        }

        //ピースの移動と回転
        if (rb != null)
        {
            //キーボード入力による移動
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 movement = new Vector2(horizontalInput, verticalInput) * moveSpeed;
            rb.velocity = movement;

            if (RightbuttonDownFlag)
                RightRotation();

            if (LeftbuttonDownFlag)
                LeftRotation();
        }

        //ドラッグするとピースがカーソルに追従して移動する
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rb.position = touchPosition + touchOffset;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

    }

    //以下、回転ボタン（UI）に登録するメソッド
    public void RightRotation()
    {
        rb.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        soundManager.Play(soundManager.rotationSE);
    }

    public void LeftRotation()
    {
        rb.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        soundManager.Play(soundManager.rotationSE);
    }

    public void OnRightButtonDown()
    {
        RightbuttonDownFlag = true;
    }

    public void OnRightButtonUp()
    {
        RightbuttonDownFlag = false;
    }

    public void OnLeftButtonDown()
    {
        LeftbuttonDownFlag = true;
    }

    public void OnLeftButtonUp()
    {
        LeftbuttonDownFlag = false;
    }
}

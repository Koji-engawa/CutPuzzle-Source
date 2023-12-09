using UnityEngine;

//カメラのズームとパン（視点移動）機能（マウス、タッチ操作両対応）
public class CameraZoomAndPan : MonoBehaviour
{
    public float zoomSpeedTouch = 0.005f;
    public float zoomSpeedMouse = 0.5f;
    public float panSpeed = 27.0f;
    public float minZoom = 1.0f;
    public float maxZoom = 30.0f;

    private Camera mainCamera;
    private Vector3 lastPanPosition;
    private Vector3 panOrigin;
    private bool isPanning = false;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // マウスホイールでズーム
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, zoomSpeedMouse);

        // ピンチイン・ピンチアウトでズーム
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            ZoomCamera(deltaMagnitudeDiff, zoomSpeedTouch);
        }

        // 右クリック・または3本指スライドでパン
        PanCamera();
    }

    void ZoomCamera(float offset, float speed)
    {
        if (mainCamera == null)
            return;

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + offset * speed, minZoom, maxZoom);
    }

    void PanCamera()
    {
        if (mainCamera == null)
            return;

        if (Input.GetMouseButtonDown(1) || (Input.touchCount >= 3 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(1).phase == TouchPhase.Began && Input.GetTouch(2).phase == TouchPhase.Began))
        {
            lastPanPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
            panOrigin = mainCamera.transform.position;
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(1) || (Input.touchCount >= 3 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended || Input.GetTouch(2).phase == TouchPhase.Ended)))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 newPanPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition) - lastPanPosition;
            Vector3 newPos = panOrigin - new Vector3(newPanPosition.x * panSpeed, newPanPosition.y * panSpeed, 0);

            mainCamera.transform.position = newPos;
        }
    }
}

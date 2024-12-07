using UnityEngine;
using Cinemachine;

public class CinematicCamera : MonoBehaviour
{
    [Header("Настройки")]
    public PolygonCollider2D sceneBounds;
    public float transitionSpeed = 2f;

    private Camera cam;
    private CinemachineVirtualCamera virtualCamera;
    private Vector3 targetPosition;
    private float targetSize;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        cam = GetComponent<Camera>();
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (cam == null && virtualCamera == null)
        {
            Debug.LogError("Не найдена ни обычная камера, ни виртуальная камера Cinemachine!");
            return;
        }

        if (sceneBounds == null)
        {
            Debug.LogError("Не указан коллайдер сцены!");
            return;
        }

        isInitialized = true;
        UpdateCameraBounds();
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * transitionSpeed);
        }
        else if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetSize, Time.deltaTime * transitionSpeed);
        }
    }

    void UpdateCameraBounds()
    {
        if (!isInitialized) return;

       
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        Vector2[] points = sceneBounds.points;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 worldPoint = sceneBounds.transform.TransformPoint(points[i]);
            min.x = Mathf.Min(min.x, worldPoint.x);
            min.y = Mathf.Min(min.y, worldPoint.y);
            max.x = Mathf.Max(max.x, worldPoint.x);
            max.y = Mathf.Max(max.y, worldPoint.y);
        }

        Vector2 boundsSize = max - min;
        Vector2 boundsCenter = (max + min) * 0.5f;

        
        targetSize = boundsSize.y * 0.5f;

       
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredWidth = targetSize * 2f * screenRatio;

       
        if (requiredWidth > boundsSize.x)
        {
            targetSize = boundsSize.x / (2f * screenRatio);
        }

        targetPosition = new Vector3(boundsCenter.x, boundsCenter.y, transform.position.z);
    }

    public void SetNewBounds(PolygonCollider2D newBounds)
    {
        sceneBounds = newBounds;
        if (isInitialized)
        {
            UpdateCameraBounds();
        }
    }

    void OnDrawGizmos()
    {
        if (sceneBounds != null)
        {
            Gizmos.color = Color.green;
            Vector2[] points = sceneBounds.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = sceneBounds.transform.TransformPoint(points[i]);
                Vector2 nextPoint = sceneBounds.transform.TransformPoint(points[(i + 1) % points.Length]);
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
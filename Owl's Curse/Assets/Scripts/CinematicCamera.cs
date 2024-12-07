using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [Header("Настройки границ")]
    public PolygonCollider2D sceneBounds; 

    [Header("Настройки камеры")]
    [Tooltip("Скорость перехода камеры к новым границам")]
    public float transitionSpeed = 2f;
    [Tooltip("Отступ от границ коллайдера")]
    public float padding = 1f;

    private Camera cam;
    private Vector3 targetPosition;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Камера не найдена!");
            return;
        }

        if (sceneBounds == null)
        {
            Debug.LogError("Не указан коллайдер сцены!");
            return;
        }

      
        UpdateCameraBounds();
    }

    void LateUpdate()
    {
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * transitionSpeed);
    }

    void UpdateCameraBounds()
    {
       
        Vector2 boundsSize = sceneBounds.bounds.size;
        Vector2 boundsCenter = sceneBounds.bounds.center;

       
        float screenRatio = (float)Screen.width / Screen.height;
        float boundsRatio = boundsSize.x / boundsSize.y;

        if (boundsRatio > screenRatio)
        {
            
            targetSize = (boundsSize.x / screenRatio) * 0.5f;
        }
        else
        {
          
            targetSize = boundsSize.y * 0.5f;
        }

        
        targetSize += padding;

 
        targetPosition = new Vector3(boundsCenter.x, boundsCenter.y, transform.position.z);
    }

    
    public void SetNewBounds(BoxCollider2D newBounds)
    {
        sceneBounds = newBounds;
        UpdateCameraBounds();
    }

   
    void OnDrawGizmos()
    {
        if (sceneBounds != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(sceneBounds.bounds.center, sceneBounds.bounds.size);
        }
    }
}
using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    private Camera mainCamera;
    private BoxCollider2D boundaryCollider;

    void Start()
    {
        mainCamera = Camera.main;
        boundaryCollider = GetComponent<BoxCollider2D>();
        CreateCameraBoundary();
    }

    void CreateCameraBoundary()
    {
        if (mainCamera == null || boundaryCollider == null) return;

        // Collider size exactly screen size
        Vector2 screenSize = GetScreenWorldSize();
        boundaryCollider.size = new Vector2(screenSize.x, screenSize.y);
        boundaryCollider.isTrigger = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("Wrap object");
            WrapObject(other.transform);
        } else if (other.CompareTag("Bullet")) {
            Destroy(other.gameObject);
        }
    }

    void WrapObject(Transform objectToWrap)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(objectToWrap.position);
        Vector3 newPosition = objectToWrap.position;

        Debug.Log("Vwpos x: " + viewportPos.x + " vwpos y: " + viewportPos.y);

        // Determine which side the object exited and wrap accordingly
        if (viewportPos.x > 1.0f || viewportPos.x < 0f)
        {
            newPosition.x = -newPosition.x;
        }
        if (viewportPos.y > 1.0f || viewportPos.y < 0f)
        {
            newPosition.y = -newPosition.y;
        }

        objectToWrap.position = newPosition;
    }

    Vector2 GetScreenWorldSize()
    {
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;
        return new Vector2(width, height);
    }

    void Update()
    {
        // Re-enforce aspect ratio if screen size changes (optional)
        if (Screen.width != Screen.currentResolution.width ||
            Screen.height != Screen.currentResolution.height)
        {
            CreateCameraBoundary();
        }
    }
}
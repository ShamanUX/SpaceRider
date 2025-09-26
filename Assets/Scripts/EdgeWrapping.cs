using UnityEngine;

public class EdgeWrapping : MonoBehaviour
{
    private Camera mainCamera;
    private Renderer objectRenderer;
    private bool isWrappingX = false;
    private bool isWrappingY = false;

    void Start()
    {
        mainCamera = Camera.main;
        objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        ScreenWrap();
    }

    void ScreenWrap()
    {
        bool isVisible = IsVisible();
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        Vector3 newPosition = transform.position;


        if (isVisible)
        {
            isWrappingX = false;
            isWrappingY = false;
            return;
        }

        if (isWrappingX && isWrappingY)
        {
            return;
        }



        if (!isWrappingX && (viewportPos.x > 1.2f || viewportPos.x < -0.2f))
        {
            newPosition.x = -newPosition.x;
            isWrappingX = true;
        }

        if (!isWrappingY && (viewportPos.y > 1.2f || viewportPos.y < -0.2f))
        {
            newPosition.y = -newPosition.y;
            isWrappingY = true;
        }

        transform.position = newPosition;
    }

    bool IsVisible()
    {
        if (objectRenderer == null) return true;
        return objectRenderer.isVisible;
    }
}
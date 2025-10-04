using UnityEngine;

public class AspectRatioEnforcer : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f; // 16:9 aspect ratio

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        ForceAspect();
    }

    void Update()
    {
        // Optional: continuously check for aspect ratio changes
        if (Application.isEditor)
        {
            ForceAspect();
        }
    }

    void ForceAspect()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float scaleHeight = currentAspect / targetAspect;

        Rect rect = mainCamera.rect;

        if (scaleHeight < 1.0f)
        {
            // Letterbox (black bars on top and bottom)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // Pillarbox (black bars on sides)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        mainCamera.rect = rect;
    }
}
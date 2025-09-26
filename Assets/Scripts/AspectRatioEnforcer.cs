using UnityEngine;

public class AspectRatioEnforcer : MonoBehaviour
{
    [Header("Target Aspect Ratio")]
    public float targetAspectWidth = 16f;
    public float targetAspectHeight = 9f;

    private Camera mainCamera;
    private float targetAspectRatio;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetAspectRatio = targetAspectWidth / targetAspectHeight;
        EnforceAspectRatio();
    }

    void EnforceAspectRatio()
    {
        // Calculate current aspect ratio
        float currentAspectRatio = (float)Screen.width / Screen.height;

        // Calculate the ratio between target and current aspect ratios
        float scaleHeight = currentAspectRatio / targetAspectRatio;

        // Create a rect for the camera viewport
        Rect rect = new Rect(0, 0, 1, 1);

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

    void Update()
    {
        // Re-enforce aspect ratio if screen size changes (optional)
        if (Screen.width != Screen.currentResolution.width ||
            Screen.height != Screen.currentResolution.height)
        {
            EnforceAspectRatio();
        }
    }
}
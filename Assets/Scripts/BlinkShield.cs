using UnityEngine;
using System.Collections;

public class BlinkShield : MonoBehaviour
{
    [Header("Blinking Opacity Settings")]
    [Range(0f, 1f)]
    public float minOpacity = 0.2f;
    [Range(0f, 1f)]
    public float maxOpacity = 0.8f;
    public float blinkSpeed = 1f;
    public bool startOnAwake = true;

    private Material material;
    private Color originalColor;
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;
        originalColor = material.color;

        if (startOnAwake)
            StartBlinking();
    }

    public void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            blinkCoroutine = StartCoroutine(BlinkOpacityRoutine());
        }
    }

    public void StopBlinking()
    {
        if (isBlinking)
        {
            isBlinking = false;
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            // Reset to original opacity
            SetOpacity(originalColor.a);
        }
    }

    private IEnumerator BlinkOpacityRoutine()
    {
        while (isBlinking)
        {
            // Smooth opacity pulse using sine wave
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            float currentOpacity = Mathf.Lerp(minOpacity, maxOpacity, t);

            SetOpacity(currentOpacity);
            yield return null;
        }
    }

    private void SetOpacity(float alpha)
    {
        Color newColor = material.color;
        newColor.a = Mathf.Clamp01(alpha);
        material.color = newColor;
    }

    void OnDestroy()
    {
        StopBlinking();
    }
}
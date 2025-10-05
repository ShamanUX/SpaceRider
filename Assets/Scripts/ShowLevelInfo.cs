using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ShowLevelInfo : MonoBehaviour
{
    [Header("References")]
    private TextMeshPro levelText;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.1f;
    public float scaleDuration = 0.7f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 0.5f;

    [Header("Animation Values")]
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 peakScale = new Vector3(1.2f, 1.2f, 1f);
    public Vector3 endScale = Vector3.one;

    private Coroutine currentAnimation;

    private void Start()
    {
        Debug.Log("Initialize leveltext");
        levelText = GetComponent<TextMeshPro>();
        
        //StartCoroutine(TestText());
    }

    IEnumerator TestText()
    {
        while (true)
        {
        ShowLevelNumber(2);
        yield return new WaitForSeconds(5);
        }

    }

    public void ShowLevelNumber(int levelNumber)
    {
        // Stop any existing animation
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Start new animation
        currentAnimation = StartCoroutine(LevelAnimation(levelNumber));
    }

    private IEnumerator LevelAnimation(int levelNumber)
    {
        levelText = GetComponent<TextMeshPro>();
;
        // Set the text
        levelText.SetText($"Level {levelNumber}");

        // Reset transform and alpha
        levelText.transform.localScale = startScale;
        Color textColor = levelText.color;
        textColor.a = 0f;
        levelText.color = textColor;

        // Ensure the text is enabled
        levelText.enabled = true;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            textColor.a = Mathf.Lerp(0f, 1f, t);
            levelText.color = textColor;
            yield return null;
        }

        // Scale animation
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;

            // Scale up then settle back
            if (t < 0.5f)
            {
                levelText.transform.localScale = Vector3.Lerp(startScale, peakScale, t * 2f);
            }
            else
            {
                levelText.transform.localScale = Vector3.Lerp(peakScale, endScale, (t - 0.5f) * 2f);
            }
            yield return null;
        }

        // Ensure final scale
        levelText.transform.localScale = endScale;

        // Display for a moment
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            textColor.a = Mathf.Lerp(1f, 0f, t);
            levelText.color = textColor;
            yield return null;
        }

        // Hide completely
        textColor.a = 0f;
        levelText.color = textColor;
        levelText.enabled = false;
    }

    // Optional: Quick test method
    [ContextMenu("Test Level Display")]
    public void TestLevelDisplay()
    {
        ShowLevelNumber(UnityEngine.Random.Range(1, 10));
    }
}
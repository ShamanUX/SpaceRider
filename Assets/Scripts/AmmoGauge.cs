using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AmmoGauge : MonoBehaviour
{
    [Header("Gauge Settings")]
    public int maxShots = 20;
    public float regenerationDelay = 0.5f; // Time before regeneration starts
    public float regenerationRate = 3f; // Shots per second

    [Header("UI References")]
    public Slider gaugeSlider;
    public Image fillImage;
    public Color fullColor = Color.green;
    public Color emptyColor = Color.red;

    private float currentShots;
    private float timeSinceLastShot;
    private bool isRegenerating;

    void Start()
    {
        RefillAmmoFully();
    }

    public void RefillAmmoFully()
    {
        currentShots = maxShots;
        UpdateGaugeUI();
    }

    void Update()
    {
        // Update time since last shot
        timeSinceLastShot += Time.deltaTime;

        // Check if we should start regenerating
        if (timeSinceLastShot >= regenerationDelay && currentShots < maxShots)
        {
            RegenerateAmmo();
        }
    }

    public bool UseShot()
    {
        if (currentShots >= 1)
        {
            currentShots--;
            timeSinceLastShot = 0f;
            isRegenerating = false;
            UpdateGaugeUI();
            return true; // Shot successful
        }
        return false; // Not enough ammo
    }

    private void RegenerateAmmo()
    {
        currentShots += regenerationRate * Time.deltaTime;
        currentShots = Mathf.Min(currentShots, maxShots);
        UpdateGaugeUI();
    }

    private void UpdateGaugeUI()
    {
        if (gaugeSlider != null)
        {
            gaugeSlider.value = currentShots / maxShots;
        }

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(emptyColor, fullColor, currentShots / maxShots);
        }
    }

    public bool CanShoot()
    {
        return currentShots >= 1;
    }

    public float GetCurrentShots()
    {
        return currentShots;
    }

    public float GetMaxShots()
    {
        return maxShots;
    }
}
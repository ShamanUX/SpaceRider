using UnityEngine;
using System.Collections;


[System.Serializable]
public class LevelConfig
{
    public enum Pattern
    {
        Random,
        SPattern
    }

    [Header("Difficulty")]
    public float scrollSpeed = 2f;
    public float spawnInterval = 1f;
    public float maxGapSize = 2f;

    [Header("Pattern")]
    public Pattern levelPattern = Pattern.SPattern;

    [Header("SPatternSettings")]
    public float sPatternStartTopHeight = 0;
    public float heightModulator = 1.5f;
}

public class SpawnObstacles : MonoBehaviour
{
    public Sprite rectangleSprite;

    public LevelConfig currentLevelConfig = new LevelConfig();

    private Camera mainCamera;
    private float screenHeight;
    private float screenWidth;


    void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
        //StartCoroutine(SpawnGatesRoutine());
        StartCoroutine(SpawnSPatternGate());
    }

    void Update()
    {
        // Move all obstacles left every frame
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            // Only apply to "Gate" obstacles
            if (obstacle.name != "Gate") continue;
            obstacle.transform.Translate(currentLevelConfig.scrollSpeed * Time.deltaTime * Vector3.left);

            // Destroy obstacles that go off-screen to the left
            if (obstacle.transform.position.x < -screenWidth * 1.5f)
            {
                Destroy(obstacle);
            }
        }
    }

    void CalculateScreenBounds()
    {
        screenHeight = mainCamera.orthographicSize * 2f;
        screenWidth = screenHeight * mainCamera.aspect;

        Debug.Log("screenHeight: " + screenHeight);
    }

    IEnumerator SpawnGatesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentLevelConfig.spawnInterval);
            SpawnRandomGate();
        }
    }

    IEnumerator SpawnSPatternGate()
    {
        float currentTopHeight= currentLevelConfig.sPatternStartTopHeight;

        while (true) {
            yield return new WaitForSeconds(currentLevelConfig.spawnInterval);
            CreateGate(currentLevelConfig.maxGapSize, currentTopHeight);
            float maxTopHeight = screenHeight - currentLevelConfig.maxGapSize;
            if (currentTopHeight + currentLevelConfig.heightModulator >= maxTopHeight || currentTopHeight + currentLevelConfig.heightModulator <= 0f)
            {
               if (currentLevelConfig.heightModulator < 0)
                // gap moving towards top
                { 
                    currentTopHeight = Mathf.Abs(currentLevelConfig.heightModulator) - currentTopHeight;
                } else
                // gap moving towards bottom
                {
                    float leftOverHeightModulation = currentTopHeight + currentLevelConfig.heightModulator - maxTopHeight;

                    currentTopHeight = screenHeight - leftOverHeightModulation - currentLevelConfig.maxGapSize;
                }
                currentLevelConfig.heightModulator = -currentLevelConfig.heightModulator;
            } else
            {
                currentTopHeight += currentLevelConfig.heightModulator;
            }
        }
    }

    void CreateGate(float gapSize, float topHeight)
    {
        // Create parent gate object
        GameObject gate = new GameObject("Gate");
        gate.tag = "Obstacle";

        float bottomHeight = screenHeight - topHeight - gapSize;

        // Extend obstacle slightly over the camera view 
        float extensionAmount = 0;

        // Spawn top rectangle
        GameObject topRect = CreateRectangle("TopGate", Color.red, "Obstacle");
        topRect.transform.parent = gate.transform;
        topRect.transform.localPosition = new Vector3(0, screenHeight / 2 - (topHeight / 2 - extensionAmount), 0);
        topRect.transform.localScale = new Vector3(0.5f, topHeight + extensionAmount, 1f);
        topRect.AddComponent<BoxCollider2D>();
        topRect.AddComponent<Obstacle>();
        topRect.layer = 6;

        // Spawn bottom rectangle
        GameObject bottomRect = CreateRectangle("BottomGate", Color.red, "Obstacle");
        bottomRect.transform.parent = gate.transform;
        bottomRect.transform.localPosition = new Vector3(0, -screenHeight / 2 + (bottomHeight / 2 - extensionAmount), 0);
        bottomRect.transform.localScale = new Vector3(0.5f, bottomHeight + extensionAmount, 1f);
        bottomRect.AddComponent<BoxCollider2D>();
        bottomRect.AddComponent<Obstacle>();
        bottomRect.layer = 6;

        // Spawn gap area
        GameObject gateGap = CreateRectangle("GateGap", Color.green, "Gap");
        gateGap.transform.parent = gate.transform;
        gateGap.transform.localPosition = new Vector3(0, screenHeight/2 - topHeight - gapSize / 2, 0);
        gateGap.transform.localScale = new Vector3(0.5f, gapSize + extensionAmount, 1f);
        BoxCollider2D gapCollider = gateGap.AddComponent<BoxCollider2D>();
        gapCollider.isTrigger = true; 
        
        // Position gate to the right of the screen
        gate.transform.position = new Vector3(
            mainCamera.transform.position.x + screenWidth / 2 + 1f,
            mainCamera.transform.position.y,
            0
        );

    }

    void SpawnRandomGate()
    {
        // Random gap size (max 1/2 screen height)
        float gapSize = Random.Range(screenHeight/10, screenHeight/4);
        float topHeight = Random.Range(0.1f, screenHeight - gapSize - 0.1f);
        CreateGate(gapSize, topHeight);
    }

    GameObject CreateRectangle(string name, Color color, string tag)
    {
        GameObject rect = new GameObject(name);
        rect.tag = tag;

        // Add sprite renderer
        SpriteRenderer sr = rect.AddComponent<SpriteRenderer>();
        sr.sprite = rectangleSprite;
        color.a = 0.5f;
        sr.color = color; // Make obstacles visible

        return rect;
    }

    public class Obstacle: MonoBehaviour
    {
       void OnCollisionEnter2D(Collision2D collision)
        {
            bool colliderIsPlayer = collision.gameObject.CompareTag("Player");
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Calculate average collision point
                Vector2 averageContactPoint = Vector2.zero;
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    averageContactPoint += contact.point;
                }
                averageContactPoint /= collision.contacts.Length;

                // Get center of the object we collided with
                Vector2 collidedObjectCenter = collision.collider.bounds.center;

                // Calculate direction FROM collision point TO center of collided object
                Vector2 forceDirection = (collidedObjectCenter - averageContactPoint).normalized;

                float forceMagnitude = 2f; // Adjust this value as needed
                // Add force in that direction
                if (colliderIsPlayer)
                {
                    forceMagnitude = 3f;
                }
                rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);
            }
        }
    }
}
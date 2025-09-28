using UnityEngine;
using System.Collections;

public class SpawnObstacles : MonoBehaviour
{
    public Sprite rectangleSprite;

    public float scrollSpeed = 2f;
    public float spawnInterval = 1f;
    public float maxGapSize = 0.5f; // Max 1/2 screen size gap

    private Camera mainCamera;
    private float screenHeight;
    private float screenWidth;

    public float sPatternGap = 2;
    public float sPatternStartTopHeight = 1;
    public float heightModulator = 1;

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
            obstacle.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

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
            yield return new WaitForSeconds(spawnInterval);
            SpawnRandomGate();
        }
    }

    IEnumerator SpawnSPatternGate()
    {

        float currentTopHeight= sPatternStartTopHeight;

        while (true) {
            yield return new WaitForSeconds(spawnInterval);
            CreateGate(sPatternGap, currentTopHeight);
            if (currentTopHeight + heightModulator >= screenHeight -0.5f || currentTopHeight + heightModulator <= 0.5f)
            {
                heightModulator = -heightModulator;
            }
            currentTopHeight += heightModulator;

        }

    }

    void CreateGate(float gapSize, float topHeight)
    {
        // Create parent gate object
        GameObject gate = new GameObject("Gate");
        gate.tag = "Obstacle";

        float bottomHeight = screenHeight - topHeight - gapSize;

        // Extend obstacle slightly over the camera view 
        float extensionAmount = 2f;

        // Spawn top rectangle
        GameObject topRect = CreateRectangle("TopGate", Color.red, "Obstacle");
        topRect.transform.parent = gate.transform;
        topRect.transform.localPosition = new Vector3(0, screenHeight / 2 - (topHeight / 2 - extensionAmount), 0);
        topRect.transform.localScale = new Vector3(0.5f, topHeight + extensionAmount, 1f);
        topRect.AddComponent<BoxCollider2D>();
        topRect.layer = 6;


        // Spawn bottom rectangle
        GameObject bottomRect = CreateRectangle("BottomGate", Color.red, "Obstacle");
        bottomRect.transform.parent = gate.transform;
        bottomRect.transform.localPosition = new Vector3(0, -screenHeight / 2 + (bottomHeight / 2 - extensionAmount), 0);
        bottomRect.transform.localScale = new Vector3(0.5f, bottomHeight + extensionAmount, 1f);
        bottomRect.AddComponent<BoxCollider2D>();
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
}
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Rendering.Universal;


[System.Serializable]
public class LevelConfig
{
    public enum Pattern
    {
        Random,
        SPattern, 
        Pause
    }

    [Header("Difficulty")]
    public float scrollSpeed = 2f;
    public float obstacleSpawnInterval = 1f;
    public float maxGapSize = 3f;
    public float minGapSize = 2f;

    [Header("Pattern")]
    public Pattern levelPattern = Pattern.SPattern;

    [Header("SPatternSettings")]
    public float sPatternStartTopHeight = 0;
    public float heightModulator = 1.5f;

    [Header("")]
    public float levelTimer;

}

public class SpawnObstacles : MonoBehaviour
{
    public Sprite rectangleSprite;
    public Sprite caveTile;

    public Color gateColor;

    [Header("Level controls")]
    public LevelConfig customConfig = new LevelConfig();
    [SerializeField] private LevelConfig currentLevelConfig = new LevelConfig();
    
    private float currentLevelTimer;

    LevelConfig easySPattern = new LevelConfig
    {
        levelTimer = 10,
        scrollSpeed = 3f,
        obstacleSpawnInterval = 1f,
        maxGapSize = 7,
        levelPattern = LevelConfig.Pattern.SPattern,
        sPatternStartTopHeight = 0,
        heightModulator = 3,
    };

    LevelConfig tightSPattern = new LevelConfig
    {
        levelTimer = 30,
        scrollSpeed = 2f,
        obstacleSpawnInterval = 0.5f,
        maxGapSize = 3,
        levelPattern = LevelConfig.Pattern.SPattern,
        sPatternStartTopHeight = 0,
        heightModulator = 1,
    };

    readonly LevelConfig startConfig = new LevelConfig
    {
        levelTimer = 10,
        scrollSpeed = 3,
        obstacleSpawnInterval = 1,
        maxGapSize = 10,
        minGapSize = 7,
        levelPattern = LevelConfig.Pattern.Random,

    };

    LevelConfig pauseConfig = new LevelConfig
    {
        levelTimer = 15,
        levelPattern = LevelConfig.Pattern.Pause
    };

    private Camera mainCamera;
    private float screenHeight;
    private float screenWidth;

    public GameObject enemyControllerObject;
    public GameObject levelInfoGameObject;
    public GameObject gateTipPrefab;
    private SpawnEnemies enemyController;
    private LevelConfig lastConfig;

    void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
        //StartCoroutine(SpawnGatesRoutine());
        currentLevelConfig = startConfig;
        //StartCoroutine(LevelRoutine(currentLevelConfig));
        //currentLevelConfig = easySPattern;
        enemyController = enemyControllerObject.GetComponent<SpawnEnemies>();
    }

    public void ResetConfig()
    {
        Debug.Log("Reset current level config");
        Debug.Log("Prev maxgap" + currentLevelConfig.maxGapSize);
        currentLevelConfig = new LevelConfig
        {
            levelTimer = startConfig.levelTimer,
            scrollSpeed = startConfig.scrollSpeed,
            obstacleSpawnInterval = startConfig.obstacleSpawnInterval,
            maxGapSize = startConfig.maxGapSize,
            minGapSize = startConfig.minGapSize,
            levelPattern = startConfig.levelPattern,
        };
        Debug.Log("Next maxgap" + currentLevelConfig.maxGapSize);
        Debug.Log("StartConfig maxgap" + startConfig.maxGapSize);
        StartCoroutine(LevelRoutine(currentLevelConfig));
    }

    void Update()
    {
        // Update time left
        if (GameObject.Find("GameStateController").GetComponent<GameStateController>().GetState() == GameStateController.GameState.Started)
        {
            currentLevelTimer -= Time.deltaTime;

        }
        GameObject timerTextParent = GameObject.Find("TimerText");
        
        if (timerTextParent)
        {
            UpdateTimerText timerText = timerTextParent.GetComponent<UpdateTimerText>();
            timerText.UpdateTime(currentLevelTimer);
        }
        

        if (currentLevelTimer < 0)
        {
            StopAllCoroutines();
            if (currentLevelConfig.levelPattern == LevelConfig.Pattern.Pause)
            {
                Debug.Log("End pause, start new");

                // Next level starting, increase difficulty
                currentLevelConfig = lastConfig;
                currentLevelConfig.maxGapSize = Math.Max(currentLevelConfig.maxGapSize - 0.3f, 6);
                currentLevelConfig.minGapSize = Math.Max(currentLevelConfig.minGapSize - 0.3f, 3);
                enemyController.SetMaxForceMultiplier(Math.Min(enemyController.GetMaxForceMultiplier() + 0.3f, 2.6f));
                enemyController.SetMaxSpeedMultiplier(Math.Min(enemyController.GetMaxSpeedMultiplier() + 0.3f, 2.6f));
            } else
            {
                float prevScrollSpeed = currentLevelConfig.scrollSpeed;
                lastConfig = currentLevelConfig;
                currentLevelConfig = pauseConfig;
                pauseConfig.scrollSpeed = prevScrollSpeed;
            }
            StartCoroutine(LevelRoutine(currentLevelConfig));
        }
    }

    private void FixedUpdate()
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

    IEnumerator LevelRoutine(LevelConfig config)
    {
        float currentTopHeight = currentLevelConfig.sPatternStartTopHeight;
        currentLevelTimer = config.levelTimer;
        bool firstLoop = true;
        while (true)
        {
            if (firstLoop)
            {
                firstLoop = false;
                Debug.Log("First loop of level routine");
                if (config.levelPattern != LevelConfig.Pattern.Pause)
                {
                    int levelNumber = FindFirstObjectByType<GameStateController>().GetLevelNumber();
                    Debug.Log("LevelNumber: " + levelNumber);
                    levelInfoGameObject.GetComponent<ShowLevelInfo>().ShowLevelNumber(FindFirstObjectByType<GameStateController>().levelNumber);
                } else
                {
                    Debug.Log("Increase level number");
                    FindAnyObjectByType<GameStateController>().levelNumber += 1;

                }
            } else
            {
                if (config.levelPattern == LevelConfig.Pattern.Random)
                {

                    SpawnRandomGate();
                }
                else if (config.levelPattern == LevelConfig.Pattern.SPattern)
                {
                    CreateGate(currentLevelConfig.maxGapSize, currentTopHeight);
                    currentTopHeight = TopHeightAdjustment(currentTopHeight);
                }
                else if (config.levelPattern == LevelConfig.Pattern.Pause)
                {

                }

            }

            yield return new WaitForSeconds(currentLevelConfig.obstacleSpawnInterval);
            
        }
    }

    float TopHeightAdjustment(float currentTopHeight) 
    {
        float maxTopHeight = screenHeight - currentLevelConfig.maxGapSize;
        if (currentTopHeight + currentLevelConfig.heightModulator >= maxTopHeight || currentTopHeight + currentLevelConfig.heightModulator <= 0f)
        {
            if (currentLevelConfig.heightModulator < 0)
            // gap moving towards top
            {
                currentTopHeight = Mathf.Abs(currentLevelConfig.heightModulator) - currentTopHeight;
            }
            else
            // gap moving towards bottom
            {
                float leftOverHeightModulation = currentTopHeight + currentLevelConfig.heightModulator - maxTopHeight;

                currentTopHeight = screenHeight - leftOverHeightModulation - currentLevelConfig.maxGapSize;
                
            }
            currentLevelConfig.heightModulator = -currentLevelConfig.heightModulator;
        }
        else
        {
            currentTopHeight += currentLevelConfig.heightModulator;
        }

        return currentTopHeight;
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
        GameObject topRect = CreateRectangle("TopGate", gateColor, "Obstacle");
        topRect.transform.parent = gate.transform;
        topRect.transform.localPosition = new Vector3(0, screenHeight / 2 - (topHeight / 2 - extensionAmount), 0);
        topRect.transform.localScale = new Vector3(0.5f, topHeight + extensionAmount, 1f);
        topRect.AddComponent<BoxCollider2D>();
        topRect.AddComponent<Obstacle>();
        topRect.layer = 6;
        //FillRectWithCaveBlocks(topRect, "top");

        // Spawn top rectangle tip
        GameObject topGateTip = Instantiate(gateTipPrefab);
        topGateTip.transform.parent = gate.transform;
        topGateTip.transform.localPosition = new Vector3(0,screenHeight / 2 - topHeight - gateTipPrefab.transform.lossyScale.y / 2, 0);
        topGateTip.transform.localScale = new Vector3(0.5f, topGateTip.transform.localScale.y, 1);
        topGateTip.transform.Rotate(Vector3.forward, 180);
        topGateTip.GetComponent<SpriteRenderer>().color = gateColor;
       
        // Spawn bottom rectangle
        GameObject bottomRect = CreateRectangle("BottomGate", gateColor, "Obstacle");
        bottomRect.transform.parent = gate.transform;
        bottomRect.transform.localPosition = new Vector3(0, -screenHeight / 2 + (bottomHeight / 2 - extensionAmount), 0);
        bottomRect.transform.localScale = new Vector3(0.5f, bottomHeight + extensionAmount, 1f);
        bottomRect.AddComponent<BoxCollider2D>();
        bottomRect.AddComponent<Obstacle>();
        bottomRect.layer = 6;
        //FillRectWithCaveBlocks(bottomRect, "bottom");
        // Spawn bottom rectangle tip
        GameObject bottomGateTip = Instantiate(gateTipPrefab);
        bottomGateTip.transform.parent = gate.transform;
        bottomGateTip.transform.localPosition = new Vector3(0, -screenHeight / 2 + bottomHeight + gateTipPrefab.transform.lossyScale.y / 2, 0);
        bottomGateTip.transform.localScale = new Vector3(0.5f, bottomGateTip.transform.localScale.y, 1);
        bottomGateTip.GetComponent<SpriteRenderer>().color = gateColor;


        // Spawn gap area
        GameObject gateGap = CreateRectangle("GateGap", new Color(0,0,0,0), "Gap");
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
        float gapSize = UnityEngine.Random.Range(currentLevelConfig.minGapSize, currentLevelConfig.maxGapSize);
        float topHeight = UnityEngine.Random.Range(0.1f, screenHeight - gapSize - 0.1f);
        CreateGate(gapSize, topHeight);
        
    }

    GameObject CreateRectangle(string name, Color color, string tag)
    {
        GameObject rect = new GameObject(name);
        rect.tag = tag;

        // Add sprite renderer
        SpriteRenderer sr = rect.AddComponent<SpriteRenderer>();
        sr.sprite = rectangleSprite;

        sr.color = color; 

        return rect;
    }

    void FillRectWithCaveBlocks(GameObject rect, string direction)
    {
        Vector3 startPosition;
        float rectWorldHeight = rect.transform.lossyScale.y;
        float blockHeight = rect.transform.lossyScale.x;
        int blocksNeeded = Mathf.RoundToInt(rectWorldHeight / blockHeight) + 1;
        
        if (direction == "top")
        {
         startPosition = new Vector3(rect.transform.position.x, rect.transform.position.y - rectWorldHeight / 2 + blockHeight / 2, 0);

        } else
        {
            startPosition = new Vector3(rect.transform.position.x, rect.transform.position.y + rectWorldHeight / 2 - blockHeight / 2, 0);
        }

        for (int i = 0; blocksNeeded > i; i++)
        {   
            GameObject caveBlock = new GameObject("CaveBlock");
            SpriteRenderer sr = caveBlock.AddComponent<SpriteRenderer>();
            sr.sprite = caveTile;
            caveBlock.transform.localScale = new Vector3(rect.transform.lossyScale.x, rect.transform.lossyScale.x, 1);
            if (direction == "top")
            {
                caveBlock.transform.position = new Vector3(startPosition.x, startPosition.y + i * blockHeight, startPosition.z);
            } else
            {
                caveBlock.transform.position = new Vector3(startPosition.x, startPosition.y - i * blockHeight, startPosition.z);
            }
            caveBlock.transform.SetParent(rect.transform);
        }
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
                    FindFirstObjectByType<AudioManager>().PlayAudio("ObstacleHit");
                }
                rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);
            }
        }
    }
}
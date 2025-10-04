using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnEnemies : MonoBehaviour
{
    public Sprite enemySprite;

    public int spawnInterval = 5;

    public float minDistanceToGate = 5;
    private enum EnemyType
    {
        Basic,
        Fast
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartEnemySpawnRoutine()
    {
        StartCoroutine(SpawnEnemy());
    }

    Vector2 DetermineSpawnLocation()
    {

        GameObject[] gateGaps = GameObject.FindGameObjectsWithTag("Gap");
        Vector2 bestPosition = transform.position; // Default to current position
        float distanceToNearestGate = -Mathf.Infinity;

        if (!GameObject.FindGameObjectWithTag("Player"))
        {
            return bestPosition;
        }

        foreach (GameObject gap in gateGaps)
        {
            // Skip gates that are behind the spawner
            float xDifference = gap.transform.position.x - transform.position.x;
            if (xDifference < 0f) continue;

            if (distanceToNearestGate == -Mathf.Infinity)
            {
                distanceToNearestGate = xDifference;
                bestPosition = gap.transform.position;
            }

            if (xDifference < distanceToNearestGate)
            {
                distanceToNearestGate = xDifference;
                bestPosition = gap.transform.position;
                Debug.Log("Gap found, y: " + gap.transform.position.y);
            }
        }

        if (distanceToNearestGate > 0 && distanceToNearestGate < minDistanceToGate)
        {
            //Debug.Log("BestPosition y :" + bestPosition.y);
            return new Vector2(transform.position.x, bestPosition.y);
        } else
        {
            return transform.position;
        }  
    }

    IEnumerator SpawnEnemy()
    {
        while (true)
        {
            // Randomly spawn either fast or slow enemies   w
            int randomNumber = Random.Range(1,11);
            EnemyConfig config;
            if ( randomNumber <=4)
            {
                // Fast enemy
                config = new EnemyConfig
                {
                    color = Color.magenta,
                    size = 0.7f,
                    moveSpeed = 3f,
                    playerChaseSpeed = 5f,
                    sightDistance = 30f,
                    health = 4
                };
            } else if (randomNumber <= 9)
            {
                // Basic enemy
                config = new EnemyConfig
                {
                    color = Color.yellow,
                    size = 1f,
                    moveSpeed = 3f,
                    playerChaseSpeed = 4f,
                    sightDistance = 30f,
                    health = 8
                };
            } else
            {
                // Super fast enemy
                config = new EnemyConfig
                {
                    size = 0.5f,
                    moveSpeed = 5f,
                    playerChaseSpeed = 7f,
                    sightDistance = 30f,
                    health = 2,
                    color = Color.red,
                };
            }

            GameObject enemyObj = new GameObject("Enemy");
            enemyObj.tag = "Enemy";
            enemyObj.layer = LayerMask.NameToLayer("Enemy");

            // Add required components
            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = enemySprite; // Use your rectangle sprite or create enemy sprite
            sr.color = config.color;

            CircleCollider2D collider = enemyObj.AddComponent<CircleCollider2D>();
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            // Add enemy component and initialize
            Enemy enemy = enemyObj.AddComponent<Enemy>();
            enemy.Initialize(config);

            enemyObj.transform.position = DetermineSpawnLocation();
            enemyObj.transform.localScale = Vector3.one * config.size;

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}

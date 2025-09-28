using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnEnemies : MonoBehaviour
{
    public Sprite enemySprite;

    public int spawnInterval = 5;
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

    IEnumerator SpawnEnemy()
    {
        while (true)
        {
            // Randomly spawn either fast or slow enemies   w
            int randomNumber = Random.Range(1,10);
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
                    sightDistance = 10f,
                    health = 50
                };
            } else
            {
                // Basic enemy
                config = new EnemyConfig
                {
                    color = Color.blue,
                    size = 1f,
                    moveSpeed = 2f,
                    playerChaseSpeed = 3f,
                    sightDistance = 10f,
                    health = 100
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

            enemyObj.transform.position = transform.position;
            enemyObj.transform.localScale = Vector3.one * config.size;

            yield return new WaitForSeconds(5);
        }
    }
}

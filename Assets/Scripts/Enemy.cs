using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

// Configuration class to hold enemy properties
[System.Serializable]
public class EnemyConfig
{
    [Header("Appearance")]
    public Color color = Color.red;
    public float size = 1f;
    public int orderInSortingLayer = 9; 

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float playerChaseSpeed = 5f;
    public float drag = 0.5f;
    public float mass = 1f;
    public float gravityScale = 0;

    [Header("Movement Physics")]
    public float moveRate = 0.2f;
    public float moveForcePerTick = 10f;
    public float maxForce = 30f;

    [Header("AI Behavior")]
    public float sightDistance = 30f;
    public float targetUpdateRate = 0.3f;

    [Header("Combat")]
    public int health = 100;
    public int damage = 10;
    public float attackRange = 2f;
    public float attackRate = 1f;
}

public class Enemy : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public EnemyConfig config;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 currentTarget;
    private bool targetIsClosestGap = false;
    private bool canSeePlayer = false;

    public SpawnEnemies enemyController;
    

    // Constructor-like initialization method
    public void Initialize(EnemyConfig enemyConfig)
    {
        config = enemyConfig;
        ApplyConfig();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(UpdateTarget), 0f, config.targetUpdateRate);
        InvokeRepeating(nameof(MoveTowardsTarget), 0f, config.moveRate);
        enemyController = FindFirstObjectByType<SpawnEnemies>();
        
    
    }


    void ApplyConfig()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        // Apply visual properties
        if (spriteRenderer != null)
        {
            spriteRenderer.color = config.color;
            transform.localScale = Vector3.one * config.size;
            spriteRenderer.sortingLayerName ="Enemy";
            spriteRenderer.sortingOrder = config.orderInSortingLayer;
        }

        // Apply physics properties
        if (rb != null)
        {
            rb.linearDamping = config.drag;
            rb.mass = config.mass;
            rb.gravityScale = config.gravityScale;
        }
    }

    void Update()
    {
        CheckPlayerLineOfSight();
        //BreakIfMovingToWrongDirection();
    }

    void CheckPlayerLineOfSight()
    {
        canSeePlayer = false;

        if (player == null) return;

        Vector2 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if player is within sight distance and angle
        if (distanceToPlayer <= config.sightDistance)
        {
            // Raycast to check if there's clear line of sight
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, LayerMask.GetMask("Obstacles"));

            if (hit.collider == null || hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                currentTarget = player.position;
            }
        }
    }

    void BreakIfMovingToWrongDirection()
    {
        if (rb.linearVelocity.magnitude < 0.1f) return;

        Vector2 dirToTarget = (currentTarget - (Vector2)transform.position).normalized;
        Vector2 velocityDir = rb.linearVelocity.normalized;

        float angle = Vector2.Angle(velocityDir, dirToTarget);
        Debug.Log("Angle to target: " + angle);

        // Apply progressive drag based on how wrong the direction is
        if (angle > 30f || angle < -30f)
        {
            // More drag the worse the angle
            float dragFactor = Mathf.Clamp(Mathf.Abs(angle / 90f), config.drag, 3f);
            rb.linearDamping = dragFactor;
            Debug.Log("Applying drag: " + dragFactor);
        }
        else
        {
            rb.linearDamping = config.drag; // Base drag
        }
    }

    void UpdateTarget()
    {
        if (canSeePlayer) return; // Already targeting player

        // Find the safest gate to move towards
        currentTarget = FindSafestGatePosition();
    }

    Vector2? FindNextGapPosition()
    {
        GameObject[] gateGaps = GameObject.FindGameObjectsWithTag("Gap");
        float distanceToNearestGate = -Mathf.Infinity;
        Vector2 bestPosition = transform.position;
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
        if (bestPosition == (Vector2)transform.position)
        {
            return null;
        } else
        {
            //Debug.Log("Found closest next gap, pos: " + bestPosition);
            return bestPosition;
        }
    }  

    Vector2 FindSafestGatePosition()
    {
        GameObject[] gateGaps = GameObject.FindGameObjectsWithTag("Gap");
        Vector2 bestPosition = transform.position; // Default to current position
        float bestDistance = -Mathf.Infinity;
        float bestHorizontalDistance = -Mathf.Infinity;
        targetIsClosestGap = false;

        if (!GameObject.FindGameObjectWithTag("Player"))
        {
            return bestPosition;
        }

        bool playerIsInFront = GameObject.FindGameObjectWithTag("Player").transform.position.x > transform.position.x ;

        foreach (GameObject gap in gateGaps)
        {
            // Skip gates that are away from the player
            float xDifference = gap.transform.position.x - transform.position.x;
            if (playerIsInFront && xDifference < 0f) continue;
            if (!playerIsInFront && xDifference > 0f) continue;

            Vector2 gapCenter = gap.transform.position;

            // Check line of sight to this gate
            float distanceToGate = Vector2.Distance(transform.position, gapCenter);
            Vector2 directionToGate = (gapCenter - (Vector2)transform.position).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToGate, distanceToGate, LayerMask.GetMask("Obstacles"));

            // If we have clear line of sight (only hit the target gate or nothing)
            bool hasLineOfSight = hit.collider == null ||
                                    hit.collider.gameObject == gap;

            if (hasLineOfSight && distanceToGate > bestDistance)
            {
                bestDistance = distanceToGate;
                bestPosition = gapCenter;
            }
        }

        // If no gates with line of sight found, find closest gap
        if (bestDistance == -Mathf.Infinity)
        {
            foreach (GameObject gap in gateGaps)
            {
                float xDifference = gap.transform.position.x - transform.position.x;
                if (xDifference < 0f) continue;

                float distanceToGate = Vector2.Distance(transform.position, gap.transform.position);
                if (bestDistance == -Mathf.Infinity)
                {
                    bestDistance = distanceToGate;
                    bestHorizontalDistance = xDifference;
                }

                if (xDifference <= bestHorizontalDistance && distanceToGate <= bestDistance)
                {
                    //Debug.Log("Fallback to closest gap");
                    bestPosition = gap.transform.position;
                    bestHorizontalDistance = xDifference;
                    targetIsClosestGap = true;
                }
            }
        }

        if (bestPosition.Equals(new Vector2(transform.position.x, transform.position.y)))
        {
             // Debug.Log("No best position found!");
        }
        return bestPosition;
    }

    void MoveTowardsTarget()
    { 
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;
        float maxSpeed = canSeePlayer ? config.playerChaseSpeed : config.moveSpeed;
         
        maxSpeed *= enemyController.GetMaxSpeedMultiplier();
        float maxForce = config.maxForce * enemyController.GetMaxForceMultiplier();

        // Use AddForce for gradual acceleration
        Vector2 desiredVelocity = direction * maxSpeed;
        Vector2 force = (desiredVelocity - rb.linearVelocity) * config.moveForcePerTick * enemyController.GetMaxForceMultiplier();

        // Limit maximum force to prevent overshooting
        force = Vector2.ClampMagnitude(force, maxForce);

        rb.AddForce(force);

        // Optional: Limit maximum velocity
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Move towards closest gap vertically to ease navigation, if no player in sight
        Vector2? closestGap = FindNextGapPosition();
        if (closestGap.HasValue && !canSeePlayer) {
            Vector2 verticalDirection = (new Vector2(transform.position.x, closestGap.Value.y) - (Vector2)transform.position);
            Vector2 desiredVerticalVelocity = verticalDirection * maxSpeed;
            Vector2 verticalForce = (desiredVerticalVelocity - new Vector2(0, rb.linearVelocityY)) * config.moveForcePerTick;
            verticalForce = Vector2.ClampMagnitude(verticalForce, config.maxForce);

            rb.AddForce(verticalForce);
        }     
    }

    void OnDrawGizmos()
    {
        // Draw line to current target
        if (Application.isPlaying)
        {
            Gizmos.color = canSeePlayer ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, currentTarget);
            Gizmos.DrawWireSphere(currentTarget, 0.3f);

            // Draw vision cone
            Gizmos.color = Color.yellow;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeKnockback(collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.normalized * 2);
            TakeDamage(1);
            Destroy(collision.gameObject);
            
        }
    }

    public void TakeKnockback(Vector2 force, float resistanceMultiplier = 1f)
    {
        // Add knockback variables
        float knockbackResistance = 1f; // Lower = more knockback

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Apply knockback force (adjust by resistance)
            rb.AddForce((1f / knockbackResistance) * resistanceMultiplier * force, ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(int amount)
    {
        config.health -= amount;
        if (config.health <= 0)
        {
            ParticleSystem damageParticles = GameObject.Find("EnemyDamage").GetComponent<ParticleSystem>();
            damageParticles.transform.position = transform.position;
            var main = damageParticles.main;

            main.startColor = gameObject.GetComponent<SpriteRenderer>().color;
            damageParticles.Play();

            Destroy(gameObject);
            FindFirstObjectByType<AudioManager>().PlayAudio("EnemyDeath");
        }
    }

}
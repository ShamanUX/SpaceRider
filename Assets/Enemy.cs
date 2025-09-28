using UnityEngine;
using System.Collections.Generic;

// Configuration class to hold enemy properties
[System.Serializable]
public class EnemyConfig
{
    [Header("Appearance")]
    public Color color = Color.red;
    public float size = 1f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float playerChaseSpeed = 5f;
    public float drag = 0.5f;
    public float mass = 1f;
    public float gravityScale = 0;

    [Header("Movement Physics")]
    public float moveRate = 0.3f;
    public float moveForcePerTick = 10f;
    public float maxForce = 20f;

    [Header("AI Behavior")]
    public float sightDistance = 8f;
    public float playerDetectionAngle = 90f;
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
    private bool canSeePlayer = false;

    private float ballWidth;

    // Constructor-like initialization method
    public void Initialize(EnemyConfig enemyConfig)
    {
        config = enemyConfig;
        ApplyConfig();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(UpdateTarget), 0f, config.targetUpdateRate);
        InvokeRepeating(nameof(MoveTowardsTarget), 0f, config.moveRate);
        ballWidth = GetComponent<Collider2D>().bounds.size.x;
        Debug.Log("ballwidth" + ballWidth);
    }


    void ApplyConfig()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        // Apply visual properties
        if (spriteRenderer != null)
        {
            spriteRenderer.color = config.color;
            transform.localScale = Vector3.one * config.size;
        }

        // Apply physics properties
        if (rb != null)
        {
            rb.drag = config.drag;
            rb.mass = config.mass;
            rb.gravityScale = config.gravityScale;
        }
    }

    void Update()
    {
        CheckPlayerLineOfSight();
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

    void UpdateTarget()
    {
        if (canSeePlayer) return; // Already targeting player

        // Find the safest gate to move towards
        currentTarget = FindSafestGatePosition();
    }

    Vector2 FindSafestGatePosition()
    {
        GameObject[] gateGaps = GameObject.FindGameObjectsWithTag("Gap");
        Vector2 bestPosition = transform.position; // Default to current position
        float bestDistance = -Mathf.Infinity;

        foreach (GameObject gap in gateGaps)
        {

            // Skip gates that are behind us
            float xDifference = gap.transform.position.x - transform.position.x;
            if (xDifference < 0) continue;

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
                if (xDifference < 0) continue;

                if (gap.transform.position.x < bestPosition.x)
                {
                    Debug.Log("Fallback to closest gap");
                    bestPosition = gap.transform.position;
                }
            }
        }

        if (bestDistance == -Mathf.Infinity)
        {
            // Debug.Log("No best position found!");
        }
        return bestPosition;
    }

    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;
        float currentSpeed = canSeePlayer ? config.playerChaseSpeed : config.moveSpeed;

        // Use AddForce for gradual acceleration
        Vector2 desiredVelocity = direction * currentSpeed;
        Vector2 force = (desiredVelocity - rb.velocity) * config.moveForcePerTick;

        // Limit maximum force to prevent overshooting
        force = Vector2.ClampMagnitude(force, config.maxForce);

        rb.AddForce(force);

        // Optional: Limit maximum velocity
        if (rb.velocity.magnitude > currentSpeed)
        {
            rb.velocity = rb.velocity.normalized * currentSpeed;
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
            TakeKnockback(collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized * 2);
            TakeDamage(5);
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
            Destroy(gameObject);
        }
    }

}
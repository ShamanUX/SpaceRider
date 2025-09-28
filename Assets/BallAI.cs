using UnityEngine;
using System.Collections.Generic;

public class BallAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 1f;
    public float playerChaseSpeed = 2f;
    public float sightDistance = 8f;

    [Header("Targeting")]
    public LayerMask obstacleLayer;
    public float targetUpdateRate = 0.3f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 currentTarget;
    private bool canSeePlayer = false;

    private float ballWidth;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(UpdateTarget), 0f, targetUpdateRate);
        ballWidth = GetComponent<Collider2D>().bounds.size.x;
        Debug.Log("ballwidth"+ ballWidth);
    }

    void Update()
    {
        CheckPlayerLineOfSight();
        MoveTowardsTarget();
    }

    void CheckPlayerLineOfSight()
    {
        canSeePlayer = false;

        if (player == null) return;

        Vector2 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if player is within sight distance and angle
        if (distanceToPlayer <= sightDistance)
        {
            // Raycast to check if there's clear line of sight
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);

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
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToGate, distanceToGate, obstacleLayer);

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
            Debug.Log("No best position found!");
        }
    return bestPosition;
    }

    float CalculateGateScore(GameObject gate, Vector2 gapCenter)
    {
        float score = 0f;

        // Prefer gates that are closer
        float distance = Vector2.Distance(transform.position, gapCenter);
        score += 10f / (distance + 1f);

        // Prefer gates with larger gaps
        float gapSize = CalculateGapSize(gate);
        score += gapSize * 5f;

        // Prefer gates that are more aligned with our current position
        float verticalAlignment = 1f - Mathf.Abs(transform.position.y - gapCenter.y) / 10f;
        score += verticalAlignment * 3f;

        // Avoid gates that are too close to screen edges
        float edgeSafety = CalculateEdgeSafety(gapCenter);
        score += edgeSafety * 2f;

        return score;
    }

    float CalculateGapSize(GameObject gate)
    {
        Transform topGate = null;
        Transform bottomGate = null;

        foreach (Transform child in gate.transform)
        {
            if (child.name.Contains("Top")) topGate = child;
            if (child.name.Contains("Bottom")) bottomGate = child;
        }

        if (topGate != null && bottomGate != null)
        {
            return Mathf.Abs(topGate.position.y - bottomGate.position.y);
        }

        return 0f;
    }

    float CalculateEdgeSafety(Vector2 position)
    {
        Camera cam = Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(position);

        // Score higher for positions away from screen edges
        float horizontalSafety = 1f - Mathf.Abs(viewportPos.x - 0.5f) * 2f;
        float verticalSafety = 1f - Mathf.Abs(viewportPos.y - 0.5f) * 2f;

        return (horizontalSafety + verticalSafety) / 2f;
    }

    void MoveTowardsTarget()
    {
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;
        float currentSpeed = canSeePlayer ? playerChaseSpeed : moveSpeed;

        // Simple movement towards target
        rb.velocity = direction * currentSpeed;
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
}
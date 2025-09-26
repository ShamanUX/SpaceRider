using UnityEngine;
using System.Collections.Generic;

public class BallAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float playerChaseSpeed = 5f;
    public float sightDistance = 8f;
    public float playerDetectionAngle = 90f;

    [Header("Targeting")]
    public LayerMask obstacleLayer;
    public float targetUpdateRate = 0.3f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 currentTarget;
    private bool canSeePlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("UpdateTarget", 0f, targetUpdateRate);
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
            float angleToPlayer = Vector2.Angle(transform.up, directionToPlayer);

            if (angleToPlayer <= playerDetectionAngle / 2f)
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
    }

    void UpdateTarget()
    {
        if (canSeePlayer) return; // Already targeting player

        // Find the safest gate to move towards
        currentTarget = FindSafestGatePosition();
    }

    Vector2 FindSafestGatePosition()
    {
        GameObject[] gates = GameObject.FindGameObjectsWithTag("Obstacle");
        Vector2 bestPosition = transform.position; // Default to current position
        float bestScore = -Mathf.Infinity;

        foreach (GameObject gate in gates)
        {
            if (gate.name != "Gate") continue;

            // Skip gates that are behind us or too far
            float xDifference = gate.transform.position.x - transform.position.x;
            if (xDifference < 0) continue; // Don't target gates behind us

            // Calculate gap center
            Vector2 gapCenter = GetGateGapCenter(gate);
            if (gapCenter == Vector2.zero) continue;

            // Score this gate based on safety and proximity
            float score = CalculateGateScore(gate, gapCenter);

            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = gapCenter;
            }
        }

        // If no good gates found, move towards center of screen
        if (bestScore == -Mathf.Infinity)
        {
            bestPosition = new Vector2(transform.position.x + 5f, 0f);
        }

        return bestPosition;
    }

    Vector2 GetGateGapCenter(GameObject gate)
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
            return (topGate.position + bottomGate.position) / 2f;
        }

        return Vector2.zero;
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

        // Optional: Rotate to face movement direction
        if (rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void OnDrawGizmos()
    {
        // Draw line to current target
        if (Application.isPlaying)
        {
            Gizmos.color = canSeePlayer ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, currentTarget);
            Gizmos.DrawWireSphere(currentTarget, 0.3f);

            // Draw vision cone
            Gizmos.color = Color.yellow;
            DrawVisionCone();
        }
    }

    void DrawVisionCone()
    {
        float halfAngle = playerDetectionAngle / 2f;
        Vector2 forward = transform.up;

        Vector2 leftDir = Quaternion.Euler(0, 0, halfAngle) * forward;
        Vector2 rightDir = Quaternion.Euler(0, 0, -halfAngle) * forward;

        Gizmos.DrawRay(transform.position, leftDir * sightDistance);
        Gizmos.DrawRay(transform.position, rightDir * sightDistance);
    }
}
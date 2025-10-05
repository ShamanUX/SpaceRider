using UnityEngine;

public class ProximityOpacity : MonoBehaviour
{
    [Header("Proximity Settings")]
    public Transform player;
    public float maxDistance = 3.5f;
    public float minDistance = 0.5f;

    [Header("Opacity Settings")]
    [Range(0f, 1f)] public float maxOpacity = 0.9f;
    [Range(0f, 1f)] public float minOpacity = 0.01f;

    [Header("Smoothing")]
    public float fadeSpeed = 10f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D wallCollider;
    private float currentOpacity;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on " + gameObject.name);
            enabled = false;
            return;
        }

        if (wallCollider == null)
        {
            Debug.LogError("No BoxCollider2D found on " + gameObject.name);
            enabled = false;
            return;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player == null)
            {
                Debug.LogError("No player assigned and no GameObject with 'Player' tag found!");
                enabled = false;
                return;
            }
        }

        currentOpacity = spriteRenderer.color.a;
    }

    void Update()
    {
        if (player == null) return;

        float distance = GetDistanceToNearestPointOnWall(player.position);
        float targetOpacity = CalculateTargetOpacity(distance);

        // Smoothly interpolate to target opacity
        currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, fadeSpeed * Time.deltaTime);

        // Apply the new opacity
        Color newColor = spriteRenderer.color;
        newColor.a = currentOpacity;
        spriteRenderer.color = newColor;
    }

    float GetDistanceToNearestPointOnWall(Vector2 playerPosition)
    {
        // Get the collider bounds in world space
        Bounds bounds = wallCollider.bounds;

        // Find the closest point on the wall's bounds to the player
        Vector2 closestPoint = bounds.ClosestPoint(playerPosition);

        // Return distance to that closest point
        return Vector2.Distance(playerPosition, closestPoint);
    }

    float CalculateTargetOpacity(float distance)
    {
        if (FindFirstObjectByType<GameStateController>().GetState() != GameStateController.GameState.Started){
            return minOpacity;
        }
        // Clamp distance between min and max range
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate normalized value (0 to 1) where 0 = minDistance, 1 = maxDistance
        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

        // Inverse lerp - closer = more opacity, farther = less opacity
        return Mathf.Lerp(maxOpacity, minOpacity, normalizedDistance);
    }

    // Optional: Visualize the proximity range and closest points in Scene view
    void OnDrawGizmosSelected()
    {
        if (player == null || wallCollider == null) return;

        // Draw wall bounds
        Gizmos.color = Color.green;
        Bounds bounds = wallCollider.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Draw proximity ranges around the entire wall
        Gizmos.color = Color.cyan;
        DrawWireRect(bounds.center, bounds.size + Vector3.one * minDistance * 2);
        Gizmos.color = Color.blue;
        DrawWireRect(bounds.center, bounds.size + Vector3.one * maxDistance * 2);

        // Draw line to closest point
        if (Application.isPlaying)
        {
            Vector2 closestPoint = wallCollider.bounds.ClosestPoint(player.position);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(player.position, closestPoint);
            Gizmos.DrawWireSphere(closestPoint, 0.2f);
        }
    }

    void DrawWireRect(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size / 2f;
        Vector3 topLeft = center + new Vector3(-halfSize.x, halfSize.y, 0);
        Vector3 topRight = center + new Vector3(halfSize.x, halfSize.y, 0);
        Vector3 bottomLeft = center + new Vector3(-halfSize.x, -halfSize.y, 0);
        Vector3 bottomRight = center + new Vector3(halfSize.x, -halfSize.y, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
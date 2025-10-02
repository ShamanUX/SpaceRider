using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject[] boundaryWalls = new GameObject[4];

    void Start()
    {
        mainCamera = Camera.main;
        CreateFourWalls();
    }

    void CreateFourWalls()
    {
        if (mainCamera == null) return;

        Vector2 screenSize = GetScreenWorldSize();
        float wallThickness = 1f; // Adjust thickness as needed

        // Destroy existing walls if they exist
        foreach (GameObject wall in boundaryWalls)
        {
            if (wall != null) Destroy(wall);
        }

        // Create four walls
        boundaryWalls[0] = CreateWall("TopWall", new Vector2(0, screenSize.y / 2 + wallThickness / 2),
            new Vector2(screenSize.x + wallThickness * 2, wallThickness));

        boundaryWalls[1] = CreateWall("BottomWall", new Vector2(0, -screenSize.y / 2 - wallThickness / 2),
            new Vector2(screenSize.x + wallThickness * 2, wallThickness));

        boundaryWalls[2] = CreateWall("RightWall", new Vector2(screenSize.x / 2 + wallThickness / 2, 0),
            new Vector2(wallThickness, screenSize.y + wallThickness * 2));

        boundaryWalls[3] = CreateWall("LeftWall", new Vector2(-screenSize.x / 2 - wallThickness / 2, 0),
            new Vector2(wallThickness, screenSize.y + wallThickness * 2));
    }

    GameObject CreateWall(string wallName, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.parent = transform;
        wall.transform.position = position;

        BoxCollider2D wallCollider = wall.AddComponent<BoxCollider2D>();
        wallCollider.size = size;
        // DISABLED TRIGGER OF WALL
        // wallCollider.isTrigger = true;

        return wall;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Wrap object");
            WrapObject(other.transform);
        }
        else if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
        }
    }

    void WrapObject(Transform objectToWrap)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(objectToWrap.position);
        Vector3 newPosition = objectToWrap.position;

        // Determine which side the object exited and wrap accordingly
        if (viewportPos.x > 1.0f || viewportPos.x < 0f)
        {
            newPosition.x = -newPosition.x;
        }
        if (viewportPos.y > 1.0f || viewportPos.y < 0f)
        {
            newPosition.y = -newPosition.y;
        }

        objectToWrap.position = newPosition;
    }

    Vector2 GetScreenWorldSize()
    {
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;
        return new Vector2(width, height);
    }

    void Update()
    {
        // Re-enforce aspect ratio if screen size changes (optional)
        if (Screen.width != Screen.currentResolution.width ||
            Screen.height != Screen.currentResolution.height)
        {
            CreateFourWalls();
        }
    }
}
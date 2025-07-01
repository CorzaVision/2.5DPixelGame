using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 20;
    [SerializeField] private float cellSize = 3.0f;

    public GameObject combatRoomPrefab;

    // For now, we are only setting up the grid and cell size.
    // Room generation logic will be added step by step later.

    private void Start()
    {
        Vector2Int roomSize = new Vector2Int(4, 4); // Not strictly needed, but for clarity
        Vector2Int gridCenter = new Vector2Int(gridSize / 2, gridSize / 2);

        // Room 1: Centered
        GameObject roomObj1 = Instantiate(combatRoomPrefab, Vector3.zero, Quaternion.identity, this.transform);
        CombatRoom room1 = roomObj1.GetComponent<CombatRoom>();
        room1.minRoomSize = 4;
        room1.maxRoomSize = 4;
        room1.doorRight = true;
        room1.doorY = 2; // For example, middle of the wall
        room1.SetupRoom(gridCenter, roomSize, cellSize, gridSize, this);
        Debug.Log("Room 1: " + room1.doorRight + " " + room1.doorY);

        // Room 2: Directly to the right of Room 1
        Vector2Int room2StartPos = gridCenter + new Vector2Int(4, 0); // Offset by width of room 1
        GameObject roomObj2 = Instantiate(combatRoomPrefab, Vector3.zero, Quaternion.identity, this.transform);
        CombatRoom room2 = roomObj2.GetComponent<CombatRoom>();
        room2.minRoomSize = 4;
        room2.maxRoomSize = 4;
        room2.doorLeft = false; // This room keeps its wall
        room2.doorY = 2; // Must match room1's doorY for alignment
        room2.adjacentDoorLeft = true;
        room2.adjacentDoorY = 2;
        room2.SetupRoom(room2StartPos, roomSize, cellSize, gridSize, this);
        Debug.Log("Room 2: " + room2.doorLeft + " " + room2.doorY);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float offset = gridSize * cellSize * 0.5f;
        return new Vector3(
            gridPos.x * cellSize - offset + cellSize * 0.5f,
            0,
            gridPos.y * cellSize - offset + cellSize * 0.5f
        ) + this.transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        float worldSize = gridSize * cellSize;
        Vector3 origin = transform.position - new Vector3(worldSize, 0, worldSize) * 0.5f;

        // Draw vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, worldSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw horizontal lines
        for (int y = 0; y <= gridSize; y++)
        {
            Vector3 start = origin + new Vector3(0, 0, y * cellSize);
            Vector3 end = start + new Vector3(worldSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        // Example: Visualize the center and sides for a room at the center
        Vector2Int gridCenter = new Vector2Int(gridSize / 2, gridSize / 2);
        Vector2Int roomSize = new Vector2Int(6, 6); // Or use actual room size if available

        // Top (Red) - just above the top row
        Gizmos.color = Color.red;
        for (int x = 0; x < gridSize; x++)
        {
            Vector2Int gridPos = new Vector2Int(x, gridSize); // y = gridSize (just above)
            Vector3 worldPos = GridToWorld(gridPos);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.3f, cellSize));
        }
        // Bottom (Blue) - just below the bottom row
        Gizmos.color = Color.blue;
        for (int x = 0; x < gridSize; x++)
        {
            Vector2Int gridPos = new Vector2Int(x, -1); // y = -1 (just below)
            Vector3 worldPos = GridToWorld(gridPos);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.3f, cellSize));
        }
        // Left (Yellow) - just left of the left column
        Gizmos.color = Color.yellow;
        for (int y = 0; y < gridSize; y++)
        {
            Vector2Int gridPos = new Vector2Int(-1, y); // x = -1 (just left)
            Vector3 worldPos = GridToWorld(gridPos);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.3f, cellSize));
        }
        // Right (Magenta) - just right of the right column
        Gizmos.color = Color.magenta;
        for (int y = 0; y < gridSize; y++)
        {
            Vector2Int gridPos = new Vector2Int(gridSize, y); // x = gridSize (just right)
            Vector3 worldPos = GridToWorld(gridPos);
            Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.3f, cellSize));
        }
    }

    public Vector2Int GetGridDirection(Vector2Int from, Vector2Int to)
    {
        return to - from; // Returns the direction from one point to another
    }

    public float GetGridAngle(Vector2Int direction)
    {
        return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg; // Returns the angle in degrees
    }

    public enum RoomSide { Top, Bottom, Left, Right }

    public Vector2Int GetEdgeCell(Vector2Int start, Vector2Int size, RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Top: return new Vector2Int(start.x + size.x / 2, start.y + size.y - 1);
            case RoomSide.Bottom: return new Vector2Int(start.x + size.x / 2, start.y);
            case RoomSide.Left: return new Vector2Int(start.x, start.y + size.y / 2);
            case RoomSide.Right: return new Vector2Int(start.x + size.x - 1, start.y + size.y / 2);
            default: return start;
        }
    }
}

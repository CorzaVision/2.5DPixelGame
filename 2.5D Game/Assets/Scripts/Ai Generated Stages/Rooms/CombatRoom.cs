using UnityEngine;

public class CombatRoom : MonoBehaviour
{
    private Vector2Int startPos;
    private Vector2Int roomSize;
    private float cellSize;
    private int gridSize;
    private StageGenerator stageGenerator;

    public GameObject floorPrefab;
    public GameObject wallStraightPrefab;
    public GameObject wallCornerPrefab;

    [SerializeField] public int minRoomSize = 3;
    [SerializeField] public int maxRoomSize = 6;

    public Vector2Int RoomSize => roomSize;

    public bool doorLeft, doorRight, doorTop, doorBottom;

    public int doorX = -1; // For top/bottom doors (column index)
    public int doorY = -1; // For left/right doors (row index)

    public bool adjacentDoorLeft;
    public int adjacentDoorY = -1;

    public bool adjacentDoorRight;
    public int adjacentDoorYRight = -1;

    public bool adjacentDoorTop;
    public int adjacentDoorXTop = -1;

    public bool adjacentDoorBottom;
    public int adjacentDoorXBottom = -1;

    public void SetupRoom(Vector2Int startPos, Vector2Int maxAllowedSize, float cellSize, int gridSize, StageGenerator stageGenerator)
    {
        this.startPos = startPos;
        this.cellSize = cellSize;
        this.gridSize = gridSize;
        this.stageGenerator = stageGenerator;

        // Random size within allowed range
        int width = Random.Range(minRoomSize, maxRoomSize + 1);
        int height = Random.Range(minRoomSize, maxRoomSize + 1);
        this.roomSize = new Vector2Int(width, height);

        GenerateRoomPrefabs();
    }

    public void GenerateRoomPrefabs()
    {
        Vector2 center = new Vector2(startPos.x + (roomSize.x - 1) / 2f, startPos.y + (roomSize.y - 1) / 2f);

        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int gridPos = startPos + new Vector2Int(x, y);
                Vector3 worldPos = stageGenerator.GridToWorld(gridPos);

                bool atLeft = x == 0;
                bool atRight = x == roomSize.x - 1;
                bool atBottom = y == 0;
                bool atTop = y == roomSize.y - 1;

                GameObject prefabToSpawn = floorPrefab;
                Quaternion rot = Quaternion.identity;

                // 1. Decide prefab
                if (atLeft && atBottom)
                    prefabToSpawn = wallCornerPrefab;
                else if (atRight && atBottom)
                    prefabToSpawn = wallCornerPrefab;
                else if (atRight && atTop)
                    prefabToSpawn = wallCornerPrefab;
                else if (atLeft && atTop)
                    prefabToSpawn = wallCornerPrefab;
                else if (atLeft)
                {
                    if ((doorLeft && y == doorY) || (adjacentDoorLeft && y == adjacentDoorY))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Left (adjacent)");
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                    }
                }
                else if (atRight)
                {
                    if ((doorRight && y == doorY) || (adjacentDoorRight && y == adjacentDoorYRight))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Right");
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                    }
                }
                else if (atTop)
                {
                    if ((doorTop && x == doorX) || (adjacentDoorTop && x == adjacentDoorXTop))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Top");
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                    }
                }
                else if (atBottom)
                {
                    if ((doorBottom && x == doorX) || (adjacentDoorBottom && x == adjacentDoorXBottom))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Bottom");
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                    }
                }

                // 2. Decide rotation
                if (prefabToSpawn == wallCornerPrefab)
                {
                    if (atLeft && atBottom) rot = Quaternion.Euler(0, -180, 0);
                    else if (atRight && atBottom) rot = Quaternion.Euler(0, 90, 0);
                    else if (atRight && atTop) rot = Quaternion.Euler(0, 0, 0);
                    else if (atLeft && atTop) rot = Quaternion.Euler(0, 270, 0);
                }
                else if (prefabToSpawn == wallStraightPrefab)
                {
                    if (atLeft) rot = Quaternion.Euler(0, 180, 0);
                    else if (atRight) rot = Quaternion.Euler(0, 0, 0);
                    else if (atTop) rot = Quaternion.Euler(0, -90, 0);
                    else if (atBottom) rot = Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    rot = Quaternion.identity; // Floor or door
                }

                Instantiate(prefabToSpawn, worldPos, rot, this.transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (roomSize == Vector2Int.zero || stageGenerator == null) return;

        // Draw the room area
        Gizmos.color = Color.green;
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int gridPos = startPos + new Vector2Int(x, y);
                Vector3 worldPos = stageGenerator.GridToWorld(gridPos);
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f));
            }
        }

        // Draw room sides (Top, Bottom, Left, Right) as before...
        // (keep your colored wire cubes for the room sides here)
    }
} 
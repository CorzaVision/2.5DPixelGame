using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StageGenerator : MonoBehaviour
{
    #region Grid Configuration
    [Header("Grid Configuration")]
    [SerializeField] private int gridSize = 20;
    [SerializeField] private float cellSize = 3.0f;
    
    // The grid data structure
    private TileType[,] grid;

    [Header("Room Generation")]
    [SerializeField] private int minRoomSize = 2;
    [SerializeField] private int maxRoomSize = 5;
    [SerializeField] private int minRoomCount = 15;
    [SerializeField] private int maxRoomCount = 25;

    [Header("Special Room Settings")]
    [SerializeField] private int minBossRoomArea = 8;
    [SerializeField] private int minMiniBossRoomArea = 6;    [SerializeField
] private bool generateBossRoom = true;
    [SerializeField] private bool generateMiniBossRoom = true;
    [SerializeField] private Vector2Int bossRoomSize = new Vector2Int(4, 4);
    [SerializeField] private Vector2Int miniBossRoomSize = new Vector2Int(3, 3);

    #endregion

    #region Room Management

    /// <summary>
    /// Represents a room in the dungeon
    /// </summary>
    [System.Serializable]
    public class Room
    {
        public Vector2Int position;
        public Vector2Int size;
        public RoomType roomType;
        
        public Room(Vector2Int pos, Vector2Int roomSize, RoomType type)
        {
            position = pos;
            size = roomSize;
            roomType = type;
        }
    }

    // Store our rooms
    private List<Room> rooms = new List<Room>();
    private Room startRoom;
    private Room exitRoom;

    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeGrid();
    }
    #endregion

    #region Grid Initialization
    /// <summary>
    /// Initialize the grid with empty tiles
    /// </summary>
    private void InitializeGrid()
    {
        // Fill entire grid with empty tiles
        grid = new TileType[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = TileType.Empty;
            }
        }

        Debug.Log($"Grid initialized with: {gridSize}x{gridSize} = {gridSize * gridSize} tiles");
        
        // Create start and exit rooms
        CreateStartAndExitRooms();
        // Place Boss Room near Exit if enabled
        if (generateBossRoom)
        {
            Vector2Int bossPos = FindValidRoomPositionNear(exitRoom, bossRoomSize);
            if (bossPos != Vector2Int.one * -1)
                CreateRoom(bossPos.x, bossPos.y, bossRoomSize.x, bossRoomSize.y, RoomType.Boss);
        }
        // Place MiniBoss Room near Exit if enabled
        if (generateMiniBossRoom)
        {
            Vector2Int miniBossPos = FindValidRoomPositionNear(exitRoom, miniBossRoomSize);
            if (miniBossPos != Vector2Int.one * -1)
                CreateRoom(miniBossPos.x, miniBossPos.y, miniBossRoomSize.x, miniBossRoomSize.y, RoomType.MiniBoss);
        }
        // Generate random rooms
        GenerateRandomRooms(minRoomCount, maxRoomCount, minRoomSize, maxRoomSize);
        // Connect all rooms with shortest path
        ConnectRoomsShortestPath();
        // Place internal room doors
        PlaceInternalRoomDoors();
        // Categorize rooms
        CategorizeRooms();
    }

    /// <summary>
    /// Creates Start and Exit rooms according to requirements
    /// </summary>
    private void CreateStartAndExitRooms()
    {
        // Create rooms
        startRoom = CreateRoom(2, 2, 2, 2, RoomType.Start);
        exitRoom = CreateRoom(gridSize - 4, gridSize - 4, 2, 2, RoomType.Exit);

        if (startRoom != null && exitRoom != null)
        {
            Debug.Log("Start and Exit rooms created successfully!");
        }

        // Get centers
        Vector2Int startCenter = new Vector2Int(
            startRoom.position.x + startRoom.size.x / 2, 
            startRoom.position.y + startRoom.size.y / 2
        );
        Vector2Int exitCenter = new Vector2Int(
            exitRoom.position.x + exitRoom.size.x / 2,
            exitRoom.position.y + exitRoom.size.y / 2
        );

    }

    private List<Vector2Int> GenerateLShapedHallway(Vector2Int from, Vector2Int to)
    {
        int x = from.x;
        int y = from.y;
        Vector2Int lastTile = from;

        bool horizontalFirst = Random.value < 0.5f;

        List<Vector2Int> path = new List<Vector2Int>();

        if (horizontalFirst)
        {
            while (x != to.x)
            {
                x += (to.x > x) ? 1 : -1;
                if (grid[x, y] == TileType.Empty)
                {
                    SetTile(x, y, TileType.Hallway);
                }
                lastTile = new Vector2Int(x, y);
                path.Add(lastTile);
            }
            while (y != to.y)
            {
                y += (to.y > y) ? 1 : -1;
                if (grid[x, y] == TileType.Empty)
                {
                    SetTile(x, y, TileType.Hallway);
                }
                lastTile = new Vector2Int(x, y);
                path.Add(lastTile);
            }
        }
        else
        {
            while (y != to.y)
            {
                y += (to.y > y) ? 1 : -1;
                if (grid[x, y] == TileType.Empty)
                {
                    SetTile(x, y, TileType.Hallway);
                }
                lastTile = new Vector2Int(x, y);
                path.Add(lastTile);
            }
            while (x != to.x)
            {
                x += (to.x > x) ? 1 : -1;
                if (grid[x, y] == TileType.Empty)
                {
                    SetTile(x, y, TileType.Hallway);
                }
                lastTile = new Vector2Int(x, y);
                path.Add(lastTile);
            }
        }
        return path;
    }
    private void GenerateRandomRooms(int minRoomCount, int maxRoomCount, int minRoomSize, int maxRoomSize)
    {
        int attempts = 0;
        int maxAttempts = 100;

        while (attempts < maxAttempts && rooms.Count < minRoomCount)
        {
            int width = Random.Range(minRoomSize, maxRoomSize + 1);
            int height = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(0, gridSize - width);
            int y = Random.Range(0, gridSize - height);

            if (!WouldOverlapWithExistingRooms(x, y, width, height))
            {
                CreateRoom(x, y, width, height, RoomType.Combat);
            }
        }
    }

    private bool WouldOverlapWithExistingRooms(int startX, int startY, int width, int height)
    {
        foreach (var room in rooms)
        {
            if (startX < room.position.x + room.size.x &&
             startX + width > room.position.x &&
             startY < room.position.y + room.size.y &&
             startY + height > room.position.y)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Grid Utilities

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }

    private bool SetTile(int x, int y, TileType type)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogWarning($"Attempted to set tile at ({x}, {y}) but it's out of bounds");
            return false;
        }
        if (grid[x, y] == TileType.Floor && type == TileType.Hallway)
        {
            Debug.LogWarning($"Attempted to set hallway tile at ({x}, {y}) but it's a floor tile");
            return false;
        }

        grid[x, y] = type;
        return true;
    }

    private TileType GetTile(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            Debug.LogWarning($"Attempted to get tile at ({x}, {y}) but it's out of bounds");
            return TileType.Empty;
        }
        return grid[x, y];
    }
    
    #endregion
    #region Room Generation

    /// <summary>
    /// Creates a room at the specified position with given size and type
    /// </summary>
    /// <param name="startX">Starting X coordinate</param>
    /// <param name="startY">Starting Y coordinate</param>
    /// <param name="width">Room width</param>
    /// <param name="height">Room height</param>
    /// <param name="roomType">Type of room to create</param>
    /// <returns>The created room, or null if placement failed</returns>
    private Room CreateRoom(int startX, int startY, int width, int height, RoomType roomType)
    {
        // Check if room fits within grid bounds
        if (!IsValidPosition(startX, startY) || !IsValidPosition(startX + width - 1, startY + height - 1))
        {
            Debug.LogWarning($"Room at ({startX}, {startY}) size {width}x{height} doesn't fit in grid");
            return null;
        }
        
        // Check for room overlap (we'll implement this next)
        if (WouldOverlapWithExistingRooms(startX, startY, width, height))
        {
            Debug.LogWarning($"Room at ({startX}, {startY}) would overlap with existing room");
            return null;
        }
        
        // Create the room by setting floor tiles
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                SetTile(x, y, TileType.Floor);
            }
        }
        
        // Create room object and add to our list
        Room newRoom = new Room(new Vector2Int(startX, startY), new Vector2Int(width, height), roomType);
        rooms.Add(newRoom);
        
        Debug.Log($"Created {roomType} room at ({startX}, {startY}) size {width}x{height}");
        return newRoom;
    }
    private Vector2Int GetRoomCenter(Room room)
    {
        return new Vector2Int(
            room.position.x + room.size.x / 2,
            room.position.y + room.size.y / 2
        );
    }
    private void ConnectRoomsShortestPath()
    {
        HashSet<Room> connectedRooms = new HashSet<Room>();
        connectedRooms.Add(startRoom); // or all rooms on the main path

        while (connectedRooms.Count < rooms.Count)
        {
            Room closestUnconnected = null;
            Room closestConnected = null;
            float minDist = float.MaxValue;

            foreach (var room in rooms)
            {
                if (connectedRooms.Contains(room)) continue;
                foreach (var connected in connectedRooms)
                {
                    float dist = Vector2Int.Distance(GetRoomCenter(room), GetRoomCenter(connected));
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestUnconnected = room;
                        closestConnected = connected;
                    }
                }
            }

            if (closestUnconnected != null && closestConnected != null)
            {
                // Generate hallway and place a single door at the junction
                var hallwayPath = GenerateLShapedHallway(GetRoomCenter(closestUnconnected), GetRoomCenter(closestConnected));
                Vector2Int doorA = FindDoorPosition(closestUnconnected, hallwayPath);
                Vector2Int doorB = FindDoorPosition(closestConnected, hallwayPath);
                SetTile(doorA.x, doorA.y, TileType.Door);
                SetTile(doorB.x, doorB.y, TileType.Door);

                connectedRooms.Add(closestUnconnected);
            }
            else
            {
                break; // All rooms are connected
            }
        }
    }

    #endregion

    #region Enums
    public enum TileType
    {
        Empty,
        Wall,
        Floor,
        Door,
        Hallway
    }

    public enum RoomType
    {
        Start,
        Exit,
        Combat,
        Treasure,
        MiniBoss,
        Boss,
        Puzzle,
        Hallway,
    }
    #endregion

    #region Grid Visualization

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        DrawGridLines();
        DrawTileContent();
    }

    /// <summary>
    /// Draws the grid lines in the Scene view
    /// </summary>
    private void DrawGridLines()
    {
        Gizmos.color = Color.gray;
        
        Vector3 worldSize = new Vector3(gridSize * cellSize, 0, gridSize * cellSize);
        Vector3 startPos = transform.position - worldSize * 0.5f;
        
        // Draw vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 lineStart = startPos + new Vector3(x * cellSize, 0, 0);
            Vector3 lineEnd = lineStart + new Vector3(0, 0, worldSize.z);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
        
        // Draw horizontal lines
        for (int y = 0; y <= gridSize; y++)
        {
            Vector3 lineStart = startPos + new Vector3(0, 0, y * cellSize);
            Vector3 lineEnd = lineStart + new Vector3(worldSize.x, 0, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }

    /// <summary>
    /// Draws colored cubes for different tile types
    /// </summary>
    private void DrawTileContent()
    {
        Vector3 worldSize = new Vector3(gridSize * cellSize, 0, gridSize * cellSize);
        Vector3 startPos = transform.position - worldSize * 0.5f;
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                TileType tile = grid[x, y];
                if (tile == TileType.Empty) continue;
                
                // Set color based on tile type
                switch (tile)
                {
                    case TileType.Floor:
                    {
                        // Find which room this tile belongs to
                        Room room = GetRoomAt(new Vector2Int(x, y));
                        if (room != null)
                        {
                            switch (room.roomType)
                            {
                                case RoomType.Start:
                                    Gizmos.color = Color.green;
                                    break;
                                case RoomType.Exit:
                                    Gizmos.color = Color.magenta;
                                    break;
                                case RoomType.Boss:
                                    Gizmos.color = Color.red;
                                    break;
                                case RoomType.Treasure:
                                    Gizmos.color = Color.yellow;
                                    break;
                                case RoomType.MiniBoss:
                                    Gizmos.color = new Color(1f, 0.5f, 0f); // orange
                                    break;
                                case RoomType.Puzzle:
                                    Gizmos.color = Color.cyan;
                                    break;
                                case RoomType.Combat:
                                default:
                                    Gizmos.color = Color.white;
                                    break;
                            }
                        }
                        else
                        {
                            Gizmos.color = Color.gray; // fallback
                        }
                        break;
                    }
                    case TileType.Wall:
                        Gizmos.color = Color.red;
                        break;
                    case TileType.Door:
                        Gizmos.color = Color.blue;
                        break;
                    case TileType.Hallway:
                        Gizmos.color = Color.grey;
                        break;
                }
                
                // Calculate world position
                Vector3 worldPos = startPos + new Vector3(
                    (x + 0.5f) * cellSize, 
                    0.5f, 
                    (y + 0.5f) * cellSize
                );
                
                // Draw cube for this tile
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.95f, 0.2f, cellSize * 0.95f));
            }
        }
    }

    #endregion

    private Vector2Int FindDoorPosition(Room room, List<Vector2Int> hallwayPath)
    {
        foreach (var tile in hallwayPath)
        {
            // Only consider tiles NOT inside the room
            if (!IsInsideRoom(room, tile))
            {
                // Check if this hallway tile is adjacent to the room
                foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    Vector2Int neighbor = tile + dir;
                    if (IsInsideRoom(room, neighbor))
                    {
                        return tile; // This is the edge tile for the door
                    }
                }
            }
        }
        return hallwayPath[0]; // fallback
    }

    private bool IsInsideRoom(Room room, Vector2Int pos)
    {
        return pos.x >= room.position.x && pos.x < room.position.x + room.size.x &&
               pos.y >= room.position.y && pos.y < room.position.y + room.size.y;
    }

    private Room GetRoomAt(Vector2Int pos)
    {
        foreach (var room in rooms)
        {
            if (pos.x >= room.position.x && pos.x < room.position.x + room.size.x &&
                pos.y >= room.position.y && pos.y < room.position.y + room.size.y)
            {
                return room;
            }
        }
        return null;
    }

    private void PlaceInternalRoomDoors()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        HashSet<(Room, Room)> processedPairs = new HashSet<(Room, Room)>();

        foreach (var room in rooms)
        {
            foreach (var perimeterTile in GetRoomPerimeter(room))
            {
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = perimeterTile + dir;
                    if (IsValidPosition(neighbor.x, neighbor.y) && grid[neighbor.x, neighbor.y] == TileType.Floor)
                    {
                        Room neighborRoom = GetRoomAt(neighbor);
                        if (neighborRoom != null && neighborRoom != room)
                        {
                            // Ensure we only process each pair once
                            var pair = room.GetHashCode() < neighborRoom.GetHashCode()
                                ? (room, neighborRoom)
                                : (neighborRoom, room);

                            if (!processedPairs.Contains(pair))
                            {
                                // Collect all shared wall tiles
                                List<Vector2Int> sharedWall = new List<Vector2Int>();
                                foreach (var tile in GetRoomPerimeter(room))
                                {
                                    foreach (var d in directions)
                                    {
                                        Vector2Int n = tile + d;
                                        if (IsValidPosition(n.x, n.y) && grid[n.x, n.y] == TileType.Floor)
                                        {
                                            Room nRoom = GetRoomAt(n);
                                            if (nRoom == neighborRoom)
                                            {
                                                sharedWall.Add(tile);
                                            }
                                        }
                                    }
                                }
                                // Pick one tile to be the door
                                if (sharedWall.Count > 0)
                                {
                                    Vector2Int doorTile = sharedWall[Random.Range(0, sharedWall.Count)];
                                    grid[doorTile.x, doorTile.y] = TileType.Door;
                                }
                                processedPairs.Add(pair);
                            }
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<Vector2Int> GetRoomPerimeter(Room room)
    {
        for (int x = room.position.x; x < room.position.x + room.size.x; x++)
        {
            yield return new Vector2Int(x, room.position.y); // bottom
            yield return new Vector2Int(x, room.position.y + room.size.y - 1); // top
        }
        for (int y = room.position.y + 1; y < room.position.y + room.size.y - 1; y++)
        {
            yield return new Vector2Int(room.position.x, y); // left
            yield return new Vector2Int(room.position.x + room.size.x - 1, y); // right
        }
    }

    private void CategorizeRooms()
    {
        // Filter out Start and Exit
        var candidates = rooms.Where(r => r != startRoom && r != exitRoom).ToList();

        // Sort by distance to Exit (closest first)
        candidates = candidates.OrderBy(r => Vector2Int.Distance(GetRoomCenter(exitRoom), GetRoomCenter(r))).ToList();

        // Assign Boss and MiniBoss if enabled and possible
        Room bossRoom = null;
        Room miniBossRoom = null;

        if (generateBossRoom && candidates.Count > 0)
        {
            bossRoom = candidates[0];
            bossRoom.roomType = RoomType.Boss;
            candidates.RemoveAt(0);
        }

        if (generateMiniBossRoom && candidates.Count > 0)
        {
            miniBossRoom = candidates[0];
            miniBossRoom.roomType = RoomType.MiniBoss;
            candidates.RemoveAt(0);
        }

        // Treasure rooms: pick a few at random (not start/exit/boss/miniboss)
        int treasureCount = Mathf.Min(2, candidates.Count);
        for (int i = 0; i < treasureCount; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            if (IsTreasureRoomValid(candidates[idx]))
            {
                candidates[idx].roomType = RoomType.Treasure;
                candidates.RemoveAt(idx);
            }
        }

        // The rest: Combat
        foreach (var room in candidates)
        {
            room.roomType = RoomType.Combat;
        }
    }

    private bool IsTreasureRoomValid(Room treasureRoom)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        bool hasCombatNeighbor = false;

        foreach (var perimeterTile in GetRoomPerimeter(treasureRoom))
        {
            foreach (var dir in directions)
            {
                Vector2Int neighbor = perimeterTile + dir;
                if (IsValidPosition(neighbor.x, neighbor.y))
                {
                    Room neighborRoom = GetRoomAt(neighbor);
                    if (neighborRoom != null && neighborRoom != treasureRoom)
                    {
                        if (neighborRoom.roomType == RoomType.Combat)
                            hasCombatNeighbor = true;
                        else // Adjacent to non-combat room
                            return false;
                    }
                    else if (grid[neighbor.x, neighbor.y] == TileType.Hallway)
                    {
                        // Adjacent to hallway
                        return false;
                    }
                }
            }
        }
        return hasCombatNeighbor;
    }

    private Vector2Int FindValidRoomPositionNear(Room referenceRoom, Vector2Int roomSize)
    {
        // Try positions in a spiral around the reference room center
        Vector2Int center = GetRoomCenter(referenceRoom);
        int maxRadius = Mathf.Max(gridSize, gridSize);
        for (int radius = 1; radius < maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue; // Only perimeter
                    int x = center.x + dx - roomSize.x / 2;
                    int y = center.y + dy - roomSize.y / 2;
                    if (IsValidPosition(x, y) && IsValidPosition(x + roomSize.x - 1, y + roomSize.y - 1))
                    {
                        if (!WouldOverlapWithExistingRooms(x, y, roomSize.x, roomSize.y))
                            return new Vector2Int(x, y);
                    }
                }
            }
        }
        return Vector2Int.one * -1; // Not found
    }
}

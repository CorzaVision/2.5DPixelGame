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
        
        // TEST: Create Start and Exit rooms on opposite sides
        CreateStartAndExitRooms();
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

        // Generate hallway and get path
        var hallwayPath = GenerateLShapedHallway(startCenter, exitCenter);

        // Place doors at the edge of each room
        Vector2Int startDoor = FindDoorPosition(startRoom, hallwayPath);
        Vector2Int exitDoor = FindDoorPosition(exitRoom, hallwayPath);
        SetTile(startDoor.x, startDoor.y, TileType.Door);
        SetTile(exitDoor.x, exitDoor.y, TileType.Door);
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
                lastTile = new Vector2Int(x, y);
                SetTile(x, y, TileType.Hallway);
                path.Add(lastTile);
            }
            while (y != to.y)
            {
                y += (to.y > y) ? 1 : -1;
                lastTile = new Vector2Int(x, y);
                SetTile(x, y, TileType.Hallway);
                path.Add(lastTile);
            }
        }
        else
        {
            while (y != to.y)
            {
                y += (to.y > y) ? 1 : -1;
                lastTile = new Vector2Int(x, y);
                SetTile(x, y, TileType.Hallway);
                path.Add(lastTile);
            }
            while (x != to.x)
            {
                x += (to.x > x) ? 1 : -1;
                lastTile = new Vector2Int(x, y);
                SetTile(x, y, TileType.Hallway);
                path.Add(lastTile);
            }
        }
        return path;
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

    /// <summary>
    /// Checks if a room would overlap with any existing rooms
    /// </summary>
    private bool WouldOverlapWithExistingRooms(int startX, int startY, int width, int height)
    {
        // For now, just return false - we'll implement proper overlap checking later
        return false;
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
                        Gizmos.color = Color.green;
                        break;
                    case TileType.Wall:
                        Gizmos.color = Color.red;
                        break;
                    case TileType.Door:
                        Gizmos.color = Color.blue;
                        break;
                    case TileType.Hallway:
                        Gizmos.color = Color.yellow;
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
            // Check if this hallway tile is adjacent to the room
            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = tile + dir;
                if (neighbor.x >= room.position.x && neighbor.x < room.position.x + room.size.x &&
                    neighbor.y >= room.position.y && neighbor.y < room.position.y + room.size.y)
                {
                    return tile; // This is the edge tile for the door
                }
            }
        }
        return hallwayPath[0]; // fallback
    }
}

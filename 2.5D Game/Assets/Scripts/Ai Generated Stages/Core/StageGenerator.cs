using UnityEngine;
using System.Collections.Generic;

public class StageGenerator : MonoBehaviour, IRoomGenerator, IStageGenerator
{
    [Header("Grid Settings")]
    public int gridSize = 20;
    public float cellSize = 1f;
    
    [Header("Room Sizes")]
    public Vector2Int minRoomSize = new Vector2Int(2, 2);
    public Vector2Int maxRoomSize = new Vector2Int(4, 4);
    
    [Header("Generation Settings")]
    public StageData stageData;
    public RoomType roomType;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool generateOnStart = false;
    
    private StageLayout currentLayout;
    private bool[,] occupiedGrid;
    
    void Start()
    {
        if (generateOnStart && stageData != null)
        {
            GenerateStage();
        }
    }
    
    public StageLayout GenerateStage()
    {
        if (stageData == null)
        {
            Debug.LogError("StageData is not assigned!");
            return null;
        }
        
        currentLayout = new StageLayout();
        occupiedGrid = new bool[gridSize, gridSize];
        
        Debug.Log("Starting stage generation...");
        
        // Place starting room
        PlaceStartingRoom();
        
        // Place exit room
        PlaceExitRoom();
        
        // Generate main path rooms
        GenerateMainPathRooms();

        // Connect rooms with hallways
        ConnectGridRooms();

        // Ensure all rooms are connected
        EnsureAllRoomsConnected();
        
        Debug.Log("Stage generation completed!");
        return currentLayout;
    }
    
    void PlaceStartingRoom()
    {
        Vector2Int startPos = stageData.startRoomPosition;
        Vector2Int startSize = stageData.startRoomSize;
        
        PlaceRoom(startPos, startSize, roomType);
        currentLayout.startRoom = currentLayout.rooms[0];
        currentLayout.playerSpawnPosition = startPos + stageData.playerSpawnPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"Placed starting room at {startPos} with size {startSize}");
        }
    }
    
    void PlaceExitRoom()
    {
        Vector2Int exitPos;
        Vector2Int exitSize = stageData.exitRoomSize;
        int minDistance = gridSize / 2; // Or any value you want

        int attempts = 0;
        int maxAttempts = 100;

        do
        {
            exitPos = new Vector2Int(
                Random.Range(0, gridSize - exitSize.x),
                Random.Range(0, gridSize - exitSize.y)
            );
            attempts++;
        }
        while (
            Vector2Int.Distance(exitPos, stageData.startRoomPosition) < minDistance
            && attempts < maxAttempts
        );

        PlaceRoom(exitPos, exitSize, roomType);
        currentLayout.exitRoom = currentLayout.rooms[currentLayout.rooms.Count - 1];
        currentLayout.mainExitPosition = exitPos;

        if (showDebugInfo)
        {
            Debug.Log($"Placed exit room at {exitPos} with size {exitSize}");
        }
    }
    
    void PlaceRoom(Vector2Int position, Vector2Int roomSize, RoomType roomType)
    {
        RoomData room = new RoomData
        {
            position = position,
            size = roomSize,
            roomType = roomType
        };
        
        // Mark grid cells as occupied
        for (int x = position.x; x < position.x + roomSize.x; x++)
        {
            for (int y = position.y; y < position.y + roomSize.y; y++)
            {
                if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                {
                    occupiedGrid[x, y] = true;
                }
            }
        }
        
        currentLayout.rooms.Add(room);
        
        if (showDebugInfo)
        {
            Debug.Log($"Placed {roomType} room at {position} with size {roomSize}");
        }
    }
    
    void GenerateMainPathRooms()
    {
        int roomsToGenerate = stageData.roomCount - 2; // Subtract start and exit rooms

        for (int i = 0; i < roomsToGenerate; i++)
        {
            Vector2Int roomSize = new Vector2Int(
                Random.Range(minRoomSize.x, maxRoomSize.x + 1),
                Random.Range(minRoomSize.y, maxRoomSize.y + 1)
            );

            Vector2Int roomPos = FindValidRoomPosition(roomSize);

            if (roomPos != new Vector2Int(-1, -1)) // -1, -1 means no valid position found
            {
                PlaceRoom(roomPos, roomSize, roomType);
            }
            else
            {
                Debug.LogWarning($"Could not find valid position for room {i}");
            }
        }
    }
    
    Vector2Int FindValidRoomPosition(Vector2Int roomSize)
    {
        int attempts = 0;
        int maxAttempts = 100;

        int gridStep = Mathf.Max(roomSize.x, roomSize.y);

        while (attempts < maxAttempts)
        {
            int x = Random.Range(0, (gridSize - roomSize.x) / gridStep + 1) * gridStep;
            int y = Random.Range(0, (gridSize - roomSize.y) / gridStep + 1) * gridStep;
            Vector2Int randomPos = new Vector2Int(x, y);

            if (CanPlaceRoomAt(randomPos, roomSize))
            {
                return randomPos;
            }

            attempts++;
        }

        return new Vector2Int(-1, -1); // No valid position found
    }

    void ConnectGridRooms()
    {
        foreach (RoomData fromRoom in currentLayout.rooms)
        {
            if (fromRoom == currentLayout.exitRoom) continue; // Don't branch from exit
            foreach (RoomData toRoom in currentLayout.rooms)
            {
                if (fromRoom == toRoom) continue; // Skip self-connection

                if (ShouldConnectRooms(fromRoom, toRoom, 2)) // 2-tile max gap
                {
                    // Avoid duplicate hallways
                    bool alreadyConnected = currentLayout.hallways.Exists(h =>
                        (h.startRoom == fromRoom.position && h.endRoom == toRoom.position) ||
                        (h.startRoom == toRoom.position && h.endRoom == fromRoom.position)
                    );
                    if (!alreadyConnected)
                    {
                        CreateHallway(fromRoom, toRoom);
                    }
                }
            }
        }
    }

    void CreateHallway(RoomData roomA, RoomData roomB)
    {
        HallwayData hallway = new HallwayData
        {
            startRoom = roomA.position,
            endRoom = roomB.position,
            isBranch = false,
            path = new List<Vector2Int>()
        };

        // Use edge points instead of centers
        Vector2Int start = GetClosestEdgePoint(roomA, roomB);
        Vector2Int end = GetClosestEdgePoint(roomB, roomA);

        Vector2Int point = start;
        int maxSteps = gridSize * 2;
        int steps = 0;

        // L-shaped hallway: horizontal, then vertical
        while (point.x != end.x && steps < maxSteps)
        {
            point.x += point.x < end.x ? 1 : -1;
            hallway.path.Add(new Vector2Int(point.x, point.y));
            steps++;
        }
        while (point.y != end.y && steps < maxSteps)
        {
            point.y += point.y < end.y ? 1 : -1;
            hallway.path.Add(new Vector2Int(point.x, point.y));
            steps++;
        }

        if (steps >= maxSteps)
        {
            Debug.LogWarning("Hallway generation exceeded max steps! Possible infinite loop avoided.");
        }

        currentLayout.hallways.Add(hallway);

        // Mark hallway cells as occupied
        foreach (Vector2Int cell in hallway.path)
        {
            if (cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize)
            {
                occupiedGrid[cell.x, cell.y] = true;
            }
        }
    }
    
    bool CanPlaceRoomAt(Vector2Int position, Vector2Int roomSize)
    {
        for (int x = position.x; x < position.x + roomSize.x; x++)
        {
            for (int y = position.y; y < position.y + roomSize.y; y++)
            {
                if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && occupiedGrid[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    // Debug methods
    public void RegenerateStage()
    {
        GenerateStage();
    }
    
    public void ClearStage()
    {
        currentLayout = null;
        occupiedGrid = null;
        Debug.Log("Stage cleared!");
    }

    Vector2Int GetClosestEdgePoint(RoomData from, RoomData to)
    {
        Vector2Int toCenter = new Vector2Int(
            to.position.x + to.size.x / 2,
            to.position.y + to.size.y / 2
        );

        int left = from.position.x;
        int right = from.position.x + from.size.x - 1;
        int top = from.position.y;
        int bottom = from.position.y + from.size.y - 1;

        int dxLeft = Mathf.Abs(toCenter.x - left);
        int dxRight = Mathf.Abs(toCenter.x - right);
        int dyTop = Mathf.Abs(toCenter.y - top);
        int dyBottom = Mathf.Abs(toCenter.y - bottom);

        int minDist = Mathf.Min(dxLeft, dxRight, dyTop, dyBottom);

        if (minDist == dxLeft)
        return new Vector2Int(left, Mathf.Clamp(toCenter.y, top, bottom));
        if (minDist == dxRight)
            return new Vector2Int(right, Mathf.Clamp(toCenter.y, top, bottom));
        if (minDist == dyTop)
            return new Vector2Int(Mathf.Clamp(toCenter.x, left, right), top);
        if (minDist == dyBottom)
            return new Vector2Int(Mathf.Clamp(toCenter.x, left, right), bottom);
        return new Vector2Int(Mathf.Clamp(toCenter.x, left, right), top);
    }

    bool RoomsAreTouching(RoomData a, RoomData b)
    {
        return a.position.x < b.position.x + b.size.x &&
               a.position.x + a.size.x > b.position.x &&
               a.position.y < b.position.y + b.size.y &&
               a.position.y + a.size.y > b.position.y;
    }
    
    void OnDrawGizmos()
    {
        if (currentLayout == null) return;
        
        // Draw rooms
        foreach (RoomData room in currentLayout.rooms)
        {
            // Different colors for different room types
            switch (room.roomType)
            {
                case RoomType.Safe:
                    Gizmos.color = Color.green; // Starting room
                    break;
                case RoomType.Boss:
                    Gizmos.color = Color.red; // Exit room
                    break;
                case RoomType.Combat:
                    Gizmos.color = Color.yellow; // Combat rooms
                    break;
                case RoomType.Treasure:
                    Gizmos.color = Color.cyan; // Treasure rooms
                    break;
            }
            
            // Draw room as a cube (full size)
            Vector3 center = new Vector3(room.position.x + room.size.x * 0.5f, 0, room.position.y + room.size.y * 0.5f);
            Vector3 size = new Vector3(room.size.x, 1, room.size.y);
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw hallways
        Gizmos.color = Color.white;
        foreach (HallwayData hallway in currentLayout.hallways)
        {
            foreach (Vector2Int point in hallway.path)
            {
                if (!IsInsideAnyRoom(point))
                {
                    Gizmos.DrawWireCube(new Vector3(point.x + 0.5f, 0, point.y + 0.5f), Vector3.one);
                }
            }
        }
    }

    bool ShouldConnectRooms(RoomData a, RoomData b, int maxGap = 2)
    {
        // Check horizontal alignment
        bool horizontallyAligned = (a.position.y < b.position.y + b.size.y) && (a.position.y + a.size.y > b.position.y);
        int horizontalGap = Mathf.Abs((a.position.x + a.size.x) - b.position.x);
        if (horizontallyAligned && horizontalGap > 0 && horizontalGap <= maxGap)
            return true;

        // Check vertical alignment
        bool verticallyAligned = (a.position.x < b.position.x + b.size.x) && (a.position.x + a.size.x > b.position.x);
        int verticalGap = Mathf.Abs((a.position.y + a.size.y) - b.position.y);
        if (verticallyAligned && verticalGap > 0 && verticalGap <= maxGap)
            return true;

        return false;
    }

    void EnsureAllRoomsConnected()
    {
        List<RoomData> connectedRooms = new List<RoomData> { currentLayout.startRoom };
        List<RoomData> unconnectedRooms = new List<RoomData>(currentLayout.rooms);
        unconnectedRooms.Remove(currentLayout.startRoom);

        while (unconnectedRooms.Count > 0)
        {
            float minDist = float.MaxValue;
            RoomData fromRoom = null;
            RoomData toRoom = null;

            // Find closest pair: one in connected, one in unconnected
            foreach (RoomData connected in connectedRooms)
            {
                foreach (RoomData unconnected in unconnectedRooms)
                {
                    float dist = Vector2Int.Distance(
                        new Vector2Int(connected.position.x + connected.size.x / 2, connected.position.y + connected.size.y / 2),
                        new Vector2Int(unconnected.position.x + unconnected.size.x / 2, unconnected.position.y + unconnected.size.y / 2)
                    );

                    if (dist < minDist)
                    {
                        minDist = dist;
                        fromRoom = connected;
                        toRoom = unconnected;
                    }
                }
            }

            if (fromRoom != null && toRoom != null)
            {
                if (!RoomsAreTouching(fromRoom, toRoom))
                {
                    CreateHallway(fromRoom, toRoom);
                    connectedRooms.Add(toRoom);
                    unconnectedRooms.Remove(toRoom);
                }
            }
            else
            {
                Debug.LogWarning("Could not find valid connection between rooms!");
                break;
            }
        }
    }

    bool IsInsideAnyRoom(Vector2Int point)
    {
        foreach (RoomData room in currentLayout.rooms)
        {
            if (point.x >= room.position.x && point.x < room.position.x + room.size.x &&
                point.y >= room.position.y && point.y < room.position.y + room.size.y)
            {
                return true;
            }
        }
        return false;
    }
}





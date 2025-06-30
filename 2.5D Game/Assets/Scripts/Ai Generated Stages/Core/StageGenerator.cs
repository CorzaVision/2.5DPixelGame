using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StageGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 20;
    public float cellSize = 1f;
    
    [Header("Room Sizes")]
    public Vector2Int minRoomSize = new Vector2Int(2, 2);
    public Vector2Int maxRoomSize = new Vector2Int(4, 4);
    
    [Header("Generation Settings")]
    public StageData stageData;
    public RoomCategory roomCategory;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool generateOnStart = false;
    
    private StageLayout currentLayout;
    private bool[,] occupiedGrid;
    
    public struct WallEdge
    {
        public Vector2Int cell;
        public Vector2Int dir; // up, down, left, right

        public WallEdge(Vector2Int c, Vector2Int d)
        {
            cell = c;
            dir = d;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WallEdge)) return false;
            return cell == ((WallEdge)obj).cell && dir == ((WallEdge)obj).dir;
        }

        public override int GetHashCode()
        {
            return cell.GetHashCode() ^ dir.GetHashCode();
        }
    }
    
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
        
        // Limit exit connections
        LimitExitConnections();
        
        // Add extra branches
        AddExtraBranches();
        
        // Clean up hallways for single-connection rooms
        CleanUpHallways();
        
        // Assign treasure rooms to dead ends
        AssignTreasureRooms();

        // Place doors
        PlaceDoors();

        // Generate room walls
        
        DebugRoomConnectivity();
        
        foreach (RoomData room in currentLayout.rooms)
        {
            GenerateRoomWalls(room);
        }
        
        Debug.Log("Stage generation completed!");

        foreach (var hallway in currentLayout.hallways)
        {
            foreach (var cell in hallway.path)
                Debug.Log($"Hallway cell: {cell}");
        }
        foreach (var room in currentLayout.rooms)
        {
            for (int x = room.position.x; x < room.position.x + room.size.x; x++)
            {
                Vector2Int top = new Vector2Int(x, room.position.y + room.size.y - 1);
                Vector2Int topNeighbor = top + Vector2Int.up;
                Debug.Log($"Room {room.roomCategory} top neighbor: {topNeighbor}, IsInAnyHallway: {IsInAnyHallway(topNeighbor)}");
                // Repeat for other edges
            }
        }

        return currentLayout;
    }
    
    void PlaceStartingRoom()
    {
        Vector2Int startPos = stageData.startRoomPosition;
        Vector2Int startSize = stageData.startRoomSize;
        
        PlaceRoom(startPos, startSize, RoomCategory.Start);
        currentLayout.startRoom = currentLayout.rooms[0];
        currentLayout.playerSpawnPosition = startPos + stageData.playerSpawnPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"Placed starting room at {startPos} with size {startSize}");
        }
    }
    
    void PlaceExitRoom()
    {
        Vector2Int exitSize = stageData.exitRoomSize;
        int edge = Random.Range(0, 4); // 0=left, 1=right, 2=top, 3=bottom
        Vector2Int exitPos = Vector2Int.zero;

        int minDistance = gridSize / 2; // or another value
        int attempts = 0;
        int maxAttempts = 20;
        do
        {
            switch (edge)
            {
                case 0: // Left edge
                    exitPos = new Vector2Int(0, Random.Range(0, gridSize - exitSize.y + 1));
                    break;
                case 1: // Right edge
                    exitPos = new Vector2Int(gridSize - exitSize.x, Random.Range(0, gridSize - exitSize.y + 1));
                    break;
                case 2: // Top edge
                    exitPos = new Vector2Int(Random.Range(0, gridSize - exitSize.x + 1), gridSize - exitSize.y);
                    break;
                case 3: // Bottom edge
                    exitPos = new Vector2Int(Random.Range(0, gridSize - exitSize.x + 1), 0);
                    break;
            }
            attempts++;
        }
        while (
            Vector2Int.Distance(exitPos, stageData.startRoomPosition) < minDistance
            && attempts < maxAttempts
        );

        PlaceRoom(exitPos, exitSize, RoomCategory.Exit);
        currentLayout.exitRoom = currentLayout.rooms[currentLayout.rooms.Count - 1];
        currentLayout.mainExitPosition = exitPos;

        if (showDebugInfo)
        {
            Debug.Log($"Placed exit room at {exitPos} with size {exitSize} on edge {edge}");
        }
    }
    
    void PlaceRoom(Vector2Int position, Vector2Int roomSize, RoomCategory roomCategory)
    {
        RoomData room = new RoomData
        {
            position = position,
            size = roomSize,
            roomCategory = roomCategory
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
            Debug.Log($"Placed {roomCategory} room at {position} with size {roomSize}");
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
                RoomCategory type = RoomCategory.Combat;
                if (Random.value < 0.1f) type = RoomCategory.Treasure; // 10% chance for treasure
                PlaceRoom(roomPos, roomSize, type);
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
        List<RoomData> connected = new List<RoomData>();
        List<RoomData> unconnected = new List<RoomData>(currentLayout.rooms);

        connected.Add(currentLayout.startRoom);
        unconnected.Remove(currentLayout.startRoom);

        while (unconnected.Count > 0)
        {
            float minDist = float.MaxValue;
            RoomData from = null, to = null;

            foreach (var c in connected)
            {
                foreach (var u in unconnected)
                {
                    float dist = Vector2Int.Distance(
                        new Vector2Int(c.position.x + c.size.x / 2, c.position.y + c.size.y / 2),
                        new Vector2Int(u.position.x + u.size.x / 2, u.position.y + u.size.y / 2)
                    );
                    if (dist < minDist)
                    {
                        minDist = dist;
                        from = c;
                        to = u;
                    }
                }
            }

            // Use your pathfinding-based CreateHallway here!
            CreateHallway(from, to);

            connected.Add(to);
            unconnected.Remove(to);
        }
    }

    void CreateHallway(RoomData roomA, RoomData roomB)
    {
        // Use room edges instead of centers for more natural connections
        Vector2Int aEdge = GetClosestEdgePoint(roomA, roomB);
        Vector2Int bEdge = GetClosestEdgePoint(roomB, roomA);
        
        List<Vector2Int> path = CreateLShapedHallway(aEdge, bEdge);

        if (path.Count > 0)
        {
            // Calculate door position between the two room edges
            Vector2Int doorPosition = GetDoorPositionBetween(aEdge, bEdge);
            
            HallwayData hallway = new HallwayData
            {
                startRoom = roomA.position,
                endRoom = roomB.position,
                isBranch = false,
                path = path,
            };

            currentLayout.hallways.Add(hallway);

            // Mark hallway cells as occupied (but don't overwrite room cells)
            foreach (Vector2Int cell in hallway.path)
            {
                if (cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize)
                {
                    // Only mark as occupied if not inside a room
                    if (!IsInsideAnyRoom(cell))
                    {
                        occupiedGrid[cell.x, cell.y] = true;
                    }
                }
            }

            // Add door positions at the edges (keep these for room reference)
            roomA.doorPositions.Add(aEdge);
            roomB.doorPositions.Add(bEdge);
            
            Debug.Log($"Created hallway from {roomA.roomCategory} to {roomB.roomCategory} with door at {doorPosition}");

            // Ensure the hallway path includes the cell just outside the room
            Vector2Int dirFromA = NormalizeDirection(path[0] - aEdge);
            Vector2Int cellOutsideA = aEdge + dirFromA;
            if (!path.Contains(cellOutsideA))
                path.Insert(0, cellOutsideA);

            Vector2Int dirFromB = NormalizeDirection(path[path.Count - 1] - bEdge);
            Vector2Int cellOutsideB = bEdge + dirFromB;
            if (!path.Contains(cellOutsideB))
                path.Add(cellOutsideB);
        }
        else
        {
            Debug.LogWarning($"Failed to create hallway from {roomA.roomCategory} to {roomB.roomCategory}");
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

        // Get room bounds
        int left = from.position.x;
        int right = from.position.x + from.size.x - 1;
        int top = from.position.y;
        int bottom = from.position.y + from.size.y - 1;

        // Calculate distances to each edge
        int dxLeft = Mathf.Abs(toCenter.x - left);
        int dxRight = Mathf.Abs(toCenter.x - right);
        int dyTop = Mathf.Abs(toCenter.y - top);
        int dyBottom = Mathf.Abs(toCenter.y - bottom);

        // Find the closest edge
        int minDist = Mathf.Min(dxLeft, dxRight, dyTop, dyBottom);

        if (minDist == dxLeft)
            return new Vector2Int(left, Mathf.Clamp(toCenter.y, top, bottom));
        if (minDist == dxRight)
            return new Vector2Int(right, Mathf.Clamp(toCenter.y, top, bottom));
        if (minDist == dyTop)
            return new Vector2Int(Mathf.Clamp(toCenter.x, left, right), top);
        if (minDist == dyBottom)
            return new Vector2Int(Mathf.Clamp(toCenter.x, left, right), bottom);
        
        // Fallback
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

        // 1. Draw room wireframes (as before)
        foreach (RoomData room in currentLayout.rooms)
        {
            // Set color based on room type if you want
            Gizmos.color = room.roomCategory == RoomCategory.Start ? Color.green :
                           room.roomCategory == RoomCategory.Exit ? Color.red :
                           Color.yellow;

            foreach (Vector2Int wallPos in room.wallPositions)
            {
                // REMOVE or COMMENT OUT this line:
                // Gizmos.DrawCube(center, new Vector3(cellSize, cellSize * 0.5f, cellSize * 0.1f)); // Adjust size as needed
            }
        }

        // 2. Wall overlap avoidance logic
        HashSet<WallEdge> placedWalls = new HashSet<WallEdge>();
        Dictionary<WallEdge, RoomCategory> wallColors = new Dictionary<WallEdge, RoomCategory>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // --- Room perimeter walls with color ---
        foreach (RoomData room in currentLayout.rooms)
        {
            for (int x = room.position.x; x < room.position.x + room.size.x; x++)
            {
                for (int y = room.position.y; y < room.position.y + room.size.y; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    foreach (var dir in directions)
                    {
                        Vector2Int neighbor = cell + dir;
                        if (!IsOpenCell(neighbor))
                        {
                            WallEdge edge = new WallEdge(cell, dir);
                            WallEdge oppositeEdge = new WallEdge(neighbor, -dir);
                            if (!placedWalls.Contains(edge) && !placedWalls.Contains(oppositeEdge))
                            {
                                placedWalls.Add(edge);
                                wallColors[edge] = room.roomCategory;
                            }
                        }
                    }
                }
            }
        }

        // --- Hallway walls (default color) ---
        foreach (HallwayData hallway in currentLayout.hallways)
        {
            foreach (Vector2Int cell in hallway.path)
            {
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = cell + dir;
                    if (!IsOpenCell(neighbor))
                    {
                        WallEdge edge = new WallEdge(cell, dir);
                        WallEdge oppositeEdge = new WallEdge(neighbor, -dir);
                        if (!placedWalls.Contains(edge) && !placedWalls.Contains(oppositeEdge))
                        {
                            placedWalls.Add(edge);
                            wallColors[edge] = RoomCategory.Corridor; // Or use a special enum for hallway
                        }
                    }
                }
            }
        }

        // --- Draw all walls with their stored color ---
        foreach (var kvp in wallColors)
        {
            WallEdge edge = kvp.Key;
            RoomCategory category = kvp.Value;

            // Set color based on category
            switch (category)
            {
                case RoomCategory.Start: Gizmos.color = Color.green; break;
                case RoomCategory.Exit: Gizmos.color = Color.red; break;
                case RoomCategory.Combat: Gizmos.color = Color.yellow; break;
                case RoomCategory.Treasure: Gizmos.color = Color.cyan; break;
                case RoomCategory.MiniBoss: Gizmos.color = Color.magenta; break;
                case RoomCategory.Boss: Gizmos.color = new Color(1.0f, 0.5f, 0.0f); break;
                case RoomCategory.Corridor: Gizmos.color = Color.white; break;
                case RoomCategory.Puzzle: Gizmos.color = Color.blue; break;
                case RoomCategory.Trap: Gizmos.color = new Color(0.5f, 0, 0); break;
                case RoomCategory.Filler: Gizmos.color = Color.gray; break;
                default: Gizmos.color = Color.gray; break;
            }

            // Draw wall at edge.cell in direction edge.dir
            Vector3 wallPos, wallSize;
            Vector2Int cell = edge.cell;
            Vector2Int dir = edge.dir;
            if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                wallPos = new Vector3(cell.x * cellSize + cellSize * 0.5f, cellSize * 0.5f, (cell.y + (dir == Vector2Int.up ? 1 : 0)) * cellSize);
                wallSize = new Vector3(cellSize, cellSize, cellSize * 0.1f);
            }
            else
            {
                wallPos = new Vector3((cell.x + (dir == Vector2Int.right ? 1 : 0)) * cellSize, cellSize * 0.5f, cell.y * cellSize + cellSize * 0.5f);
                wallSize = new Vector3(cellSize * 0.1f, cellSize, cellSize);
            }
            Gizmos.DrawCube(wallPos, wallSize);
        }

        // 3. Draw cubic grid inside each room (as before)
        foreach (RoomData room in currentLayout.rooms)
        {
            // ... set Gizmos.color based on room type ...
            Vector3 center = new Vector3(
                (room.position.x + room.size.x * 0.5f) * cellSize,
                0,
                (room.position.y + room.size.y * 0.5f) * cellSize
            );
            Vector3 size = new Vector3(room.size.x * cellSize, 1, room.size.y * cellSize);
            Gizmos.DrawWireCube(center, size);
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

    public RoomData GenerateRoom(Vector2Int position, Vector2Int size, RoomCategory roomCategory)
    {
        RoomData room = new RoomData
        {
            position = position,
            size = size,
            roomCategory = roomCategory
        };

        // Optionally, mark grid as occupied if you want to track this
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                {
                    occupiedGrid[x, y] = true;
                }
            }
        }

        // Optionally, add to currentLayout.rooms if that's your workflow
        // currentLayout.rooms.Add(room);

        return room;
    }

    // Placeholder for future prefab instantiation
    void PlaceRoomTile(int x, int y, RoomCategory category)
    {
        // In the future: Instantiate the correct prefab here
        // For now: Debug.Log($"Would place {category} tile at ({x},{y})");
    }

    void PlaceHallwayTile(int x, int y)
    {
        // In the future: Instantiate the hallway prefab here
        // For now: Debug.Log($"Would place hallway tile at ({x},{y})");
    }

    // Use these in your room/hallway generation:
    void PrepareRoomTiles()
    {
        foreach (RoomData room in currentLayout.rooms)
        {
            for (int x = room.position.x; x < room.position.x + room.size.x; x++)
            {
                for (int y = room.position.y; y < room.position.y + room.size.y; y++)
                {
                    PlaceRoomTile(x, y, room.roomCategory);
                }
            }
        }
    }

    void PrepareHallwayTiles()
    {
        foreach (HallwayData hallway in currentLayout.hallways)
        {
            foreach (Vector2Int cell in hallway.path)
            {
                if (!IsInsideAnyRoom(cell))
                {
                    PlaceHallwayTile(cell.x, cell.y);
                }
            }
        }
    }

    void LimitExitConnections()
    {
        if (currentLayout == null || currentLayout.exitRoom == null) return;

        RoomData exitRoom = currentLayout.exitRoom;
        List<Vector2Int> connections = new List<Vector2Int>(exitRoom.connectedRooms);

        // If more than one connection, keep only the first
        if (connections.Count > 1)
        {
            Vector2Int keep = connections[0];
            exitRoom.connectedRooms = new List<Vector2Int> { keep };

            // Optionally, remove hallway cells leading to the other connections
            // (This step is optional and can be tricky if you want to erase the extra hallways visually)
        }
    }

    void AddExtraBranches(float branchChance = 0.25f, float maxBranchDistance = 8f)
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < currentLayout.rooms.Count; i++)
        {
            for (int j = i + 1; j < currentLayout.rooms.Count; j++)
            {
                var a = currentLayout.rooms[i];
                var b = currentLayout.rooms[j];

                // Don't connect start directly to exit
                if ((a.roomCategory == RoomCategory.Start && b.roomCategory == RoomCategory.Exit) ||
                    (a.roomCategory == RoomCategory.Exit && b.roomCategory == RoomCategory.Start))
                    continue;

                // Only connect if not already connected and within distance
                if (Vector2Int.Distance(a.position, b.position) < maxBranchDistance && rng.NextDouble() < branchChance)
                {
                    // Check if already connected (optional, depending on your data structure)
                    // If not, create a hallway
                    Vector2Int aCenter = new Vector2Int(a.position.x + a.size.x / 2, a.position.y + a.size.y / 2);
                    Vector2Int bCenter = new Vector2Int(b.position.x + b.size.x / 2, b.position.y + b.size.y / 2);

                    if (!PathExists(aCenter, bCenter))
                    {
                        CreateHallway(a, b);
                    }
                }
            }
        }
    }

    void PlaceDoors()
    {
        foreach (RoomData room in currentLayout.rooms)
        {
            bool doorPlaced = false;
            for (int x = room.position.x; x < room.position.x + room.size.x && !doorPlaced; x++)
            {
                for (int y = room.position.y; y < room.position.y + room.size.y && !doorPlaced; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    foreach (Vector2Int dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                    {
                        Vector2Int neighbor = cell + dir;
                        if (!IsInsideAnyRoom(neighbor) && IsInAnyHallway(neighbor))
                        {
                            // Place door at the boundary
                            Vector3 roomPos = new Vector3(cell.x * cellSize + cellSize * 0.5f, cellSize * 0.5f, cell.y * cellSize + cellSize * 0.5f);
                            Vector3 hallPos = new Vector3(neighbor.x * cellSize + cellSize * 0.5f, cellSize * 0.5f, neighbor.y * cellSize + cellSize * 0.5f);
                            Vector3 doorPos = (roomPos + hallPos) / 2f;
                            // Just store positions, do NOT draw Gizmos here
                            room.wallPositions.Add(cell);
                            room.doorPositions.Add(cell);
                            doorPlaced = true; // Only one door per room-hallway connection
                break;
                        }
                    }
                }
            }
        }
    }

    bool IsInAnyHallway(Vector2Int point)
    {
        foreach (HallwayData hallway in currentLayout.hallways)
        {
            if (hallway.path.Contains(point))
            {
                return true;
            }
        }
        return false;
    }

    void PlaceDoor(Vector2Int cell, Vector2Int dir)
    {
        // Implementation of PlaceDoor method
        // This is a placeholder and should be replaced with the actual implementation
        Debug.Log($"Placing door at {cell} in direction {dir}");
    }

    bool CanAddEntrance(RoomData room)
    {
        return room.doorPositions.Count < 2; // or connectedRooms.Count < 2
    }

    void CleanUpHallways()
    {
        foreach (RoomData room in currentLayout.rooms)
        {
            // Find all hallways connected to this room
            var connectedHallways = currentLayout.hallways
                .Where(h => h.startRoom == room.position || h.endRoom == room.position)
                    .ToList();

            if (connectedHallways.Count > 1 && room.doorPositions.Count == 1)
            {
                // Keep only the first hallway, remove the rest
                for (int i = 1; i < connectedHallways.Count; i++)
                {
                    currentLayout.hallways.Remove(connectedHallways[i]);
                }
            }
        }
    }

    void AssignTreasureRooms()
    {
        foreach (RoomData room in currentLayout.rooms)
        {
            // Skip start and exit rooms
            if (room.roomCategory == RoomCategory.Start || room.roomCategory == RoomCategory.Exit)
                continue;

            // Count connections (doors or connectedRooms)
            int connections = room.doorPositions.Count; // or room.connectedRooms.Count

            if (connections == 1)
            {
                // Assign as treasure room (if you want a certain % of dead ends to be treasure)
                if (Random.value < 0.5f) // 50% chance, tweak as desired
                    room.roomCategory = RoomCategory.Treasure;
            }
            else if (room.roomCategory == RoomCategory.Treasure)
            {
                // If not a dead end, demote to combat or filler
                room.roomCategory = RoomCategory.Combat;
            }
        }
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < gridSize && next.y >= 0 && next.y < gridSize &&
                    !cameFrom.ContainsKey(next))
                {
                    // Allow start/end points even if occupied, otherwise check if unoccupied
                    bool canTraverse = (next == start || next == end) || (!occupiedGrid[next.x, next.y] && !IsInsideAnyRoom(next));
                    
                    if (canTraverse)
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                    }
                }
            }
        }

        // Reconstruct path
        List<Vector2Int> path = new List<Vector2Int>();
        if (!cameFrom.ContainsKey(end)) 
        {
            Debug.LogWarning($"No path found from {start} to {end}");
            return path; // No path found
        }
        
        for (Vector2Int at = end; at != start; at = cameFrom[at])
            path.Add(at);
        path.Add(start);
        path.Reverse();
        return path;
    }

    bool PathExists(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) return true;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < gridSize && next.y >= 0 && next.y < gridSize &&
                    !visited.Contains(next))
                {
                    // Allow start/end points, or check if it's a hallway or unoccupied
                    bool canTraverse = (next == start || next == end) || 
                                     IsInAnyHallway(next) || 
                                     !occupiedGrid[next.x, next.y];
                    
                    if (canTraverse)
                    {
                        queue.Enqueue(next);
                        visited.Add(next);
                    }
                }
            }
        }
        return false;
    }

    void DebugRoomConnectivity()
    {
        Debug.Log($"Total rooms: {currentLayout.rooms.Count}");
        Debug.Log($"Total hallways: {currentLayout.hallways.Count}");
        
        foreach (RoomData room in currentLayout.rooms)
        {
            Debug.Log($"{room.roomCategory} room at {room.position} has {room.doorPositions.Count} doors");
        }
    }

    Vector2Int GetDoorPositionBetween(Vector2Int roomAEdge, Vector2Int roomBEdge)
    {
        // Calculate the midpoint between the two room edges
        int doorX = (roomAEdge.x + roomBEdge.x) / 2;
        int doorY = (roomAEdge.y + roomBEdge.y) / 2;
        
        return new Vector2Int(doorX, doorY);
    }

    Vector2Int GetHallwayEntryPoint(RoomData room, List<Vector2Int> hallwayPath)
    {
        // Find the first cell in the hallway path that's adjacent to the room
        foreach (Vector2Int cell in hallwayPath)
        {
            // Check if this cell is adjacent to the room
            if (IsAdjacentToRoom(cell, room))
            {
                return cell;
            }
        }
        
        // Fallback: return the first cell of the hallway
        return hallwayPath.Count > 0 ? hallwayPath[0] : Vector2Int.zero;
    }

    bool IsAdjacentToRoom(Vector2Int cell, RoomData room)
    {
        // Check if the cell is adjacent to any edge of the room
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInsideRoom(neighbor, room))
            {
                return true;
            }
        }
        return false;
    }

    bool IsInsideRoom(Vector2Int cell, RoomData room)
    {
        return cell.x >= room.position.x && cell.x < room.position.x + room.size.x &&
               cell.y >= room.position.y && cell.y < room.position.y + room.size.y;
    }

    List<Vector2Int> CreateLShapedHallway(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        // First, move horizontally
        while (current.x != end.x)
        {
            current.x += (end.x > current.x) ? 1 : -1;
            path.Add(current);
        }
        // Then, move vertically
        while (current.y != end.y)
        {
            current.y += (end.y > current.y) ? 1 : -1;
            path.Add(current);
        }
        return path;
    }

    List<Vector2Int> GetRoomEdgeCells(RoomData room, Vector2Int dir)
    {
        List<Vector2Int> edgeCells = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int cell in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInsideRoom(neighbor, room))
            {
                edgeCells.Add(cell);
            }
        }
        return edgeCells;
    }

    Vector3 ToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize + cellSize * 0.5f, 0, cell.y * cellSize + cellSize * 0.5f);
    }

    List<Vector2Int> GetRoomWallPositions(RoomData room)
    {
        List<Vector2Int> wallPositions = new List<Vector2Int>();

        for (int x = room.position.x; x < room.position.x + room.size.x; x++)
        {
            Vector2Int top = new Vector2Int(x, room.position.y + room.size.y - 1);
            Vector2Int bottom = new Vector2Int(x, room.position.y);

            if (!room.doorPositions.Contains(top)) wallPositions.Add(top);
            if (!room.doorPositions.Contains(bottom)) wallPositions.Add(bottom);

            for (int y = room.position.y; y < room.position.y + room.size.y; y++)
            {
                Vector2Int left = new Vector2Int(room.position.x, y);
                Vector2Int right = new Vector2Int(room.position.x + room.size.x - 1, y);

                if (!room.doorPositions.Contains(left)) wallPositions.Add(left);
                if (!room.doorPositions.Contains(right)) wallPositions.Add(right);
            }
        }

        return wallPositions;
    }

    void GenerateRoomWalls(RoomData room)
    {
        room.wallPositions.Clear();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Top and bottom edges
        for (int x = room.position.x; x < room.position.x + room.size.x; x++)
        {
            // Top edge
            Vector2Int top = new Vector2Int(x, room.position.y + room.size.y - 1);
            Vector2Int topNeighbor = top + Vector2Int.up;
            if (!IsInAnyHallway(topNeighbor) && !IsInsideAnyRoom(topNeighbor))
                room.wallPositions.Add(top);

            // Bottom edge
            Vector2Int bottom = new Vector2Int(x, room.position.y);
            Vector2Int bottomNeighbor = bottom + Vector2Int.down;
            if (!IsInAnyHallway(bottomNeighbor) && !IsInsideAnyRoom(bottomNeighbor))
                room.wallPositions.Add(bottom);
        }

        // Left and right edges
        for (int y = room.position.y; y < room.position.y + room.size.y; y++)
        {
            // Left edge
            Vector2Int left = new Vector2Int(room.position.x, y);
            Vector2Int leftNeighbor = left + Vector2Int.left;
            if (!IsInAnyHallway(leftNeighbor) && !IsInsideAnyRoom(leftNeighbor))
                room.wallPositions.Add(left);

            // Right edge
            Vector2Int right = new Vector2Int(room.position.x + room.size.x - 1, y);
            Vector2Int rightNeighbor = right + Vector2Int.right;
            if (!IsInAnyHallway(rightNeighbor) && !IsInsideAnyRoom(rightNeighbor))
                room.wallPositions.Add(right);
        }
    }

    Vector2Int NormalizeDirection(Vector2Int dir)
    {
        return new Vector2Int(
            dir.x == 0 ? 0 : (dir.x > 0 ? 1 : -1),
            dir.y == 0 ? 0 : (dir.y > 0 ? 1 : -1)
        );
    }

    bool IsOpenCell(Vector2Int cell)
    {
        return IsInsideAnyRoom(cell) || IsInAnyHallway(cell);
    }
}




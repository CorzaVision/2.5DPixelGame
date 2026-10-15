using System.Collections.Generic;
using UnityEngine;

public class GeneratorPrototype : MonoBehaviour
{
    [Header("Dungeon Layout Settings")]
    [SerializeField] private int gridSize = 12; // Grid size (12x12)
    [SerializeField] private float cellSize = 1f; // Cell size for grid (1 unit spacing)
    [SerializeField] private int gridBoundsMargin = 1; // Margin from grid edges for room placement (1 cell = minimal margin)
    
    [Header("Room Generation Settings")]
    [SerializeField] private int minRooms = 8; // Minimum number of rooms to generate
    [SerializeField] private int maxRooms = 15; // Maximum number of rooms to generate
    [SerializeField] private int maxAttempts = 100; // Maximum attempts to place a room
    
    [Header("Performance Optimization")]
    [SerializeField] private bool useMeshCombining = true; // Combine terrain cubes into single mesh
    [SerializeField] private bool useStaticBatching = true; // Use Unity's static batching
    
    [Header("Randomization")]
    [SerializeField] private int randomSeed = 0; // Random seed for dungeon generation

    [Header("Corridor Generation")]
    [SerializeField] private bool generateCorridors = true;
    [SerializeField] private int maxCorridorsPerRoom = 2; // Maximum corridors per room (increased for T-sections)
    [SerializeField] private bool allowTSections = true; // Allow T-section rooms
    [SerializeField] private float tSectionChance = 0.3f; // Chance for a room to become a T-section
    
    public enum RoomType
    {
        Start,
        End,
        Combat,
        Treasure,
        Boss,
        MiniBoss,
        Hallway
    }

    [Header("Prefabs")]
    public GameObject baseTerrainCube; // Base terrain cube
    public GameObject floorPlane; // Floor plane for rooms

    public enum TileType
    {
        Cube,
        Floor,
    }

    // Grid data
    private TileType[,] tileGrid;

    // Room data
    private Vector2Int startRoomPos;
    private Vector2Int endRoomPos;
    private Dictionary<Vector2Int, RoomType> roomPositions = new Dictionary<Vector2Int, RoomType>();
    private Dictionary<Vector2Int, int> roomSizes = new Dictionary<Vector2Int, int>();
    private List<Vector2Int> placedRooms = new List<Vector2Int>(); // Track placed room positions

    // Performance optimization
    private List<GameObject> terrainCubes = new List<GameObject>(); // Track individual cubes for combining
    private GameObject combinedTerrainMesh; // Combined terrain mesh object
    
    private Dictionary<Vector2Int, List<Vector2Int>> roomConnections = new Dictionary<Vector2Int, List<Vector2Int>>(); // Track room connections
    private List<Vector2Int> corridorPositions = new List<Vector2Int>(); // Track corridor positions

    private const int START_END_ROOM_SIZE = 2; // Start and end rooms are always 2x2
    private const int REGULAR_ROOM_SIZE = 2; // Regular rooms are 2x2

    void Start()
    {
        if (this.transform == null)
        {
            Debug.LogError("Transform is null - cannot generate dungeon");
            return;
        }

        Debug.Log($"Starting organic dungeon generation - Grid: {gridSize}x{gridSize}, Cell Size: {cellSize}");
        
        // Initialize random seed
        if (randomSeed == 0)
        {
            randomSeed = System.DateTime.Now.Millisecond;
        }
        Random.InitState(randomSeed);
        Debug.Log($"Using random seed: {randomSeed}");
        
        // Initialize grid
        tileGrid = new TileType[gridSize, gridSize];

        // Generate organic dungeon layout
        GenerateOrganicDungeon();

        // Generate base terrain (cubes)
        GenerateBaseTerrain();
    }
    
    [ContextMenu("Generate New Dungeon")]
    public void GenerateNewDungeon()
    {
        // Clear existing rooms
        roomPositions.Clear();
        roomSizes.Clear();
        placedRooms.Clear();
        
        // Clear existing terrain
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Set new random seed
        randomSeed = System.DateTime.Now.Millisecond;
        Random.InitState(randomSeed);
        Debug.Log($"Generated new dungeon with seed: {randomSeed}");
        
        // Initialize grid
        tileGrid = new TileType[gridSize, gridSize];
        
        // Generate organic dungeon layout
        GenerateOrganicDungeon();
        
        // Generate terrain
        GenerateBaseTerrain();
    }

    private void GenerateOrganicDungeon()
    {
        // Clear previous data
        roomPositions.Clear();
        roomSizes.Clear();
        placedRooms.Clear();
        roomConnections.Clear();
        corridorPositions.Clear();
        
        Debug.Log("=== GENERATING ORGANIC DUNGEON LAYOUT ===");
        
        // Place start room at a random edge position
        PlaceStartRoom();
        
        // Place end room at opposite side
        PlaceEndRoom();
        
        // Generate the main room path
        GenerateRoomPath();
        
        // Generate additional rooms
        GenerateAdditionalRooms();
        
        // Generate corridors between rooms
        if (generateCorridors)
        {
            GenerateCorridors();
        }
        
        // Create room connections (for visualization)
        CreateRoomConnections();
        
        // Generate base terrain
        GenerateBaseTerrain();
        
        Debug.Log($"Generated dungeon with {placedRooms.Count} rooms and {corridorPositions.Count} corridors");
    }
    
    private void GenerateRoomPath()
    {
        Debug.Log("=== GENERATING ROOM PATH FROM START TO END ===");
        
        // Create a path of rooms from start to end
        List<Vector2Int> pathPositions = CreatePathPositions();
        
        foreach (Vector2Int pathPos in pathPositions)
        {
            if (!roomPositions.ContainsKey(pathPos))
            {
                RoomType roomType = GetRandomRoomType();
                PlaceRoom(pathPos, roomType, GetRoomTypeName(roomType), REGULAR_ROOM_SIZE);
                placedRooms.Add(pathPos);
                Debug.Log($"Placed {roomType} room at {pathPos} on path");
            }
        }
    }
    
    private List<Vector2Int> CreatePathPositions()
    {
        List<Vector2Int> path = new List<Vector2Int>();
        
        // Calculate direction from start to end
        Vector2Int direction = endRoomPos - startRoomPos;
        int distance = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        
        Debug.Log($"Creating path from {startRoomPos} to {endRoomPos}, distance: {distance}");
        
        // Create a winding path with some randomness
        Vector2Int currentPos = startRoomPos;
        path.Add(currentPos);
        
        int maxSteps = distance * 2; // Allow for some winding
        int steps = 0;
        
        while (currentPos != endRoomPos && steps < maxSteps)
        {
            steps++;
            
            // Calculate direction to end
            Vector2Int toEnd = endRoomPos - currentPos;
            
            // Add some randomness to the path
            Vector2Int nextStep;
            if (Random.Range(0f, 1f) < 0.7f) // 70% chance to move toward end
            {
                // Move toward end
                nextStep = currentPos + GetDirectionStep(toEnd);
            }
            else
            {
                // Random step - only use valid directions
                Vector2Int[] validDirections = {
                    new Vector2Int(1, 0), new Vector2Int(-1, 0),
                    new Vector2Int(0, 1), new Vector2Int(0, -1)
                };
                Vector2Int randomDir = validDirections[Random.Range(0, validDirections.Length)];
                nextStep = currentPos + randomDir * (REGULAR_ROOM_SIZE + 1);
            }
            
            // Validate the next position
            if (IsValidRoomPosition(nextStep, REGULAR_ROOM_SIZE))
            {
                currentPos = nextStep;
                path.Add(currentPos);
                Debug.Log($"Path step {steps}: {currentPos}");
            }
            else
            {
                // Try to find a valid position nearby
                Vector2Int? validPos = FindValidPositionNearby(currentPos);
                if (validPos.HasValue)
                {
                    currentPos = validPos.Value;
                    path.Add(currentPos);
                    Debug.Log($"Path step {steps} (adjusted): {currentPos}");
                }
                else
                {
                    Debug.LogWarning($"Could not find valid position near {currentPos}, stopping path");
                    break;
                }
            }
        }
        
        // Ensure end room is in the path
        if (!path.Contains(endRoomPos))
        {
            path.Add(endRoomPos);
        }
        
        Debug.Log($"Created path with {path.Count} positions: {string.Join(" -> ", path)}");
        return path;
    }
    
    private Vector2Int GetDirectionStep(Vector2Int direction)
    {
        // Convert direction to a single step with proper spacing
        Vector2Int step = Vector2Int.zero;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            step.x = direction.x > 0 ? REGULAR_ROOM_SIZE + 1 : -(REGULAR_ROOM_SIZE + 1);
            }
            else
            {
            step.y = direction.y > 0 ? REGULAR_ROOM_SIZE + 1 : -(REGULAR_ROOM_SIZE + 1);
        }
        
        return step;
    }
    
    private Vector2Int? FindValidPositionNearby(Vector2Int pos)
    {
        // Try positions around the current position
        Vector2Int[] offsets = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1),
            new Vector2Int(1, -1), new Vector2Int(-1, -1)
        };
        
        foreach (Vector2Int offset in offsets)
        {
            Vector2Int testPos = pos + offset * (REGULAR_ROOM_SIZE + 1);
            if (IsValidRoomPosition(testPos, REGULAR_ROOM_SIZE))
            {
                return testPos;
            }
        }
        
        return null;
    }
    
    private void PlaceStartRoom()
    {
        startRoomPos = GetRandomEdgePosition();
        
        // Ensure the position is within bounds
        if (IsValidRoomPosition(startRoomPos, START_END_ROOM_SIZE))
        {
            roomPositions[startRoomPos] = RoomType.Start;
            roomSizes[startRoomPos] = START_END_ROOM_SIZE;
            placedRooms.Add(startRoomPos);
            
            PlaceRoom(startRoomPos, RoomType.Start, "Start", START_END_ROOM_SIZE);
            Debug.Log($"Placed Start room at {startRoomPos}");
        }
        else
        {
            Debug.LogError($"Invalid start room position: {startRoomPos}");
            // Fallback to a safe position
            startRoomPos = new Vector2Int(gridBoundsMargin, gridBoundsMargin);
            roomPositions[startRoomPos] = RoomType.Start;
            roomSizes[startRoomPos] = START_END_ROOM_SIZE;
            placedRooms.Add(startRoomPos);
            
            PlaceRoom(startRoomPos, RoomType.Start, "Start", START_END_ROOM_SIZE);
            Debug.Log($"Placed Start room at fallback position {startRoomPos}");
        }
    }
    
    private void PlaceEndRoom()
    {
        endRoomPos = GetOppositeEdgePosition(startRoomPos);
        
        // Ensure the position is within bounds
        if (IsValidRoomPosition(endRoomPos, START_END_ROOM_SIZE))
        {
            roomPositions[endRoomPos] = RoomType.End;
            roomSizes[endRoomPos] = START_END_ROOM_SIZE;
            placedRooms.Add(endRoomPos);
            
            PlaceRoom(endRoomPos, RoomType.End, "End", START_END_ROOM_SIZE);
            Debug.Log($"Placed End room at {endRoomPos}");
        }
        else
        {
            Debug.LogError($"Invalid end room position: {endRoomPos}");
            // Fallback to a safe position
            endRoomPos = new Vector2Int(gridSize - gridBoundsMargin - START_END_ROOM_SIZE, gridSize - gridBoundsMargin - START_END_ROOM_SIZE);
            roomPositions[endRoomPos] = RoomType.End;
            roomSizes[endRoomPos] = START_END_ROOM_SIZE;
            placedRooms.Add(endRoomPos);
            
            PlaceRoom(endRoomPos, RoomType.End, "End", START_END_ROOM_SIZE);
            Debug.Log($"Placed End room at fallback position {endRoomPos}");
        }
    }
    
    private void GenerateAdditionalRooms()
    {
        int attempts = 0;
        int roomsAdded = 0;
        int maxAdditionalRooms = maxRooms - placedRooms.Count;
        
        while (roomsAdded < maxAdditionalRooms && attempts < maxAttempts)
        {
            Vector2Int? newRoomPos = TryPlaceAdjacentRoom();
            
            if (newRoomPos.HasValue)
            {
                RoomType roomType = GetRandomRoomType();
                int roomSize = REGULAR_ROOM_SIZE;
                
                roomPositions[newRoomPos.Value] = roomType;
                roomSizes[newRoomPos.Value] = roomSize;
                placedRooms.Add(newRoomPos.Value);
                
                PlaceRoom(newRoomPos.Value, roomType, $"Additional_{roomType}", roomSize);
                roomsAdded++;
                
                Debug.Log($"Added additional {roomType} room at {newRoomPos.Value}");
            }
            
            attempts++;
        }
        
        Debug.Log($"Added {roomsAdded} additional rooms after {attempts} attempts");
    }
    
    private void GenerateCorridors()
    {
        Debug.Log("=== GENERATING CORRIDORS ===");
        
        // Initialize room connections
        foreach (Vector2Int room in placedRooms)
        {
            roomConnections[room] = new List<Vector2Int>();
        }
        
        // Generate corridors for each room
        foreach (Vector2Int room in placedRooms)
        {
            GenerateCorridorsForRoom(room);
        }
        
        // Ensure connectivity from start to end
        EnsureStartToEndConnectivity();
        
        Debug.Log($"Generated {corridorPositions.Count} corridor tiles");
    }
    
    private void GenerateCorridorsForRoom(Vector2Int room)
    {
        List<Vector2Int> adjacentRooms = GetStrictlyAdjacentRooms(room);
        List<Vector2Int> availableConnections = new List<Vector2Int>();
        
        // Find rooms that aren't already connected
        foreach (Vector2Int adjacentRoom in adjacentRooms)
        {
            if (!roomConnections[room].Contains(adjacentRoom) && 
                !roomConnections[adjacentRoom].Contains(room))
            {
                availableConnections.Add(adjacentRoom);
            }
        }
        
        // Determine how many connections this room should have
        int targetConnections = 1; // Default: one connection per room
        
        // Check if this room should be a T-section
        if (allowTSections && availableConnections.Count >= 2)
        {
            // Path rooms (start/end) are more likely to be T-sections
            bool isPathRoom = (room == startRoomPos || room == endRoomPos);
            float chance = isPathRoom ? tSectionChance * 2f : tSectionChance;
            
            if (Random.Range(0f, 1f) < chance)
            {
                targetConnections = 2; // T-section: connects to 2 other rooms
                Debug.Log($"Room {room} becomes T-section with {availableConnections.Count} available connections");
            }
        }
        
        // Limit connections per room
        int maxConnections = Mathf.Min(maxCorridorsPerRoom, availableConnections.Count, targetConnections);
        
        for (int i = 0; i < maxConnections; i++)
        {
            if (availableConnections.Count == 0) break;
            
            // Pick a random available connection
            int randomIndex = Random.Range(0, availableConnections.Count);
            Vector2Int targetRoom = availableConnections[randomIndex];
            
            // Create corridor between rooms
            CreateCorridorBetweenRooms(room, targetRoom);
            
            // Mark as connected
            roomConnections[room].Add(targetRoom);
            roomConnections[targetRoom].Add(room);
            
            // Remove from available connections
            availableConnections.RemoveAt(randomIndex);
        }
        
        if (maxConnections > 1)
        {
            Debug.Log($"Room {room} connected to {maxConnections} other rooms (T-section)");
        }
    }
    
    private void CreateCorridorBetweenRooms(Vector2Int roomA, Vector2Int roomB)
    {
        Vector2Int corridorPoint = FindCorridorConnectionPoint(roomA, roomB);
        
        if (corridorPoint != Vector2Int.zero)
        {
            // Place corridor tile
            if (floorPlane != null) // Use the existing floorPlane prefab
            {
                Vector3 corridorPos = GridToWorld(corridorPoint);
                corridorPos.y = 0.1f; // Same height as floor tiles
                
                GameObject corridorTile = Instantiate(floorPlane, corridorPos, Quaternion.identity, this.transform);
                corridorTile.name = $"Corridor_{roomA}_{roomB}";
                
                // Scale the corridor to match cell size
                float corridorScale = cellSize / 10f;
                corridorTile.transform.localScale = new Vector3(corridorScale, 1f, corridorScale);
                
                Debug.Log($"Created corridor at {corridorPoint} between {roomA} and {roomB}");
            }
            else
            {
                Debug.LogWarning("Floor plane prefab is not assigned - cannot place corridor tiles");
            }
            
            corridorPositions.Add(corridorPoint);
            Debug.Log($"Added corridor position: {corridorPoint}");
        }
        else
        {
            Debug.LogWarning($"Could not find corridor connection point between {roomA} and {roomB}");
        }
    }
    
    private void EnsureStartToEndConnectivity()
    {
        // Check if start and end are connected
        if (!AreRoomsConnected(startRoomPos, endRoomPos))
        {
            Debug.LogWarning("Start and end rooms not connected - creating direct path");
            
            // Find a path from start to end
            List<Vector2Int> path = FindPathThroughRooms(startRoomPos, endRoomPos);
            
            if (path != null && path.Count > 1)
            {
                // Create corridors along the path
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector2Int roomA = path[i];
                    Vector2Int roomB = path[i + 1];
                    
                    if (!roomConnections[roomA].Contains(roomB))
                    {
                        CreateCorridorBetweenRooms(roomA, roomB);
                        roomConnections[roomA].Add(roomB);
                        roomConnections[roomB].Add(roomA);
                    }
                }
            }
        }
    }
    
    private bool AreRoomsConnected(Vector2Int roomA, Vector2Int roomB)
    {
        // Simple BFS to check if rooms are connected
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(roomA);
        visited.Add(roomA);
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current == roomB)
                return true;
            
            foreach (Vector2Int connectedRoom in roomConnections[current])
            {
                if (!visited.Contains(connectedRoom))
                {
                    visited.Add(connectedRoom);
                    queue.Enqueue(connectedRoom);
                }
            }
        }
        
        return false;
    }
    
    private Vector2Int? TryPlaceAdjacentRoom()
    {
        // Get a random existing room to build from
        if (placedRooms.Count == 0) return null;
        
        Vector2Int baseRoom = placedRooms[Random.Range(0, placedRooms.Count)];
        int baseRoomSize = roomSizes[baseRoom];
        
        // Try different directions around the base room
        Vector2Int[] directions = {
            new Vector2Int(1, 0),   // Right
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(1, 1),   // Diagonal
            new Vector2Int(-1, 1),  // Diagonal
            new Vector2Int(1, -1),  // Diagonal
            new Vector2Int(-1, -1)  // Diagonal
        };
        
        // Shuffle directions for randomness
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = Random.Range(i, directions.Length);
            Vector2Int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
        
        foreach (Vector2Int direction in directions)
        {
            // Calculate potential room position with 1-cell spacing
            Vector2Int potentialPos = baseRoom + direction * (baseRoomSize + 1);
            
            // Check if this position is valid and available
            if (IsValidRoomPosition(potentialPos, REGULAR_ROOM_SIZE))
            {
                return potentialPos;
            }
        }
        
        return null;
    }
    
    private bool IsValidRoomPosition(Vector2Int pos, int roomSize)
    {
        // Check bounds - ensure room fits within grid with margin
        if (pos.x < gridBoundsMargin || pos.y < gridBoundsMargin ||
            pos.x + roomSize > gridSize - gridBoundsMargin ||
            pos.y + roomSize > gridSize - gridBoundsMargin)
        {
            Debug.Log($"Position {pos} with size {roomSize} is outside bounds (grid: {gridSize}, margin: {gridBoundsMargin})");
            return false;
        }
        
        // Check if position overlaps with existing rooms
        foreach (Vector2Int existingRoom in placedRooms)
        {
            int existingSize = roomSizes[existingRoom];
            
            // Check for overlap (including 1-cell spacing)
            bool overlaps = !(pos.x + roomSize + 1 <= existingRoom.x || 
                            existingRoom.x + existingSize + 1 <= pos.x ||
                            pos.y + roomSize + 1 <= existingRoom.y || 
                            existingRoom.y + existingSize + 1 <= pos.y);
            
            if (overlaps)
            {
                Debug.Log($"Position {pos} overlaps with existing room {existingRoom} (size: {existingSize})");
                return false;
            }
        }
        
        return true;
    }
    
    private Vector2Int GetRandomEdgePosition()
    {
        // Get a random position on the edge of the grid
        int side = Random.Range(0, 4); // 0: top, 1: right, 2: bottom, 3: left
        
        switch (side)
        {
            case 0: // Top edge
                return new Vector2Int(Random.Range(gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE), 
                                    gridBoundsMargin);
            case 1: // Right edge
                return new Vector2Int(gridSize - gridBoundsMargin - START_END_ROOM_SIZE, 
                                    Random.Range(gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE));
            case 2: // Bottom edge
                return new Vector2Int(Random.Range(gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE), 
                                    gridSize - gridBoundsMargin - START_END_ROOM_SIZE);
            case 3: // Left edge
                return new Vector2Int(gridBoundsMargin, 
                                    Random.Range(gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE));
            default:
                return new Vector2Int(gridBoundsMargin, gridBoundsMargin);
        }
    }
    
    private Vector2Int GetOppositeEdgePosition(Vector2Int startPos)
    {
        // Find a position on the opposite side from the start room
        Vector2Int center = new Vector2Int(gridSize / 2, gridSize / 2);
        Vector2 direction = ((Vector2)(center - startPos)).normalized;
        
        // Calculate opposite position
        Vector2Int oppositePos = center + Vector2Int.RoundToInt(direction * (gridSize / 2 - gridBoundsMargin - START_END_ROOM_SIZE));
        
        // Clamp to valid bounds
        oppositePos.x = Mathf.Clamp(oppositePos.x, gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE);
        oppositePos.y = Mathf.Clamp(oppositePos.y, gridBoundsMargin, gridSize - gridBoundsMargin - START_END_ROOM_SIZE);
        
        return oppositePos;
    }
    
    private void CreateRoomConnections()
    {
        Debug.Log("=== ROOM CONNECTIONS ===");
        
        // Rooms are already connected by the path generation
        // Just verify we have a valid path
        bool hasValidPath = CheckPathValidity();
        
        if (hasValidPath)
        {
            Debug.Log("Room path is valid - start to end connection confirmed");
        }
        else
        {
            Debug.LogWarning("Room path may not be fully connected");
        }
    }
    
    private bool CheckPathValidity()
    {
        // Simple check: ensure we can reach end from start through placed rooms
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
        
        toVisit.Enqueue(startRoomPos);
        visited.Add(startRoomPos);
        
        while (toVisit.Count > 0)
        {
            Vector2Int current = toVisit.Dequeue();
            
            if (current == endRoomPos)
            {
                return true; // Found path to end
            }
            
            // Check adjacent rooms
            foreach (Vector2Int room in placedRooms)
            {
                if (!visited.Contains(room) && AreRoomsAdjacent(current, room, roomSizes[current]))
                {
                    visited.Add(room);
                    toVisit.Enqueue(room);
                }
            }
        }
        
        return false; // No path found
    }
    
    private void AdjustRoomPlacementForConnectivity()
    {
        // This method is no longer needed - rooms are placed in a path
        Debug.Log("Room placement is handled by path generation");
    }
    
    private List<Vector2Int> FindPathFromStartToEnd()
    {
        // This method is no longer needed - we create the path by placing rooms
        return new List<Vector2Int>();
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        // This method is no longer needed - we use IsValidRoomPosition
        return IsValidRoomPosition(pos, REGULAR_ROOM_SIZE);
    }
    
    private void CreateHallwayBetweenRooms(Vector2Int roomA, Vector2Int roomB)
    {
        // This method is no longer needed - we don't create hallways
        Debug.Log($"Rooms at {roomA} and {roomB} are adjacent but no hallway needed");
    }
    
    private Vector2Int GetHallwayPosition(Vector2Int roomA, Vector2Int roomB)
    {
        // This method is no longer needed - we don't create hallways
        return Vector2Int.zero;
    }
    
    private void CreateHallwayBetweenPoints(Vector2Int pointA, Vector2Int pointB)
    {
        // This method is no longer needed - we don't create hallways
        Debug.Log($"Points {pointA} and {pointB} are connected without hallways");
    }
    
    private void CreateAdditionalRoomConnections()
    {
        // This method is no longer needed - we don't create additional hallways
        Debug.Log("No additional hallways needed - rooms are connected directly");
    }
    
    private RoomType GetRandomRoomType()
    {
        int random = Random.Range(0, 100);
        if (random < 85) // 85% chance for Combat rooms
        {
            return RoomType.Combat;
        }
        else // 15% chance for Treasure rooms
        {
            return RoomType.Treasure;
        }
    }

    // Helper to check if two rooms are adjacent (including diagonals)
    private bool AreRoomsAdjacent(Vector2Int a, Vector2Int b, int size)
    {
        for (int ax = 0; ax < size; ax++)
        {
            for (int ay = 0; ay < size; ay++)
            {
                Vector2Int cellA = a + new Vector2Int(ax, ay);
                for (int bx = -1; bx <= size; bx++)
                {
                    for (int by = -1; by <= size; by++)
                    {
                        Vector2Int cellB = b + new Vector2Int(bx, by);
                        if (cellA == cellB)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    private void PlaceRoom(Vector2Int pos, RoomType roomType, string roomName, int roomSizeToUse)
    {
        // Hierarchy safety check
        if (this.transform == null)
        {
            Debug.LogError($"Cannot place {roomName} room - generator transform is null!");
            return;
        }

        if (floorPlane == null)
        {
            Debug.LogError($"Floor plane prefab is not assigned - cannot place {roomName} room!");
            return;
        }

        // Track room data
        roomPositions[pos] = roomType;
        roomSizes[pos] = roomSizeToUse;

        // Place floor planes for the entire room area - PERFECT GRID ALIGNMENT
        for (int x = 0; x < roomSizeToUse; x++)
        {
            for (int y = 0; y < roomSizeToUse; y++)
            {
                Vector2Int tilePos = pos + new Vector2Int(x, y);
                Vector3 worldPos = GridToWorld(tilePos);
                worldPos.y = 0.1f; // Slightly above ground
                
                GameObject floorTile = Instantiate(floorPlane, worldPos, Quaternion.identity, this.transform);
                floorTile.name = $"{roomName}_Floor_{x}_{y}";
                
                // Scale the floor plane to match cell size (Unity planes are 10x10 by default)
                float planeScale = cellSize / 10f; // Scale down from 10x10 to cellSize
                floorTile.transform.localScale = new Vector3(planeScale, 1f, planeScale);
                
                // Debug: Log the actual position vs expected position
                Debug.Log($"Placed {roomName} floor tile at grid {tilePos} -> world {worldPos}, scale {planeScale}");
            }
        }
        
        Debug.Log($"Placed {roomName} room at {pos} with size {roomSizeToUse} - Grid aligned");
    }

    private void GenerateBaseTerrain()
    {
        // Hierarchy safety check
        if (this.transform == null)
        {
            Debug.LogError("Cannot generate base terrain - generator transform is null!");
            return;
        }

        if (baseTerrainCube == null)
        {
            Debug.LogError("Base terrain cube prefab is not assigned!");
            return;
        }

        Debug.Log("Starting base terrain generation");
        
        // Clear previous terrain
        terrainCubes.Clear();
        if (combinedTerrainMesh != null)
        {
            DestroyImmediate(combinedTerrainMesh);
        }
        
        // Fill entire grid with base terrain cubes, but skip room areas
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                
                // Check if this position is in any room area
                bool isInRoomArea = false;
                foreach (var roomEntry in roomPositions)
                {
                    Vector2Int roomPos = roomEntry.Key;
                    int roomSize = roomSizes[roomPos];
                    
                    bool isInThisRoom = x >= roomPos.x && x < roomPos.x + roomSize && 
                                       y >= roomPos.y && y < roomPos.y + roomSize;
                    if (isInThisRoom)
                    {
                        isInRoomArea = true;
                        break;
                    }
                }
                
                // Only place cube if not in a room area
                if (!isInRoomArea)
                {
                    Vector3 worldPos = GridToWorld(currentPos);
                    worldPos.y = 4.0f; // Set to 4.0f as requested
                    
                    GameObject cube = Instantiate(baseTerrainCube, worldPos, Quaternion.identity, this.transform);
                    cube.name = $"TerrainCube_{x}_{y}";
                    
                    // Scale cube to match cell size
                    cube.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    
                    // Debug: Log the actual position vs expected position
                    Debug.Log($"Placed terrain cube at grid {currentPos} -> world {worldPos}, scale {cellSize}");
                    
                    // Enable static batching if requested
                    if (useStaticBatching)
                    {
                        cube.isStatic = true;
                    }
                    
                    terrainCubes.Add(cube); // Add to list for combining
                }
            }
        }

        // Combine meshes if requested
        if (useMeshCombining && terrainCubes.Count > 0)
        {
            CombineTerrainMeshes();
        }

        Debug.Log($"Base terrain generation complete - {terrainCubes.Count} cubes placed");
    }
    
    private void CombineTerrainMeshes()
    {
        Debug.Log("Combining terrain meshes for performance optimization...");
        
        // Create combined mesh object
        combinedTerrainMesh = new GameObject("CombinedTerrainMesh");
        combinedTerrainMesh.transform.SetParent(this.transform);
        
        // Get mesh filters and renderers from all terrain cubes
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        
        foreach (GameObject cube in terrainCubes)
        {
            MeshFilter meshFilter = cube.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                CombineInstance combine = new CombineInstance();
                combine.mesh = meshFilter.sharedMesh;
                combine.transform = cube.transform.localToWorldMatrix;
                combineInstances.Add(combine);
            }
        }
        
        if (combineInstances.Count > 0)
        {
            // Create combined mesh
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
            
            // Add mesh components to combined object
            MeshFilter combinedMeshFilter = combinedTerrainMesh.AddComponent<MeshFilter>();
            MeshRenderer combinedMeshRenderer = combinedTerrainMesh.AddComponent<MeshRenderer>();
            
            combinedMeshFilter.sharedMesh = combinedMesh;
            
            // Use the same material as the original cubes
            if (terrainCubes.Count > 0)
            {
                MeshRenderer originalRenderer = terrainCubes[0].GetComponent<MeshRenderer>();
                if (originalRenderer != null)
                {
                    combinedMeshRenderer.sharedMaterial = originalRenderer.sharedMaterial;
                }
            }
            
            // Disable individual cubes (they're now part of the combined mesh)
            foreach (GameObject cube in terrainCubes)
            {
                cube.SetActive(false);
            }
            
            Debug.Log($"Successfully combined {combineInstances.Count} terrain cubes into single mesh");
        }
        else
        {
            Debug.LogWarning("No valid meshes found for combining");
        }
    }
    
    [ContextMenu("Regenerate Terrain")]
    public void RegenerateTerrain()
    {
        // Clear existing terrain
        terrainCubes.Clear();
        if (combinedTerrainMesh != null)
        {
            DestroyImmediate(combinedTerrainMesh);
        }
        
        // Remove all terrain cubes
        List<Transform> childrenToRemove = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("TerrainCube_") || child.name == "CombinedTerrainMesh")
            {
                childrenToRemove.Add(child);
            }
        }
        
        foreach (Transform child in childrenToRemove)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Regenerate terrain
        GenerateBaseTerrain();
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        // PERFECT GRID ALIGNMENT - each grid cell is exactly cellSize units
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    public TileType GetTileTypeAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize)
            return TileType.Cube;
        return tileGrid[pos.x, pos.y];
    }

    public void SetTileTypeAt(Vector2Int pos, TileType type)
    {
        if (pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize)
        tileGrid[pos.x, pos.y] = type;
    }

    public RoomType GetRoomTypeAt(Vector2Int pos)
    {
        return roomPositions.ContainsKey(pos) ? roomPositions[pos] : RoomType.Combat;
    }

    private int GetRoomSizeAt(Vector2Int roomPos)
    {
        return roomSizes.ContainsKey(roomPos) ? roomSizes[roomPos] : 0;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw grid overlay
        DrawGridOverlay(cellSize);
        
        // Draw room visualizations
        DrawRoomVisualizations(cellSize);
        
        // Draw room connections
        DrawRoomConnections(cellSize);
    }

    private void DrawGridOverlay(float cellSize)
    {
        Gizmos.color = Color.white;
        
        // Draw grid lines that match the actual terrain cubes and floor planes
        // Each grid cell corresponds to exactly one terrain cube or floor plane
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, 0, gridSize * cellSize);
            Gizmos.DrawLine(start, end);
        }
        
        for (int y = 0; y <= gridSize; y++)
        {
            Vector3 start = new Vector3(0, 0, y * cellSize);
            Vector3 end = new Vector3(gridSize * cellSize, 0, y * cellSize);
            Gizmos.DrawLine(start, end);
    }

        // Draw cell center indicators that match the actual terrain
        Gizmos.color = Color.gray;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorld(gridPos);
                
                // Check if this cell is in a room or terrain
                bool isInRoom = false;
                foreach (var roomEntry in roomPositions)
                {
                    Vector2Int roomPos = roomEntry.Key;
                    int roomSize = roomSizes[roomPos];
                    
                    if (x >= roomPos.x && x < roomPos.x + roomSize && 
                        y >= roomPos.y && y < roomPos.y + roomSize)
                    {
                        isInRoom = true;
                        break;
                    }
                }
                
                // Draw different indicators for rooms vs terrain - USING SAME POSITIONING AS PREFABS
                if (isInRoom)
                {
                    // Room cells - match floor plane positioning
                    Vector3 cellCenter = worldPos;
                    cellCenter.y = 0.05f; // Slightly above ground like floor planes
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f));
                }
                else
                {
                    // Terrain cells - match cube positioning
                    Vector3 cellCenter = worldPos;
                    cellCenter.y = 0.05f; // Slightly above ground like cubes
                Gizmos.color = Color.gray;
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize * 0.6f, 0.1f, cellSize * 0.6f));
                }
            }
        }
    }

    private void DrawRoomVisualizations(float cellSize)
    {
        // Draw room outlines and fill - USING SAME POSITIONING AS PREFABS
        foreach (var room in roomPositions)
        {
            Vector2Int pos = room.Key;
            RoomType type = room.Value;
            int size = roomSizes[pos];
            
            // Use the EXACT SAME positioning logic as the prefabs
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2Int tilePos = pos + new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorld(tilePos);
                    worldPos.y = 0.1f; // Same Y position as floor planes
                    
                    // Calculate gizmo position to match the scaled floor plane
                    float planeScale = cellSize / 10f; // Same scale as floor planes
                    Vector3 gizmoCenter = worldPos;
                    Vector3 gizmoSize = new Vector3(cellSize, 0.1f, cellSize); // Match floor plane size
                    
                    // Draw gizmo for this tile
                    Gizmos.color = GetRoomColor(type);
                    Gizmos.DrawWireCube(gizmoCenter, gizmoSize);
                    
                    // Draw room fill (semi-transparent)
                    Color fillColor = GetRoomColor(type);
                    fillColor.a = 0.3f;
                    Gizmos.color = fillColor;
                    Gizmos.DrawCube(gizmoCenter, gizmoSize);
                }
            }
            
            // Draw room type label at room center
            #if UNITY_EDITOR
            Vector3 roomCenter = GridToWorld(pos + new Vector2Int(size / 2, size / 2));
            roomCenter.y = 0.2f;
            UnityEditor.Handles.Label(roomCenter, type.ToString());
            #endif
        }
    }
    
    private void DrawRoomConnections(float cellSize)
    {
        // Draw connections between adjacent rooms - USING SAME POSITIONING AS PREFABS
        Gizmos.color = Color.green;
        
        for (int i = 0; i < placedRooms.Count; i++)
        {
            for (int j = i + 1; j < placedRooms.Count; j++)
            {
                Vector2Int roomA = placedRooms[i];
                Vector2Int roomB = placedRooms[j];
                
                if (AreRoomsAdjacent(roomA, roomB, roomSizes[roomA]))
                {
                    // Calculate room centers using same logic as prefabs
                    Vector3 posA = GridToWorld(roomA + new Vector2Int(roomSizes[roomA] / 2, roomSizes[roomA] / 2));
                    Vector3 posB = GridToWorld(roomB + new Vector2Int(roomSizes[roomB] / 2, roomSizes[roomB] / 2));
                    posA.y = 0.15f;
                    posB.y = 0.15f;
                    
                    Gizmos.DrawLine(posA, posB);
                }
            }
        }
        
        // Draw hallway-style path from start to end
        Gizmos.color = Color.red;
        
        if (startRoomPos != Vector2Int.zero && endRoomPos != Vector2Int.zero)
        {
            // Find the hallway-style path through connected rooms
            List<Vector2Int> path = FindPathThroughRooms(startRoomPos, endRoomPos);
            
            if (path != null && path.Count > 1)
            {
                Debug.Log($"Drawing hallway path with {path.Count} rooms: {string.Join(" -> ", path)}");
                
                // Draw the hallway-style room-to-room path
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector2Int roomA = path[i];
                    Vector2Int roomB = path[i + 1];
                    
                    Vector3 posA = GridToWorld(roomA + new Vector2Int(roomSizes[roomA] / 2, roomSizes[roomA] / 2));
                    Vector3 posB = GridToWorld(roomB + new Vector2Int(roomSizes[roomB] / 2, roomSizes[roomB] / 2));
                    posA.y = 0.25f; // Slightly higher than green lines
                    posB.y = 0.25f;
                    
                    // Draw multiple lines to make it appear thicker
                    Gizmos.DrawLine(posA, posB);
                    Gizmos.DrawLine(posA + Vector3.up * 0.01f, posB + Vector3.up * 0.01f);
                    Gizmos.DrawLine(posA + Vector3.up * 0.02f, posB + Vector3.up * 0.02f);
                    
                    Debug.Log($"Hallway line from room {roomA} to {roomB}");
                }
            }
            else
            {
                Debug.LogWarning("No valid hallway path found - showing all room connections instead");
                
                // Fallback: Show all room connections as red lines
                Gizmos.color = Color.red;
                int connectionCount = 0;
                
                for (int i = 0; i < placedRooms.Count; i++)
                {
                    for (int j = i + 1; j < placedRooms.Count; j++)
                    {
                        Vector2Int roomA = placedRooms[i];
                        Vector2Int roomB = placedRooms[j];
                        
                        if (AreRoomsAdjacentWithSpacing(roomA, roomB))
                        {
                            Vector3 posA = GridToWorld(roomA + new Vector2Int(roomSizes[roomA] / 2, roomSizes[roomA] / 2));
                            Vector3 posB = GridToWorld(roomB + new Vector2Int(roomSizes[roomB] / 2, roomSizes[roomB] / 2));
                            posA.y = 0.25f;
                            posB.y = 0.25f;
                            
                            // Draw multiple lines to make it appear thicker
                            Gizmos.DrawLine(posA, posB);
                            Gizmos.DrawLine(posA + Vector3.up * 0.01f, posB + Vector3.up * 0.01f);
                            Gizmos.DrawLine(posA + Vector3.up * 0.02f, posB + Vector3.up * 0.02f);
                            
                            connectionCount++;
                            Debug.Log($"Red connection: {roomA} -> {roomB}");
                        }
                    }
                }
                
                Debug.Log($"Showed {connectionCount} room connections in red");
            }
        }
        
        // Draw corridor/hallway connections between rooms
        DrawCorridorConnections(cellSize);
    }
    
    private void DrawCorridorConnections(float cellSize)
    {
        Gizmos.color = Color.cyan; // Cyan for corridors
        
        for (int i = 0; i < placedRooms.Count; i++)
        {
            for (int j = i + 1; j < placedRooms.Count; j++)
            {
                Vector2Int roomA = placedRooms[i];
                Vector2Int roomB = placedRooms[j];
                
                if (AreRoomsAdjacentWithSpacing(roomA, roomB))
                {
                    // Find the corridor connection point between rooms
                    Vector2Int corridorPoint = FindCorridorConnectionPoint(roomA, roomB);
                    
                    if (corridorPoint != Vector2Int.zero)
                    {
                        Vector3 corridorPos = GridToWorld(corridorPoint);
                        corridorPos.y = 0.3f; // Higher than other gizmos
                        
                        // Draw a small cube to represent the corridor connection
                        float corridorSize = cellSize * 0.3f;
                        Gizmos.DrawWireCube(corridorPos, new Vector3(corridorSize, corridorSize, corridorSize));
                        
                        // Draw lines from room centers to corridor point
                        Vector3 posA = GridToWorld(roomA + new Vector2Int(roomSizes[roomA] / 2, roomSizes[roomA] / 2));
                        Vector3 posB = GridToWorld(roomB + new Vector2Int(roomSizes[roomB] / 2, roomSizes[roomB] / 2));
                        posA.y = 0.3f;
                        posB.y = 0.3f;
                        
                        Gizmos.DrawLine(posA, corridorPos);
                        Gizmos.DrawLine(corridorPos, posB);
                        
                        Debug.Log($"Corridor connection: {roomA} <-> {roomB} at {corridorPoint}");
                    }
                }
            }
        }
        
        // Highlight T-section rooms
        DrawTSectionRooms(cellSize);
    }
    
    private void DrawTSectionRooms(float cellSize)
    {
        Gizmos.color = Color.magenta; // Magenta for T-section rooms
        
        foreach (Vector2Int room in placedRooms)
        {
            if (roomConnections.ContainsKey(room) && roomConnections[room].Count > 1)
            {
                // This is a T-section room (connects to multiple rooms)
                Vector3 roomCenter = GridToWorld(room + new Vector2Int(roomSizes[room] / 2, roomSizes[room] / 2));
                roomCenter.y = 0.4f; // Higher than corridor gizmos
                
                // Draw a sphere to highlight T-section rooms
                float sphereSize = cellSize * 0.5f;
                Gizmos.DrawWireSphere(roomCenter, sphereSize);
                
                // Draw lines to all connected rooms
                foreach (Vector2Int connectedRoom in roomConnections[room])
                {
                    Vector3 connectedCenter = GridToWorld(connectedRoom + new Vector2Int(roomSizes[connectedRoom] / 2, roomSizes[connectedRoom] / 2));
                    connectedCenter.y = 0.4f;
                    
                    Gizmos.DrawLine(roomCenter, connectedCenter);
                }
                
                Debug.Log($"T-section room {room} connects to {roomConnections[room].Count} rooms: {string.Join(", ", roomConnections[room])}");
            }
        }
    }
    
    private Vector2Int FindCorridorConnectionPoint(Vector2Int roomA, Vector2Int roomB)
    {
        // Find the corridor connection point between two adjacent rooms with 1-cell spacing
        int sizeA = roomSizes[roomA];
        int sizeB = roomSizes[roomB];
        
        // Calculate room boundaries
        Vector2Int roomAMin = roomA;
        Vector2Int roomAMax = roomA + new Vector2Int(sizeA, sizeA);
        Vector2Int roomBMin = roomB;
        Vector2Int roomBMax = roomB + new Vector2Int(sizeB, sizeB);
        
        // Check for horizontal adjacency with spacing
        if (roomAMax.x + 1 == roomBMin.x || roomBMax.x + 1 == roomAMin.x)
        {
            // Rooms are horizontally adjacent with 1-cell spacing
            int sharedY = Mathf.Max(roomA.y, roomB.y);
            int sharedYEnd = Mathf.Min(roomA.y + sizeA, roomB.y + sizeB);
            
            if (sharedY < sharedYEnd)
            {
                // There's a shared edge - corridor goes in the space between rooms
                int corridorY = sharedY + (sharedYEnd - sharedY) / 2;
                int corridorX = (roomAMax.x + 1 == roomBMin.x) ? roomAMax.x + 1 : roomBMax.x + 1;
                return new Vector2Int(corridorX, corridorY);
            }
        }
        
        // Check for vertical adjacency with spacing
        if (roomAMax.y + 1 == roomBMin.y || roomBMax.y + 1 == roomAMin.y)
        {
            // Rooms are vertically adjacent with 1-cell spacing
            int sharedX = Mathf.Max(roomA.x, roomB.x);
            int sharedXEnd = Mathf.Min(roomA.x + sizeA, roomB.x + sizeB);
            
            if (sharedX < sharedXEnd)
            {
                // There's a shared edge - corridor goes in the space between rooms
                int corridorX = sharedX + (sharedXEnd - sharedX) / 2;
                int corridorY = (roomAMax.y + 1 == roomBMin.y) ? roomAMax.y + 1 : roomBMax.y + 1;
                return new Vector2Int(corridorX, corridorY);
            }
        }
        
        return Vector2Int.zero; // No valid corridor connection found
    }
    
    private List<Vector2Int> FindPathThroughRooms(Vector2Int start, Vector2Int end)
    {
        Debug.Log($"Finding hallway-style path from {start} to {end}");
        Debug.Log($"Total placed rooms: {placedRooms.Count}");
        
        // Use BFS to find path through ONLY adjacent rooms (no diagonal shortcuts)
        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        
        toVisit.Enqueue(start);
        visited.Add(start);
        
        while (toVisit.Count > 0)
        {
            Vector2Int current = toVisit.Dequeue();
            Debug.Log($"Checking room {current}");
            
            if (current == end)
            {
                Debug.Log($"Found hallway path to end room {end}");
                // Reconstruct path
                List<Vector2Int> path = new List<Vector2Int>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start);
                path.Reverse();
                Debug.Log($"Hallway path: {string.Join(" -> ", path)}");
                return path;
            }
            
            // Find ONLY adjacent rooms (no diagonal connections)
            List<Vector2Int> adjacentRooms = GetStrictlyAdjacentRooms(current);
            
            foreach (Vector2Int neighbor in adjacentRooms)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    toVisit.Enqueue(neighbor);
                    Debug.Log($"Hallway connection: {current} -> {neighbor}");
                }
            }
        }
        
        Debug.LogWarning($"No hallway path found from {start} to {end}");
        return null;
    }
    
    private List<Vector2Int> GetStrictlyAdjacentRooms(Vector2Int room)
    {
        List<Vector2Int> adjacent = new List<Vector2Int>();
        
        foreach (Vector2Int otherRoom in placedRooms)
        {
            if (otherRoom != room && AreRoomsAdjacentWithSpacing(room, otherRoom))
            {
                adjacent.Add(otherRoom);
            }
        }
        
        // Sort by distance to end for more natural hallway path
        adjacent.Sort((a, b) => Vector2Int.Distance(a, endRoomPos).CompareTo(Vector2Int.Distance(b, endRoomPos)));
        
        return adjacent;
    }
    
    private bool AreRoomsAdjacentWithSpacing(Vector2Int roomA, Vector2Int roomB)
    {
        // Check if rooms are adjacent with 1-cell spacing between them
        int roomASize = roomSizes[roomA];
        int roomBSize = roomSizes[roomB];
        
        // Calculate room boundaries
        Vector2Int roomAMin = roomA;
        Vector2Int roomAMax = roomA + new Vector2Int(roomASize, roomASize);
        Vector2Int roomBMin = roomB;
        Vector2Int roomBMax = roomB + new Vector2Int(roomBSize, roomBSize);
        
        // Check if rooms are adjacent with 1-cell spacing
        // Rooms are adjacent if they have exactly 1 cell between them
        bool horizontallyAdjacent = (roomAMax.x + 1 == roomBMin.x || roomBMax.x + 1 == roomAMin.x) &&
                                   (roomAMin.y < roomBMax.y && roomAMax.y > roomBMin.y);
        
        bool verticallyAdjacent = (roomAMax.y + 1 == roomBMin.y || roomBMax.y + 1 == roomAMin.y) &&
                                 (roomAMin.x < roomBMax.x && roomAMax.x > roomBMin.x);
        
        return horizontallyAdjacent || verticallyAdjacent;
    }

    private Color GetRoomColor(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Start: return Color.green;
            case RoomType.End: return Color.red;
            case RoomType.Combat: return Color.yellow;
            case RoomType.Treasure: return Color.blue;
            case RoomType.Boss: return Color.magenta;
            case RoomType.MiniBoss: return Color.cyan;
            case RoomType.Hallway: return Color.gray;
            default: return Color.white;
        }
    }

    private string GetRoomTypeName(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Start: return "Start";
            case RoomType.End: return "End";
            case RoomType.Combat: return "Combat";
            case RoomType.Treasure: return "Treasure";
            case RoomType.Boss: return "Boss";
            case RoomType.MiniBoss: return "MiniBoss";
            case RoomType.Hallway: return "Hallway";
            default: return "Combat";
        }
    }

    [ContextMenu("Check Prefab Alignment")]
    public void CheckPrefabAlignment()
    {
        Debug.Log("=== CHECKING PREFAB ALIGNMENT ===");
        
        if (baseTerrainCube != null)
        {
            Debug.Log($"Base Terrain Cube: {baseTerrainCube.name}");
            Debug.Log($"- Pivot Point: {baseTerrainCube.transform.position}");
            Debug.Log($"- Local Scale: {baseTerrainCube.transform.localScale}");
            Debug.Log($"- Bounds: {baseTerrainCube.GetComponent<Renderer>()?.bounds}");
        }
        else
        {
            Debug.LogWarning("Base Terrain Cube prefab is null!");
        }
        
        if (floorPlane != null)
        {
            Debug.Log($"Floor Plane: {floorPlane.name}");
            Debug.Log($"- Pivot Point: {floorPlane.transform.position}");
            Debug.Log($"- Local Scale: {floorPlane.transform.localScale}");
            Debug.Log($"- Bounds: {floorPlane.GetComponent<Renderer>()?.bounds}");
        }
        else
        {
            Debug.LogWarning("Floor Plane prefab is null!");
        }
        
        Debug.Log($"Grid Cell Size: {cellSize}");
        Debug.Log($"Grid Size: {gridSize}x{gridSize}");
        
        // Check a few sample grid positions
        for (int i = 0; i < 3; i++)
        {
            Vector2Int gridPos = new Vector2Int(i, i);
            Vector3 worldPos = GridToWorld(gridPos);
            Debug.Log($"Grid {gridPos} -> World {worldPos}");
        }
    }
    
    [ContextMenu("Fix Prefab Alignment")]
    public void FixPrefabAlignment()
    {
        Debug.Log("=== FIXING PREFAB ALIGNMENT ===");
        
        // Check if prefabs need pivot adjustment
        if (baseTerrainCube != null)
        {
            // Unity cubes typically have pivot at center, which should be correct
            Debug.Log("Base Terrain Cube pivot should be at center (0.5, 0.5, 0.5)");
        }
        
        if (floorPlane != null)
        {
            // Unity planes typically have pivot at center, which should be correct
            Debug.Log("Floor Plane pivot should be at center (0.5, 0.5, 0.5)");
        }
        
        Debug.Log("If gizmos don't align, check:");
        Debug.Log("1. Prefab pivot points (should be at center)");
        Debug.Log("2. Prefab scale (should be 1,1,1)");
        Debug.Log("3. Prefab rotation (should be 0,0,0)");
        Debug.Log("4. Cell size setting (should match prefab size)");
    }
}
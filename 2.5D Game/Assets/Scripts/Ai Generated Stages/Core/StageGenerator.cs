using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StageGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 20;
    [SerializeField] private float cellSize = 3.0f;
    [SerializeField] private int roomCount = 10;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 6;

    public GameObject combatRoomPrefab;
    public GameObject entranceRoomPrefab;
    


    private Dictionary<Vector2Int, Dictionary<RoomSide, DoorInfo>> roomDoorData = new Dictionary<Vector2Int, Dictionary<RoomSide, DoorInfo>>();

    private CombatRoom[,] grid;
    private TileType[,] tileGrid;

    private List<CombatRoom> activeRooms = new List<CombatRoom>();

    public class RoomInfo
    {
        public CombatRoom room;
        public Vector2Int pos;   // bottom-left grid position
        public Vector2Int size;  // width/height
    }
    List<RoomInfo> placedRooms = new List<RoomInfo>();

    // For now, we are only setting up the grid and cell size.
    // Room generation logic will be added step by step later.

    private void Start()
    {
        float gridCenterX = gridSize / 2f;
        float gridCenterY = gridSize / 2f;
        grid = new CombatRoom[gridSize, gridSize];
        tileGrid = new TileType[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                tileGrid[x, y] = TileType.Empty;
            }
        }

        int roomW = 4, roomH = 4;
        Vector2Int roomSize = new Vector2Int(roomW, roomH);
        int centerX = Mathf.FloorToInt(gridSize / 2f - roomW / 2f);
        int centerY = Mathf.FloorToInt(gridSize / 2f - roomH / 2f);
        Vector2Int centerPos = new Vector2Int(centerX, centerY);

        // Place center room
        PlaceRoom(centerPos, roomSize);

        // Initialize frontier with positions adjacent to center room
        List<Vector2Int> frontier = new List<Vector2Int>();
        AddToFrontier(centerPos, roomSize, frontier);

        // Place additional rooms randomly
        int targetRoomCount = 9; // You can make this configurable
        while (placedRooms.Count < targetRoomCount && frontier.Count > 0)
        {
            // Randomly pick a position from frontier
            int randomIndex = Random.Range(0, frontier.Count);
            Vector2Int candidatePos = frontier[randomIndex];
            
            // Remove this position from frontier
            frontier.RemoveAt(randomIndex);
            
            // Check if we can place a room here
            if (CanPlaceRoom(candidatePos, roomSize, grid, gridSize))
            {
                PlaceRoom(candidatePos, roomSize);
                AddToFrontier(candidatePos, roomSize, frontier);
            }
        }

        // After all rooms are placed and added to activeRooms

        // Example for center and up room:
        CombatRoom centerRoom = activeRooms[0];
        CombatRoom upRoom     = activeRooms[1];
        CombatRoom downRoom   = activeRooms[2];
        CombatRoom leftRoom   = activeRooms[3];
        CombatRoom rightRoom  = activeRooms[4];

        int doorX = roomW / 2; // or random between 0 and roomW-1

        // Top wall of center room, bottom wall of up room
        Vector2Int centerWallLocal = new Vector2Int(doorX, roomH - 1);
        Vector2Int upWallLocal = new Vector2Int(doorX, 0);

        if (centerRoom.GetTileTypeAt(centerWallLocal) == TileType.Wall &&
            upRoom.GetTileTypeAt(upWallLocal) == TileType.Wall)
        {
            centerRoom.SetTileTypeAt(centerWallLocal, TileType.Door);
            upRoom.SetTileTypeAt(upWallLocal, TileType.Door);
        }

        // For each unique pair of rooms
        for (int i = 0; i < placedRooms.Count; i++)
        {
            var roomA = placedRooms[i];
            for (int j = i + 1; j < placedRooms.Count; j++)
            {
                var roomB = placedRooms[j];

                // RIGHT wall of A to LEFT wall of B
                if (roomA.pos.x + roomA.size.x == roomB.pos.x)
                {
                    int overlapStart = Mathf.Max(roomA.pos.y, roomB.pos.y);
                    int overlapEnd = Mathf.Min(roomA.pos.y + roomA.size.y - 1, roomB.pos.y + roomB.size.y - 1);
                    List<int> possibleYs = new List<int>();
                    for (int y = overlapStart; y <= overlapEnd; y++)
                    {
                        Vector2Int aLocal = new Vector2Int(roomA.size.x - 1, y - roomA.pos.y);
                        Vector2Int bLocal = new Vector2Int(0, y - roomB.pos.y);

                        // Check that both tiles are walls AND that there is a room on both sides
                        if (roomA.room.GetTileTypeAt(aLocal) == TileType.Wall &&
                            roomB.room.GetTileTypeAt(bLocal) == TileType.Wall)
                        {
                            possibleYs.Add(y);
                        }
                    }
                    if (possibleYs.Count > 0)
                    {
                        int chosenY = possibleYs[Random.Range(0, possibleYs.Count)];
                        Vector2Int aLocal = new Vector2Int(roomA.size.x - 1, chosenY - roomA.pos.y);
                        Vector2Int bLocal = new Vector2Int(0, chosenY - roomB.pos.y);
                        roomA.room.SetTileTypeAt(aLocal, TileType.Door);
                        roomB.room.SetTileTypeAt(bLocal, TileType.Door);
                    }
                }

                // TOP wall of A to BOTTOM wall of B
                if (roomA.pos.y + roomA.size.y == roomB.pos.y)
                {
                    int overlapStart = Mathf.Max(roomA.pos.x, roomB.pos.x);
                    int overlapEnd = Mathf.Min(roomA.pos.x + roomA.size.x - 1, roomB.pos.x + roomB.size.x - 1);
                    List<int> possibleXs = new List<int>();
                    for (int x = overlapStart; x <= overlapEnd; x++)
                    {
                        Vector2Int aLocal = new Vector2Int(x - roomA.pos.x, roomA.size.y - 1);
                        Vector2Int bLocal = new Vector2Int(x - roomB.pos.x, 0);
                        if (roomA.room.GetTileTypeAt(aLocal) == TileType.Wall && roomB.room.GetTileTypeAt(bLocal) == TileType.Wall)
                            possibleXs.Add(x);
                    }
                    if (possibleXs.Count > 0)
                    {
                        int chosenX = possibleXs[Random.Range(0, possibleXs.Count)];
                        Vector2Int aLocal = new Vector2Int(chosenX - roomA.pos.x, roomA.size.y - 1);
                        Vector2Int bLocal = new Vector2Int(chosenX - roomB.pos.x, 0);
                        roomA.room.SetTileTypeAt(aLocal, TileType.Door);
                        roomB.room.SetTileTypeAt(bLocal, TileType.Door);
                    }
                }
            }
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + cellSize * 0.5f,
            0,
            gridPos.y * cellSize + cellSize * 0.5f
        ) + this.transform.position;
    }

    private void OnDrawGizmos()
    {
        // Draw the grid lines
        Gizmos.color = Color.gray;
        float worldSize = gridSize * cellSize;
        Vector3 origin = this.transform.position;

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

        // Draw tile types from rooms
        if (activeRooms != null)
        {
            foreach (CombatRoom room in activeRooms)
            {
                if (room != null)
                {
                    Dictionary<Vector2Int, TileType> roomTiles = room.GetRoomTileTypes();
                    
                    foreach (var kvp in roomTiles)
                    {
                        Vector2Int gridPos = kvp.Key;
                        TileType tileType = kvp.Value;
                        Vector3 worldPos = GridToWorld(gridPos);
                        
                        switch (tileType)
                        {
                            case TileType.Empty:
                                Gizmos.color = Color.clear;
                                break;
                            case TileType.Floor:
                                Gizmos.color = Color.green;
                                break;
                            case TileType.Door:
                                Gizmos.color = Color.magenta;
                                break;
                            case TileType.Wall:
                                Gizmos.color = Color.yellow;
                                break;
                            case TileType.Corner:
                                Gizmos.color = Color.red;
                                break;
                            default:
                                Gizmos.color = Color.white;
                                break;
                        }
                        
                        if (tileType != TileType.Empty)
                        {
                            Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.8f, 0.1f, cellSize * 0.8f));
                        }
                    }
                }
            }
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

    private bool IsValidDoorPosition(int x, int y, RoomSide side)
    {
        // Ensure door is on a wall, not a corner
        // Ensure door is within room bounds
        // Ensure door connects to an adjacent room
        return true; // Placeholder return, actual implementation needed
    }

    private Vector2Int CalculateCenterDoorPosition(Vector2Int roomPos, Vector2Int roomSize, RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Top:
                return new Vector2Int(roomPos.x + roomSize.x / 2, roomPos.y + roomSize.y - 1);
            case RoomSide.Bottom:
                return new Vector2Int(roomPos.x + roomSize.x / 2, roomPos.y);
            case RoomSide.Left:
                return new Vector2Int(roomPos.x, roomPos.y + roomSize.y / 2);
            case RoomSide.Right:
                return new Vector2Int(roomPos.x + roomSize.x - 1, roomPos.y + roomSize.y / 2);
            default:
                return roomPos;
        }
    }

    private bool CanConnectThroughWall(Vector2Int roomAPos, Vector2Int roomASize, Vector2Int roomBPos, Vector2Int roomBSize, RoomSide side)
    {
        // Step 1: Check if rooms are adjacent based on wall side
        Vector2Int expectedRoomBPos = Vector2Int.zero;
        
        switch (side)
        {
            case RoomSide.Right:
                expectedRoomBPos = new Vector2Int(roomAPos.x + 1, roomAPos.y);
                break;
            case RoomSide.Top:
                expectedRoomBPos = new Vector2Int(roomAPos.x, roomAPos.y + 1);
                break;
            default:
                return false; // Left and Bottom walls don't initiate connections
        }
        
        // Check if room B is actually at the expected position
        if (roomBPos != expectedRoomBPos)
            return false;

        // Step 2: Check wall compatibility
        // Calculate the door position for room A
        Vector2Int doorPosA = CalculateCenterDoorPosition(roomAPos, roomASize, side);
        
        // Calculate the corresponding door position for room B
        RoomSide oppositeSide = GetOppositeSide(side);
        Vector2Int doorPosB = CalculateCenterDoorPosition(roomBPos, roomBSize, oppositeSide);
        
        // Check if the door positions are adjacent (they should be)
        if (!ArePositionsAdjacent(doorPosA, doorPosB))
            return false;

        // Step 3: Validate bounds - ensure both rooms are within grid size
        // Check if room A is within bounds
        if (roomAPos.x < 0 || roomAPos.y < 0 || 
            roomAPos.x + roomASize.x > gridSize || roomAPos.y + roomASize.y > gridSize)
            return false;
        
        // Check if room B is within bounds
        if (roomBPos.x < 0 || roomBPos.y < 0 || 
            roomBPos.x + roomBSize.x > gridSize || roomBPos.y + roomBSize.y > gridSize)
            return false;
        
        // All checks passed - rooms can connect through this wall
        return true;
    }

    private RoomSide GetOppositeSide(RoomSide side)
    {
        switch (side)
        {
            case RoomSide.Right: return RoomSide.Left;
            case RoomSide.Left: return RoomSide.Right;
            case RoomSide.Top: return RoomSide.Bottom;
            case RoomSide.Bottom: return RoomSide.Top;
            default: throw new System.ArgumentException("Invalid RoomSide");
        }
    }

    private bool ArePositionsAdjacent(Vector2Int posA, Vector2Int posB)
    {
        return Mathf.Abs(posA.x - posB.x) <= 1 && Mathf.Abs(posA.y - posB.y) <= 1;
    }

    private void StoreDoorData(Vector2Int roomPos, RoomSide side, Vector2Int doorPos)
    {
        if (!roomDoorData.ContainsKey(roomPos))
        {
            roomDoorData[roomPos] = new Dictionary<RoomSide, DoorInfo>();
        }
        roomDoorData[roomPos][side] = new DoorInfo { hasDoor = true, doorPosition = doorPos, wallSide = side };
    }

    public Dictionary<RoomSide, DoorInfo> GetDoorDataForRoom(Vector2Int roomPos)
    {
        if (roomDoorData.ContainsKey(roomPos))
        {
            return roomDoorData[roomPos];
        }
        return new Dictionary<RoomSide, DoorInfo>();
    }

    private Vector2Int CalculateRoomPosition(Vector2Int gridPos, Vector2Int roomSize)
    {
        // Calculate position based on room size and grid position
        // This ensures rooms are positioned correctly based on their actual sizes
        return new Vector2Int(gridPos.x * roomSize.x, gridPos.y * roomSize.y);
    }

    private List<Vector2Int> GetCellsForRoom(Vector2Int roomPos, Vector2Int roomSize)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                cells.Add(roomPos + new Vector2Int(x, y));
            }
        }
        return cells;
    }

    bool IsAdjacentToExistingRoom(List<Vector2Int> cells, HashSet<Vector2Int> occupiedCells)
    {
        foreach (var cell in cells)
        {
            // Check the four cardinal directions
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                if (occupiedCells.Contains(cell + dir))
                    return true;
            }
        }
        return false;
    }

    bool FitsInGrid(Vector2Int candidatePos, Vector2Int size, int gridSize)
    {
        return candidatePos.x >= 0 && candidatePos.y >= 0 &&
               candidatePos.x + size.x <= gridSize &&
               candidatePos.y + size.y <= gridSize;
    }

    void PlaceRoomAt(Vector2Int cell)
    {
        GameObject roomObj = Instantiate(combatRoomPrefab, GridToWorld(cell), Quaternion.identity, this.transform);
        CombatRoom room = roomObj.GetComponent<CombatRoom>();
        grid[cell.x, cell.y] = room;
    }

    List<Vector2Int> GetEmptyNeighbors(Vector2Int cell, CombatRoom[,] grid, int gridSize)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int n = cell + dir;
            if (n.x >= 0 && n.x < gridSize && n.y >= 0 && n.y < gridSize && grid[n.x, n.y] == null)
                neighbors.Add(n);
        }
        return neighbors;
    }

    bool CanPlace2x2Room(Vector2Int cell, CombatRoom[,] grid, int gridSize)
    {
        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
            {
                int nx = cell.x + dx;
                int ny = cell.y + dy;
                if (nx < 0 || nx >= gridSize || ny < 0 || ny >= gridSize || grid[nx, ny] != null)
                    return false;
            }
        return true;
    }

    void Place2x2Room(Vector2Int cell)
    {
        GameObject roomObj = Instantiate(combatRoomPrefab, GridToWorld(cell), Quaternion.identity, this.transform);
        CombatRoom room = roomObj.GetComponent<CombatRoom>();
        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
                grid[cell.x + dx, cell.y + dy] = room;
    }

    bool CanPlaceRoom(Vector2Int cell, Vector2Int size, CombatRoom[,] grid, int gridSize)
    {
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
            {
                int nx = cell.x + dx;
                int ny = cell.y + dy;
                if (nx < 0 || nx >= gridSize || ny < 0 || ny >= gridSize || grid[nx, ny] != null)
                    return false;
            }
        return true;
    }

    void PlaceRoom(Vector2Int roomPos, Vector2Int roomSize)
    {
        // Mark tiles as floor (reserve space)
        for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
                tileGrid[roomPos.x + x, roomPos.y + y] = TileType.Floor;

        // Instantiate the room
        GameObject roomObj = Instantiate(combatRoomPrefab, GridToWorld(roomPos), Quaternion.identity, this.transform);
        CombatRoom room = roomObj.GetComponent<CombatRoom>();
        room.SetupRoom(roomPos, roomSize, cellSize, gridSize, this, roomPos);
        activeRooms.Add(room);

        placedRooms.Add(new RoomInfo { room = room, pos = roomPos, size = roomSize });
    }

    List<Vector2Int> GetEmptyNeighbors(Vector2Int cell, Vector2Int size, CombatRoom[,] grid, int gridSize)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector2Int baseCell = cell + new Vector2Int(dx, dy);
                foreach (var dir in dirs)
                {
                    Vector2Int n = baseCell + dir;
                    if (n.x >= 0 && n.x < gridSize && n.y >= 0 && n.y < gridSize && grid[n.x, n.y] == null && !neighbors.Contains(n))
                        neighbors.Add(n);
                }
            }
        }
        return neighbors;
    }

    private void AddToFrontier(Vector2Int roomPos, Vector2Int roomSize, List<Vector2Int> frontier)
    {
        // Add all positions adjacent to this room's walls
        Vector2Int[] adjacentPositions = {
            new Vector2Int(roomPos.x, roomPos.y + roomSize.y),      // Above
            new Vector2Int(roomPos.x, roomPos.y - roomSize.y),      // Below
            new Vector2Int(roomPos.x - roomSize.x, roomPos.y),      // Left
            new Vector2Int(roomPos.x + roomSize.x, roomPos.y),      // Right
        };
        
        foreach (Vector2Int pos in adjacentPositions)
        {
            // Only add if it's within grid bounds and not already in frontier
            if (pos.x >= 0 && pos.x + roomSize.x <= gridSize && 
                pos.y >= 0 && pos.y + roomSize.y <= gridSize &&
                !frontier.Contains(pos))
            {
                frontier.Add(pos);
            }
        }
    }
}

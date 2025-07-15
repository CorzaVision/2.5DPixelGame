using UnityEngine;
using System.Collections.Generic;

public class GeneratorPrototype : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 20;
    public float cellSize = 3.0f;
    public int roomCount = 10;
    public int minRoomSize = 3;
    public int maxRoomSize = 6;
    public GameObject roomPrefab;

    private PrototypeCombatRoom[,] grid;
    private List<PrototypeCombatRoom> rooms = new List<PrototypeCombatRoom>();
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private List<Vector2Int> doorPositions = new List<Vector2Int>();
    private HashSet<(Vector2Int, Vector2Int)> connectedPairs = new HashSet<(Vector2Int, Vector2Int)>();

    // 1. Add a tileGrid to track each cell's type
    private TileType[,] tileGrid;

    // Add Hallway to TileType
    public enum TileType
    {
        Empty,
        Floor,
        Wall,
        Door,
        Corner,
        Hallway
    }

    // Add door data tracking
    private Dictionary<Vector2Int, Dictionary<RoomSide, DoorInfo>> roomDoorData = new Dictionary<Vector2Int, Dictionary<RoomSide, DoorInfo>>();
    // Add door reservation tracking
    private Dictionary<(Vector2Int, RoomSide), Vector2Int> doorReservations = new Dictionary<(Vector2Int, RoomSide), Vector2Int>();

    public int roomW = 4, roomH = 4;

    void Start()
    {
        // Initialize tileGrid first
        tileGrid = new TileType[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                tileGrid[x, y] = TileType.Empty;

        grid = new PrototypeCombatRoom[gridSize, gridSize];
        List<Vector2Int> placedRoomPositions = new List<Vector2Int>();
        Vector2Int center = new Vector2Int(gridSize / 2 - roomW / 2, gridSize / 2 - roomH / 2);
        PrototypeCombatRoom centerRoom = PlaceRoom(center);
        placedRoomPositions.Add(center);

        List<Vector2Int> frontier = new List<Vector2Int>();
        AddToFrontier(center, frontier);

        int targetRoomCount = roomCount;
        int maxIterations = 1000; // Prevent infinite loops
        int iterationCount = 0;
        while (placedRoomPositions.Count < targetRoomCount && frontier.Count > 0 && iterationCount < maxIterations)
        {
            iterationCount++;
            int idx = Random.Range(0, frontier.Count);
            Vector2Int candidate = frontier[idx];
            frontier.RemoveAt(idx);
            if (CanPlaceRoom(candidate))
            {
                PlaceRoom(candidate);
                placedRoomPositions.Add(candidate);
                AddToFrontier(candidate, frontier);
            }
        }

        // After all rooms are placed, carve doors between adjacent rooms
        HashSet<(Vector2Int, Vector2Int)> processedPairs = new HashSet<(Vector2Int, Vector2Int)>();
        foreach (var posA in placedRoomPositions)
        {
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int posB = posA + dir * roomW; // assumes square rooms
                if (!placedRoomPositions.Contains(posB) || posA == posB) continue;
                // Ensure each pair is only processed once
                var pair = (posA, posB);
                var reversePair = (posB, posA);
                if (processedPairs.Contains(pair) || processedPairs.Contains(reversePair)) continue;
                processedPairs.Add(pair);
                RoomSide sideA = RoomSide.Right, sideB = RoomSide.Left;
                if (dir == Vector2Int.right) { sideA = RoomSide.Right; sideB = RoomSide.Left; }
                else if (dir == Vector2Int.left) { sideA = RoomSide.Left; sideB = RoomSide.Right; }
                else if (dir == Vector2Int.up) { sideA = RoomSide.Top; sideB = RoomSide.Bottom; }
                else if (dir == Vector2Int.down) { sideA = RoomSide.Bottom; sideB = RoomSide.Top; }
                int overlapStart, overlapEnd;
                if (dir.x != 0)
                {
                    overlapStart = Mathf.Max(posA.y, posB.y);
                    overlapEnd = Mathf.Min(posA.y + roomH - 1, posB.y + roomH - 1);
                }
                else
                {
                    overlapStart = Mathf.Max(posA.x, posB.x);
                    overlapEnd = Mathf.Min(posA.x + roomW - 1, posB.x + roomW - 1);
                }
                if (overlapStart > overlapEnd) continue;
                List<int> possible = new List<int>();
                for (int i = overlapStart; i <= overlapEnd; i++) possible.Add(i);
                // Only create one door at a random valid position
                List<(Vector2Int aLocal, Vector2Int bLocal, Vector2Int aWorld, Vector2Int bWorld, int i)> validDoorPositions = new List<(Vector2Int, Vector2Int, Vector2Int, Vector2Int, int)>();
                foreach (int i in possible)
                {
                    Vector2Int aLocal, bLocal;
                    if (dir.x != 0)
                    {
                        aLocal = GetWallDoorLocal(sideA, i - posA.y, roomW, roomH);
                        bLocal = GetWallDoorLocal(sideB, i - posB.y, roomW, roomH);
                    }
                    else
                    {
                        aLocal = GetWallDoorLocal(sideA, i - posA.x, roomW, roomH);
                        bLocal = GetWallDoorLocal(sideB, i - posB.x, roomW, roomH);
                    }
                    Vector2Int aWorld = posA + aLocal;
                    Vector2Int bWorld = posB + bLocal;
                    if (tileGrid[aWorld.x, aWorld.y] == TileType.Wall && tileGrid[bWorld.x, bWorld.y] == TileType.Wall)
                    {
                        validDoorPositions.Add((aLocal, bLocal, aWorld, bWorld, i));
                    }
                }
                if (validDoorPositions.Count > 0)
                {
                    var chosen = validDoorPositions[Random.Range(0, validDoorPositions.Count)];
                    tileGrid[chosen.aWorld.x, chosen.aWorld.y] = TileType.Door;
                    tileGrid[chosen.bWorld.x, chosen.bWorld.y] = TileType.Door;
                    if (!roomDoorData.ContainsKey(posA)) roomDoorData[posA] = new Dictionary<RoomSide, DoorInfo>();
                    if (!roomDoorData.ContainsKey(posB)) roomDoorData[posB] = new Dictionary<RoomSide, DoorInfo>();
                    roomDoorData[posA][sideA] = new DoorInfo { hasDoor = true, doorPosition = chosen.aLocal };
                    roomDoorData[posB][sideB] = new DoorInfo { hasDoor = true, doorPosition = chosen.bLocal };
                    Debug.Log($"[GEN-DOOR] Carved door between {posA} ({sideA}, {chosen.aLocal}) and {posB} ({sideB}, {chosen.bLocal})");
                }
            }
        }

        foreach (var room in rooms)
        {
            if (room == null) continue;
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(room.transform.position.x / cellSize - roomW / 2), Mathf.RoundToInt(room.transform.position.z / cellSize - roomH / 2));
            room.SetupRoom(pos, new Vector2Int(roomW, roomH), cellSize, gridSize, this, pos);
        }
    }

    PrototypeCombatRoom PlaceRoom(Vector2Int pos)
    {
        if (tileGrid == null)
        {
            Debug.LogError("tileGrid is not initialized!");
            return null;
        }
        if (roomPrefab == null)
        {
            Debug.LogError("roomPrefab is not assigned!");
            return null;
        }
        Vector3 worldPos = new Vector3(pos.x * cellSize + (roomW * cellSize) / 2f, 0, pos.y * cellSize + (roomH * cellSize) / 2f);
        GameObject obj = Instantiate(roomPrefab, worldPos, Quaternion.identity, this.transform);
        PrototypeCombatRoom room = obj.GetComponent<PrototypeCombatRoom>();
        if (room == null)
        {
            Debug.LogError("PrototypeCombatRoom component missing on prefab!");
            return null;
        }
        rooms.Add(room);
        // Mark all occupied cells for this room and set wall tiles
        for (int x = 0; x < roomW; x++)
            for (int y = 0; y < roomH; y++)
            {
                int gx = pos.x + x;
                int gy = pos.y + y;
                if (gx >= 0 && gx < gridSize && gy >= 0 && gy < gridSize)
                {
                    occupied.Add(new Vector2Int(gx, gy));
                    grid[gx, gy] = room;
                    // Set corners as TileType.Corner, other perimeter as Wall
                    bool isCorner = (x == 0 || x == roomW - 1) && (y == 0 || y == roomH - 1);
                    if (isCorner)
                        tileGrid[gx, gy] = TileType.Corner;
                    else if (x == 0 || x == roomW - 1 || y == 0 || y == roomH - 1)
                        tileGrid[gx, gy] = TileType.Wall;
                    else
                        tileGrid[gx, gy] = TileType.Floor;
                }
                else
                {
                    Debug.LogWarning($"Room at {pos} exceeds grid bounds at ({gx},{gy})");
                }
            }
        return room;
    }

    void AddToFrontier(Vector2Int pos, List<Vector2Int> frontier)
    {
        Vector2Int[] directions = {
            new Vector2Int(0, roomH),    // Up
            new Vector2Int(0, -roomH),   // Down
            new Vector2Int(-roomW, 0),   // Left
            new Vector2Int(roomW, 0),    // Right
        };
        foreach (var dir in directions)
        {
            Vector2Int candidate = pos + dir;
            if (!frontier.Contains(candidate) && CanPlaceRoom(candidate))
                frontier.Add(candidate);
        }
    }

    bool CanPlaceRoom(Vector2Int pos)
    {
        // Check if the room would be out of bounds
        if (pos.x < 0 || pos.y < 0 || pos.x + roomW > gridSize || pos.y + roomH > gridSize)
            return false;
        for (int x = 0; x < roomW; x++)
            for (int y = 0; y < roomH; y++)
                if (occupied.Contains(new Vector2Int(pos.x + x, pos.y + y)))
                    return false;
        return true;
    }

    PrototypeCombatRoom GetRoomAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= gridSize || pos.y >= gridSize)
            return null;
        return grid[pos.x, pos.y];
    }

    // Converts a grid position to a world position (center of cell)
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize + cellSize / 2f, 0, gridPos.y * cellSize + cellSize / 2f);
    }

    // Returns door data for a given room position (placeholder implementation)
    public Dictionary<RoomSide, DoorInfo> GetDoorDataForRoom(Vector2Int roomGridPos)
    {
        if (roomDoorData.ContainsKey(roomGridPos)) {
            Debug.Log($"[GetDoorData] Found door data for room {roomGridPos}");
            return roomDoorData[roomGridPos];
        }
        Debug.LogWarning($"[GetDoorData] No door data found for room {roomGridPos}");
        return new Dictionary<RoomSide, DoorInfo>();
    }

    // Helper to get a valid local door position for a given wall and overlap
    private Vector2Int GetWallDoorLocal(RoomSide side, int overlap, int roomW, int roomH)
    {
        switch (side)
        {
            case RoomSide.Left: return new Vector2Int(0, overlap);
            case RoomSide.Right: return new Vector2Int(roomW - 1, overlap);
            case RoomSide.Top: return new Vector2Int(overlap, roomH - 1);
            case RoomSide.Bottom: return new Vector2Int(overlap, 0);
            default: return Vector2Int.zero;
        }
    }

    // Draw a straight hallway between two points (doorA, doorB)
    private void DrawStraightHallway(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        while (current != to)
        {
            if (tileGrid[current.x, current.y] == TileType.Empty)
                tileGrid[current.x, current.y] = TileType.Hallway;
            // Move towards 'to' (first x, then y)
            if (current.x != to.x)
                current.x += (to.x > current.x) ? 1 : -1;
            else if (current.y != to.y)
                current.y += (to.y > current.y) ? 1 : -1;
        }
        // Mark the last tile
        if (tileGrid[to.x, to.y] == TileType.Empty)
            tileGrid[to.x, to.y] = TileType.Hallway;
    }

    void OnDrawGizmos()
    {
        // Only draw if tileGrid is initialized
        if (tileGrid == null)
            return;
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
        // Draw tile types from tileGrid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                TileType tileType = tileGrid[x, y];
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
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
                    if (tileType == TileType.Door)
                    {
                        Debug.Log($"Door tile at grid ({x},{y}) world {worldPos}");
                    }
                }
            }
        }
        // Draw rooms as yellow wireframes
        Gizmos.color = Color.yellow;
        foreach (var room in rooms)
        {
            if (room == null) continue;
            Vector3 pos = room.transform.position;
            Gizmos.DrawWireCube(pos, new Vector3(roomW * cellSize, 0.1f, roomH * cellSize));
        }
        // Draw doors as magenta cubes
        Gizmos.color = Color.magenta;
        foreach (var door in doorPositions)
        {
            Gizmos.DrawCube(new Vector3(door.x * cellSize + cellSize / 2f, 0, door.y * cellSize + cellSize / 2f), new Vector3(cellSize * 0.5f, 0.2f, cellSize * 0.5f));
        }
    }
}

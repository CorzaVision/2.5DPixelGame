using UnityEngine;
using System.Collections.Generic;

public class PrototypeCombatRoom : MonoBehaviour
{
    private Vector2Int startPos;
    private Vector2Int roomSize;
    private float cellSize;
    private int gridSize;
    private GeneratorPrototype generatorPrototype;

    public GameObject floorPrefab;
    public GameObject wallStraightPrefab;
    public GameObject wallCornerPrefab;

    [SerializeField] public int minRoomSize = 3;
    [SerializeField] public int maxRoomSize = 6;

    public Vector2Int RoomSize => roomSize;

    public bool DoorLeft;
    public bool DoorRight;
    public bool DoorTop;
    public bool DoorBottom;

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
    
    private TileType[,] tileTypes;

    public void SetupRoom(Vector2Int startPos, Vector2Int size, float cellSize, int gridSize, GeneratorPrototype generatorPrototype, Vector2Int roomGridPos)
    {
        this.startPos = startPos;
        this.cellSize = cellSize;
        this.gridSize = gridSize;
        this.generatorPrototype = generatorPrototype;
        this.roomSize = size;
        
        tileTypes = new TileType[roomSize.x, roomSize.y];
        
        // Get door data for this room (from generator only)
        Dictionary<RoomSide, DoorInfo> doorData = generatorPrototype.GetDoorDataForRoom(roomGridPos);
        Debug.Log($"[ROOM-SETUP] Room at {startPos} received door data:");
        foreach (var kvp in doorData)
        {
            Debug.Log($"  Side {kvp.Key}: hasDoor={kvp.Value.hasDoor}, localPos={kvp.Value.doorPosition}");
        }
        GenerateRoomPrefabs(doorData);
    }

    public void GenerateRoomPrefabs(Dictionary<RoomSide, DoorInfo> doorData)
    {
        Vector2 center = new Vector2(startPos.x + (roomSize.x - 1) / 2f, startPos.y + (roomSize.y - 1) / 2f);

        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int gridPos = startPos + new Vector2Int(x, y);
                Vector3 worldPos = generatorPrototype.GridToWorld(gridPos);

                bool atLeft = x == 0;
                bool atRight = x == roomSize.x - 1;
                bool atBottom = y == 0;
                bool atTop = y == roomSize.y - 1;

                GameObject prefabToSpawn = floorPrefab;
                Quaternion rot = Quaternion.identity;

                // 1. Decide prefab
                if (atLeft && atBottom)
                {
                    prefabToSpawn = wallCornerPrefab;
                    SetTileTypeAt(new Vector2Int(x, y), TileType.Corner);
                }
                else if (atRight && atBottom)
                {
                    prefabToSpawn = wallCornerPrefab;
                    SetTileTypeAt(new Vector2Int(x, y), TileType.Corner);
                }
                else if (atRight && atTop)
                {
                    prefabToSpawn = wallCornerPrefab;
                    SetTileTypeAt(new Vector2Int(x, y), TileType.Corner);
                }
                else if (atLeft && atTop)
                {
                    prefabToSpawn = wallCornerPrefab;
                    SetTileTypeAt(new Vector2Int(x, y), TileType.Corner);
                }
                else if (atLeft)
                {
                    if (HasDoorAtPosition(doorData, RoomSide.Left, x, y))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atRight)
                {
                    if (HasDoorAtPosition(doorData, RoomSide.Right, x, y))
                    {
                        prefabToSpawn = floorPrefab;
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atTop)
                {
                    if (HasDoorAtPosition(doorData, RoomSide.Top, x, y))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atBottom)
                {
                    if (HasDoorAtPosition(doorData, RoomSide.Bottom, x, y))
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else
                {
                    // Interior floor tile
                    prefabToSpawn = floorPrefab;
                    SetTileTypeAt(new Vector2Int(x, y), TileType.Floor);
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

    private bool HasDoorAtPosition(Dictionary<RoomSide, DoorInfo> doorData, RoomSide side, int x, int y)
    {
        if (doorData.ContainsKey(side) && doorData[side].hasDoor)
        {
            Vector2Int doorPos = doorData[side].doorPosition;
            Vector2Int currentPos = startPos + new Vector2Int(x, y);
            return doorPos == currentPos;
        }
        return false;
    }

    public void SetTileTypeAt(Vector2Int localPos, TileType tileType)
    {
        if (localPos.x >= 0 && localPos.x < roomSize.x && localPos.y >= 0 && localPos.y < roomSize.y)
        {
            tileTypes[localPos.x, localPos.y] = tileType;
            if (tileType == TileType.Door)
                Debug.Log($"Set door tile at local {localPos} in room at {startPos}");
        }
    }

    public TileType GetTileTypeAt(Vector2Int localPos)
    {
        if (localPos.x >= 0 && localPos.x < roomSize.x && localPos.y >= 0 && localPos.y < roomSize.y)
            return tileTypes[localPos.x, localPos.y];
        return TileType.Empty;
    }

    public Dictionary<Vector2Int, TileType> GetRoomTileTypes()
    {
        Dictionary<Vector2Int, TileType> roomTiles = new Dictionary<Vector2Int, TileType>();
        
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int globalPos = startPos + new Vector2Int(x, y);
                roomTiles[globalPos] = GetTileTypeAt(new Vector2Int(x, y));
            }
        }
        
        return roomTiles;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (tileTypes == null) return;
        Gizmos.color = Color.magenta;
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                if (tileTypes[x, y] == TileType.Door)
                {
                    Vector3 worldPos = transform.position + new Vector3((x - roomSize.x / 2f + 0.5f) * cellSize, 0.2f, (y - roomSize.y / 2f + 0.5f) * cellSize);
                    Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.3f, 0.2f, cellSize * 0.3f));
                }
            }
        }
    }
#endif
}
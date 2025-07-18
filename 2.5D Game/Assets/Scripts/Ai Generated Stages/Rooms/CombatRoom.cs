using UnityEngine;
using System.Collections.Generic;

public class CombatRoom : MonoBehaviour, IRoomGenerator
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

    public void SetupRoom(Vector2Int startPos, Vector2Int size, float cellSize, int gridSize, StageGenerator stageGenerator, Vector2Int roomGridPos)
    {
        this.startPos = startPos;
        this.cellSize = cellSize;
        this.gridSize = gridSize;
        this.stageGenerator = stageGenerator;
        this.roomSize = size;
        
        tileTypes = new TileType[roomSize.x, roomSize.y];
        
        // Get door data for this room
        Dictionary<RoomSide, DoorInfo> doorData = stageGenerator.GetDoorDataForRoom(roomGridPos);
        
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
                Vector3 worldPos = stageGenerator.GridToWorld(gridPos);

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
                        Debug.Log("Door Left");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        Debug.Log("Wall Straight Left");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atRight)
                {
                    if (DoorRight && y == doorY && y < roomSize.y)
                    {
                        prefabToSpawn = floorPrefab;
                        Debug.Log("Door Right");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        Debug.Log("Wall Straight Right");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atTop)
                {
                    if (DoorTop && x == doorX && x < roomSize.x)
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Top");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        Debug.Log("Wall Straight Top");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Wall);
                    }
                }
                else if (atBottom)
                {
                    if (DoorBottom && x == doorX && x < roomSize.x)
                    {
                        prefabToSpawn = floorPrefab; // or doorPrefab
                        Debug.Log("Door Bottom");
                        SetTileTypeAt(new Vector2Int(x, y), TileType.Door);
                    }
                    else
                    {
                        prefabToSpawn = wallStraightPrefab;
                        Debug.Log("Wall Straight Bottom");
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
            tileTypes[localPos.x, localPos.y] = tileType;
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
}
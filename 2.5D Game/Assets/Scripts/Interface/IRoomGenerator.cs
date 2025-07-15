using UnityEngine;
using System.Collections.Generic;

public enum RoomSide { Top, Bottom, Left, Right }

public enum TileType
{
    Empty,
    Floor,
    Wall,
    Door,
    Corner
}

[System.Serializable]
public struct DoorInfo
{
    public bool hasDoor;
    public Vector2Int doorPosition;
    public RoomSide wallSide;
}

public interface IRoomGenerator
{
    void SetupRoom(Vector2Int startPos, Vector2Int size, float cellSize, int gridSize, StageGenerator stageGenerator, Vector2Int roomGridPos);
    
    // Tile type management
    TileType GetTileTypeAt(Vector2Int localPos);
    void SetTileTypeAt(Vector2Int localPos, TileType tileType);
    Vector2Int RoomSize { get; }
}

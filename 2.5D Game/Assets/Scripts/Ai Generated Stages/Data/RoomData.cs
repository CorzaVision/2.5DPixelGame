using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomData
{
    public Vector2Int position;
    public Vector2Int size;
    public List<Vector2Int> connectedHallways = new List<Vector2Int>();
    public List<Vector2Int> connectedRooms = new List<Vector2Int>();
    public List<Vector2Int> enemyPositions = new List<Vector2Int>();
    public List<Vector2Int> doorPositions = new List<Vector2Int>();
    public RoomType roomType;
        
}
public enum RoomType
{
    Combat,
    Treasure,
    Boss,
    Safe,
}

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HallwayData
{

    public Vector2Int startRoom;
    public Vector2Int endRoom;
    public List<Vector2Int> path = new List<Vector2Int>();
    public bool isBranch;
    
}

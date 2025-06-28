using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageLayout
{
    public List<RoomData> rooms = new List<RoomData>();
    public List<HallwayData> hallways = new List<HallwayData>();
    public Vector2Int stageSize;
    
    // Persistent elements
    public RoomData startRoom;
    public RoomData exitRoom;
    public Vector2Int playerSpawnPosition;
    public Vector2Int mainExitPosition;
    
    // Protected positions (AI cannot modify these)
    public List<Vector2Int> protectedPositions = new List<Vector2Int>();
    
    public void InitializeProtectedPositions()
    {
        protectedPositions.Clear();
        
        // Add start room area
        if (startRoom != null)
        {
            for (int x = startRoom.position.x; x < startRoom.position.x + startRoom.size.x; x++)
            {
                for (int y = startRoom.position.y; y < startRoom.position.y + startRoom.size.y; y++)
                {
                    protectedPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // Add exit room area
        if (exitRoom != null)
        {
            for (int x = exitRoom.position.x; x < exitRoom.position.x + exitRoom.size.x; x++)
            {
                for (int y = exitRoom.position.y; y < exitRoom.position.y + exitRoom.size.y; y++)
                {
                    protectedPositions.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}

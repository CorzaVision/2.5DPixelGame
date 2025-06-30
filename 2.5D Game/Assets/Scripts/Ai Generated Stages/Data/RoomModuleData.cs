using UnityEngine;

[CreateAssetMenu(fileName = "RoomModuleData", menuName = "AGS/RoomModule")]
public class RoomModuleData : ScriptableObject
{
    [Header("Room Module Identity")]
    public string roomName;
    public RoomCategory roomCategory;
    public GameObject roomPrefab;

    [Header("Door Positions")]
    public Vector2Int[] doorPositions;

    [Header("Connection Rules")]
    public bool canConnectToStart;
    public bool canConnectToExit;
    public bool canConnectToBoss;
    public bool canConnectToMiniBoss;
}

using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "AGS/StageData")]
public class StageData : ScriptableObject
{
    [Header("Stage Configuration")]
    public string stageName = "Default Stage";
    public int roomCount = 4;
    public float layoutComplexity = 0.5f;
    public float enemyDensity = 0.3f;
    
    [Header("Starting Location")]
    public Vector2Int startRoomPosition = Vector2Int.zero;
    public Vector2Int startRoomSize = new Vector2Int(3, 3);
    public Vector2Int playerSpawnPosition = new Vector2Int(1, 1); // Relative to start room
    
    [Header("Exit Doorway")]
    public Vector2Int mainExitPosition = new Vector2Int(15, 0);
    public Vector2Int exitRoomSize = new Vector2Int(3, 3);
    
    [Header("Room Settings")]
    public Vector2Int roomSize = new Vector2Int(3, 3);
    public int minEnemiesPerRoom = 2;
    public int maxEnemiesPerRoom = 5;
    
    [Header("Generation Settings")]
    public bool allowBranches = true;
    public float branchChance = 0.3f;
    public int maxBranchDepth = 2;
    
}

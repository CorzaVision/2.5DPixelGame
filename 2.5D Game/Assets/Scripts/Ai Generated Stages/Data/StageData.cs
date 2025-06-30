using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "AGS/StageData")]
public class StageData : ScriptableObject
{
    [Header("Stage Configuration")]
    public string stageName = "Default Stage";
    [Header("Stage Type")]
    public int currentStage = 1;
    public StageType stageType = StageType.Regular;

    [Header("Room Distribution")]
    public int combatRoomCount = 5;
    public int treasureRoomCount = 2;
    public int bossRoomCount = 0;
    public int miniBossRoomCount = 0;
    public int roomCount = 10;

    [Header("Room Size Ranges")]
    [Header("Combat Rooms")]
    public Vector2Int combatRoomMinSize = new Vector2Int(3, 3);
    public Vector2Int combatRoomMaxSize = new Vector2Int(4, 4);

    [Header("Treasure Rooms")]
    public Vector2Int treasureRoomMinSize = new Vector2Int(2, 2);
    public Vector2Int treasureRoomMaxSize = new Vector2Int(3, 3);

    [Header("Boss Rooms")]
    public Vector2Int bossRoomMinSize = new Vector2Int(6, 6);
    public Vector2Int bossRoomMaxSize = new Vector2Int(8, 8);

    [Header("Mini Boss Rooms")]
    public Vector2Int miniBossRoomMinSize = new Vector2Int(5, 5);
    public Vector2Int miniBossRoomMaxSize = new Vector2Int(6, 6);

    
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

    [Header("Boss Room Settings")]
    public Vector2Int bossRoomSize = new Vector2Int(6, 8);
    public Vector2Int miniBossRoomSize = new Vector2Int(5, 6);
    public bool bossRoomIsFinalRoom = true;
    
    [Header("Generation Settings")]
    public bool allowBranches = true;
    public float branchChance = 0.3f;
    public int maxBranchDepth = 2;

    public enum StageType
    {
        Regular,
        Boss,
        MiniBoss,
    }
    public int GetTotalRoomCount()
    {
        int total = combatRoomCount + treasureRoomCount;
        if (stageType == StageType.Boss)
        {
            total += bossRoomCount;
        }
        else if (stageType == StageType.MiniBoss)
        {
            total += miniBossRoomCount;
        }
        return total;
    }

    }


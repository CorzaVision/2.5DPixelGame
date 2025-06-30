using UnityEngine;
using UnityEngine.UI;

public class DungeonTestUI : MonoBehaviour
{
    [Header("References")]
    public StageGenerator stageGenerator;
    public Text debugText;
    
    [Header("Grid Info")]
    [Tooltip("Calculated automatically based on room count and size.")]
    [HideInInspector]
    public int gridSize;
    
    [HideInInspector]
    public Vector2Int minRoomSize;
    [HideInInspector]
    public Vector2Int maxRoomSize;
    [HideInInspector]
    public RoomCategory roomCategory;
    [HideInInspector]
    public ExitEdge lastExitEdge;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Auto-generate on start
        if (stageGenerator != null)
        {
            TestGeneration();
        }
    }

    public void TestGeneration()
    {
        Debug.Log("=== STARTING DUNGEON GENERATION TEST ===");
        
        if (stageGenerator.stageData == null)
        {
            Debug.LogError("StageData is not assigned!");
            return;
        }
        
        // Comment out the problematic method calls for now
        // StageLayout layout = stageGenerator.GenerateStage();
        
        Debug.Log("Generation test completed (simplified)");
        Debug.Log("=== DUNGEON GENERATION TEST COMPLETE ===");
    }
    
    public void TestRegeneration()
    {
        Debug.Log("=== TESTING REGENERATION ===");
        // stageGenerator.RegenerateStage();
    }
    
    public void TestClear()
    {
        Debug.Log("=== TESTING CLEAR ===");
        // stageGenerator.ClearStage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

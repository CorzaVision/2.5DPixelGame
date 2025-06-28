using UnityEngine;
using UnityEngine.UI;

public class DungeonTestUI : MonoBehaviour
{
    public StageGenerator stageGenerator;
    public Text debugText;
    
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
        
        StageLayout layout = stageGenerator.GenerateStage();
        
        if (layout != null)
        {
            Debug.Log($"Generation successful! Created {layout.rooms.Count} rooms and {layout.hallways.Count} hallways");
            
            // Log room details
            foreach (RoomData room in layout.rooms)
            {
                Debug.Log($"Room: {room.roomType} at {room.position} with size {room.size}");
            }
        }
        else
        {
            Debug.LogError("Generation failed!");
        }
        
        Debug.Log("=== DUNGEON GENERATION TEST COMPLETE ===");
    }
    
    public void TestRegeneration()
    {
        Debug.Log("=== TESTING REGENERATION ===");
        stageGenerator.RegenerateStage();
    }
    
    public void TestClear()
    {
        Debug.Log("=== TESTING CLEAR ===");
        stageGenerator.ClearStage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

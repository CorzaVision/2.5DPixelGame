using UnityEngine;

public class StageManager
{
    [Header("Stage Configuration")]
    public StageData currentStageData;

    [Header("Generation Settings")]
    public bool generateOnStart = true;
    public bool showDebugInfo = true;

    [Header("References")]
    public StageGenerator stageGenerator;

    public void Start()
    {
        if (generateOnStart && currentStageData != null)
        {
            stageGenerator.stageData = currentStageData;
        }
    }
}
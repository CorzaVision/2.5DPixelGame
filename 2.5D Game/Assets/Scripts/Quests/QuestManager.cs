using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    #region SerializeField
    [Header("Quest Management")]
    [SerializeField] private List<QuestData> availableQuests;
    [SerializeField] private bool showDebug = true;

    #endregion

    #region Private Fields
    private List<QuestData> activeQuests; // List of active quests
    private List<QuestData> completedQuests; // List of completed quests
    private PlayerStats playerStats; // Player stats
    private PlayerInventory playerInventory; // Player inventory

    #endregion

    #region Events
    public event Action<QuestData> OnQuestAccepted; // Event when a quest is accepted
    public event Action<QuestData> OnQuestUpdated; // Event when a quest is updated
    public event Action<QuestData> OnQuestCompleted; // Event when a quest is completed
    public event Action<QuestData> OnRewardClaimed; // Event when a quest reward is claimed

    #endregion


    #region Unity Lifecycle
    private void Awake()
    {
        InitializeQuestManager(); // Initialize the quest manager
        InitializeSingleton(); // Initialize the singleton
    }

    private void Start()
    {
        InitializeComponents(); // Initialize the components
        ValidateComponents(); // Check if the components are valid
    }
    #endregion

    #region Initialize
    private void InitializeQuestManager()
    {
        activeQuests = new List<QuestData>();
        completedQuests = new List<QuestData>();
    }

    private void InitializeComponents()
    {
        playerStats = FindObjectOfType<PlayerStats>(); // Get the player stats
        playerInventory = FindObjectOfType<PlayerInventory>(); // Get the player inventory
    }

    private void ValidateComponents()
    {
        if (playerStats == null)
        {
            Debug.LogError("Player stats not found");
        }
        if (playerInventory == null)
        {
            Debug.LogError("Player inventory not found");
        }
    }
    #endregion

    #region Singleton / Get Instance
    public static QuestManager Instance { get; private set; } // Only one instance of the quest manager

    private void InitializeSingleton() // Initialize the singleton
    {
        if (Instance == null) // if the instance is null, set it to this
        {
            Instance = this; // set the instance to this script
        }
        else // Of another quest manager exists, destroy the game object
        {
            Destroy(gameObject); // destroy the game object if there is already an Quest Manager
        }
    }
    #endregion

    #region Quest Management

    public bool AcceptQuest(QuestData quest) // Accept a quest
    {
        if (quest == null) 
        {
            if (showDebug) Debug.LogWarning("Quest is null");
            return false;
        }
        if (activeQuests.Contains(quest))
        {
            if (showDebug) Debug.LogWarning("Quest already accepted"); 
            return false; 
        }
        if (completedQuests.Contains(quest))
        {
            if (showDebug) Debug.LogWarning("Quest already completed");
            return false;
        }

        activeQuests.Add(quest); // add the quest to the list
        quest.questStatus = QuestStatus.Active; // set the quest status to active

        OnQuestAccepted?.Invoke(quest); // invoke the quest accepted event

        if (showDebug) Debug.Log($"Quest {quest.questName} accepted");
        return true;
    }

    public bool UpdateObjective(int questID, QuestObjectiveType objectiveType, int objectiveAmount)
    {
        // Find quest by ID
        QuestData quest = activeQuests.FirstOrDefault(q => q.questID == questID); // Find the quest by ID
        if (quest == null)
        {
            if (showDebug) Debug.LogWarning($"Quest with ID {questID} not found");
            return false;
        }

        // Find Matching Objective
        QuestObjective objective = quest.objectives.Find(o => o.objectiveType == objectiveType);
        if (objective == null)
        {
            if (showDebug) Debug.LogWarning($"Objective with type {objectiveType} not found");
            return false;
        }

        // update the objective progress
        objective.currentAmount += objectiveAmount;
        objective.objectiveStatus = QuestObjectiveStatus.InProgress;

        // Check if the objective is completed
        if (objective.currentAmount >= objective.requiredAmount)
        {
            objective.isCompleted = true;
            objective.objectiveStatus = QuestObjectiveStatus.Completed;
        }

        if (quest.objectives.All(o => o.isCompleted)) // Check if all objectives are completed
        {
            CompleteQuest(questID);
        }

        OnQuestUpdated?.Invoke(quest);

        if (showDebug) Debug.Log($"Objective {objective.ObjectiveName} updated");
        return true;
    }

    public bool CompleteQuest(int questID)
    {
        // Check if quest is in the list, if not return
        QuestData quest = activeQuests.FirstOrDefault(q => q.questID == questID); // Find the quest by ID
        if (quest == null)
        {
            if (showDebug) Debug.LogWarning($"Quest with ID {questID} not found");
            return false;
        }

        if (!quest.objectives.All(o => o.isCompleted))
        {
            if (showDebug) Debug.Log($"Quest {quest.questName} objective is not completed");
            return false;
        }

        quest.questStatus = QuestStatus.Completed;

        OnQuestCompleted?.Invoke(quest);

        if (showDebug) Debug.Log($"Quest {quest.questName} completed");
        return true;
    }

    public bool ClaimQuestReward(int questID)
    {
        // Find quest by ID                   
        QuestData quest = activeQuests.FirstOrDefault(q => q.questID == questID); // Find the quest by ID
        if (quest == null)
        {
            if (showDebug) Debug.LogWarning($"Quest with ID {questID} not found");
            return false;
        }
        // Check if quest is completed, if not return
        if (quest.questStatus != QuestStatus.Completed)
        {
            if (showDebug) Debug.LogWarning($"Quest with ID {questID} is not completed");
            return false;
        }
        // Give rewards to player
        quest.rewards.GiveRewards(playerStats, playerInventory);
        // Remove the quest from the list
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        quest.questStatus = QuestStatus.Claimed;

        OnRewardClaimed?.Invoke(quest);

        if (showDebug) Debug.Log($"Quest {quest.questName} claimed");
        return true;
    }

    public List<QuestData> GetActiveQuests()
    {
        // Get all quests that are active
        // Return the quests
        return new List<QuestData>(activeQuests);
    }

    #endregion

}

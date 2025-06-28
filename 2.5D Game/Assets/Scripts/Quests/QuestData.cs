using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "QuestData", menuName = "Quests/QuestData")]
public class QuestData : ScriptableObject, IQuest
{
    [Header("Quest Information")]
    public int questID;
    public string questName;
    public string questDescription;
    public QuestType questType;
    public QuestSubType questSubType;
    public QuestStatus questStatus;
    
    [Header("Quest Requirements")]
    public int requiredLevel;
    public List<QuestData> prerequisiteQuests;
    
    [Header("Quest Objectives")]
    public List<QuestObjective> objectives;
    
    [Header("Quest Rewards")]
    public QuestReward rewards;
    
    [Header("Quest Settings")]
    public bool isRepeatable;
    public bool autoComplete;
    public float timeLimit; // 0 = no time limit
    
    // IQuest Interface Implementation
    int IQuest.QuestID => questID;
    string IQuest.QuestName => questName;
    string IQuest.QuestDescription => questDescription;
    QuestType IQuest.QuestType => questType;
    QuestStatus IQuest.QuestStatus 
    { 
        get => questStatus; 
        set => questStatus = value; 
    }
    QuestSubType IQuest.QuestSubType => questSubType;
    List<IQuest> IQuest.SubQuests => prerequisiteQuests.Select(q => (IQuest)q).ToList();
    string IQuest.ObjectiveName  // Get the objective name
    { 
        get => objectives[0].ObjectiveName;
        set 
        {
            if (objectives.Count > 0)
                objectives[0].ObjectiveName = value;
        }
    }
    List<QuestObjectiveType> IQuest.objectiveTypes 
    { 
        get => objectives.Select(o => o.objectiveType).ToList();
        set 
        {
            // This is complex - you'd need to update individual objectives
            // Probably not what you want
        }
    }
    List<QuestObjectiveStatus> IQuest.objectiveStatuses 
    { 
        get => objectives.Select(o => o.objectiveStatus).ToList(); 
        set 
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                objectives[i].objectiveStatus = value[i];
            }
        }
    }
    bool IQuest.isRepeatable => isRepeatable;
    void IQuest.GiveRewards(PlayerStats playerStats, PlayerInventory playerInventory)
    {
        rewards.GiveRewards(playerStats, playerInventory);
    }
}

[System.Serializable]
public class QuestObjective
{
    public int ObjectiveID;
    public string ObjectiveName;
    public string objectiveDescription;
    public QuestObjectiveType objectiveType;
    public QuestObjectiveStatus objectiveStatus;
    public int requiredAmount;
    public int currentAmount;
    public bool isCompleted;
    public bool isFailed;
    public QuestStatus questStatus;
    public bool isTutorialObjective;
    public string  tutorialInstruction;
    public string requiredAction;
    public TutorialObjectiveType tutorialType;


    public QuestObjective(string name, string description, QuestObjectiveType type, QuestObjectiveStatus status, int requiredAmount, int currentAmount, bool isCompleted)
    {
        ObjectiveName = name;
        objectiveDescription = description;
        objectiveType = type;
        objectiveStatus = status;
        this.requiredAmount = requiredAmount;
        this.currentAmount = currentAmount;
        this.isCompleted = isCompleted;
        isFailed = false;
    }
}

[System.Serializable]
public class QuestReward
{
    [Header("Item Reward")]
    public ItemData itemReward;
    public int itemRewardCount = 1;
    [Header("Experience Reward")]
    public int experienceReward;
    [Header("Currency Reward")]
    public int currencyReward;
    public CurrencyType currencyType;

    public void GiveRewards(PlayerStats playerStats, PlayerInventory playerInventory)
    {
        if (experienceReward > 0)
        {
            playerStats.GainExperience(experienceReward);
        }
        if (currencyReward > 0)
        {
            playerStats.AddCurrency(currencyType, currencyReward);
        }
        if (itemReward != null && itemRewardCount > 0)
        {
            for (int i = 0; i < itemRewardCount; i++)
            {
                playerInventory.AddItem(itemReward);
            }
        }
    }
}

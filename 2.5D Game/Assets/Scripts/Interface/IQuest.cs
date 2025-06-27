using UnityEngine;
using System.Collections.Generic;

public enum QuestType {
    MainQuest,
    SideQuest,
    DailyQuest,
    WeeklyQuest,
    MonthlyQuest,
}
public enum QuestSubType {
    Bounty,
    Tutorial,
    Target,
    Goal,
}
public enum QuestStatus {
    Active,
    Completed,
    Failed,
    Claimed,
}

public enum QuestObjectiveType {
    KillEnemy,
    GatherItem,
    DeliverItem,
    ExploreArea,
    CraftItem,
}

public enum QuestObjectiveStatus {
    NotStarted,
    InProgress,
    Completed,
    Failed,
}



public interface IQuest
{
    // Quest Definition (Fixed)
    int QuestID { get; }
    string QuestName { get; }
    string QuestDescription { get; }
    QuestType QuestType { get; }
    QuestSubType QuestSubType { get; }
    List<IQuest> SubQuests { get; }
    bool isRepeatable { get; }
    
    // Quest State (Changes during gameplay)
    QuestStatus QuestStatus { get; set; }
    string ObjectiveName { get; set; }
    List<QuestObjectiveType> objectiveTypes { get; set; }
    List<QuestObjectiveStatus> objectiveStatuses { get; set; }
    
    void GiveRewards(PlayerStats playerStats, PlayerInventory playerInventory);
}



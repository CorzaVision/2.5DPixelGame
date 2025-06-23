using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LootTable", menuName = "Loot/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        public ItemData item;
        [Range(0f, 1f)]
        public float dropChance = 0.1f; // 10% chance by default
        public int minCount = 1;
        public int maxCount = 1;
        public int minLevel = 1;
        public int maxLevel = 100;
        [Tooltip("If true, this item will always drop if conditions are met")]
        public bool guaranteed = false;
        
        [Header("Rarity Settings")]
        [Tooltip("Minimum rarity required for this item to drop")]
        public ItemRarity minimumRarity = ItemRarity.Common;
        [Tooltip("Bonus drop chance multiplier for higher rarities")]
        public float rarityBonusMultiplier = 1.5f;
    }

    [Header("Loot Table Settings")]
    public string lootTableName = "Default Loot Table";
    public List<LootEntry> possibleLoot = new List<LootEntry>();
    
    [Header("Drop Settings")]
    [Range(0, 10)]
    public int minItemsToDrop = 0;
    [Range(0, 10)]
    public int maxItemsToDrop = 3;
    
    [Header("Level Scaling")]
    [Tooltip("Should drop chances scale with enemy level?")]
    public bool scaleWithLevel = true;
    [Tooltip("Multiplier for drop chances per level")]
    public float levelMultiplier = 0.1f;
    
    [Header("Rarity Settings")]
    [Tooltip("Base chance for each rarity tier to be considered")]
    [Range(0f, 1f)]
    public float commonChance = 1.0f;
    [Range(0f, 1f)]
    public float uncommonChance = 0.3f;
    [Range(0f, 1f)]
    public float rareChance = 0.1f;
    [Range(0f, 1f)]
    public float epicChance = 0.05f;
    
    [Tooltip("Should rarity chances scale with enemy level?")]
    public bool scaleRarityWithLevel = true;
    [Tooltip("Rarity chance multiplier per level")]
    public float rarityLevelMultiplier = 0.05f;

    [Header("Debug Settings")]
    public bool debugMode = false;
}

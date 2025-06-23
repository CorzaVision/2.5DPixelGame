using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines a loot table for enemies or other loot sources.
/// This class manages drop chances, rarity scaling, and level-based modifications.
/// </summary>
[CreateAssetMenu(fileName = "LootTable", menuName = "Loot/Loot Table")]
public class LootTable : ScriptableObject
{
    /// <summary>
    /// Represents a single loot entry with item data and drop conditions.
    /// </summary>
    [System.Serializable]
    public class LootEntry
    {
        [Header("Item Data")]
        public ItemData item;

        [Header("Drop Conditions")]
        [Range(0f, 1f)]
        [Tooltip("Base drop chance for this item (0.1 = 10%)")]
        public float dropChance = 0.1f;
        
        [Header("Item Count")]
        [Tooltip("Minimum number of items to drop")]
        public int minCount = 1;
        [Tooltip("Maximum number of items to drop")]
        public int maxCount = 1;

        [Header("Level Requirements")]
        [Tooltip("Minimum enemy level required for this item to drop")]
        public int minLevel = 1;
        [Tooltip("Maximum enemy level for this item to drop")]
        public int maxLevel = 100;

        [Header("Drop Behavior")]
        [Tooltip("If true, this item will always drop if conditions are met")]
        public bool guaranteed = false;

        [Header("Rarity Settings")]
        [Tooltip("Minimum rarity required for this item to drop")]
        public ItemRarity minimumRarity = ItemRarity.Common;
        [Tooltip("Bonus drop chance multiplier for higher rarities")]
        public float rarityBonusMultiplier = 1.5f;
    }

    [Header("Basic Settings")]
    [Tooltip("Name of this loot table for identification")]
    public string lootTableName = "Default Loot Table";

    [Header("Loot Entries")]
    [Tooltip("List of all possible items that can drop from this loot table")]
    public List<LootEntry> possibleLoot = new List<LootEntry>();

    [Header("Drop Settings")]
    [Range(0, 10)]
    [Tooltip("Minimum number of items to attempt to drop")]
    public int minItemsToDrop = 0;
    [Range(0, 10)]
    [Tooltip("Maximum number of items to attempt to drop")]
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

    [Header("Rarity Level Scaling")]
    [Tooltip("Should rarity chances scale with enemy level?")]
    public bool scaleRarityWithLevel = true;
    [Tooltip("Rarity chance multiplier per level")]
    public float rarityLevelMultiplier = 0.05f;

    [Header("Debug Settings")]
    [Tooltip("Enable debug logging for this loot table")]
    public bool debugMode = false;
}

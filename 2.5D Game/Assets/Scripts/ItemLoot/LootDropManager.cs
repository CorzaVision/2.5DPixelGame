using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages loot drops from enemies and other sources in the game.
/// This script handles loot generation, rarity rolls, and loot bag creation.
/// </summary>
public class LootDropManager : MonoBehaviour
{
    [Header("Loot Drop Settings")]
    [SerializeField] private GameObject lootBagPrefab;
    [SerializeField] private float dropRadius = 2f;
    [SerializeField] private bool showDebug = true;

    // Singleton Instance
    public static LootDropManager Instance { get; private set; }

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the singleton pattern for the LootDropManager.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Drops loot from an enemy based on their loot table and level.
    /// </summary>
    /// <param name="lootTable">The loot table to generate items from.</param>
    /// <param name="enemyPosition">The position where the enemy died.</param>
    /// <param name="enemyLevel">The level of the enemy.</param>
    public void DropLootFromEnemy(LootTable lootTable, Vector3 enemyPosition, int enemyLevel)
    {
        if (lootTable == null)
        {
            if (showDebug) 
            {
                Debug.LogWarning("DropLootFromEnemy called with a null loot table!");
            }
            return;
        }

        List<ItemInstance> droppedItems = GenerateLoot(lootTable, enemyLevel);

        if (droppedItems.Count > 0)
        {
            CreateLootBag(droppedItems, enemyPosition);
            if (showDebug) 
            {
                Debug.Log($"Dropped {droppedItems.Count} items from enemy at {enemyPosition}");
            }
        }
        else
        {
            if (showDebug) 
            {
                Debug.Log("No items dropped from enemy based on loot table rolls.");
            }
        }
    }

    #endregion

    #region Loot Generation

    /// <summary>
    /// Generates loot items based on the loot table and enemy level.
    /// </summary>
    /// <param name="lootTable">The loot table to generate from.</param>
    /// <param name="enemyLevel">The level of the enemy.</param>
    /// <returns>List of generated item instances.</returns>
    private List<ItemInstance> GenerateLoot(LootTable lootTable, int enemyLevel)
    {
        List<ItemInstance> generatedLoot = new List<ItemInstance>();

        // Add guaranteed drops first
        AddGuaranteedDrops(lootTable, enemyLevel, generatedLoot);

        // Add random drops
        AddRandomDrops(lootTable, enemyLevel, generatedLoot);
        
        return generatedLoot;
    }

    /// <summary>
    /// Adds guaranteed drops to the loot list.
    /// </summary>
    /// <param name="lootTable">The loot table to check.</param>
    /// <param name="enemyLevel">The enemy level.</param>
    /// <param name="generatedLoot">The list to add items to.</param>
    private void AddGuaranteedDrops(LootTable lootTable, int enemyLevel, List<ItemInstance> generatedLoot)
    {
        var guaranteedDrops = lootTable.possibleLoot
            .Where(e => e.guaranteed && enemyLevel >= e.minLevel && enemyLevel <= e.maxLevel);
        
        foreach (var entry in guaranteedDrops)
        {
            int count = Random.Range(entry.minCount, entry.maxCount + 1);
            if (count > 0)
            {
                generatedLoot.Add(new ItemInstance(entry.item, count));
            }
        }
    }

    /// <summary>
    /// Adds random drops to the loot list based on rarity rolls.
    /// </summary>
    /// <param name="lootTable">The loot table to check.</param>
    /// <param name="enemyLevel">The enemy level.</param>
    /// <param name="generatedLoot">The list to add items to.</param>
    private void AddRandomDrops(LootTable lootTable, int enemyLevel, List<ItemInstance> generatedLoot)
    {
        int itemsToDrop = Random.Range(lootTable.minItemsToDrop, lootTable.maxItemsToDrop + 1);

        for (int i = 0; i < itemsToDrop; i++)
        {
            // First roll: Determine rarity
            ItemRarity rolledRarity = RollForRarity(lootTable, enemyLevel);

            // Get eligible items for this rarity
            var eligibleItems = lootTable.possibleLoot
                .Where(e => !e.guaranteed && e.item.itemRarity == rolledRarity && 
                           enemyLevel >= e.minLevel && enemyLevel <= e.maxLevel)
                .ToList();
            
            if (eligibleItems.Any())
            {
                // Second roll: Pick an item and check drop chance
                LootTable.LootEntry selectedEntry = eligibleItems[Random.Range(0, eligibleItems.Count)];

                if (Random.Range(0f, 1f) <= CalculateDropChance(selectedEntry, lootTable, enemyLevel))
                {
                    int itemCount = Random.Range(selectedEntry.minCount, selectedEntry.maxCount + 1);
                    if (itemCount > 0)
                    {
                        generatedLoot.Add(new ItemInstance(selectedEntry.item, itemCount));
                    }
                }
            }
        }
    }

    #endregion

    #region Rarity System

    /// <summary>
    /// Rolls for item rarity based on loot table settings and enemy level.
    /// </summary>
    /// <param name="lootTable">The loot table containing rarity chances.</param>
    /// <param name="enemyLevel">The level of the enemy.</param>
    /// <returns>The rolled item rarity.</returns>
    private ItemRarity RollForRarity(LootTable lootTable, int enemyLevel)
    {
        float epicChance = lootTable.epicChance;
        float rareChance = lootTable.rareChance;
        float uncommonChance = lootTable.uncommonChance;

        if (lootTable.scaleRarityWithLevel)
        {
            ApplyLevelScalingToRarity(lootTable, enemyLevel, ref epicChance, ref rareChance, ref uncommonChance);
        }

        return DetermineRarityFromRoll(epicChance, rareChance, uncommonChance);
    }

    /// <summary>
    /// Applies level scaling to rarity chances.
    /// </summary>
    /// <param name="lootTable">The loot table.</param>
    /// <param name="enemyLevel">The enemy level.</param>
    /// <param name="epicChance">Reference to epic chance.</param>
    /// <param name="rareChance">Reference to rare chance.</param>
    /// <param name="uncommonChance">Reference to uncommon chance.</param>
    private void ApplyLevelScalingToRarity(LootTable lootTable, int enemyLevel, 
        ref float epicChance, ref float rareChance, ref float uncommonChance)
    {
        float levelBonus = (enemyLevel - 1) * lootTable.rarityLevelMultiplier;
        epicChance += levelBonus * 0.5f;
        rareChance += levelBonus * 0.75f;
        uncommonChance += levelBonus;
    }

    /// <summary>
    /// Determines the rarity based on rolled values.
    /// </summary>
    /// <param name="epicChance">Epic rarity chance.</param>
    /// <param name="rareChance">Rare rarity chance.</param>
    /// <param name="uncommonChance">Uncommon rarity chance.</param>
    /// <returns>The determined item rarity.</returns>
    private ItemRarity DetermineRarityFromRoll(float epicChance, float rareChance, float uncommonChance)
    {
        float roll = Random.Range(0f, 1f);

        if (roll <= epicChance) return ItemRarity.Epic;
        if (roll <= rareChance) return ItemRarity.Rare;
        if (roll <= uncommonChance) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }

    /// <summary>
    /// Calculates the final drop chance for an item entry.
    /// </summary>
    /// <param name="entry">The loot entry to calculate for.</param>
    /// <param name="lootTable">The loot table.</param>
    /// <param name="enemyLevel">The enemy level.</param>
    /// <returns>The calculated drop chance.</returns>
    private float CalculateDropChance(LootTable.LootEntry entry, LootTable lootTable, int enemyLevel)
    {
        float finalChance = entry.dropChance;
        
        if (lootTable.scaleWithLevel)
        {
            finalChance += (enemyLevel - 1) * lootTable.levelMultiplier;
        }
        
        return Mathf.Clamp01(finalChance);
    }

    #endregion

    #region Loot Bag Creation

    /// <summary>
    /// Creates a loot bag at the specified position with the given items.
    /// </summary>
    /// <param name="items">The items to put in the loot bag.</param>
    /// <param name="position">The position to create the loot bag at.</param>
    private void CreateLootBag(List<ItemInstance> items, Vector3 position)
    {
        if (lootBagPrefab == null)
        {
            Debug.LogError("Loot bag prefab not assigned in LootDropManager!");
            return;
        }

        Vector3 dropPosition = CalculateDropPosition(position);
        GameObject lootBagObject = Instantiate(lootBagPrefab, dropPosition, Quaternion.identity);
        
        SetupLootBag(lootBagObject, items);
    }

    /// <summary>
    /// Calculates a random drop position within the drop radius.
    /// </summary>
    /// <param name="basePosition">The base position to drop from.</param>
    /// <returns>The calculated drop position.</returns>
    private Vector3 CalculateDropPosition(Vector3 basePosition)
    {
        return basePosition + new Vector3(
            Random.Range(-dropRadius, dropRadius), 
            0, 
            Random.Range(-dropRadius, dropRadius)
        );
    }

    /// <summary>
    /// Sets up the loot bag with the specified items.
    /// </summary>
    /// <param name="lootBagObject">The loot bag GameObject.</param>
    /// <param name="items">The items to add to the bag.</param>
    private void SetupLootBag(GameObject lootBagObject, List<ItemInstance> items)
    {
        InteractableBag interactableBag = lootBagObject.GetComponent<InteractableBag>();

        if (interactableBag != null)
        {
            interactableBag.lootbag = new Bag(items);
        }
        else
        {
            Debug.LogError("InteractableBag component not found on the Loot Bag Prefab!");
        }
    }

    #endregion
}

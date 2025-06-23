using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootDropManager : MonoBehaviour
{
    [Header("Loot Drop Settings")]
    [SerializeField] private GameObject lootBagPrefab;
    [SerializeField] private float dropRadius = 2f;
    [SerializeField] private bool showDebug = true;

    public static LootDropManager Instance { get; private set; }

    private void Awake()
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

    public void DropLootFromEnemy(LootTable lootTable, Vector3 enemyPosition, int enemyLevel)
    {
        if (lootTable == null)
        {
            if (showDebug) Debug.LogWarning("DropLootFromEnemy called with a null loot table!");
            return;
        }

        List<ItemInstance> droppedItems = GenerateLoot(lootTable, enemyLevel);

        if (droppedItems.Count > 0)
        {
            CreateLootBag(droppedItems, enemyPosition);
            if (showDebug) Debug.Log($"Dropped {droppedItems.Count} items from enemy at {enemyPosition}");
        }
        else
        {
            if (showDebug) Debug.Log("No items dropped from enemy based on loot table rolls.");
        }
    }

    private List<ItemInstance> GenerateLoot(LootTable lootTable, int enemyLevel)
    {
        List<ItemInstance> generatedLoot = new List<ItemInstance>();

        // 1. Add guaranteed drops first. These ignore rarity rolls.
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

        // 2. Determine how many random items we should try to drop.
        int itemsToDrop = Random.Range(lootTable.minItemsToDrop, lootTable.maxItemsToDrop + 1);

        for (int i = 0; i < itemsToDrop; i++)
        {
            // 3. FIRST ROLL: Determine the rarity of this specific drop.
            ItemRarity rolledRarity = RollForRarity(lootTable, enemyLevel);

            // 4. Get all items that match the rolled rarity and enemy level.
            var eligibleItems = lootTable.possibleLoot
                .Where(e => !e.guaranteed && e.item.itemRarity == rolledRarity && enemyLevel >= e.minLevel && enemyLevel <= e.maxLevel)
                .ToList();
            
            if (eligibleItems.Any())
            {
                // 5. SECOND ROLL: Pick one item from the eligible list.
                LootTable.LootEntry selectedEntry = eligibleItems[Random.Range(0, eligibleItems.Count)];

                // 6. Roll for its individual drop chance.
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
        
        return generatedLoot;
    }

    private ItemRarity RollForRarity(LootTable lootTable, int enemyLevel)
    {
        float epicChance = lootTable.epicChance;
        float rareChance = lootTable.rareChance;
        float uncommonChance = lootTable.uncommonChance;

        if (lootTable.scaleRarityWithLevel)
        {
            float levelBonus = (enemyLevel - 1) * lootTable.rarityLevelMultiplier;
            epicChance += levelBonus * 0.5f;
            rareChance += levelBonus * 0.75f;
            uncommonChance += levelBonus;
        }

        float roll = Random.Range(0f, 1f);

        if (roll <= epicChance) return ItemRarity.Epic;
        if (roll <= rareChance) return ItemRarity.Rare;
        if (roll <= uncommonChance) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }

    private float CalculateDropChance(LootTable.LootEntry entry, LootTable lootTable, int enemyLevel)
    {
        float finalChance = entry.dropChance;
        if (lootTable.scaleWithLevel)
        {
            finalChance += (enemyLevel - 1) * lootTable.levelMultiplier;
        }
        return Mathf.Clamp01(finalChance);
    }

    private void CreateLootBag(List<ItemInstance> items, Vector3 position)
    {
        if (lootBagPrefab == null)
        {
            Debug.LogError("Loot bag prefab not assigned in LootDropManager!");
            return;
        }

        Vector3 dropPosition = position + new Vector3(Random.Range(-dropRadius, dropRadius), 0, Random.Range(-dropRadius, dropRadius));

        GameObject lootBagObject = Instantiate(lootBagPrefab, dropPosition, Quaternion.identity);
        InteractableBag interactableBag = lootBagObject.GetComponent<InteractableBag>();

        if (interactableBag != null)
        {
            interactableBag.lootbag = new Bag(items); // Use a constructor to pass the items
        }
        else
        {
            Debug.LogError("InteractableBag component not found on the Loot Bag Prefab!");
        }
    }
}

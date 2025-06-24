using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines the properties and stats of an item in the game.
/// This class contains all the data needed for weapons, armor, consumables, and quest items.
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Item Information")]
    public int itemID;
    public string itemName;
    public string itemDescription;
    public Texture icon;
    public int itemLevel;
    public ItemRarity itemRarity;

    [Header("Item Properties")]
    public int count = 1;
    public int maxCount = 1;
    public ItemType itemType;
    public bool isStackable => itemType == ItemType.Consumable || itemType == ItemType.Currency;
    public bool isEquippable => itemType == ItemType.Weapon || itemType == ItemType.Armor || itemType == ItemType.Bag;

    [Header("Item Subtypes")]
    public WeaponSubType weaponSubType;
    public ArmorSubType armorSubType;
    public ConsumableSubType consumableSubType;
    public QuestSubType questSubType;
    public CurrencySubType currencySubType;
    public CraftingMaterialSubType craftingMaterialSubType;

    [Header("Weapon Properties")]
    public WeaponType weaponType;
    public WeaponHand weaponHand;
    public WeaponWeight weaponWeight;

    [Header("Armor Properties")]
    public ArmorWeight armorWeight;

    [Header("Weapon Stats")]
    public int damage;
    public int weaponCritChance;
    public int weaponCritDamage;

    [Header("Defense & Armor Stats")]
    public int armorRating;
    public int defenseRating;
    public int magicDefenseRating;
    public int staminaRating;
    public int healthRating;
    public int manaRating;
    

    [Header(" Offense & Armor Stats")]
    public int damageBonus;
    public int critChanceBonus;
    public int critDamageBonus;
    public int attackSpeedBonus;

    [Header("Consumable Stats")]
    public int healthRestore;
    public int manaRestore;
    public int staminaRestore;
    public int critChanceRestore;
    public PotionType potionType;
    public int potionCount;
    public int potionMaxCount;

    [Header("Quest Stats")]
    public int questID;
    public int questProgress;
    public int questProgressMax;
    public int questReward;
    public int questRewardMax;
    public int questRewardMin;

    [Header("Currency Stats")]
    public CurrencyType currencyType;
    public int currencyValue = 1;
    public int currencyMinValue = 1;
    public int currencyMaxValue = 1;

    [Header("Bag Properties")]
    public bool isBag = false;
    public BagData bagDataReference;

    [Header("Crafting Material Properties")]
    public CraftingMaterialTier craftingMaterialTier; // 1 = basic, 2 = advanced, 3 = expert, 4 = master, 5 = Unique
    public int craftingMaterialValue = 1;
    public int craftingMaterialMinValue = 1;
    public int craftingMaterialMaxValue = 1;

    /// <summary>
    /// Calculates the total value of this weapon including rarity multiplier.
    /// </summary>
    /// <returns>The calculated total weapon value.</returns>
    public int GetWeaponValue()
    {
        int statValue = damage * 5 + weaponCritDamage * 2 + weaponCritChance * 3;
        int rarityMultiplier = GetRarityMultiplier();
        return statValue * rarityMultiplier;
    }

    /// <summary>
    /// Calculates the total value of this armor including rarity multiplier.
    /// </summary>
    /// <returns>The calculated total armor value.</returns>
    public int GetArmorValue()
    {
        int defensiveValue = armorRating * 5 + healthRating * 3 + defenseRating * 2 + magicDefenseRating * 2;
        int offensiveValue = damageBonus * 4 + critChanceBonus * 3 + critDamageBonus * 2 + attackSpeedBonus * 2;
        int totalValue = defensiveValue + offensiveValue;
        int rarityMultiplier = GetRarityMultiplier();
        return totalValue * rarityMultiplier;
    }

    /// <summary>
    /// Calculates the total value of this consumable.
    /// </summary>
    /// <returns>The calculated total consumable value.</returns>
    public int GetConsumableValue()
    {
        int baseValue = healthRestore * 2 + manaRestore * 2 + staminaRestore * 2;
        int potionBonus = potionCount > 1 ? potionCount * 10 : 0;
        return baseValue + potionBonus;
    }

    /// <summary>
    /// Gets the value multiplier based on item rarity.
    /// </summary>
    /// <returns>The multiplier value for the rarity.</returns>
    private int GetRarityMultiplier()
    {
        switch (itemRarity)
        {
            case ItemRarity.Common:
                return 1;
            case ItemRarity.Uncommon:
                return 2;
            case ItemRarity.Rare:
                return 4;
            case ItemRarity.Epic:
                return 8;
            default:
                return 1;
        }
    }
}
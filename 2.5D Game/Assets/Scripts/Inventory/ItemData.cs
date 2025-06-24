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
    public bool isEquippable => itemType == ItemType.Weapon || itemType == ItemType.Armor;

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

    [Header("Crafting Material Properties")]
    public CraftingMaterialTier craftingMaterialTier; // 1 = basic, 2 = advanced, 3 = expert, 4 = master, 5 = Unique
    public int craftingMaterialValue = 1;
    public int craftingMaterialMinValue = 1;
    public int craftingMaterialMaxValue = 1;
}
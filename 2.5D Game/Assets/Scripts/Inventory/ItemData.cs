using UnityEngine;

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
    public bool isStackable => itemType == ItemType.Consumable;
    public bool isEquippable => itemType == ItemType.Weapon || itemType == ItemType.Armor;

    [Header("Item Subtypes")]
    public WeaponSubType weaponSubType;
    public ArmorSubType armorSubType;
    public ConsumableSubType consumableSubType;
    public QuestSubType questSubType;

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

    [Header("Armor Stats")]
    public int armor;
    public int health;
    public int mana;
    public int stamina;
    public int armorCritChance;
    public int armorCritDamage;

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
}

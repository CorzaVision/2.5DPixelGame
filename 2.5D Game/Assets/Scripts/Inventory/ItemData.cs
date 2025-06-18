using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public int itemID;
    public int count;
    public int maxCount;
    public int itemLevel;
    public string itemName;
    public string itemDescription;
    public string itemIcon;
    [Header("Item Type")]

    public ItemType itemType;
    public WeaponSubType weaponSubType;
    public ArmorSubType armorSubType;
    public ConsumableSubType consumableSubType;
    public QuestSubType questSubType;
    public WeaponType weaponType;
    public WeaponHand weaponHand;
    public ArmorWeight armorWeight;
    public WeaponWeight weaponWeight;

    [Header("Item Rarity")]
    public ItemRarity itemRarity;

    [Header("Consumable Stats")]
    public int healthRestore;
    public int manaRestore;
    public int staminaRestore;
    public int critChanceRestore;

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

    [Header("Quest Stats")]
    public int questID;
    public int questProgress;
    public int questProgressMax;
    public int questReward;
    public int questRewardMax;
    public int questRewardMin;

}

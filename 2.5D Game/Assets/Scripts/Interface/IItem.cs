using UnityEngine;

   public enum ItemType
   {
    Weapon,
    Armor,
    Consumable,
    Quest,
   }
   public enum WeaponSubType
   {
    Sword,
    Bow,
    Axe,
    Shield,
   }

   public enum ArmorSubType
   {
    Helmet,
    Chestplate,
    Leggings,
    Boots,
   }

   public enum ConsumableSubType
   {
    Potion,
    Food,
    Scroll,
   }

   public enum QuestSubType
   {
    Quest,
    QuestItem,
   }


   public enum ItemCategory
   {
    Weapon,
    Armor,
    Consumable,
    Quest,
   }
   public enum ItemRarity
   {
    Common, 
    Uncommon,
    Rare,
    Epic,
   }

   public enum ArmorWeight
   {
    Light,
    Medium,
    Heavy,
   }

   public enum WeaponWeight
   {
    Light,
    Medium,
    Heavy,
   }

   public enum WeaponType
   {
    Melee,
    Ranged,
    Magic,
   }

   public enum WeaponHand
   {
    OneHanded,
    TwoHanded,
   }

   public enum PotionType
   {
    Health,
    Mana,
    Stamina,
   }

    public interface IItem
    {
        int itemID { get; set; }
        int count { get; set; }
        int maxCount { get; set; }
        int itemLevel { get; set; }
        string itemName { get; set; }
        string itemDescription { get; set; }
        string itemIcon { get; set; }
        ItemType itemType { get; set; }
        ItemRarity itemRarity { get; set; }
        WeaponSubType weaponSubType { get; set; }
        ArmorSubType armorSubType { get; set; }
        ConsumableSubType consumableSubType { get; set; }
        QuestSubType questSubType { get; set; }
        WeaponType weaponType { get; set; }
        WeaponHand weaponHand { get; set; }
        ArmorWeight armorWeight { get; set; }
        WeaponWeight weaponWeight { get; set; }
        PotionType potionType { get; set; }
        int potionCount { get; set; }
        int potionMaxCount { get; set; }
    }


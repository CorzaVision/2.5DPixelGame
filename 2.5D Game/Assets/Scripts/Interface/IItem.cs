using UnityEngine;

   public enum ItemType
   {
    Weapon,
    Armor,
    Consumable,
    Quest,
   }
   public enum ItemSubType
   {
    Sword,
    Bow,
    Axe,
    Shield,
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
        ItemSubType itemSubType { get; set; }
        ItemRarity itemRarity { get; set; }
    }


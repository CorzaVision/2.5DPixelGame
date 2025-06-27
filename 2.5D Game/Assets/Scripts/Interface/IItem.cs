using UnityEngine;

   public enum ItemType
   {
    Weapon,
    Armor,
    Consumable,
    Currency,
    CraftingMaterial,
    Bag,
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
   public enum ItemCategory
   {
    Weapon,
    Armor,
    Consumable,
    Currency,
    CraftingMaterial,
    Bag,
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

   public enum CurrencyType
   {
    Copper,
    Silver,
    Gold,
   }

   public enum CurrencySubType
   {
    Gold,
    Silver,
    Copper
   }
   public enum CraftingMaterialSubType
   {
    Ore,
    Ingot,
    Wood,
    Herb,
    Gem,
    Cloth,
    Leather,
    Hide,
    Feather,
    Bone,
    Misc,
   }
   public enum CraftingMaterialTier
   {
    Basic = 1,
    Advanced = 2,
    Expert = 3,
    Master = 4,
    Unique = 5,
   }
   public enum BagTier
   {
    Basic = 1,
    Advanced = 2,
    Expert = 3,
    Unique = 4,
   }
   public enum BagType
   {
    Small,
    Medium,
    Large,
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
        WeaponType weaponType { get; set; }
        WeaponHand weaponHand { get; set; }
        ArmorWeight armorWeight { get; set; }
        WeaponWeight weaponWeight { get; set; }
        PotionType potionType { get; set; }
        int potionCount { get; set; }
        int potionMaxCount { get; set; }
        CurrencyType currencyType { get; set; }
        int currencyMaxValue { get; set; }
        int currencyMinValue { get; set; }
        int currencyValue { get; set; }
        CurrencySubType currencySubType { get; set; }
        CraftingMaterialSubType craftingMaterialSubType { get; set; }
        CraftingMaterialTier craftingMaterialTier { get; set; }
        int craftingMaterialValue { get; set; }
        int craftingMaterialMinValue { get; set; }
        int craftingMaterialMaxValue { get; set; }
        BagData bagDataReference { get; set; }
        bool isBag { get; set; }
    }


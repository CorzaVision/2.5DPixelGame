using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public int count;
    public int maxCount;
    public int itemLevel;
    public string itemName;
    public string itemDescription;
    public string itemIcon;
    public ItemType itemType;
    public ItemSubType itemSubType;
    public ItemRarity itemRarity;
}

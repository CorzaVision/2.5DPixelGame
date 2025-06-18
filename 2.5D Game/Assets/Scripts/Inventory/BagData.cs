using UnityEngine;

[CreateAssetMenu(fileName = "BagData", menuName = "Inventory/Bag")]
public class BagData : ScriptableObject
{
    public string bagName;
    public int bagID;
    public int slotCount;
    public BagType bagType;

    public enum BagType
    {
        Backpack,
        Chest,
        Death,
        LootBag,
    }
}

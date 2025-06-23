using UnityEngine;

/// <summary>
/// ScriptableObject that defines the properties of a bag type in the game.
/// This class contains metadata about different types of bags and their capacities.
/// </summary>
[CreateAssetMenu(fileName = "BagData", menuName = "Inventory/Bag")]
public class BagData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Display name of the bag")]
    public string bagName;
    
    [Tooltip("Unique identifier for this bag type")]
    public int bagID;
    
    [Header("Bag Properties")]
    [Tooltip("Number of item slots this bag can hold")]
    public int slotCount;
    
    [Tooltip("Type of bag this represents")]
    public BagType bagType;

    /// <summary>
    /// Enumeration of different bag types in the game.
    /// </summary>
    public enum BagType
    {
        /// <summary>Player's main inventory bag</summary>
        Backpack,
        /// <summary>Storage container for items</summary>
        Chest,
        /// <summary>Bag that appears when player dies</summary>
        Death,
        /// <summary>Loot bag dropped by enemies</summary>
        LootBag,
    }
}

using UnityEngine;
using System;

/// <summary>
/// Represents an instance of an item in the game, containing the item data and stack count.
/// This class provides unique identification for each item instance and handles stacking.
/// </summary>
[System.Serializable]
public class ItemInstance
{
    [Header("Item Instance Data")]
    public string uniqueId;
    public ItemData itemData;
    public int count;

    #region Constructors

    /// <summary>
    /// Parameterless constructor for Unity serialization.
    /// Creates an empty item instance with a new unique ID.
    /// </summary>
    public ItemInstance()
    {
        uniqueId = Guid.NewGuid().ToString();
        count = 1;
        itemData = null;
    }

    /// <summary>
    /// Constructor for creating new item instances with specified data and count.
    /// </summary>
    /// <param name="itemData">The item data for this instance.</param>
    /// <param name="count">The number of items in this stack (default: 1).</param>
    public ItemInstance(ItemData itemData, int count = 1)
    {
        uniqueId = Guid.NewGuid().ToString();
        this.itemData = itemData;
        this.count = count;
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Checks if this item instance can be stacked with another instance.
    /// </summary>
    /// <param name="other">The other item instance to check.</param>
    /// <returns>True if the items can be stacked, false otherwise.</returns>
    public bool CanStackWith(ItemInstance other)
    {
        return itemData != null && 
               other.itemData != null && 
               itemData.itemID == other.itemData.itemID && 
               itemData.isStackable;
    }

    /// <summary>
    /// Gets the maximum stack size for this item.
    /// </summary>
    /// <returns>The maximum number of items that can be in this stack.</returns>
    public int GetMaxStackSize()
    {
        return itemData?.maxCount ?? 1;
    }

    /// <summary>
    /// Checks if this stack is full.
    /// </summary>
    /// <returns>True if the stack is at maximum capacity, false otherwise.</returns>
    public bool IsStackFull()
    {
        return count >= GetMaxStackSize();
    }

    #endregion
}
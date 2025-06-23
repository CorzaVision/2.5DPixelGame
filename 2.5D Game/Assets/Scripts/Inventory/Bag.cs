using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a bag that can hold items, either as a player's bag or a loot container.
/// This class manages item storage and provides methods for adding and removing items.
/// </summary>
[System.Serializable]
public class Bag
{
    [Header("Bag Data")]
    public BagData bagData;
    public List<ItemInstance> items = new List<ItemInstance>();

    #region Constructors

    /// <summary>
    /// Default constructor for Unity serialization.
    /// </summary>
    public Bag() { }

    /// <summary>
    /// Constructor that creates a bag with specified bag data.
    /// </summary>
    /// <param name="data">The bag data to associate with this bag.</param>
    public Bag(BagData data)
    {
        bagData = data;
        items = new List<ItemInstance>();
    }

    /// <summary>
    /// Constructor that creates a bag with a list of items (typically for loot bags).
    /// </summary>
    /// <param name="items">The items to add to the bag.</param>
    public Bag(List<ItemInstance> items)
    {
        bagData = null;
        this.items = items;
    }

    #endregion

    #region Item Management

    /// <summary>
    /// Adds an item to the bag.
    /// </summary>
    /// <param name="item">The item instance to add.</param>
    public void AddItem(ItemInstance item)
    {
        items.Add(item);
    }

    /// <summary>
    /// Removes an item from the bag.
    /// </summary>
    /// <param name="item">The item instance to remove.</param>
    public void RemoveItem(ItemInstance item)
    {
        items.Remove(item);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Gets the number of items in the bag.
    /// </summary>
    /// <returns>The count of items in the bag.</returns>
    public int GetItemCount()
    {
        return items.Count;
    }

    /// <summary>
    /// Checks if the bag is empty.
    /// </summary>
    /// <returns>True if the bag has no items, false otherwise.</returns>
    public bool IsEmpty()
    {
        return items.Count == 0;
    }

    /// <summary>
    /// Clears all items from the bag.
    /// </summary>
    public void Clear()
    {
        items.Clear();
    }

    #endregion
}

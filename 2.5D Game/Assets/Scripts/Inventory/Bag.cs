using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a bag that can hold items, either as a player's bag or a loot container.
/// This class manages item storage and provides methods for adding and removing items.
/// </summary>
[System.Serializable]
public class Bag
{
    [Header("Bag Data")]
    public BagData bagData;
    public List<ItemInstance> slots;
    public ItemInstance bagItem;

    public Bag(List<ItemInstance> items)
    {
        bagData = null;
        slots = new List<ItemInstance>(new ItemInstance[items.Count]);
        for (int i = 0; i < items.Count; i++)
        {
            slots[i] = items[i];
        }
    }


    #region Item Management

    /// <summary>
    /// Adds an item to the bag.
    /// </summary>
    /// <param name="item">The item instance to add.</param>
    public bool AddItem(ItemInstance item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                return true;
            }
        }
        return false; // Bag is full
    }

    /// <summary>
    /// Removes an item from the bag.
    /// </summary>
    /// <param name="slotIndex">The index of the item to remove.</param>
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
            slots[slotIndex] = null;
    }

    /// <summary>
    /// Gets all items in the bag.
    /// </summary>
    /// <returns>A list of all items in the bag.</returns>
    public List<ItemInstance> GetAllItems()
    {
        List<ItemInstance> items = new List<ItemInstance>();
        foreach (var slot in slots)
            if (slot != null) items.Add(slot);
        return items;
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Gets the number of items in the bag.
    /// </summary>
    /// <returns>The count of items in the bag.</returns>
    public int GetItemCount()
    {
        return slots.Count;
    }

    /// <summary>
    /// Checks if the bag is empty.
    /// </summary>
    /// <returns>True if the bag has no items, false otherwise.</returns>
    public bool IsEmpty()
    {
        return slots.All(item => item == null);
    }

    /// <summary>
    /// Clears all items from the bag.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] = null;
        }
    }

    #endregion
}

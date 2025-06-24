using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using System.Linq;

/// <summary>
/// Manages the player's inventory system, including items, bags, and UI interactions.
/// This script handles item storage, stacking, and inventory display.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    #region Serialized Fields

    [Header("Inventory Data")]
    [Tooltip("List of item instances in the player's inventory")]
    public List<ItemInstance> items = new List<ItemInstance>();
    
    [Header("UI Controller")]
    [Tooltip("Reference to the inventory UI controller")]
    public InventoryUIController inventoryUIController;

    [Header("Bag System")]
    [Tooltip("List of bags the player owns")]
    public List<Bag> bags = new List<Bag>();
    [Tooltip("Maximum number of bags the player can carry")]
    public int maxBags = 2;

    #endregion

    #region Item Management

    /// <summary>
    /// Adds an item to the player's inventory with proper stacking logic.
    /// </summary>
    /// <param name="item">The item data to add.</param>
    public void AddItem(ItemData item)
    {
        if (item.isStackable)
        {
            AddStackableItem(item);
        }
        else
        {
            AddNonStackableItem(item);
        }
    }

    /// <summary>
    /// Adds a stackable item to the inventory with stacking logic.
    /// </summary>
    /// <param name="item">The stackable item to add.</param>
    private void AddStackableItem(ItemData item)
    {
        var existing = items.Find(i => i.itemData.itemID == item.itemID);
        if (existing != null)
        {
            existing.count = Mathf.Min(existing.count + item.count, item.maxCount);
            Debug.Log($"Stacked item: {item.itemName} (new count: {existing.count})");
        }
        else
        {
            items.Add(new ItemInstance(item));
            Debug.Log($"Added new stackable item: {item.itemName}");
        }
    }

    /// <summary>
    /// Adds a non-stackable item to the inventory.
    /// </summary>
    /// <param name="item">The non-stackable item to add.</param>
    private void AddNonStackableItem(ItemData item)
    {
        items.Add(new ItemInstance(item));
        Debug.Log($"Added non-stackable item: {item.itemName}");
    }

    /// <summary>
    /// Adds an item instance to the inventory with advanced stacking logic.
    /// </summary>
    /// <param name="itemInstance">The item instance to add.</param>
    public void AddItemInstance(ItemInstance itemInstance)
    {
        if (itemInstance.itemData.isStackable)
        {
            AddStackableItemInstance(itemInstance);
        }
        else
        {
            AddNonStackableItemInstance(itemInstance);
        }
    }

    /// <summary>
    /// Adds a stackable item instance with advanced stacking distribution.
    /// </summary>
    /// <param name="itemInstance">The stackable item instance to add.</param>
    private void AddStackableItemInstance(ItemInstance itemInstance)
    {
        // Find all existing stacks of the same item that are not full
        List<ItemInstance> existingStacks = items.FindAll(i => 
            i.itemData.itemID == itemInstance.itemData.itemID && 
            i.count < i.itemData.maxCount);

        int amountToAdd = itemInstance.count;

        // Distribute the new items into existing, non-full stacks first
        foreach (var stack in existingStacks)
        {
            if (amountToAdd <= 0) break;

            int spaceAvailable = stack.itemData.maxCount - stack.count;
            int amountToTransfer = Mathf.Min(amountToAdd, spaceAvailable);

            stack.count += amountToTransfer;
            amountToAdd -= amountToTransfer;
            Debug.Log($"Added {amountToTransfer} to an existing stack of {stack.itemData.itemName}. New count: {stack.count}");
        }

        // If there are still items left over, create new stacks for them
        while (amountToAdd > 0)
        {
            int amountForNewStack = Mathf.Min(amountToAdd, itemInstance.itemData.maxCount);
            
            ItemInstance newStack = new ItemInstance(itemInstance.itemData, amountForNewStack);
            items.Add(newStack);
            
            amountToAdd -= amountForNewStack;
            Debug.Log($"Created a new stack of {newStack.itemData.itemName} with {newStack.count} items.");
        }
    }

    /// <summary>
    /// Adds a non-stackable item instance to the inventory.
    /// </summary>
    /// <param name="itemInstance">The non-stackable item instance to add.</param>
    private void AddNonStackableItemInstance(ItemInstance itemInstance)
    {
        items.Add(itemInstance);
        Debug.Log($"Added non-stackable item: {itemInstance.itemData.itemName}");
    }

    /// <summary>
    /// Removes an item instance from the inventory.
    /// </summary>
    /// <param name="itemInstance">The item instance to remove.</param>
    /// <returns>True if the item was successfully removed, false otherwise.</returns>
    public bool RemoveItemInstance(ItemInstance itemInstance)
    {
        return items.Remove(itemInstance);
    }

    #endregion

    #region Bag Management

    /// <summary>
    /// Adds a new bag to the player's inventory.
    /// </summary>
    /// <param name="bagData">The bag data to create the bag from.</param>
    public void AddBag(BagData bagData)
    {
        if (bags.Count >= maxBags)
        {
            Debug.LogWarning($"Cannot add bag: Maximum bag limit ({maxBags}) reached.");
            return;
        }

        Bag newBag = new Bag { bagData = bagData };
        bags.Add(newBag);
        Debug.Log($"Added bag: {bagData.bagName}");
    }

    #endregion

    #region UI Management

    /// <summary>
    /// Toggles the inventory UI visibility.
    /// This method is kept unchanged as requested.
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryUIController != null)
        {
            // We need to get the root visual element from the UI Document to check its style
            var root = inventoryUIController.GetComponent<UIDocument>().rootVisualElement;
            var panel = root.Q<VisualElement>("inventory-panel");

            // Check if the panel is currently hidden
            if (panel.style.display == DisplayStyle.None)
            {
                inventoryUIController.Show(this); // Pass this inventory to the UI
                Debug.Log("Inventory shown");
            }
            else
            {
                inventoryUIController.Hide();
                Debug.Log("Inventory hidden");
            }
        }
        else
        {
            Debug.LogError("InventoryUIController is not assigned on the PlayerInventory script!");
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Gets the total number of items in the inventory.
    /// </summary>
    /// <returns>The count of items in the inventory.</returns>
    public int GetItemCount()
    {
        return items.Count;
    }

    /// <summary>
    /// Checks if the inventory is empty.
    /// </summary>
    /// <returns>True if the inventory has no items, false otherwise.</returns>
    public bool IsEmpty()
    {
        return items.Count == 0;
    }

    /// <summary>
    /// Gets the number of bags the player owns.
    /// </summary>
    /// <returns>The count of bags in the inventory.</returns>
    public int GetBagCount()
    {
        return bags.Count;
    }

    /// <summary>
    /// Checks if the player can add more bags.
    /// </summary>
    /// <returns>True if more bags can be added, false otherwise.</returns>
    public bool CanAddBag()
    {
        return bags.Count < maxBags;
    }

    #endregion

    #region Sorting & Organization

    public enum SortType
    {
        Name,
        Type,
        Rarity,
        Level,
        Amount,
        Value,
    }
    /// <summary>
    /// Sorts the inventory items based on the sorting type.
    /// </summary>
    /// <param name="sortType">The sorting type to use.</param>
    public void SortInventory(SortType sortType)
    {
        switch (sortType)
        {
            case SortType.Name:
                items.Sort((a, b) => string.Compare(a.itemData.itemName, b.itemData.itemName, StringComparison.OrdinalIgnoreCase));
                break;
            case SortType.Type:
                items.Sort((a, b) => string.Compare(a.itemData.itemType.ToString(), b.itemData.itemType.ToString(), StringComparison.OrdinalIgnoreCase));
                break;
            case SortType.Rarity:
                items.Sort((a, b) => string.Compare(a.itemData.itemRarity.ToString(), b.itemData.itemRarity.ToString(), StringComparison.OrdinalIgnoreCase));
                break;
            case SortType.Level:
                items.Sort((a, b) => a.itemData.itemLevel.CompareTo(b.itemData.itemLevel));
                break;
            case SortType.Amount:
                items.Sort((a, b) => a.count.CompareTo(b.count));
                break;
            case SortType.Value:
                items.Sort((a, b) => CalculateItemValue(a).CompareTo(CalculateItemValue(b)));
                break;
            default:
                Debug.LogError($"Invalid sorting type: {sortType}");
                break;
        }
    }

    /// <summary>
    /// Calculates the value of an item for sorting purposes.
    /// </summary>
    /// <param name="item">The item to calculate the value of.</param>
    /// <returns>The value of the item.</returns>
    private int CalculateItemValue(ItemInstance item)
    {
        switch (item.itemData.itemType)
        {
            case ItemType.CraftingMaterial:
                return item.itemData.craftingMaterialValue * item.count;
            case ItemType.Weapon:
                return item.itemData.GetWeaponValue();
            case ItemType.Armor:
                return item.itemData.GetArmorValue();
            case ItemType.Consumable:
                return item.itemData.GetConsumableValue();
            default:
                return item.itemData.itemLevel * 100 + (int)item.itemData.itemRarity;
        }
    }

    /// <summary>
    /// Automatically consolidates partial stacks of the same item type.
    /// </summary>
    public void AutoStackItems()
    {
        // Group items by their itemID
        var itemGroups = items.GroupBy(item => item.itemData.itemID).ToList();
        
        // Clear current inventory
        items.Clear();
        
        foreach (var group in itemGroups)
        {
            if (!group.First().itemData.isStackable)
            {
                // Non-stackable items just get added back
                items.AddRange(group);
                continue;
            }
            
            // Calculate total count for this item type
            int totalCount = group.Sum(item => item.count);
            int maxStackSize = group.First().itemData.maxCount;
            
            // Create full stacks
            while (totalCount >= maxStackSize)
            {
                items.Add(new ItemInstance(group.First().itemData, maxStackSize));
                totalCount -= maxStackSize;
            }
            
            // Add remaining items as a partial stack
            if (totalCount > 0)
            {
                items.Add(new ItemInstance(group.First().itemData, totalCount));
            }
        }
        
        Debug.Log("Auto-stack completed");
    }

    #endregion
}

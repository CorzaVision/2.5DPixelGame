using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    public List<ItemInstance> items = new List<ItemInstance>();
    public InventoryUIController inventoryUIController;

    public List<Bag> bags = new List<Bag>();
    public int maxBags = 2;



public void AddItem(ItemData item)
{
    if (item.isStackable)
    {
        var existing = items.Find(i => i.itemData.itemID == item.itemID);
        if (existing != null)
        {
            existing.count = Mathf.Min(existing.count + item.count, item.maxCount);
            Debug.Log("Stacked item: " + item.itemName + " (new count: " + existing.count + ")");
        }
        else
        {
            items.Add(new ItemInstance(item));
            Debug.Log("Added new stackable item: " + item.itemName);
        }
    }
    else
    {
        items.Add(new ItemInstance(item));
        Debug.Log("Added non-stackable item: " + item.itemName);
    }
}
    public void AddBag(BagData bagData)
    {
        Bag newBag = new Bag { bagData = bagData };
        bags.Add(newBag);
        Debug.Log("Added bag: " + bagData.bagName);
    }

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

    public void AddItemInstance(ItemInstance itemInstance)
    {
 if (itemInstance.itemData.isStackable)
        {
            // Find all existing stacks of the same item that are not full.
            List<ItemInstance> existingStacks = items.FindAll(i => 
                i.itemData.itemID == itemInstance.itemData.itemID && 
                i.count < i.itemData.maxCount);

            int amountToAdd = itemInstance.count;

            // Distribute the new items into existing, non-full stacks first.
            foreach (var stack in existingStacks)
            {
                if (amountToAdd <= 0) break;

                int spaceAvailable = stack.itemData.maxCount - stack.count;
                int amountToTransfer = Mathf.Min(amountToAdd, spaceAvailable);

                stack.count += amountToTransfer;
                amountToAdd -= amountToTransfer;
                Debug.Log($"Added {amountToTransfer} to an existing stack of {stack.itemData.itemName}. New count: {stack.count}");
            }

            // If there are still items left over, create new stacks for them.
            while (amountToAdd > 0)
            {
                int amountForNewStack = Mathf.Min(amountToAdd, itemInstance.itemData.maxCount);
                
                ItemInstance newStack = new ItemInstance(itemInstance.itemData, amountForNewStack);
                items.Add(newStack);
                
                amountToAdd -= amountForNewStack;
                Debug.Log($"Created a new stack of {newStack.itemData.itemName} with {newStack.count} items.");
            }
        }
        else // For non-stackable items, just add it.
        {
            items.Add(itemInstance);
            Debug.Log("Added non-stackable item: " + itemInstance.itemData.itemName);
        }
    }

    public bool RemoveItemInstance(ItemInstance itemInstance)
    {
        return items.Remove(itemInstance);
    }
}

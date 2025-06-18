using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public BagData testBagData;
    public ItemData testItemData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Test: Add a bag
        playerInventory.AddBag(testBagData);

        // Test: Add an item to main inventory (no bags)
        playerInventory.AddItem(testItemData);

        // Test: Add an item to the first bag (if any)
        if (playerInventory.bags.Count > 0)
        {
            playerInventory.bags[0].items.Add(testItemData);
            Debug.Log("Added item to first bag: " + testItemData.itemName);
        }

        // Print out inventory contents
        PrintInventory();
    }

    void PrintInventory()
    {
        Debug.Log("Main Inventory Items:");
        foreach (var item in playerInventory.items)
        {
            Debug.Log("- " + item.itemName);
        }

        for (int i = 0; i < playerInventory.bags.Count; i++)
        {
            var bag = playerInventory.bags[i];
            Debug.Log($"Bag {i + 1}: {bag.bagData.bagName}");
            foreach (var item in bag.items)
            {
                Debug.Log("  - " + item.itemName);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

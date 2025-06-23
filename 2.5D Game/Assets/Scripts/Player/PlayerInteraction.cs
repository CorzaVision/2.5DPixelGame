using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableBag currentLootBag;

    // Direct references - assign these in the Inspector!
    public PlayerInventory playerInventory;
    public LootUIController lootUIController; // <-- The new direct reference

    public void OnInteract()
    {
        // Check if we have the required components assigned
        if (lootUIController == null)
        {
            Debug.LogError("LootUIController is not assigned on the PlayerInteraction component!");
            return;
        }
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory is not assigned on the PlayerInteraction component!");
            return;
        }

        // Now, perform the interaction logic
        if (currentLootBag != null)
        {
            // Talk directly to the UI Controller
            lootUIController.ShowLoot(currentLootBag.lootbag, playerInventory, currentLootBag);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LootBag"))
        {
            currentLootBag = other.GetComponent<InteractableBag>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LootBag"))
        {
            currentLootBag = null;
        }
    }
}
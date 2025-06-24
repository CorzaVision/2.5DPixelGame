using UnityEngine;

/// <summary>
/// Handles player interaction with loot bags and other interactable objects.
/// This script manages trigger detection and communication with the loot UI system.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private LootUIController lootUIController;
    [SerializeField] private PlayerStats playerStats;
    // Interaction State
    private InteractableBag currentLootBag;

    #region Unity Lifecycle

    private void Awake()
    {
        ValidateComponents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Validates that required components are properly assigned.
    /// </summary>
    private void ValidateComponents()
    {
        if (lootUIController == null)
        {
            Debug.LogError("LootUIController is not assigned on the PlayerInteraction component!");
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory is not assigned on the PlayerInteraction component!");
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Performs interaction with the current loot bag if available.
    /// </summary>
    public void OnInteract()
    {
        if (!ValidateInteractionComponents())
        {
            return;
        }

        if (currentLootBag != null)
        {
            PerformLootInteraction();
        }
    }

    #endregion

    #region Trigger Detection

    /// <summary>
    /// Handles entering trigger zones for loot bags.
    /// </summary>
    /// <param name="other">The collider that was entered.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LootBag"))
        {
            currentLootBag = other.GetComponent<InteractableBag>();
        }
    }

    /// <summary>
    /// Handles exiting trigger zones for loot bags.
    /// </summary>
    /// <param name="other">The collider that was exited.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LootBag"))
        {
            currentLootBag = null;
        }
    }

    #endregion

    #region Interaction Logic

    /// <summary>
    /// Validates that all required components for interaction are available.
    /// </summary>
    /// <returns>True if all components are valid, false otherwise.</returns>
    private bool ValidateInteractionComponents()
    {
        if (lootUIController == null)
        {
            Debug.LogError("LootUIController is not assigned on the PlayerInteraction component!");
            return false;
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory is not assigned on the PlayerInteraction component!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Performs the actual loot interaction by showing the loot UI.
    /// </summary>
    private void PerformLootInteraction()
    {
        lootUIController.ShowLoot(currentLootBag.lootbag, playerInventory, playerStats, currentLootBag);
    }

    #endregion
}
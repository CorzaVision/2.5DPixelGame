using UnityEngine;

/// <summary>
/// Represents an interactable loot bag that can be opened by the player.
/// This script manages the bag's contents and cleanup when all items are looted.
/// </summary>
public class InteractableBag : MonoBehaviour
{
    [Header("Bag Data")]
    public Bag lootbag;

    #region Public Interface

    /// <summary>
    /// Sets the bag's contents.
    /// </summary>
    /// <param name="bag">The bag containing the loot items.</param>
    public void SetBagContents(Bag bag)
    {
        lootbag = bag;
    }

    /// <summary>
    /// Called when all items have been looted from the bag.
    /// Destroys the bag GameObject.
    /// </summary>
    public void OnAllLooted()
    {
        Destroy(gameObject);
    }

    #endregion
}

using UnityEngine;

public class InteractableBag : MonoBehaviour
{
    // This bag's contents are set by the LootDropManager
    public Bag lootbag;

    // This method is called by the LootUIController when the bag is empty
    public void OnAllLooted()
    {
        Destroy(gameObject);
    }

}

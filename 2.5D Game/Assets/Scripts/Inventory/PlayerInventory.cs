using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();

    public List<Bag> bags = new List<Bag>();
    public int maxBags = 2;


    
    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log("Added item: " + item.itemName);
    }

    public void AddBag(BagData bagData)
    {
        Bag newBag = new Bag { bagData = bagData };
        bags.Add(newBag);
        Debug.Log("Added bag: " + bagData.bagName);
    }
}

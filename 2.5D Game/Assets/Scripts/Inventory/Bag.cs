using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Bag
{
    public BagData bagData;
    public List<ItemInstance> items = new List<ItemInstance>();

    public Bag() { }

    public Bag(BagData data)
    {
        bagData = data;
        items = new List<ItemInstance>();
    }


    public void AddItem(ItemInstance item)
    {
        items.Add(item);
    }

    public void RemoveItem(ItemInstance item)
    {
        items.Remove(item);
    }

    public Bag(List<ItemInstance> items)
    {
        this.bagData = null;
        this.items = items;
    }


}

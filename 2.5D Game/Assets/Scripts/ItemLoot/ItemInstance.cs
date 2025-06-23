using UnityEngine;
using System;

[System.Serializable]
public class ItemInstance
{
    public string uniqueId;
    public ItemData itemData;
    public int count;
    
    // Parameterless constructor for Unity serialization
    public ItemInstance()
    {
        this.uniqueId = Guid.NewGuid().ToString();
        this.count = 1;
        this.itemData = null;
    }
    
    // Constructor for creating new instances
    public ItemInstance(ItemData itemData, int count = 1)
    {
        this.uniqueId = Guid.NewGuid().ToString();
        this.itemData = itemData;
        this.count = count;
    }
}
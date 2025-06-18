using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Bag
{
    public BagData bagData;
    public List<ItemData> items = new List<ItemData>();

    public Bag() { }
}

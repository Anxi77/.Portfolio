using UnityEngine;
using System.Linq;

public abstract class Item
{
    protected ItemData itemData;
    public virtual ItemData GetItemData() => itemData;
    public virtual void Initialize(ItemData data)
    {
        itemData = data;
    }

}
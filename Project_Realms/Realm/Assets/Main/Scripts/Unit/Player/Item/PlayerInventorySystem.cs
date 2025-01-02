using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EquipmentIndex 
{
    public ItemID itemID;
    public GameObject[] objsToAcitve;
}

public class PlayerInventorySystem : MonoBehaviour
{
    public List<EquipmentIndex> equipmentIndices = new List<EquipmentIndex>();
    private List<Item> items = new List<Item>();
    private Dictionary<ItemID, int> itemCounts = new Dictionary<ItemID, int>();
    private InventoryUI inventoryUI;

    public void Initialize(InventoryUI inventory)
    {
        inventoryUI = inventory;
    }

    public bool HasItem(ItemID itemID)
    {
        return itemCounts.ContainsKey(itemID) && itemCounts[itemID] > 0;
    }

    public bool AddItem(Item item)
    {
        items.Add(item);
        if (!itemCounts.ContainsKey(item.ItemID))
            itemCounts[item.ItemID] = 0;
        itemCounts[item.ItemID]++;

        if (inventoryUI != null)
            inventoryUI.TryAddItem(item);

        return true;
    }

    public bool RemoveItem(Item item)
    {
        if (!HasItem(item.ItemID)) return false;

        items.Remove(item);
        itemCounts[item.ItemID]--;
        return true;
    }

    public List<ItemID> GetItemIDs()
    {
        return items.Select(item => item.ItemID).ToList();
    }

    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }

    public void Clear()
    {
        items.Clear();
        itemCounts.Clear();
    }

    [System.Serializable]
    public struct SaveData
    {
        public List<ItemID> itemIDs;
        public List<int> counts;
    }

    public SaveData GetSaveData()
    {
        return new SaveData
        {
            itemIDs = itemCounts.Keys.ToList(),
            counts = itemCounts.Values.ToList()
        };
    }

    public void LoadFromSaveData(SaveData data)
    {
        Clear();
        for (int i = 0; i < data.itemIDs.Count; i++)
        {
            itemCounts[data.itemIDs[i]] = data.counts[i];
            // 아이템 인스턴스 생성 로직 필요
        }
    }
}
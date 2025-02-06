using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour, IInitializable
{
    private List<InventorySlot> slots = new();
    private Dictionary<EquipmentSlot, Item> equippedItems = new();
    private int gold;
    private InventoryData savedState;
    private PlayerStatSystem playerStat;
    public const int MAX_SLOTS = 20;
    public bool IsInitialized { get; private set; }
    public int MaxSlots => MAX_SLOTS;

    private void Awake()
    {
        playerStat = GetComponent<PlayerStatSystem>();
    }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            slots.Clear();
            equippedItems.Clear();
            gold = 0;

            LoadSavedInventory();

            IsInitialized = true;
        }
    }

    private void LoadSavedInventory()
    {
        var savedData = PlayerDataManager.Instance.CurrentInventoryData;

        if (savedData != null)
        {
            LoadInventoryData(savedData);
        }
    }

    public List<InventorySlot> GetSlots()
    {
        return new List<InventorySlot>(slots);
    }

    public InventoryData GetInventoryData()
    {
        return new InventoryData
        {
            slots = new List<InventorySlot>(slots),

            equippedItems = equippedItems.ToDictionary
            (
                kvp => kvp.Key,
                kvp => kvp.Value.GetItemData().ID
            ),

            gold = this.gold
        };
    }

    public void LoadInventoryData(InventoryData data)
    {
        if (data == null) return;
        slots = new List<InventorySlot>(data.slots);
        gold = data.gold;

        foreach (var kvp in data.equippedItems)
        {
            var itemData = ItemManager.Instance.GetItem(kvp.Value);
            if (itemData != null)
            {
                EquipItem(itemData, kvp.Key);
            }
        }
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData == null) return;

        if (itemData.MaxStack > 1)
        {
            var existingSlot = slots.Find(slot =>
                slot.itemId == itemData.ID &&
                slot.amount < itemData.MaxStack);
            if (existingSlot != null)
            {
                existingSlot.amount++;
                return;
            }
        }

        if (slots.Count < MAX_SLOTS)
        {
            slots.Add(new InventorySlot
            {
                itemId = itemData.ID,
                amount = 1,
                isEquipped = false
            });
        }

        else
        {
            Debug.LogWarning("Inventory is full!");
        }
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        if (equippedItems.TryGetValue(slot, out var item))
        {
            if (item != null && item.GetItemData() != null)
            {
                return item;
            }

            else
            {
                Debug.LogWarning($"Found null item or ItemData in slot {slot}");

                equippedItems.Remove(slot);
            }
        }
        return null;
    }

    public void EquipToSlot(Item item, EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            UnequipFromSlot(slot);
        }

        equippedItems[slot] = item;

        var inventorySlot = slots.Find(s => s.itemId == item.GetItemData().ID);

        if (inventorySlot != null)
        {
            inventorySlot.isEquipped = true;
        }
    }

    public void UnequipFromSlot(EquipmentSlot slot)
    {

        if (equippedItems.TryGetValue(slot, out var item))
        {
            var inventorySlot = slots.Find(s => s.itemId == item.GetItemData().ID);

            if (inventorySlot != null)
            {
                inventorySlot.isEquipped = false;
            }

            equippedItems.Remove(slot);
        }
    }

    public void EquipItem(ItemData itemData, EquipmentSlot slot)
    {
        if (itemData == null)
        {
            Debug.LogError("Attempted to equip null ItemData");

            return;
        }

        Debug.Log($"Attempting to equip {itemData.Name} to slot {slot}");

        if (equippedItems.ContainsKey(slot))
        {
            UnequipFromSlot(slot);
        }

        Item newItem = CreateEquipmentItem(itemData);

        if (newItem != null)
        {
            newItem.Initialize(itemData);
            equippedItems[slot] = newItem;

            var inventorySlot = slots.Find(s => s.itemId == itemData.ID);

            if (inventorySlot != null)
            {
                inventorySlot.isEquipped = true;
            }

            if (playerStat != null)
            {
                foreach (var stat in itemData.Stats)
                {
                    playerStat.AddModifier(stat);
                }
            }

            Debug.Log($"Successfully equipped {itemData.Name} to slot {slot}");
        }

        else
        {
            Debug.LogError($"Failed to create equipment item for {itemData.Name}");
        }
    }

    private Item CreateEquipmentItem(ItemData itemData)
    {
        try
        {
            Item newItem = itemData.Type switch
            {
                ItemType.Weapon => new WeaponItem(itemData),
                ItemType.Armor => new ArmorItem(itemData),
                ItemType.Accessory => new AccessoryItem(itemData),

                _ => null
            };

            if (newItem == null)
            {
                Debug.LogError($"Failed to create item of type: {itemData.Type}");
            }

            else
            {
                Debug.Log($"Successfully created {itemData.Type} item: {itemData.Name}");
            }

            return newItem;

        }

        catch (Exception e)
        {
            Debug.LogError($"Error creating equipment item: {e.Message}");

            return null;
        }
    }

    public void SaveInventoryState()
    {
        PlayerDataManager.Instance.SaveInventoryData(GetInventoryData());
    }

    public void SaveEquippedItems()
    {
        PlayerDataManager.Instance.SaveInventoryData(GetInventoryData());

        savedState.equippedItems = equippedItems.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.GetItemData().ID
        );
    }

    public void RestoreEquippedItems()
    {
        if (savedState?.equippedItems == null) return;
        foreach (var slot in equippedItems.Keys.ToList())
        {
            UnequipFromSlot(slot);
        }

        foreach (var kvp in savedState.equippedItems)
        {
            var itemData = ItemManager.Instance.GetItem(kvp.Value);

            if (itemData != null)
            {
                EquipItem(itemData, kvp.Key);
            }
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in equippedItems.Keys.ToList())
        {
            UnequipFromSlot(slot);
        }

        slots.Clear();
        equippedItems.Clear();
        gold = 0;

        savedState = null;
    }

    public void RemoveItem(string itemId)
    {
        var slot = slots.Find(s => s.itemId == itemId);
        if (slot != null)
        {
            var itemData = ItemManager.Instance.GetItem(slot.itemId);
            foreach (var stat in itemData.Stats)
            {
                playerStat.RemoveModifier(stat);
            }
            slots.Remove(slot);
        }
    }
}
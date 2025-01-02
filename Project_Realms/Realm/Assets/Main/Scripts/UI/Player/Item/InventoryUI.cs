using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentSlotSetup
    {
        public Slot slot;
        public ItemType slotType;
    }

    [Header("Equipment Slots")]
    [SerializeField] private List<EquipmentSlotSetup> equipmentSlots = new List<EquipmentSlotSetup>();

    [Header("Inventory Settings")]
    [SerializeField] private Transform inventorySlotsParent;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private int slotCount = 20;

    [Header("UI References")]
    [SerializeField] private GameObject dragItemHolderPrefab;
    private DragItemHolder dragItemHolder;

    private StatUI statUI;

    public DragItemHolder DragHolder => dragItemHolder;

    private List<Slot> equipmentSlotList;
    private List<Slot> inventorySlots;
    private Player player;

    public List<Slot> EquipmentSlots => equipmentSlotList;

    private PlayerInventorySystem inventorySystem;

    public void Initialize(Player player, PlayerUI playerUI)
    {
        this.player = player;
        this.statUI = playerUI.statUI;
        // DragItemHolder 생성
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && dragItemHolderPrefab != null)
        {
            dragItemHolder = Instantiate(dragItemHolderPrefab, canvas.transform)
                .GetComponent<DragItemHolder>();
        }

        // 슬롯들 초기화
        equipmentSlotList = new List<Slot>();
        foreach (var slotSetup in equipmentSlots)
        {
            if (slotSetup.slot != null)
            {
                slotSetup.slot.Initialize(player, this, statUI);
                slotSetup.slot.SetAsEquipmentSlot(slotSetup.slotType);
                equipmentSlotList.Add(slotSetup.slot);
            }
        }

        CreateInventorySlots();

        inventorySystem = player.InventorySystem;
        
        // 인벤토리 시스템 초기화 (UI 준비 완료 후)
        inventorySystem.Initialize(this);
    }

    private void CreateInventorySlots()
    {
        inventorySlots = new List<Slot>();

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = PoolManager.Instance.Spawn<Slot>(slotPrefab.gameObject, inventorySlotsParent.position, Quaternion.identity);
            slot.gameObject.transform.SetParent(inventorySlotsParent);
            slot.Initialize(player, this, statUI);

            inventorySlots.Add(slot);
        }
    }

    public bool HasFreeInventorySpace()
    {
        return inventorySlots.Any(slot => slot.IsEmpty());
    }

    public Slot GetNextEmptyInventorySlot()
    {
        return inventorySlots.FirstOrDefault(s => s.IsEmpty());
    }

    public void CompactInventory()
    {
        var items = inventorySlots.Where(s => !s.IsEmpty())
                                .Select(s => s.Item)
                                .ToList();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < items.Count)
                inventorySlots[i].PlaceItem(items[i]);
            else
                inventorySlots[i].ClearSlot();
        }
    }

    public Slot GetEquipmentSlot(ItemType itemType)
    {
        return equipmentSlotList.FirstOrDefault(s =>
            s.AllowedItemType == itemType && s.IsEmpty());
    }

    public bool TryAddItem(Item item)
    {
        var emptySlot = GetNextEmptyInventorySlot();
        if (emptySlot == null) return false;

        emptySlot.PlaceItem(item);
        return true;
    }

    public bool TryRemoveItem(Item item)
    {
        if (!inventorySystem.HasItem(item.ItemID)) return false;

        inventorySystem.RemoveItem(item);
        // UI 슬롯에서도 제거
        var slot = inventorySlots.FirstOrDefault(s => s.Item == item);
        slot?.ClearSlot();
        return true;
    }

    // 저장/로드 관련 메서드
    [System.Serializable]
    public class InventoryData
    {
        public PlayerInventorySystem.SaveData inventoryData;
    }

    public InventoryData GetSaveData()
    {
        return new InventoryData
        {
            inventoryData = inventorySystem.GetSaveData()
        };
    }

    public void LoadData(InventoryData data)
    {
        foreach (var slot in inventorySlots.Concat(equipmentSlotList))
        {
            slot.ClearSlot();
        }

        inventorySystem.LoadFromSaveData(data.inventoryData);

        var items = inventorySystem.GetAllItems();
        for (int i = 0; i < items.Count && i < inventorySlots.Count; i++)
        {
            inventorySlots[i].PlaceItem(items[i]);
        }
    }

    public bool HasItem(ItemID itemID)
    {
        return inventorySystem.HasItem(itemID);
    }
}

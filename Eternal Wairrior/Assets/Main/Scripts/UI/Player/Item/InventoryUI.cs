using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InventoryUI : MonoBehaviour, IInitializable
{
    [Header("Settings")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private Transform equipmentSlotsParent;
    [SerializeField] private ItemSlotUI[] equipmentSlots;

    private Inventory inventory;
    private List<ItemSlotUI> slotUIs = new();
    private bool isOpen = false;
    private bool isInventoryAccessible = false;

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitForPlayerAndInitialize());
            }
            else
            {
                InitializeDirectly();
            }
        }
    }

    private void InitializeDirectly()
    {
        if (GameManager.Instance?.player != null)
        {
            inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory == null)
            {
                Debug.LogError("Inventory component not found on player!");
                return;
            }

            InitializeUI();
            inventoryPanel.SetActive(false);
            IsInitialized = true;
            Debug.Log("InventoryUI initialized successfully");
        }
        else
        {
            Debug.LogWarning("Player not found, initialization delayed");
        }
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        inventory = GameManager.Instance.player.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on player!");
            yield break;
        }

        InitializeUI();
        inventoryPanel.SetActive(false);
        IsInitialized = true;
        Debug.Log("InventoryUI initialized successfully");
    }

    private void InitializeUI()
    {
        // 장비 슬롯 초기화 (먼저 초기화)
        if (equipmentSlots != null)
        {
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                if (equipmentSlots[i] != null)
                {
                    equipmentSlots[i].Initialize(-1, inventory);
                    Debug.Log($"Initialized equipment slot {i}");
                }
            }
        }
        else
        {
            Debug.LogError("Equipment slots array is null!");
        }

        // 일반 슬롯 초기화
        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            var slotUI = Instantiate(slotPrefab, slotsParent);
            slotUI.Initialize(i, inventory);
            slotUIs.Add(slotUI);
        }
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            Debug.Log("InventoryUI not initialized yet");
            return;
        }

        if (!isInventoryAccessible)
        {
            Debug.Log("Inventory not accessible");
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Toggle key pressed, opening/closing inventory");
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (!IsInitialized || inventory == null)
        {
            Debug.LogWarning("Cannot toggle inventory: Not initialized");
            return;
        }

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        Debug.Log($"Inventory toggled: {isOpen}");

        if (isOpen)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (!IsInitialized || inventory == null)
        {
            Debug.LogWarning("Cannot update UI: Inventory not initialized");
            return;
        }

        try
        {
            // 일반 슬롯 업데이트
            var slots = inventory.GetSlots();
            for (int i = 0; i < slotUIs.Count; i++)
            {
                if (i < slots.Count)
                {
                    slotUIs[i].UpdateUI(slots[i]);
                }
                else
                {
                    slotUIs[i].UpdateUI(null);
                }
            }

            // 장비 슬롯 업데이트
            if (equipmentSlots != null)
            {
                // 각 장비 슬롯 타입에 대해 명시적으로 처리
                UpdateEquipmentSlot(EquipmentSlot.Weapon, 0);
                UpdateEquipmentSlot(EquipmentSlot.Armor, 1);
                UpdateEquipmentSlot(EquipmentSlot.Ring1, 2);
                UpdateEquipmentSlot(EquipmentSlot.Ring2, 3);
                UpdateEquipmentSlot(EquipmentSlot.Necklace, 4);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating inventory UI: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateEquipmentSlot(EquipmentSlot equipSlot, int slotIndex)
    {
        try
        {
            // 기본 유효성 검사
            if (inventory == null)
            {
                Debug.LogWarning("Inventory is null");
                return;
            }

            if (equipmentSlots == null)
            {
                Debug.LogWarning("Equipment slots array is null");
                return;
            }

            if (slotIndex < 0 || slotIndex >= equipmentSlots.Length)
            {
                Debug.LogWarning($"Invalid slot index: {slotIndex}");
                return;
            }

            var slot = equipmentSlots[slotIndex];
            if (slot == null)
            {
                Debug.LogWarning($"Equipment slot at index {slotIndex} is null");
                return;
            }

            // 장착된 아이템 가져오기
            var equippedItem = inventory.GetEquippedItem(equipSlot);
            if (equippedItem != null)
            {
                var itemData = equippedItem.GetItemData();
                if (itemData != null)
                {
                    Debug.Log($"Updating equipment slot {equipSlot} with item: {itemData.Name}");
                    slot.UpdateUI(new InventorySlot
                    {
                        itemId = itemData.ID,
                        amount = 1,
                        isEquipped = true
                    });
                }
                else
                {
                    Debug.LogWarning($"ItemData is null for equipped item in slot {equipSlot}");
                    slot.UpdateUI(null);
                }
            }
            else
            {
                // 장착된 아이템이 없는 경우
                Debug.Log($"No item equipped in slot {equipSlot}");
                slot.UpdateUI(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating equipment slot {equipSlot}: {e.Message}\n{e.StackTrace}");
        }
    }

    private EquipmentSlot GetEquipmentSlotForUI(ItemSlotUI slotUI)
    {
        if (equipmentSlots == null) return EquipmentSlot.None;

        int index = System.Array.IndexOf(equipmentSlots, slotUI);
        if (index >= 0 && index < System.Enum.GetValues(typeof(EquipmentSlot)).Length)
        {
            return (EquipmentSlot)index;
        }

        Debug.LogWarning($"Invalid equipment slot index: {index}");
        return EquipmentSlot.None;
    }

    public void SetInventoryAccessible(bool accessible)
    {
        Debug.Log($"Setting inventory accessible: {accessible}");
        isInventoryAccessible = accessible;
        if (!accessible && isOpen)
        {
            ToggleInventory();
        }
    }
}
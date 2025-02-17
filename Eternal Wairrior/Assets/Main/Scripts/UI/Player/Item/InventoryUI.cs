using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InventoryUI : MonoBehaviour, IInitializable
{
    [Header("Settings")]
    private GameObject inventoryPanel;
    private Transform slotsParent;
    private ItemSlotUI slotPrefab;
    private ItemSlotUI[] equipmentSlots;

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
            return;
        }

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

            if (equipmentSlots != null)
            {
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

            var equippedItem = inventory.GetEquippedItem(equipSlot);
            if (equippedItem != null)
            {
                var itemData = equippedItem.GetItemData();
                if (itemData != null)
                {
                    Debug.Log($"Updating equipment slot {equipSlot} with item: {itemData.Name}");
                    slot.UpdateUI(new InventorySlot
                    {
                        itemData = itemData,
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
                Debug.Log($"No item equipped in slot {equipSlot}");
                slot.UpdateUI(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating equipment slot {equipSlot}: {e.Message}\n{e.StackTrace}");
        }
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
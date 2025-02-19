using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour, IInitializable
{
    #region Variables
    [Header("Settings")]
    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private Transform slotsParent;

    [SerializeField]
    private ItemSlotUI slotPrefab;

    [SerializeField]
    private ItemSlotUI[] equipmentSlots;

    private Inventory inventory;
    private List<ItemSlotUI> slotUIs = new();
    private bool isOpen = false;
    private bool isInventoryAccessible = false;

    public bool IsInitialized { get; private set; }
    #endregion

    #region Initialization
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
        if (GameManager.Instance?.player == null)
        {
            Debug.LogWarning("Player not found, initialization delayed");
            return;
        }

        SetupInventory();
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        SetupInventory();
    }

    private void SetupInventory()
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
    }

    private void InitializeUI()
    {
        InitializeEquipmentSlots();
        InitializeInventorySlots();
    }

    private void InitializeEquipmentSlots()
    {
        if (equipmentSlots == null)
        {
            Debug.LogError("Equipment slots array is null!");
            return;
        }

        foreach (var equipSlot in equipmentSlots)
        {
            if (equipSlot != null)
            {
                equipSlot.Initialize(inventory);
            }
        }
    }

    private void InitializeInventorySlots()
    {
        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            var slotUI = Instantiate(slotPrefab, slotsParent);
            slotUI.Initialize(inventory);
            slotUI.slotType = SlotType.Inventory;
            slotUIs.Add(slotUI);
        }
    }
    #endregion

    #region UI Updates
    public void UpdateUI()
    {
        if (!IsInitialized || inventory == null)
        {
            Debug.LogWarning("Cannot update UI: Inventory not initialized");
            return;
        }

        try
        {
            UpdateInventorySlots();
            UpdateEquipmentSlots();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating inventory UI: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateInventorySlots()
    {
        var slots = inventory.GetSlots();
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].UpdateUI(i < slots.Count ? slots[i] : null);
        }
    }

    private void UpdateEquipmentSlots()
    {
        if (equipmentSlots != null)
        {
            foreach (var equipSlot in equipmentSlots)
            {
                if (equipSlot != null)
                {
                    UpdateEquipmentSlot(equipSlot);
                }
            }
        }
    }

    private void UpdateEquipmentSlot(ItemSlotUI equipSlot)
    {
        try
        {
            if (inventory == null)
            {
                Debug.LogWarning("Inventory is null");
                return;
            }

            var equipmentSlot = GetEquipmentSlotFromSlotType(equipSlot.slotType);
            if (equipmentSlot == EquipmentSlot.None)
            {
                Debug.LogWarning($"Invalid slot type: {equipSlot.slotType}");
                return;
            }

            var equippedItem = inventory.GetEquippedItem(equipmentSlot);
            if (equippedItem != null)
            {
                var itemData = equippedItem.GetItemData();
                if (itemData != null)
                {
                    equipSlot.UpdateUI(
                        new InventorySlot
                        {
                            itemData = itemData,
                            amount = 1,
                            isEquipped = true,
                        }
                    );
                }
                else
                {
                    equipSlot.UpdateUI(null);
                }
            }
            else
            {
                equipSlot.UpdateUI(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating equipment slot: {e.Message}\n{e.StackTrace}");
        }
    }
    #endregion

    #region Utilities
    private EquipmentSlot GetEquipmentSlotFromSlotType(SlotType slotType)
    {
        return slotType switch
        {
            SlotType.Weapon => EquipmentSlot.Weapon,
            SlotType.Armor => EquipmentSlot.Armor,
            SlotType.Ring1 => EquipmentSlot.Ring1,
            SlotType.Ring2 => EquipmentSlot.Ring2,
            SlotType.Necklace => EquipmentSlot.Necklace,
            _ => EquipmentSlot.None,
        };
    }

    #endregion
}

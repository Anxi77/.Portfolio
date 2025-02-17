using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables
    [Header("UI Components")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject equippedIndicator;
    [SerializeField] private GameObject tooltipPrefab;

    [Header("Slot Settings")]
    public SlotType slotType;

    private Inventory inventory;
    private InventorySlot slotData;
    private ItemTooltip tooltip;
    #endregion

    #region Initialization
    public void Initialize(Inventory inventory)
    {
        this.inventory = inventory;
    }
    #endregion

    #region UI Updates
    public void UpdateUI(InventorySlot slot)
    {
        slotData = slot;

        if (slot == null || slot.itemData == null)
        {
            SetSlotEmpty();
            return;
        }

        UpdateSlotVisuals(slot.itemData, slot.amount, slot.isEquipped);
    }

    private void SetSlotEmpty()
    {
        itemIcon.enabled = false;
        amountText.enabled = false;
        equippedIndicator.SetActive(false);
    }

    private void UpdateSlotVisuals(ItemData itemData, int amount, bool isEquipped)
    {
        if (itemData == null) return;

        itemIcon.enabled = true;
        itemIcon.sprite = itemData.Icon;

        amountText.enabled = itemData.MaxStack > 1;
        if (amountText.enabled)
        {
            amountText.text = amount.ToString();
        }

        equippedIndicator.SetActive(isEquipped);
        backgroundImage.color = GetRarityColor(itemData.Rarity);
    }
    #endregion

    #region Item Interactions
    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotData?.itemData == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
            return;
        }

        HandleLeftClick();
    }

    private void HandleRightClick()
    {
        if (slotType == SlotType.Inventory)
        {
            DropItem();
        }
    }

    private void HandleLeftClick()
    {
        var itemData = slotData.itemData;
        if (itemData == null)
        {
            Debug.LogError($"Failed to get item data for ID: {slotData.itemData.ID}");
            return;
        }

        if (slotType != SlotType.Inventory)
        {
            UnequipItem();
        }
        else if (IsEquippableItem(itemData.Type))
        {
            EquipItem(itemData);
        }

        UIManager.Instance.UpdateInventoryUI();
    }

    private void DropItem()
    {
        if (slotData?.itemData == null) return;

        inventory.RemoveItem(slotData.itemData.ID);
        UIManager.Instance.UpdateInventoryUI();
        Debug.Log($"Dropped item: {slotData.itemData.Name}");
    }

    private void UnequipItem()
    {
        var equipSlot = GetEquipmentSlot();
        Debug.Log($"Unequipping from slot {equipSlot}");
        inventory.UnequipFromSlot(equipSlot);
    }

    private void EquipItem(ItemData itemData)
    {
        var equipSlot = GetEquipmentSlotForItemType(itemData.Type);
        if (equipSlot != EquipmentSlot.None)
        {
            Debug.Log($"Equipping {itemData.Name} to slot {equipSlot}");

            var equippedItem = inventory.GetEquippedItem(equipSlot);
            if (equippedItem != null)
            {
                inventory.UnequipFromSlot(equipSlot);
            }

            inventory.RemoveItem(itemData.ID);
            inventory.EquipItem(itemData, equipSlot);
        }
    }

    private bool IsEquippableItem(ItemType itemType)
    {
        return itemType == ItemType.Weapon || itemType == ItemType.Armor || itemType == ItemType.Accessory;
    }
    #endregion

    #region Tooltip
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotData?.itemData != null)
        {
            ShowTooltip(slotData.itemData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void ShowTooltip(ItemData itemData)
    {
        if (tooltip != null) return;

        tooltip = PoolManager.Instance.Spawn<ItemTooltip>(tooltipPrefab, Input.mousePosition, Quaternion.identity);
        if (tooltip != null)
        {
            tooltip.transform.SetParent(transform);
            tooltip.SetupTooltip(itemData);
            tooltip.Show(Input.mousePosition);
        }
    }

    private void HideTooltip()
    {
        if (tooltip != null)
        {
            PoolManager.Instance.Despawn(tooltip);
            tooltip = null;
        }
    }
    #endregion

    #region Equipment Slot Utilities
    private EquipmentSlot GetEquipmentSlotForItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => EquipmentSlot.Weapon,
            ItemType.Armor => EquipmentSlot.Armor,
            ItemType.Accessory => GetFirstEmptyAccessorySlot(),
            _ => EquipmentSlot.None
        };
    }

    private EquipmentSlot GetFirstEmptyAccessorySlot()
    {
        if (inventory.GetEquippedItem(EquipmentSlot.Ring1) == null) return EquipmentSlot.Ring1;
        if (inventory.GetEquippedItem(EquipmentSlot.Ring2) == null) return EquipmentSlot.Ring2;
        if (inventory.GetEquippedItem(EquipmentSlot.Necklace) == null) return EquipmentSlot.Necklace;
        return EquipmentSlot.None;
    }

    private EquipmentSlot GetEquipmentSlot()
    {
        return slotType switch
        {
            SlotType.Weapon => EquipmentSlot.Weapon,
            SlotType.Armor => EquipmentSlot.Armor,
            SlotType.Ring1 => EquipmentSlot.Ring1,
            SlotType.Ring2 => EquipmentSlot.Ring2,
            SlotType.Necklace => EquipmentSlot.Necklace,
            _ => EquipmentSlot.None
        };
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.3f, 1f, 0.3f),
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),
            ItemRarity.Epic => new Color(0.8f, 0.3f, 1f),
            ItemRarity.Legendary => new Color(1f, 0.8f, 0.2f),
            _ => Color.white
        };
    }
    #endregion
}
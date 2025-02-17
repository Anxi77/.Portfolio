using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image itemIcon;
    private Image backgroundImage;
    private TextMeshProUGUI amountText;
    private GameObject equippedIndicator;
    private GameObject tooltipPrefab;

    private int slotIndex;
    private Inventory inventory;
    private InventorySlot slotData;
    private ItemTooltip tooltip;

    public void Initialize(int index, Inventory inventory)
    {
        this.slotIndex = index;
        this.inventory = inventory;
    }

    public void UpdateUI(InventorySlot slot)
    {
        slotData = slot;

        if (slot == null || slot.itemData == null)
        {
            itemIcon.enabled = false;
            amountText.enabled = false;
            equippedIndicator.SetActive(false);
            return;
        }

        var itemData = slot.itemData;
        if (itemData == null) return;

        itemIcon.enabled = true;
        itemIcon.sprite = itemData.Icon;

        amountText.enabled = itemData.MaxStack > 1;
        if (amountText.enabled)
        {
            amountText.text = slot.amount.ToString();
        }

        equippedIndicator.SetActive(slot.isEquipped);
        backgroundImage.color = GetRarityColor(itemData.Rarity);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotData == null || slotData.itemData == null) return;

        var itemData = slotData.itemData;
        if (itemData != null)
        {
            ShowTooltip(itemData);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotData == null || slotData.itemData == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (slotIndex != -1)
            {
                DropItem();
            }
            return;
        }

        var itemData = slotData.itemData;
        if (itemData == null)
        {
            Debug.LogError($"Failed to get item data for ID: {slotData.itemData.ID}");
            return;
        }

        Debug.Log($"Clicked item: {itemData.Name} of type {itemData.Type}");

        if (slotIndex == -1)
        {
            Debug.Log($"Unequipping from slot {GetEquipmentSlot()}");
            inventory.UnequipFromSlot(GetEquipmentSlot());
        }
        else if (itemData.Type == ItemType.Weapon || itemData.Type == ItemType.Armor || itemData.Type == ItemType.Accessory)
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
                inventory.RemoveItem(slotData.itemData.ID);
                inventory.EquipItem(itemData, equipSlot);
            }
        }

        UIManager.Instance.UpdateInventoryUI();
    }

    private void DropItem()
    {
        if (slotData == null || slotData.itemData == null) return;

        var itemData = slotData.itemData;
        if (itemData != null)
        {
            inventory.RemoveItem(slotData.itemData.ID);

            UIManager.Instance.UpdateInventoryUI();

            Debug.Log($"Dropped item: {itemData.Name}");
        }
    }

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
        return transform.GetSiblingIndex() switch
        {
            0 => EquipmentSlot.Weapon,
            1 => EquipmentSlot.Armor,
            2 => EquipmentSlot.Ring1,
            3 => EquipmentSlot.Ring2,
            4 => EquipmentSlot.Necklace,
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
}
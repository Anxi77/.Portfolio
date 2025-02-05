using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject equippedIndicator;
    [SerializeField] private GameObject tooltipPrefab;

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

        if (slot == null || string.IsNullOrEmpty(slot.itemId))
        {
            itemIcon.enabled = false;
            amountText.enabled = false;
            equippedIndicator.SetActive(false);
            return;
        }

        var itemData = ItemManager.Instance.GetItem(slot.itemId);
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
        if (slotData == null || string.IsNullOrEmpty(slotData.itemId)) return;

        var itemData = ItemManager.Instance.GetItem(slotData.itemId);
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
        if (slotData == null || string.IsNullOrEmpty(slotData.itemId)) return;

        // 우클릭 처리
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (slotIndex != -1)  // 장비 슬롯이 아닌 경우에만 버리기 가능
            {
                DropItem();
            }
            return;
        }

        // 좌클릭 처리 (기존 코드)
        var itemData = ItemManager.Instance.GetItem(slotData.itemId);
        if (itemData == null)
        {
            Debug.LogError($"Failed to get item data for ID: {slotData.itemId}");
            return;
        }

        Debug.Log($"Clicked item: {itemData.Name} of type {itemData.Type}");

        // 장비 슬롯인 경우 - 장비 해제
        if (slotIndex == -1)
        {
            Debug.Log($"Unequipping from slot {GetEquipmentSlot()}");
            inventory.UnequipFromSlot(GetEquipmentSlot());
        }
        // 일반 슬롯인 경우 - 장비 장착
        else if (itemData.Type == ItemType.Weapon || itemData.Type == ItemType.Armor || itemData.Type == ItemType.Accessory)
        {
            var equipSlot = GetEquipmentSlotForItemType(itemData.Type);
            if (equipSlot != EquipmentSlot.None)
            {
                Debug.Log($"Equipping {itemData.Name} to slot {equipSlot}");

                // 기존에 장착된 아이템이 있다면 인벤토리로 되돌림
                var equippedItem = inventory.GetEquippedItem(equipSlot);
                if (equippedItem != null)
                {
                    inventory.UnequipFromSlot(equipSlot);
                }

                // 인벤토리에서 아이템 제거 후 장비 슬롯으로 이동
                inventory.RemoveItem(slotData.itemId);
                inventory.EquipItem(itemData, equipSlot);
            }
        }

        UIManager.Instance.UpdateInventoryUI();
    }

    private void DropItem()
    {
        if (slotData == null || string.IsNullOrEmpty(slotData.itemId)) return;

        var itemData = ItemManager.Instance.GetItem(slotData.itemId);
        if (itemData != null)
        {
            // 인벤토리에서 아이템 제거
            inventory.RemoveItem(slotData.itemId);

            // UI 업데이트
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
        return EquipmentSlot.Ring1; // 모든 슬롯이 찼다면 Ring1에 덮어씌우기
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
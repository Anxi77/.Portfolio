using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Slot slot;
    private bool isInventorySlot;
    private InventoryUI inventory;

    public bool IsInventorySlot { get => isInventorySlot; set => isInventorySlot = value; }

    public void InitializeSlotHandler(InventoryUI inventoryRef)
    {
        if (slot == null)
            slot = GetComponent<Slot>();

        inventory = inventoryRef;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.IsEmpty()) return;
        inventory.DragHolder.StartDrag(slot);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        inventory.DragHolder.DropItem();
        if (IsInventorySlot)
            inventory.CompactInventory();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragHolder = inventory.DragHolder;
        if (!dragHolder.IsDragging) return;

        if (slot.CanAcceptItem(dragHolder.DraggedItem))
        {
            if (slot.IsEmpty())
            {
                dragHolder.SetTargetSlot(slot);
            }
            else
            {
                Slot sourceSlot = dragHolder.SourceSlot;

                if (sourceSlot.CanAcceptItem(slot.Item))
                {
                    Item tempItem = slot.Item;

                    slot.ClearSlot();
                    dragHolder.SetTargetSlot(slot);

                    sourceSlot.PlaceItem(tempItem);
                }
            }
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot.IsEmpty()) return;

        if (IsInventorySlot)
        {
            foreach (Slot equipSlot in inventory.EquipmentSlots)
            {
                if (equipSlot.CanAcceptItem(slot.Item))
                {
                    if (equipSlot.IsEmpty())
                    {
                        Item itemToEquip = slot.Item;
                        slot.ClearSlot();
                        equipSlot.PlaceItem(itemToEquip);
                    }
                    else
                    {
                        Item equippedItem = equipSlot.Item;
                        Item inventoryItem = slot.Item;

                        slot.ClearSlot();
                        equipSlot.ClearSlot();

                        equipSlot.PlaceItem(inventoryItem);
                        slot.PlaceItem(equippedItem);
                    }

                    inventory.CompactInventory();
                    break;
                }
            }
        }
        else
        {
            if (inventory.HasFreeInventorySpace())
            {
                Item unequippedItem = slot.Item;  
                slot.ClearSlot();                 
                inventory.TryAddItem(unequippedItem);  
                inventory.CompactInventory();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slot.IsEmpty() || !gameObject.activeInHierarchy) return;

        var itemInstance = slot.Item.InstanceData;
        TooltipWindow.Instance.ShowTooltip(
            itemInstance.GetItemName(),
            itemInstance.GetRarity(),
            itemInstance.GetItemType(),
            itemInstance.GetDescription(),
            itemInstance.GetStatsTooltip(),
            slot.Item.Icon,
            eventData.position.x
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipWindow.Instance.HideTooltip();
    }
}
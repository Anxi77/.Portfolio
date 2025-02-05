using UnityEngine;
using System.Linq;

public abstract class Item
{
    protected ItemData itemData;
    protected bool isEquipped = false;
    protected EquipmentSlot currentSlot = EquipmentSlot.None;

    public virtual void Initialize(ItemData data)
    {
        itemData = data;
        isEquipped = false;
        currentSlot = EquipmentSlot.None;
    }

    public bool TryEquip(Player player, EquipmentSlot slot)
    {
        if (!CanEquipToSlot(slot))
            return false;

        var inventory = player.GetComponent<Inventory>();
        var playerStat = player.GetComponent<PlayerStat>();

        if (inventory != null && playerStat != null)
        {
            // 기존 장비 해제
            if (inventory.GetEquippedItem(slot) != null)
            {
                inventory.UnequipFromSlot(slot);
            }

            // 새 장비 장착
            playerStat.EquipItem(itemData.Stats, slot);
            isEquipped = true;
            currentSlot = slot;
            inventory.EquipToSlot(this, slot);
            return true;
        }

        return false;
    }

    public void Unequip(Player player)
    {
        if (!isEquipped)
            return;

        var inventory = player.GetComponent<Inventory>();
        var playerStat = player.GetComponent<PlayerStat>();

        if (inventory != null && playerStat != null)
        {
            playerStat.UnequipItem(currentSlot);
            inventory.UnequipFromSlot(currentSlot);
            isEquipped = false;
            currentSlot = EquipmentSlot.None;
        }
    }

    private bool CanEquipToSlot(EquipmentSlot slot)
    {
        return (itemData.Type, slot) switch
        {
            (ItemType.Weapon, EquipmentSlot.Weapon) => true,
            (ItemType.Armor, EquipmentSlot.Armor) => true,
            (ItemType.Accessory, EquipmentSlot.Necklace) => true,
            (ItemType.Accessory, EquipmentSlot.Ring1) => true,
            (ItemType.Accessory, EquipmentSlot.Ring2) => true,
            _ => false
        };
    }

    public virtual ItemData GetItemData() => itemData;
    public bool IsEquipped => isEquipped;
    public EquipmentSlot CurrentSlot => currentSlot;
}

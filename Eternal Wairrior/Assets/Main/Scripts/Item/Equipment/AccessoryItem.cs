using System.Collections.Generic;
using UnityEngine;

public class AccessoryItem : EquipmentItem
{
    private AccessoryType accessoryType;

    public AccessoryItem(ItemData itemData) : base(itemData)
    {
        if (itemData.Type != ItemType.Accessory)
        {
            Debug.LogError($"Attempted to create AccessoryItem with non-accessory ItemData: {itemData.Type}");
        }
    }

    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        DetermineAccessoryType(data);
    }

    private void DetermineAccessoryType(ItemData data)
    {
        accessoryType = data.ID switch
        {
            var id when id.Contains("necklace") || id.Contains("amulet") || id.Contains("pendant")
                => AccessoryType.Necklace,
            var id when id.Contains("ring")
                => AccessoryType.Ring,
            _ => AccessoryType.None
        };

        if (accessoryType == AccessoryType.None)
        {
            Debug.LogWarning($"Cannot determine accessory type for item: {data.ID}");
        }
    }

    public void SetAccessorySlot(EquipmentSlot slot)
    {
        bool isValidSlot = (accessoryType, slot) switch
        {
            (AccessoryType.Necklace, EquipmentSlot.Necklace) => true,
            (AccessoryType.Ring, EquipmentSlot.Ring1) => true,
            (AccessoryType.Ring, EquipmentSlot.Ring2) => true,
            _ => false
        };

        if (!isValidSlot)
        {
            Debug.LogError($"Invalid slot {slot} for accessory type {accessoryType}");
            return;
        }

        equipmentSlot = slot;
    }

    protected override void ValidateItemType(ItemType type)
    {
        if (type != ItemType.Accessory)
        {
            Debug.LogError($"잘못된 아이템 타입입니다: {type}. AccessoryItem은 ItemType.Accessory이어야 합니다.");
        }
    }

    public AccessoryType GetAccessoryType() => accessoryType;
}
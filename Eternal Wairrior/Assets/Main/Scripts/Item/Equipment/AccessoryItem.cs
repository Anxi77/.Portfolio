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

    protected override void ValidateItemType(ItemType type)
    {
        if (type != ItemType.Accessory)
        {
            Debug.LogError($"잘못된 아이템 타입입니다: {type}. AccessoryItem은 ItemType.Accessory이어야 합니다.");
        }
    }
}
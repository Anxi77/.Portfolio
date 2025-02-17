using UnityEngine;

public class WeaponItem : EquipmentItem
{
    public WeaponItem(ItemData itemData) : base(itemData)
    {
        if (itemData.Type != ItemType.Weapon)
        {
            Debug.LogError($"Attempted to create WeaponItem with non-weapon ItemData: {itemData.Type}");
        }
    }
    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        equipmentSlot = EquipmentSlot.Weapon;
        ValidateItemType(data.Type);
    }

    protected override void ValidateItemType(ItemType type)
    {
        if (type != ItemType.Weapon)
        {
            Debug.LogError($"Invalid item type: {type}. WeaponItem must be ItemType.Weapon.");
        }
    }
}
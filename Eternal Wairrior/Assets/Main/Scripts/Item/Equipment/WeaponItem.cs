using UnityEngine;

public class WeaponItem : EquipmentItem
{
    public WeaponItem(ItemData itemData) : base(itemData)
    {
        if (itemData.type != ItemType.Weapon)
        {
            Debug.LogError($"Attempted to create WeaponItem with non-weapon ItemData: {itemData.type}");
        }
    }
    public override void Initialize(ItemData data)
    {
        base.Initialize(data);
        equipmentSlot = EquipmentSlot.Weapon;
        ValidateItemType(data.type);
    }

    protected override void ValidateItemType(ItemType type)
    {
        if (type != ItemType.Weapon)
        {
            Debug.LogError($"잘못된 아이템 타입입니다: {type}. WeaponItem은 ItemType.Weapon이어야 합니다.");
        }
    }
}
using System.Collections.Generic;

public class ItemEffectApplier
{
    private PlayerStat playerStat;

    public void ApplyItemEffects(ItemData item, EquipmentSlot slot)
    {
        foreach (var stat in item.Stats)
        {
            var sourceType = GetSourceTypeForSlot(slot);
            playerStat.AddStatModifier(
                stat.statType,
                sourceType,
                stat.increaseType,
                stat.amount
            );
        }
    }

    public void RemoveItemEffects(ItemData item, EquipmentSlot slot)
    {
        foreach (var stat in item.Stats)
        {
            var sourceType = GetSourceTypeForSlot(slot);
            playerStat.RemoveStatModifier(
                stat.statType,
                sourceType,
                stat.increaseType,
                stat.amount
            );
        }
    }

    private SourceType GetSourceTypeForSlot(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => SourceType.Equipment_Weapon,
            EquipmentSlot.Armor => SourceType.Equipment_Armor,
            EquipmentSlot.Ring2 or EquipmentSlot.Ring1 or EquipmentSlot.Necklace => SourceType.Equipment_Accessory,
            EquipmentSlot.Special => SourceType.Equipment_Special,
            _ => SourceType.Consumable
        };
    }
}
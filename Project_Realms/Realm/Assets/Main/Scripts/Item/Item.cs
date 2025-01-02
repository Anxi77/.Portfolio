using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData itemData;
    private ItemInstanceData instanceData;

    public ItemID ItemID => itemData.ItemID;
    public ItemType ItemType => itemData.ItemType;
    public GameObject ItemPrefab => itemData.ItemPrefab;
    public string Description => itemData.Description;
    public Sprite Icon => itemData.Icon;
    public IReadOnlyList<ItemData.ItemStat> Stats => instanceData.Stats;
    public ItemData ItemData => itemData;
    public ItemInstanceData InstanceData => instanceData;

    public void Initialize(ItemData template, ItemInstanceData instance)
    {
        itemData = template;
        instanceData = instance;
    }

    protected virtual void ApplyStatModifier(ICharacterStats stats, StatType statType, float value, StatModifierType modType)
    {
        stats.AddModifier(statType, new StatModifier(value, modType, this, SourceType.Equipment));
    }

    public void ApplyStats(ICharacterStats characterStats)
    {
        foreach (var stat in Stats)
        {
            if (stat.flatValue != 0)
                ApplyStatModifier(characterStats, stat.statType, stat.flatValue, StatModifierType.Flat);
            if (stat.percentValue != 0)
                ApplyStatModifier(characterStats, stat.statType, stat.percentValue, StatModifierType.PercentAdd);
        }
    }

    public void RemoveStats(ICharacterStats characterStats)
    {
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            characterStats.GetStat(statType)?.RemoveAllModifiersFromSource(this);
        }
    }
}
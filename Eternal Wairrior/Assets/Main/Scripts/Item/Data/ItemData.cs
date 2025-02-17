using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

[Serializable]
public class SerializableItemList
{
    public List<ItemData> items = new();
}

[Serializable]
public class DropTablesWrapper
{
    public List<DropTableData> dropTables = new();
}

[Serializable]
public class ItemData
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    public ElementType Element { get; set; }
    public int MaxStack { get; set; } = 1;
    public float DropRate { get; set; }
    public int MinAmount { get; set; } = 1;
    public int MaxAmount { get; set; } = 1;
    public string IconPath { get; set; }
    public ItemStatRangeData StatRanges { get; set; } = new();
    public List<StatModifier> Stats { get; set; } = new();
    public ItemEffectRangeData EffectRanges { get; set; } = new();
    public List<ItemEffectData> Effects { get; set; } = new();

    [JsonIgnore] private Sprite _icon;
    [JsonIgnore] public Sprite Icon => _icon ??= !string.IsNullOrEmpty(IconPath) ? Resources.Load<Sprite>(IconPath) : null;

    [JsonIgnore] public int amount = 1;

    #region Stats Management
    public void AddStat(StatModifier stat)
    {
        if (stat == null) return;
        Stats.RemoveAll(s => s.Type == stat.Type && s.Source == stat.Source && s.IncreaseType == stat.IncreaseType && Math.Abs(s.Value - stat.Value) < float.Epsilon);
        Stats.Add(stat);
    }

    public StatModifier GetStat(StatType statType) =>
        Stats.FirstOrDefault(s => s.Type == statType);

    public float GetStatValue(StatType statType) =>
        GetStat(statType)?.Value ?? 0f;

    public void ClearStats() => Stats.Clear();
    #endregion

    #region Effects Management
    public void AddEffect(ItemEffectData effect)
    {
        if (effect == null) return;
        Effects.Add(effect);
    }

    public void RemoveEffect(string effectId) =>
        Effects.RemoveAll(e => e.effectId == effectId);

    public ItemEffectData GetEffect(string effectId) =>
        Effects.FirstOrDefault(e => e.effectId == effectId);

    public List<ItemEffectData> GetEffectsByType(EffectType type) =>
        Effects.Where(e => e.effectType == type).ToList();

    public List<ItemEffectData> GetEffectsForSkill(SkillType skillType) =>
        Effects.Where(e => e.applicableSkills?.Contains(skillType) ?? false).ToList();

    public List<ItemEffectData> GetEffectsForElement(ElementType elementType) =>
        Effects.Where(e => e.applicableElements?.Contains(elementType) ?? false).ToList();
    #endregion

    #region Cloning
    public ItemData Clone()
    {
        return JsonConvert.DeserializeObject<ItemData>(
            JsonConvert.SerializeObject(this)
        );
    }
    #endregion
}

#region Stat & Effects
[Serializable]
public class ItemStatRange
{
    public StatType statType;
    public float minValue;
    public float maxValue;
    public float weight = 1f;
    public IncreaseType increaseType = IncreaseType.Flat;
}

[Serializable]
public class ItemStatRangeData
{
    public List<ItemStatRange> possibleStats = new();
    public int minStatCount = 1;
    public int maxStatCount = 4;

    public Dictionary<ItemRarity, int> additionalStatsByRarity = new()
    {
        { ItemRarity.Common, 0 },
        { ItemRarity.Uncommon, 1 },
        { ItemRarity.Rare, 2 },
        { ItemRarity.Epic, 3 },
        { ItemRarity.Legendary, 4 }
    };
}

[Serializable]
public class ItemEffectRange
{
    public string effectId;
    public string effectName;
    public string description;
    public EffectType effectType;
    public float minValue;
    public float maxValue;
    public float weight = 1f;
    public SkillType[] applicableSkills;
    public ElementType[] applicableElements;
}

[Serializable]
public class ItemEffectRangeData
{
    public string itemId;
    public ItemType itemType;
    public List<ItemEffectRange> possibleEffects = new List<ItemEffectRange>();
    public int minEffectCount = 1;
    public int maxEffectCount = 3;

    public Dictionary<ItemRarity, int> additionalEffectsByRarity = new Dictionary<ItemRarity, int>
    {
        { ItemRarity.Common, 0 },
        { ItemRarity.Uncommon, 1 },
        { ItemRarity.Rare, 2 },
        { ItemRarity.Epic, 3 },
        { ItemRarity.Legendary, 4 }
    };
}

[Serializable]
public class ItemEffectData
{
    public string effectId;
    public string effectName;
    public EffectType effectType;
    public float value;
    public ItemRarity minRarity;
    public ItemType[] applicableTypes;
    public SkillType[] applicableSkills;
    public ElementType[] applicableElements;

    public bool CanApplyTo(ItemData item, SkillType skillType = SkillType.None, ElementType element = ElementType.None)
    {
        if (item.Rarity < minRarity) return false;
        if (applicableTypes != null && !applicableTypes.Contains(item.Type)) return false;
        if (skillType != SkillType.None && applicableSkills != null && !applicableSkills.Contains(skillType)) return false;
        if (element != ElementType.None && applicableElements != null && !applicableElements.Contains(element)) return false;
        return true;
    }
}
#endregion

#region Drop Table

[Serializable]
public class DropTableEntry
{
    [SerializeField]
    public string itemId;
    [SerializeField]
    public float dropRate;
    [SerializeField]
    public ItemRarity rarity;
    [SerializeField]
    public int minAmount = 1;
    [SerializeField]
    public int maxAmount = 1;
}

[Serializable]
public class DropTableData
{
    [SerializeField]
    public EnemyType enemyType;
    [SerializeField]
    public List<DropTableEntry> dropEntries = new();
    [SerializeField]
    public float guaranteedDropRate = 0.1f;
    [SerializeField]
    public int maxDrops = 3;
}

#endregion
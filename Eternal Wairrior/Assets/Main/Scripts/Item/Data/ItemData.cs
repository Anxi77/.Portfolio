using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using System;
using Newtonsoft.Json;

[Serializable]
public enum ItemType
{
    None,
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Material
}

[Serializable]
public enum AccessoryType
{
    None,
    Necklace,
    Ring
}

[Serializable]
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
}

[Serializable]
public class ItemData
{
    // 기본 아이템 데이터
    [JsonProperty] public string ID { get; set; }
    [JsonProperty] public string Name { get; set; }
    [JsonProperty] public string Description { get; set; }
    [JsonProperty] public ItemType Type { get; set; }
    [JsonProperty] public ItemRarity Rarity { get; set; }
    [JsonProperty] public ElementType Element { get; set; }
    [JsonProperty] public int MaxStack { get; set; } = 1;
    [JsonProperty] public float DropRate { get; set; }
    [JsonProperty] public int MinAmount { get; set; } = 1;
    [JsonProperty] public int MaxAmount { get; set; } = 1;
    [JsonProperty] public string IconPath { get; set; }
    [JsonProperty] public ItemStatRangeData StatRanges { get; set; } = new();
    [JsonProperty] public List<StatContainer> Stats { get; set; } = new();
    [JsonProperty] public ItemEffectRangeData EffectRanges { get; set; } = new();
    [JsonProperty] public List<ItemEffectData> Effects { get; set; } = new();
    [JsonProperty] public Dictionary<string, float> EffectValues { get; set; } = new();

    // 런타임 전용 데이터
    [NonSerialized] private Sprite _icon;
    [JsonIgnore]
    public Sprite Icon => _icon ??= !string.IsNullOrEmpty(IconPath) ? Resources.Load<Sprite>(IconPath) : null;

    // 런타임 데이터
    [JsonIgnore] public int amount = 1;

    #region Stats Management
    public void AddStat(StatContainer stat)
    {
        if (stat == null) return;
        Stats.RemoveAll(s => s.statType == stat.statType);
        Stats.Add(stat);
    }

    public StatContainer GetStat(StatType statType) =>
        Stats.FirstOrDefault(s => s.statType == statType);

    public float GetStatValue(StatType statType) =>
        GetStat(statType)?.amount ?? 0f;

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
    /// <summary>
    /// 아이템 데이터의 깊은 복사본을 생성.
    /// JSON 직렬화/역직렬화를 사용하여 모든 중첩된 객체들의 새로운 인스턴스를 생성.
    /// </summary>
    public ItemData Clone()
    {
        return JsonConvert.DeserializeObject<ItemData>(
            JsonConvert.SerializeObject(this)
        );
    }
    #endregion
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

[Serializable]
public enum EffectType
{
    None,
    DamageBonus,
    CooldownReduction,
    ProjectileSpeed,
    ProjectileRange,
    HomingEffect,
    AreaRadius,
    AreaDuration,
    ElementalPower
}

[Serializable]
public class SerializableItemList
{
    public List<ItemData> items = new();
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
public class ItemStatData
{
    public float damage;
    public float defense;
    public float hp;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;
    public float hpRegen;

    public float criticalChance;
    public float criticalDamage;
    public float lifeSteal;
    public float reflectDamage;
    public float dodgeChance;

    public List<StatContainer> ConvertToStatContainers()
    {
        var containers = new List<StatContainer>();

        if (damage > 0) containers.Add(new StatContainer(StatType.Damage, SourceType.Weapon, IncreaseType.Add, damage));
        if (defense > 0) containers.Add(new StatContainer(StatType.Defense, SourceType.Armor, IncreaseType.Add, defense));
        if (hp > 0) containers.Add(new StatContainer(StatType.MaxHp, SourceType.Armor, IncreaseType.Add, hp));
        if (moveSpeed > 0) containers.Add(new StatContainer(StatType.MoveSpeed, SourceType.Accessory, IncreaseType.Mul, moveSpeed));
        if (attackSpeed > 0) containers.Add(new StatContainer(StatType.AttackSpeed, SourceType.Weapon, IncreaseType.Mul, attackSpeed));
        if (attackRange > 0) containers.Add(new StatContainer(StatType.AttackRange, SourceType.Weapon, IncreaseType.Mul, attackRange));
        if (hpRegen > 0) containers.Add(new StatContainer(StatType.HpRegenRate, SourceType.Accessory, IncreaseType.Add, hpRegen));

        if (criticalChance > 0) containers.Add(new StatContainer(StatType.CriticalChance, SourceType.Weapon, IncreaseType.Add, criticalChance));
        if (criticalDamage > 0) containers.Add(new StatContainer(StatType.CriticalDamage, SourceType.Weapon, IncreaseType.Add, criticalDamage));
        if (lifeSteal > 0) containers.Add(new StatContainer(StatType.LifeSteal, SourceType.Weapon, IncreaseType.Add, lifeSteal));
        if (reflectDamage > 0) containers.Add(new StatContainer(StatType.ReflectDamage, SourceType.Armor, IncreaseType.Add, reflectDamage));
        if (dodgeChance > 0) containers.Add(new StatContainer(StatType.DodgeChance, SourceType.Accessory, IncreaseType.Add, dodgeChance));

        return containers;
    }
}

[Serializable]
public class ItemStatRange
{
    public StatType statType;
    public float minValue;
    public float maxValue;
    public float weight = 1f;
    public IncreaseType increaseType = IncreaseType.Add;
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
public class DropTablesWrapper
{
    public List<DropTableData> dropTables = new();
}


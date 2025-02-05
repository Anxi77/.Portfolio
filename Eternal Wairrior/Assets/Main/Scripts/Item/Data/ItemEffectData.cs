using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
public class ItemEffectData
{
    [SerializeField]
    public string effectId;
    [SerializeField]
    public string effectName;
    [SerializeField]
    public EffectType effectType;
    [SerializeField]
    public float value;
    [SerializeField]
    public ItemRarity minRarity;
    [SerializeField]
    public ItemType[] applicableTypes;
    [SerializeField]
    public SkillType[] applicableSkills;
    [SerializeField]
    public ElementType[] applicableElements;
    [SerializeField]
    public float weight;

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
public class ItemEffectPool
{
    [SerializeField]
    public ItemType itemType;
    [SerializeField]
    public ItemRarity minRarity;
    [SerializeField]
    public List<ItemEffectData> effects = new();
}
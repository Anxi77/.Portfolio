using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemEffectRange
{
    [SerializeField]
    public string effectId;
    [SerializeField]
    public string effectName;
    [SerializeField]
    public string description;
    [SerializeField]
    public EffectType effectType;
    [SerializeField]
    public float minValue;
    [SerializeField]
    public float maxValue;
    [SerializeField]
    public float weight = 1f;
    [SerializeField]
    public ItemRarity minRarity = ItemRarity.Common;
    [SerializeField]
    public ItemType[] applicableTypes;
    [SerializeField]
    public SkillType[] applicableSkills;
    [SerializeField]
    public ElementType[] applicableElements;
}

[Serializable]
public class ItemEffectRangeData
{
    [SerializeField]
    public string itemId;
    [SerializeField]
    public ItemType itemType;
    [SerializeField]
    public List<ItemEffectRange> possibleEffects = new List<ItemEffectRange>();
    [SerializeField]
    public int minEffectCount = 1;
    [SerializeField]
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

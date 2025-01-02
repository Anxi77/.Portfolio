using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Item Generation Rule", menuName = "Game/Item Generation Rule")]
public class ItemGenerationRuleSO : ScriptableObject
{
    [Serializable]
    public class RarityColorSettings
    {
        public ItemRarity rarity;
        public Color color = Color.white;
    }

    [Header("Rarity Colors")]
    public RarityColorSettings[] rarityColors = new[]
    {
        new RarityColorSettings { rarity = ItemRarity.Common, color = Color.white },
        new RarityColorSettings { rarity = ItemRarity.Uncommon, color = new Color(0.118f, 1f, 0f) },
        new RarityColorSettings { rarity = ItemRarity.Rare, color = new Color(0f, 0.439f, 0.867f) },
        new RarityColorSettings { rarity = ItemRarity.Epic, color = new Color(0.639f, 0.208f, 0.933f) },
        new RarityColorSettings { rarity = ItemRarity.Legendary, color = new Color(1f, 0.502f, 0f) }
    };

    public Color GetColorForRarity(ItemRarity rarity)
    {
        var setting = Array.Find(rarityColors, x => x.rarity == rarity);
        return setting?.color ?? Color.white;
    }

    [Serializable]
    public class StatGenerationRule
    {
        public StatType statType;
        [Header("Value Settings")]
        public Vector2 flatValueRange; 
        public Vector2 percentValueRange;  
        public bool usePercentValue;  

        [Header("Generation Settings")]
        [Range(0f, 100f)]
        public float generationChance;
    }

    [Serializable]
    public class ItemGenerationRule
    {
        public ItemData itemTemplate; 
        [Range(0f, 100f)]
        public float baseDropChance; 
        public List<StatGenerationRule> possibleStats;  
        [Range(0, 6)]
        public int maxStatCount = 4; 
    }

    [Serializable]
    public class MonsterDropRule
    {
        public MonsterType monsterType;
        [Range(0f, 100f)]
        public float itemDropChance; 
        public List<ItemGenerationRule> possibleItems; 
    }

    public MonsterDropRule[] monsterDropRules;

    public MonsterDropRule GetDropRuleForMonster(MonsterType monsterType)
    {
        return Array.Find(monsterDropRules, rule => rule.monsterType == monsterType);
    }
}
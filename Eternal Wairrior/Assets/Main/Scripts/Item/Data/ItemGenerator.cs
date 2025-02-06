using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class ItemGenerator : MonoBehaviour
{
    public ItemData GenerateItem(string itemId, ItemRarity? targetRarity = null)
    {
        var newItem = ItemDataManager.Instance.itemDatabase[itemId].Clone();

        if (targetRarity.HasValue)
        {
            newItem.Rarity = targetRarity.Value;
        }

        Debug.Log($"Generating item: {newItem.Name} with rarity: {newItem.Rarity}");

        GenerateStats(newItem);

        GenerateEffects(newItem);

        return newItem;
    }

    private void GenerateStats(ItemData item)
    {
        if (item.StatRanges == null || item.StatRanges.possibleStats == null)
        {
            Debug.LogWarning($"No stat ranges defined for item: {item.ID}");
            return;
        }

        item.Stats.Clear();

        int additionalStats = item.StatRanges.additionalStatsByRarity.GetValueOrDefault(item.Rarity, 0);
        int statCount = Random.Range(
            item.StatRanges.minStatCount,
            Mathf.Min(item.StatRanges.maxStatCount + additionalStats + 1,
                     item.StatRanges.possibleStats.Count)
        );

        Debug.Log($"Generating {statCount} stats for item {item.ID}");

        var availableStats = item.StatRanges.possibleStats
            .ToList();

        for (int i = 0; i < statCount && availableStats.Any(); i++)
        {
            var selectedStat = SelectStatByWeight(availableStats);
            if (selectedStat != null)
            {
                float value = GenerateStatValue(selectedStat, item.Rarity);
                SourceType sourceType = (SourceType)Enum.Parse(typeof(SourceType), item.Type.ToString());
                item.AddStat(new StatModifier(selectedStat.statType, sourceType, IncreaseType.Flat, value));

                Debug.Log($"Added stat: {selectedStat.statType} = {value}");
                availableStats.Remove(selectedStat);
            }
        }
    }

    private void GenerateEffects(ItemData item)
    {
        if (item.EffectRanges == null || item.EffectRanges.possibleEffects == null)
        {
            Debug.LogWarning($"No effect ranges defined for item: {item.ID}");
            return;
        }

        item.Effects.Clear();

        int additionalEffects = item.EffectRanges.additionalEffectsByRarity.GetValueOrDefault(item.Rarity, 0);
        int effectCount = Random.Range(
            item.EffectRanges.minEffectCount,
            Mathf.Min(item.EffectRanges.maxEffectCount + additionalEffects + 1,
                     item.EffectRanges.possibleEffects.Count)
        );

        Debug.Log($"Generating {effectCount} effects for item {item.ID}");

        var availableEffects = item.EffectRanges.possibleEffects
            .ToList();

        for (int i = 0; i < effectCount && availableEffects.Any(); i++)
        {
            var selectedEffect = SelectEffectByWeight(availableEffects);
            if (selectedEffect != null)
            {
                float value = GenerateEffectValue(selectedEffect, item.Rarity);
                var effectData = new ItemEffectData
                {
                    effectId = selectedEffect.effectId,
                    effectName = selectedEffect.effectName,
                    effectType = selectedEffect.effectType,
                    value = value,
                    applicableSkills = selectedEffect.applicableSkills,
                    applicableElements = selectedEffect.applicableElements
                };

                item.AddEffect(effectData);
                Debug.Log($"Added effect: {effectData.effectName} with value {value}");
                availableEffects.Remove(selectedEffect);
            }
        }
    }

    private ItemStatRange SelectStatByWeight(List<ItemStatRange> stats)
    {
        float totalWeight = stats.Sum(s => s.weight);
        float randomValue = (float)(Random.value * totalWeight);

        float currentWeight = 0;
        foreach (var stat in stats)
        {
            currentWeight += stat.weight;
            if (randomValue <= currentWeight)
            {
                return stat;
            }
        }

        return stats.LastOrDefault();
    }

    private ItemEffectRange SelectEffectByWeight(List<ItemEffectRange> effects)
    {
        float totalWeight = effects.Sum(e => e.weight);
        float randomValue = (float)(Random.value * totalWeight);

        float currentWeight = 0;
        foreach (var effect in effects)
        {
            currentWeight += effect.weight;
            if (randomValue <= currentWeight)
            {
                return effect;
            }
        }

        return effects.LastOrDefault();
    }

    private float GenerateStatValue(ItemStatRange statRange, ItemRarity rarity)
    {
        float baseValue = (float)(Random.value * (statRange.maxValue - statRange.minValue) + statRange.minValue);

        float rarityMultiplier = 1 + ((int)rarity * 0.2f);
        float finalValue = baseValue * rarityMultiplier;

        switch (statRange.increaseType)
        {
            case IncreaseType.Flat:
                finalValue = Mathf.Round(finalValue);
                break;
            case IncreaseType.Multiply:
                finalValue = Mathf.Round(finalValue * 100) / 100;
                break;
        }

        return finalValue;
    }

    private float GenerateEffectValue(ItemEffectRange effectRange, ItemRarity rarity)
    {
        float baseValue = (float)(Random.value * (effectRange.maxValue - effectRange.minValue) + effectRange.minValue);
        float rarityMultiplier = 1 + ((int)rarity * 0.2f);
        return baseValue * rarityMultiplier;
    }

    public List<ItemData> GenerateDrops(DropTableData dropTable, float luckMultiplier = 1f)
    {
        if (dropTable == null || dropTable.dropEntries == null)
        {
            Debug.LogWarning("Invalid drop table");
            return new List<ItemData>();
        }

        var drops = new List<ItemData>();
        int dropCount = 0;

        if (Random.value < dropTable.guaranteedDropRate)
        {
            var guaranteedDrop = GenerateGuaranteedDrop(dropTable);
            if (guaranteedDrop != null)
            {
                drops.Add(guaranteedDrop);
                dropCount++;
            }
        }

        foreach (var entry in dropTable.dropEntries)
        {
            if (dropCount >= dropTable.maxDrops) break;

            float adjustedDropRate = entry.dropRate * luckMultiplier;
            if (Random.value < adjustedDropRate)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    item.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    drops.Add(item);
                    dropCount++;
                    Debug.Log($"Generated drop: {item.Name} x{item.amount}");
                }
            }
        }

        return drops;
    }

    private ItemData GenerateGuaranteedDrop(DropTableData dropTable)
    {
        float totalWeight = dropTable.dropEntries.Sum(entry => entry.dropRate);
        float randomValue = Random.value * totalWeight;

        float currentWeight = 0;
        foreach (var entry in dropTable.dropEntries)
        {
            currentWeight += entry.dropRate;
            if (randomValue <= currentWeight)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    item.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    Debug.Log($"Generated guaranteed drop: {item.Name} x{item.amount}");
                    return item;
                }
            }
        }

        return null;
    }
}
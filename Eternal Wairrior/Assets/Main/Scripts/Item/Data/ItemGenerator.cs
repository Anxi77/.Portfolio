using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    private Dictionary<string, ItemData> itemDatabase;
    private System.Random random = new System.Random();

    public ItemGenerator(Dictionary<string, ItemData> database)
    {
        itemDatabase = database;
    }

    public ItemData GenerateItem(string itemId, ItemRarity? targetRarity = null)
    {
        if (!itemDatabase.TryGetValue(itemId, out var baseItem))
        {
            Debug.LogError($"Item not found in database: {itemId}");
            return null;
        }

        var newItem = baseItem.Clone();

        // ���Ƽ ����
        if (targetRarity.HasValue)
        {
            newItem.Rarity = targetRarity.Value;
        }

        Debug.Log($"Generating item: {newItem.Name} with rarity: {newItem.Rarity}");

        // ���� ����
        GenerateStats(newItem);

        // ����Ʈ ����
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

        // Ƽ  ߰   
        int additionalStats = item.StatRanges.additionalStatsByRarity.GetValueOrDefault(item.Rarity, 0);
        int statCount = random.Next(
            item.StatRanges.minStatCount,
            Mathf.Min(item.StatRanges.maxStatCount + additionalStats + 1,
                     item.StatRanges.possibleStats.Count)
        );

        Debug.Log($"Generating {statCount} stats for item {item.ID}");

        // ����ġ ��� ���� ����
        var availableStats = item.StatRanges.possibleStats
            .Where(stat => stat.minRarity <= item.Rarity)
            .ToList();

        for (int i = 0; i < statCount && availableStats.Any(); i++)
        {
            var selectedStat = SelectStatByWeight(availableStats);
            if (selectedStat != null)
            {
                float value = GenerateStatValue(selectedStat, item.Rarity);
                item.AddStat(new StatContainer
                {
                    statType = selectedStat.statType,
                    amount = value,
                    sourceType = selectedStat.sourceType
                });

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

        // Ƽ  ߰ Ʈ  
        int additionalEffects = item.EffectRanges.additionalEffectsByRarity.GetValueOrDefault(item.Rarity, 0);
        int effectCount = random.Next(
            item.EffectRanges.minEffectCount,
            Mathf.Min(item.EffectRanges.maxEffectCount + additionalEffects + 1,
                     item.EffectRanges.possibleEffects.Count)
        );

        Debug.Log($"Generating {effectCount} effects for item {item.ID}");

        // ����ġ ��� ����Ʈ ����
        var availableEffects = item.EffectRanges.possibleEffects
            .Where(effect => effect.minRarity <= item.Rarity)
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
                    applicableTypes = selectedEffect.applicableTypes,
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
        float randomValue = (float)(random.NextDouble() * totalWeight);

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
        float randomValue = (float)(random.NextDouble() * totalWeight);

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
        float baseValue = (float)(random.NextDouble() * (statRange.maxValue - statRange.minValue) + statRange.minValue);

        // ���Ƽ�� ���� �� ����
        float rarityMultiplier = 1 + ((int)rarity * 0.2f);
        float finalValue = baseValue * rarityMultiplier;

        // ���� Ÿ�Կ� ���� ó��
        switch (statRange.increaseType)
        {
            case IncreaseType.Add:
                finalValue = Mathf.Round(finalValue);
                break;
            case IncreaseType.Mul:
                finalValue = Mathf.Round(finalValue * 100) / 100;
                break;
        }

        return finalValue;
    }

    private float GenerateEffectValue(ItemEffectRange effectRange, ItemRarity rarity)
    {
        float baseValue = (float)(random.NextDouble() * (effectRange.maxValue - effectRange.minValue) + effectRange.minValue);
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

        // ����� ��� üũ
        if (random.NextDouble() < dropTable.guaranteedDropRate)
        {
            var guaranteedDrop = GenerateGuaranteedDrop(dropTable);
            if (guaranteedDrop != null)
            {
                drops.Add(guaranteedDrop);
                dropCount++;
            }
        }

        // �Ϲ� ��� ����
        foreach (var entry in dropTable.dropEntries)
        {
            if (dropCount >= dropTable.maxDrops) break;

            float adjustedDropRate = entry.dropRate * luckMultiplier;
            if (random.NextDouble() < adjustedDropRate)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    // ������ ���� ����
                    item.amount = random.Next(entry.minAmount, entry.maxAmount + 1);
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
        // ����ġ �հ� ���
        float totalWeight = dropTable.dropEntries.Sum(entry => entry.dropRate);
        float randomValue = (float)(random.NextDouble() * totalWeight);

        // ����ġ ��� ������ ����
        float currentWeight = 0;
        foreach (var entry in dropTable.dropEntries)
        {
            currentWeight += entry.dropRate;
            if (randomValue <= currentWeight)
            {
                var item = GenerateItem(entry.itemId, entry.rarity);
                if (item != null)
                {
                    item.amount = random.Next(entry.minAmount, entry.maxAmount + 1);
                    Debug.Log($"Generated guaranteed drop: {item.Name} x{item.amount}");
                    return item;
                }
            }
        }

        return null;
    }
}
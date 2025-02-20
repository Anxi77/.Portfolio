using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerStatSystem : MonoBehaviour
{
    public Player player;
    private Dictionary<StatType, float> currentStats = new();
    private Dictionary<SourceType, List<StatModifier>> activeModifiers = new();

    public event Action<StatType, float> OnStatChanged;

    private void Start()
    {
        InitializeStats();
        player = GetComponent<Player>();
        StringBuilder sb = new();
        foreach (var stat in currentStats)
        {
            sb.AppendLine($"{stat.Key}: {stat.Value}");
        }
        print(sb);
    }

    private void InitializeStats()
    {
        var defaultData = PlayerDataManager.Instance.CurrentPlayerStatData;
        LoadFromSaveData(defaultData);
    }

    public void LoadFromSaveData(PlayerStatData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("[PlayerStatSystem] Save Data is null");
            return;
        }

        currentStats = new Dictionary<StatType, float>(saveData.baseStats);

        float maxHp = currentStats.GetValueOrDefault(StatType.MaxHp);
        currentStats[StatType.CurrentHp] = maxHp;

        activeModifiers.Clear();
        foreach (var modifierData in saveData.permanentModifiers)
        {
            AddModifier(
                new StatModifier(
                    modifierData.statType,
                    modifierData.sourceType,
                    modifierData.increaseType,
                    modifierData.amount
                )
            );
        }

        RecalculateAllStats();
    }

    public PlayerStatData CreateSaveData()
    {
        var saveData = new PlayerStatData();

        foreach (var stat in currentStats)
        {
            saveData.baseStats[stat.Key] = GetBaseValue(stat.Key);
        }

        foreach (var modifierList in activeModifiers.Values)
        {
            foreach (var modifier in modifierList.Where(m => IsPermanentSource(m.Source)))
            {
                saveData.permanentModifiers.Add(
                    new StatModifierSaveData(
                        modifier.Type,
                        modifier.Source,
                        modifier.IncreaseType,
                        modifier.Value
                    )
                );
            }
        }

        return saveData;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (!activeModifiers.ContainsKey(modifier.Source))
        {
            activeModifiers[modifier.Source] = new List<StatModifier>();
        }

        activeModifiers[modifier.Source].Add(modifier);
        RecalculateStats(modifier.Type);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (activeModifiers.ContainsKey(modifier.Source))
        {
            activeModifiers[modifier.Source].Remove(modifier);
            RecalculateStats(modifier.Type);
        }
    }

    private void RecalculateStats(StatType statType)
    {
        float baseValue = GetBaseValue(statType);
        float addValue = 0;
        float mulValue = 1f;

        foreach (var modifierList in activeModifiers.Values)
        {
            foreach (var modifier in modifierList.Where(m => m.Type == statType))
            {
                if (modifier.IncreaseType == IncreaseType.Flat)
                    addValue += modifier.Value;
                else
                    mulValue *= (1 + modifier.Value);
            }
        }

        float newValue = (baseValue + addValue) * mulValue;
        if (
            currentStats.ContainsKey(statType)
            && !Mathf.Approximately(currentStats[statType], newValue)
        )
        {
            currentStats[statType] = newValue;
            OnStatChanged?.Invoke(statType, newValue);
        }
    }

    private void RecalculateAllStats()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            RecalculateStats(statType);
        }
    }

    public float GetStat(StatType type)
    {
        return currentStats.TryGetValue(type, out float value) ? value : 0f;
    }

    private float GetBaseValue(StatType type)
    {
        return currentStats.TryGetValue(type, out float value) ? value : 0f;
    }

    private bool IsPermanentSource(SourceType source)
    {
        return source == SourceType.Weapon
            || source == SourceType.Armor
            || source == SourceType.Accessory
            || source == SourceType.Special;
    }

    public void SetCurrentHp(float value)
    {
        float maxHp = GetStat(StatType.MaxHp);
        float newHp = Mathf.Clamp(value, 0, maxHp);

        if (
            !currentStats.ContainsKey(StatType.CurrentHp)
            || !Mathf.Approximately(currentStats[StatType.CurrentHp], newHp)
        )
        {
            currentStats[StatType.CurrentHp] = newHp;
            OnStatChanged?.Invoke(StatType.CurrentHp, newHp);
        }
    }

    public void ActivateHoming(bool activate)
    {
        foreach (var skill in player.skills)
        {
            if (skill is ProjectileSkills ProjectileSkills)
            {
                ProjectileSkills.UpdateHomingState(activate);
            }
        }
    }

    public void RemoveStatsBySource(SourceType source)
    {
        if (activeModifiers.ContainsKey(source))
        {
            foreach (var modifier in activeModifiers[source])
            {
                RemoveModifier(modifier);
            }
        }
    }

    public void RemoveStatsBySource(StatType statType, SourceType source)
    {
        if (activeModifiers.ContainsKey(source))
        {
            if (activeModifiers[source].Any(modifier => modifier.Type == statType))
            {
                RemoveModifier(
                    activeModifiers[source].First(modifier => modifier.Type == statType)
                );
            }
        }
    }

    public void UpdateStatsForLevel(int level)
    {
        // update stats for level
    }

    public float GetCurrentHp() => GetStat(StatType.CurrentHp);

    public float GetMaxHp() => GetStat(StatType.MaxHp);
}

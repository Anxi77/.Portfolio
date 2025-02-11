using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerStatSystem : MonoBehaviour
{
    public Player player;
    private Dictionary<StatType, float> currentStats = new();
    private Dictionary<SourceType, List<StatModifier>> activeModifiers = new();

    public event Action<StatType, float> OnStatChanged;

    private void Awake()
    {
        InitializeStats();
        player = GetComponent<Player>();
    }

    private void InitializeStats()
    {
        var defaultData = new PlayerStatData();
        LoadFromSaveData(defaultData);
    }

    public void LoadFromSaveData(PlayerStatData saveData)
    {
        // 기본 스탯 초기화
        currentStats = new Dictionary<StatType, float>(saveData.baseStats);

        // 영구 수정자 적용
        activeModifiers.Clear();
        foreach (var modifierData in saveData.permanentModifiers)
        {
            AddModifier(new StatModifier(
                modifierData.statType,
                modifierData.sourceType,
                modifierData.increaseType,
                modifierData.amount
            ));
        }

        RecalculateAllStats();
    }

    public PlayerStatData CreateSaveData()
    {
        var saveData = new PlayerStatData();

        // 기본 스탯 저장
        foreach (var stat in currentStats)
        {
            saveData.baseStats[stat.Key] = GetBaseValue(stat.Key);
        }

        // 영구 수정자 저장
        foreach (var modifierList in activeModifiers.Values)
        {
            foreach (var modifier in modifierList.Where(m => IsPermanentSource(m.Source)))
            {
                saveData.permanentModifiers.Add(new PlayerStatData.StatModifierSaveData(
                    modifier.Type,
                    modifier.Source,
                    modifier.IncreaseType,
                    modifier.Value
                ));
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
        if (currentStats.ContainsKey(statType) && !Mathf.Approximately(currentStats[statType], newValue))
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
        return source == SourceType.Weapon ||
               source == SourceType.Armor ||
               source == SourceType.Accessory ||
               source == SourceType.Special;
    }

    public void SetCurrentHp(float value)
    {
        float maxHp = GetStat(StatType.MaxHp);
        float newHp = Mathf.Clamp(value, 0, maxHp);

        if (!currentStats.ContainsKey(StatType.CurrentHp) ||
            !Mathf.Approximately(currentStats[StatType.CurrentHp], newHp))
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
                RemoveModifier(activeModifiers[source].First(modifier => modifier.Type == statType));
            }
        }
    }

    public void UpdateStatsForLevel(int level)
    {
        // 레벨에 따라 스탯 업데이트
    }

    public float GetCurrentHp() => GetStat(StatType.CurrentHp);
    public float GetMaxHp() => GetStat(StatType.MaxHp);
}
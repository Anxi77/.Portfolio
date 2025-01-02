using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class UnitStats : MonoBehaviour, ICharacterStats
{
    [System.Serializable]
    public class StatInitializer
    {
        public StatType Type;
        public float BaseValue;
        public float PointIncrease = 1f;
    }

    protected Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

    protected abstract StatInitializer[] GetInitialStats();

    public virtual void InitializeStats()
    {
        stats.Clear();
        var initialStats = GetInitialStats();

        //Debug.Log($"Initializing stats with {initialStats.Length} values");

        foreach (var statInit in initialStats)
        {
            //Debug.Log($"Initializing {statInit.Type} with base value {statInit.BaseValue}");
            stats[statInit.Type] = new FloatStat(statInit.BaseValue, statInit.PointIncrease);

            if (statInit.Type == StatType.MaxHealth)
            {
                stats[StatType.Health] = new FloatStat(statInit.BaseValue, statInit.PointIncrease);
            }
            if (statInit.Type == StatType.MaxMana)
            {
                stats[StatType.Mana] = new FloatStat(statInit.BaseValue, statInit.PointIncrease);
            }
        }
    }

    private void CheckStats()
    {
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            if (!stats.ContainsKey(statType))
            {
                stats[statType] = new FloatStat(0f);
                //Debug.Log($"{statType}");
            }
        }
    }

    public virtual float GetStatValue(StatType statType)
    {
        if (stats.TryGetValue(statType, out Stat stat))
        {
            return (float)stat.Value;
        }
        //Debug.LogWarning($" {statType} 이 없는데 접근하려 합니다."); 
        return 0f;
    }

    public virtual void AddModifier(StatType statType, StatModifier modifier)
    {
        if (stats.TryGetValue(statType, out Stat stat))
        {
            if (statType == StatType.MaxHealth)
            {
                float oldMaxHealth = (float)stat.Value;
                stat.AddModifier(modifier);
                float newMaxHealth = (float)stat.Value;

                if (oldMaxHealth > 0 && stats.TryGetValue(StatType.Health, out Stat healthStat))
                {
                    float healthRatio = (float)healthStat.Value / oldMaxHealth;
                    float newHealth = newMaxHealth * healthRatio;

                    healthStat.RemoveAllModifiersFromSource(modifier.Source);
                    ((FloatStat)healthStat).InvestPoint(newHealth - (float)healthStat.Value);
                }
            }
            else
            {
                stat.AddModifier(modifier);
            }
        }
    }

    public virtual void RemoveModifier(StatType statType, StatModifier modifier)
    {
        if (stats.TryGetValue(statType, out Stat stat))
        {
            stat.RemoveModifier(modifier);
        }
    }

    public Dictionary<StatType, float> GetCurrentStats()
    {
        Dictionary<StatType, float> currentStats = new Dictionary<StatType, float>();
        foreach (var stat in stats)
        {
            currentStats[stat.Key] = (float)stat.Value.Value;
        }
        return currentStats;
    }

    public Stat GetStat(StatType statType)
    {
        if (stats.TryGetValue(statType, out Stat stat))
        {
            return stat;
        }
        //Debug.Log($"Stat {statType} 이 없는데 접근하려 합니다.");
        return null;
    }

    public virtual float GetPointIncreaseAmount(StatType statType)
    {
        var initialStats = GetInitialStats();
        var statInit = initialStats.FirstOrDefault(s => s.Type == statType);

        if (statInit != null)
        {
            return statInit.PointIncrease;
        }

        return 0f;
    }

    public virtual void SetStatCurrentValue(StatType statType, float value)
    {
        if (stats.TryGetValue(statType, out Stat stat))
        {
            if (stat is FloatStat floatStat)
            {
                floatStat.SetBaseValue(value);
            }
        }
    }
}

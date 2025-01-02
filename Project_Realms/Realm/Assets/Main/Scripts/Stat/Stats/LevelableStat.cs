using UnityEngine;

public class LevelableStat : Stat
{
    protected object baseValue;
    protected object lastValue;
    protected float growthValue;  // 레벨당 증가량
    protected float growthRate;   // 레벨당 증가율 (퍼센트)
    protected int currentLevel = 1;
    protected bool isInteger;

    public LevelableStat(object baseValue, float growthValue = 0f, float growthRate = 0f)
        : base()
    {
        this.baseValue = baseValue;
        this.growthValue = growthValue;
        this.growthRate = growthRate;
        this.isInteger = baseValue is int;
        this.lastValue = baseValue;
    }

    public override object Value
    {
        get
        {
            if (isDirty)
            {
                CalculateFinalValue();
                isDirty = false;
            }
            return lastValue;
        }
    }

    public void SetLevel(int level)
    {
        if (level < 1) level = 1;
        currentLevel = level;
        isDirty = true;
    }

    public override void InvestPoint(float increaseAmount)
    {
        investedPoints++;
        if (isInteger)
        {
            baseValue = (int)baseValue + Mathf.RoundToInt(increaseAmount);
        }
        else
        {
            baseValue = (float)baseValue + increaseAmount;
        }
        isDirty = true;
    }

    protected override void CalculateFinalValue()
    {
        float baseValueFloat = isInteger ? (int)baseValue : (float)baseValue;
        float leveledBase = baseValueFloat;

        leveledBase += growthValue * (currentLevel - 1);

        if (growthRate > 0)
        {
            leveledBase *= Mathf.Pow(1 + growthRate, currentLevel - 1);
        }

        // 이후 기존 modifier 계산
        float finalValue = leveledBase;
        float sumPercentAdd = 0;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];

            if (mod.Type == StatModifierType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModifierType.PercentAdd)
            {
                sumPercentAdd += mod.Value;

                if (i + 1 >= modifiers.Count || modifiers[i + 1].Type != StatModifierType.PercentAdd)
                {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0;
                }
            }
            else if (mod.Type == StatModifierType.PercentMult)
            {
                finalValue *= 1 + mod.Value;
            }
        }

        lastValue = isInteger ? Mathf.RoundToInt(finalValue) : finalValue;
    }

    public void SetBaseValue(object newBaseValue)
    {
        if (isInteger && newBaseValue is int intValue)
        {
            baseValue = intValue;
        }
        else if (!isInteger && newBaseValue is float floatValue)
        {
            baseValue = floatValue;
        }
        isDirty = true;
    }
}
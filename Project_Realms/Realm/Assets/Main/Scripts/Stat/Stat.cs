using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Stat
{
    protected readonly List<StatModifier> modifiers;
    protected bool isDirty = true;
    protected int investedPoints = 0;

    protected Stat()
    {
        modifiers = new List<StatModifier>();
    }

    public abstract object Value { get; }
    public abstract void InvestPoint(float increaseAmount);

    #region ���� ���� ���� �޼����
    public virtual void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        isDirty = true;
    }

    public virtual void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
        isDirty = true;
    }

    protected abstract void CalculateFinalValue();

    public int GetInvestedPoints() => investedPoints;
    #endregion

    #region ��ó�� ���Ȱ�� ���۸޼���
    public List<StatModifier> GetModifiersFromSource(SourceType sourceType)
    {
        return modifiers.Where(mod => mod.SourceType == sourceType).ToList();
    }

    public bool HasModifierFromSource(SourceType sourceType)
    {
        return modifiers.Any(mod => mod.SourceType == sourceType);
    }

    public void RemoveAllModifiersFromSource(SourceType sourceType)
    {
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].SourceType == sourceType)
            {
                modifiers.RemoveAt(i);
                isDirty = true;
            }
        }
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;

        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].Source == source)
            {
                modifiers.RemoveAt(i);
                isDirty = true;
                didRemove = true;
            }
        }
        return didRemove;
    }

    public float GetTotalValueFromSource(SourceType sourceType)
    {
        float value = 0f;
        float sumPercentAdd = 0;
        var sourceModifiers = GetModifiersFromSource(sourceType);

        foreach (var mod in sourceModifiers.Where(m => m.Type == StatModifierType.Flat))
        {
            value += mod.Value;
        }

        foreach (var mod in sourceModifiers.Where(m => m.Type == StatModifierType.PercentAdd))
        {
            sumPercentAdd += mod.Value;
        }
        if (sumPercentAdd != 0)
        {
            value *= (1 + sumPercentAdd);
        }

        foreach (var mod in sourceModifiers.Where(m => m.Type == StatModifierType.PercentMult))
        {
            value *= (1 + mod.Value);
        }

        return value;
    }
    #endregion

    public void ClearAllModifiers()
    {
        modifiers.Clear();
        isDirty = true;
    }
}

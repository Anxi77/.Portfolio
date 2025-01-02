public class FloatStat : Stat
{
    protected float baseValue;
    public float BaseValue => baseValue;
    protected float lastValue;
    protected float pointIncrease;

    public FloatStat(float baseValue, float pointIncrease = 1f) : base()
    {
        this.baseValue = baseValue;
        this.pointIncrease = pointIncrease;
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

    public override void InvestPoint(float increaseAmount)
    {
        investedPoints++;
        baseValue += increaseAmount;
        isDirty = true;
    }

    protected override void CalculateFinalValue()
    {
        float finalValue = baseValue;
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

        lastValue = finalValue;
    }

    public void SetBaseValue(float value)
    {
        baseValue = value;
        isDirty = true;
    }
}
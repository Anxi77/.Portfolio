using System;

public class StatModifier
{
    public StatType Type { get; }
    public SourceType Source { get; }
    public IncreaseType IncreaseType { get; }
    public float Value { get; }

    public StatModifier(StatType type, SourceType source, IncreaseType increaseType, float value)
    {
        Type = type;
        Source = source;
        IncreaseType = increaseType;
        Value = value;
    }

    public override bool Equals(object obj)
    {
        if (obj is StatModifier other)
        {
            return Type == other.Type &&
                   Source == other.Source &&
                   IncreaseType == other.IncreaseType &&
                   Math.Abs(Value - other.Value) < float.Epsilon;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Source.GetHashCode();
            hash = hash * 23 + IncreaseType.GetHashCode();
            hash = hash * 23 + Value.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return $"[{Source}] {Type} {(IncreaseType == IncreaseType.Flat ? "+" : "x")} {Value}";
    }
}

/*
 StatModifierType의 계산 방식:
 
 1. Flat: 고정값을 더하거나 빼는 단순연산.
 예시) 기본값 100에 Flat +30 적용 => 130
 
 2. PercentAdd: 여러 퍼센트 수정자들을 먼저 더한 후 한 번에 적용.
 예시) 기본값 100에 PercentAdd +20%, +30% 적용
      => 100 * (1 + 0.2 + 0.3) = 100 * 1.5 = 150
 
 3. PercentMult: 각각의 퍼센트 수정자를 순차적으로 곱셈.
 예시) 기본값 100에 PercentMult +20%, +30% 적용
      => 100 * (1 + 0.2) * (1 + 0.3) = 100 * 1.2 * 1.3 = 156

 최종 계산 순서: Flat -> PercentAdd -> PercentMult
 예시) 기본값 100에 Flat +20, PercentAdd +50%, PercentMult +30% 적용
      1) 100 + 20 = 120 (Flat)
      2) 120 * (1 + 0.5) = 180 (PercentAdd)
      3) 180 * (1 + 0.3) = 234 (PercentMult)
*/
public class StatModifier
{
    public readonly float Value;
    public readonly StatModifierType Type;
    public readonly object Source;
    public readonly SourceType SourceType;

    public StatModifier(float value, StatModifierType type, object source = null, SourceType sourceType = SourceType.None)
    {
        Value = (type == StatModifierType.PercentAdd || type == StatModifierType.PercentMult)
            ? value / 100f
            : value;
        Type = type;
        Source = source;
        SourceType = sourceType;
    }

    public StatModifier(float value, StatModifierType type, SourceType sourceType)
        : this(value, type, null, sourceType)
    {
    }
}
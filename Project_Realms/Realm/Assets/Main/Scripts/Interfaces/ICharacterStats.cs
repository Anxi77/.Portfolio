using Unity.VisualScripting.Antlr3.Runtime.Misc;

public interface ICharacterStats
{
    void InitializeStats();
    /// <summary>
    /// 특정 스탯의 최종 계산된 값을 반환.
    /// 스탯의 현재 값만 필요할 때 사용.
    /// </summary>
    /// <param name="statType">확인하고자 하는 스탯 타입</param>
    /// <returns>모든 수정자가 적용된 최종 스탯 값</returns>
    /// <example>
    /// float currentHealth = stats.GetStatValue(StatType.Health);
    /// float attackPower = stats.GetStatValue(StatType.Attack);
    /// </example>
    float GetStatValue(StatType statType);

    /// <summary>
    /// 특정 스탯에 수정자를 추가.
    /// </summary>
    /// <param name="statType">수정하고자 하는 스탯 타입</param>
    /// <param name="modifier">추가할 스탯 수정자</param>
    void AddModifier(StatType statType, StatModifier modifier);

    /// <summary>
    /// 특정 스탯에서 수정자를 제거.
    /// </summary>
    /// <param name="statType">수정자를 제거할 스탯 타입</param>
    /// <param name="modifier">제거할 스탯 수정자</param>
    void RemoveModifier(StatType statType, StatModifier modifier);

    /// <summary>
    /// 특정 스탯의 Stat 객체를 직접 반환.
    /// 스탯 수정자를 관리하거나 특정 출처의 효과를 관리할 때 사용.
    /// GetStatValue와 달리 스탯 객체 자체에 접근하여 더 세밀한 제어가 가능.
    /// </summary>
    /// <param name="statType">가져올 스탯 타입</param>
    /// <returns>해당 스탯의 Stat 객체</returns>
    /// <example>
    /// // 특정 장비의 모든 효과 제거
    /// stats.GetStat(StatType.Attack).RemoveAllModifiersFromSource(equipmentItem);
    /// 
    /// // 버프 효과가 있는지 확인
    /// bool hasBuff = stats.GetStat(StatType.Defense).HasModifierFromSource(SourceType.Buff);
    /// </example>
    Stat GetStat(StatType statType);
}

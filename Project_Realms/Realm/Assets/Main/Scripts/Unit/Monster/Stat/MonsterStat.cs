using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : UnitStats
{
    [Header("Monster Stats")]
    [SerializeField]
    private StatInitializer[] initialStats = new StatInitializer[]
    {
        new StatInitializer()
        {
            Type = StatType.Level,
            BaseValue = 1f,
            PointIncrease = 0f,
        },
        new StatInitializer()
        {
            Type= StatType.MoveSpeed,
            BaseValue= 4f,
            PointIncrease = 0.2f,
        },
        new StatInitializer()
        {
            Type = StatType.Defense,
            BaseValue = 10f,
            PointIncrease = 2f,
        },
        new StatInitializer()
        {
            Type = StatType.DropExp,
            BaseValue= 20f,
            PointIncrease = 3f,
        },
        new StatInitializer()
        {
            Type = StatType.AttackSpeed,
            BaseValue = 1.5f,
            PointIncrease=0f,
        },
        new StatInitializer()
        {
            Type = StatType.AttackRange,
            BaseValue = 2f,
            PointIncrease =0f,

        },
        new StatInitializer()
        {
            Type = StatType.Damage,
            BaseValue = 50f,
            PointIncrease=20f,
        },
        new StatInitializer()
        {
            Type = StatType.MaxHealth,
            BaseValue = 250f,
            PointIncrease = 50f,
        },
    };

    protected override StatInitializer[] GetInitialStats()
    {
        return initialStats;
    }

    public void SetMonsterLevel(int level)
    {
        // 기본 레벨 설정
        stats[StatType.Level] = new LevelableStat(level);

        foreach (var statInit in initialStats)
        {
            if (statInit.Type == StatType.Level) continue;

            // 기존 스탯 가져오기
            if (stats.TryGetValue(statInit.Type, out Stat currentStat))
            {
                // 레벨에 따른 증가량 계산
                float increase = statInit.PointIncrease * (level - 1);

                // 레벨 보정치 추가
                AddModifier(statInit.Type,
                    new StatModifier(
                        increase,
                        StatModifierType.Flat,
                        this,
                        SourceType.Level
                    )
                );
            }
        }

        // Health를 MaxHealth와 동일하게 설정
        if (stats.ContainsKey(StatType.MaxHealth))
        {
            float maxHealth = GetStatValue(StatType.MaxHealth);
            stats[StatType.Health] = new FloatStat(maxHealth);
        }
    }
}

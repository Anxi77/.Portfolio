using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashStat : SkillStat
{

    [Header("대쉬 스킬 스탯")]
    [SerializeField]
    private StatInitializer[] initialStats = new StatInitializer[]
    {
        new StatInitializer()
        {
            Type = SkillStatType.SkillLevel,
        },
        new StatInitializer
        {
            Type= SkillStatType.ManaCost
        },
        new StatInitializer()
        {
            Type = SkillStatType.Cooldown,
        },
        new StatInitializer()
        {
            Type= SkillStatType.DashDistance,
        }
    };

    protected override StatInitializer[] GetInitialStats()
    {
        return initialStats;
    }
}

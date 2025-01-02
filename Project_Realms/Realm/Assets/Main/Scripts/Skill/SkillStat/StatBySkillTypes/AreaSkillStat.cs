using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSkillStat : SkillStat
{
    [Header("장판스킬 스텟")]
    [SerializeField]
    private StatInitializer[] initialStats = new StatInitializer[]
    {
        new StatInitializer
        {
            Type = SkillStatType.SkillLevel,
        },
        new StatInitializer
        {
            Type = SkillStatType.Damage,
        },
        new StatInitializer
        {
            Type= SkillStatType.ManaCost
        },
        new StatInitializer 
        {
            Type = SkillStatType.Duration,
        },
        new StatInitializer
        {
            Type = SkillStatType.SpawnRange,
        },
        new StatInitializer
        {
            Type = SkillStatType.SpawnScale,
        },
        new StatInitializer
        {
            Type = SkillStatType.SpawnCount,
        },
        new StatInitializer
        {
            Type = SkillStatType.SpawnInterval,
        },
        new StatInitializer
        {
            Type = SkillStatType.IsSpawnAtCursor,
        },
        new StatInitializer
        {
            Type = SkillStatType.IsSpawnAtEnemy,
        },
        new StatInitializer
        {
            Type = SkillStatType.Cooldown,
        }

    };

    protected override StatInitializer[] GetInitialStats()
    {
        return initialStats;
    }

}


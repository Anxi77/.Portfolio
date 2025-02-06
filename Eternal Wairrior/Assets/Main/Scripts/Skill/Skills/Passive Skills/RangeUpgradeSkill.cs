using UnityEngine;

public class RangeUpgradeSkill : PermanentPassiveSkill
{
    public override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return;

        if (_attackRangeIncrease > 0)
        {
            playerStat.AddModifier(new StatModifier(StatType.AttackRange, SourceType.Passive, IncreaseType.Multiply, _attackRangeIncrease / 100f));
            Debug.Log($"Applied permanent attack range increase: {_attackRangeIncrease}%");
        }

        if (_expAreaIncrease > 0)
        {
            playerStat.AddModifier(new StatModifier(StatType.ExpCollectionRadius, SourceType.Passive, IncreaseType.Multiply, _expAreaIncrease / 100f));
            Debug.Log($"Applied permanent exp collection radius increase: {_expAreaIncrease}%");
        }
    }

    public override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return;

        if (_attackRangeIncrease > 0)
        {
            playerStat.RemoveModifier(new StatModifier(StatType.AttackRange, SourceType.Passive, IncreaseType.Multiply, _attackRangeIncrease / 100f));
        }

        if (_expAreaIncrease > 0)
        {
            playerStat.RemoveModifier(new StatModifier(StatType.ExpCollectionRadius, SourceType.Passive, IncreaseType.Multiply, _expAreaIncrease / 100f));
        }
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        _attackRangeIncrease = stats.attackRangeIncrease;
        _expAreaIncrease = stats.expAreaIncrease;

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    protected override SkillData CreateDefaultSkillData()
    {
        var data = base.CreateDefaultSkillData();
        data.metadata.Name = "Range Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.player?.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return "Permanently increases attack range and experience collection radius";

        string baseDesc = "Permanently increases attack range and experience collection radius";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentRange = playerStat.GetStat(StatType.AttackRange);
            float currentExpRadius = playerStat.GetStat(StatType.ExpCollectionRadius);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Range: +{_attackRangeIncrease:F1}% (Current: {currentRange:F1})" +
                       $"\nExp Collection Radius: +{_expAreaIncrease:F1}% (Current: {currentExpRadius:F1})";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Range Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack range and experience collection radius";
    public override SkillType GetSkillType() => SkillType.Passive;
}
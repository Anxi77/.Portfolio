using UnityEngine;

public class AttackRegenUpgradeSkill : PermanentPassiveSkill
{
    public override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return;

        if (_damageIncrease > 0)
        {
            playerStat.AddModifier(new StatModifier(StatType.Damage, SourceType.Passive, IncreaseType.Multiply, _damageIncrease / 100f));
            Debug.Log($"Applied permanent damage increase: {_damageIncrease}%");
        }

        if (_hpRegenIncrease > 0)
        {
            playerStat.AddModifier(new StatModifier(StatType.HpRegenRate, SourceType.Passive, IncreaseType.Multiply, _hpRegenIncrease / 100f));
            Debug.Log($"Applied permanent HP regen rate increase: {_hpRegenIncrease}%");
        }
    }

    public override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return;

        if (_damageIncrease > 0)
        {
            playerStat.RemoveModifier(new StatModifier(StatType.Damage, SourceType.Passive, IncreaseType.Multiply, _damageIncrease / 100f));
        }

        if (_hpRegenIncrease > 0)
        {
            playerStat.RemoveModifier(new StatModifier(StatType.HpRegenRate, SourceType.Passive, IncreaseType.Multiply, _hpRegenIncrease / 100f));
        }
    }

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.player?.GetComponent<PlayerStatSystem>();
        if (playerStat == null) return "Permanently increases attack damage and HP regeneration rate";

        string baseDesc = "Permanently increases attack damage and HP regeneration rate";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentDamage = playerStat.GetStat(StatType.Damage);
            float currentHPRegen = playerStat.GetStat(StatType.HpRegenRate);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Damage: +{_damageIncrease:F1}% (Current: {currentDamage:F1})" +
                       $"\nHP Regen Rate: +{_hpRegenIncrease:F1}% (Current: {currentHPRegen:F1}/s)";
        }
        return baseDesc;
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        _damageIncrease = stats.damageIncrease;
        _hpRegenIncrease = stats.hpRegenIncrease;

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    protected override SkillData CreateDefaultSkillData()
    {
        var data = base.CreateDefaultSkillData();
        data.metadata.Name = "Combat Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    protected override string GetDefaultSkillName() => "Combat Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack damage and HP regeneration rate";
    public override SkillType GetSkillType() => SkillType.Passive;
}
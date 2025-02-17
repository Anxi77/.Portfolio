using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PassiveSkill : Skill
{
    #region Runtime Stats
    public List<StatModifier> statModifiers;

    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Passive Effect Stats")]
    [SerializeField] protected float _effectDuration = 5f;
    [SerializeField] protected float _cooldown = 10f;
    [SerializeField] protected float _triggerChance = 100f;
    [SerializeField] protected float _damageIncrease = 0f;
    [SerializeField] protected float _defenseIncrease = 0f;
    [SerializeField] protected float _expAreaIncrease = 0f;
    [SerializeField] protected bool _homingActivate = false;
    [SerializeField] protected float _hpIncrease = 0f;
    [SerializeField] protected float _moveSpeedIncrease = 0f;
    [SerializeField] protected float _attackSpeedIncrease = 0f;
    [SerializeField] protected float _attackRangeIncrease = 0f;
    [SerializeField] protected float _hpRegenIncrease = 0f;
    [SerializeField] protected bool _isPermanent = false;
    #endregion

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance?.player != null)
        {
            StopAllCoroutines();
            Player player = GameManager.Instance.player;
            var playerStat = player.GetComponent<PlayerStatSystem>();

            playerStat.RemoveStatsBySource(SourceType.Passive);
            if (_homingActivate)
                player.ActivateHoming(false);

            Debug.Log($"Removed all effects for {skillData?.Name ?? "Unknown Skill"}");
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        if (skillData == null) return;

        var playerStat = GameManager.Instance?.player?.GetComponent<PlayerStatSystem>();
        if (playerStat != null)
        {
            float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);
            Debug.Log($"Before Initialize - HP: {playerStat.GetStat(StatType.CurrentHp)}/{playerStat.GetStat(StatType.MaxHp)} ({currentHpRatio:F2})");

            InitializeSkillData();

            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = Mathf.Max(1f, newMaxHp * currentHpRatio);
            playerStat.SetCurrentHp(newCurrentHp);

            Debug.Log($"After Initialize - HP: {newCurrentHp}/{newMaxHp} ({currentHpRatio:F2})");
            if (skillData.GetCurrentTypeStat() is PassiveSkillStat passiveSkillStat)
            {
                if (!passiveSkillStat.isPermanent)
                {
                    StartCoroutine(PassiveEffectCoroutine());
                }
                else
                {
                    ApplyPassiveEffect();
                }
            }
        }
        else
        {
            Debug.LogError($"PlayerStatSystem not found for {skillData.Name}");
        }
    }

    protected override void InitializeSkillData()
    {
        if (skillData == null) return;

        var csvStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.ID,
            currentLevel,
            SkillType.Passive
        ) as PassiveSkillStat;

        if (csvStats != null)
        {
            UpdateInspectorValues(csvStats);
            skillData.SetStatsForLevel(skillData.GetCurrentTypeStat().baseStat.skillLevel, csvStats);
        }
        else
        {
            Debug.LogWarning($"No CSV data found for {skillData.Name}, using default values");
            var defaultStats = new PassiveSkillStat
            {
                baseStat = new BaseSkillStat

                {
                    damage = _damage,
                    skillLevel = currentLevel,
                    maxSkillLevel = 5,
                    element = ElementType.None,
                    elementalPower = _elementalPower
                },
                moveSpeedIncrease = _moveSpeedIncrease,
                effectDuration = _effectDuration,
                cooldown = _cooldown,
                triggerChance = _triggerChance,
                damageIncrease = _damageIncrease,
                defenseIncrease = _defenseIncrease,
                expAreaIncrease = _expAreaIncrease,
                homingActivate = _homingActivate,
                hpIncrease = _hpIncrease,

            };
            skillData.SetStatsForLevel(skillData.GetCurrentTypeStat().baseStat.skillLevel, defaultStats);
        }
    }

    protected virtual IEnumerator PassiveEffectCoroutine()
    {
        while (true)
        {
            if (Random.Range(0f, 100f) <= _triggerChance)
            {
                ApplyPassiveEffect();
            }
            yield return new WaitForSeconds(_cooldown);
        }
    }

    protected virtual void ApplyPassiveEffect()
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;

        StartCoroutine(ApplyTemporaryEffects(player));
    }

    protected virtual IEnumerator ApplyTemporaryEffects(Player player)
    {
        var playerStat = player.GetComponent<PlayerStatSystem>();
        if (playerStat == null) yield break;

        float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);
        bool anyEffectApplied = false;

        if (_damageIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.Damage, _damageIncrease);
            anyEffectApplied = true;
        }

        if (_defenseIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.Defense, _defenseIncrease);
            anyEffectApplied = true;
        }

        if (_expAreaIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.ExpCollectionRadius, _expAreaIncrease);
            anyEffectApplied = true;
        }

        if (_homingActivate)
        {
            player.ActivateHoming(true);
            anyEffectApplied = true;
        }

        if (_hpIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.MaxHp, _hpIncrease);
            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = newMaxHp * currentHpRatio;
            playerStat.SetCurrentHp(newCurrentHp);
            anyEffectApplied = true;
        }

        if (_moveSpeedIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.MoveSpeed, _moveSpeedIncrease);
            anyEffectApplied = true;
        }

        if (_attackSpeedIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.AttackSpeed, _attackSpeedIncrease);
            anyEffectApplied = true;
        }

        if (_attackRangeIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.AttackRange, _attackRangeIncrease);
            anyEffectApplied = true;
        }

        if (_hpRegenIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.HpRegenRate, _hpRegenIncrease);
            anyEffectApplied = true;
        }

        if (_homingActivate)
        {
            player.ActivateHoming(false);
        }

        if (anyEffectApplied)
        {
            yield return new WaitForSeconds(_effectDuration);

            currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);
            playerStat.RemoveStatsBySource(SourceType.Passive);

            if (_hpIncrease > 0)
            {
                float newMaxHp = playerStat.GetStat(StatType.MaxHp);
                float newCurrentHp = newMaxHp * currentHpRatio;
                playerStat.SetCurrentHp(newCurrentHp);
            }

        }
    }

    public void RemoveEffectFromPlayer(Player player)
    {
        if (player == null) return;

        PlayerStatSystem playerStat = player.GetComponent<PlayerStatSystem>();
        foreach (var modifier in statModifiers)
        {
            playerStat.RemoveModifier(modifier);
        }
        statModifiers.Clear();
    }

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is PassiveSkillStat passiveStats)
        {
            UpdateInspectorValues(passiveStats);
        }
    }

    protected virtual void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        var playerStat = GameManager.Instance?.player?.GetComponent<PlayerStatSystem>();
        float currentHpRatio = 1f;
        if (playerStat != null)
        {
            currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);
            Debug.Log($"Before UpdateInspectorValues - HP: {playerStat.GetStat(StatType.CurrentHp)}/{playerStat.GetStat(StatType.MaxHp)} ({currentHpRatio:F2})");
        }

        Debug.Log($"[PassiveSkills] Before Update - Level: {currentLevel}");

        currentLevel = stats.baseStat.skillLevel;
        _damage = stats.baseStat.damage;
        _elementalPower = stats.baseStat.elementalPower;
        _effectDuration = stats.effectDuration;
        _cooldown = stats.cooldown;
        _triggerChance = stats.triggerChance;
        _damageIncrease = stats.damageIncrease;
        _defenseIncrease = stats.defenseIncrease;
        _expAreaIncrease = stats.expAreaIncrease;
        _homingActivate = stats.homingActivate;
        _hpIncrease = stats.hpIncrease;
        _moveSpeedIncrease = stats.moveSpeedIncrease;
        _attackSpeedIncrease = stats.attackSpeedIncrease;
        _attackRangeIncrease = stats.attackRangeIncrease;
        _hpRegenIncrease = stats.hpRegenIncrease;

        Debug.Log($"[PassiveSkills] After Update - Level: {currentLevel}");

        if (playerStat != null)
        {
            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = Mathf.Max(1f, newMaxHp * currentHpRatio);
            playerStat.SetCurrentHp(newCurrentHp);
            Debug.Log($"After UpdateInspectorValues - HP: {newCurrentHp}/{newMaxHp} ({currentHpRatio:F2})");
        }
    }
    protected void ApplyStatModifier(PlayerStatSystem playerStat, StatType statType, float percentageIncrease)
    {
        if (percentageIncrease <= 0) return;

        float currentStat = playerStat.GetStat(statType);
        float increase = currentStat * (percentageIncrease / 100f);
        StatModifier modifier = new StatModifier(statType, SourceType.Passive, IncreaseType.Flat, increase);
        playerStat.AddModifier(modifier);
        statModifiers.Add(modifier);
        Debug.Log($"Applied {statType} increase: Current({currentStat}) + {percentageIncrease}% = {currentStat + increase}");
    }
}

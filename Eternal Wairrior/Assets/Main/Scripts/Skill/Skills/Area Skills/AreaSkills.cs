using UnityEngine;
using System;

public abstract class AreaSkills : Skill
{
    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Area Stats")]
    [SerializeField] protected float _radius = 5f;
    [SerializeField] protected float _duration = 5f;
    [SerializeField] protected float _tickRate = 0.1f;
    [SerializeField] protected bool _isPersistent = true;
    [SerializeField] protected float _moveSpeed = 180f;

    public float Damage => _damage;
    public float ElementalPower => _elementalPower;
    public float Radius => _radius;
    public float Duration => _duration;
    public float TickRate => _tickRate;
    public bool IsPersistent => _isPersistent;
    public float MoveSpeed => _moveSpeed;
    protected override void InitializeSkillData()
    {
        if (skillData == null) return;

        var csvStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.ID,
            skillData.GetCurrentTypeStat().baseStat.skillLevel,
            SkillType.Area
        ) as AreaSkillStat;

        if (csvStats != null)
        {
            UpdateInspectorValues(csvStats);
            skillData.SetStatsForLevel(skillData.GetCurrentTypeStat().baseStat.skillLevel, csvStats);
        }
        else
        {
            Debug.LogWarning($"No CSV data found for {skillData.Name}, using default values");
            var defaultStats = new AreaSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillLevel = currentLevel,
                    maxSkillLevel = 5,
                    element = skillData?.Element ?? ElementType.None,
                    elementalPower = _elementalPower
                },
                radius = _radius,
                duration = _duration,
                tickRate = _tickRate,
                isPersistent = _isPersistent,
                moveSpeed = _moveSpeed
            };
            skillData.SetStatsForLevel(currentLevel, defaultStats);
        }
    }

    protected AreaSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(currentLevel) as AreaSkillStat;
            if (stats == null)
            {
                stats = new AreaSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = 1,
                        maxSkillLevel = 5,
                        element = skillData?.Element ?? ElementType.None,
                        elementalPower = 1f
                    },
                    radius = _radius,
                    duration = _duration,
                    tickRate = _tickRate,
                    isPersistent = _isPersistent,
                    moveSpeed = _moveSpeed
                };
                skillData?.SetStatsForLevel(1, stats);
            }
            return stats;
        }
    }

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is AreaSkillStat areaStats)
        {
            UpdateInspectorValues(areaStats);
        }
    }

    protected virtual void UpdateInspectorValues(AreaSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        Debug.Log($"[AreaSkills] Before Update - Level: {currentLevel}");

        currentLevel = stats.baseStat.skillLevel;

        _damage = stats.baseStat.damage;
        _elementalPower = stats.baseStat.elementalPower;
        _radius = stats.radius;
        _duration = stats.duration;
        _tickRate = stats.tickRate;
        _isPersistent = stats.isPersistent;
        _moveSpeed = stats.moveSpeed;

        Debug.Log($"[AreaSkills] After Update - Level: {currentLevel}");
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = skillData?.Description ?? "Area skill description";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nRadius: {Radius:F1}" +
                       $"\nDuration: {Duration:F1}s" +
                       $"\nTick Rate: {TickRate:F1}s" +
                       $"\nMove Speed: {MoveSpeed:F1}";
        }
        return baseDesc;
    }

    protected override void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        base.OnValidate();

        if (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized)
        {
            return;
        }

        if (skillData == null)
        {
            return;
        }

        try
        {
            var currentStats = TypedStats;
            if (currentStats == null || currentStats.baseStat == null)
            {
                return;
            }

            currentStats.baseStat.damage = _damage;
            currentStats.baseStat.skillLevel = currentLevel;
            currentStats.baseStat.elementalPower = _elementalPower;
            currentStats.radius = _radius;
            currentStats.duration = _duration;
            currentStats.tickRate = _tickRate;
            currentStats.isPersistent = _isPersistent;
            currentStats.moveSpeed = _moveSpeed;

            _damage = currentStats.baseStat.damage;
            currentLevel = currentStats.baseStat.skillLevel;
            _elementalPower = currentStats.baseStat.elementalPower;
            _radius = currentStats.radius;
            _duration = currentStats.duration;
            _tickRate = currentStats.tickRate;
            _isPersistent = currentStats.isPersistent;
            _moveSpeed = currentStats.moveSpeed;

            skillData.SetStatsForLevel(currentLevel, currentStats);
            Debug.Log($"Updated stats for {GetType().Name} from inspector");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error in OnValidate for {GetType().Name}: {e.Message}");
        }
    }

    public void ModifyRadius(float multiplier)
    {
        _radius *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as AreaSkillStat;
        if (currentStats != null)
        {
            currentStats.radius = _radius;
        }
    }

    public void ModifyDuration(float multiplier)
    {
        _duration *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as AreaSkillStat;
        if (currentStats != null)
        {
            currentStats.duration = _duration;
        }
    }

    public override void ModifyDamage(float multiplier)
    {
        _damage *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat();
        if (currentStats?.baseStat != null)
        {
            currentStats.baseStat.damage = _damage;
        }
    }

    public override void ModifyCooldown(float multiplier)
    {
        _tickRate *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as AreaSkillStat;
        if (currentStats != null)
        {
            currentStats.tickRate = _tickRate;
        }
    }
}

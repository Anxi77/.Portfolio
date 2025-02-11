using UnityEngine;
using System;

[Serializable]
public class SkillStatData
{
    // Basic Info
    public SkillID SkillID { get; set; }
    public int Level { get; set; }

    // Base Stats
    public float Damage { get; set; }
    public int MaxSkillLevel { get; set; }
    public ElementType Element { get; set; }
    public float ElementalPower { get; set; }

    // Projectile Stats
    public float ProjectileSpeed { get; set; }
    public float ProjectileScale { get; set; }
    public float ShotInterval { get; set; }
    public int PierceCount { get; set; }
    public float AttackRange { get; set; }
    public float HomingRange { get; set; }
    public bool IsHoming { get; set; }
    public float ExplosionRad { get; set; }
    public int ProjectileCount { get; set; }
    public float InnerInterval { get; set; }

    // Area Stats
    public float Radius { get; set; }
    public float Duration { get; set; }
    public float TickRate { get; set; }
    public bool IsPersistent { get; set; }
    public float MoveSpeed { get; set; }

    // Passive Stats
    public float EffectDuration { get; set; }
    public float Cooldown { get; set; }
    public float TriggerChance { get; set; }
    public float DamageIncrease { get; set; }
    public float DefenseIncrease { get; set; }
    public float ExpAreaIncrease { get; set; }
    public bool HomingActivate { get; set; }
    public float HpIncrease { get; set; }
    public float MoveSpeedIncrease { get; set; }
    public float AttackSpeedIncrease { get; set; }
    public float AttackRangeIncrease { get; set; }
    public float HpRegenIncrease { get; set; }

    // 필드들을 프로퍼티로 변환하면서 기존 필드명과의 호환성을 위한 매핑
    public SkillID skillID { get => SkillID; set => SkillID = value; }
    public int level { get => Level; set => Level = value; }
    public float damage { get => Damage; set => Damage = value; }
    public int maxSkillLevel { get => MaxSkillLevel; set => MaxSkillLevel = value; }
    public ElementType element { get => Element; set => Element = value; }
    public float elementalPower { get => ElementalPower; set => ElementalPower = value; }
    public float projectileSpeed { get => ProjectileSpeed; set => ProjectileSpeed = value; }
    public float projectileScale { get => ProjectileScale; set => ProjectileScale = value; }
    public float shotInterval { get => ShotInterval; set => ShotInterval = value; }
    public int pierceCount { get => PierceCount; set => PierceCount = value; }
    public float attackRange { get => AttackRange; set => AttackRange = value; }
    public float homingRange { get => HomingRange; set => HomingRange = value; }
    public bool isHoming { get => IsHoming; set => IsHoming = value; }
    public float explosionRad { get => ExplosionRad; set => ExplosionRad = value; }
    public int projectileCount { get => ProjectileCount; set => ProjectileCount = value; }
    public float innerInterval { get => InnerInterval; set => InnerInterval = value; }
    public float radius { get => Radius; set => Radius = value; }
    public float duration { get => Duration; set => Duration = value; }
    public float tickRate { get => TickRate; set => TickRate = value; }
    public bool isPersistent { get => IsPersistent; set => IsPersistent = value; }
    public float moveSpeed { get => MoveSpeed; set => MoveSpeed = value; }
    public float effectDuration { get => EffectDuration; set => EffectDuration = value; }
    public float cooldown { get => Cooldown; set => Cooldown = value; }
    public float triggerChance { get => TriggerChance; set => TriggerChance = value; }
    public float damageIncrease { get => DamageIncrease; set => DamageIncrease = value; }
    public float defenseIncrease { get => DefenseIncrease; set => DefenseIncrease = value; }
    public float expAreaIncrease { get => ExpAreaIncrease; set => ExpAreaIncrease = value; }
    public bool homingActivate { get => HomingActivate; set => HomingActivate = value; }
    public float hpIncrease { get => HpIncrease; set => HpIncrease = value; }
    public float moveSpeedIncrease { get => MoveSpeedIncrease; set => MoveSpeedIncrease = value; }
    public float attackSpeedIncrease { get => AttackSpeedIncrease; set => AttackSpeedIncrease = value; }
    public float attackRangeIncrease { get => AttackRangeIncrease; set => AttackRangeIncrease = value; }
    public float hpRegenIncrease { get => HpRegenIncrease; set => HpRegenIncrease = value; }

    public SkillStatData()
    {
        skillID = SkillID.None;
        level = 1;
        damage = 10f;
        maxSkillLevel = 5;
        element = ElementType.None;
        elementalPower = 1f;

        projectileSpeed = 10f;
        projectileScale = 1f;
        shotInterval = 1f;
        pierceCount = 1;
        attackRange = 10f;
        homingRange = 5f;
        isHoming = false;
        explosionRad = 0f;
        projectileCount = 1;
        innerInterval = 0.1f;

        radius = 5f;
        duration = 3f;
        tickRate = 1f;
        isPersistent = false;
        moveSpeed = 0f;

        effectDuration = 5f;
        cooldown = 10f;
        triggerChance = 100f;
        damageIncrease = 0f;
        defenseIncrease = 0f;
        expAreaIncrease = 0f;
        homingActivate = false;
        hpIncrease = 0f;
        moveSpeedIncrease = 0f;
        attackSpeedIncrease = 0f;
        attackRangeIncrease = 0f;
        hpRegenIncrease = 0f;
    }

    public SkillStatData Clone()
    {
        return new SkillStatData
        {
            skillID = this.skillID,
            level = this.level,
            damage = this.damage,
            maxSkillLevel = this.maxSkillLevel,
            element = this.element,
            elementalPower = this.elementalPower,

            projectileSpeed = this.projectileSpeed,
            projectileScale = this.projectileScale,
            shotInterval = this.shotInterval,
            pierceCount = this.pierceCount,
            attackRange = this.attackRange,
            homingRange = this.homingRange,
            isHoming = this.isHoming,
            explosionRad = this.explosionRad,
            projectileCount = this.projectileCount,
            innerInterval = this.innerInterval,

            radius = this.radius,
            duration = this.duration,
            tickRate = this.tickRate,
            isPersistent = this.isPersistent,
            moveSpeed = this.moveSpeed,

            effectDuration = this.effectDuration,
            cooldown = this.cooldown,
            triggerChance = this.triggerChance,
            damageIncrease = this.damageIncrease,
            defenseIncrease = this.defenseIncrease,
            expAreaIncrease = this.expAreaIncrease,
            homingActivate = this.homingActivate,
            hpIncrease = this.hpIncrease,
            moveSpeedIncrease = this.moveSpeedIncrease,
            attackSpeedIncrease = this.attackSpeedIncrease,
            attackRangeIncrease = this.attackRangeIncrease,
            hpRegenIncrease = this.hpRegenIncrease
        };
    }

    public ISkillStat CreateSkillStat(SkillType skillType)
    {
        var baseStats = new BaseSkillStat
        {
            damage = this.damage,
            maxSkillLevel = this.maxSkillLevel,
            skillLevel = this.level,
            element = this.element,
            elementalPower = this.elementalPower
        };

        switch (skillType)
        {
            case SkillType.Projectile:
                return new ProjectileSkillStat
                {
                    baseStat = baseStats,
                    projectileSpeed = projectileSpeed,
                    projectileScale = projectileScale,
                    shotInterval = shotInterval,
                    pierceCount = pierceCount,
                    attackRange = attackRange,
                    homingRange = homingRange,
                    isHoming = isHoming,
                    explosionRad = explosionRad,
                    projectileCount = projectileCount,
                    innerInterval = innerInterval
                };

            case SkillType.Area:
                return new AreaSkillStat
                {
                    baseStat = baseStats,
                    radius = radius,
                    duration = duration,
                    tickRate = tickRate,
                    isPersistent = isPersistent,
                    moveSpeed = moveSpeed
                };

            case SkillType.Passive:
                return new PassiveSkillStat
                {
                    baseStat = baseStats,
                    effectDuration = effectDuration,
                    cooldown = cooldown,
                    triggerChance = triggerChance,
                    damageIncrease = damageIncrease,
                    defenseIncrease = defenseIncrease,
                    expAreaIncrease = expAreaIncrease,
                    homingActivate = homingActivate,
                    hpIncrease = hpIncrease,
                    moveSpeedIncrease = moveSpeedIncrease,
                    attackSpeedIncrease = attackSpeedIncrease,
                    attackRangeIncrease = attackRangeIncrease,
                    hpRegenIncrease = hpRegenIncrease
                };

            default:
                throw new System.ArgumentException($"Invalid skill type: {skillType}");
        }
    }

    public bool ValidateStats()
    {
        // ⺻ 
        if (level <= 0)
        {
            Debug.LogError($"Invalid level value for skill {skillID}: {level}");
            return false;
        }
        if (damage < 0)
        {
            Debug.LogError($"Invalid damage value for skill {skillID}: {damage}");
            return false;
        }
        if (maxSkillLevel <= 0)
        {
            Debug.LogError($"Invalid max skill level for skill {skillID}: {maxSkillLevel}");
            return false;
        }

        // ŸԺ
        var skillData = SkillDataManager.Instance.GetSkillData(skillID);
        if (skillData == null)
        {
            Debug.LogError($"Could not find skill data for {skillID}");
            return false;
        }

        switch (skillData.type)
        {
            case SkillType.Projectile:
                return ValidateProjectileStats();
            case SkillType.Area:
                return ValidateAreaStats();
            case SkillType.Passive:
                return ValidatePassiveStats();
            default:
                Debug.LogError($"Unknown skill type for {skillID}");
                return false;
        }
    }

    private bool ValidateProjectileStats()
    {
        if (projectileSpeed < 0)
        {
            Debug.LogError($"Invalid projectile speed for skill {skillID}: {projectileSpeed}");
            return false;
        }
        if (projectileScale <= 0)
        {
            Debug.LogError($"Invalid projectile scale for skill {skillID}: {projectileScale}");
            return false;
        }
        if (shotInterval < 0)
        {
            Debug.LogError($"Invalid shot interval for skill {skillID}: {shotInterval}");
            return false;
        }
        if (pierceCount < 0)
        {
            Debug.LogError($"Invalid pierce count for skill {skillID}: {pierceCount}");
            return false;
        }
        if (attackRange <= 0)
        {
            Debug.LogError($"Invalid attack range for skill {skillID}: {attackRange}");
            return false;
        }
        if (homingRange < 0)
        {
            Debug.LogError($"Invalid homing range for skill {skillID}: {homingRange}");
            return false;
        }
        if (explosionRad < 0)
        {
            Debug.LogError($"Invalid explosion radius for skill {skillID}: {explosionRad}");
            return false;
        }
        if (projectileCount <= 0)
        {
            Debug.LogError($"Invalid projectile count for skill {skillID}: {projectileCount}");
            return false;
        }
        if (innerInterval < 0)
        {
            Debug.LogError($"Invalid inner interval for skill {skillID}: {innerInterval}");
            return false;
        }
        return true;
    }

    private bool ValidateAreaStats()
    {
        if (radius <= 0)
        {
            Debug.LogError($"Invalid radius for skill {skillID}: {radius}");
            return false;
        }
        if (duration <= 0)
        {
            Debug.LogError($"Invalid duration for skill {skillID}: {duration}");
            return false;
        }
        if (tickRate <= 0)
        {
            Debug.LogError($"Invalid tick rate for skill {skillID}: {tickRate}");
            return false;
        }
        if (moveSpeed < 0)
        {
            Debug.LogError($"Invalid move speed for skill {skillID}: {moveSpeed}");
            return false;
        }
        return true;
    }

    private bool ValidatePassiveStats()
    {
        if (effectDuration < 0)
        {
            Debug.LogError($"Invalid effect duration for skill {skillID}: {effectDuration}");
            return false;
        }
        if (cooldown < 0)
        {
            Debug.LogError($"Invalid cooldown for skill {skillID}: {cooldown}");
            return false;
        }
        if (triggerChance < 0 || triggerChance > 100)
        {
            Debug.LogError($"Invalid trigger chance for skill {skillID}: {triggerChance}");
            return false;
        }
        if (damageIncrease < 0)
        {
            Debug.LogError($"Invalid damage increase for skill {skillID}: {damageIncrease}");
            return false;
        }
        if (defenseIncrease < 0)
        {
            Debug.LogError($"Invalid defense increase for skill {skillID}: {defenseIncrease}");
            return false;
        }
        if (expAreaIncrease < 0)
        {
            Debug.LogError($"Invalid exp area increase for skill {skillID}: {expAreaIncrease}");
            return false;
        }
        if (hpIncrease < 0)
        {
            Debug.LogError($"Invalid HP increase for skill {skillID}: {hpIncrease}");
            return false;
        }
        if (moveSpeedIncrease < 0)
        {
            Debug.LogError($"Invalid move speed increase for skill {skillID}: {moveSpeedIncrease}");
            return false;
        }
        if (attackSpeedIncrease < 0)
        {
            Debug.LogError($"Invalid attack speed increase for skill {skillID}: {attackSpeedIncrease}");
            return false;
        }
        if (attackRangeIncrease < 0)
        {
            Debug.LogError($"Invalid attack range increase for skill {skillID}: {attackRangeIncrease}");
            return false;
        }
        if (hpRegenIncrease < 0)
        {
            Debug.LogError($"Invalid HP regen increase for skill {skillID}: {hpRegenIncrease}");
            return false;
        }

        return true;
    }
}
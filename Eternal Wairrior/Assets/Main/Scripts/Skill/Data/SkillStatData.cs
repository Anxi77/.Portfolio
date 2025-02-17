using UnityEngine;
using System;

[Serializable]
public class SkillStatData
{
    #region Basic Info
    public SkillID SkillID { get; set; }
    public int Level { get; set; }
    public float Damage { get; set; }
    public int MaxSkillLevel { get; set; }
    public ElementType Element { get; set; }
    public float ElementalPower { get; set; }
    #endregion

    #region ProjectileSkill Stats
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
    #endregion

    #region AreaSkill Stats
    public float Radius { get; set; }
    public float Duration { get; set; }
    public float TickRate { get; set; }
    public bool IsPersistent { get; set; }
    public float MoveSpeed { get; set; }
    #endregion

    #region PassiveSkill Stats
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
    #endregion

    #region Getters and Setters
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
    #endregion

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
                throw new ArgumentException($"Invalid skill type: {skillType}");
        }
    }
}
using Unity.VisualScripting;
using System;

public interface ISkillStat
{
    BaseSkillStat baseStat { get; set; }
    SkillType skillType { get; }
}

[Serializable]
public class BaseSkillStat
{
    public float damage;
    public string skillName;
    public int skillLevel;
    public int maxSkillLevel;
    public ElementType element;
    public float elementalPower;

    public BaseSkillStat()
    {
        damage = 10f;
        skillLevel = 1;
        maxSkillLevel = 5;
        element = ElementType.None;
        elementalPower = 1f;
    }

    public BaseSkillStat(BaseSkillStat source)
    {
        this.damage = source.damage;
        this.skillName = source.skillName;
        this.skillLevel = source.skillLevel;
        this.maxSkillLevel = source.maxSkillLevel;
        this.element = source.element;
        this.elementalPower = source.elementalPower;
    }
}

[Serializable]
public class ProjectileSkillStat : ISkillStat
{
    public BaseSkillStat baseStat { get; set; }
    public SkillType skillType => SkillType.Projectile;

    public float projectileSpeed;
    public float projectileScale;
    public float shotInterval;
    public int pierceCount;
    public float attackRange;
    public float homingRange;
    public bool isHoming;
    public float explosionRad;
    public int projectileCount;
    public float innerInterval;

    public ProjectileSkillStat() { }

    public ProjectileSkillStat(ProjectileSkillStat source)
    {
        this.baseStat = new BaseSkillStat(source.baseStat);
        this.projectileSpeed = source.projectileSpeed;
        this.projectileScale = source.projectileScale;
        this.shotInterval = source.shotInterval;
        this.pierceCount = source.pierceCount;
        this.attackRange = source.attackRange;
        this.homingRange = source.homingRange;
        this.isHoming = source.isHoming;
        this.explosionRad = source.explosionRad;
        this.projectileCount = source.projectileCount;
        this.innerInterval = source.innerInterval;
    }
}

[Serializable]
public class AreaSkillStat : ISkillStat
{
    public BaseSkillStat baseStat { get; set; }
    public SkillType skillType => SkillType.Area;

    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;

    public AreaSkillStat() { }

    public AreaSkillStat(AreaSkillStat source)
    {
        this.baseStat = new BaseSkillStat(source.baseStat);
        this.radius = source.radius;
        this.duration = source.duration;
        this.tickRate = source.tickRate;
        this.isPersistent = source.isPersistent;
        this.moveSpeed = source.moveSpeed;
    }
}

[Serializable]
public class PassiveSkillStat : ISkillStat
{
    public BaseSkillStat baseStat { get; set; }
    public SkillType skillType => SkillType.Passive;

    public float effectDuration;
    public float cooldown;
    public float triggerChance;
    public float damageIncrease;
    public float defenseIncrease;
    public float expAreaIncrease;
    public bool homingActivate;
    public float hpIncrease;
    public float moveSpeedIncrease;
    public float attackSpeedIncrease;
    public float attackRangeIncrease;
    public float hpRegenIncrease;
    public bool isPermanent;
    public PassiveSkillStat()
    {
        baseStat = new BaseSkillStat();
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

    public PassiveSkillStat(PassiveSkillStat source)
    {
        this.baseStat = new BaseSkillStat(source.baseStat);
        this.effectDuration = source.effectDuration;
        this.cooldown = source.cooldown;
        this.triggerChance = source.triggerChance;
        this.damageIncrease = source.damageIncrease;
        this.defenseIncrease = source.defenseIncrease;
        this.expAreaIncrease = source.expAreaIncrease;
        this.homingActivate = source.homingActivate;
        this.hpIncrease = source.hpIncrease;
        this.moveSpeedIncrease = source.moveSpeedIncrease;
        this.attackSpeedIncrease = source.attackSpeedIncrease;
        this.attackRangeIncrease = source.attackRangeIncrease;
        this.hpRegenIncrease = source.hpRegenIncrease;
    }
}

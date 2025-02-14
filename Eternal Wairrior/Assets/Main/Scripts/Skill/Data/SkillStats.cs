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
        damage = source.damage;
        skillName = source.skillName;
        skillLevel = source.skillLevel;
        maxSkillLevel = source.maxSkillLevel;
        element = source.element;
        elementalPower = source.elementalPower;
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
        baseStat = new BaseSkillStat(source.baseStat);
        projectileSpeed = source.projectileSpeed;
        projectileScale = source.projectileScale;
        shotInterval = source.shotInterval;
        pierceCount = source.pierceCount;
        attackRange = source.attackRange;
        homingRange = source.homingRange;
        isHoming = source.isHoming;
        explosionRad = source.explosionRad;
        projectileCount = source.projectileCount;
        innerInterval = source.innerInterval;
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
        baseStat = new BaseSkillStat(source.baseStat);
        radius = source.radius;
        duration = source.duration;
        tickRate = source.tickRate;
        isPersistent = source.isPersistent;
        moveSpeed = source.moveSpeed;
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
        baseStat = new BaseSkillStat(source.baseStat);
        effectDuration = source.effectDuration;
        cooldown = source.cooldown;
        triggerChance = source.triggerChance;
        damageIncrease = source.damageIncrease;
        defenseIncrease = source.defenseIncrease;
        expAreaIncrease = source.expAreaIncrease;
        homingActivate = source.homingActivate;
        hpIncrease = source.hpIncrease;
        moveSpeedIncrease = source.moveSpeedIncrease;
        attackSpeedIncrease = source.attackSpeedIncrease;
        attackRangeIncrease = source.attackRangeIncrease;
        hpRegenIncrease = source.hpRegenIncrease;
    }
}

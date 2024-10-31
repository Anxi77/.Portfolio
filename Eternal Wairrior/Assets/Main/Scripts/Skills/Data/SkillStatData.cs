using UnityEngine;

public class SkillStatData
{
    public SkillID skillID;
    public int level;

    // �⺻ ����
    public float damage;
    public int maxSkillLevel;
    public ElementType element;
    public float elementalPower;

    // �߻�ü ��ų ����
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

    // ���� ��ų ����
    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;

    // �нú� ��ų ����
    public float effectDuration;
    public float cooldown;
    public float triggerChance;

    // CSV �����ͷκ��� ���� ��ü ����
    public ISkillStat CreateSkillStat(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Projectile:
                var projStat = new ProjectileSkillStat();
                projStat.baseStat = CreateBaseStats();

                projStat.projectileSpeed = projectileSpeed;
                projStat.projectileScale = projectileScale;
                projStat.shotInterval = shotInterval;
                projStat.pierceCount = pierceCount;
                projStat.attackRange = attackRange;
                projStat.homingRange = homingRange;
                projStat.isHoming = isHoming;
                projStat.explosionRad = explosionRad;
                projStat.projectileCount = projectileCount;
                projStat.innerInterval = innerInterval;

                return projStat;

            case SkillType.Area:
                var areaStat = new AreaSkillStat();
                areaStat.baseStat = CreateBaseStats();

                areaStat.radius = radius;
                areaStat.duration = duration;
                areaStat.tickRate = tickRate;
                areaStat.isPersistent = isPersistent;
                areaStat.moveSpeed = moveSpeed;

                return areaStat;

            case SkillType.Passive:
                var passiveStat = new PassiveSkillStat();
                passiveStat.baseStat = CreateBaseStats();

                passiveStat.effectDuration = effectDuration;
                passiveStat.cooldown = cooldown;
                passiveStat.triggerChance = triggerChance;

                return passiveStat;

            default:
                Debug.LogError($"Invalid skill type: {skillType}");
                return null;
        }
    }

    private BaseSkillStat CreateBaseStats()
    {
        return new BaseSkillStat
        {
            damage = damage,
            maxSkillLevel = maxSkillLevel,
            skillLevel = level,
            element = element,
            elementalPower = elementalPower
        };
    }

    // �⺻������ �ʱ�ȭ�ϴ� ������
    public SkillStatData()
    {
        // �⺻ ���� �ʱ�ȭ
        damage = 10f;
        maxSkillLevel = 5;
        level = 1;
        element = ElementType.None;
        elementalPower = 1f;

        // �߻�ü ��ų ���� �ʱ�ȭ
        projectileSpeed = 10f;
        projectileScale = 1f;
        shotInterval = 0.5f;
        pierceCount = 1;
        attackRange = 10f;
        homingRange = 5f;
        isHoming = false;
        explosionRad = 0f;
        projectileCount = 1;
        innerInterval = 0.1f;

        // ���� ��ų ���� �ʱ�ȭ
        radius = 3f;
        duration = 5f;
        tickRate = 1f;
        isPersistent = false;
        moveSpeed = 0f;

        // �нú� ��ų ���� �ʱ�ȭ
        effectDuration = 5f;
        cooldown = 10f;
        triggerChance = 0.5f;
    }
}
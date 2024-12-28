using UnityEngine;

public class ExplodingShot : ProjectileSkills
{
    protected override void Start()
    {
        base.Start();
        InitializeSkillStats();
    }

    protected override void Fire()
    {
        Projectile proj = PoolManager.Instance.Spawn<Projectile>(
            skillData.projectile,
            transform.position,
            transform.rotation
        );

        if (proj != null)
        { 
            InitializeProjectile(proj);
            proj.SetDirection(fireDir);
            proj.transform.localScale = Vector3.one * ProjectileScale;
        }
    }

    private void InitializeSkillStats()
    {
        if (skillData.GetStatsForLevel(1) == null)
        {
            var stats = new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillName = "Missile Launcher",
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = ElementType.Fire,
                    elementalPower = _elementalPower
                },
                projectileSpeed = _projectileSpeed,
                projectileScale = _projectileScale,
                shotInterval = _shotInterval,
                pierceCount = _pierceCount,
                attackRange = _attackRange,
                homingRange = _homingRange,
                isHoming = _isHoming,
                explosionRad = _explosionRadius,
                projectileCount = _projectileCount,
                innerInterval = _innerInterval
            };
            skillData.SetStatsForLevel(1, stats);
        }
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Fires explosive projectiles that create area damage";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDirect Damage: {Damage:F1}" +
                       $"\nExplosion Radius: {ExplosionRadius:F1}" +
                       $"\nFire Rate: {1 / ShotInterval:F1} shots/s" +
                       $"\nRange: {AttackRange:F1}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Exploding Shot";
    protected override string GetDefaultDescription() => "Fires explosive projectiles that create area damage";
    public override SkillType GetSkillType() => SkillType.Projectile;
}

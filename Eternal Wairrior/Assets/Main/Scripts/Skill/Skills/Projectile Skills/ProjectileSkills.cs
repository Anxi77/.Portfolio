using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileSkills : Skill
{
    [Header("Base Stats")]
    protected float _damage = 10f;
    protected float _elementalPower = 1f;

    [Header("Projectile Stats")]
    protected float _projectileSpeed = 25f;
    protected float _projectileScale = 1f;
    protected float _shotInterval = 0.5f;
    protected int _pierceCount = 1;
    protected float _attackRange = 6f;
    protected float _homingRange = 3.5f;
    protected bool _isHoming = false;
    protected float _explosionRadius = 0f;
    protected int _projectileCount = 1;
    protected float _innerInterval = 0.1f;
    public float Damage => _damage;
    public float ElementalPower => _elementalPower;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileScale => _projectileScale;
    public float ShotInterval => _shotInterval;
    public int PierceCount => _pierceCount;
    public float AttackRange => _attackRange;
    public float HomingRange => _homingRange;
    public bool IsHoming => _isHoming;
    public float ExplosionRadius => _explosionRadius;
    public int ProjectileCount => _projectileCount;
    public float InnerInterval => _innerInterval;
    protected FireMode currentFireMode = FireMode.Auto;
    protected bool canFire = false;
    protected float fireTimer = 0f;
    protected Vector2 fireDir;

    protected override void InitializeSkillData()
    {
        if (skillData == null) return;

        var csvStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.ID,
            currentLevel,
            SkillType.Projectile
        ) as ProjectileSkillStat;

        if (csvStats != null)
        {
            skillData.SetStatsForLevel(skillData.GetCurrentTypeStat().baseStat.skillLevel, csvStats);
        }
        else
        {
            Debug.LogWarning($"No CSV data found for {skillData.Name}, using default values");
            var defaultStats = new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillLevel = currentLevel,
                    maxSkillLevel = 5,
                    element = skillData.Element,
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
            skillData.SetStatsForLevel(currentLevel, defaultStats);
        }
    }

    protected ProjectileSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(currentLevel) as ProjectileSkillStat;
            if (stats == null)
            {
                stats = new ProjectileSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = 1,
                        maxSkillLevel = 5,
                        element = skillData.Element,
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
                skillData?.SetStatsForLevel(1, stats);
            }
            return stats;
        }
    }

    protected override IEnumerator WaitForInitialization()
    {
        yield return base.WaitForInitialization();
        isInitialized = true;
        canFire = true;
    }

    protected virtual void Update()
    {
        if (!isInitialized || !canFire) return;

        CalcDirection();
        UpdateFiring();
    }

    protected virtual void UpdateFiring()
    {
        switch (currentFireMode)
        {
            case FireMode.Manual:
                if (Input.GetMouseButtonDown(0))
                {
                    Fire();
                }
                break;

            case FireMode.Auto:
            case FireMode.AutoHoming:
                fireTimer += Time.deltaTime;
                if (fireTimer >= ShotInterval)
                {
                    if (currentFireMode == FireMode.AutoHoming)
                    {
                        if (AreEnemiesInRange())
                        {
                            FireMultiple();
                        }
                    }
                    else
                    {
                        Fire();
                    }
                    fireTimer = 0f;
                }
                break;
        }
    }

    protected virtual void FireMultiple()
    {
        StartCoroutine(FireMultipleCoroutine());
    }

    protected virtual IEnumerator FireMultipleCoroutine()
    {
        for (int i = 0; i < ProjectileCount; i++)
        {
            if (AreEnemiesInRange())
            {
                Fire();
                yield return new WaitForSeconds(InnerInterval);
            }
            else
            {
                break;
            }
        }
    }

    protected virtual void Fire()
    {
        if (!isInitialized) return;

        Vector3 spawnPosition = transform.position + transform.up * 0.5f;

        var pool = GetComponent<ObjectPool>();
        if (pool != null && skillData?.ProjectilePrefab != null)
        {
            Projectile proj = pool.Spawn<Projectile>(
                skillData.ProjectilePrefab,
                spawnPosition,
                transform.rotation
            );

            if (proj != null)
            {
                InitializeProjectile(proj);
            }
        }
    }

    protected virtual void InitializeProjectile(Projectile proj)
    {
        proj.damage = Damage;
        proj.moveSpeed = ProjectileSpeed;
        proj.isHoming = IsHoming;
        proj.transform.localScale *= ProjectileScale;
        proj.pierceCount = PierceCount;
        proj.maxTravelDistance = AttackRange;
        proj.elementType = skillData.Element;
        proj.elementalPower = ElementalPower;

        proj.SetInitialTarget(FindNearestEnemy());
    }

    #region Enemy Searching Methods

    protected virtual void CalcDirection()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        fireDir = (mouseWorldPos - (Vector2)transform.position).normalized;
        transform.up = fireDir;
    }

    protected virtual bool AreEnemiesInRange()
    {
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (Vector2.Distance(transform.position, enemy.transform.position) <= HomingRange)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual Enemy FindNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= HomingRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
    #endregion

    public virtual void UpdateHomingState(bool activate)
    {
        _isHoming = activate;
        currentFireMode = activate ? FireMode.AutoHoming : FireMode.Auto;

        if (!activate)
        {
            _homingRange = 0f;
        }
        else
        {
            _homingRange = skillData.ProjectileStat.homingRange;
        }

        Debug.Log($"Homing state updated for {skillData.Name}: {activate}");
    }

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is ProjectileSkillStat projectileStats)
        {
            UpdateInspectorValues(projectileStats);
        }
    }

    protected virtual void UpdateInspectorValues(ProjectileSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        Debug.Log($"[ProjectileSkills] Before Update - Level: {currentLevel}");

        currentLevel = stats.baseStat.skillLevel;

        _damage = stats.baseStat.damage;
        _elementalPower = stats.baseStat.elementalPower;
        _projectileSpeed = stats.projectileSpeed;
        _projectileScale = stats.projectileScale;
        _shotInterval = stats.shotInterval;
        _pierceCount = stats.pierceCount;
        _attackRange = stats.attackRange;
        _homingRange = stats.homingRange;
        _isHoming = stats.isHoming;
        _explosionRadius = stats.explosionRad;
        _projectileCount = stats.projectileCount;
        _innerInterval = stats.innerInterval;

        Debug.Log($"[ProjectileSkills] After Update - Level: {currentLevel}");
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = skillData?.Description ?? "Projectile skill description";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nProjectile Speed: {ProjectileSpeed:F1}" +
                       $"\nShot Interval: {ShotInterval:F1}s" +
                       $"\nPierce Count: {PierceCount}" +
                       $"\nAttack Range: {AttackRange:F1}" +
                       $"\nProjectile Count: {ProjectileCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }

            if (ExplosionRadius > 0)
            {
                baseDesc += $"\nExplosion Radius: {ExplosionRadius:F1}";
            }
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
            currentStats.projectileSpeed = _projectileSpeed;
            currentStats.projectileScale = _projectileScale;
            currentStats.shotInterval = _shotInterval;
            currentStats.pierceCount = _pierceCount;
            currentStats.attackRange = _attackRange;
            currentStats.homingRange = _homingRange;
            currentStats.isHoming = _isHoming;
            currentStats.explosionRad = _explosionRadius;
            currentStats.projectileCount = _projectileCount;
            currentStats.innerInterval = _innerInterval;

            _damage = currentStats.baseStat.damage;
            currentLevel = currentStats.baseStat.skillLevel;
            _elementalPower = currentStats.baseStat.elementalPower;
            _projectileSpeed = currentStats.projectileSpeed;
            _projectileScale = currentStats.projectileScale;
            _shotInterval = currentStats.shotInterval;
            _pierceCount = currentStats.pierceCount;
            _attackRange = currentStats.attackRange;
            _homingRange = currentStats.homingRange;
            _isHoming = currentStats.isHoming;
            _explosionRadius = currentStats.explosionRad;
            _projectileCount = currentStats.projectileCount;
            _innerInterval = currentStats.innerInterval;

            skillData.SetStatsForLevel(currentLevel, currentStats);
            Debug.Log($"Updated stats for {GetType().Name} from inspector");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in OnValidate for {GetType().Name}: {e.Message}");
        }
    }

    private void OnDisable()
    {
        canFire = false;
        isInitialized = false;
        StopAllCoroutines();
    }

    public void ModifyProjectileSpeed(float multiplier)
    {
        _projectileSpeed *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as ProjectileSkillStat;
        if (currentStats != null)
        {
            currentStats.projectileSpeed = _projectileSpeed;
        }
    }

    public void ModifyProjectileRange(float multiplier)
    {
        _attackRange *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as ProjectileSkillStat;
        if (currentStats != null)
        {
            currentStats.attackRange = _attackRange;
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
        _shotInterval *= multiplier;
        var currentStats = skillData?.GetCurrentTypeStat() as ProjectileSkillStat;
        if (currentStats != null)
        {
            currentStats.shotInterval = _shotInterval;
        }
    }
}

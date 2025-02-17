using UnityEngine;

public class GravityProjectileSkill : ProjectileSkills
{
    [Header("Gravity Projectile Settings")]
    [SerializeField] private float _gravityForce = 10f;
    [SerializeField] private float _gravityDamageInterval = 0.5f;
    [SerializeField] private float _startSize = 1f;
    [SerializeField] private float _endSize = 3f;
    [SerializeField] private float _growthDuration = 2f;
    [SerializeField] private GravityProjectile gravityProjectilePrefab;
    [SerializeField] private KeyCode homingTriggerKey = KeyCode.LeftShift;

    public override void Initialize()
    {
        base.Initialize();
        currentFireMode = FireMode.Auto;
    }

    protected override void Update()
    {
        if (!isInitialized || !canFire) return;

        CalcDirection();

        if (Input.GetKeyDown(homingTriggerKey))
        {
            UpdateHomingState(!IsHoming);
        }

        UpdateFiring();
    }

    protected override void Fire()
    {
        if (!isInitialized || skillData?.ProjectilePrefab == null)
        {
            Debug.LogWarning("Cannot fire: not initialized or missing projectile");
            return;
        }

        Vector3 spawnPosition = transform.position + transform.up * 0.5f;

        var proj = PoolManager.Instance.Spawn<GravityProjectile>(
            skillData.ProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (proj != null)
        {
            InitializeProjectile(proj);
            SetProjectileDirection(proj);
            Debug.Log("Gravity projectile fired");
        }
    }

    private void SetProjectileDirection(GravityProjectile proj)
    {
        if (IsHoming)
        {
            Enemy target = FindNearestEnemy();
            if (target != null)
            {
                proj.SetTarget(target);
                proj.SetHoming(true);
                Vector2 dirToTarget = (target.transform.position - proj.transform.position).normalized;
                proj.SetDirection(dirToTarget);
            }
        }
        else
        {
            proj.SetDirection(fireDir);
        }
    }

    protected override Enemy FindNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistance = AttackRange;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0) return null;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    protected override void InitializeProjectile(Projectile proj)
    {
        base.InitializeProjectile(proj);

        if (proj is GravityProjectile gravityProj)
        {
            gravityProj.SetSizeParameters(_startSize, _endSize, _growthDuration);
            gravityProj.SetMaxTravelDistance(AttackRange);
            gravityProj.SetProjectileSpeed(ProjectileSpeed);
        }
    }

    protected override void UpdateInspectorValues(ProjectileSkillStat stats)
    {
        base.UpdateInspectorValues(stats);

        float levelMultiplier = 1f + (currentLevel - 1) * 0.2f;
        _gravityForce *= levelMultiplier;
        _endSize *= levelMultiplier;
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Creates gravity wells that pull and damage enemies";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage per Tick: {Damage:F1}" +
                       $"\nGravity Force: {_gravityForce:F1}" +
                       $"\nDamage Interval: {_gravityDamageInterval:F1}s" +
                       $"\nInitial Size: {_startSize:F1}" +
                       $"\nFinal Size: {_endSize:F1}" +
                       $"\nGrowth Duration: {_growthDuration:F1}s" +
                       $"\nRange: {AttackRange:F1}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }

            baseDesc += $"\n\nPress {homingTriggerKey} to toggle homing mode";
        }
        return baseDesc;
    }

    public override void UpdateHomingState(bool activate)
    {
        _isHoming = activate;
        currentFireMode = activate ? FireMode.AutoHoming : FireMode.Auto;

        if (!activate)
        {
            _homingRange = 0f;
        }
        else
        {
            _homingRange = TypedStats.homingRange;
        }
    }
}
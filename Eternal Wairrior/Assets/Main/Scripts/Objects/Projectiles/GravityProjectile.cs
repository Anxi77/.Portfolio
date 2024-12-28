using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityProjectile : Projectile
{
    [Header("Gravity Settings")]
    [SerializeField] private float _gravityForce =  5f;
    [SerializeField] private float _gravityDamageInterval = 0.5f;
    [SerializeField] private float _startSize = 1f;
    [SerializeField] private float _endSize = 3f;
    [SerializeField] private float _growthDuration = 2f;
    [SerializeField] private float _shrinkDuration = 0.5f;
    [SerializeField] private float _finalDamageMultiplier = 2f;
    [SerializeField] private float _finalDamageRadius = 3f;

    private List<Enemy> affectedEnemies = new List<Enemy>();
    private float damageTimer = 0f;
    private float currentSize;
    private float growthTimer = 0f;
    private CircleCollider2D circleCollider;
    private bool isShrinking = false;
    private float shrinkTimer = 0f;
    private new float maxTravelDistance;
    private Vector3 startPosition;
    private bool hasReachedDestination = false;
    private Vector3 projectileDirection;
    private float projectileSpeed = 10f;
    [SerializeField] private float homingStrength = 5f;

    private bool hasStartedGrowth = false;

    protected override void Awake()
    {
        base.Awake();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        ResetProjectile();
    }

    protected override void Update()
    {
        if (!hasReachedDestination)
        {
            MoveAndCheckTarget();
        }
        else if (!hasStartedGrowth)
        {
            hasStartedGrowth = true;
            StartGrowthPhase();
        }
        else
        {
            UpdateGrowthAndEffects();
        }
    }

    private void MoveAndCheckTarget()
    {
        if (isHoming && targetEnemy != null && targetEnemy.gameObject.activeSelf)
        {
            Vector2 directionToTarget = (targetEnemy.transform.position - transform.position).normalized;
            projectileDirection = Vector3.Lerp(projectileDirection, directionToTarget, Time.deltaTime * homingStrength);

            float distanceToTarget = Vector2.Distance(transform.position, targetEnemy.transform.position);
            if (distanceToTarget <= 0.5f)
            {
                hasReachedDestination = true;
                return;
            }
        }

        transform.position += projectileDirection * projectileSpeed * Time.deltaTime;
    }

    private void StartGrowthPhase()
    {
        currentSize = _startSize;
        growthTimer = 0f;
        UpdateSize(currentSize);
    }

    private void UpdateGrowthAndEffects()
    {
        UpdateGrowth();
        ApplyGravityEffect();
        ApplyDamageOverTime();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Enemy>(out _))
        {
            hasReachedDestination = true;
        }
    }

    private void UpdateGrowth()
    {
        if (!isShrinking && growthTimer < _growthDuration)
        {
            growthTimer += Time.deltaTime;
            float t = growthTimer / _growthDuration;
            currentSize = Mathf.Lerp(_startSize, _endSize, t);
            UpdateSize(currentSize);
        }
        else if (!isShrinking)
        {
            isShrinking = true;
            shrinkTimer = 0f;
        }
        else
        {
            shrinkTimer += Time.deltaTime;
            float t = shrinkTimer / _shrinkDuration;
            currentSize = Mathf.Lerp(_endSize, 0, t);
            UpdateSize(currentSize);

            if (shrinkTimer >= _shrinkDuration)
            {
                ApplyFinalDamage();
                var imparticle = PoolManager.Instance.Spawn<ParticleSystem>(impactParticle.gameObject, transform.position, Quaternion.identity);
                imparticle.Play();
                PoolManager.Instance.Despawn<GravityProjectile>(this);
                PoolManager.Instance.Despawn<ParticleSystem>(imparticle, 1.5f);
            }
        }
    }

    private void UpdateSize(float size)
    {
        transform.localScale = Vector3.one * size;
        if (circleCollider != null)
        {
            circleCollider.radius = 0.5f;
        }
    }

    private void ApplyGravityEffect()
    {
        float currentRadius = currentSize / 2f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                if (!affectedEnemies.Contains(enemy))
                {
                    affectedEnemies.Add(enemy);
                }

                Vector2 directionToProjectile = (Vector2)(transform.position - enemy.transform.position);
                float distance = directionToProjectile.magnitude;

                float gravityMultiplier = 1f - (distance / currentRadius);
                gravityMultiplier = Mathf.Clamp01(gravityMultiplier);

                Vector2 force = directionToProjectile.normalized * _gravityForce * gravityMultiplier;
                enemy.transform.position += (Vector3)(force * Time.deltaTime);
            }
        }

        affectedEnemies.RemoveAll(enemy =>
            Vector2.Distance(enemy.transform.position, transform.position) > currentRadius ||
            enemy == null ||
            !enemy.gameObject.activeSelf);
    }

    private void ApplyDamageOverTime()
    {
        damageTimer += Time.deltaTime;

        if (damageTimer >= _gravityDamageInterval)
        {
            foreach (Enemy enemy in affectedEnemies)
            {
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    enemy.TakeDamage(damage / 10);
                }
            }
            damageTimer = 0f;
        }
    }

    private void ApplyFinalDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _finalDamageRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.TakeDamage(damage * _finalDamageMultiplier);
            }
        }
    }

    public override void ResetProjectile()
    {
        base.ResetProjectile();
        hasReachedDestination = false;
        hasStartedGrowth = false;
        affectedEnemies.Clear();
        damageTimer = 0f;
        growthTimer = 0f;
        shrinkTimer = 0f;
        isShrinking = false;
        currentSize = _startSize;
        UpdateSize(_startSize);
        startPosition = transform.position;
        projectileDirection = Vector2.zero;
    }

    public void SetSizeParameters(float startSize, float endSize, float duration)
    {
        _startSize = startSize;
        _endSize = endSize;
        _growthDuration = duration;
    }

    public void SetMaxTravelDistance(float distance)
    {
        maxTravelDistance = distance;
    }

    public override void SetDirection(Vector2 dir)
    {
        projectileDirection = dir.normalized;
    }

    public void SetProjectileSpeed(float speed)
    {
        projectileSpeed = speed;
    }

    public void SetTarget(Enemy enemy)
    {
        targetEnemy = enemy;
    }

    public void SetHoming(bool homing)
    {
        isHoming = homing;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentSize / 2f);

        if (isShrinking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _finalDamageRadius);
        }
    }

    public override void CheckTravelDistance()
    {
        if (!hasReachedDestination)
        {
            float distanceTraveled = Vector2.Distance(transform.position, startPosition);
            if (distanceTraveled >= maxTravelDistance)
            {
                hasReachedDestination = true;
            }
        }
    }
}

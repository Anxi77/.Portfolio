using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingProjectile : Projectile
{
    [Header("Explosion Settings")]
    [SerializeField] protected float _explosionRadius = 2f;

    public float explosionRad { get => _explosionRadius; set => _explosionRadius = value; }

    private ParticleSystem projectileParticle;

    protected override void Awake()
    {
        base.Awake();
        projectileParticle = GetComponentInChildren<ParticleSystem>();
    }

    public override void Move()
    {
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    public override void SetDirection(Vector2 newDirection)
    {
        base.SetDirection(newDirection);
        if (projectileParticle != null)
        {
            projectileParticle.gameObject.transform.up = direction;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        StartCoroutine(ExplodeCoroutine());
    }

    private IEnumerator ExplodeCoroutine()
    {
        moveSpeed = 0;
        projectileParticle.gameObject.SetActive(false);

        ParticleSystem impactInstance = PoolManager.Instance.Spawn<ParticleSystem>(
            impactParticle.gameObject,
            transform.position,
            transform.rotation
        );

        if (impactInstance != null)
        {
            impactInstance.Play();
            float explosionRadius = GetParticleSystemRadius(impactInstance);

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    enemy.TakeDamage(_damage);

                    if (_elementType != ElementType.None && _elementalPower > 0)
                    {
                        ElementalEffects.ApplyElementalEffect(_elementType, _elementalPower, hitCollider.gameObject);
                    }
                }
            }

            yield return new WaitForSeconds(impactInstance.main.duration);
            PoolManager.Instance.Despawn(impactInstance);
        }

        PoolManager.Instance.Despawn(this);
    }

    private float GetParticleSystemRadius(ParticleSystem particleSystem)
    {
        var main = particleSystem.main;
        var startSize = main.startSize;

        if (startSize.mode == ParticleSystemCurveMode.Constant)
        {
            return startSize.constant / 2f;
        }
        else if (startSize.mode == ParticleSystemCurveMode.TwoConstants)
        {
            return Mathf.Max(startSize.constantMin, startSize.constantMax) / 2f;
        }
        else
        {
            return (startSize.constantMin + startSize.constantMax) / 4f;
        }
    }

    public override void ResetProjectile()
    {
        base.ResetProjectile();
        _explosionRadius = 2f;
    }
}

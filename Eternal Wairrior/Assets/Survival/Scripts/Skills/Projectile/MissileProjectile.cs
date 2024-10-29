using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileProjectile : Projectile
{
    public float explosionRad;
    private ParticleSystem projectileParticle;

    protected override void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        projectileParticle = GetComponentInChildren<ParticleSystem>();
        coll.enabled = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (coll != null)
        {
            coll.radius = 0.01f;
        }
    }

    protected override void Update()
    {
        base.Update();       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(ExplodeCoroutine());
    }

    private IEnumerator ExplodeCoroutine()
    {
        moveSpeed = 0;
        projectileParticle.Stop();

        // ����Ʈ ��ƼŬ ���� �� ���
        ParticleSystem impactInstance = LeanPool.Spawn(impactParticle, transform.position, transform.rotation);
        impactInstance.Play();

        // ��ƼŬ �ý����� ���� ũ�⸦ ������
        float explosionRadius = GetParticleSystemRadius(impactInstance);

        // ���� �ݰ� ���� �ݶ��̴� ���� �� ������ ����
        Collider2D[] contactedColls = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        Explode(contactedColls);

        // ��ƼŬ ���� �ð���ŭ ���
        yield return new WaitForSeconds(impactInstance.main.duration);

        // ����Ʈ ��ƼŬ �ν��Ͻ� ����
        LeanPool.Despawn(impactInstance);

        // �̻��� ������Ÿ�� ����
        LeanPool.Despawn(gameObject);
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
            // �ٸ� ����� ��� ��հ� ���
            return (startSize.constantMin + startSize.constantMax) / 4f;
        }
    }

    private void Explode(Collider2D[] contactedColls)
    {
        foreach (Collider2D contactedColl in contactedColls)
        {
            if (contactedColl.CompareTag("Enemy"))
            {
                Enemy enemy = contactedColl.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"���� ���� �� �� ����: {contactedColl.name}");
                }
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayAreaEffect : AreaEffect
{
    [Header("메인공격 유무")]
    [SerializeField] private bool isMainDamage = true;
    [Header("메인공격 딜레이")]
    [SerializeField] private float mainDamageDelay = 0f;
    [Header("메인공격 크기 배율")]
    [SerializeField, Range(1, 10)] private float MainDamageScale = 1f;
    [Header("메인공격 데미지 배율")]
    [SerializeField,Range(1, 10)]private float MainDamageMultiplier = 1f;
    [Header("여진피해 유무")]
    [SerializeField] private bool isAfterShock = true;
    [Header("여진피해 크기 배율")]
    [SerializeField,Range(1,10)] private float afterShockScale = 1f;
    [Header("여진피해 딜레이")]
    [SerializeField, Range(0,10)] private float afterShockDelay;
    [Header("여진피해 데미지 배율")]
    [SerializeField, Range(0, 1)] private float afterShockMultiplier = 1f;
    [Header("파티클의 duration을 직접 수정해야할때 담으시오")]
    [SerializeField] private ParticleSystem particle = null;

    public override void Initialize(AreaEffectData Data)
    {
        base.Initialize(Data);
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particle.main;
            main.duration = areaData.duration + mainDamageDelay;
            particle.Play();
        }
    }

    private void Update()
    {
        durationTime += Time.deltaTime;

        if (durationTime >= areaData.duration + mainDamageDelay)
        {
            StopCoroutine(attackCoroutine);
            PoolManager.Instance.Despawn<AreaEffect>(this);  
        }
    }

    public override IEnumerator AttackRoutine()
    {
        bool isOwnerPlayer = areaData.owner is Player;

        yield return new WaitForSeconds(mainDamageDelay);
        if (isMainDamage == true)
        {
            MainDamage(isOwnerPlayer);
        }
        while (true)
        {
            if (isAfterShock)
            {
                yield return new WaitForSeconds(afterShockDelay);

                if (areaData.duration + mainDamageDelay <= durationTime)
                {
                    PoolManager.Instance.Despawn<AreaEffect>(this);
                    yield break;
                }

                Collider[] colliders = Physics.OverlapSphere(transform.position,( areaData.areaScale * afterShockScale)/2f);
                foreach (Collider collider in colliders)
                {
                    if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                        continue;

                    if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
                    {
                        targetUnit.TakeDamage(areaData.damage * afterShockMultiplier);
                    }
                }
            }
        }
    }

    private void MainDamage(bool isOwnerPlayer)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, (areaData.areaScale * MainDamageScale)/2f);
        foreach (Collider collider in colliders)
        {
            if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                continue;

            if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
            {
                targetUnit.TakeDamage(areaData.damage* MainDamageMultiplier);
            }
        }
    }


}

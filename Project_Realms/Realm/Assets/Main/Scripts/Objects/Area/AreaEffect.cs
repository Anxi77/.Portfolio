using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{

    protected AreaEffectData areaData;
    protected float durationTime;
    protected Coroutine attackCoroutine;

    public virtual void Initialize(AreaEffectData Data)
    {
        this.areaData = Data;
        transform.localScale = Vector3.one * areaData.areaScale;
        durationTime = 0;
        StartAttack();
    }

    private void StartAttack()
    {
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private void Update()
    {
        durationTime += Time.deltaTime;

        if (durationTime >= areaData.duration)
        {
            StopCoroutine(attackCoroutine);
            PoolManager.Instance.Despawn<AreaEffect>(this);
        }
    }

    public virtual IEnumerator AttackRoutine()
    {
        bool isOwnerPlayer = areaData.owner is Player;

        while (true)
        {
            if (areaData.duration < durationTime)
            {
                PoolManager.Instance.Despawn<AreaEffect>(this);
                yield break;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, areaData.areaScale);
            foreach (Collider collider in colliders)
            {
                if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                    continue;

                if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
                {
                    targetUnit.TakeDamage(areaData.damage);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}

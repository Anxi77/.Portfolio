using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeAttack : DefaultSkill
{
    private bool isOwnerPlayer;

    protected override void UseSkill()
    {
        if (isSkillInProgress == true) return;
        isOwnerPlayer = Owner is Player;
        Unit target = Owner.Target;
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            float attackRange = GetAttackRange();

            if (distanceToTarget <= attackRange && !isAttackInProgress)
            {
                Owner.StopMoving();
                Owner.transform.LookAt(target.transform);

                float attackSpeed = GetAttackSpeed();
                if (isOwnerPlayer) 
                {
                    Owner.Animator.SetFloat("AttackSpeed", attackSpeed);
                }
                Owner.Animator.SetTrigger("Attack");

                StartCoroutine(MeleeAttackRoutine());
            }
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        isSkillInProgress = true;

        while (!Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return null;
        }

        float damagePoint = 0.4f;
        while (Owner.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < damagePoint)
        {
            yield return null;
        }

        PerformSectorAttack();

        while (Owner.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.97f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(GetAttackDelay());

        OnAttackComplete();
    }

    private void PerformSectorAttack()
    {
        Vector3 playerPosition = transform.position;
        Vector3 forward = Owner.transform.forward;
        float attackRange = GetAttackRange();

        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, attackRange, targetLayer);

        foreach (Collider collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - playerPosition).normalized;
            float angle = Vector3.Angle(forward, directionToTarget);

            if (angle <= attackAngle / 2)
            {
                if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                    return;

                if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
                {
                    targetUnit.TakeDamage(GetDamage());
                }
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Owner == null) return;

        Vector3 position = transform.position;
        Vector3 forward = transform.forward;
        float range = GetAttackRange();

        Gizmos.color = Color.red;

        int segments = 20;
        float angleStep = attackAngle / segments;
        Vector3 previousPoint = position + Quaternion.Euler(0, -attackAngle / 2, 0) * forward * range;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (-attackAngle / 2) + (angleStep * i);
            Vector3 currentPoint = position + Quaternion.Euler(0, currentAngle, 0) * forward * range;

            Gizmos.DrawLine(position, currentPoint);
            if (i > 0)
                Gizmos.DrawLine(previousPoint, currentPoint);

            previousPoint = currentPoint;
        }
    }
#endif
}
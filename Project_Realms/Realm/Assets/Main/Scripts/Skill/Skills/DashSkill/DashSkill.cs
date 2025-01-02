using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class DashSkill : Skill
{

    private Collider ownerCollider;
    private Quaternion ownerQuaternion;
    private Vector3 ownerDashdistance;

    public override void Initialize(Unit owner)
    {
        base.Initialize(owner);
        ownerCollider = owner.GetComponentInParent<Collider>();
        if (ownerCollider == null)
        {
            Debug.LogError($"{gameObject.name} 대쉬대상 콜라이더없음!");
        }
        skillStat.InitializeStats();
    }

    public override bool TryUseSkill()
    {
        if (Owner.IsDashing == true) return false;
        if (GetTargetDirection() == false) return false;
        return base.TryUseSkill();
    }

    protected override void UseSkill()
    {
        StartCoroutine(DashCoroutine());
    }



    private IEnumerator DashCoroutine()
    {
        Owner.IsDashing = true;
        ownerCollider.enabled = false;

        Owner.Animator.SetTrigger("Attack");
        float dashDistance = skillStat.GetStatValue<float>(SkillStatType.DashDistance);
        Vector3 startPosition = Owner.transform.position;
        Vector3 targetPosition = startPosition + ownerDashdistance;
        float dashTime = 0f;
        float dashDuration = 0.2f;


        Owner.transform.rotation = ownerQuaternion;



        while (dashTime < dashDuration)
        {
            dashTime += Time.deltaTime;
            float t = dashTime / dashDuration;

            Owner.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Owner.transform.position = targetPosition;
        Owner.Agent.Warp(targetPosition);

        yield return new WaitForSeconds(0.3f);
        Owner.Animator.SetTrigger("Idle");



        ownerCollider.enabled = true;
        Owner.IsDashing = false;



    }


    private bool GetTargetDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - Owner.transform.position).normalized;
            direction.y = 0;

            direction = direction * 10f;

            float maxDashDistance = Mathf.Max(skillStat.GetStatValue<float>(SkillStatType.DashDistance));

            if (direction.magnitude > maxDashDistance)
            {
                direction = direction.normalized * maxDashDistance;
            }

            if (Physics.Raycast(Owner.transform.position, direction.normalized, out RaycastHit hit, direction.magnitude))
            {
                direction = direction.normalized * hit.distance;
            }

            ownerDashdistance = direction;
            ownerQuaternion = Quaternion.LookRotation(direction);
            return true;
        }
        return false;
    }

}

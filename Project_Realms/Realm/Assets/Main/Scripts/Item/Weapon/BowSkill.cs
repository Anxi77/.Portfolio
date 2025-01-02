using System.Collections;
using UnityEngine;

public class BowSkill : DefaultSkill
{
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Projectile arrowPrefab;
    private float lastFireTime;

    [Header("Bow Animation")]
    [SerializeField] private float drawDuration = 0.3f; 
    [SerializeField] private float resetDuration = 0.2f;

    private bool isDrawing = false;
    private Coroutine drawRoutine;
    private Vector3? targetDirection;  

    public override void Initialize(Unit owner)
    {
        base.Initialize(owner);
        if (owner is Player)
        {
            UpdateArrowSpawnPoint();
        }
        else
        {
            UpdateFirePoint();
        }
    }

    private void UpdateFirePoint()
    {
        if (Owner is Pet)
        {
            arrowSpawnPoint = Owner.transform.Find("FirePoint");
            if (arrowSpawnPoint == null)
            {
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(Owner.transform);
                firePoint.transform.localPosition = new Vector3(0, 1f, 0.5f);  
                arrowSpawnPoint = firePoint.transform;
            }
        }
    }


    private void UpdateArrowSpawnPoint()
    {
        if (weaponHolder?.CurrentWeaponObject != null)
        {
            Transform weaponSpawnPoint = weaponHolder.CurrentWeaponObject.transform.Find("ArrowSpawnPoint");
            if (weaponSpawnPoint != null)
            {
                arrowSpawnPoint = weaponSpawnPoint;
            }
            else
            {
                Debug.LogWarning("ArrowSpawnPoint not found on weapon prefab");
            }
        }
    }

    protected override void UseSkill()
    {
        if (Owner is Player)
        {
            PlayerFire();
        }
        else if (Owner is Pet)
        {
            PetFire();
        }
        else
        {
            Fire();
        }
    }

    private void UnitFire()
    {
        if (Time.time - lastFireTime >= 1f / GetAttackSpeed())
        {
            Fire();
        }
    }

    private void Fire()
    {
        Owner.Animator.SetTrigger("Attack");
        FireArrow();
        lastFireTime = Time.time;
        Owner.Animator.SetTrigger("Idle");
    }

    private void PlayerFire()
    {
        if (Time.time - lastFireTime >= 1f / GetAttackSpeed() && !isDrawing)
        {
            if (drawRoutine != null)
                StopCoroutine(drawRoutine);
            drawRoutine = StartCoroutine(DrawAndFireRoutine());
        }
    }

    private IEnumerator DrawAndFireRoutine()
    {
        isSkillInProgress = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            targetDirection = (targetPoint - transform.position).normalized;
            targetDirection = new Vector3(targetDirection.Value.x, 0, targetDirection.Value.z);
            Owner.transform.rotation = Quaternion.LookRotation(targetDirection.Value);

            if (arrowSpawnPoint != null)
            {
                arrowSpawnPoint.rotation = Owner.transform.rotation;
            }
        }

        if (weaponHolder?.CurrentIKSetup?.offHandIK != null)
        {
            Owner.Animator.SetTrigger("Attack");
            float elapsedTime = 0f;
            float startWeight = weaponHolder.CurrentIKSetup.offHandIK.weight;

            while (elapsedTime < drawDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / drawDuration;
                weaponHolder.CurrentIKSetup.offHandIK.weight = Mathf.Lerp(startWeight, 0.3f, t);
                yield return null;
            }

            FireArrow();
            lastFireTime = Time.time;

            elapsedTime = 0f;
            startWeight = weaponHolder.CurrentIKSetup.offHandIK.weight;

            while (elapsedTime < resetDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / resetDuration;
                weaponHolder.CurrentIKSetup.offHandIK.weight = Mathf.Lerp(startWeight, 0f, t);
                yield return null;
            }
        }
        else
        {
            FireArrow();
            lastFireTime = Time.time;
        }

        isSkillInProgress = false;

        if (Owner?.Animator != null)
        {
            Owner.Animator.SetTrigger("Idle");
        }

        yield break;
    }

    private void FireArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null && targetDirection.HasValue)
        {
            Vector3 targetPoint = transform.position + targetDirection.Value * GetAttackRange();

            Projectile arrow = PoolManager.Instance.Spawn<Projectile>(
                arrowPrefab.gameObject,
                arrowSpawnPoint.position,

                Quaternion.LookRotation(targetDirection.Value));

            if (arrow != null)
            {
                float totalDamage = GetDamage();

                ProjectileData data = new ProjectileData
                {
                    owner = Owner,
                    Damage = totalDamage,
                    Speed = 15f,
                    Range = 15f,
                    PierceCount = -1
                };

                arrow.Initialize(data);
            }
        }

        // 발사 후 방향 초기화
        targetDirection = null;
    }

    private void PetFire()
    {
        if (Owner is Pet pet && arrowSpawnPoint != null && pet.enemyTarget != null)
        {
            Owner.Animator.SetTrigger("Attack");

            Vector3 directionToTarget = (pet.enemyTarget.position - transform.position).normalized;
            directionToTarget.y = 0;

            Owner.transform.rotation = Quaternion.LookRotation(directionToTarget);
            arrowSpawnPoint.rotation = Owner.transform.rotation;  // arrowSpawnPoint 회전 추가

            Projectile arrow = PoolManager.Instance.Spawn<Projectile>(
                arrowPrefab.gameObject,
                arrowSpawnPoint.position,
                Quaternion.LookRotation(directionToTarget));

            if (arrow != null)
            {
                float totalDamage = GetDamage();

                ProjectileData data = new ProjectileData
                {
                    owner = Owner,
                    Damage = totalDamage,
                    Speed = 15f,
                    Range = 15f,
                    IsHoming = true,
                    HomingRange = 10f,
                    Scale = 0.5f,
                    PierceCount = 0
                };

                arrow.Initialize(data);
            }

            lastFireTime = Time.time;
            Owner.Animator.SetTrigger("Idle");
        }
    }

}
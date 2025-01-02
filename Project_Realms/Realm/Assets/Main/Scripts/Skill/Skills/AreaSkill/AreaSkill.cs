using System.Collections;
using UnityEngine;

public class AreaSkill : Skill
{
    [SerializeField] private AreaEffect areaPrefab;
    private Vector3 spawnPoint;
    private AreaSkillStat areaSkillStat;

    public override void Initialize(Unit owner)
    {
        base.Initialize(owner);
        areaSkillStat = (AreaSkillStat)skillStat;
        areaSkillStat.InitializeStats();
    }


    protected override void UseSkill()
    {
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        int spawnCount = Mathf.RoundToInt(areaSkillStat.GetStatValue<float>(SkillStatType.SpawnCount));
        float spawnInterval = areaSkillStat.GetStatValue<float>(SkillStatType.SpawnInterval);

        isSkillInProgress = true;

        for (int i = 0; i < spawnCount; i++)
        {
            Owner.Animator.SetTrigger("Attack");
            AreaEffectData areaData = new AreaEffectData()
            {
                owner = Owner,
                damage = areaSkillStat.GetStatValue<float>(SkillStatType.Damage),
                areaScale = areaSkillStat.GetStatValue<float>(SkillStatType.SpawnScale),
                duration = areaSkillStat.GetStatValue<float>(SkillStatType.Duration)
            };

            SpawnArea(areaData);

            if (spawnInterval > 0 && i < spawnCount - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        Owner.Animator.SetTrigger("Idle");

        yield return new WaitForSeconds(0.3f);
        isSkillInProgress = false;
    }

    private void SetSpawnPoint()
    {
        if (areaSkillStat.GetStatValue<int>(SkillStatType.IsSpawnAtCursor) == 1)
        {
            SpawnAtMouseCursor();
        }
        else if (areaSkillStat.GetStatValue<int>(SkillStatType.IsSpawnAtEnemy) == 1)
        {
            SpawnAtTargetPosition();
        }
        else
        {
            SpawnAtMouseCursor();
        }
    }

    private void SpawnAtMouseCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);

            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;

            Owner.transform.rotation = Quaternion.LookRotation(direction);

            if (Vector3.Distance(transform.position, targetPoint) > areaSkillStat.GetStatValue<float>(SkillStatType.SpawnRange))
            {
                spawnPoint = transform.position + direction * areaSkillStat.GetStatValue<float>(SkillStatType.SpawnRange);
            }
            else
            {
                spawnPoint = targetPoint;
            }
        }
    }

    private void SpawnAtTargetPosition()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, areaSkillStat.GetStatValue<float>(SkillStatType.SpawnRange));
        Transform nearestTarget = null;
        float nearestDistance = float.MaxValue;

        bool isOwnerPlayer = Owner is Player;

        foreach (Collider collider in colliders)
        {
            if (!collider.TryGetComponent<Unit>(out Unit targetUnit))
                continue;

            if ((isOwnerPlayer && targetUnit is Monster) || (!isOwnerPlayer && targetUnit is Player))
            {
                float distance = Vector3.Distance(transform.position, targetUnit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = targetUnit.transform;
                }
            }
        }

        if (nearestTarget != null)
        {
            Vector3 direction = (nearestTarget.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
            spawnPoint = nearestTarget.position;
        }
        else
        {
            if (isOwnerPlayer)
            {
                SpawnAtMouseCursor();
            }
            else
            {
                SpawnAtRandomPosition();
            }
        }
    }

    private void SpawnAtRandomPosition()
    {
        float spawnRange = areaSkillStat.GetStatValue<float>(SkillStatType.SpawnRange);
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(0f, spawnRange);

        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        spawnPoint = transform.position + randomDirection * randomDistance;

        transform.rotation = Quaternion.LookRotation(randomDirection);
    }


    private void SpawnArea(AreaEffectData data)
    {
        SetSpawnPoint();
        AreaEffect area = PoolManager.Instance.Spawn<AreaEffect>
            (areaPrefab.gameObject, spawnPoint, Quaternion.identity);
        area.Initialize(data);
    }


    public override void LevelUp()
    {
        base.LevelUp();
    }
}

public struct AreaEffectData
{
    public Unit owner;
    public float damage;
    public float duration;
    public float areaScale;
}

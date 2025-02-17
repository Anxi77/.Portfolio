using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlowFieldSkill : AreaSkills
{
    [Header("Spawn Settings")]
    [SerializeField] private float cooldown = 8f;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int baseFieldCount = 1;
    [SerializeField] private int additionalFieldPerLevel = 1;
    [SerializeField] private GameObject slowFieldPrefab;
    private List<SlowField> activeFields = new List<SlowField>();

    public override void Initialize()
    {
        base.Initialize();
        if (slowFieldPrefab == null)
        {
            Debug.LogError("SlowFieldPrefab is not assigned!");
            return;
        }
        StartCoroutine(AutoSpawnCoroutine());
    }

    private List<Vector3> GetRandomPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        int totalFields = baseFieldCount + (additionalFieldPerLevel * (skillData.GetCurrentTypeStat().baseStat.skillLevel - 1));
        int attempts = 0;
        const int MAX_ATTEMPTS = 100;
        float minDistanceBetweenFields = Radius * 2;

        while (positions.Count < totalFields && attempts < MAX_ATTEMPTS)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 newPosition = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);

            bool isTooClose = false;
            foreach (Vector3 existingPos in positions)
            {
                if (Vector3.Distance(newPosition, existingPos) < minDistanceBetweenFields)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (!isTooClose)
            {
                positions.Add(newPosition);
            }

            attempts++;
        }

        return positions;
    }

    private IEnumerator AutoSpawnCoroutine()
    {
        while (true)
        {
            SpawnFields();
            yield return new WaitForSeconds(Duration);
            DestroyFields();
            yield return new WaitForSeconds(cooldown - Duration);
        }
    }

    private void SpawnFields()
    {
        List<Vector3> positions = GetRandomPositions();
        foreach (Vector3 pos in positions)
        {
            GameObject fieldObj = Instantiate(slowFieldPrefab, pos, Quaternion.identity);
            SlowField field = fieldObj.GetComponent<SlowField>();
            if (field != null)
            {
                field.Initialize(Damage, Radius, Duration, TickRate);
                activeFields.Add(field);
            }
        }
    }

    private void DestroyFields()
    {
        foreach (SlowField field in activeFields)
        {
            if (field != null)
            {
                Destroy(field.gameObject);
            }
        }
        activeFields.Clear();
    }

    public override bool SkillLevelUpdate(int newLevel)
    {
        if (base.SkillLevelUpdate(newLevel))
        {
            DestroyFields();
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = skillData?.Description ?? "SlowField description";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            int totalFields = baseFieldCount + (additionalFieldPerLevel * (skillData.GetCurrentTypeStat().baseStat.skillLevel - 1));
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nField Count: {totalFields}" +
                       $"\nField Radius: {Radius:F1}" +
                       $"\nDuration: {Duration:F1}s" +
                       $"\nCooldown: {cooldown:F1}s";
        }
        return baseDesc;
    }
}
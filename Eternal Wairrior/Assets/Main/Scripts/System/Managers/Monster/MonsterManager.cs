using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterManager : SingletonManager<MonsterManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Spawn Settings")]
    [Tooltip("스폰 최소/최대 수, Y : 최대")]
    public Vector2Int minMaxCount;

    [Tooltip("최소/최대 스폰 거리.\n X : 최소, Y : 최대")]
    public Vector2 minMaxDist;
    public float spawnInterval;

    [Header("Monster Settings")]
    public MeleeEnemy meleeEnemyPrefab;
    public RangedEnemy rangedEnemyPrefab;

    [Header("Boss Settings")]
    public BossMonster bossPrefab;
    public Vector2 bossSpawnOffset = new Vector2(0, 5f);

    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    private bool isBossDefeated = false;
    private Vector3 lastBossPosition;

    public bool IsBossDefeated => isBossDefeated;
    public Vector3 LastBossPosition => lastBossPosition;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!PoolManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for PoolManager to initialize...");
            return;
        }
        try
        {
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing MonsterManager: {e.Message}");
            IsInitialized = false;
        }
    }

    #region Spawn Management
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }
    }

    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            isSpawning = false;
        }

        ClearCurrentEnemies();
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            int enemyCount = Random.Range(minMaxCount.x, minMaxCount.y);
            SpawnEnemies(enemyCount);
        }
    }

    private void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 playerPos = GameManager.Instance.player.transform.position;
            Vector2 spawnPos = GetValidSpawnPosition(playerPos);

            if (Random.value < 0.5f)
            {
                PoolManager.Instance.Spawn<MeleeEnemy>(
                    meleeEnemyPrefab.gameObject,
                    spawnPos,
                    Quaternion.identity
                );
            }
            else
            {
                PoolManager.Instance.Spawn<RangedEnemy>(
                    rangedEnemyPrefab.gameObject,
                    spawnPos,
                    Quaternion.identity
                );
            }
        }
    }

    private Vector2 GetValidSpawnPosition(Vector2 playerPos)
    {
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 ranPos = Random.insideUnitCircle;
            Vector2 spawnPos =
                (ranPos * (minMaxDist.y - minMaxDist.x)) + (ranPos.normalized * minMaxDist.x);
            Vector2 finalPos = playerPos + spawnPos;

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(finalPos);
            if (node != null && node.walkable)
            {
                return finalPos;
            }
        }

        return FindNearestWalkablePosition(playerPos);
    }

    private Vector2 FindNearestWalkablePosition(Vector2 centerPos)
    {
        float searchRadius = 1f;
        float maxSearchRadius = minMaxDist.y;
        float radiusIncrement = 1f;

        while (searchRadius <= maxSearchRadius)
        {
            for (float angle = 0; angle < 360; angle += 45)
            {
                float radian = angle * Mathf.Deg2Rad;
                Vector2 checkPos =
                    centerPos
                    + new Vector2(
                        Mathf.Cos(radian) * searchRadius,
                        Mathf.Sin(radian) * searchRadius
                    );

                Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPos);
                if (node != null && node.walkable)
                {
                    return checkPos;
                }
            }
            searchRadius += radiusIncrement;
        }

        return centerPos;
    }
    #endregion

    #region Boss Management
    public void SpawnStageBoss()
    {
        StopSpawning();
        ClearCurrentEnemies();

        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 spawnPos = playerPos + new Vector3(bossSpawnOffset.x, bossSpawnOffset.y, 0);

        BossMonster boss = PoolManager.Instance.Spawn<BossMonster>(
            bossPrefab.gameObject,
            spawnPos,
            Quaternion.identity
        );

        isBossDefeated = false;
    }

    public void OnBossDefeated(Vector3 position)
    {
        isBossDefeated = true;
        lastBossPosition = position;
        GameLoopManager.Instance.GetCurrentHandler<StageStateHandler>()?.OnBossDefeated(position);
    }
    #endregion

    private void ClearCurrentEnemies()
    {
        var enemies = FindObjectsOfType<Enemy>().Where(e => !(e is BossMonster));
        foreach (var enemy in enemies)
        {
            PoolManager.Instance.Despawn(enemy);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class MonsterSpawnData
{
    public Transform spawnPoint;
    public Monster monsterPrefab;
    public int requiredCheckpointId;
    public float spawnDelay = 3f;
    public bool hasSpawned = false;
}

[System.Serializable]
public class EliteMonsterData
{
    public Monster eliteMonsterPrefab;
    public Transform spawnPoint;
    public int checkpointId;
}

public class MonsterManager : SingletonManager<MonsterManager>
{
    [SerializeField] private List<MonsterSpawnData> spawnDataList = new List<MonsterSpawnData>();
    [SerializeField] private List<EliteMonsterData> eliteMonsterDataList = new List<EliteMonsterData>();

    [Header("Monster Gauge Settings")]
    [SerializeField] private float maxGauge = 100f;
    [SerializeField] private float currentGauge = 0f;
    [SerializeField] private float gaugePerKill = 10f;

    public List<Monster> currentMonsters = new List<Monster>();

    private int currentCheckpoint = 0;
    private Monster currentEliteMonster;

    [SerializeField] private int maxMobCount = 20;

    public float GaugePercentage => currentGauge / maxGauge;

    [Header("Barriers")]
    [SerializeField] private List<Barrier> barriers = new List<Barrier>();

    [Header("Continuous Spawn Settings")]
    [SerializeField] private float respawnDelay = 5f;
    private Dictionary<MonsterSpawnData, Coroutine> spawnCoroutines = new Dictionary<MonsterSpawnData, Coroutine>();

    public bool isInitialized = false;

    private bool isEliteSpawning = false;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);
        GameManager.Instance.OnCheckpointReached += HandleCheckpoint;
        Monster.OnMonsterDeath += HandleMonsterDeath;
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCheckpointReached -= HandleCheckpoint;
        Monster.OnMonsterDeath -= HandleMonsterDeath;
    }

    private void HandleMonsterDeath(Monster monster)
    {
        if (monster == currentEliteMonster)
        {
            currentEliteMonster = null;
            DisableBarrier(currentCheckpoint);

            if (currentCheckpoint == 3)
            {
                HandleCheckpointFinalBossDeath();
            }
            return;
        }

        AddGauge(gaugePerKill);
    }

    private void AddGauge(float amount)
    {
        if (isEliteSpawning || currentEliteMonster != null) return;

        currentGauge = Mathf.Min(currentGauge + amount, maxGauge);

        if (currentGauge >= maxGauge)
        {
            StartCoroutine(TriggerEliteSpawn());
        }
    }

    private IEnumerator TriggerEliteSpawn()
    {
        if (isEliteSpawning) yield break;

        isEliteSpawning = true;

        foreach (Monster monster in currentMonsters)
        {
            if (monster != null && monster.IsAlive && monster != currentEliteMonster)
            {
                monster.MonsterDie();
            }
        }

        foreach (var coroutine in spawnCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        spawnCoroutines.Clear();

        yield return new WaitForSeconds(2f);

        SpawnEliteMonster(currentCheckpoint);

        currentGauge = 0f;
        isEliteSpawning = false;
    }

    private void SpawnEliteMonster(int checkpointId)
    {
        var eliteMonsterData = eliteMonsterDataList.Find(data => data.checkpointId == checkpointId);
        if (eliteMonsterData != null && eliteMonsterData.eliteMonsterPrefab != null)
        {
            currentEliteMonster = Instantiate(
                eliteMonsterData.eliteMonsterPrefab.gameObject,
                eliteMonsterData.spawnPoint.position,
                Quaternion.identity
            ).GetComponent<Monster>();
            currentEliteMonster.Initialize();
        }
    }

    private void HandleCheckpoint(int checkpointId)
    {
        foreach (var coroutine in spawnCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        spawnCoroutines.Clear();

        currentCheckpoint = checkpointId;
        currentGauge = 0f;
        CheckAndSpawnMonsters();
    }

    private void CheckAndSpawnMonsters()
    {
        foreach (var spawnData in spawnDataList)
        {
            if (!spawnData.hasSpawned && spawnData.requiredCheckpointId == currentCheckpoint)
            {
                StartContinuousSpawn(spawnData);
            }
        }
    }

    private void StartContinuousSpawn(MonsterSpawnData spawnData)
    {
        spawnData.hasSpawned = true;
        if (spawnCoroutines.ContainsKey(spawnData))
        {
            StopCoroutine(spawnCoroutines[spawnData]);
        }
        var coroutine = StartCoroutine(ContinuousSpawnRoutine(spawnData));
        spawnCoroutines[spawnData] = coroutine;
    }

    private IEnumerator ContinuousSpawnRoutine(MonsterSpawnData spawnData)
    {
        yield return new WaitForSeconds(spawnData.spawnDelay);

        while (currentEliteMonster == null)
        {
            if (currentMonsters.Count <= maxMobCount)
            {
                Monster monster = PoolManager.Instance.Spawn<Monster>(
                    spawnData.monsterPrefab.gameObject,
                    spawnData.spawnPoint.position,
                    Quaternion.identity
                    );
                monster.Initialize();
                currentMonsters.Add(monster);
            }


            yield return new WaitForSeconds(respawnDelay);
        }
    }

    public void AddSpawnData(MonsterSpawnData spawnData)
    {
        spawnDataList.Add(spawnData);
    }

    private void DisableBarrier(int checkpointId)
    {
        var barrier = barriers.Find(b => b.CheckpointId == checkpointId);
        barrier?.DisableBarrier();
    }

    private void EnableAllBarriers()
    {
        foreach (var barrier in barriers)
        {
            barrier.EnableBarrier();
        }
    }

    public void ResetSpawnData()
    {
        foreach (var coroutine in spawnCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        spawnCoroutines.Clear();

        foreach (var spawnData in spawnDataList)
        {
            spawnData.hasSpawned = false;
        }
        currentCheckpoint = 0;
        currentGauge = 0f;
        currentEliteMonster = null;
        EnableAllBarriers();
    }

    private void HandleCheckpointFinalBossDeath()
    {
        StartCoroutine(FinalBossDeathRoutine());
    }

    private IEnumerator FinalBossDeathRoutine()
    {
        yield return new WaitForSeconds(10f);

        GameManager.Instance.ResetAllCheckpoints();
        PlayerManager.instance.RespawnPlayer(GameManager.instance.spawnPoint);
    }
}
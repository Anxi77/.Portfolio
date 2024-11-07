using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                ResetAllData();
                break;
            case "GameScene":
                InitializeMainStage();
                StartCoroutine(InitializeMonsterSpawn());
                break;
            case "BossStage":
                InitializeBossStage();
                break;
            case "TestScene":
                InitializeTestStage();
                break;
        }
    }

    private void ResetAllData()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            PlayerUnitManager.Instance.InitializeNewPlayer();
            SkillManager.Instance.ResetForNewStage();
        }

        // ���� ����
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();
    }

    private IEnumerator InitializeMainStage()
    {
        // �Ŵ������� �ʱ�ȭ�� ������ ���
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            SkillDataManager.Instance != null &&
            SkillDataManager.Instance.IsInitialized);

        var player = GameManager.Instance?.player;
        if (player != null)
        {
            PlayerUnitManager.Instance.LoadGameState();
            SkillManager.Instance.ResetForNewStage();
            InitializeStageItems();
        }
    }

    private void InitializeBossStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // �ӽ� ȿ���� ����
            PlayerUnitManager.Instance.ClearTemporaryEffects();
        }
    }

    private void InitializeTestStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // �׽�Ʈ�� ���� ����
            PlayerUnitManager.Instance.InitializeTestPlayer();

            // ��ų �ʱ�ȭ
            SkillManager.Instance.ResetForNewStage();

            // ������ �ʱ�ȭ
            InitializeStageItems();
        }
    }

    private IEnumerator InitializeMonsterSpawn()
    {
        // �÷��̾ ������ �ʱ�ȭ�� ������ ���
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        // Ǯ �ʱ�ȭ
        ClearAllPools();
        InitializeObjectPools();

        // ���� �Ŵ��� �ʱ�ȭ �� ���� ����
        MonsterManager.Instance.StartSpawning();
    }

    private void ClearAllPools()
    {
        PoolManager.Instance.ClearAllPools();
    }

    private void InitializeObjectPools()
    {
        PoolManager.Instance.InitializePool();
    }

    private void InitializeStageItems()
    {
        // ������̺� �ʱ�ȭ
        ItemManager.Instance.LoadDropTables();
    }

    private void CleanupStageItems()
    {
        var droppedItems = FindObjectsOfType<Item>();
        foreach (var item in droppedItems)
        {
            PoolManager.Instance.Despawn<Item>(item);
        }
    }

    public IEnumerator LoadStageAsync(string sceneName)
    {
        // 1. ���� ���� ����
        GameManager.Instance.SaveGameData();
        PlayerUnitManager.Instance.SaveGameState();

        // 2. �� ��ȯ �غ�
        CleanupStageItems();
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();

        // 3. �� �ε�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. �� �� �ʱ�ȭ - �̺�Ʈ ������� ����
        PlayerUnitManager.Instance.OnGameStateLoaded += OnGameStateLoaded;
        PlayerUnitManager.Instance.LoadGameState();
    }

    private void OnGameStateLoaded()
    {
        // �̺�Ʈ ���� ����
        PlayerUnitManager.Instance.OnGameStateLoaded -= OnGameStateLoaded;

        // ���� �ʱ�ȭ ����
        switch (SceneManager.GetActiveScene().name)
        {
            case "GameScene":
                InitializeMainStage();
                StartCoroutine(InitializeMonsterSpawn());
                break;
            case "BossStage":
                InitializeBossStage();
                break;
        }
    }

    public void HandleStageClearRewards(StageType stageType)
    {
        var rewards = GenerateStageRewards(stageType);
        foreach (var reward in rewards)
        {
            PlayerUnitManager.Instance.AddItem(reward);
        }
    }

    private List<ItemData> GenerateStageRewards(StageType stageType)
    {
        var rewards = new List<ItemData>();
        switch (stageType)
        {
            case StageType.Normal:
                rewards.AddRange(ItemManager.Instance.GetRandomItems(3));
                break;
            case StageType.Boss:
                rewards.AddRange(ItemManager.Instance.GetRandomItems(1, ItemType.Weapon));
                rewards.AddRange(ItemManager.Instance.GetRandomItems(2, ItemType.Armor));
                break;
        }
        return rewards;
    }

    public void LoadTestScene()
    {
        StartCoroutine(LoadStageAsync("TestScene"));
    }

    public void ReturnToMainStage()
    {
        StartCoroutine(LoadStageAsync("GameScene"));
    }

    public void TransferToMainStage(Player player, Vector3 spawnPosition)
    {
        StartCoroutine(TransferPlayerCoroutine(player, spawnPosition));
    }

    private IEnumerator TransferPlayerCoroutine(Player player, Vector3 spawnPosition)
    {
        if (player == null)
        {
            Debug.LogError("Player is null!");
            yield break;
        }

        // 1. ���� ���� ����
        PlayerUnitManager.Instance.SaveGameState();

        // 2. �� ��ȯ �غ�
        CleanupStageItems();
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();

        // 3. �� �ε�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. �÷��̾� ��ġ ����
        player.transform.position = spawnPosition;

        // 5. ���� ���� �ε� �� �ʱ�ȭ
        PlayerUnitManager.Instance.OnGameStateLoaded += OnGameStateLoaded;
        PlayerUnitManager.Instance.LoadGameState();

        // 6. ���� �������� �ʱ�ȭ
        yield return StartCoroutine(InitializeMainStage());
        StartCoroutine(InitializeMonsterSpawn());
    }

    // �������� ���� ���������� �̵��� �� ����� �� �ִ� ���� �޼���
    public void TransferFromTownToMainStage(Player player)
    {
        // ���� ���������� �⺻ ���� ��ġ ����
        Vector3 mainStageSpawnPosition = new Vector3(0, 0, 0); // ���ϴ� ���� ��ġ�� ����
        TransferToMainStage(player, mainStageSpawnPosition);
    }

    private const float STAGE_DURATION = 600f; // 10��
    private Portal bossPortal;
    private Portal townPortal;

    public void StartMainStageLoop()
    {
        StartCoroutine(MainStageLoopCoroutine());
    }

    private IEnumerator MainStageLoopCoroutine()
    {
        // 1. �������� Ÿ�̸� ����
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);

        // 2. �Ϲ� ���� ���� ����
        MonsterManager.Instance.StartSpawning();

        // 3. Ÿ�̸� ���� ���
        yield return new WaitUntil(() => StageTimeManager.Instance.IsStageTimeUp());

        // 4. ���� ���� �˸�
        UIManager.Instance.ShowBossWarning();

        // 5. ���� ����
        yield return StartCoroutine(SpawnBossWithDelay());

        // 6. ���� óġ ���
        yield return new WaitUntil(() => IsBossDefeated());

        // 7. ���� ��Ż ����
        SpawnTownPortal();
    }

    private IEnumerator SpawnBossWithDelay()
    {
        yield return new WaitForSeconds(3f); // ��� �޽��� ǥ�� �ð�
        MonsterManager.Instance.SpawnStageBoss();
    }

    private bool IsBossDefeated()
    {
        return MonsterManager.Instance.IsBossDefeated;
    }

    private void SpawnTownPortal()
    {
        Vector3 bossPosition = MonsterManager.Instance.LastBossPosition;
        // �������� ���� �����ϵ��� ����
        GameObject portalPrefab = Resources.Load<GameObject>("Prefabs/TownPortal");
        if (portalPrefab != null)
        {
            townPortal = PoolManager.Instance.Spawn<Portal>(portalPrefab, bossPosition, Quaternion.identity);
            townPortal.Initialize("Town", OnTownPortalEnter);
        }
        else
        {
            Debug.LogError("TownPortal prefab not found in Resources folder!");
        }
    }

    private void OnTownPortalEnter()
    {
        StartCoroutine(ReturnToTownCoroutine());
    }

    private IEnumerator ReturnToTownCoroutine()
    {
        // 1. ���� ���� ����
        PlayerUnitManager.Instance.SaveGameState();

        // 2. ���� ����
        HandleStageClearRewards(StageType.Boss);

        // 3. �� ��ȯ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TownScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. �÷��̾� ��ġ ����
        GameManager.Instance.player.transform.position = GetTownSpawnPosition();

        // 5. ���� ����
        PlayerUnitManager.Instance.LoadGameState();
    }

    private Vector3 GetTownSpawnPosition()
    {
        // ������ ���� ����Ʈ ��ȯ
        return new Vector3(0, 0, 0); // ���� ���� ���� ��ġ�� ���� �ʿ�
    }
}

public enum StageType
{
    Normal,
    Boss
}
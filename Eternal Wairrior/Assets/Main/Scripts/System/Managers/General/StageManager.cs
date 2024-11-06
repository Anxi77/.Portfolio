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

    private void InitializeMainStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // ���� �ʱ�ȭ
            PlayerUnitManager.Instance.LoadGameState();

            // ��ų �ʱ�ȭ
            SkillManager.Instance.ResetForNewStage();

            // ������ �ʱ�ȭ
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

        // 4. �� �� �ʱ�ȭ
        switch (sceneName)
        {
            case "GameScene":
                InitializeMainStage();
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
}

public enum StageType
{
    Normal,
    Boss
}
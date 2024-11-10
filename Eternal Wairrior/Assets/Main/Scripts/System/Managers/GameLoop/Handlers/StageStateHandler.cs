using UnityEngine;
using System.Collections;
using static StageManager;

public class StageStateHandler : IGameStateHandler
{
    private const float STAGE_DURATION = 600f;
    private bool isBossPhase = false;
    private bool isInitialized = false;

    public void OnEnter()
    {
        Debug.Log("Entering Stage state");
        isInitialized = false;

        // UI �ʱ� ����
        UIManager.Instance.ClearUI();

        // �κ��丮 UI ��Ȱ��ȭ
        UIManager.Instance.SetInventoryAccessible(false);
        UIManager.Instance.HideInventory();
        Debug.Log("Inventory UI disabled for Stage");

        // �÷��̾� ���� �� �ʱ�ȭ
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Game);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Stage");

            MonoBehaviour coroutineRunner = GameLoopManager.Instance;
            coroutineRunner.StartCoroutine(InitializeStageAfterPlayerSpawn());
        }
        else
        {
            InitializeStage();
        }
    }

    private IEnumerator InitializeStageAfterPlayerSpawn()
    {
        // �ε� ��ũ���� ������ ����� ������ ���
        while (UIManager.Instance.IsLoadingScreenVisible())
        {
            yield return null;
        }

        // �ణ�� ������ �༭ ȭ�� ��ȯ�� ������ ������ ��ٸ�
        yield return new WaitForSeconds(0.2f);

        InitializeStage();
    }

    private void InitializeStage()
    {
        // ����� ������ �ε�
        if (GameManager.Instance.HasSaveData())
        {
            PlayerUnitManager.Instance.LoadGameState();
        }

        // ī�޶� ����
        CameraManager.Instance.SetupCamera(SceneType.Game);

        // PathFinding Ȱ��ȭ
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(true);
            PathFindingManager.Instance.InitializeWithNewCamera();
        }

        // �÷��̾� UI �ʱ�ȭ
        if (UIManager.Instance?.playerUIPanel != null)
        {
            UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
            UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // �÷��̾� ����ġ üũ
        GameManager.Instance.StartLevelCheck();

        // �������� Ÿ�̸� ����
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);
        UIManager.Instance.stageTimeUI.gameObject.SetActive(true);
        Debug.Log("Stage timer started");

        Debug.Log("Stage initialization complete");
        isInitialized = true;

        // �ε��� �Ϸ�� �� ���� ���� ����
        GameLoopManager.Instance.StartCoroutine(StartMonsterSpawningWhenReady());
    }

    private IEnumerator StartMonsterSpawningWhenReady()
    {
        // �ణ�� ���� �ð��� �ξ� �ٸ� �ʱ�ȭ�� �Ϸ�ǵ��� ��
        yield return new WaitForSeconds(0.5f);

        // ���� �Ŵ��� �ʱ�ȭ �� ���� ����
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.StartSpawning();
            Debug.Log("Monster spawning started");
        }
    }

    public void OnExit()
    {
        Debug.Log("Exiting Stage state");
        isInitialized = false;

        // �÷��̾� ������ ���� �� ������Ʈ ����
        if (GameManager.Instance?.player != null)
        {
            // �κ��丮 ���� ����
            if (GameManager.Instance.player.GetComponent<Inventory>() != null)
            {
                GameManager.Instance.player.GetComponent<Inventory>().SaveInventoryState();
            }

            // �÷��̾� ���� ����
            PlayerUnitManager.Instance?.SaveGameState();

            // �÷��̾� ������Ʈ ����
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        // �������� ���� �ý��� ����
        MonsterManager.Instance?.StopSpawning();
        StageTimeManager.Instance?.PauseTimer();
        StageTimeManager.Instance?.ResetTimer();
        CameraManager.Instance?.ClearCamera();

        // PathFinding ��Ȱ��ȭ
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }
    }

    public void OnUpdate()
    {
        if (!isInitialized) return;

        if (!isBossPhase && StageTimeManager.Instance.IsStageTimeUp())
        {
            StartBossPhase();
        }
    }

    public void OnFixedUpdate() { }

    private void StartBossPhase()
    {
        isBossPhase = true;
        UIManager.Instance?.ShowBossWarning();
        MonsterManager.Instance?.SpawnStageBoss();
    }

    public void OnBossDefeated(Vector3 position)
    {
        StageManager.Instance?.SpawnTownPortal(position);
    }
}
using UnityEngine;
using System.Collections;
using static StageManager;

public class StageStateHandler : IGameStateHandler
{
    private const float STAGE_DURATION = 600f;
    private bool isBossPhase = false;

    public void OnEnter()
    {
        Debug.Log("Entering Stage state");

        // UI �ʱ� ����
        UIManager.Instance.ClearUI();

        // �÷��̾� ���� �� �ʱ�ȭ
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Game);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Stage");

            // �÷��̾� ���� �� ��� ����Ͽ� ��� ������Ʈ�� �ʱ�ȭ�ǵ��� ��
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
        // �÷��̾� ������Ʈ���� ������ �ʱ�ȭ�� ������ ���
        yield return new WaitForSeconds(0.1f);
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
        if (UIManager.Instance?.playerUIManager != null)
        {
            UIManager.Instance.playerUIManager.gameObject.SetActive(true);
            UIManager.Instance.playerUIManager.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // �÷��̾� ����ġ üũ
        GameManager.Instance.StartLevelCheck();

        // ���� �Ŵ��� �ʱ�ȭ
        MonsterManager.Instance.StartSpawning();

        // �������� Ÿ�̸� ����
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);

        Debug.Log("Stage initialization complete");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Stage state");
        // �������� ���� �� ������ ����
        PlayerUnitManager.Instance.SaveGameState();

        MonsterManager.Instance.StopSpawning();
        CameraManager.Instance.ClearCamera();
    }

    public void OnUpdate()
    {
        if (!isBossPhase && StageTimeManager.Instance.IsStageTimeUp())
        {
            StartBossPhase();
        }
    }

    public void OnFixedUpdate() { }

    private void StartBossPhase()
    {
        isBossPhase = true;
        UIManager.Instance.ShowBossWarning();
        MonsterManager.Instance.SpawnStageBoss();
    }

    public void OnBossDefeated(Vector3 position)
    {
        // ���� óġ ��ġ�� Ÿ�� ��Ż ����
        StageManager.Instance.SpawnTownPortal(position);
    }
}
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
        isInitialized = false;

        UIManager.Instance.ClearUI();

        UIManager.Instance.SetInventoryAccessible(false);
        UIManager.Instance.HideInventory();

        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Game);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);

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
        while (UIManager.Instance.IsLoadingScreenVisible())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        InitializeStage();
    }

    private void InitializeStage()
    {
        if (GameManager.Instance.HasSaveData())
        {
            PlayerUnitManager.Instance.LoadGameState();
        }

        CameraManager.Instance.SetupCamera(SceneType.Game);

        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(true);
            PathFindingManager.Instance.InitializeWithNewCamera();
        }

        if (UIManager.Instance?.playerUIPanel != null)
        {
            UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
            UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
        }

        GameManager.Instance.StartLevelCheck();

        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);
        UIManager.Instance.stageTimeUI.gameObject.SetActive(true);
        isInitialized = true;

        GameLoopManager.Instance.StartCoroutine(StartMonsterSpawningWhenReady());
    }

    private IEnumerator StartMonsterSpawningWhenReady()
    {
        yield return new WaitForSeconds(0.5f);

        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.StartSpawning();
        }
    }

    public void OnExit()
    {
        isInitialized = false;

        if (GameManager.Instance?.player != null)
        {
            if (GameManager.Instance.player.GetComponent<Inventory>() != null)
            {
                GameManager.Instance.player.GetComponent<Inventory>().SaveInventoryState();
            }

            PlayerUnitManager.Instance?.SaveGameState();

            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        MonsterManager.Instance?.StopSpawning();
        StageTimeManager.Instance?.PauseTimer();
        StageTimeManager.Instance?.ResetTimer();
        CameraManager.Instance?.ClearCamera();

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
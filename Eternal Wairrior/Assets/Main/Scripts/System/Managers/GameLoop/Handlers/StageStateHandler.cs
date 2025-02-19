using System.Collections;
using UnityEngine;
using static StageManager;

public class StageStateHandler : BaseStateHandler
{
    private const float STAGE_DURATION = 600f;
    private bool isBossPhase = false;
    private bool isInitialized = false;

    public override void OnEnter()
    {
        base.OnEnter();
        isInitialized = false;

        UI.SetInventoryAccessible(false);
        UI.HideInventory();

        if (Game != null && Game.player == null)
        {
            Vector3 spawnPos = PlayerUnit.GetSpawnPosition(SceneType.Game);
            PlayerUnit.SpawnPlayer(spawnPos);
            StartCoroutine(InitializeStageAfterPlayerSpawn());
        }
        else
        {
            InitializeStage();
        }
    }

    private IEnumerator InitializeStageAfterPlayerSpawn()
    {
        while (UI.IsLoadingScreenVisible())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        InitializeStage();
    }

    private void InitializeStage()
    {
        if (Game.HasSaveData())
        {
            PlayerUnit.LoadGameState();
        }

        CameraManager.Instance.SetupCamera(SceneType.Game);

        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(true);
            PathFindingManager.Instance.InitializeWithNewCamera();
        }

        if (UI != null && UI.playerUIPanel != null)
        {
            UI.playerUIPanel.gameObject.SetActive(true);
            UI.playerUIPanel.InitializePlayerUI(Game.player);
        }

        Game.StartLevelCheck();

        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);
        UI.stageTimeUI.gameObject.SetActive(true);
        isInitialized = true;

        StartCoroutine(StartMonsterSpawningWhenReady());
    }

    private IEnumerator StartMonsterSpawningWhenReady()
    {
        yield return new WaitForSeconds(0.5f);

        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.StartSpawning();
        }
    }

    public override void OnExit()
    {
        isInitialized = false;

        base.OnExit();

        MonsterManager.Instance?.StopSpawning();
        StageTimeManager.Instance?.PauseTimer();
        StageTimeManager.Instance?.ResetTimer();
        CameraManager.Instance?.ClearCamera();

        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }
    }

    public override void OnUpdate()
    {
        if (!isInitialized)
            return;

        if (!isBossPhase && StageTimeManager.Instance.IsStageTimeUp())
        {
            StartBossPhase();
        }
    }

    private void StartBossPhase()
    {
        isBossPhase = true;
        UI?.ShowBossWarning();
        MonsterManager.Instance?.SpawnStageBoss();
    }

    public void OnBossDefeated(Vector3 position)
    {
        StageManager.Instance?.SpawnTownPortal(position);
    }
}

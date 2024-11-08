using UnityEngine;
using System.Collections;
using static StageManager;

public class TownStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Debug.Log("Entering Town state");

        // UI �ʱ� ����
        UIManager.Instance.ClearUI();

        // Ÿ�� ���� �� �ʱ�ȭ
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Town");

            // �÷��̾� ���� �� ��� ����Ͽ� ��� ������Ʈ�� �ʱ�ȭ�ǵ��� ��
            MonoBehaviour coroutineRunner = GameLoopManager.Instance;
            coroutineRunner.StartCoroutine(InitializeTownAfterPlayerSpawn());
        }
        else
        {
            InitializeTown();
        }
    }

    private IEnumerator InitializeTownAfterPlayerSpawn()
    {
        // �÷��̾� ������Ʈ���� ������ �ʱ�ȭ�� ������ ���
        yield return new WaitForSeconds(0.1f);
        InitializeTown();
    }

    private void InitializeTown()
    {
        // ī�޶� ����
        CameraManager.Instance.SetupCamera(SceneType.Town);

        // PathFinding ��Ȱ��ȭ
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }

        // �÷��̾� UI �ʱ�ȭ
        if (UIManager.Instance?.playerUIManager != null)
        {
            UIManager.Instance.playerUIManager.gameObject.SetActive(true);
            UIManager.Instance.playerUIManager.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // ���� �������� ��Ż ����
        StageManager.Instance.SpawnGameStagePortal();
        Debug.Log("Game stage portal spawned");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Town state");
        PlayerUnitManager.Instance.SaveGameState();
    }

    public void OnUpdate()
    {
        // Ÿ�� ���� ������Ʈ ����
    }

    public void OnFixedUpdate() { }
}
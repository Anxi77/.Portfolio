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

        // �÷��̾ ���� �������� Ȯ��
        if (GameManager.Instance?.player?.playerStatus == Player.Status.Dead)
        {
            Debug.Log("Player is dead, respawning...");
            RespawnPlayer();
        }
        // �÷��̾ ���� ���
        else if (GameManager.Instance?.player == null)
        {
            Debug.Log("No player found, spawning new player");
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
        }

        // �÷��̾� �ʱ�ȭ �� UI ������ �ڷ�ƾ���� ����
        MonoBehaviour coroutineRunner = GameLoopManager.Instance;
        coroutineRunner.StartCoroutine(InitializeTownAfterPlayerSpawn());
    }

    private void RespawnPlayer()
    {
        // ���� �÷��̾� ����
        if (GameManager.Instance.player != null)
        {
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        // �� �÷��̾� ����
        Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
        PlayerUnitManager.Instance.SpawnPlayer(spawnPos);

        // �÷��̾� ���� ����
        if (GameManager.Instance.player != null)
        {
            GameManager.Instance.player.playerStatus = Player.Status.Alive;
            PlayerUnitManager.Instance.LoadGameState();
        }
    }

    private IEnumerator InitializeTownAfterPlayerSpawn()
    {
        // �÷��̾ ������ �ʱ�ȭ�� ������ ���
        while (GameManager.Instance?.player == null || !GameManager.Instance.player.IsInitialized)
        {
            yield return null;
        }

        InitializeTown();
    }

    private void InitializeTown()
    {
        if (GameManager.Instance?.player == null)
        {
            Debug.LogError("Cannot initialize town: Player is null");
            return;
        }

        // ī�޶� ����
        CameraManager.Instance.SetupCamera(SceneType.Town);

        // PathFinding ��Ȱ��ȭ
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }

        // UI �ʱ�ȭ
        if (UIManager.Instance != null)
        {
            // �÷��̾� UI �ʱ�ȭ
            if (UIManager.Instance.playerUIPanel != null)
            {
                UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
                UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
                Debug.Log("Player UI initialized");
            }

            // �κ��丮 ������ �ε�
            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var savedData = PlayerDataManager.Instance.CurrentInventoryData;
                if (savedData != null)
                {
                    inventory.LoadInventoryData(savedData);  // ���� �޼��� ���
                    Debug.Log("Inventory data loaded");
                }
            }

            // �κ��丮 UI �ʱ�ȭ �� Ȱ��ȭ
            UIManager.Instance.InitializeInventoryUI();
            UIManager.Instance.SetInventoryAccessible(true);
            UIManager.Instance.UpdateInventoryUI();
            Debug.Log("Inventory UI initialized and enabled");
        }

        // ���� �������� ��Ż ����
        StageManager.Instance.SpawnGameStagePortal();
        Debug.Log("Game stage portal spawned");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Town state");

        // �κ��丮 ������ ����
        if (GameManager.Instance?.player != null)
        {
            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var inventoryData = inventory.GetInventoryData();  // ���� �޼��� ���
                PlayerDataManager.Instance.SaveInventoryData(inventoryData);
                Debug.Log("Inventory data saved");
            }
            PlayerUnitManager.Instance.SaveGameState();
        }

        // �κ��丮 UI ��Ȱ��ȭ
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetInventoryAccessible(false);
            UIManager.Instance.HideInventory();
            Debug.Log("Inventory UI disabled");
        }
    }

    public void OnUpdate() { }

    public void OnFixedUpdate() { }
}
using UnityEngine;
using System.Collections;
using static StageManager;

public class TownStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Debug.Log("Entering Town state");

        UIManager.Instance.ClearUI();

        if (GameManager.Instance?.player?.playerStatus == Player.Status.Dead)
        {
            Debug.Log("Player is dead, respawning...");
            RespawnPlayer();
        }
        else if (GameManager.Instance?.player == null)
        {
            Debug.Log("No player found, spawning new player");
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
        }

        MonoBehaviour coroutineRunner = GameLoopManager.Instance;
        coroutineRunner.StartCoroutine(InitializeTownAfterPlayerSpawn());
    }

    private void RespawnPlayer()
    {
        if (GameManager.Instance.player != null)
        {
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
        PlayerUnitManager.Instance.SpawnPlayer(spawnPos);

        if (GameManager.Instance.player != null)
        {
            GameManager.Instance.player.playerStatus = Player.Status.Alive;
            PlayerUnitManager.Instance.LoadGameState();
        }
    }

    private IEnumerator InitializeTownAfterPlayerSpawn()
    {
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

        CameraManager.Instance.SetupCamera(SceneType.Town);

        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.playerUIPanel != null)
            {
                UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
                UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
                Debug.Log("Player UI initialized");
            }

            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                PlayerDataManager.Instance.LoadPlayerData();
            }

            UIManager.Instance.InitializeInventoryUI();
            UIManager.Instance.SetInventoryAccessible(true);
            UIManager.Instance.UpdateInventoryUI();
            Debug.Log("Inventory UI initialized and enabled");
        }

        StageManager.Instance.SpawnGameStagePortal();
        Debug.Log("Game stage portal spawned");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Town state");

        if (GameManager.Instance?.player != null)
        {
            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var inventoryData = inventory.GetInventoryData();
                PlayerDataManager.Instance.SaveInventoryData(inventoryData);
                Debug.Log("Inventory data saved");
            }
            PlayerUnitManager.Instance.SaveGameState();
        }

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
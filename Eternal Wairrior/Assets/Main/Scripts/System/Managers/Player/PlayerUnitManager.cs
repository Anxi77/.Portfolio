using System;
using UnityEngine;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>, IInitializable
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Vector3 defaultSpawnPosition = Vector3.zero;

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        try
        {
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing PlayerUnitManager: {e.Message}");
            IsInitialized = false;
        }
    }

    public void SpawnPlayer(Vector3 position)
    {
        if (GameManager.Instance.player != null)
        {
            Debug.LogWarning("Player already exists, destroying old player");
            Destroy(GameManager.Instance.player.gameObject);
        }

        try
        {
            GameObject playerObj = Instantiate(playerPrefab, position, Quaternion.identity);
            Player player = playerObj.GetComponent<Player>();

            if (player != null)
            {
                InitializePlayer(player);
            }
            else
            {
                Debug.LogError("Player component not found on spawned object");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error spawning player: {e.Message}");
        }
    }

    public void InitializePlayer(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot initialize null player");
            return;
        }

        try
        {
            GameManager.Instance.player = player;

            PlayerStatSystem playerStat = player.GetComponent<PlayerStatSystem>();
            if (playerStat != null)
            {
                if (PlayerDataManager.Instance.HasSaveData())
                {
                    LoadGameState();
                }
            }

            if (player.characterControl != null)
            {
                player.characterControl.Initialize();
            }

            player.playerStatus = Player.Status.Alive;

            player.StartCombatSystems();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing player: {e.Message}");
        }
    }

    public Vector3 GetSpawnPosition(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Town:
                return new Vector3(0, 0, 0);
            case SceneType.Game:
            case SceneType.Test:
                return defaultSpawnPosition;
            default:
                return Vector3.zero;
        }
    }

    public void SaveGameState()
    {
        if (GameManager.Instance?.player == null)
            return;

        var player = GameManager.Instance.player;
        var playerStat = player.GetComponent<PlayerStatSystem>();
        var inventory = player.GetComponent<Inventory>();

        if (playerStat != null && inventory != null)
        {
            PlayerData data = new PlayerData();
            data.stats = playerStat.CreateSaveData();
            data.inventory = inventory.GetInventoryData();
            PlayerDataManager.Instance.SavePlayerData(data);
        }
    }

    public void LoadGameState()
    {
        if (GameManager.Instance?.player == null)
            return;

        var player = GameManager.Instance.player;
        var playerStat = player.GetComponent<PlayerStatSystem>();
        var inventory = player.GetComponent<Inventory>();

        if (playerStat != null && inventory != null)
        {
            var savedData = PlayerDataManager.Instance.LoadPlayerData();
            if (savedData != null)
            {
                playerStat.LoadFromSaveData(savedData.stats);
                inventory.LoadInventoryData(savedData.inventory);
            }
        }
    }

    public void ClearTemporaryEffects()
    {
        if (GameManager.Instance?.player == null)
            return;

        var player = GameManager.Instance.player;

        if (player.playerStat != null)
        {
            player.playerStat.RemoveStatsBySource(SourceType.Buff);
            player.playerStat.RemoveStatsBySource(SourceType.Debuff);

            player.playerStat.RemoveStatsBySource(SourceType.Consumable);
        }

        player.ResetPassiveEffects();
    }
}

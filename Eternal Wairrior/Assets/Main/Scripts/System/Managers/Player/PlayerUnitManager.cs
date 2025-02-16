using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using static StageManager;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>, IInitializable
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing PlayerUnitManager...");
            IsInitialized = true;
            Debug.Log("PlayerUnitManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PlayerUnitManager: {e.Message}");
            IsInitialized = false;
        }
    }

    public void SpawnPlayer(Vector3 position)
    {
        Debug.Log($"Spawning player at position: {position}");

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
                Debug.Log("Player spawned and initialized successfully");
            }
            else
            {
                Debug.LogError("Player component not found on spawned object");
            }
        }
        catch (System.Exception e)
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
            Debug.Log("Starting player initialization...");

            GameManager.Instance.player = player;

            PlayerStatSystem playerStat = player.GetComponent<PlayerStatSystem>();
            if (playerStat != null)
            {
                float maxHp = playerStat.GetStat(StatType.MaxHp);
                playerStat.SetCurrentHp(maxHp);
                Debug.Log($"Player stats initialized - MaxHP: {maxHp}");

                if (PlayerDataManager.Instance.HasSaveData("CurrentSave"))
                {
                    LoadGameState();
                }
            }

            if (player.characterControl != null)
            {
                player.characterControl.Initialize();
                Debug.Log("Character control initialized");
            }

            player.playerStatus = Player.Status.Alive;

            player.StartCombatSystems();

            Debug.Log("Player initialization completed successfully");
        }
        catch (System.Exception e)
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
        if (GameManager.Instance?.player == null) return;

        var player = GameManager.Instance.player;
        var playerStat = player.GetComponent<PlayerStatSystem>();
        var inventory = player.GetComponent<Inventory>();

        if (playerStat != null)
        {
            PlayerDataManager.Instance.SaveCurrentPlayerStatData();
        }

        if (inventory != null)
        {
            PlayerDataManager.Instance.SaveInventoryData(inventory.GetInventoryData());
        }
    }

    public void LoadGameState()
    {
        if (GameManager.Instance?.player == null) return;

        var player = GameManager.Instance.player;
        var inventory = player.GetComponent<Inventory>();

        var savedData = PlayerDataManager.Instance.LoadPlayerData("CurrentSave");
        if (savedData != null)
        {
            if (inventory != null)
            {
                inventory.LoadInventoryData(savedData.inventory);
            }
        }
    }

    public void ClearTemporaryEffects()
    {
        if (GameManager.Instance?.player == null) return;

        var player = GameManager.Instance.player;

        if (player.playerStat != null)
        {
            player.playerStat.RemoveStatsBySource(SourceType.Buff);
            player.playerStat.RemoveStatsBySource(SourceType.Debuff);

            player.playerStat.RemoveStatsBySource(SourceType.Consumable);
        }

        player.ResetPassiveEffects();

        Debug.Log("Cleared all temporary effects from player");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonManager<GameManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    internal List<Enemy> enemies = new List<Enemy>();
    internal Player player;

    private string lastSavedScene = "TownScene";
    private Vector3 lastSavedPosition = Vector3.zero;
    private bool hasInitializedGame = false;

    private int lastPlayerLevel = 1;
    private Coroutine levelCheckCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!PlayerDataManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for PlayerDataManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing GameManager...");
            IsInitialized = true;
            Debug.Log("GameManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing GameManager: {e.Message}");
            IsInitialized = false;
        }
    }

    public void StartLevelCheck()
    {
        if (levelCheckCoroutine != null)
        {
            StopCoroutine(levelCheckCoroutine);
            levelCheckCoroutine = null;
        }

        if (player != null && player.playerStatus != Player.Status.Dead)
        {
            levelCheckCoroutine = StartCoroutine(CheckLevelUp());
            Debug.Log("Started level check coroutine");
        }
    }

    private IEnumerator CheckLevelUp()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is null in GameManager");
            yield break;
        }

        lastPlayerLevel = player.level;
        Debug.Log($"Starting level check at level: {lastPlayerLevel}");

        while (true)
        {
            if (player == null || player.playerStatus == Player.Status.Dead)
            {
                Debug.Log("Player is dead or null, stopping level check");
                levelCheckCoroutine = null;
                yield break;
            }

            if (player.level > lastPlayerLevel)
            {
                Debug.Log($"Level Up detected: {lastPlayerLevel} -> {player.level}");
                lastPlayerLevel = player.level;
                OnPlayerLevelUp();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnPlayerLevelUp()
    {
        // UI 매니저에게 레벨업 UI를 표시하도록 요청
        UIManager.Instance?.ShowLevelUpPanel();
    }

    public void StopLevelCheck()
    {
        if (levelCheckCoroutine != null)
        {
            StopCoroutine(levelCheckCoroutine);
            levelCheckCoroutine = null;
        }
    }

    #region Player Management
    public void InitializePlayer(Player newPlayer)
    {
        player = newPlayer;
        if (player != null)
        {
            if (PlayerUnitManager.Instance.IsInitialized)
            {
                PlayerUnitManager.Instance.InitializePlayer(player);

                var savedData = PlayerDataManager.Instance.LoadPlayerData("CurrentSave");
                if (savedData != null)
                {
                    PlayerUnitManager.Instance.LoadGameState();
                }
                else
                {
                    PlayerUnitManager.Instance.InitializePlayer(player);
                }
            }
            else
            {
                Debug.LogError("PlayerUnitManager is not initialized!");
            }
        }
    }
    #endregion

    #region Game State Management
    public void InitializeNewGame()
    {
        if (!hasInitializedGame)
        {
            PlayerDataManager.Instance.InitializeDefaultData();
            hasInitializedGame = true;
            Debug.Log("Game initialized for the first time");
        }
        else
        {
            Debug.Log("Resetting existing game");
            ClearGameData();
            PlayerDataManager.Instance.InitializeDefaultData();
        }
    }

    public void SaveGameData()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.SaveGameState();
        }
    }

    public void LoadGameData()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.LoadGameState();
        }
    }

    public void ClearGameData()
    {
        PlayerDataManager.Instance.ClearAllRuntimeData();
        lastSavedScene = "TownScene";
        lastSavedPosition = Vector3.zero;
    }

    public bool HasSaveData()
    {
        return PlayerDataManager.Instance != null &&
               PlayerDataManager.Instance.HasSaveData("CurrentSave");
    }
    #endregion

    #region Scene Management
    public void SaveCurrentSceneAndPosition(string sceneName, Vector3 position)
    {
        lastSavedScene = sceneName;
        lastSavedPosition = position;
    }

    public string GetLastSavedScene()
    {
        return lastSavedScene;
    }

    public Vector3 GetLastSavedPosition()
    {
        return lastSavedPosition;
    }
    #endregion

    private void OnApplicationQuit()
    {
        try
        {
            SaveGameData();
            PlayerDataManager.Instance.SaveWithBackup();
            CleanupTemporaryResources();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during application quit: {e.Message}");
        }
    }

    private void CleanupTemporaryResources()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.ClearTemporaryEffects();
        }
        Resources.UnloadUnusedAssets();
    }
}

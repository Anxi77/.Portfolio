using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    internal List<Enemy> enemies = new List<Enemy>();
    internal Player player;
    private bool hasInitializedGame = false;

    private int lastPlayerLevel = 1;
    private Coroutine levelCheckCoroutine;

    public void Initialize()
    {
        if (!PlayerDataManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for PlayerDataManager to initialize...");
            return;
        }

        try
        {
            IsInitialized = true;
        }
        catch (Exception e)
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
        UIManager.Instance?.ShowLevelUpPanel();
    }

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
    }

    public bool HasSaveData()
    {
        return PlayerDataManager.Instance != null &&
               PlayerDataManager.Instance.HasSaveData();
    }
    #endregion

    private void OnApplicationQuit()
    {
        try
        {
            SaveGameData();
            CleanupTemporaryResources();
        }
        catch (Exception e)
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

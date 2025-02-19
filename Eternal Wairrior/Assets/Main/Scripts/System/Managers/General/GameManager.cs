using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    internal List<Enemy> enemies = new();
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

        while (true)
        {
            if (player == null || player.playerStatus == Player.Status.Dead)
            {
                levelCheckCoroutine = null;
                yield break;
            }

            if (player.level > lastPlayerLevel)
            {
                lastPlayerLevel = player.level;
                OnPlayerLevelUp();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnPlayerLevelUp()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelUpPanel();
        }
    }

    #region Game State Management
    public void InitializeNewGame()
    {
        if (!hasInitializedGame)
        {
            PlayerDataManager.Instance.InitializeDefaultData();
            hasInitializedGame = true;
        }
        else
        {
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
        return PlayerDataManager.Instance != null && PlayerDataManager.Instance.HasSaveData();
    }
    #endregion

    private void OnApplicationQuit()
    {
        try
        {
            SaveGameData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during application quit: {e.Message}");
        }
    }
}

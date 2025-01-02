using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>, IInitializable
{
    private Player Player;
    public Player player { get => Player; set => Player = value; }
    public bool IsInitialized { get; private set; }

    [SerializeField]
    private List<Checkpoint> checkpoints = new List<Checkpoint>();
    private int currentCheckpointId = -1;

    public Transform spawnPoint;

    public event Action<int> OnCheckpointReached;

    [SerializeField]
    private float respawnDelay = 3f;

    public void HandlePlayerDeath()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Transform respawnPosition = spawnPoint;
        PlayerManager.Instance.RespawnPlayer(respawnPosition);
    }

    protected override void Awake()
    {
        IsInitialized = false;
        base.Awake();
        StartCoroutine(InitializeRoutine());
    }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            StartCoroutine(InitializeRoutine());
        }
    }

    private IEnumerator InitializeRoutine()
    {
        Checkpoint[] foundCheckpoints = FindObjectsOfType<Checkpoint>();
        checkpoints.AddRange(foundCheckpoints);

        yield return new WaitUntil(() => PlayerManager.instance != null);
        PlayerManager.Instance.Initialize(spawnPoint);
        CinemachineVirtualCamera vCam = Camera.main.GetComponentInChildren<CinemachineVirtualCamera>();
        vCam.LookAt = player.transform;
        vCam.Follow = player.transform;
        yield return new WaitUntil(() => PlayerManager.instance.IsInitialized);
        yield return new WaitUntil(() => CheckRequiredComponents());
        IsInitialized = true;
        Debug.Log("GameManager initialized successfully");
    }

    private bool CheckRequiredComponents()
    {        
        if (player == null) return false;
        if (!player.IsInitialized) return false;

        return true;
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnResume()
    {
        UIManager.instance.pausePanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    #region Checkpoint System

    public void TriggerCheckpoint(int checkpointId)
    {
        currentCheckpointId = checkpointId;
        OnCheckpointReached?.Invoke(checkpointId);
        SaveCheckpoint();
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.ClearAllDroppedItems();
        }
    }

    public void ResetAllCheckpoints()
    {
        currentCheckpointId = -1;
        foreach (var checkpoint in checkpoints)
        {
            checkpoint.ResetCheckpoint();
        }
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.ResetSpawnData();
        }
    }

    public int GetCurrentCheckpointId()
    {
        return currentCheckpointId;
    }

    private void SaveCheckpoint()
    {
        PlayerPrefs.SetInt("LastCheckpoint", currentCheckpointId);
        PlayerPrefs.Save();
    }

    public void LoadCheckpoint()
    {
        currentCheckpointId = PlayerPrefs.GetInt("LastCheckpoint", -1);
    }

    #endregion
}

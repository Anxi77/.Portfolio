using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class PlayerDataManager : DataManager<PlayerDataManager>
{
    private const string SAVE_FOLDER = "PlayerData";
    private string SAVE_PATH => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
    private const string DEFAULT_SAVE_SLOT = "DefaultSave";

    private PlayerStatData currentPlayerStatData;
    private InventoryData currentInventoryData;
    private LevelData currentLevelData = new LevelData { level = 1, exp = 0f };
    public PlayerStatData CurrentPlayerStatData => currentPlayerStatData;
    public InventoryData CurrentInventoryData => currentInventoryData;

    protected override void LoadRuntimeData()
    {
        try
        {
            var data = JSONIO<PlayerData>.LoadData(DEFAULT_SAVE_SLOT);
            if (data != null)
            {
                currentPlayerStatData = data.stats;
                currentInventoryData = data.inventory;
                currentLevelData = data.levelData;
            }
            else
            {
                CreateDefaultFiles();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading PlayerData: {e.Message}");
        }
    }

    protected virtual void CreateDefaultFiles()
    {
        currentPlayerStatData = new PlayerStatData();
        currentInventoryData = new InventoryData();
        currentLevelData = new LevelData { level = 1, exp = 0f };
        JSONIO<PlayerData>.SaveData(DEFAULT_SAVE_SLOT, new PlayerData { stats = currentPlayerStatData, inventory = currentInventoryData, levelData = currentLevelData });
    }

    public virtual void SaveWithBackup()
    {
        try
        {
            if (!Directory.Exists(SAVE_PATH))
                Directory.CreateDirectory(SAVE_PATH);
            BackupIO.CreateBackup(SAVE_PATH);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during backup: {e.Message}");
        }
    }

    public virtual void ClearAllRuntimeData()
    {
        currentPlayerStatData = new PlayerStatData();
        currentInventoryData = new InventoryData();
        currentLevelData = new LevelData { level = 1, exp = 0f };
        JSONIO<PlayerData>.DeleteData(DEFAULT_SAVE_SLOT);
    }

    public void LoadPlayerStatData(PlayerStatData data)
    {
        if (data != null)
        {
            currentPlayerStatData = data;
        }
    }

    public void SaveCurrentPlayerStatData()
    {
        var player = FindObjectOfType<Player>();
        if (player != null && player.TryGetComponent<PlayerStatSystem>(out var statSystem))
        {
            currentPlayerStatData = statSystem.CreateSaveData();
        }
    }

    public void SavePlayerData(string saveSlot, PlayerData data)
    {
        if (!IsInitialized) Initialize();
        JSONIO<PlayerData>.SaveData(saveSlot, data);
        SaveWithBackup();
    }

    public PlayerData LoadPlayerData(string saveSlot)
    {
        if (!IsInitialized) Initialize();
        var data = JSONIO<PlayerData>.LoadData(saveSlot);
        if (data != null)
        {
            LoadPlayerStatData(data.stats);
            LoadInventoryData(data.inventory);
        }
        return data;
    }

    public void LoadInventoryData(InventoryData data)
    {
        if (data != null)
        {
            currentInventoryData = data;
        }
    }

    public void SaveInventoryData(InventoryData data)
    {
        currentInventoryData = data;
        try
        {
            EnsureDirectoryExists();
            JSONIO<InventoryData>.SaveData(DEFAULT_SAVE_SLOT, currentInventoryData);
            Debug.Log($"Successfully saved inventory data to: {DEFAULT_SAVE_SLOT}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving inventory data: {e.Message}");
        }
    }

    private void EnsureDirectoryExists()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created directory: {savePath}");
        }
    }

    public bool HasSaveData(string saveSlot)
    {
        if (!IsInitialized) Initialize();
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, $"{saveSlot}.json");
        return File.Exists(savePath);
    }
}


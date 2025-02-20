using System.IO;
using UnityEngine;

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
        JSONIO<PlayerData>.SaveData(
            DEFAULT_SAVE_SLOT,
            new PlayerData
            {
                stats = currentPlayerStatData,
                inventory = currentInventoryData,
                levelData = currentLevelData,
            }
        );
    }

    public virtual void ClearAllRuntimeData()
    {
        currentPlayerStatData = new PlayerStatData();
        currentInventoryData = new InventoryData();
        currentLevelData = new LevelData { level = 1, exp = 0f };
        JSONIO<PlayerData>.DeleteData(DEFAULT_SAVE_SLOT);
    }

    public void SavePlayerData(PlayerData data)
    {
        if (!IsInitialized)
            Initialize();
        JSONIO<PlayerData>.SaveData(DEFAULT_SAVE_SLOT, data);
    }

    public PlayerData LoadPlayerData()
    {
        if (!IsInitialized)
            Initialize();
        return JSONIO<PlayerData>.LoadData(DEFAULT_SAVE_SLOT);
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

    public bool HasSaveData()
    {
        if (!IsInitialized)
            Initialize();
        return File.Exists(DEFAULT_SAVE_SLOT);
    }
}

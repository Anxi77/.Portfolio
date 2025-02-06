using UnityEngine;
using System.IO;
using System;

public class PlayerDataManager : DataManager<PlayerDataManager>, IInitializable
{

    private const string SAVE_FOLDER = "PlayerData";
    private string SAVE_PATH => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
    private const string DEFAULT_SAVE_SLOT = "DefaultSave";

    private PlayerStatData currentPlayerStatData;
    private InventoryData currentInventoryData;
    private LevelData currentLevelData = new LevelData { level = 1, exp = 0f };

    public new bool IsInitialized { get; private set; }
    public PlayerStatData CurrentPlayerStatData => currentPlayerStatData;
    public InventoryData CurrentInventoryData => currentInventoryData;

    [System.Serializable]
    public class PlayerSaveData
    {
        public PlayerStatData stats;
        public InventoryData inventory;
        public LevelData levelData;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing PlayerDataManager...");
            InitializeDefaultData();
            IsInitialized = true;
            Debug.Log("PlayerDataManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PlayerDataManager: {e.Message}");
            IsInitialized = false;
        }
    }

    protected override void InitializeManagers()
    {
    }

    protected override void CreateResourceFolders()
    {
        try
        {
            if (!Directory.Exists(SAVE_PATH))
            {
                Directory.CreateDirectory(SAVE_PATH);
                Debug.Log($"Created save directory at: {SAVE_PATH}");
            }

            string resourcePath = Path.Combine(Application.dataPath, "Resources", SAVE_FOLDER);
            if (!Directory.Exists(resourcePath))
            {
                Directory.CreateDirectory(resourcePath);
                Debug.Log($"Created resource directory at: {resourcePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating directories: {e.Message}");
        }
    }

    protected override void CreateDefaultFiles()
    {
        var defaultStatData = Resources.Load<PlayerStatData>("DefaultPlayerStats");
        if (defaultStatData == null)
        {
            Debug.LogWarning("Default player stats not found, creating new...");
            defaultStatData = ScriptableObject.CreateInstance<PlayerStatData>();
        }
        currentPlayerStatData = Instantiate(defaultStatData);
        currentInventoryData = new InventoryData();
    }

    public override void SaveWithBackup()
    {
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
    }

    public override void ClearAllRuntimeData()
    {
        currentPlayerStatData = null;
        currentPlayerStatData = ScriptableObject.CreateInstance<PlayerStatData>();
        currentInventoryData = new InventoryData();
        currentLevelData = new LevelData { level = 1, exp = 0f };
        JSONIO<PlayerSaveData>.DeleteData(DEFAULT_SAVE_SLOT);
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
        currentPlayerStatData.SavePermanentStats();
    }

    public void SavePlayerData(string saveSlot, PlayerSaveData data)
    {
        if (!IsInitialized) Initialize();
        JSONIO<PlayerSaveData>.SaveData(saveSlot, data);
        SaveWithBackup();
    }

    public PlayerSaveData LoadPlayerData(string saveSlot)
    {
        if (!IsInitialized) Initialize();
        var data = JSONIO<PlayerSaveData>.LoadData(saveSlot);
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
            string json = JsonUtility.ToJson(data);
            string path = Path.Combine(Application.persistentDataPath, SAVE_PATH, "inventory.json");
            File.WriteAllText(path, json);
            Debug.Log($"Successfully saved inventory data to: {path}");
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

[System.Serializable]
public class LevelData
{
    public int level = 1;
    public float exp = 0f;
}

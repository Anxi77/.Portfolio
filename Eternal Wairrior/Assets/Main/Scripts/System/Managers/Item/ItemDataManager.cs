using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemDataManager : MonoBehaviour, IInitializable
{
    #region Singleton
    private static ItemDataManager instance;
    public static ItemDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemDataManager>();
                if (instance == null)
                {
                    var go = new GameObject("ItemDataManager");
                    instance = go.AddComponent<ItemDataManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Constants
    private const string ITEM_DB_PATH = "Items/Database";
    private const string DROP_TABLES_PATH = "Items/DropTables";
    #endregion

    #region Fields
    private Dictionary<string, ItemData> itemDatabase = new();
    private Dictionary<EnemyType, DropTableData> dropTables = new();
    private bool isInitialized;
    #endregion

    #region Properties
    public bool IsInitialized => isInitialized;
    #endregion

    #region Serialization Classes

    [Serializable]
    public class SerializableItemList
    {
        public List<ItemData> items = new();
    }

    [Serializable]
    public class SerializableDropTableEntry
    {
        public EnemyType enemyType;
        public float guaranteedDropRate;
        public int maxDrops;
        public List<DropTableEntry> dropEntries = new();
    }
    #endregion

    #region Initialization
    public void Initialize()
    {
        if (!isInitialized)
        {
            try
            {
                Debug.Log("Initializing ItemDataManager...");
                LoadAllData();
                isInitialized = true;
                Debug.Log("ItemDataManager initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing ItemDataManager: {e.Message}");
                isInitialized = false;
            }
        }
    }

    private void LoadAllData()
    {
        try
        {
            LoadItemDatabase();
            LoadDropTables();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
    }
    #endregion

    #region Data Loading
    private void LoadItemDatabase()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{ITEM_DB_PATH}/ItemDatabase");
            if (jsonAsset != null)
            {
                var serializableData = JsonUtility.FromJson<SerializableItemList>(jsonAsset.text);
                if (serializableData?.items != null)
                {
                    itemDatabase = serializableData.items.ToDictionary(item => item.ID);
                }
                else
                {
                    Debug.LogError("Failed to deserialize item data or items list is null");
                }
            }
            else
            {
                Debug.LogError($"ItemDatabase.json not found at path: Resources/{ITEM_DB_PATH}/ItemDatabase");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading item database: {e.Message}\n{e.StackTrace}");
        }
    }

    private void LoadDropTables()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{DROP_TABLES_PATH}/DropTables");
            if (jsonAsset != null)
            {
                var wrapper = JsonUtility.FromJson<DropTablesWrapper>(jsonAsset.text);
                dropTables = wrapper.dropTables.ToDictionary(dt => dt.enemyType);
            }
            else
            {
                Debug.LogError("No drop tables found.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading drop tables: {e.Message}");
        }
    }
    #endregion

    #region Data Access
    public List<ItemData> GetAllItemData()
    {
        return new List<ItemData>(itemDatabase.Values);
    }

    public ItemData GetItemData(string itemId)
    {
        if (itemDatabase.TryGetValue(itemId, out var itemData))
        {
            return itemData.Clone();
        }
        Debug.LogWarning($"Item not found: {itemId}");
        return null;
    }

    public bool HasItem(string itemId)
    {
        return itemDatabase.ContainsKey(itemId);
    }

    public Dictionary<string, ItemData> GetItemDatabase()
    {
        return new Dictionary<string, ItemData>(itemDatabase);
    }

    public Dictionary<EnemyType, DropTableData> GetDropTables()
    {
        return new Dictionary<EnemyType, DropTableData>(dropTables);
    }
    #endregion
}

[System.Serializable]
public class DropTablesWrapper
{
    public List<DropTableData> dropTables = new();
}

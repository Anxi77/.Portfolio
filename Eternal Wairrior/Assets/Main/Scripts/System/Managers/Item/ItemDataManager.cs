using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class ItemDataManager : DataManager<ItemDataManager>
{
    #region Constants
    private const string ITEM_DB_PATH = "Items/Database";
    private const string DROP_TABLES_PATH = "Items/DropTables";
    #endregion

    #region Fields
    public Dictionary<string, ItemData> itemDatabase = new();
    public Dictionary<EnemyType, DropTableData> dropTables = new();
    #endregion

    #region Data Loading
    protected override void LoadRuntimeData()
    {
        try
        {
            LoadItemDatabase();
            LoadDropTables();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
    }

    private void LoadItemDatabase()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{ITEM_DB_PATH}/ItemDatabase");
            if (jsonAsset != null)
            {
                var serializableData = JsonConvert.DeserializeObject<SerializableItemList>(
                    jsonAsset.text
                );
                if (serializableData?.items != null)
                {
                    itemDatabase = serializableData.items.ToDictionary(item => item.ID);
                    foreach (var item in itemDatabase)
                    {
                        Debug.Log(
                            $"[ItemDataManager] Loaded item [Name : {item.Value.Name}] [ID : {item.Value.ID}]"
                        );
                    }
                }
                else
                {
                    Debug.LogError(
                        "[ItemDataManager] Failed to deserialize item data or items list is null"
                    );
                }
                Debug.Log($"[ItemDataManager] Total loaded item data count : {itemDatabase.Count}");
            }
            else
            {
                Debug.LogError(
                    $"[ItemDataManager] ItemDatabase.json not found at path: Resources/{ITEM_DB_PATH}/ItemDatabase"
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[ItemDataManager] Error loading item database: {e.Message}\n{e.StackTrace}"
            );
        }
    }

    private void LoadDropTables()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{DROP_TABLES_PATH}/DropTables");
            if (jsonAsset != null)
            {
                var wrapper = JsonConvert.DeserializeObject<DropTablesWrapper>(jsonAsset.text);
                dropTables = wrapper.dropTables.ToDictionary(dt => dt.enemyType);
            }
            else
            {
                Debug.LogError("No drop tables found.");
            }
        }
        catch (Exception e)
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

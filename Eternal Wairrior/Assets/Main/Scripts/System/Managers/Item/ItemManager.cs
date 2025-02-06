using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class ItemManager : SingletonManager<ItemManager>, IInitializable
{
    [SerializeField] private GameObject worldDropItemPrefab;
    private ItemGenerator itemGenerator;
    private bool isInitialized;

    public bool IsInitialized => isInitialized;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void Initialize()
    {
        if (!IsInitialized)
        {
            try
            {
                while (ItemDataManager.Instance == null)
                {
                    Debug.Log("Waiting for ItemDataManager...");
                    while (!ItemDataManager.Instance.IsInitialized)
                    {
                        Debug.Log("Waiting for ItemDataManager to Load Datas...");
                    }
                }
                isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing ItemManager: {e.Message}\n{e.StackTrace}");
                isInitialized = false;
            }
        }
    }

    public void DropItem(ItemData itemData, Vector3 position)
    {
        if (itemData == null || worldDropItemPrefab == null) return;

        var worldDropItem = PoolManager.Instance.Spawn<WorldDropItem>(worldDropItemPrefab, position, Quaternion.identity);
        if (worldDropItem != null)
        {
            worldDropItem.Initialize(itemData);

            if (worldDropItem.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 smallRandomOffset = Random.insideUnitCircle * 0.3f;
                rb.AddForce(smallRandomOffset, ForceMode2D.Impulse);
            }
        }
    }

    public List<ItemData> GetDropsForEnemy(EnemyType enemyType, float luckMultiplier = 1f)
    {
        var dropTable = ItemDataManager.Instance.GetDropTables().GetValueOrDefault(enemyType);
        if (dropTable == null) return new List<ItemData>();
        return itemGenerator.GenerateDrops(dropTable, luckMultiplier);
    }

    public ItemData GetItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("Attempted to get item with null or empty ID");
            return null;
        }

        var item = itemGenerator.GenerateItem(itemId);
        if (item == null)
        {
            Debug.LogError($"Failed to generate item with ID: {itemId}");
            return null;
        }

        Debug.Log($"Generated item: {item.Name} with {item.Stats?.Count ?? 0} stats and {item.Effects?.Count ?? 0} effects");
        return item;
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        return ItemDataManager.Instance.GetAllItemData()
            .Where(item => item.Type == type)
            .Select(item => item.Clone())
            .ToList();
    }

    public List<ItemData> GetItemsByRarity(ItemRarity rarity)
    {
        return ItemDataManager.Instance.GetAllItemData()
            .Where(item => item.Rarity == rarity)
            .Select(item => item.Clone())
            .ToList();
    }
}

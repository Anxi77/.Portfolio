using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : SingletonManager<ItemManager>, IInitializable
{
    [SerializeField] private GameObject worldDropItemPrefab;
    private ItemDataManager itemDataManager;
    private ItemGenerator itemGenerator;
    private bool isInitialized;

    public bool IsInitialized => isInitialized;

    public void Initialize()
    {
        if (!IsInitialized)
        {
            try
            {
                Debug.Log("Initializing ItemManager...");
                InitializeManagers();
                isInitialized = true;
                Debug.Log("ItemManager initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing ItemManager: {e.Message}\n{e.StackTrace}");
                isInitialized = false;
            }
        }
    }

    private void InitializeManagers()
    {
        try
        {
            // ItemDataManager �ʱ�ȭ ���
            while (ItemDataManager.Instance == null)
            {
                Debug.Log("Waiting for ItemDataManager...");
                return;
            }

            itemDataManager = ItemDataManager.Instance;
            if (!itemDataManager.IsInitialized)
            {
                Debug.Log("Waiting for ItemDataManager to initialize...");
                return;
            }

            // ItemGenerator �ʱ�ȭ
            var itemDatabase = itemDataManager.GetAllItemData();
            if (itemDatabase != null && itemDatabase.Any())
            {
                itemGenerator = new ItemGenerator(itemDatabase.ToDictionary(item => item.id));
                Debug.Log("ItemGenerator initialized successfully");
            }
            else
            {
                Debug.LogWarning("Item database is empty or not loaded");
                itemGenerator = new ItemGenerator(new Dictionary<string, ItemData>());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in InitializeManagers: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    public void DropItem(ItemData itemData, Vector3 position)
    {
        if (itemData == null || worldDropItemPrefab == null) return;

        var worldDropItem = PoolManager.Instance.Spawn<WorldDropItem>(worldDropItemPrefab, position, Quaternion.identity);
        if (worldDropItem != null)
        {
            worldDropItem.Initialize(itemData);

            // ���� ȿ�� �����ϰų�, ���� ���� ���� �����¸� �߰�
            if (worldDropItem.TryGetComponent<Rigidbody2D>(out var rb))
            {
                // ���� ���� ���� ������ ���� (���û���)
                Vector2 smallRandomOffset = Random.insideUnitCircle * 0.3f;
                rb.AddForce(smallRandomOffset, ForceMode2D.Impulse);
            }
        }
    }

    public List<ItemData> GetDropsForEnemy(EnemyType enemyType, float luckMultiplier = 1f)
    {
        var dropTable = itemDataManager.GetDropTables().GetValueOrDefault(enemyType);
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

        // ������ ����
        var item = itemGenerator.GenerateItem(itemId);
        if (item == null)
        {
            Debug.LogError($"Failed to generate item with ID: {itemId}");
            return null;
        }

        Debug.Log($"Generated item: {item.name} with {item.stats?.Count ?? 0} stats and {item.effects?.Count ?? 0} effects");
        return item;
    }

    public List<ItemData> GetItemsByType(ItemType type)
    {
        return itemDataManager.GetAllItemData()
            .Where(item => item.type == type)
            .Select(item => item.Clone())
            .ToList();
    }

    public List<ItemData> GetItemsByRarity(ItemRarity rarity)
    {
        return itemDataManager.GetAllItemData()
            .Where(item => item.rarity == rarity)
            .Select(item => item.Clone())
            .ToList();
    }
}

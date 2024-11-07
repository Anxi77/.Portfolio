using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    private Dictionary<string, ItemData> availableItems = new();
    private Dictionary<string, Item> activeItems = new();
    private Dictionary<EnemyType, DropTableData> dropTables = new();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        LoadItemData();
        LoadDropTables();
    }

    private void LoadItemData()
    {
        var items = ItemDataManager.Instance.GetAllItemData();
        foreach (var item in items)
        {
            availableItems[item.id] = item;
        }
    }

    public void LoadDropTables()
    {
        var tables = ItemDataManager.Instance.GetDropTables();
        foreach (var kvp in tables)
        {
            dropTables[kvp.Key] = kvp.Value;
        }
    }

    public void EquipItem(string itemId, EquipmentSlot slot)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            var player = GameManager.Instance?.player;
            if (player != null)
            {
                var playerStat = player.GetComponent<PlayerStat>();
                playerStat.EquipItem(itemData.stats, slot);
            }
        }
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            var playerStat = player.GetComponent<PlayerStat>();
            playerStat.UnequipItem(slot);
        }
    }

    public List<ItemData> GetRandomItems(int count, ItemType type = ItemType.None)
    {
        var filteredItems = availableItems.Values
            .Where(item => type == ItemType.None || item.type == type)
            .ToList();

        var result = new List<ItemData>();
        while (result.Count < count && filteredItems.Any())
        {
            int index = UnityEngine.Random.Range(0, filteredItems.Count);
            result.Add(filteredItems[index].Clone());
            filteredItems.RemoveAt(index);
        }

        return result;
    }

    public ItemData GetItem(string itemId)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return itemData.Clone();
        }
        return null;
    }

    // ������ ��� ����
    public void DropItem(ItemData itemData, Vector3 position)
    {
        if (itemData?.prefab == null) return;

        var droppedItem = PoolManager.Instance.Spawn<Item>(
            itemData.prefab,
            position + Random.insideUnitSphere * 1f,
            Quaternion.identity
        );
        droppedItem.Initialize(itemData);
    }

    public void DropItems(List<ItemData> items, Vector3 position, float spreadRadius = 1f)
    {
        foreach (var item in items)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spreadRadius;
            randomOffset.z = 0;  // Y���� ����
            DropItem(item, position + randomOffset);
        }
    }

    // ������ ���͸�/�˻�
    public List<ItemData> GetItemsByType(ItemType type)
    {
        return availableItems.Values
            .Where(item => item.type == type)
            .Select(item => item.Clone())
            .ToList();
    }

    public List<ItemData> GetItemsByRarity(ItemRarity rarity)
    {
        return availableItems.Values
            .Where(item => item.rarity == rarity)
            .Select(item => item.Clone())
            .ToList();
    }

    // ������ ���� ����
    public bool CanStackMore(string itemId, int currentAmount)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return currentAmount < itemData.maxStack;
        }
        return false;
    }

    public int GetRemainingStackSpace(string itemId, int currentAmount)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return itemData.maxStack - currentAmount;
        }
        return 0;
    }

    // ������ ��/����
    public float CompareItems(ItemData item1, ItemData item2)
    {
        float GetItemScore(ItemData item)
        {
            float score = 0;
            foreach (var stat in item.stats)
            {
                score += CalculateStatValue(stat);
            }
            return score * GetRarityMultiplier(item.rarity);
        }

        return GetItemScore(item2) - GetItemScore(item1);
    }

    private float CalculateStatValue(StatContainer stat)
    {
        // �� ������ ����ġ ���
        return stat.statType switch
        {
            StatType.Damage => stat.amount * 2f,
            StatType.Defense => stat.amount * 1.5f,
            StatType.MaxHp => stat.amount * 1f,
            _ => stat.amount
        };
    }

    private float GetRarityMultiplier(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 1f,
            ItemRarity.Uncommon => 1.2f,
            ItemRarity.Rare => 1.5f,
            ItemRarity.Epic => 2f,
            ItemRarity.Legendary => 3f,
            _ => 1f
        };
    }

    // ������ ��ȭ/���׷��̵� (�ʿ��� ���)
    public bool TryUpgradeItem(ItemData item)
    {
        // ������ ��ȭ ����
        return false;
    }

    // �����/�׽�Ʈ��
    public void SpawnRandomItems(Vector3 position, int count, ItemType type = ItemType.None)
    {
        var items = GetRandomItems(count, type);
        DropItems(items, position);
    }

    public List<ItemData> GetDropsForEnemy(EnemyType enemyType, float luckMultiplier = 1f)
    {
        if (dropTables.TryGetValue(enemyType, out var dropTable))
        {
            var drops = new List<ItemData>();
            int dropCount = 0;

            // ����� ��� Ȯ�� üũ
            if (Random.value < dropTable.guaranteedDropRate * luckMultiplier)
            {
                var guaranteedDrop = GetRandomDropFromTable(dropTable, luckMultiplier);
                if (guaranteedDrop != null)
                {
                    drops.Add(guaranteedDrop);
                    dropCount++;
                }
            }

            // �߰� ��� �õ�
            foreach (var entry in dropTable.dropEntries)
            {
                if (dropCount >= dropTable.maxDrops) break;

                if (Random.value < entry.dropRate * luckMultiplier)
                {
                    if (availableItems.TryGetValue(entry.itemId, out var itemData))
                    {
                        var drop = itemData.Clone();
                        drop.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                        drops.Add(drop);
                        dropCount++;
                    }
                }
            }

            return drops;
        }

        return new List<ItemData>();
    }

    private ItemData GetRandomDropFromTable(DropTableData dropTable, float luckMultiplier)
    {
        float totalWeight = 0f;
        foreach (var entry in dropTable.dropEntries)
        {
            totalWeight += entry.dropRate * luckMultiplier;
        }

        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var entry in dropTable.dropEntries)
        {
            currentWeight += entry.dropRate * luckMultiplier;
            if (random <= currentWeight)
            {
                if (availableItems.TryGetValue(entry.itemId, out var itemData))
                {
                    var drop = itemData.Clone();
                    drop.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    return drop;
                }
            }
        }

        return null;
    }

    public List<GameObject> GetAllItemPrefabs()
    {
        return availableItems.Values
            .Where(item => item.prefab != null)
            .Select(item => item.prefab)
            .Distinct()
            .ToList();
    }

    public void SaveItemState()
    {
        ItemDataManager.Instance.SaveItemDatabase();
    }

    public void RestoreItemState()
    {
        LoadItemData();
        LoadDropTables();
    }
}
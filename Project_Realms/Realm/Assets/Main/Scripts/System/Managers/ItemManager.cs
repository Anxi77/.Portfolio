using System.Collections.Generic;
using UnityEngine;
using static ItemGenerationRuleSO;
using System.Collections;

public class ItemManager : SingletonManager<ItemManager>
{
    [SerializeField] private ItemGenerationRuleSO itemGenerationRules;
    [SerializeField] private List<ItemData> defaultPlayerItems;

    private List<Item> items = new List<Item>();
    private Dictionary<Item, ItemInstanceData> itemInstanceData = new Dictionary<Item, ItemInstanceData>();

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        StartCoroutine(InitializeRoutine());
    }

    public IEnumerator InitializeRoutine()
    {
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized && UIManager.instance != null && UIManager.Instance.IsInitialized);
        GiveDefaultItemsToPlayer();
    }

    public List<Item> GenerateRandomItems(MonsterType monsterType, Vector3 position)
    {
        var dropRule = itemGenerationRules.GetDropRuleForMonster(monsterType);
        if (dropRule == null) return new List<Item>();

        if (UnityEngine.Random.Range(0f, 100f) > dropRule.itemDropChance)
            return new List<Item>();

        List<Item> droppedItems = new List<Item>();
        Vector3 dropPosition = position;

        foreach (var itemRule in dropRule.possibleItems)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= itemRule.baseDropChance)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                Vector3 itemPosition = dropPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);

                Item newItem = GenerateItem(itemRule, itemPosition);
                if (newItem != null)
                {
                    droppedItems.Add(newItem);
                }
            }
        }

        return droppedItems;
    }

    private List<Item> worldDroppedItems = new List<Item>();

    private Item GenerateItem(ItemGenerationRule itemRule, Vector3 position)
    {
        if (itemRule == null || itemRule.itemTemplate == null) return null;

        GameObject itemObj = Instantiate(itemRule.itemTemplate.WorldDropPrefab, position, Quaternion.identity);
        Item item = itemObj.GetComponent<Item>();
        if (item == null) return null;

        var instanceData = new ItemInstanceData(itemRule.itemTemplate);
        instanceData.SetRarity(itemRule.itemTemplate.Rarity,
            itemGenerationRules.GetColorForRarity(itemRule.itemTemplate.Rarity));

        GenerateRandomStats(instanceData, itemRule);

        itemInstanceData[item] = instanceData;
        item.Initialize(itemRule.itemTemplate, instanceData);
        items.Add(item);
        worldDroppedItems.Add(item);

        return item;
    }

    private void GenerateRandomStats(ItemInstanceData instanceData, ItemGenerationRule itemRule)
    {
        var selectedStats = new List<StatGenerationRule>();

        foreach (var statRule in itemRule.possibleStats)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= statRule.generationChance)
                selectedStats.Add(statRule);
        }

        int statCount = Mathf.Min(itemRule.maxStatCount, selectedStats.Count);
        if (statCount <= 0) return;

        ShuffleList(selectedStats);

        for (int i = 0; i < statCount; i++)
        {
            var statRule = selectedStats[i];

            float flatValue = 0f;
            float percentValue = 0f;

            if (statRule.flatValueRange.y > 0)
            {
                flatValue = UnityEngine.Random.Range(statRule.flatValueRange.x, statRule.flatValueRange.y);
            }

            if (statRule.usePercentValue)
            {
                percentValue = UnityEngine.Random.Range(statRule.percentValueRange.x, statRule.percentValueRange.y);
            }

            instanceData.AddStat(statRule.statType, flatValue, percentValue);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void RemoveItem(Item item)
    {
        if (itemInstanceData.ContainsKey(item))
        {
            itemInstanceData.Remove(item);
        }
        items.Remove(item);
    }

    public ItemInstanceData GetItemInstanceData(Item item)
    {
        if (itemInstanceData.TryGetValue(item, out ItemInstanceData instance))
        {
            return instance;
        }
        return null;
    }

    public void GiveDefaultItemsToPlayer()
    {
        if (defaultPlayerItems == null || defaultPlayerItems.Count == 0)
        {
            Debug.LogWarning("기본 아이템이 없습니다.");
            return;
        }

        var player = GameManager.Instance.player;
        var playerInventory = player.InventorySystem;
        if (playerInventory == null)
        {
            Debug.LogError("플레이어의 인벤토리를 찾을 수 없습니다.");
            return;
        }

        var playerUI = UIManager.instance.PlayerUI;
        if (playerUI == null || playerUI.inventoryUI == null)
        {
            Debug.LogError("InventoryUI를 찾을 수 없습니다.");
            return;
        }

        foreach (var itemTemplate in defaultPlayerItems)
        {
            Item item = CreateItem(itemTemplate, ItemRarity.Common, Color.white);
            if (item != null)
            {
                playerUI.inventoryUI.TryAddItem(item);
            }
        }
    }

    public void ClearAllDroppedItems()
    {
        foreach (var item in worldDroppedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
                RemoveItem(item);
            }
        }
        worldDroppedItems.Clear();
    }




    private Item CreateItem(ItemData template, ItemRarity rarity, Color nameColor)
    {
        GameObject itemObj = new GameObject(template.ItemID.ToString());
        itemObj.transform.SetParent(GameManager.instance.player.transform);
        Item item = itemObj.AddComponent<Item>();

        var instanceData = new ItemInstanceData(template);
        instanceData.SetRarity(rarity, nameColor);

        foreach (var stat in template.Stats)
        {
            instanceData.AddStat(stat.statType, stat.flatValue, stat.percentValue);
        }

        itemInstanceData[item] = instanceData;
        item.Initialize(template, instanceData);
        items.Add(item);

        itemObj.SetActive(false);

        return item;
    }
}

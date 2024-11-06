using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>
{
    private PlayerStat playerStat;
    private Inventory inventory;
    private Dictionary<SourceType, List<StatContainer>> temporaryEffects = new();
    private Dictionary<SourceType, List<StatContainer>> temporaryEffectsBackup;

    public void Initialize(Player player)
    {
        if (player == null) return;

        playerStat = player.GetComponent<PlayerStat>();
        inventory = player.GetComponent<Inventory>();

        ClearTemporaryEffects();
    }

    public void InitializeTestPlayer()
    {
        if (playerStat != null)
        {
            // �׽�Ʈ�� �⺻ ���� ����
            playerStat.ResetToBase();
            playerStat.level = 10;  // �׽�Ʈ�� ����
            playerStat.currentExp = 0;

            // �׽�Ʈ�� ��ȭ�� ����
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Base, IncreaseType.Add, 500f);
            playerStat.AddStatModifier(StatType.Damage, SourceType.Base, IncreaseType.Add, 50f);
            playerStat.AddStatModifier(StatType.Defense, SourceType.Base, IncreaseType.Add, 20f);
            playerStat.AddStatModifier(StatType.MoveSpeed, SourceType.Base, IncreaseType.Add, 2f);
            playerStat.AddStatModifier(StatType.AttackSpeed, SourceType.Base, IncreaseType.Add, 1f);
            playerStat.AddStatModifier(StatType.ExpCollectionRadius, SourceType.Base, IncreaseType.Add, 3f);

            playerStat.RestoreFullHealth();
        }

        // �׽�Ʈ�� ������ ����
        if (inventory != null)
        {
            var testItems = GetTestItems();
            foreach (var item in testItems)
            {
                inventory.AddItem(item);
            }
        }
    }

    private List<ItemData> GetTestItems()
    {
        return new List<ItemData>
        {
            ItemManager.Instance.GetItem("test_sword"),
            ItemManager.Instance.GetItem("test_armor"),
            ItemManager.Instance.GetItem("test_accessory")
        };
    }

    public void InitializeNewPlayer()
    {
        // �� �÷��̾� �⺻ ����
        if (playerStat != null)
        {
            playerStat.ResetToBase();
            playerStat.level = 1;
            playerStat.currentExp = 0;
            playerStat.RestoreFullHealth();
        }

        // �⺻ ������ ����
        if (inventory != null)
        {
            var startingItems = GetStartingItems();
            foreach (var item in startingItems)
            {
                inventory.AddItem(item);
            }
        }
    }

    private List<ItemData> GetStartingItems()
    {
        return new List<ItemData>
        {
            ItemManager.Instance.GetItem("default_sword"),
            ItemManager.Instance.GetItem("basic_armor")
        };
    }

    // ��Ÿ�� ������ ����
    public void SaveRuntimeData()
    {
        SavePlayerState();
        ClearTemporaryEffects(); // �ӽ� ȿ���� �������� ����

        // PlayerDataManager�� ���� ���� ���� ������ ����
        if (playerStat != null)
        {
            GameManager.Instance.playerDataManager.SaveCurrentPlayerStatData();
        }
    }

    public void LoadRuntimeData()
    {
        // PlayerDataManager�κ��� ��� ������ �ε�
        if (playerStat != null)
        {
            // ���� ������ �ε�
            var statData = GameManager.Instance.playerDataManager.CurrentPlayerStatData;
            playerStat.LoadStats(statData);

            // ���� ������ �ε�
            var (level, exp) = GameManager.Instance.playerDataManager.LoadLevelData();
            playerStat.level = level;
            playerStat.currentExp = exp;
        }

        // �κ��丮 ������ �ε�
        if (inventory != null)
        {
            var inventoryData = GameManager.Instance.playerDataManager.LoadInventoryData();
            inventory.LoadInventoryData(inventoryData);
        }

        // ��Ÿ�� ���� �ʱ�ȭ
        ClearTemporaryEffects();
        RestorePlayerState();
    }

    // ��Ÿ�� ���� ����
    public void AddTemporaryEffect(StatContainer effect, float duration = 0f)
    {
        if (!temporaryEffects.ContainsKey(effect.buffType))
        {
            temporaryEffects[effect.buffType] = new List<StatContainer>();
        }

        temporaryEffects[effect.buffType].Add(effect);
        playerStat?.AddStatModifier(effect.statType, effect.buffType, effect.incType, effect.amount);

        if (duration > 0)
        {
            StartCoroutine(RemoveEffectAfterDelay(effect, duration));
        }
    }

    public void RemoveTemporaryEffect(StatContainer effect)
    {
        if (temporaryEffects.TryGetValue(effect.buffType, out var effects))
        {
            effects.Remove(effect);
            playerStat?.RemoveStatModifier(effect.statType, effect.buffType, effect.incType, effect.amount);
        }
    }

    public void ClearTemporaryEffects()
    {
        foreach (var effects in temporaryEffects.Values)
        {
            foreach (var effect in effects)
            {
                playerStat?.RemoveStatModifier(
                    effect.statType,
                    effect.buffType,
                    effect.incType,
                    effect.amount
                );
            }
        }
        temporaryEffects.Clear();
    }

    // �������� ��ȯ ����
    public void SavePlayerState()
    {
        playerStat?.SaveCurrentState();
        inventory?.SaveInventoryState();
    }

    public void RestorePlayerState()
    {
        playerStat?.RestoreState();
        inventory?.RestoreInventoryState();
    }

    // ������ ó��
    public void HandleLevelUp()
    {
        if (playerStat != null)
        {
            // ������ �� ���� ����
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Level, IncreaseType.Add, 10f);
            playerStat.AddStatModifier(StatType.Damage, SourceType.Level, IncreaseType.Add, 2f);
            playerStat.AddStatModifier(StatType.Defense, SourceType.Level, IncreaseType.Add, 1f);

            // HP ȸ��
            playerStat.RestoreFullHealth();
        }
    }

    // ������ ����
    public void EquipItem(ItemData itemData, EquipmentSlot slot)
    {
        inventory?.EquipItem(itemData, slot);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        inventory?.UnequipFromSlot(slot);
    }

    public void AddItem(ItemData itemData)
    {
        inventory?.AddItem(itemData);
    }

    // ���� üũ
    public bool IsAlive()
    {
        return playerStat != null && playerStat.currentHp > 0;
    }

    public float GetCurrentHpRatio()
    {
        if (playerStat == null) return 0f;
        return playerStat.currentHp / playerStat.GetFinalStatValue(StatType.MaxHp);
    }

    // ���� ����/�ε� ����
    public void SavePlayerData()
    {
        if (playerStat != null)
        {
            // �ӽ� ȿ�� �����ϰ� �������� �����͸� ����
            ClearTemporaryEffects();

            // ���� ���� ������ ����
            GameManager.Instance.playerDataManager.SaveCurrentPlayerStatData();

            // �κ��丮 ������ ����
            if (inventory != null)
            {
                var inventoryData = inventory.GetInventoryData();
                GameManager.Instance.playerDataManager.SaveInventoryData(inventoryData);
            }

            // ������ ����ġ ����
            GameManager.Instance.playerDataManager.SaveLevelData(
                playerStat.level,
                playerStat.currentExp
            );
        }
    }

    public void LoadPlayerData()
    {
        // ���� ������ �ε�
        var statData = GameManager.Instance.playerDataManager.CurrentPlayerStatData;
        if (statData != null && playerStat != null)
        {
            playerStat.LoadStats(statData);
        }

        // �κ��丮 ������ �ε�
        var inventoryData = GameManager.Instance.playerDataManager.LoadInventoryData();
        if (inventoryData != null && inventory != null)
        {
            inventory.LoadInventoryData(inventoryData);
        }

        // ���� ������ �ε�
        var (level, exp) = GameManager.Instance.playerDataManager.LoadLevelData();
        if (playerStat != null)
        {
            playerStat.level = level;
            playerStat.currentExp = exp;
        }
    }

    private IEnumerator RemoveEffectAfterDelay(StatContainer effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveTemporaryEffect(effect);
    }

    // �������� ��ȯ�� ���� �ӽ� ���� ����
    public void SaveGameState()
    {
        SavePlayerState();        // ���� ���� ���� ����
        SaveTemporaryEffects();   // ���� �������� ����/����� ����
    }

    public void LoadGameState()
    {
        RestorePlayerState();     // ����� ���� ���� ����
        RestoreTemporaryEffects(); // ����� ����/����� ����
    }

    private void SaveTemporaryEffects()
    {
        // ���� �������� �ӽ� ȿ���� ����
        temporaryEffectsBackup = new Dictionary<SourceType, List<StatContainer>>(temporaryEffects);
    }

    private void RestoreTemporaryEffects()
    {
        // ����� �ӽ� ȿ���� ����
        if (temporaryEffectsBackup != null)
        {
            foreach (var kvp in temporaryEffectsBackup)
            {
                foreach (var effect in kvp.Value)
                {
                    AddTemporaryEffect(effect);
                }
            }
        }
    }
}
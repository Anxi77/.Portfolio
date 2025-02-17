using System;
using System.Collections.Generic;
[Serializable]
public class PlayerData
{
    public PlayerStatData stats;
    public InventoryData inventory;
    public LevelData levelData;
}

[Serializable]
public class LevelData
{
    public int level = 1;
    public float exp = 0f;
}

[Serializable]
public class PlayerStatData
{
    public Dictionary<StatType, float> baseStats = new();
    public List<StatModifierSaveData> permanentModifiers = new();

    [Serializable]
    public class StatModifierSaveData
    {
        public StatType statType;
        public SourceType sourceType;
        public IncreaseType increaseType;
        public float amount;

        public StatModifierSaveData(StatType statType, SourceType sourceType, IncreaseType increaseType, float amount)
        {
            this.statType = statType;
            this.sourceType = sourceType;
            this.increaseType = increaseType;
            this.amount = amount;
        }
    }

    public PlayerStatData()
    {
        InitializeDefaultStats();
    }

    private void InitializeDefaultStats()
    {
        baseStats[StatType.MaxHp] = 100f;
        baseStats[StatType.Damage] = 5f;
        baseStats[StatType.Defense] = 2f;
        baseStats[StatType.MoveSpeed] = 5f;
        baseStats[StatType.AttackSpeed] = 1f;
        baseStats[StatType.AttackRange] = 2f;
        baseStats[StatType.ExpCollectionRadius] = 3f;
        baseStats[StatType.HpRegenRate] = 1f;
    }
}

[Serializable]
public class InventoryData
{
    public List<InventorySlot> slots = new();
    public int gold;
    public Dictionary<EquipmentSlot, string> equippedItems = new();
}

[Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;
    public bool isEquipped;
}
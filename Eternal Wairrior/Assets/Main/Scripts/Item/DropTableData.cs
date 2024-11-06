using System.Collections.Generic;

[System.Serializable]
public class DropTableData
{
    public EnemyType enemyType;
    public List<DropTableEntry> dropEntries = new();
    public float guaranteedDropRate = 0.1f; // �ּ� ��� Ȯ��
    public int maxDrops = 3; // �ִ� ��� ����
}

[System.Serializable]
public class DropTableEntry
{
    public string itemId;
    public float dropRate;
    public ItemRarity rarity;
    public int minAmount = 1;
    public int maxAmount = 1;
}
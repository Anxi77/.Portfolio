[System.Serializable]
public class AreaSkillStat : ISkillStat
{
    public BaseSkillStat baseStat { get; set; }

    // �⺻ ���� �Ӽ�
    public float radius { get; set; }
    public float tickRate { get; set; }
    public float moveSpeed { get; set; }

    // ���� ȿ�� ���� �Ӽ�
    public AreaPersistenceData persistenceData { get; set; }

    public AreaSkillStat()
    {
        baseStat = new BaseSkillStat();
        persistenceData = new AreaPersistenceData();
    }
}

[System.Serializable]
public class AreaPersistenceData
{
    public bool isPersistent { get; set; }
    public float duration { get; set; }
    public float effectInterval { get; set; }
}
// ���� �������̽�
public interface ISkillStat
{
    BaseSkillStat baseStat { get; set; }
}

// ��� ��ų�� ���������� ������ �⺻ ����
[System.Serializable]
public struct BaseSkillStat
{
    public float damage;
    public string skillName;
    public int skillLevel;
    public int maxSkillLevel;
    public ElementType element;        // ��ų �Ӽ�
    public float elementalPower;       // �Ӽ� ȿ�� ���
}

// �߻�ü ��ų ���� ����
[System.Serializable]
public struct ProjectileSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float projectileSpeed;
    public float projectileScale;
    public float shotInterval;
    public int pierceCount;
    public float attackRange;
    public float homingRange;
    public bool isHoming;
    public float explosionRad;
    public int projectileCount;
    public float innerInterval;
}

// ���� ��ų ���� ����
[System.Serializable]
public struct AreaSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;
}

// �нú� ��ų ���� ����
[System.Serializable]
public struct PassiveSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float effectDuration;
    public float cooldown;
    public float triggerChance;
}
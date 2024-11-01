public class ProjectileStats
{
    // �⺻ ����
    public float damage { get; set; }
    public float moveSpeed { get; set; }
    public float scale { get; set; }
    public ElementType elementType { get; set; }
    public float elementalPower { get; set; }

    // ����ü ���� ����
    public int pierceCount { get; set; }
    public float maxTravelDistance { get; set; }

    // ���� ȿ�� ����
    public ProjectilePersistenceData persistenceData { get; set; }

    public ProjectileStats()
    {
        persistenceData = new ProjectilePersistenceData();
    }
}
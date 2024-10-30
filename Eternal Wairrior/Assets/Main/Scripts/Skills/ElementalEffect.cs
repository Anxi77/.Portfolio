using UnityEngine;

public static class ElementalEffects
{
    public static void ApplyElementalEffect(ElementType element, float elementalPower, GameObject target)
    {
        switch (element)
        {
            case ElementType.Dark:
                ApplyDarkEffect(elementalPower, target);
                break;
            case ElementType.Water:
                ApplyWaterEffect(elementalPower, target);
                break;
            case ElementType.Fire:
                ApplyFireEffect(elementalPower, target);
                break;
            case ElementType.Earth:
                ApplyEarthEffect(elementalPower, target);
                break;
        }
    }

    private static void ApplyDarkEffect(float power, GameObject target)
    {
        // ��� �Ӽ� ȿ��: ����� ���� ����
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyDefenseDebuff(power, 5f); // 5�ʰ� ���� ����
        }
    }

    private static void ApplyWaterEffect(float power, GameObject target)
    {
        // �� �Ӽ� ȿ��: ����� �̵��ӵ� ����
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplySlowEffect(power, 3f); // 3�ʰ� �̵��ӵ� ����
        }
    }

    private static void ApplyFireEffect(float power, GameObject target)
    {
        // �� �Ӽ� ȿ��: ���� ������
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyDotDamage(power, 0.5f, 3f); // 3�ʰ� 0.5�ʸ��� ������
        }
    }

    private static void ApplyEarthEffect(float power, GameObject target)
    {
        // ���� �Ӽ� ȿ��: ����
        if (target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.ApplyStun(power, 2f); // 2�ʰ� ����
        }
    }
}
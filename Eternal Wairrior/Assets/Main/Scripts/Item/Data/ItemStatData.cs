using System.Collections.Generic;

[System.Serializable]
public class ItemStatData
{
    public string itemId;
    public string name;
    public ItemType type;
    public ItemRarity rarity;
    public ElementType element;
    public float dropRate;

    // �⺻ ����
    public float damage;
    public float defense;
    public float hp;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;
    public float hpRegen;

    // Ư�� ����
    public float criticalChance;
    public float criticalDamage;
    public float lifeSteal;
    public float reflectDamage;
    public float dodgeChance;

    public List<StatContainer> ConvertToStatContainers()
    {
        var containers = new List<StatContainer>();

        // �⺻ ����
        if (damage > 0) containers.Add(new StatContainer(StatType.Damage, SourceType.Equipment_Weapon, IncreaseType.Add, damage));
        if (defense > 0) containers.Add(new StatContainer(StatType.Defense, SourceType.Equipment_Armor, IncreaseType.Add, defense));
        if (hp > 0) containers.Add(new StatContainer(StatType.MaxHp, SourceType.Equipment_Armor, IncreaseType.Add, hp));
        if (moveSpeed > 0) containers.Add(new StatContainer(StatType.MoveSpeed, SourceType.Equipment_Accessory, IncreaseType.Mul, moveSpeed));
        if (attackSpeed > 0) containers.Add(new StatContainer(StatType.AttackSpeed, SourceType.Equipment_Weapon, IncreaseType.Mul, attackSpeed));
        if (attackRange > 0) containers.Add(new StatContainer(StatType.AttackRange, SourceType.Equipment_Weapon, IncreaseType.Mul, attackRange));
        if (hpRegen > 0) containers.Add(new StatContainer(StatType.HpRegenRate, SourceType.Equipment_Accessory, IncreaseType.Add, hpRegen));

        // Ư�� ����
        if (criticalChance > 0) containers.Add(new StatContainer(StatType.CriticalChance, SourceType.Equipment_Weapon, IncreaseType.Add, criticalChance));
        if (criticalDamage > 0) containers.Add(new StatContainer(StatType.CriticalDamage, SourceType.Equipment_Weapon, IncreaseType.Add, criticalDamage));
        if (lifeSteal > 0) containers.Add(new StatContainer(StatType.LifeSteal, SourceType.Equipment_Weapon, IncreaseType.Add, lifeSteal));
        if (reflectDamage > 0) containers.Add(new StatContainer(StatType.ReflectDamage, SourceType.Equipment_Armor, IncreaseType.Add, reflectDamage));
        if (dodgeChance > 0) containers.Add(new StatContainer(StatType.DodgeChance, SourceType.Equipment_Accessory, IncreaseType.Add, dodgeChance));

        return containers;
    }
}

using UnityEngine;

public enum StatType
{
    Level,
    MaxHealth,
    Health,
    Mana,
    MaxMana,
    ManaRegenRate,
    HealthRegenRate,
    Damage,
    Defense,
    MoveSpeed,
    AttackSpeed,
    AttackRange,
    ExpRange,
    DropExp
}

public enum SkillID
{
    BasicSwordAttack,
    BasicBowAttack,
    MonsterSkill,
    DragonBreath,
    DragonBite,
    DragonNova,
    TestSkill,
    FireBall,
    FireRain,
    MagicChain,
    Dash,
    Nuclear,
    ThunderStroke,
    IceFatalWheel,
    BattleOrders,
    BattleCry,
    Haste,
    ManaRegen,
    HpRegen,
    PouPou



}
public enum SkillStatType
{
    #region Common Skill Stats

    SkillLevel,
    ManaCost,
    Damage,
    Duration,
    Cooldown,
    HomingLevel,

    #endregion

    #region Projectile Related Skills

    ProjectileSpeed,
    ProjectileRange,
    ProjectileScale,
    ProjectileCount,
    ExplosionRadius,
    IsHoming,
    HomingRange,
    PierceCount,
    ShotInterval,
    InnerInterval,

    #endregion

    #region Area Related Stats

    SpawnRange,
    SpawnScale,
    SpawnCount,
    SpawnInterval,
    IsSpawnAtCursor,
    IsSpawnAtEnemy,

    #endregion

    #region Buff Related Stats

    BuffValue,
    DeBuffAreaScale,

    #endregion

    DashDistance

}

public enum SourceType
{
    None,
    BaseStats,
    Level,
    Equipment,
    Skill,
    Buff,
    Debuff,
}


public enum StatModifierType
{
    Flat,
    PercentAdd,
    PercentMult
}

public enum ItemID
{
    Sword,
    Bow,
    Arrow,
    Ring,
    OldArmor,
    IronArmor,
    KnightArmor,
    WandererArmor,
    NimbleArmor,
    PrideOfArcher,
    CallingOfTemplar,
    TemplarArmor,
    HermesArmor,
    
}

public enum ItemType
{
    None,
    Weapon,
    Sword,
    Bow,
    Armor,
    Accessory,
    Pet,
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum MonsterType
{
    Normal,
    Elite,
    Boss,
    MiniBoss,
    Unique
}
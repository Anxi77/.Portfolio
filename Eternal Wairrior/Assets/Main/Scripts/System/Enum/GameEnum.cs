using System;

#region Skill

public enum SkillType
{
    None = 0,
    Projectile,
    Area,
    Passive
}

public enum SkillID
{
    None = 100000,
    //Earth
    Vine,        // Area
    EarthRift,   // Projectile
    GaiasGrace,  // Passive
    //Water
    FrostTide,   // Area
    FrostHunt,   // Projectile
    TidalEssence,// Passive
    //Dark
    ShadowWaltz, // Area
    EventHorizon, // Projectile
    AbyssalExpansion, // Passive
    //Fire
    Flame, // Projectile
    FireRing, // Area
    ThermalElevation // Passive
}

#endregion

#region Item

[Serializable]
public enum EffectType
{
    None,
    DamageBonus,
    CooldownReduction,
    ProjectileSpeed,
    ProjectileRange,
    HomingEffect,
    AreaRadius,
    AreaDuration,
    ElementalPower
}

[Serializable]
public enum ItemType
{
    None,
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Material
}

[Serializable]
public enum AccessoryType
{
    None,
    Necklace,
    Ring
}

[Serializable]
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
}

#endregion

#region Player

[Serializable]
public enum StatType
{
    None,
    MaxHp,
    CurrentHp,
    Damage,
    Defense,
    MoveSpeed,
    AttackSpeed,
    AttackRange,
    AttackRadius,
    ExpCollectionRadius,
    HpRegenRate,
    ExpGainRate,
    GoldGainRate,
    CriticalChance,
    CriticalDamage,
    FireResistance,
    IceResistance,
    LightningResistance,
    PoisonResistance,
    StunResistance,
    SlowResistance,
    Luck,
    DodgeChance,
    ReflectDamage,
    LifeSteal,
}

[Serializable]
public enum SourceType
{
    None,
    Base,
    Level,
    Passive,
    Active,
    Weapon,
    Armor,
    Accessory,
    Special,
    Consumable,
    Buff,
    Debuff
}

[Serializable]
public enum IncreaseType
{
    Flat,
    Multiply
}

#endregion


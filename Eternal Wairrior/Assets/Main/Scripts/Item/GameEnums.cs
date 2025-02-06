using System;

[Serializable]
public enum ElementType
{
    None = 0,
    Dark,    // Reduces target's defense
    Water,   // Slows target's movement
    Fire,    // Deals damage over time
    Earth    // Can stun targets
}

[Serializable]
public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Necklace,
    Ring1,
    Ring2,
    Special
}


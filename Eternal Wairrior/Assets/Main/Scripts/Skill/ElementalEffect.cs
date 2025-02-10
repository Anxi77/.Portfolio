using UnityEngine;

public static class ElementalEffects
{
    private const float DARK_EFFECT_DURATION = 5f;
    private const float WATER_EFFECT_DURATION = 3f;
    private const float FIRE_EFFECT_DURATION = 3f;
    private const float FIRE_TICK_RATE = 0.5f;
    private const float EARTH_EFFECT_DURATION = 2f;

    public static void ApplyElementalEffect(ElementType element, float elementalPower, GameObject target)
    {
        if (target == null || elementalPower <= 0) return;

        if (!target.TryGetComponent<Enemy>(out Enemy enemy))
        {
            Debug.LogWarning($"Failed to apply elemental effect: Target {target.name} is not an enemy");
            return;
        }

        switch (element)
        {
            case ElementType.Dark:
                ApplyDarkEffect(elementalPower, enemy);
                break;
            case ElementType.Water:
                ApplyWaterEffect(elementalPower, enemy);
                break;
            case ElementType.Fire:
                ApplyFireEffect(elementalPower, enemy);
                break;
            case ElementType.Earth:
                ApplyEarthEffect(elementalPower, enemy);
                break;
            case ElementType.None:
                // No elemental effect
                break;
            default:
                Debug.LogWarning($"Unknown element type: {element}");
                break;
        }
    }

    private static void ApplyDarkEffect(float power, Enemy enemy)
    {
        // Dark effect: Reduces target's defense
        float defenseReduction = Mathf.Clamp(power * 0.2f, 0.1f, 0.5f); // 10-50% defense reduction
        enemy.ApplyDefenseDebuff(defenseReduction, DARK_EFFECT_DURATION);

        // Visual feedback
        Debug.Log($"Applied Dark effect to {enemy.name}: {defenseReduction * 100}% defense reduction for {DARK_EFFECT_DURATION}s");
    }

    private static void ApplyWaterEffect(float power, Enemy enemy)
    {
        // Water effect: Reduces movement speed
        float slowAmount = Mathf.Clamp(power * 0.3f, 0.2f, 0.6f); // 20-60% slow
        enemy.ApplySlowEffect(slowAmount, WATER_EFFECT_DURATION);

        // Visual feedback
        Debug.Log($"Applied Water effect to {enemy.name}: {slowAmount * 100}% slow for {WATER_EFFECT_DURATION}s");
    }

    private static void ApplyFireEffect(float power, Enemy enemy)
    {
        // Fire effect: Deals damage over time
        float dotDamage = power * 0.15f; // 15% of elemental power as DoT
        enemy.ApplyDotDamage(dotDamage, FIRE_TICK_RATE, FIRE_EFFECT_DURATION);

        // Visual feedback
        Debug.Log($"Applied Fire effect to {enemy.name}: {dotDamage} damage every {FIRE_TICK_RATE}s for {FIRE_EFFECT_DURATION}s");
    }

    private static void ApplyEarthEffect(float power, Enemy enemy)
    {
        // Earth effect: Stuns the target
        float stunDuration = Mathf.Clamp(power * 0.1f, 0.5f, EARTH_EFFECT_DURATION); // 0.5-2s stun
        enemy.ApplyStun(power, stunDuration);

        // Visual feedback
        Debug.Log($"Applied Earth effect to {enemy.name}: Stunned for {stunDuration}s");
    }

    // Helper method to calculate effect power based on base power and scaling
    private static float CalculateEffectPower(float basePower, float scaling)
    {
        return Mathf.Clamp(basePower * scaling, 0f, 100f);
    }
}
using System.Collections.Generic;

public static class SkillEffectFactory
{
    public static List<ISkillInteractionEffect> CreateEffects(ItemData itemData)
    {
        var effects = new List<ISkillInteractionEffect>();

        foreach (var effectData in itemData.Effects)
        {
            var effect = CreateEffectFromData(effectData);
            if (effect != null)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    private static ISkillInteractionEffect CreateEffectFromData(ItemEffectData effectData)
    {
        return effectData.effectType switch
        {
            EffectType.DamageBonus => new SkillStatAmplifierEffect(effectData),
            EffectType.CooldownReduction => new SkillStatAmplifierEffect(effectData),
            EffectType.ProjectileSpeed => new SkillStatAmplifierEffect(effectData),
            EffectType.ProjectileRange => new SkillStatAmplifierEffect(effectData),
            EffectType.HomingEffect => new HomingActivatorEffect(effectData),
            EffectType.AreaRadius => new SkillStatAmplifierEffect(effectData),
            EffectType.AreaDuration => new SkillStatAmplifierEffect(effectData),
            EffectType.ElementalPower => new ElementalAmplifierEffect(effectData),
            _ => null
        };
    }
}


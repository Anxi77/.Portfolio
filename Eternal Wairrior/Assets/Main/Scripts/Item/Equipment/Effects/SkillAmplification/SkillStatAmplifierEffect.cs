using System.Collections.Generic;
using System.Linq;

public class SkillStatAmplifierEffect : SkillInteractionEffectBase
{
    public float damageMultiplier { get; private set; }
    public float cooldownReduction { get; private set; }
    public float rangeMultiplier { get; private set; }
    public float durationMultiplier { get; private set; }
    public float projectileSpeedMultiplier { get; private set; }
    public List<SkillType> applicableSkillTypes { get; private set; }
    public List<ElementType> applicableElements { get; private set; }

    public SkillStatAmplifierEffect(ItemEffectData effectData) : base(effectData)
    {
        switch (effectData.effectType)
        {
            case EffectType.DamageBonus:
                damageMultiplier = 1f + effectData.value;
                break;
            case EffectType.CooldownReduction:
                cooldownReduction = effectData.value;
                break;
            case EffectType.ProjectileSpeed:
                projectileSpeedMultiplier = 1f + effectData.value;
                break;
            case EffectType.ProjectileRange:
            case EffectType.AreaRadius:
                rangeMultiplier = 1f + effectData.value;
                break;
            case EffectType.AreaDuration:
                durationMultiplier = 1f + effectData.value;
                break;
        }

        applicableSkillTypes = effectData.applicableSkills?.ToList() ?? new List<SkillType>();
        applicableElements = effectData.applicableElements?.ToList() ?? new List<ElementType>();
    }

    public bool CanApplyTo(SkillType skillType, ElementType elementType = ElementType.None)
    {
        if (applicableSkillTypes.Any() && !applicableSkillTypes.Contains(skillType)) return false;
        if (elementType != ElementType.None && applicableElements.Any() && !applicableElements.Contains(elementType)) return false;
        return true;
    }

    public override void ModifySkillStats(Skill skill)
    {
        if (!CanApplyTo(skill.skillData.Type, skill.skillData.Element)) return;

        var stats = skill.skillData.GetCurrentTypeStat();
        if (stats == null) return;

        stats.baseStat.damage *= damageMultiplier;

        switch (stats)
        {
            case ProjectileSkillStat projectileStats:
                projectileStats.projectileSpeed *= projectileSpeedMultiplier;
                projectileStats.attackRange *= rangeMultiplier;
                break;
            case AreaSkillStat areaStats:
                areaStats.radius *= rangeMultiplier;
                areaStats.duration *= durationMultiplier;
                break;
            case PassiveSkillStat passiveStats:
                passiveStats.cooldown *= (1f - cooldownReduction);
                break;
        }
    }
}

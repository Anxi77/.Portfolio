using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ElementalAmplifierEffect : SkillInteractionEffectBase
{
    private readonly List<ElementType> applicableElements;
    private readonly float elementalPowerBonus;

    public ElementalAmplifierEffect(ItemEffectData effectData) : base(effectData)
    {
        this.applicableElements = effectData.applicableElements?.ToList() ?? new List<ElementType>();
        this.elementalPowerBonus = effectData.value;
    }

    public override void ModifySkillStats(Skill skill)
    {
        var skillData = skill.GetSkillData();
        if (skillData == null) return;

        if (!applicableElements.Contains(skillData.element)) return;

        var stats = skillData.GetCurrentTypeStat();
        if (stats?.baseStat != null)
        {
            stats.baseStat.elementalPower += elementalPowerBonus;
        }
    }
}

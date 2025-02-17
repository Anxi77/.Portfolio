using System.Collections.Generic;
using System.Linq;

public class ElementalAmplifierEffect : SkillInteractionEffectBase
{
    private readonly List<ElementType> applicableElements;
    private readonly float elementalPowerBonus;

    public ElementalAmplifierEffect(ItemEffectData effectData) : base(effectData)
    {
        applicableElements = effectData.applicableElements?.ToList() ?? new List<ElementType>();
        elementalPowerBonus = effectData.value;
    }

    public override void ModifySkillStats(Skill skill)
    {
        var skillData = skill.GetSkillData();
        if (skillData == null) return;

        if (!applicableElements.Contains(skillData.Element)) return;

        var stats = skillData.GetCurrentTypeStat();
        if (stats?.baseStat != null)
        {
            stats.baseStat.elementalPower += elementalPowerBonus;
        }
    }
}

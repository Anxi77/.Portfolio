using System.Collections.Generic;
using System.Linq;

public class HomingActivatorEffect : SkillInteractionEffectBase
{
    private readonly float homingRange;
    private readonly List<SkillType> applicableSkillTypes;

    public HomingActivatorEffect(ItemEffectData effectData) : base(effectData)
    {
        homingRange = effectData.value;
        applicableSkillTypes = effectData.applicableSkills?.ToList() ?? new List<SkillType>();
    }

    public override void ModifySkillStats(Skill skill)
    {
        if (!applicableSkillTypes.Contains(skill.skillData.Type)) return;
        if (!(skill is ProjectileSkills projectileSkill)) return;

        var skillData = skill.skillData;
        if (skillData != null)
        {
            var stats = skillData.GetCurrentTypeStat() as ProjectileSkillStat;
            if (stats != null)
            {
                stats.isHoming = true;
                stats.homingRange = homingRange;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : SingletonManager<SkillManager>
{
    public List<Skill> allSkillsPrefabs;

    public Skill GetSkill(SkillID skillID)
    {
        foreach (Skill skill in allSkillsPrefabs)
        {
            if (skill.data.skillID == skillID)
            {
                return skill;
            }
        }
        Debug.LogError($"스킬매니저에 {skillID} 스킬 없어용");
        return null;
    }

}

using UnityEngine;

public class DataValidator
{
    public bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null || skillData.metadata == null)
            return false;

        // 메타데이터 검증
        if (string.IsNullOrEmpty(skillData.metadata.Name) ||
            skillData.metadata.ID == SkillID.None ||
            skillData.metadata.Type == SkillType.None)
            return false;

        // 리소스 검증
        if (!ValidateResources(skillData))
            return false;

        // 스탯 데이터 검증
        if (!ValidateStats(skillData))
            return false;

        return true;
    }

    private bool ValidateResources(SkillData skillData)
    {
        // 필수 리소스 체크
        if (skillData.metadata.Prefab == null)
            return false;

        // 프로젝타일 타입의 경우 추가 검증
        if (skillData.metadata.Type == SkillType.Projectile &&
            skillData.projectile == null)
            return false;

        return true;
    }

    private bool ValidateStats(SkillData skillData)
    {
        var stats = skillData.GetCurrentTypeStat();
        if (stats == null || stats.baseStat == null)
            return false;

        return true;
    }
}
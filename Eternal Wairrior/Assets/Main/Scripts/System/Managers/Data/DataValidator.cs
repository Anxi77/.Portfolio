using UnityEngine;

public class DataValidator
{
    public bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null || skillData.metadata == null)
            return false;

        // ��Ÿ������ ����
        if (string.IsNullOrEmpty(skillData.metadata.Name) ||
            skillData.metadata.ID == SkillID.None ||
            skillData.metadata.Type == SkillType.None)
            return false;

        // ���ҽ� ����
        if (!ValidateResources(skillData))
            return false;

        // ���� ������ ����
        if (!ValidateStats(skillData))
            return false;

        return true;
    }

    private bool ValidateResources(SkillData skillData)
    {
        // �ʼ� ���ҽ� üũ
        if (skillData.metadata.Prefab == null)
            return false;

        // ������Ÿ�� Ÿ���� ��� �߰� ����
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
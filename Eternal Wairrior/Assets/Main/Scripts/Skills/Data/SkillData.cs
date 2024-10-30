using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct SkillData
{
    public SkillID _SkillID;
    public SkillType _SkillType;
    public string Name;
    public string Description;
    public int id;
    public GameObject[] prefabsByLevel;
    public GameObject projectile;
    public Image icon;

    // Ÿ�Ժ� ����
    public ProjectileSkillStat projectileStat;
    public AreaSkillStat areaStat;
    public PassiveSkillStat passiveStat;

    // ���� ��ų Ÿ�Կ� �´� ���� ��ȯ
    public ISkillStat GetCurrentTypeStat()
    {
        return _SkillType switch
        {
            SkillType.Projectile => projectileStat,
            SkillType.Area => areaStat,
            SkillType.Passive => passiveStat,
            _ => throw new System.ArgumentException("Invalid skill type")
        };
    }
}

[System.Serializable]
public class SkillDataWrapper
{
    public List<SkillData> skillDatas;
}
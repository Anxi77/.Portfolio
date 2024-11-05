using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class SerializedSkillStats
{
    public SkillID skillID;
    public List<SkillStatData> levelStats = new List<SkillStatData>();
}

[System.Serializable]
public class SkillResourceReferences
{
    public string iconPath;
    public string metadataPrefabPath;
    public string projectilePrefabPath;
    public List<string> levelPrefabPaths = new List<string>();
}

public class SkillDataContainer : ScriptableObject
{
    public List<SkillData> skillList;
    public List<SerializedSkillStats> serializedSkillStats = new List<SerializedSkillStats>();
    public Dictionary<SkillID, SkillResourceReferences> resourceReferences;

    public void SaveData(List<SkillData> newSkillList, Dictionary<SkillID, List<SkillStatData>> newSkillStatsList)
    {
        // ��ų ����Ʈ ����
        skillList = new List<SkillData>(newSkillList);

        // ��ų ���� ���� - Dictionary�� SerializedSkillStats ����Ʈ�� ��ȯ
        serializedSkillStats.Clear();
        foreach (var pair in newSkillStatsList)
        {
            serializedSkillStats.Add(new SerializedSkillStats
            {
                skillID = pair.Key,
                levelStats = new List<SkillStatData>(pair.Value)
            });
        }

        // ���ҽ� ���۷��� ����
        resourceReferences = new Dictionary<SkillID, SkillResourceReferences>();
        foreach (var skill in skillList)
        {
            if (skill.metadata == null || skill.metadata.ID == SkillID.None) continue;

            var refs = new SkillResourceReferences();

            // ������ ��� ����
            if (skill.icon != null)
            {
                refs.iconPath = AssetDatabase.GetAssetPath(skill.icon);
            }

            // ��Ÿ������ ������ ��� ����
            if (skill.metadata.Prefab != null)
            {
                refs.metadataPrefabPath = AssetDatabase.GetAssetPath(skill.metadata.Prefab);
            }

            // ������Ÿ�� ������ ��� ����
            if (skill.projectile != null)
            {
                refs.projectilePrefabPath = AssetDatabase.GetAssetPath(skill.projectile);
            }

            // ������ ������ ��� ����
            if (skill.prefabsByLevel != null)
            {
                foreach (var prefab in skill.prefabsByLevel)
                {
                    if (prefab != null)
                    {
                        refs.levelPrefabPaths.Add(AssetDatabase.GetAssetPath(prefab));
                    }
                }
            }

            resourceReferences[skill.metadata.ID] = refs;
        }
    }

    private void OnEnable()
    {
        if (skillList == null)
            skillList = new List<SkillData>();

        if (serializedSkillStats == null)
            serializedSkillStats = new List<SerializedSkillStats>();

        if (resourceReferences == null)
            resourceReferences = new Dictionary<SkillID, SkillResourceReferences>();

        // SerializedSkillStats�� Dictionary�� ��ȯ
        var skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
        foreach (var serializedStats in serializedSkillStats)
        {
            skillStatsList[serializedStats.skillID] = serializedStats.levelStats;
        }

        // ���ҽ� ���۷��� ����
        foreach (var skill in skillList)
        {
            if (skill.metadata == null || skill.metadata.ID == SkillID.None) continue;

            if (resourceReferences.TryGetValue(skill.metadata.ID, out var refs))
            {
                // ������ ����
                if (!string.IsNullOrEmpty(refs.iconPath))
                {
                    skill.icon = AssetDatabase.LoadAssetAtPath<Sprite>(refs.iconPath);
                    skill.metadata.Icon = skill.icon;
                }

                // ��Ÿ������ ������ ����
                if (!string.IsNullOrEmpty(refs.metadataPrefabPath))
                {
                    skill.metadata.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(refs.metadataPrefabPath);
                }

                // ������Ÿ�� ������ ����
                if (!string.IsNullOrEmpty(refs.projectilePrefabPath))
                {
                    skill.projectile = AssetDatabase.LoadAssetAtPath<GameObject>(refs.projectilePrefabPath);
                }

                // ������ ������ ����
                if (refs.levelPrefabPaths.Count > 0)
                {
                    skill.prefabsByLevel = new GameObject[refs.levelPrefabPaths.Count];
                    for (int i = 0; i < refs.levelPrefabPaths.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(refs.levelPrefabPaths[i]))
                        {
                            skill.prefabsByLevel[i] = AssetDatabase.LoadAssetAtPath<GameObject>(refs.levelPrefabPaths[i]);
                        }
                    }
                }
            }

            // ��ų ���� ����
            if (skillStatsList.TryGetValue(skill.metadata.ID, out var stats))
            {
                foreach (var statData in stats)
                {
                    var skillStat = statData.CreateSkillStat(skill.metadata.Type);
                    skill.SetStatsForLevel(statData.level, skillStat);
                }
            }
        }
    }

    public Dictionary<SkillID, List<SkillStatData>> GetSkillStatsList()
    {
        var result = new Dictionary<SkillID, List<SkillStatData>>();
        foreach (var serializedStats in serializedSkillStats)
        {
            result[serializedStats.skillID] = new List<SkillStatData>(serializedStats.levelStats);
        }
        return result;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

[Serializable]
public class SkillMetadata
{
    public SkillID ID;
    public string Name;
    public string Description;
    public SkillType Type;
    public ElementType Element;
    public int Tier;
    public string[] Tags;
    public GameObject Prefab;
    public Sprite Icon;
}

[Serializable]
public class SkillData
{
    public SkillID ID;
    public string skillName;
    public string description;
    public SkillType type;
    public ElementType element;
    public int tier;
    public string[] tags;
    [JsonIgnore]
    public GameObject defualtPrefab { get => Resources.Load<GameObject>(prefabPath); private set => prefabPath = value.name; }
    public string prefabPath;
    [JsonIgnore]
    private Dictionary<int, ISkillStat> statsByLevel;

    [JsonIgnore]
    public Sprite icon;
    public string iconPath;

    [JsonIgnore]
    public GameObject projectile { get => Resources.Load<GameObject>(projectilePath); private set => projectilePath = value.name; }
    public string projectilePath;

    [JsonIgnore]
    public GameObject[] prefabsByLevel;
    public string[] prefabsByLevelPaths;

    public BaseSkillStat baseStats;
    public ProjectileSkillStat projectileStat;
    public AreaSkillStat areaStat;
    public PassiveSkillStat passiveStat;
    public ResourceReferenceData resourceReferences;

    public SkillData()
    {
        ID = SkillID.None;
        skillName = "None";
        description = "None";
        type = SkillType.None;
        element = ElementType.None;
        tier = 0;
        tags = new string[0];
        baseStats = new BaseSkillStat();
        statsByLevel = new Dictionary<int, ISkillStat>();
        prefabsByLevel = new GameObject[0];
        projectileStat = new ProjectileSkillStat { baseStat = baseStats };
        areaStat = new AreaSkillStat { baseStat = baseStats };
        passiveStat = new PassiveSkillStat { baseStat = baseStats };
        resourceReferences = new ResourceReferenceData();

    }

    public ISkillStat GetStatsForLevel(int level)
    {
        if (statsByLevel == null)
        {
            statsByLevel = new Dictionary<int, ISkillStat>();
            Debug.LogWarning($"statsByLevel was null for skill {skillName ?? "Unknown"}");
        }

        if (statsByLevel.TryGetValue(level, out var stats))
            return stats;

        var defaultStats = CreateDefaultStats();
        statsByLevel[level] = defaultStats;
        return defaultStats;
    }

    public void SetStatsForLevel(int level, ISkillStat stats)
    {
        if (stats?.baseStat == null)
        {
            Debug.LogError("Attempting to set null stats");
            return;
        }

        try
        {
            baseStats = new BaseSkillStat(stats.baseStat);

            switch (stats)
            {
                case ProjectileSkillStat projectileStats:
                    projectileStat = new ProjectileSkillStat(projectileStats);
                    break;
                case AreaSkillStat areaStats:
                    areaStat = new AreaSkillStat(areaStats);
                    break;
                case PassiveSkillStat passiveStats:
                    passiveStat = new PassiveSkillStat(passiveStats);
                    break;
            }

            if (statsByLevel == null) statsByLevel = new Dictionary<int, ISkillStat>();
            statsByLevel[level] = stats;

            Debug.Log($"Successfully set stats for level {level}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting stats: {e.Message}");
        }
    }

    public ISkillStat GetCurrentTypeStat()
    {
        switch (type)
        {
            case SkillType.Projectile:
                return projectileStat;
            case SkillType.Area:

                return areaStat;
            case SkillType.Passive:
                return passiveStat;
            default:
                return null;
        }
    }

    private ISkillStat CreateDefaultStats()
    {
        switch (type)
        {
            case SkillType.Projectile:
                return new ProjectileSkillStat();

            case SkillType.Area:
                return new AreaSkillStat();
            case SkillType.Passive:
                return new PassiveSkillStat();
            default:
                Debug.LogWarning($"Creating default ProjectileSkillStat for unknown type: {type}");
                return new ProjectileSkillStat();
        }
    }

    public int GetMaxLevel()
    {
        return statsByLevel.Keys.Count > 0 ? statsByLevel.Keys.Max() : 1;
    }

    public void RemoveLevel(int level)
    {
        if (statsByLevel.ContainsKey(level))
        {
            statsByLevel.Remove(level);
        }
    }
}

[Serializable]
public class ResourceReferenceData
{

    public List<string> keys = new List<string>();
    public List<AssetReference> values = new List<AssetReference>();

    public void Add(string key, AssetReference value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public bool TryGetValue(string key, out AssetReference value)
    {
        int index = keys.IndexOf(key);
        if (index != -1)
        {
            value = values[index];
            return true;
        }
        value = null;
        return false;
    }

    public bool ContainsKey(string key)
    {
        return keys.Contains(key);
    }
}

[Serializable]
public class AssetReference
{
    public string path;
}

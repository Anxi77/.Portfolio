using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

[Serializable]
public class SkillData : ICloneable
{
    #region Properties
    public SkillID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillType Type { get; set; }
    public ElementType Element { get; set; }
    public int Tier { get; set; }
    public string[] Tags { get; set; }
    [JsonIgnore]
    public GameObject Prefab { get; set; }
    public string PrefabPath { get; set; }
    [JsonIgnore]
    private Dictionary<int, ISkillStat> StatsByLevel { get; set; }
    [JsonIgnore]
    public Sprite Icon { get; set; }
    public string IconPath { get; set; }
    [JsonIgnore]
    public GameObject ProjectilePrefab { get; set; }
    public string ProjectilePath { get; set; }
    [JsonIgnore]
    public GameObject[] PrefabsByLevel;
    public string[] PrefabsByLevelPaths { get; set; }
    [JsonIgnore]
    public BaseSkillStat BaseStats { get; set; }
    [JsonIgnore]
    public ProjectileSkillStat ProjectileStat { get; set; }
    [JsonIgnore]
    public AreaSkillStat AreaStat { get; set; }
    [JsonIgnore]
    public PassiveSkillStat PassiveStat { get; set; }
    public ResourceReferenceData ResourceReferences { get; set; }
    #endregion

    public SkillData()
    {
        ID = SkillID.None;
        Name = "None";
        Description = "None";
        Type = SkillType.None;
        Element = ElementType.None;
        Tier = 0;
        Tags = new string[0];
        BaseStats = new BaseSkillStat();
        StatsByLevel = new Dictionary<int, ISkillStat>();
        PrefabsByLevel = new GameObject[0];
        ProjectileStat = new ProjectileSkillStat { baseStat = BaseStats };
        AreaStat = new AreaSkillStat { baseStat = BaseStats };
        PassiveStat = new PassiveSkillStat { baseStat = BaseStats };
        ResourceReferences = new ResourceReferenceData();
    }

    public ISkillStat GetStatsForLevel(int level)
    {
        if (StatsByLevel == null)
        {
            StatsByLevel = new Dictionary<int, ISkillStat>();
            Debug.LogWarning($"StatsByLevel was null for skill {Name ?? "Unknown"}");
        }

        if (StatsByLevel.TryGetValue(level, out var stats))
            return stats;

        var defaultStats = CreateDefaultStats();
        StatsByLevel[level] = defaultStats;
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
            BaseStats = new BaseSkillStat(stats.baseStat);

            switch (stats)
            {
                case ProjectileSkillStat projectileStats:
                    ProjectileStat = new ProjectileSkillStat(projectileStats);
                    break;
                case AreaSkillStat areaStats:
                    AreaStat = new AreaSkillStat(areaStats);
                    break;
                case PassiveSkillStat passiveStats:
                    PassiveStat = new PassiveSkillStat(passiveStats);
                    break;
            }

            if (StatsByLevel == null) StatsByLevel = new Dictionary<int, ISkillStat>();
            StatsByLevel[level] = stats;

            Debug.Log($"Successfully set stats for level {level}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting stats: {e.Message}");
        }
    }

    public ISkillStat GetCurrentTypeStat()
    {
        switch (Type)
        {
            case SkillType.Projectile:
                return ProjectileStat;
            case SkillType.Area:
                return AreaStat;
            case SkillType.Passive:
                return PassiveStat;
            default:
                return null;
        }
    }

    private ISkillStat CreateDefaultStats()
    {
        switch (Type)
        {
            case SkillType.Projectile:
                return new ProjectileSkillStat();

            case SkillType.Area:
                return new AreaSkillStat();
            case SkillType.Passive:
                return new PassiveSkillStat();
            default:
                Debug.LogWarning($"Creating default ProjectileSkillStat for unknown type: {Type}");
                return new ProjectileSkillStat();
        }
    }

    public int GetMaxLevel()
    {
        return StatsByLevel.Keys.Count > 0 ? StatsByLevel.Keys.Max() : 1;
    }

    public void RemoveLevel(int level)
    {
        if (StatsByLevel.ContainsKey(level))
        {
            StatsByLevel.Remove(level);
        }
    }

    #region ICloneable
    public object Clone()
    {
        return new SkillData
        {
            ID = this.ID,
            Name = this.Name,
            Description = this.Description,
            Type = this.Type,
            Element = this.Element,
            Tier = this.Tier,
            Tags = (string[])this.Tags?.Clone(),
            PrefabPath = this.PrefabPath,
            IconPath = this.IconPath,
            ProjectilePath = this.ProjectilePath,
            PrefabsByLevelPaths = (string[])this.PrefabsByLevelPaths?.Clone(),
            ResourceReferences = new ResourceReferenceData
            {
                Keys = new List<string>(this.ResourceReferences?.Keys ?? new List<string>()),
                Values = new List<AssetReference>(this.ResourceReferences?.Values ?? new List<AssetReference>())
            }
        };
    }
    #endregion
}

[Serializable]
public class ResourceReferenceData
{
    public List<string> Keys = new List<string>();
    public List<AssetReference> Values = new List<AssetReference>();

    public void Add(string key, AssetReference value)
    {
        Keys.Add(key);
        Values.Add(value);
    }

    public void Clear()
    {
        Keys.Clear();
        Values.Clear();
    }

    public bool TryGetValue(string key, out AssetReference value)
    {
        int index = Keys.IndexOf(key);
        if (index != -1)
        {
            value = Values[index];
            return true;
        }
        value = null;
        return false;
    }

    public bool ContainsKey(string key)
    {
        return Keys.Contains(key);
    }
}

[Serializable]
public class AssetReference
{
    public string Path;
}

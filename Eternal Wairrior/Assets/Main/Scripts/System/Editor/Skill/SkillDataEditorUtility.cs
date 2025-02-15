using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public static class SkillDataEditorUtility
{
    #region Constants
    private const string SKILL_DB_PATH = "SkillData/Json";
    private const string SKILL_ICON_PATH = "SkillData/Icons";
    private const string SKILL_PREFAB_PATH = "SkillData/Prefabs";
    private const string SKILL_STAT_PATH = "SkillData/Stats";
    #endregion

    #region Data Management
    private static Dictionary<SkillID, SkillData> skillDatabase = new();
    private static Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new();
    #endregion

    static SkillDataEditorUtility()
    {
        JSONIO<SkillData>.SetCustomPath(SKILL_DB_PATH);
        CSVIO<SkillStatData>.SetCustomPath(SKILL_STAT_PATH);
    }

    public static Dictionary<SkillID, SkillData> GetSkillDatabase()
    {
        if (!skillDatabase.Any())
        {
            LoadSkillDatabase();
        }
        return new Dictionary<SkillID, SkillData>(skillDatabase);
    }

    public static Dictionary<SkillID, Dictionary<int, SkillStatData>> GetStatDatabase()
    {
        if (!statDatabase.Any())
        {
            LoadStatDatabase();
        }
        return statDatabase;
    }

    private static void LoadSkillDatabase()
    {
        try
        {
            skillDatabase.Clear();
            foreach (SkillID skillId in Enum.GetValues(typeof(SkillID)))
            {
                if (skillId == SkillID.None) continue;

                var skillData = JSONIO<SkillData>.LoadData(skillId.ToString());
                if (skillData != null)
                {
                    // 아이콘 로드
                    if (!string.IsNullOrEmpty(skillData.IconPath))
                    {
                        skillData.Icon = ResourceIO<Sprite>.LoadData(skillData.IconPath);
                    }

                    // 기본 프리팹 로드
                    if (!string.IsNullOrEmpty(skillData.PrefabPath))
                    {
                        skillData.Prefab = ResourceIO<GameObject>.LoadData(skillData.PrefabPath);
                    }

                    // 프로젝타일 프리팹 로드 (해당하는 경우)
                    if (skillData.Type == SkillType.Projectile && !string.IsNullOrEmpty(skillData.ProjectilePath))
                    {
                        skillData.ProjectilePrefab = ResourceIO<GameObject>.LoadData(skillData.ProjectilePath);
                    }

                    // 레벨별 프리팹 로드
                    if (skillData.PrefabsByLevelPaths != null)
                    {
                        skillData.PrefabsByLevel = new GameObject[skillData.PrefabsByLevelPaths.Length];
                        for (int i = 0; i < skillData.PrefabsByLevelPaths.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(skillData.PrefabsByLevelPaths[i]))
                            {
                                skillData.PrefabsByLevel[i] = ResourceIO<GameObject>.LoadData(skillData.PrefabsByLevelPaths[i]);
                            }
                        }
                    }

                    skillDatabase[skillId] = skillData;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading skill database: {e.Message}");
            skillDatabase = new Dictionary<SkillID, SkillData>();
        }
    }

    private static void LoadStatDatabase()
    {
        try
        {
            statDatabase.Clear();

            // 각 스킬 타입별로 CSV 파일 로드
            LoadStatsFromCSV("ProjectileSkillStats", SkillType.Projectile);
            LoadStatsFromCSV("AreaSkillStats", SkillType.Area);
            LoadStatsFromCSV("PassiveSkillStats", SkillType.Passive);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading stat database: {e.Message}");
            statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
        }
    }

    private static void LoadStatsFromCSV(string fileName, SkillType expectedType)
    {
        var stats = CSVIO<SkillStatData>.LoadBulkData(fileName);
        foreach (var stat in stats)
        {
            if (!statDatabase.ContainsKey(stat.SkillID))
            {
                statDatabase[stat.SkillID] = new Dictionary<int, SkillStatData>();
            }
            statDatabase[stat.SkillID][stat.Level] = stat;
        }
    }

    public static void SaveSkillData(SkillData skillData)
    {
        if (skillData == null) return;
        if (skillData.ID == SkillID.None || skillData.Type == SkillType.None)
        {
            Debug.LogError("Cannot save skill data with None ID or Type");
            return;
        }

        try
        {
            // 리소스 저장
            SaveSkillResources(skillData);

            // JSON 데이터 저장
            JSONIO<SkillData>.SaveData(skillData.ID.ToString(), skillData);
            skillDatabase[skillData.ID] = skillData.Clone() as SkillData;

            // 스탯 데이터 저장
            SaveStatData(skillData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}");
        }
    }

    private static void SaveStatData(SkillData skillData)
    {
        if (skillData.ID == SkillID.None || skillData.Type == SkillType.None) return;

        if (!statDatabase.ContainsKey(skillData.ID))
        {
            statDatabase[skillData.ID] = new Dictionary<int, SkillStatData>();
            var defaultStat = CreateDefaultStatData(skillData);
            statDatabase[skillData.ID][1] = defaultStat;
        }

        SaveStatDatabase();
    }

    private static void SaveSkillResources(SkillData skillData)
    {
        // 아이콘 경로 설정 (실제 저장은 이미 EditorWindow에서 수행됨)
        if (skillData.Icon != null && string.IsNullOrEmpty(skillData.IconPath))
        {
            skillData.IconPath = $"{SKILL_ICON_PATH}/{skillData.ID}_Icon";
        }

        // 프리팹 저장
        if (skillData.Prefab != null)
        {
            ResourceIO<GameObject>.SaveData($"{SKILL_PREFAB_PATH}/{skillData.ID}_Prefab", skillData.Prefab);
        }

        // 프로젝타일 프리팹 저장
        if (skillData.Type == SkillType.Projectile && skillData.ProjectilePrefab != null)
        {
            ResourceIO<GameObject>.SaveData($"{SKILL_PREFAB_PATH}/{skillData.ID}_Projectile", skillData.ProjectilePrefab);
        }

        // 레벨별 프리팹 저장
        if (skillData.PrefabsByLevel != null)
        {
            for (int i = 0; i < skillData.PrefabsByLevel.Length; i++)
            {
                if (skillData.PrefabsByLevel[i] != null)
                {
                    ResourceIO<GameObject>.SaveData(
                        $"{SKILL_PREFAB_PATH}/{skillData.ID}_Level_{i + 1}",
                        skillData.PrefabsByLevel[i]
                    );
                }
            }
        }
    }

    public static void SaveStatDatabase()
    {
        var projectileStats = new List<SkillStatData>();
        var areaStats = new List<SkillStatData>();
        var passiveStats = new List<SkillStatData>();

        foreach (var skillStats in statDatabase.Values)
        {
            foreach (var stat in skillStats.Values)
            {
                if (!skillDatabase.ContainsKey(stat.SkillID)) continue;

                var skill = skillDatabase[stat.SkillID];
                switch (skill.Type)
                {
                    case SkillType.Projectile:
                        projectileStats.Add(stat);
                        break;
                    case SkillType.Area:
                        areaStats.Add(stat);
                        break;
                    case SkillType.Passive:
                        passiveStats.Add(stat);
                        break;
                }
            }
        }

        CSVIO<SkillStatData>.SaveBulkData("ProjectileSkillStats", projectileStats);
        CSVIO<SkillStatData>.SaveBulkData("AreaSkillStats", areaStats);
        CSVIO<SkillStatData>.SaveBulkData("PassiveSkillStats", passiveStats);
    }

    public static void DeleteSkillData(SkillID skillId)
    {
        try
        {
            if (skillDatabase.Remove(skillId))
            {
                // JSON 파일 삭제
                JSONIO<SkillData>.DeleteData(skillId.ToString());

                // 리소스 파일들 삭제
                DeleteSkillResources(skillId);

                // 스탯 데이터 삭제
                statDatabase.Remove(skillId);
                SaveStatDatabase();

                AssetDatabase.Refresh();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting skill {skillId}: {e.Message}");
        }
    }

    private static void DeleteSkillResources(SkillID skillId)
    {
        // 아이콘 삭제
        ResourceIO<Sprite>.DeleteData($"{SKILL_ICON_PATH}/{skillId}_Icon");

        // 프리팹 삭제
        ResourceIO<GameObject>.DeleteData($"{SKILL_PREFAB_PATH}/{skillId}_Prefab");

        // 프로젝타일 프리팹 삭제
        ResourceIO<GameObject>.DeleteData($"{SKILL_PREFAB_PATH}/{skillId}_Projectile");

        // 레벨별 프리팹 삭제
        var stats = GetStatDatabase().GetValueOrDefault(skillId);
        if (stats != null)
        {
            int maxLevel = stats.Values.Max(s => s.MaxSkillLevel);
            for (int i = 1; i <= maxLevel; i++)
            {
                ResourceIO<GameObject>.DeleteData($"{SKILL_PREFAB_PATH}/{skillId}_Level_{i}");
            }
        }
    }

    public static void InitializeDefaultData()
    {
        try
        {
            // 기존 데이터 정리
            ClearAllData();

            // 데이터베이스 초기화
            skillDatabase = new Dictionary<SkillID, SkillData>();
            statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

            // Resources 폴더 내의 모든 관련 디렉토리 정리
            string resourceRoot = Path.Combine(Application.dataPath, "Resources");
            CleanDirectory(Path.Combine(resourceRoot, SKILL_DB_PATH));
            CleanDirectory(Path.Combine(resourceRoot, SKILL_ICON_PATH));
            CleanDirectory(Path.Combine(resourceRoot, SKILL_PREFAB_PATH));
            CleanDirectory(Path.Combine(resourceRoot, SKILL_STAT_PATH));

            // 기본 CSV 파일 생성
            CreateDefaultCSVFiles();

            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error resetting data: {e.Message}");
        }
    }

    private static void ClearAllData()
    {
        try
        {
            // JSON 데이터 삭제
            JSONIO<SkillData>.ClearAll();

            // CSV 데이터 삭제
            CSVIO<SkillStatData>.ClearAll();

            // 리소스 캐시 초기화
            ResourceIO<Sprite>.ClearCache();
            ResourceIO<GameObject>.ClearCache();

            // 데이터베이스 초기화
            skillDatabase.Clear();
            statDatabase.Clear();

            // Resources 폴더 정리
            string resourceRoot = Path.Combine(Application.dataPath, "Resources");
            if (Directory.Exists(resourceRoot))
            {
                string[] paths = new[] { SKILL_DB_PATH, SKILL_ICON_PATH, SKILL_PREFAB_PATH, SKILL_STAT_PATH };
                foreach (var path in paths)
                {
                    CleanDirectory(Path.Combine(resourceRoot, path));
                }
            }

            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error clearing data: {e.Message}");
        }
    }

    private static void CleanDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            try
            {
                // 디렉토리 내의 모든 파일 삭제
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string assetPath = file.Replace('\\', '/');
                        if (assetPath.StartsWith(Application.dataPath))
                        {
                            assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                            AssetDatabase.DeleteAsset(assetPath);
                        }
                    }
                }

                // 디렉토리 재생성
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error cleaning directory {path}: {e.Message}");
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void CreateDefaultCSVFiles()
    {
        var headers = new List<string> { "skillname", "skilltype", "skillid", "description" };
        headers.AddRange(typeof(SkillStatData).GetProperties()
            .Where(p => p.CanRead && p.CanWrite && p.Name != "SkillID")
            .OrderBy(p => p.Name)
            .Select(p => p.Name.ToLower()));

        string headerLine = string.Join(",", headers);

        CSVIO<SkillStatData>.CreateDefaultFile("ProjectileSkillStats", headerLine);
        CSVIO<SkillStatData>.CreateDefaultFile("AreaSkillStats", headerLine);
        CSVIO<SkillStatData>.CreateDefaultFile("PassiveSkillStats", headerLine);
    }

    private static SkillStatData CreateDefaultStatData(SkillData skillData)
    {
        if (skillData.ID == SkillID.None || skillData.Type == SkillType.None)
            throw new ArgumentException("Cannot create default stats for skill with None ID or Type");

        var defaultStat = new SkillStatData
        {
            SkillID = skillData.ID,
            Level = 1,
            MaxSkillLevel = 5,
            Damage = 10f,
            ElementalPower = 1f,
            Element = skillData.Element
        };

        switch (skillData.Type)
        {
            case SkillType.Projectile:
                defaultStat.ProjectileSpeed = 10f;
                defaultStat.ProjectileScale = 1f;
                defaultStat.ShotInterval = 0.5f;
                defaultStat.PierceCount = 1;
                defaultStat.AttackRange = 10f;
                break;

            case SkillType.Area:
                defaultStat.Radius = 5f;
                defaultStat.Duration = 3f;
                defaultStat.TickRate = 1f;
                defaultStat.IsPersistent = false;
                break;

            case SkillType.Passive:
                defaultStat.EffectDuration = 5f;
                defaultStat.Cooldown = 10f;
                defaultStat.TriggerChance = 1f;
                break;

            default:
                throw new ArgumentException($"Invalid skill type: {skillData.Type}");
        }

        return defaultStat;
    }

    public static void SaveWithBackup()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupPath = Path.Combine(SKILL_DB_PATH, "Backups", timestamp);

        // JSON 데이터 백업
        foreach (var skill in skillDatabase.Values)
        {
            JSONIO<SkillData>.SaveData($"{backupPath}/{skill.ID}_Data", skill);
        }

        // 스탯 데이터 백업
        string backupStatPath = $"{SKILL_STAT_PATH}/Backups/{timestamp}";
        SaveStatsToBackup(backupStatPath);

        Debug.Log($"Backup created at: {backupPath}");
    }

    private static void SaveStatsToBackup(string backupPath)
    {
        var projectileStats = new List<SkillStatData>();
        var areaStats = new List<SkillStatData>();
        var passiveStats = new List<SkillStatData>();

        foreach (var skillStats in statDatabase.Values)
        {
            foreach (var stat in skillStats.Values)
            {
                if (!skillDatabase.ContainsKey(stat.SkillID)) continue;

                var skill = skillDatabase[stat.SkillID];
                switch (skill.Type)
                {
                    case SkillType.Projectile:
                        projectileStats.Add(stat);
                        break;
                    case SkillType.Area:
                        areaStats.Add(stat);
                        break;
                    case SkillType.Passive:
                        passiveStats.Add(stat);
                        break;
                }
            }
        }

        string fullBackupPath = Path.Combine("Assets/Resources", backupPath);
        Directory.CreateDirectory(fullBackupPath);

        CSVIO<SkillStatData>.SaveBulkData($"{backupPath}/ProjectileSkillStats", projectileStats);
        CSVIO<SkillStatData>.SaveBulkData($"{backupPath}/AreaSkillStats", areaStats);
        CSVIO<SkillStatData>.SaveBulkData($"{backupPath}/PassiveSkillStats", passiveStats);
    }
}
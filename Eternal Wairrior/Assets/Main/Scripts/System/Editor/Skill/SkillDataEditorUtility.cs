using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json;

public static class SkillDataEditorUtility
{
    #region Constants
    private const string RESOURCE_ROOT = "Assets/Resources";
    private const string SKILL_DB_PATH = "SkillData/Json";
    private const string SKILL_ICON_PATH = "SkillData/Icons";
    private const string SKILL_PREFAB_PATH = "SkillData/Prefabs";
    private const string SKILL_STAT_PATH = "SkillData/Stats";
    #endregion

    #region Data Management
    private static Dictionary<SkillID, SkillData> skillDatabase = new();
    private static Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new();

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

    public static void SaveSkillData(SkillData skillData)
    {
        if (skillData == null) return;

        try
        {
            // 아이콘 리소스 저장
            SaveSkillResources(skillData);

            // 데이터베이스에 저장
            var clonedData = skillData.Clone();
            bool isNewSkill = !skillDatabase.ContainsKey(skillData.ID);
            skillDatabase[skillData.ID] = (SkillData)clonedData;

            // 스탯 데이터베이스 저장
            if (isNewSkill)
            {
                // 새로운 스킬인 경우 기본 스탯 생성
                if (!statDatabase.ContainsKey(skillData.ID))
                {
                    statDatabase[skillData.ID] = new Dictionary<int, SkillStatData>();
                    var defaultStat = CreateDefaultStatData(skillData);
                    statDatabase[skillData.ID][1] = defaultStat;
                    Debug.Log($"Created default stats for new skill: {skillData.Name}");
                }
            }
            else if (statDatabase.ContainsKey(skillData.ID))
            {
                // 기존 스킬의 경우 타입이 변경되었는지 확인
                var stats = statDatabase[skillData.ID];
                bool typeChanged = false;

                foreach (var stat in stats.Values)
                {
                    if (skillDatabase.TryGetValue(stat.SkillID, out var skill) && skill.Type != skillData.Type)
                    {
                        typeChanged = true;
                        break;
                    }
                }

                if (typeChanged)
                {
                    Debug.Log($"Skill type changed for {skillData.Name}. Migrating stats...");
                    var migratedStats = new Dictionary<int, SkillStatData>();
                    foreach (var kvp in stats)
                    {
                        var migratedStat = MigrateStatData(kvp.Value, skillData.Type);
                        migratedStats[kvp.Key] = migratedStat;
                    }
                    statDatabase[skillData.ID] = migratedStats;
                }
            }

            SaveDatabase();
            SaveStatDatabase();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    private static SkillStatData CreateDefaultStatData(SkillData skillData)
    {
        var defaultStat = new SkillStatData
        {
            SkillID = skillData.ID,
            Level = 1,
            MaxSkillLevel = 5,
            Damage = 10f,
            ElementalPower = 1f,
            Element = skillData.Element
        };

        // 스킬 타입에 따른 기본값 설정
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
        }

        return defaultStat;
    }

    private static SkillStatData MigrateStatData(SkillStatData oldStat, SkillType newType)
    {
        var newStat = new SkillStatData
        {
            SkillID = oldStat.SkillID,
            Level = oldStat.Level,
            MaxSkillLevel = oldStat.MaxSkillLevel,
            Damage = oldStat.Damage,
            ElementalPower = oldStat.ElementalPower,
            Element = oldStat.Element
        };

        // 새로운 타입에 따른 기본값 설정
        switch (newType)
        {
            case SkillType.Projectile:
                newStat.ProjectileSpeed = 10f;
                newStat.ProjectileScale = 1f;
                newStat.ShotInterval = 0.5f;
                newStat.PierceCount = 1;
                newStat.AttackRange = 10f;
                break;

            case SkillType.Area:
                newStat.Radius = 5f;
                newStat.Duration = 3f;
                newStat.TickRate = 1f;
                newStat.IsPersistent = false;
                break;

            case SkillType.Passive:
                newStat.EffectDuration = 5f;
                newStat.Cooldown = 10f;
                newStat.TriggerChance = 1f;
                break;
        }

        return newStat;
    }

    public static void DeleteSkillData(SkillID skillId)
    {
        try
        {
            // 메모리에서 스킬 삭제
            if (skillDatabase.TryGetValue(skillId, out var skill) && skillDatabase.Remove(skillId))
            {
                // 아이콘 리소스 삭제
                string iconPath = $"Assets/Resources/SkillData/Icons/{skillId}_Icon.png";
                if (File.Exists(iconPath))
                {
                    AssetDatabase.DeleteAsset(iconPath);
                }

                // 프리팹 리소스 삭제
                string prefabPath = $"Assets/Resources/SkillData/Prefabs/{skillId}_Prefab.prefab";
                if (File.Exists(prefabPath))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }

                // 레벨별 프리팹 삭제
                var stats = GetStatDatabase().GetValueOrDefault(skillId);
                if (stats != null)
                {
                    int maxLevel = stats.Values.Max(s => s.MaxSkillLevel);
                    for (int i = 1; i <= maxLevel; i++)
                    {
                        string levelPrefabPath = $"Assets/Resources/SkillData/Prefabs/{skillId}_Level_{i}.prefab";
                        if (File.Exists(levelPrefabPath))
                        {
                            AssetDatabase.DeleteAsset(levelPrefabPath);
                        }
                    }
                }

                // 스탯 데이터베이스에서도 삭제
                statDatabase.Remove(skillId);

                // JSON 파일 삭제
                string jsonPath = $"Assets/Resources/SkillData/Json/{skillId}_Data.json";
                if (File.Exists(jsonPath))
                {
                    AssetDatabase.DeleteAsset(jsonPath);
                }

                SaveDatabase();
                SaveStatDatabase();

                AssetDatabase.Refresh();
                Debug.Log($"Skill {skillId} and its resources deleted successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting skill {skillId}: {e.Message}");
        }
    }

    public static void SaveDatabase()
    {
        try
        {
            foreach (var skill in skillDatabase.Values)
            {
                string jsonPath = Path.Combine(RESOURCE_ROOT, SKILL_DB_PATH, $"{skill.ID}_Data.json");
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                string jsonData = JsonConvert.SerializeObject(skill, Formatting.Indented);
                File.WriteAllText(jsonPath, jsonData);
            }

            Debug.Log("Skill database saved successfully");
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving skill database: {e.Message}");
        }
    }

    public static void SaveStatDatabase()
    {
        try
        {
            var projectileStats = new List<SkillStatData>();
            var areaStats = new List<SkillStatData>();
            var passiveStats = new List<SkillStatData>();
            var unassignedStats = new List<SkillStatData>();  // SkillType.None인 경우를 위한 리스트

            foreach (var skillStats in statDatabase.Values)
            {
                if (skillStats == null || !skillStats.Any()) continue;

                foreach (var stat in skillStats.Values)
                {
                    if (stat == null || !skillDatabase.ContainsKey(stat.SkillID)) continue;

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
                        case SkillType.None:
                            unassignedStats.Add(stat);
                            Debug.LogWarning($"Skill {skill.Name} (ID: {skill.ID}) has no type assigned");
                            break;
                    }
                }
            }

            // 디렉토리 생성
            string statPath = Path.Combine(RESOURCE_ROOT, SKILL_STAT_PATH);
            Directory.CreateDirectory(statPath);

            SaveStatsToCSV("ProjectileSkillStats", projectileStats);
            SaveStatsToCSV("AreaSkillStats", areaStats);
            SaveStatsToCSV("PassiveSkillStats", passiveStats);
            if (unassignedStats.Any())
            {
                SaveStatsToCSV("UnassignedSkillStats", unassignedStats);
                Debug.LogWarning($"Found {unassignedStats.Count} skills with no type assigned. Check UnassignedSkillStats.csv");
            }

            AssetDatabase.Refresh();
            Debug.Log("Skill stats saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving skill stats: {e.Message}\n{e.StackTrace}");
        }
    }

    public static void SaveWithBackup()
    {
        string backupPath = Path.Combine(RESOURCE_ROOT, SKILL_DB_PATH, "Backups");
        Directory.CreateDirectory(backupPath);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 스킬 데이터 백업
        foreach (var skill in skillDatabase.Values)
        {
            string backupFile = Path.Combine(backupPath, $"{skill.ID}_Data_Backup_{timestamp}.json");
            File.WriteAllText(backupFile, JsonConvert.SerializeObject(skill, Formatting.Indented));
        }

        // 스탯 데이터 백업
        string statsBackupPath = Path.Combine(backupPath, $"Stats_Backup_{timestamp}");
        Directory.CreateDirectory(statsBackupPath);

        var projectileStats = new List<SkillStatData>();
        var areaStats = new List<SkillStatData>();
        var passiveStats = new List<SkillStatData>();

        foreach (var skillStats in statDatabase.Values)
        {
            foreach (var stat in skillStats.Values)
            {
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

        SaveStatsToCSV(Path.Combine(statsBackupPath, "ProjectileSkillStats"), projectileStats);
        SaveStatsToCSV(Path.Combine(statsBackupPath, "AreaSkillStats"), areaStats);
        SaveStatsToCSV(Path.Combine(statsBackupPath, "PassiveSkillStats"), passiveStats);

        Debug.Log($"Backup created at: {backupPath}");
        AssetDatabase.Refresh();
    }

    #endregion

    #region Private Methods
    private static void LoadSkillDatabase()
    {
        try
        {
            skillDatabase.Clear();
            string[] jsonFiles = Directory.GetFiles(Path.Combine(RESOURCE_ROOT, SKILL_DB_PATH), "*_Data.json");

            foreach (string jsonPath in jsonFiles)
            {
                string json = File.ReadAllText(jsonPath);
                var skillData = JsonConvert.DeserializeObject<SkillData>(json);
                if (skillData != null)
                {
                    skillDatabase[skillData.ID] = skillData;
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

            LoadStatsFromCSV("ProjectileSkillStats");
            LoadStatsFromCSV("AreaSkillStats");
            LoadStatsFromCSV("PassiveSkillStats");
            LoadStatsFromCSV("UnassignedSkillStats");  // None 타입 스킬 로드
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading stat database: {e.Message}");
            statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
        }
    }

    private static void LoadStatsFromCSV(string fileName)
    {
        string filePath = Path.Combine(RESOURCE_ROOT, SKILL_STAT_PATH, $"{fileName}.csv");
        if (!File.Exists(filePath))
        {
            Debug.Log($"Creating new stats file: {filePath}");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // 기본 헤더 생성 (스킬 정보 포함)
            var headers = new List<string> { "skillname", "skilltype", "skillid", "description" };
            headers.AddRange(typeof(SkillStatData).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.Name != "SkillID")  // SkillID는 별도 컬럼으로 처리
                .OrderBy(p => p.Name)
                .Select(p => p.Name.ToLower()));

            File.WriteAllText(filePath, string.Join(",", headers));
            AssetDatabase.Refresh();
            return;
        }

        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
            {
                Debug.LogWarning($"Stats file is empty: {filePath}");
                return;
            }

            var headers = lines[0].Trim().Split(',');
            if (lines.Length == 1)
            {
                return;
            }

            // 파일 이름에 따른 스킬 타입 결정
            SkillType expectedType = SkillType.None;
            switch (fileName)
            {
                case "ProjectileSkillStats":
                    expectedType = SkillType.Projectile;
                    break;
                case "AreaSkillStats":
                    expectedType = SkillType.Area;
                    break;
                case "PassiveSkillStats":
                    expectedType = SkillType.Passive;
                    break;
                case "UnassignedSkillStats":
                    expectedType = SkillType.None;
                    break;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(',');
                if (values.Length != headers.Length)
                {
                    Debug.LogWarning($"Invalid line format in {fileName}.csv at line {i + 1}");
                    continue;
                }

                string skillName = values[0];
                string skillType = values[1];
                string skillId = values[2];

                // 스킬 타입 검증
                if (!Enum.TryParse<SkillType>(skillType, out SkillType currentType))
                {
                    Debug.LogError($"Invalid skill type {skillType} for skill {skillName}");
                    continue;
                }

                // 파일과 스킬 타입이 일치하는지 확인
                if (expectedType != SkillType.None && currentType != expectedType)
                {
                    Debug.LogWarning($"Skill {skillName} has type {currentType} but is in {expectedType} file. Skipping...");
                    continue;
                }

                var statData = new SkillStatData();
                bool hasError = false;

                // SkillID 설정
                try
                {
                    if (Enum.TryParse<SkillID>(skillId, out SkillID id))
                    {
                        statData.SkillID = id;
                    }
                    else
                    {
                        Debug.LogError($"Invalid SkillID {skillId} for skill {skillName}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing SkillID for skill {skillName}: {e.Message}");
                    continue;
                }

                // 나머지 스탯 데이터 파싱
                for (int j = 3; j < headers.Length; j++)
                {
                    try
                    {
                        SetStatValue(statData, headers[j].Trim(), values[j].Trim());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error parsing value for {headers[j]} in skill {skillName}: {e.Message}");
                        hasError = true;
                        break;
                    }
                }

                if (!hasError && statData.SkillID != SkillID.None)
                {
                    if (!statDatabase.ContainsKey(statData.SkillID))
                    {
                        statDatabase[statData.SkillID] = new Dictionary<int, SkillStatData>();
                    }
                    statDatabase[statData.SkillID][statData.Level] = statData;
                    Debug.Log($"Loaded stats for {skillName} ({currentType}, ID: {statData.SkillID}) level {statData.Level}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading stats from {fileName}.csv: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void SaveStatsToCSV(string fileName, List<SkillStatData> stats)
    {
        try
        {
            string fullPath = Path.Combine(RESOURCE_ROOT, SKILL_STAT_PATH, $"{fileName}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var properties = typeof(SkillStatData).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.Name != "SkillID")  // SkillID는 별도 컬럼으로 처리
                .OrderBy(p => p.Name)
                .ToArray();

            // 헤더에 스킬 정보 컬럼 추가
            var headers = new List<string> { "skillname", "skilltype", "skillid", "description" };  // 설명 컬럼 추가
            headers.AddRange(properties.Select(p => p.Name.ToLower()));
            var lines = new List<string> { string.Join(",", headers) };

            if (stats != null && stats.Any())
            {
                foreach (var stat in stats.OrderBy(s => s.SkillID).ThenBy(s => s.Level))
                {
                    if (!skillDatabase.TryGetValue(stat.SkillID, out var skillData))
                        continue;

                    var values = new List<string>
                    {
                        skillData.Name.Replace(",", ";"),  // CSV 포맷 보호를 위해 콤마를 세미콜론으로 변경
                        skillData.Type.ToString(),
                        ((int)stat.SkillID).ToString(),
                        skillData.Description.Replace(",", ";")  // 설명 추가
                    };

                    values.AddRange(properties.Select(p =>
                    {
                        var value = p.GetValue(stat);
                        if (value == null) return "";
                        if (value is bool b) return b ? "1" : "0";
                        if (value is float f) return f.ToString("F6");
                        if (value is ElementType elementType) return ((int)elementType).ToString();
                        return value.ToString();
                    }));

                    lines.Add(string.Join(",", values));
                }

                // 타입이 None인 경우 경고 메시지 추가
                if (fileName == "UnassignedSkillStats")
                {
                    Debug.LogWarning($"Warning: Found {stats.Count} skills with no type assigned in {fileName}");
                    foreach (var stat in stats)
                    {
                        if (skillDatabase.TryGetValue(stat.SkillID, out var skillData))
                        {
                            Debug.LogWarning($"  - {skillData.Name} (ID: {skillData.ID}): {skillData.Description}");
                        }
                    }
                }

                Debug.Log($"Saved {stats.Count} stats to {fileName}.csv");
            }
            else
            {
                Debug.Log($"Created empty stats file with headers: {fileName}.csv");
            }

            File.WriteAllLines(fullPath, lines);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving stats to CSV {fileName}: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void SaveSkillResources(SkillData skillData)
    {
        try
        {
            if (skillData.Icon != null)
            {
                string iconPath = AssetDatabase.GetAssetPath(skillData.Icon);
                if (!string.IsNullOrEmpty(iconPath))
                {
                    string targetPath = Path.Combine(RESOURCE_ROOT, SKILL_ICON_PATH, $"{skillData.ID}_Icon.png");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    AssetDatabase.CopyAsset(iconPath, targetPath);
                }
            }

            if (skillData.Prefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(skillData.Prefab);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    string targetPath = Path.Combine(RESOURCE_ROOT, SKILL_PREFAB_PATH, $"{skillData.ID}_Prefab.prefab");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    AssetDatabase.CopyAsset(prefabPath, targetPath);
                }
            }

            if (skillData.Type == SkillType.Projectile && skillData.ProjectilePrefab != null)
            {
                string projectilePath = AssetDatabase.GetAssetPath(skillData.ProjectilePrefab);
                if (!string.IsNullOrEmpty(projectilePath))
                {
                    string targetPath = Path.Combine(RESOURCE_ROOT, SKILL_PREFAB_PATH, $"{skillData.ID}_Projectile.prefab");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    AssetDatabase.CopyAsset(projectilePath, targetPath);
                }
            }

            if (skillData.PrefabsByLevel != null)
            {
                for (int i = 0; i < skillData.PrefabsByLevel.Length; i++)
                {
                    if (skillData.PrefabsByLevel[i] != null)
                    {
                        string levelPrefabPath = AssetDatabase.GetAssetPath(skillData.PrefabsByLevel[i]);
                        if (!string.IsNullOrEmpty(levelPrefabPath))
                        {
                            string targetPath = Path.Combine(RESOURCE_ROOT, SKILL_PREFAB_PATH, $"{skillData.ID}_Level_{i + 1}.prefab");
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            AssetDatabase.CopyAsset(levelPrefabPath, targetPath);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving skill resources: {e.Message}");
        }
    }

    private static void SetStatValue(SkillStatData statData, string fieldName, string value)
    {
        try
        {
            var property = typeof(SkillStatData).GetProperty(
                char.ToUpper(fieldName[0]) + fieldName.Substring(1)
            );

            if (property != null)
            {
                object convertedValue;
                if (property.PropertyType == typeof(bool))
                {
                    convertedValue = value.ToLower() == "true" || value == "1";
                }
                else if (property.PropertyType == typeof(SkillID))
                {
                    if (!System.Enum.TryParse<SkillID>(value, out var skillId))
                        throw new System.Exception($"Failed to parse SkillID: {value}");
                    convertedValue = skillId;
                }
                else if (property.PropertyType == typeof(ElementType))
                {
                    if (!System.Enum.TryParse<ElementType>(value, out var elementType))
                        throw new System.Exception($"Failed to parse ElementType: {value}");
                    convertedValue = elementType;
                }
                else
                {
                    convertedValue = System.Convert.ChangeType(value, property.PropertyType);
                }

                property.SetValue(statData, convertedValue);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting stat value: {e.Message}");
        }
    }

    public static void InitializeDefaultData()
    {
        try
        {
            EnsureDirectoryStructure();

            // 기존 데이터 정리
            ClearAllData();

            // 데이터베이스 초기화
            skillDatabase.Clear();
            statDatabase.Clear();

            SaveDatabase();
            SaveStatDatabase();

            AssetDatabase.Refresh();
            Debug.Log("Data reset successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error resetting data: {e.Message}");
        }
    }

    private static void EnsureDirectoryStructure()
    {
        var paths = new[]
        {
            Path.Combine(RESOURCE_ROOT, SKILL_DB_PATH),
            Path.Combine(RESOURCE_ROOT, SKILL_ICON_PATH),
            Path.Combine(RESOURCE_ROOT, SKILL_PREFAB_PATH),
            Path.Combine(RESOURCE_ROOT, SKILL_STAT_PATH)
        };

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }
    }

    private static void ClearAllData()
    {
        try
        {
            var paths = new[]
            {
                Path.Combine(RESOURCE_ROOT, SKILL_DB_PATH),
                Path.Combine(RESOURCE_ROOT, SKILL_ICON_PATH),
                Path.Combine(RESOURCE_ROOT, SKILL_PREFAB_PATH),
                Path.Combine(RESOURCE_ROOT, SKILL_STAT_PATH)
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (!file.EndsWith(".meta"))
                        {
                            if (file.EndsWith(".csv"))
                            {
                                // CSV 파일의 경우 헤더만 남기고 초기화
                                string fileName = Path.GetFileName(file);
                                var headers = new List<string> { "skillname", "skilltype", "skillid", "description" };
                                headers.AddRange(typeof(SkillStatData).GetProperties()
                                    .Where(p => p.CanRead && p.CanWrite && p.Name != "SkillID")
                                    .OrderBy(p => p.Name)
                                    .Select(p => p.Name.ToLower()));

                                File.WriteAllText(file, string.Join(",", headers));
                                Debug.Log($"Reset CSV file to headers only: {fileName}");
                            }
                            else
                            {
                                // CSV 파일이 아닌 경우 파일 삭제
                                File.Delete(file);
                                Debug.Log($"Deleted file: {Path.GetFileName(file)}");
                            }
                        }
                    }
                }
            }

            // 메모리상의 데이터도 초기화
            skillDatabase.Clear();
            statDatabase.Clear();

            AssetDatabase.Refresh();
            Debug.Log("All data cleared successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error clearing data: {e.Message}");
        }
    }
    #endregion
}
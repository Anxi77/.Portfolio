using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Text;

public class SkillDataManager : DataManager<SkillDataManager>, IInitializable
{
    public new bool IsInitialized { get; private set; }

    private const string RESOURCE_PATH = "SkillData";
    private const string PREFAB_PATH = "SkillData/Prefabs";
    private const string ICON_PATH = "SkillData/Icons";
    private const string STAT_PATH = "SkillData/Stats";
    private const string JSON_PATH = "SkillData/Json";

    private Dictionary<SkillID, SkillData> skillDatabase = new Dictionary<SkillID, SkillData>();
    private Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
    private Dictionary<SkillID, Dictionary<int, GameObject>> levelPrefabDatabase = new Dictionary<SkillID, Dictionary<int, GameObject>>();

    private SkillEditorDataContainer editorData;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            InitializeForEditor();
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            InitializeRuntime();
        }
    }

    public void InitializeRuntime()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager...");
            InitializeDefaultData();
            LoadAllSkillData();
            IsInitialized = true;
            Debug.Log("SkillDataManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillDataManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void LoadAllSkillData()
    {
        try
        {
            Debug.Log("Starting LoadAllSkillData...");

            var jsonFiles = Resources.LoadAll<TextAsset>(JSON_PATH);
            Debug.Log($"Found {jsonFiles.Length} JSON files in Resources/{JSON_PATH}");

            foreach (var jsonAsset in jsonFiles)
            {
                try
                {
                    string fileName = jsonAsset.name;
                    Debug.Log($"Processing JSON file: {fileName}");

                    string skillIdStr = fileName.Replace("_Data", "");
                    Debug.Log($"Extracted skill ID: {skillIdStr}");

                    if (System.Enum.TryParse<SkillID>(skillIdStr, out SkillID skillId))
                    {
                        var skillData = JsonUtility.FromJson<SkillData>(jsonAsset.text);
                        if (skillData != null)
                        {
                            // 기본 리소스 로드
                            string iconPath = $"{ICON_PATH}/{skillId}_Icon";
                            string prefabPath = $"{PREFAB_PATH}/{skillId}_Prefab";

                            Debug.Log($"Loading icon from: {iconPath}");
                            Debug.Log($"Loading prefab from: {prefabPath}");

                            // 먼저 스탯 데이터 로드
                            LoadSkillStats(skillId, skillData.type);

                            // 그 다음 레벨별 프리팹 로드
                            LoadLevelPrefabs(skillId, skillData);

                            skillDatabase[skillId] = skillData;
                            Debug.Log($"Successfully loaded skill: {skillData.skillName} (ID: {skillId})");
                        }
                        else
                        {
                            Debug.LogError($"Failed to deserialize JSON for skill {skillId}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse SkillID from filename: {fileName}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing JSON file {jsonAsset.name}: {e.Message}\n{e.StackTrace}");
                }
            }

            Debug.Log($"LoadAllSkillData completed. Loaded {skillDatabase.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LoadAllSkillData: {e.Message}\n{e.StackTrace}");
        }
    }

    private void LoadSkillStats(SkillID skillId, SkillType type)
    {
        try
        {
            string statsFileName = type switch
            {
                SkillType.Projectile => "ProjectileSkillStats",
                SkillType.Area => "AreaSkillStats",
                SkillType.Passive => "PassiveSkillStats",
                _ => null
            };

            if (statsFileName == null)
            {
                Debug.LogError($"Invalid skill type for loading stats: {type}");
                return;
            }

            //Debug.Log($"Loading stats for skill {skillId} from {statsFileName}");

            // Resources 폴더에서 CSV 파일 로드
            var textAsset = Resources.Load<TextAsset>($"{STAT_PATH}/{statsFileName}");
            if (textAsset == null)
            {
                Debug.LogError($"Failed to load stats file: {statsFileName}");
                return;
            }

            var lines = textAsset.text.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError($"Invalid CSV format in {statsFileName}");
                return;
            }

            var headers = lines[0].Trim().Split(',');
            var stats = new Dictionary<int, SkillStatData>();

            if (statDatabase.TryGetValue(skillId, out var existingStats))
            {
                stats = existingStats;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(',');
                if (values.Length != headers.Length) continue;

                var statData = new SkillStatData();
                for (int j = 0; j < headers.Length; j++)
                {
                    SetStatValue(statData, headers[j].ToLower(), values[j]);
                }

                if (statData.skillID == skillId)
                {
                    bool isValidType = type switch
                    {
                        SkillType.Projectile => true,
                        SkillType.Area => true,
                        SkillType.Passive => true,
                        _ => false
                    };

                    if (isValidType)
                    {
                        stats[statData.level] = statData;
                        //Debug.Log($"Loaded level {statData.level} stats for skill {skillId}");
                    }
                }
            }

            if (stats.Any())
            {
                statDatabase[skillId] = stats;
                Debug.Log($"Successfully loaded {stats.Count} stat entries for skill {skillId}");
            }
            else
            {
                Debug.LogWarning($"No valid stats found for skill {skillId} of type {type} in {statsFileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading stats for skill {skillId}: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SetStatValue(SkillStatData statData, string fieldName, string value)
    {
        try
        {
            fieldName = fieldName.ToLower();

            string propertyName = fieldName switch
            {
                "skillid" => "SkillID",
                "level" => "Level",
                "damage" => "Damage",
                "maxskilllevel" => "MaxSkillLevel",
                "element" => "Element",
                "elementalpower" => "ElementalPower",
                "projectilespeed" => "ProjectileSpeed",
                "projectilescale" => "ProjectileScale",
                "shotinterval" => "ShotInterval",
                "piercecount" => "PierceCount",
                "attackrange" => "AttackRange",
                "homingrange" => "HomingRange",
                "ishoming" => "IsHoming",
                "explosionrad" => "ExplosionRad",
                "projectilecount" => "ProjectileCount",
                "innerinterval" => "InnerInterval",
                "radius" => "Radius",
                "duration" => "Duration",
                "tickrate" => "TickRate",
                "ispersistent" => "IsPersistent",
                "movespeed" => "MoveSpeed",
                "effectduration" => "EffectDuration",
                "cooldown" => "Cooldown",
                "triggerchance" => "TriggerChance",
                "damageincrease" => "DamageIncrease",
                "defenseincrease" => "DefenseIncrease",
                "expareaincrease" => "ExpAreaIncrease",
                "homingactivate" => "HomingActivate",
                "hpincrease" => "HpIncrease",
                "movespeedincrease" => "MoveSpeedIncrease",
                "attackspeedincrease" => "AttackSpeedIncrease",
                "attackrangeincrease" => "AttackRangeIncrease",
                "hpregenincrease" => "HpRegenIncrease",
                _ => null
            };

            if (propertyName == null)
            {
                Debug.LogWarning($"No mapping found for field: {fieldName}");
                return;
            }

            var property = typeof(SkillStatData).GetProperty(propertyName);
            if (property != null)
            {
                try
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
                    //Debug.Log($"Successfully set {propertyName} to {value}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error converting value '{value}' for field '{fieldName}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Property not found: {propertyName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetStatValue for field '{fieldName}': {e.Message}");
        }
    }

    protected void InitializeManagers()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager managers...");

            // 각 매니저 초기화 전에 폴더 생성 확인
            CreateResourceFolders();

            // 데이터베이스 초기화
            skillDatabase = new Dictionary<SkillID, SkillData>();
            statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
            levelPrefabDatabase = new Dictionary<SkillID, Dictionary<int, GameObject>>();

            isInitialized = true;
            Debug.Log("SkillDataManager managers initialized successfully");
        }
        catch (System.Exception e)
        {
            isInitialized = false;
            Debug.LogError($"Failed to initialize SkillDataManager managers: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    protected void CreateResourceFolders()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string[] paths = new string[]
        {
            RESOURCE_PATH,
            PREFAB_PATH,
            ICON_PATH,
            STAT_PATH,
            JSON_PATH
        };

        foreach (string path in paths)
        {
            string fullPath = Path.Combine(resourcesPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created directory: {fullPath}");
            }
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    protected void CreateDefaultFiles()
    {
        CreateDefaultCSVFiles();
    }

    private void CreateDefaultCSVFiles()
    {
        try
        {
            Debug.Log("Creating default CSV files...");

            var baseHeaders = new string[] {
                "skillid",
                "level",
                "damage",
                "maxskilllevel",
                "element",
                "elementalpower"
            };

            var projectileHeaders = baseHeaders.Concat(new string[] {
                "projectilespeed",
                "projectilescale",
                "shotinterval",
                "piercecount",
                "attackrange",
                "homingrange",
                "ishoming",
                "explosionrad",
                "projectilecount",
                "innerinterval"
            });

            var areaHeaders = baseHeaders.Concat(new string[] {
                "radius",
                "duration",
                "tickrate",
                "ispersistent",
                "movespeed"
            });

            var passiveHeaders = baseHeaders.Concat(new string[] {
                "effectduration",
                "cooldown",
                "triggerchance",
                "damageincrease",
                "defenseincrease",
                "expareaincrease",
                "homingactivate",
                "hpincrease",
                "movespeedincrease",
                "attackspeedincrease",
                "attackrangeincrease",
                "hpregenincrease"
            });

            Debug.Log("Creating ProjectileSkillStats file...");
            CSVIO<SkillStatData>.CreateDefaultFile("ProjectileSkillStats", string.Join(",", projectileHeaders));

            Debug.Log("Creating AreaSkillStats file...");
            CSVIO<SkillStatData>.CreateDefaultFile("AreaSkillStats", string.Join(",", areaHeaders));

            Debug.Log("Creating PassiveSkillStats file...");
            CSVIO<SkillStatData>.CreateDefaultFile("PassiveSkillStats", string.Join(",", passiveHeaders));

            Debug.Log("Successfully created all default CSV files");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating default CSV files: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }

#if UNITY_EDITOR
    public void ClearAllRuntimeData()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (EditorWindow.focusedWindow != null &&
                EditorWindow.focusedWindow.GetType().Name == "SkillDataEditorWindow")
            {
                CSVIO<SkillStatData>.ClearAll();
                JSONIO<SkillData>.ClearAll();

                skillDatabase?.Clear();
                statDatabase?.Clear();
                levelPrefabDatabase?.Clear();

                Debug.Log("Cleared all skill data from editor window");
            }
        }
    }
#endif
    public void SaveSkillData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("Cannot save null skill data or metadata");
            return;
        }

        try
        {
            var id = skillData.ID;

            if (skillDatabase == null)
            {
                skillDatabase = new Dictionary<SkillID, SkillData>();
            }

            skillDatabase[id] = skillData;

            // JSON 데이터 저장
            JSONIO<SkillData>.SaveData($"{id}_Data", skillData);

            // 리소스 저장
            if (skillData.icon != null)
            {
                ResourceIO<Sprite>.SaveData($"{id}_Icon", skillData.icon);
            }

            if (skillData.defualtPrefab != null)
            {
                ResourceIO<GameObject>.SaveData($"{id}_Prefab", skillData.defualtPrefab);
            }

            // 레벨별 프리팹 저장
            if (skillData.prefabsByLevel != null)
            {
                for (int i = 0; i < skillData.prefabsByLevel.Length; i++)
                {
                    if (skillData.prefabsByLevel[i] != null)
                    {
                        string prefabPath = $"{id}_Level_{i + 1}_Prefab";
                        ResourceIO<GameObject>.SaveData(prefabPath, skillData.prefabsByLevel[i]);
                        Debug.Log($"Saved level {i + 1} prefab for {skillData.skillName}");
                    }
                }
            }

            if (skillData.type == SkillType.Projectile && skillData.projectile != null)
            {
                ResourceIO<GameObject>.SaveData($"{id}_Projectile", skillData.projectile);
            }

            Debug.Log($"Successfully saved skill data for {skillData.skillName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data for {skillData.skillName}: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SaveAllData(List<SkillData> skillDataList, List<SkillEditorDataContainer.SkillLevelStats> skillStats)
    {
        try
        {
            if (skillDataList == null || skillDataList.Count == 0)
            {
                Debug.LogError("No skills to save in the database");
                return;
            }

            Debug.Log($"Starting to save {skillDataList.Count} skills...");

            // 스킬 데이터 저장
            foreach (var skillData in skillDataList)
            {
                if (skillData?.metadata == null) continue;
#if UNITY_EDITOR
                // 리소스 파일들 저장
                SaveSkillResources(skillData);
#endif
                // JSON 데이터 저장
                string jsonPath = Path.Combine(Application.dataPath, "Resources", JSON_PATH, $"{skillData.metadata.ID}_Data.json");
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                string jsonData = JsonUtility.ToJson(skillData, true);
                File.WriteAllText(jsonPath, jsonData);
                Debug.Log($"Saved JSON data for skill: {skillData.metadata.Name}");
            }

            // 스킬 스탯 데이터 저장
            if (skillStats != null)
            {
                var projectileStats = new List<SkillStatData>();
                var areaStats = new List<SkillStatData>();
                var passiveStats = new List<SkillStatData>();

                // 스킬 타입별로 스탯 데이터 분류
                foreach (var skillStatEntry in skillStats)
                {
                    if (skillStatEntry == null) continue;

                    var skillData = skillDataList.Find(s => s.metadata.ID == skillStatEntry.skillID);
                    if (skillData == null) continue;

                    foreach (var statData in skillStatEntry.GetSkillStatDataList())
                    {
                        statData.skillID = skillData.metadata.ID;
                        statData.element = skillData.metadata.Element;

                        switch (skillData.metadata.Type)
                        {
                            case SkillType.Projectile:
                                projectileStats.Add(statData);
                                break;
                            case SkillType.Area:
                                areaStats.Add(statData);
                                break;
                            case SkillType.Passive:
                                passiveStats.Add(statData);
                                break;
                        }
                    }
                }

                // 각 타입별로 CSV 파일 직접 생성
                SaveSkillStatsToCSV("ProjectileSkillStats", projectileStats);
                SaveSkillStatsToCSV("AreaSkillStats", areaStats);
                SaveSkillStatsToCSV("PassiveSkillStats", passiveStats);
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log("Successfully saved all skill data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveSkillStatsToCSV(string fileName, List<SkillStatData> stats)
    {
        if (stats == null || stats.Count == 0) return;

        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", STAT_PATH, $"{fileName}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // 대문자로 시작하는 프로퍼티만 선택 (소문자 버전 제외)
            var properties = typeof(SkillStatData).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && char.IsUpper(p.Name[0]))
                .ToArray();

            var sb = new StringBuilder();

            // 헤더 작성 (한 번만, 소문자로 변환)
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name.ToLower())));

            // 데이터 작성
            foreach (var stat in stats.OrderBy(s => s.SkillID).ThenBy(s => s.Level))
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(stat);
                    if (value == null) return "";
                    if (value is bool b) return b ? "1" : "0";
                    return value.ToString();
                });
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(fullPath, sb.ToString());
            Debug.Log($"Saved {stats.Count} entries to {fileName}.csv");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving CSV file {fileName}: {e.Message}");
        }
    }
#if UNITY_EDITOR
    private void SaveSkillResources(SkillData skillData)
    {
        try
        {
            string prefabPath = Path.Combine(Application.dataPath, "Resources", PREFAB_PATH);
            string iconPath = Path.Combine(Application.dataPath, "Resources", ICON_PATH);

            Directory.CreateDirectory(prefabPath);
            Directory.CreateDirectory(iconPath);

            // 아이콘 저장
            if (skillData.icon != null)
            {
                string iconAssetPath = AssetDatabase.GetAssetPath(skillData.icon);
                if (!string.IsNullOrEmpty(iconAssetPath))
                {
                    string destPath = Path.Combine(iconPath, $"{skillData.metadata.ID}_Icon.png");
                    AssetDatabase.CopyAsset(iconAssetPath, "Assets/Resources/" + ICON_PATH + $"/{skillData.metadata.ID}_Icon.png");
                }
            }

            // 프리팹 저장
            if (skillData.metadata.Prefab != null)
            {
                string prefabAssetPath = AssetDatabase.GetAssetPath(skillData.metadata.Prefab);
                if (!string.IsNullOrEmpty(prefabAssetPath))
                {
                    AssetDatabase.CopyAsset(prefabAssetPath, "Assets/Resources/" + PREFAB_PATH + $"/{skillData.metadata.ID}_Prefab.prefab");
                }
            }

            // 프로젝타일 프리팹 저장
            if (skillData.metadata.Type == SkillType.Projectile && skillData.projectile != null)
            {
                string projectileAssetPath = AssetDatabase.GetAssetPath(skillData.projectile);
                if (!string.IsNullOrEmpty(projectileAssetPath))
                {
                    AssetDatabase.CopyAsset(projectileAssetPath, "Assets/Resources/" + PREFAB_PATH + $"/{skillData.metadata.ID}_Projectile.prefab");
                }
            }

            // 레벨별 프리팹 저장
            if (skillData.prefabsByLevel != null)
            {
                for (int i = 0; i < skillData.prefabsByLevel.Length; i++)
                {
                    if (skillData.prefabsByLevel[i] != null)
                    {
                        string levelPrefabPath = AssetDatabase.GetAssetPath(skillData.prefabsByLevel[i]);
                        if (!string.IsNullOrEmpty(levelPrefabPath))
                        {
                            AssetDatabase.CopyAsset(levelPrefabPath, "Assets/Resources/" + PREFAB_PATH + $"/{skillData.metadata.ID}_Level_{i + 1}.prefab");
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resources for skill {skillData.metadata.Name}: {e.Message}");
        }
    }
#endif
    // 런타임 데이터 접근 메서드들
    public SkillData GetSkillData(SkillID id)
    {
        if (skillDatabase.TryGetValue(id, out var data))
            return data;

        try
        {
            // JSON에서 데이터 로드
            var skillData = JSONIO<SkillData>.LoadData($"{id}_Data");
            if (skillData != null)
            {
                // 리소스 로드
                skillData.icon = ResourceIO<Sprite>.LoadData($"{id}_Icon");
                skillData.metadata.Prefab = ResourceIO<GameObject>.LoadData($"{id}_Metadata");

                if (skillData.metadata.Type == SkillType.Projectile)
                {
                    skillData.projectile = ResourceIO<GameObject>.LoadData($"{id}_Projectile");
                }

                skillDatabase[id] = skillData;
                return skillData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data for {id}: {e.Message}");
        }

        return null;
    }

    public List<SkillData> GetAllSkillData()
    {
        return new List<SkillData>(skillDatabase.Values);
    }

    public ISkillStat GetSkillStatsForLevel(SkillID id, int level, SkillType type)
    {
        var statData = GetSkillStats(id, level);
        if (statData != null)
        {
            return statData.CreateSkillStat(type);
        }
        return null;
    }

    public SkillStatData GetSkillStats(SkillID id, int level)
    {
        if (statDatabase.TryGetValue(id, out var levelStats) &&
            levelStats.TryGetValue(level, out var statData))
        {
            return statData;
        }
        return null;
    }

    // 스킬 업그레이드 관련 메서드
    public void UpdateSkillLevel(SkillID id, int newLevel)
    {
        if (skillDatabase.TryGetValue(id, out var skillData))
        {
            var stats = GetSkillStats(id, newLevel);
            if (stats != null)
            {
                var skillStat = stats.CreateSkillStat(skillData.metadata.Type);
                skillData.SetStatsForLevel(newLevel, skillStat);
            }
        }
    }

    private void InitializeForEditor()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager for Editor mode...");

            // 매니저 초기화
            InitializeManagers();

            if (!isInitialized)
            {
                throw new System.Exception("Failed to initialize managers");
            }

            // 리소스 폴더 생성
            CreateResourceFolders();

            Debug.Log("SkillDataManager initialized for Editor mode");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillDataManager for Editor: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }
#if UNITY_EDITOR
    private void OnDestroy()
    {
        // 에디터 모드서 직접 삭제할 때만 데이터 정리
        if (Application.isEditor && !Application.isPlaying)
        {
            // 에디터 창이 닫힐 때만 정리
            if (EditorWindow.focusedWindow != null &&
                EditorWindow.focusedWindow.GetType().Name == "SkillDataEditorWindow")
            {
                ClearAllRuntimeData();
                Debug.Log("Cleaned up skill data from editor window close");
            }
        }
        // 런타임 중에는 게임오브젝트만 제거하고 데이터는 유지
        else if (Application.isPlaying)
        {
            if (instance == this)
            {
                instance = null;
            }
            Debug.Log("SkillDataManager destroyed during runtime (data preserved)");
        }
    }
#endif
    // LoadSkillData 메서드 수정
    private SkillData LoadSkillData(SkillID id)
    {
        try
        {
            // JSON에서 데이터 로드
            string resourcePath = $"{id}_Data";
            var skillData = JSONIO<SkillData>.LoadData(resourcePath);

            if (skillData != null)
            {
                Debug.Log($"Loading resources for skill {id}...");

                // 기본 리소스 로드
                skillData.icon = ResourceIO<Sprite>.LoadData($"{id}_Icon");
                skillData.metadata.Prefab = ResourceIO<GameObject>.LoadData($"{id}_Prefab");

                // 레벨별 프리팹 로드
                var stats = GetSkillStats(id, 1);  // 레벨 1의 스탯으로 최대 레벨 확인
                if (stats != null)
                {
                    int maxLevel = stats.maxSkillLevel;
                    skillData.prefabsByLevel = new GameObject[maxLevel];

                    for (int i = 0; i < maxLevel; i++)
                    {
                        skillData.prefabsByLevel[i] = ResourceIO<GameObject>.LoadData($"{id}_Level_{i + 1}");

                        if (skillData.prefabsByLevel[i] != null)
                        {
                            Debug.Log($"Successfully loaded level {i + 1} prefab for {skillData.metadata.Name}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load level {i + 1} prefab");
                        }
                    }
                }

                // 프로젝타일 스킬의 경우 추가 프리팹 로드
                if (skillData.metadata.Type == SkillType.Projectile)
                {
                    skillData.projectile = ResourceIO<GameObject>.LoadData($"{id}_Projectile");
                }

                Debug.Log($"Successfully loaded skill data for {skillData.metadata.Name}");
                return skillData;
            }
            else
            {
                Debug.LogError($"Failed to load JSON data for skill {id}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data for {id}: {e.Message}\n{e.StackTrace}");
        }

        return null;
    }

    // 레벨별 프리팹 로드를 위한 새로운 메서드 추가
    private void LoadLevelPrefabs(SkillID skillId, SkillData skillData)
    {
        try
        {
            // 스킬의 최대 레벨 정보 가져오기
            var stats = GetSkillStats(skillId, 1); // 레벨 1의 스탯으로 최대 벨 확인
            if (stats != null)
            {
                int maxLevel = stats.maxSkillLevel;
                skillData.prefabsByLevel = new GameObject[maxLevel];

                for (int i = 0; i < maxLevel; i++)
                {
                    // 파일 이름만 사용하도록 수정
                    string levelPrefabName = $"{skillId}_Level_{i + 1}";
                    var levelPrefab = Resources.Load<GameObject>($"{PREFAB_PATH}/{levelPrefabName}");

                    if (levelPrefab != null)
                    {
                        skillData.prefabsByLevel[i] = levelPrefab;
                        //Debug.Log($"Loaded level {i + 1} prefab for skill {skillId}");

                        // 레벨별 프리팹 데이터베이스에도 저장
                        if (!levelPrefabDatabase.ContainsKey(skillId))
                        {
                            levelPrefabDatabase[skillId] = new Dictionary<int, GameObject>();
                        }
                        levelPrefabDatabase[skillId][i + 1] = levelPrefab;
                    }
                    else
                    {
                        Debug.LogWarning($"No prefab found for skill {skillId} at level {i + 1}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Could not find stats for skill {skillId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level prefabs for skill {skillId}: {e.Message}\n{e.StackTrace}");
        }
    }

    public GameObject GetLevelPrefab(SkillID skillId, int level)
    {
        if (levelPrefabDatabase.TryGetValue(skillId, out var levelPrefabs))
        {
            if (levelPrefabs.TryGetValue(level, out var prefab))
            {
                return prefab;
            }
        }
        return null;
    }
}

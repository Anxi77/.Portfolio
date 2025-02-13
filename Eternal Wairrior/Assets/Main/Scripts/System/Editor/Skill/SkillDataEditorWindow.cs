using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class SkillDataEditorWindow : EditorWindow
{
    #region Fields
    private Dictionary<SkillID, SkillData> skillDatabase = new();
    private Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new();
    private string searchText = "";
    private SkillType typeFilter = SkillType.None;
    private ElementType elementFilter = ElementType.None;
    private SkillID selectedSkillId;
    private Vector2 mainScrollPosition;
    private GUIStyle headerStyle;
    private Vector2 skillListScrollPosition;
    private Vector2 skillDetailScrollPosition;
    private Dictionary<SkillID, bool> levelFoldouts = new();

    // 섹션 토글 상태
    private bool showBasicInfo = true;
    private bool showResources = true;
    private bool showLevelStats = true;
    #endregion

    #region Properties
    private SkillData CurrentSkill
    {
        get
        {
            return skillDatabase.TryGetValue(selectedSkillId, out var skill) ? skill : null;
        }
    }
    #endregion

    [MenuItem("Tools/Skill Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillDataEditorWindow>("Skill Data Editor");
    }

    private void OnEnable()
    {
        RefreshData();
    }

    private void RefreshData()
    {
        Debug.Log("RefreshData called");
        RefreshSkillDatabase();
        RefreshStatDatabase();
    }

    private void RefreshSkillDatabase()
    {
        Debug.Log("RefreshSkillDatabase called");
        skillDatabase = SkillDataEditorUtility.GetSkillDatabase();
    }

    private void RefreshStatDatabase()
    {
        Debug.Log("RefreshStatDatabase called");
        statDatabase = SkillDataEditorUtility.GetStatDatabase();
    }

    private void OnGUI()
    {
        if (headerStyle == null)
        {
            InitializeStyles();
        }

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.Space(10);

            float footerHeight = 25f;
            float contentHeight = position.height - footerHeight - 35f;
            EditorGUILayout.BeginVertical(GUILayout.Height(contentHeight));
            {
                DrawMainContent();
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            DrawFooter();
        }
        EditorGUILayout.EndVertical();
    }

    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(5, 5, 10, 10)
        };
    }

    private void DrawMainContent()
    {
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);
        {
            DrawSkillsTab();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(25));
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SaveAllData();
            }
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                LoadAllData();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Create Backup", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SkillDataEditorUtility.SaveWithBackup();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Reset to Default", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Reset to Default",
                    "Are you sure you want to reset all data to default? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    SkillDataEditorUtility.InitializeDefaultData();
                    selectedSkillId = SkillID.None;

                    EditorApplication.delayCall += () =>
                    {
                        RefreshData();
                        Repaint();
                    };

                    EditorUtility.SetDirty(this);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSkillsTab()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                DrawSkillList();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            DrawVerticalLine(Color.gray);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                DrawSkillDetails();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSkillList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Search & Filter", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            searchText = EditorGUILayout.TextField("Search", searchText);
            typeFilter = (SkillType)EditorGUILayout.EnumPopup("Type", typeFilter);
            elementFilter = (ElementType)EditorGUILayout.EnumPopup("Element", elementFilter);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            float listHeight = position.height - 300;
            skillListScrollPosition = EditorGUILayout.BeginScrollView(
                skillListScrollPosition,
                GUILayout.Height(listHeight)
            );
            {
                var filteredSkills = FilterSkills();
                foreach (var skill in filteredSkills)
                {
                    bool isSelected = skill.ID == selectedSkillId;
                    GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                    if (GUILayout.Button(skill.Name, GUILayout.Height(25)))
                    {
                        selectedSkillId = skill.ID;
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create New Skill", GUILayout.Height(30)))
        {
            CreateNewSkill();
        }
    }

    private void DrawSkillDetails()
    {
        try
        {
            if (CurrentSkill == null)
            {
                EditorGUILayout.LabelField("Select a skill to edit", headerStyle);
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                skillDetailScrollPosition = EditorGUILayout.BeginScrollView(
                    skillDetailScrollPosition,
                    GUILayout.Height(position.height - 100)
                );
                try
                {
                    if (showBasicInfo)
                    {
                        DrawBasicInfo();
                    }

                    if (showResources)
                    {
                        EditorGUILayout.Space(10);
                        DrawResources();
                    }

                    if (showLevelStats)
                    {
                        EditorGUILayout.Space(10);
                        DrawLevelStats();
                    }

                    EditorGUILayout.Space(20);
                    DrawDeleteButton();
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndVertical();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in DrawSkillDetails: {e.Message}\n{e.StackTrace}");
            EditorGUIUtility.ExitGUI();
        }
    }

    private void DrawLevelStats()
    {
        if (CurrentSkill == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Level Stats", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var stats = SkillDataEditorUtility.GetStatDatabase().GetValueOrDefault(CurrentSkill.ID);
            if (stats == null || !stats.Any())
            {
                EditorGUILayout.HelpBox("No level stats found. Click 'Add Level' to create level 1 stats.", MessageType.Info);
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                foreach (var levelStat in stats.Values.OrderBy(s => s.Level))
                {
                    if (!levelFoldouts.ContainsKey(CurrentSkill.ID))
                        levelFoldouts[CurrentSkill.ID] = false;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        levelFoldouts[CurrentSkill.ID] = EditorGUILayout.Foldout(
                            levelFoldouts[CurrentSkill.ID],
                            $"Level {levelStat.Level}",
                            true
                        );

                        if (levelFoldouts[CurrentSkill.ID])
                        {
                            EditorGUILayout.Space(5);
                            DrawStatFields(levelStat);
                            EditorGUILayout.Space(5);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    SkillDataEditorUtility.SaveStatDatabase();
                    EditorUtility.SetDirty(this);
                }
            }

            if (GUILayout.Button("Add Level"))
            {
                AddNewLevel(CurrentSkill.ID);
                EditorUtility.SetDirty(this);
                Repaint();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Basic Information", headerStyle);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // SkillID 수정 가능하도록 추가
            EditorGUI.BeginChangeCheck();
            SkillID newId = (SkillID)EditorGUILayout.EnumPopup("Skill ID", CurrentSkill.ID);
            if (EditorGUI.EndChangeCheck() && newId != CurrentSkill.ID)
            {
                // 기존 데이터베이스에서 해당 ID가 있는지 확인
                if (newId != SkillID.None && skillDatabase.ContainsKey(newId))
                {
                    EditorUtility.DisplayDialog("Error", $"SkillID {newId} already exists!", "OK");
                }
                else
                {
                    try
                    {
                        var oldId = CurrentSkill.ID;
                        Debug.Log($"Changing skill ID from {oldId} to {newId}");

                        // 1. 스킬 데이터의 ID 변경
                        CurrentSkill.ID = newId;

                        // 2. 데이터베이스 업데이트
                        skillDatabase.Remove(oldId);
                        skillDatabase[newId] = CurrentSkill;

                        // 3. 스탯 데이터베이스 업데이트 (있는 경우에만)
                        if (statDatabase.TryGetValue(oldId, out var stats))
                        {
                            statDatabase.Remove(oldId);
                            var newStats = new Dictionary<int, SkillStatData>();
                            foreach (var kvp in stats)
                            {
                                var statData = kvp.Value;
                                statData.SkillID = newId;
                                newStats[kvp.Key] = statData;
                            }
                            statDatabase[newId] = newStats;
                            Debug.Log($"Updated stats for {stats.Count} levels");
                        }

                        // 4. 리소스 파일 경로 및 파일 이름 업데이트 (있는 경우에만)
                        CurrentSkill.IconPath = UpdateResourceIfExists(
                            $"Assets/Resources/SkillData/Icons/{oldId}_Icon.png",
                            $"Assets/Resources/SkillData/Icons/{newId}_Icon.png",
                            CurrentSkill.IconPath);

                        CurrentSkill.PrefabPath = UpdateResourceIfExists(
                            $"Assets/Resources/SkillData/Prefabs/{oldId}_Prefab.prefab",
                            $"Assets/Resources/SkillData/Prefabs/{newId}_Prefab.prefab",
                            CurrentSkill.PrefabPath);

                        CurrentSkill.ProjectilePath = UpdateResourceIfExists(
                            $"Assets/Resources/SkillData/Prefabs/{oldId}_Projectile.prefab",
                            $"Assets/Resources/SkillData/Prefabs/{newId}_Projectile.prefab",
                            CurrentSkill.ProjectilePath);
                        // 5. JSON 파일 업데이트 (있는 경우에만)
                        string oldJsonFile = $"Assets/Resources/SkillData/Json/{oldId}_Data.json";
                        string newJsonFile = $"Assets/Resources/SkillData/Json/{newId}_Data.json";
                        if (System.IO.File.Exists(oldJsonFile))
                        {
                            AssetDatabase.MoveAsset(oldJsonFile, newJsonFile);
                            Debug.Log($"Moved JSON file from {oldJsonFile} to {newJsonFile}");
                        }

                        // 6. 레벨별 프리팹 업데이트 (있는 경우에만)
                        if (CurrentSkill.PrefabsByLevelPaths != null)
                        {
                            for (int i = 0; i < CurrentSkill.PrefabsByLevelPaths.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(CurrentSkill.PrefabsByLevelPaths[i]))
                                {
                                    string oldLevelFile = $"Assets/Resources/SkillData/Prefabs/{oldId}_Level_{i + 1}.prefab";
                                    string newLevelFile = $"Assets/Resources/SkillData/Prefabs/{newId}_Level_{i + 1}.prefab";
                                    CurrentSkill.PrefabsByLevelPaths[i] = UpdateResourceIfExists(oldLevelFile, newLevelFile, CurrentSkill.PrefabsByLevelPaths[i]);
                                }
                            }
                        }

                        // 7. 선택된 스킬 ID 업데이트
                        selectedSkillId = newId;

                        // 8. 변경사항 저장
                        SkillDataEditorUtility.SaveSkillData(CurrentSkill);
                        SkillDataEditorUtility.SaveStatDatabase();

                        // 9. 에디터 갱신
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        EditorUtility.SetDirty(this);

                        Debug.Log($"Successfully changed skill ID from {oldId} to {newId}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error changing skill ID: {e.Message}\n{e.StackTrace}");
                        EditorUtility.DisplayDialog("Error", "Failed to change skill ID. Check console for details.", "OK");
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            CurrentSkill.Name = EditorGUILayout.TextField("Name", CurrentSkill.Name);
            CurrentSkill.Description = EditorGUILayout.TextField("Description", CurrentSkill.Description);
            CurrentSkill.Type = (SkillType)EditorGUILayout.EnumPopup("Type", CurrentSkill.Type);
            CurrentSkill.Element = (ElementType)EditorGUILayout.EnumPopup("Element", CurrentSkill.Element);

            if (EditorGUI.EndChangeCheck())
            {
                SkillDataEditorUtility.SaveSkillData(CurrentSkill);
                EditorUtility.SetDirty(this);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawResources()
    {
        if (CurrentSkill == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 현재 아이콘 표시
            if (CurrentSkill.Icon != null)
            {
                float size = 64f;
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size), GUILayout.Height(size));
                EditorGUI.DrawPreviewTexture(rect, CurrentSkill.Icon.texture);
                EditorGUILayout.Space(5);
            }

            EditorGUI.BeginChangeCheck();
            var newIcon = (Sprite)EditorGUILayout.ObjectField(
                "Icon",
                CurrentSkill.Icon,
                typeof(Sprite),
                false
            );

            if (EditorGUI.EndChangeCheck() && newIcon != null)
            {
                CurrentSkill.Icon = newIcon;
                GUI.changed = true;
            }

            EditorGUILayout.Space(5);

            // 기본 프리팹
            CurrentSkill.Prefab = (GameObject)EditorGUILayout.ObjectField(
                "Base Prefab",
                CurrentSkill.Prefab,
                typeof(GameObject),
                false
            );

            // 프로젝타일 프리팹 (해당하는 경우)
            if (CurrentSkill.Type == SkillType.Projectile)
            {
                CurrentSkill.ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField(
                    "Projectile Prefab",
                    CurrentSkill.ProjectilePrefab,
                    typeof(GameObject),
                    false
                );
            }

            // 레벨별 프리팹
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);

            int newSize = EditorGUILayout.IntField("Level Count", CurrentSkill.PrefabsByLevel?.Length ?? 0);
            if (newSize != CurrentSkill.PrefabsByLevel?.Length)
            {
                Array.Resize(ref CurrentSkill.PrefabsByLevel, newSize);
                GUI.changed = true;
            }

            if (CurrentSkill.PrefabsByLevel != null)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < CurrentSkill.PrefabsByLevel.Length; i++)
                {
                    CurrentSkill.PrefabsByLevel[i] = (GameObject)EditorGUILayout.ObjectField(
                        $"Level {i + 1}",
                        CurrentSkill.PrefabsByLevel[i],
                        typeof(GameObject),
                        false
                    );
                }
                EditorGUI.indentLevel--;
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawStatFields(SkillStatData stat)
    {
        // 기본 스탯
        stat.Damage = EditorGUILayout.FloatField("Damage", stat.Damage);
        stat.MaxSkillLevel = EditorGUILayout.IntField("Max Level", stat.MaxSkillLevel);
        stat.ElementalPower = EditorGUILayout.FloatField("Elemental Power", stat.ElementalPower);

        // 스킬 타입별 특수 스탯
        var skill = skillDatabase[stat.SkillID];
        switch (skill.Type)
        {
            case SkillType.Projectile:
                DrawProjectileStats(stat);
                break;
            case SkillType.Area:
                DrawAreaStats(stat);
                break;
            case SkillType.Passive:
                DrawPassiveStats(stat);
                break;
        }
    }

    private void DrawProjectileStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.ProjectileSpeed = EditorGUILayout.FloatField("Speed", stat.ProjectileSpeed);
            stat.ProjectileScale = EditorGUILayout.FloatField("Scale", stat.ProjectileScale);
            stat.ShotInterval = EditorGUILayout.FloatField("Shot Interval", stat.ShotInterval);
            stat.PierceCount = EditorGUILayout.IntField("Pierce Count", stat.PierceCount);
            stat.AttackRange = EditorGUILayout.FloatField("Attack Range", stat.AttackRange);
            stat.HomingRange = EditorGUILayout.FloatField("Homing Range", stat.HomingRange);
            stat.IsHoming = EditorGUILayout.Toggle("Is Homing", stat.IsHoming);
            stat.ExplosionRad = EditorGUILayout.FloatField("Explosion Radius", stat.ExplosionRad);
            stat.ProjectileCount = EditorGUILayout.IntField("Projectile Count", stat.ProjectileCount);
            stat.InnerInterval = EditorGUILayout.FloatField("Inner Interval", stat.InnerInterval);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawAreaStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.Radius = EditorGUILayout.FloatField("Radius", stat.Radius);
            stat.Duration = EditorGUILayout.FloatField("Duration", stat.Duration);
            stat.TickRate = EditorGUILayout.FloatField("Tick Rate", stat.TickRate);
            stat.IsPersistent = EditorGUILayout.Toggle("Is Persistent", stat.IsPersistent);
            stat.MoveSpeed = EditorGUILayout.FloatField("Move Speed", stat.MoveSpeed);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawPassiveStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.EffectDuration = EditorGUILayout.FloatField("Effect Duration", stat.EffectDuration);
            stat.Cooldown = EditorGUILayout.FloatField("Cooldown", stat.Cooldown);
            stat.TriggerChance = EditorGUILayout.FloatField("Trigger Chance", stat.TriggerChance);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Passive Effects", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.DamageIncrease = EditorGUILayout.FloatField("Damage Increase (%)", stat.DamageIncrease);
            stat.DefenseIncrease = EditorGUILayout.FloatField("Defense Increase (%)", stat.DefenseIncrease);
            stat.ExpAreaIncrease = EditorGUILayout.FloatField("Exp Area Increase (%)", stat.ExpAreaIncrease);
            stat.HomingActivate = EditorGUILayout.Toggle("Homing Activate", stat.HomingActivate);
            stat.HpIncrease = EditorGUILayout.FloatField("HP Increase (%)", stat.HpIncrease);
            stat.MoveSpeedIncrease = EditorGUILayout.FloatField("Move Speed Increase (%)", stat.MoveSpeedIncrease);
            stat.AttackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed Increase (%)", stat.AttackSpeedIncrease);
            stat.AttackRangeIncrease = EditorGUILayout.FloatField("Attack Range Increase (%)", stat.AttackRangeIncrease);
            stat.HpRegenIncrease = EditorGUILayout.FloatField("HP Regen Increase (%)", stat.HpRegenIncrease);
        }
        EditorGUILayout.EndVertical();
    }

    private List<SkillData> FilterSkills()
    {
        return skillDatabase.Values.Where(skill =>
            (string.IsNullOrEmpty(searchText) ||
                skill.Name.ToLower().Contains(searchText.ToLower()))
                &&
            (typeFilter == SkillType.None ||
            skill.Type == typeFilter) &&
            (elementFilter == ElementType.None ||
            skill.Element == elementFilter)
        ).ToList();
    }

    private void CreateNewSkill()
    {
        var newSkill = new SkillData
        {
            ID = SkillID.None,
            Name = "New Skill",
            Description = "New skill description",
            Type = SkillType.None,
            Element = ElementType.None
        };

        try
        {
            // 데이터베이스에 추가
            skillDatabase[SkillID.None] = newSkill;
            SkillDataEditorUtility.SaveSkillData(newSkill);

            // 로컬 데이터베이스 갱신
            RefreshSkillDatabase();

            // 새 스킬 선택
            selectedSkillId = newSkill.ID;

            // 기본 스탯 생성
            var defaultStat = new SkillStatData
            {
                SkillID = newSkill.ID,
                Level = 1,
                MaxSkillLevel = 5,
                Damage = 10f,
                ElementalPower = 1f
            };

            if (!statDatabase.ContainsKey(newSkill.ID))
            {
                statDatabase[newSkill.ID] = new Dictionary<int, SkillStatData>();
            }
            statDatabase[newSkill.ID][1] = defaultStat;
            SkillDataEditorUtility.SaveStatDatabase();

            GUI.changed = true;
            Repaint();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating new skill: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error",
                "Failed to create new skill. Check console for details.",
                "OK");
        }
    }

    private void AddNewLevel(SkillID skillId)
    {
        var stats = SkillDataEditorUtility.GetStatDatabase().GetValueOrDefault(skillId);
        if (stats == null)
        {
            stats = new Dictionary<int, SkillStatData>();
            SkillDataEditorUtility.GetStatDatabase()[skillId] = stats;
        }

        int newLevel = stats.Count > 0 ? stats.Values.Max(s => s.Level) + 1 : 1;
        var newStat = new SkillStatData
        {
            SkillID = skillId,
            Level = newLevel,
            Damage = 10f,
            MaxSkillLevel = 5,
            Element = CurrentSkill.Element,
            ElementalPower = 1f
        };

        // 스킬 타입에 따른 기본값 설정
        switch (CurrentSkill.Type)
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
                break;
            case SkillType.Passive:
                newStat.EffectDuration = 5f;
                newStat.Cooldown = 10f;
                newStat.TriggerChance = 1f;
                break;
        }

        stats[newLevel] = newStat;
        SkillDataEditorUtility.SaveStatDatabase();
    }

    private void DrawDeleteButton()
    {
        EditorGUILayout.Space(20);

        if (GUILayout.Button("Delete Skill", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Delete Skill",
                $"Are you sure you want to delete '{CurrentSkill.Name}'?",
                "Delete", "Cancel"))
            {
                SkillDataEditorUtility.DeleteSkillData(CurrentSkill.ID);
                selectedSkillId = SkillID.None;

                // 에디터 데이터 새로고침
                EditorApplication.delayCall += () =>
                {
                    RefreshData();
                    Repaint();
                };

                EditorUtility.SetDirty(this);
            }
        }
    }

    private void DrawVerticalLine(Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(1));
        EditorGUI.DrawRect(rect, color);
    }

    private void LoadAllData()
    {
        EditorUtility.DisplayProgressBar("Loading Data", "Loading skills...", 0.3f);

        try
        {
            // 데이터베이스 새로고침 전에 현재 선택된 스킬 ID 저장
            var previousSelectedId = selectedSkillId;

            // 모든 데이터 새로고침
            RefreshData();

            // 이전에 선택된 스킬이 여전히 존재하는지 확인
            if (previousSelectedId != SkillID.None && !skillDatabase.ContainsKey(previousSelectedId))
            {
                selectedSkillId = SkillID.None;
            }

            Debug.Log("All data loaded successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void SaveAllData()
    {
        EditorUtility.DisplayProgressBar("Saving Data", "Saving skills...", 0.3f);

        try
        {
            // 변경된 스킬들 저장
            foreach (var skill in skillDatabase.Values)
            {
                SkillDataEditorUtility.SaveSkillData(skill);
            }

            // 스탯 데이터베이스 저장
            SkillDataEditorUtility.SaveStatDatabase();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 저장 후 즉시 리프레시하지 않고, 다음 프레임에서 수행
            EditorApplication.delayCall += () =>
            {
                RefreshSkillDatabase();
            };

            Debug.Log("All data saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // 리소스 파일 업데이트를 위한 헬퍼 메서드
    private string UpdateResourceIfExists(string oldPath, string newPath, string currentPath)
    {
        if (System.IO.File.Exists(oldPath))
        {
            AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(currentPath))
            {
                return currentPath.Replace(
                    Path.GetFileNameWithoutExtension(oldPath),
                    Path.GetFileNameWithoutExtension(newPath)
                );
            }
        }
        return currentPath;
    }
}
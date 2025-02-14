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
        skillDatabase = SkillDataEditorUtility.GetSkillDatabase();
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
                RefreshData();
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
                    RefreshData();
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

    private void DrawBasicInfo()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Basic Information", headerStyle);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            SkillID newId = (SkillID)EditorGUILayout.EnumPopup("Skill ID", CurrentSkill.ID);
            if (EditorGUI.EndChangeCheck() && newId != CurrentSkill.ID)
            {
                if (newId != SkillID.None && !skillDatabase.ContainsKey(newId))
                {
                    var skillData = CurrentSkill.Clone() as SkillData;
                    var oldId = skillData.ID;

                    // 이전 ID 데이터 삭제
                    SkillDataEditorUtility.DeleteSkillData(oldId);

                    // ID 변경 및 저장
                    skillData.ID = newId;
                    SkillDataEditorUtility.SaveSkillData(skillData);

                    // 선택된 ID 업데이트
                    selectedSkillId = newId;

                    // UI 새로고침
                    RefreshData();
                }
            }

            EditorGUI.BeginChangeCheck();
            CurrentSkill.Name = EditorGUILayout.TextField("Name", CurrentSkill.Name);
            CurrentSkill.Description = EditorGUILayout.TextField("Description", CurrentSkill.Description);
            CurrentSkill.Type = (SkillType)EditorGUILayout.EnumPopup("Type", CurrentSkill.Type);
            CurrentSkill.Element = (ElementType)EditorGUILayout.EnumPopup("Element", CurrentSkill.Element);

            if (EditorGUI.EndChangeCheck())
            {
                SaveCurrentSkill();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawResources()
    {
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
                SaveCurrentSkill();
            }

            EditorGUILayout.Space(5);

            // 기본 프리팹
            EditorGUI.BeginChangeCheck();
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
                SaveCurrentSkill();
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

            if (EditorGUI.EndChangeCheck())
            {
                SaveCurrentSkill();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawLevelStats()
    {
        if (CurrentSkill == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Level Stats", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var stats = statDatabase.GetValueOrDefault(CurrentSkill.ID);
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
                }
            }

            if (GUILayout.Button("Add Level"))
            {
                AddNewLevel();
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
        switch (CurrentSkill.Type)
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
            Element = ElementType.None,
            IconPath = "",
            PrefabPath = "",
            ProjectilePath = "",
            PrefabsByLevelPaths = new string[0],
            ResourceReferences = new ResourceReferenceData()
        };

        SkillDataEditorUtility.SaveSkillData(newSkill);
        selectedSkillId = newSkill.ID;
        RefreshData();
    }

    private void AddNewLevel()
    {
        if (CurrentSkill == null) return;

        var stats = statDatabase.GetValueOrDefault(CurrentSkill.ID);
        int newLevel = stats?.Values.Max(s => s.Level) + 1 ?? 1;

        var newStat = new SkillStatData
        {
            SkillID = CurrentSkill.ID,
            Level = newLevel,
            MaxSkillLevel = 5,
            Damage = 10f,
            ElementalPower = 1f,
            Element = CurrentSkill.Element
        };

        if (!statDatabase.ContainsKey(CurrentSkill.ID))
        {
            statDatabase[CurrentSkill.ID] = new Dictionary<int, SkillStatData>();
        }
        statDatabase[CurrentSkill.ID][newLevel] = newStat;

        SkillDataEditorUtility.SaveStatDatabase();
        RefreshData();
    }

    private void SaveCurrentSkill()
    {
        if (CurrentSkill != null)
        {
            SkillDataEditorUtility.SaveSkillData(CurrentSkill);
            RefreshData();
        }
    }

    private void SaveAllData()
    {
        foreach (var skill in skillDatabase.Values)
        {
            SkillDataEditorUtility.SaveSkillData(skill);
        }
        SkillDataEditorUtility.SaveStatDatabase();
        RefreshData();
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
                RefreshData();
            }
        }
    }

    private void DrawVerticalLine(Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(1));
        EditorGUI.DrawRect(rect, color);
    }
}
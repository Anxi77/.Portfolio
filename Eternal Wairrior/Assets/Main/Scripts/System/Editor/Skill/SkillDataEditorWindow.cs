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

        ResourceIO<Sprite>.ClearCache();
        ResourceIO<GameObject>.ClearCache();

        foreach (var skill in skillDatabase.Values)
        {
            if (!string.IsNullOrEmpty(skill.IconPath))
            {
                skill.Icon = ResourceIO<Sprite>.LoadData(skill.IconPath);
            }

            if (!string.IsNullOrEmpty(skill.PrefabPath))
            {
                skill.Prefab = ResourceIO<GameObject>.LoadData(skill.PrefabPath);
            }

            if (skill.Type == SkillType.Projectile && !string.IsNullOrEmpty(skill.ProjectilePath))
            {
                skill.ProjectilePrefab = ResourceIO<GameObject>.LoadData(skill.ProjectilePath);
            }

            if (skill.PrefabsByLevelPaths != null)
            {
                skill.PrefabsByLevel = new GameObject[skill.PrefabsByLevelPaths.Length];
                for (int i = 0; i < skill.PrefabsByLevelPaths.Length; i++)
                {
                    if (!string.IsNullOrEmpty(skill.PrefabsByLevelPaths[i]))
                    {
                        skill.PrefabsByLevel[i] = ResourceIO<GameObject>.LoadData(skill.PrefabsByLevelPaths[i]);
                    }
                }
            }
        }
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

                    SkillDataEditorUtility.DeleteSkillData(oldId);

                    skillData.ID = newId;
                    SkillDataEditorUtility.SaveSkillData(skillData);

                    selectedSkillId = newId;

                    RefreshData();
                }
            }

            EditorGUI.BeginChangeCheck();
            CurrentSkill.Name = EditorGUILayout.TextField("Name", CurrentSkill.Name);
            CurrentSkill.Description = EditorGUILayout.TextField("Description", CurrentSkill.Description);
            CurrentSkill.Type = (SkillType)EditorGUILayout.EnumPopup("Type", CurrentSkill.Type);
            CurrentSkill.Element = (ElementType)EditorGUILayout.EnumPopup("Element", CurrentSkill.Element);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);

            var stats = statDatabase.GetValueOrDefault(CurrentSkill.ID);
            if (stats != null && stats.Any())
            {
                var firstStat = stats.Values.First();
                EditorGUI.BeginChangeCheck();
                int newMaxLevel = EditorGUILayout.IntField("Max Skill Level", firstStat.MaxSkillLevel);

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var levelStat in stats.Values)
                    {
                        levelStat.MaxSkillLevel = newMaxLevel;
                    }

                    if (CurrentSkill.PrefabsByLevel == null || CurrentSkill.PrefabsByLevel.Length != newMaxLevel)
                    {
                        var prefabs = CurrentSkill.PrefabsByLevel;
                        var paths = CurrentSkill.PrefabsByLevelPaths;
                        Array.Resize(ref prefabs, newMaxLevel);
                        Array.Resize(ref paths, newMaxLevel);
                        CurrentSkill.PrefabsByLevel = prefabs;
                        CurrentSkill.PrefabsByLevelPaths = paths;
                    }

                    if (!statDatabase.ContainsKey(CurrentSkill.ID))
                    {
                        statDatabase[CurrentSkill.ID] = new Dictionary<int, SkillStatData>();
                    }

                    var currentStats = statDatabase[CurrentSkill.ID];
                    var existingLevels = currentStats.Keys.ToList();

                    for (int level = 1; level <= newMaxLevel; level++)
                    {
                        if (!currentStats.ContainsKey(level))
                        {
                            var newStat = new SkillStatData
                            {
                                SkillID = CurrentSkill.ID,
                                Level = level,
                                MaxSkillLevel = newMaxLevel,
                                Damage = 10f + (level - 1) * 5f,
                                ElementalPower = 1f + (level - 1) * 0.2f,
                                Element = CurrentSkill.Element
                            };
                            currentStats[level] = newStat;
                        }
                    }

                    foreach (var level in existingLevels.Where(l => l > newMaxLevel))
                    {
                        currentStats.Remove(level);
                    }

                    SkillDataEditorUtility.SaveStatDatabase();
                    SaveCurrentSkill();
                }
            }

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
        if (CurrentSkill == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

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
                string resourcePath = $"SkillData/Icons/{CurrentSkill.ID}_Icon";
                ResourceIO<Sprite>.SaveData(resourcePath, newIcon);
                CurrentSkill.IconPath = resourcePath;
                CurrentSkill.Icon = ResourceIO<Sprite>.LoadData(resourcePath);

                SaveCurrentSkill();

                var currentId = selectedSkillId;
                EditorApplication.delayCall += () =>
                {
                    RefreshData();
                    selectedSkillId = currentId;
                    Repaint();
                };
            }

            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            var newPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Base Prefab",
                CurrentSkill.Prefab,
                typeof(GameObject),
                false
            );

            if (EditorGUI.EndChangeCheck() && newPrefab != null)
            {
                string resourcePath = $"SkillData/Prefabs/{CurrentSkill.ID}_Prefab";
                ResourceIO<GameObject>.SaveData(resourcePath, newPrefab);
                CurrentSkill.PrefabPath = resourcePath;
                CurrentSkill.Prefab = ResourceIO<GameObject>.LoadData(resourcePath);
                SaveCurrentSkill();

                var currentId = selectedSkillId;
                EditorApplication.delayCall += () =>
                {
                    RefreshData();
                    selectedSkillId = currentId;
                    Repaint();
                };
            }

            if (CurrentSkill.Type == SkillType.Projectile)
            {
                EditorGUI.BeginChangeCheck();
                var newProjectilePrefab = (GameObject)EditorGUILayout.ObjectField(
                    "Projectile Prefab",
                    CurrentSkill.ProjectilePrefab,
                    typeof(GameObject),
                    false
                );

                if (EditorGUI.EndChangeCheck() && newProjectilePrefab != null)
                {
                    string resourcePath = $"SkillData/Prefabs/{CurrentSkill.ID}_Projectile";
                    ResourceIO<GameObject>.SaveData(resourcePath, newProjectilePrefab);
                    CurrentSkill.ProjectilePath = resourcePath;
                    CurrentSkill.ProjectilePrefab = ResourceIO<GameObject>.LoadData(resourcePath);
                    SaveCurrentSkill();

                    var currentId = selectedSkillId;
                    EditorApplication.delayCall += () =>
                    {
                        RefreshData();
                        selectedSkillId = currentId;
                        Repaint();
                    };
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);

            var stats = statDatabase.GetValueOrDefault(CurrentSkill.ID);
            if (stats != null && stats.Any())
            {
                var maxLevel = stats.Values.First().MaxSkillLevel;
                if (CurrentSkill.PrefabsByLevel == null || CurrentSkill.PrefabsByLevel.Length != maxLevel)
                {
                    var prefabs = CurrentSkill.PrefabsByLevel;
                    var paths = CurrentSkill.PrefabsByLevelPaths;
                    Array.Resize(ref prefabs, maxLevel);
                    Array.Resize(ref paths, maxLevel);
                    CurrentSkill.PrefabsByLevel = prefabs;
                    CurrentSkill.PrefabsByLevelPaths = paths;
                    SaveCurrentSkill();
                }

                if (CurrentSkill.PrefabsByLevel != null)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < CurrentSkill.PrefabsByLevel.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newLevelPrefab = (GameObject)EditorGUILayout.ObjectField(
                            $"Level {i + 1}",
                            CurrentSkill.PrefabsByLevel[i],
                            typeof(GameObject),
                            false
                        );

                        if (EditorGUI.EndChangeCheck() && newLevelPrefab != null)
                        {
                            string resourcePath = $"SkillData/Prefabs/{CurrentSkill.ID}_Level_{i + 1}";
                            ResourceIO<GameObject>.SaveData(resourcePath, newLevelPrefab);
                            CurrentSkill.PrefabsByLevelPaths[i] = resourcePath;
                            CurrentSkill.PrefabsByLevel[i] = ResourceIO<GameObject>.LoadData(resourcePath);
                            SaveCurrentSkill();

                            var currentId = selectedSkillId;
                            EditorApplication.delayCall += () =>
                            {
                                RefreshData();
                                selectedSkillId = currentId;
                                Repaint();
                            };
                        }
                    }
                    EditorGUI.indentLevel--;
                }
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
                EditorGUILayout.HelpBox("No level stats found. Set Max Skill Level in Basic Info to create level stats.", MessageType.Info);
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
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawStatFields(SkillStatData stat)
    {
        stat.Damage = EditorGUILayout.FloatField("Damage", stat.Damage);
        stat.ElementalPower = EditorGUILayout.FloatField("Elemental Power", stat.ElementalPower);

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
        var window = EditorWindow.GetWindow<SkillCreationPopup>("Create New Skill");
        window.Initialize((selectedId, selectedType) =>
        {
            if (selectedId != SkillID.None && selectedType != SkillType.None)
            {
                var newSkill = new SkillData
                {
                    ID = selectedId,
                    Name = $"New {selectedType} Skill",
                    Description = $"New {selectedType} skill description",
                    Type = selectedType,
                    Element = ElementType.None,
                    IconPath = "",
                    PrefabPath = "",
                    ProjectilePath = "",
                    PrefabsByLevelPaths = new string[0]
                };

                SkillDataEditorUtility.SaveSkillData(newSkill);
                selectedSkillId = newSkill.ID;
                RefreshData();
            }
        });
    }

    public class SkillCreationPopup : EditorWindow
    {
        private SkillID selectedId = SkillID.None;
        private SkillType selectedType = SkillType.None;
        private Action<SkillID, SkillType> onConfirm;
        private Vector2 scrollPosition;

        public void Initialize(Action<SkillID, SkillType> callback)
        {
            onConfirm = callback;
            minSize = new Vector2(300, 400);
            maxSize = new Vector2(300, 400);
            position = new Rect(Screen.width / 2, Screen.height / 2, 300, 400);
            Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Create New Skill", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);

                EditorGUI.BeginChangeCheck();
                selectedType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", selectedType);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedId = SkillID.None;
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Select Skill ID", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    var skillDatabase = SkillDataEditorUtility.GetSkillDatabase();
                    foreach (SkillID id in System.Enum.GetValues(typeof(SkillID)))
                    {
                        if (id == SkillID.None) continue;
                        if (skillDatabase.ContainsKey(id)) continue;

                        bool isSelected = id == selectedId;
                        GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                        if (GUILayout.Button(id.ToString(), GUILayout.Height(25)))
                        {
                            selectedId = id;
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                GUI.enabled = false;
                EditorGUILayout.EnumPopup("Selected Type", selectedType);
                EditorGUILayout.EnumPopup("Selected ID", selectedId);
                GUI.enabled = true;

                EditorGUILayout.Space(10);

                GUI.enabled = selectedId != SkillID.None && selectedType != SkillType.None;
                if (GUILayout.Button("Create", GUILayout.Height(30)))
                {
                    onConfirm?.Invoke(selectedId, selectedType);
                    Close();
                }
                GUI.enabled = true;

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    Close();
                }
            }
            EditorGUILayout.EndVertical();
        }
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
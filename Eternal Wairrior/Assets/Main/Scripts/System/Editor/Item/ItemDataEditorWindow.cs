using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

public class ItemDataEditorWindow : EditorWindow
{
    private enum EditorTab
    {
        Items,
        DropTables
    }

    #region Fields
    private Dictionary<string, ItemData> itemDatabase = new();
    private Dictionary<EnemyType, DropTableData> dropTables = new();
    private string searchText = "";
    private ItemType typeFilter = ItemType.None;
    private ItemRarity rarityFilter = ItemRarity.Common;
    private string selectedItemId;
    private Vector2 mainScrollPosition;
    private EditorTab currentTab;
    private GUIStyle headerStyle;
    private GUIStyle tabStyle;
    private Vector2 itemListScrollPosition;
    private Vector2 itemDetailScrollPosition;
    private Vector2 dropTableScrollPosition;
    private Dictionary<EnemyType, bool> dropTableFoldouts = new();

    // 섹션 토글 상태
    private bool showStatRanges = true;
    private bool showEffects = true;
    private bool showResources = true;
    #endregion

    #region Properties
    private ItemData CurrentItem
    {
        get
        {
            if (string.IsNullOrEmpty(selectedItemId)) return null;
            return itemDatabase.TryGetValue(selectedItemId, out var item) ? item : null;
        }
    }
    #endregion

    [MenuItem("Tools/Item Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataEditorWindow>("Item Data Editor");
    }

    private void OnEnable()
    {
        RefreshData();
    }

    private void RefreshData()
    {
        Debug.Log("RefreshData called");
        RefreshItemDatabase();
        RefreshDropTables();
    }

    private void RefreshItemDatabase()
    {
        Debug.Log("RefreshItemDatabase called");
        itemDatabase = ItemDataEditorUtility.GetItemDatabase();
    }

    private void RefreshDropTables()
    {
        Debug.Log("RefreshDropTables called");
        dropTables = ItemDataEditorUtility.GetDropTables();
    }

    private void OnGUI()
    {
        if (headerStyle == null || tabStyle == null)
        {
            InitializeStyles();
        }

        EditorGUILayout.BeginVertical();
        {
            DrawTabs();
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

        tabStyle = new GUIStyle(EditorStyles.toolbarButton)
        {
            fixedHeight = 25,
            fontStyle = FontStyle.Bold
        };
    }

    private void DrawMainContent()
    {
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);
        {
            switch (currentTab)
            {
                case EditorTab.Items:
                    DrawItemsTab();
                    break;
                case EditorTab.DropTables:
                    DrawDropTablesTab();
                    break;
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawTabs()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(25));
        {
            if (GUILayout.Toggle(currentTab == EditorTab.Items, "Items", tabStyle))
                currentTab = EditorTab.Items;
            if (GUILayout.Toggle(currentTab == EditorTab.DropTables, "Drop Tables", tabStyle))
                currentTab = EditorTab.DropTables;
        }
        EditorGUILayout.EndHorizontal();
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
                ItemDataEditorUtility.SaveWithBackup();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Reset to Default", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Reset to Default",
                    "Are you sure you want to reset all data to default? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    ItemDataEditorUtility.InitializeDefaultData();
                    selectedItemId = null;

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

    private void DrawItemsTab()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                DrawItemList();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            DrawVerticalLine(Color.gray);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                DrawItemDetails();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Search & Filter", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            searchText = EditorGUILayout.TextField("Search", searchText);
            typeFilter = (ItemType)EditorGUILayout.EnumPopup("Type", typeFilter);
            rarityFilter = (ItemRarity)EditorGUILayout.EnumPopup("Rarity", rarityFilter);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            float listHeight = position.height - 300;
            itemListScrollPosition = EditorGUILayout.BeginScrollView(
                itemListScrollPosition,
                GUILayout.Height(listHeight)
            );
            {
                var filteredItems = FilterItems();
                foreach (var item in filteredItems)
                {
                    bool isSelected = item.ID == selectedItemId;
                    GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                    if (GUILayout.Button(item.Name, GUILayout.Height(25)))
                    {
                        selectedItemId = item.ID;
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create New Item", GUILayout.Height(30)))
        {
            CreateNewItem();
        }
    }

    private void DrawItemDetails()
    {
        try
        {
            if (CurrentItem == null)
            {
                EditorGUILayout.LabelField("Select an item to edit", headerStyle);
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                itemDetailScrollPosition = EditorGUILayout.BeginScrollView(
                    itemDetailScrollPosition,
                    GUILayout.Height(position.height - 100)
                );
                try
                {
                    // 기본 정보
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUI.BeginChangeCheck();
                        string newId = EditorGUILayout.TextField("ID", CurrentItem.ID);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (!string.IsNullOrEmpty(newId) && newId != CurrentItem.ID)
                            {
                                CurrentItem.ID = newId;
                                selectedItemId = newId;
                                EditorUtility.SetDirty(this);
                            }
                        }

                        CurrentItem.Name = EditorGUILayout.TextField("Name", CurrentItem.Name);
                        CurrentItem.Description = EditorGUILayout.TextField("Description", CurrentItem.Description);
                        CurrentItem.Type = (ItemType)EditorGUILayout.EnumPopup("Type", CurrentItem.Type);
                        CurrentItem.Rarity = (ItemRarity)EditorGUILayout.EnumPopup("Rarity", CurrentItem.Rarity);
                        CurrentItem.MaxStack = EditorGUILayout.IntField("Max Stack", CurrentItem.MaxStack);
                    }
                    EditorGUILayout.EndVertical();

                    if (showStatRanges)
                    {
                        EditorGUILayout.Space(10);
                        DrawStatRanges();
                    }

                    if (showEffects)
                    {
                        EditorGUILayout.Space(10);
                        DrawEffects();
                    }

                    if (showResources)
                    {
                        EditorGUILayout.Space(10);
                        DrawResources();
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
            Debug.LogError($"Error in DrawItemDetails: {e.Message}\n{e.StackTrace}");
            // GUI Layout 상태를 리셋
            EditorGUIUtility.ExitGUI();
        }
    }

    private void DrawDropTablesTab()
    {
        EditorGUILayout.BeginVertical();
        {
            // 드롭테이블 헤더
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Enemy Drop Tables", headerStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ItemDataEditorUtility.SaveDropTables();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            dropTableScrollPosition = EditorGUILayout.BeginScrollView(
                dropTableScrollPosition,
                GUILayout.Height(position.height - 100)
            );
            {
                // 각 몬스터 타입별 드롭테이블
                foreach (EnemyType enemyType in System.Enum.GetValues(typeof(EnemyType)))
                {
                    if (enemyType == EnemyType.None) continue;
                    if (!dropTableFoldouts.ContainsKey(enemyType))
                        dropTableFoldouts[enemyType] = false;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        // 드롭테이블 헤더
                        EditorGUILayout.BeginHorizontal();
                        {
                            var headerStyle = new GUIStyle(EditorStyles.foldout)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 12
                            };
                            dropTableFoldouts[enemyType] = EditorGUILayout.Foldout(
                                dropTableFoldouts[enemyType],
                                $"{enemyType} Drop Table",
                                true,
                                headerStyle
                            );
                        }
                        EditorGUILayout.EndHorizontal();
                        // 드롭테이블 내용
                        if (dropTableFoldouts[enemyType])
                        {
                            EditorGUILayout.Space(5);
                            DrawDropTableSettings(enemyType);
                            EditorGUILayout.Space(5);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawDropTableSettings(EnemyType enemyType)
    {
        var dropTables = ItemDataEditorUtility.GetDropTables();
        EditorGUI.BeginChangeCheck();
        if (!dropTables.TryGetValue(enemyType, out var dropTable))
        {
            dropTable = new DropTableData
            {
                enemyType = enemyType,
                dropEntries = new List<DropTableEntry>(),
                guaranteedDropRate = 0.1f,
                maxDrops = 3
            };
            dropTables[enemyType] = dropTable;
        }
        // 기본 설정 그룹
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUI.indentLevel++;
            dropTable.guaranteedDropRate = EditorGUILayout.Slider(
                new GUIContent("Guaranteed Drop Rate", "Chance for a guaranteed drop"),
                dropTable.guaranteedDropRate,
                0f,
                1f
            );
            dropTable.maxDrops = EditorGUILayout.IntSlider(
                new GUIContent("Max Drops", "Maximum number of items that can drop"),
                dropTable.maxDrops,
                1,
                10
            );
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
        // 드롭 엔트리 그룹
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Drop Entries", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Entry", GUILayout.Width(80)))
                {
                    var defaultItem = itemDatabase.Values.FirstOrDefault();
                    if (defaultItem != null)
                    {
                        dropTable.dropEntries.Add(new DropTableEntry
                        {
                            itemId = defaultItem.ID,
                            dropRate = 0.1f,
                            rarity = ItemRarity.Common,
                            minAmount = 1,
                            maxAmount = 1
                        });
                        GUI.changed = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            if (dropTable.dropEntries == null)
                dropTable.dropEntries = new List<DropTableEntry>();
            // 엔트리 목록
            for (int i = 0; i < dropTable.dropEntries.Count; i++)
            {
                bool shouldRemove = false;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    ItemDataEditorUtility.DrawDropTableEntry(dropTable, i, out shouldRemove);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);

                if (shouldRemove)
                {
                    dropTable.dropEntries.RemoveAt(i);
                    i--;
                    GUI.changed = true;
                }
            }
        }
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck() || GUI.changed)
        {
            ItemDataEditorUtility.SaveDropTables();
            EditorUtility.SetDirty(this);
        }
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("Editor Settings", headerStyle);
            EditorGUILayout.Space(10);
            // 백업 설정
            EditorGUILayout.LabelField("Backup Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Backup"))
            {
                ItemDataEditorUtility.SaveWithBackup();
            }
            EditorGUILayout.Space(10);
            // 데이터 초기화
            EditorGUILayout.LabelField("Data Management", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Data",
                    "Are you sure you want to reset all data to default? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    ItemDataEditorUtility.InitializeDefaultData();
                    RefreshItemDatabase();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawStatRanges()
    {
        if (CurrentItem == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        try
        {
            bool changed = false;
            // 스탯 개수 범위 설정
            EditorGUILayout.BeginHorizontal();
            {
                int newMinCount = EditorGUILayout.IntField("Stat Count", CurrentItem.StatRanges.minStatCount);
                int newMaxCount = EditorGUILayout.IntField("to", CurrentItem.StatRanges.maxStatCount);

                if (newMinCount != CurrentItem.StatRanges.minStatCount || newMaxCount != CurrentItem.StatRanges.maxStatCount)
                {
                    CurrentItem.StatRanges.minStatCount = newMinCount;
                    CurrentItem.StatRanges.maxStatCount = newMaxCount;
                    changed = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // 가능한 스탯 목록
            for (int i = 0; i < CurrentItem.StatRanges.possibleStats.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var statRange = CurrentItem.StatRanges.possibleStats[i];

                    StatType newStatType = (StatType)EditorGUILayout.EnumPopup("Stat Type", statRange.statType);
                    if (newStatType != statRange.statType)
                    {
                        statRange.statType = newStatType;
                        changed = true;
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        float newMinValue = EditorGUILayout.FloatField("Value Range", statRange.minValue);
                        float newMaxValue = EditorGUILayout.FloatField("to", statRange.maxValue);
                        if (newMinValue != statRange.minValue || newMaxValue != statRange.maxValue)
                        {
                            statRange.minValue = newMinValue;
                            statRange.maxValue = newMaxValue;
                            changed = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    float newWeight = EditorGUILayout.Slider("Weight", statRange.weight, 0f, 1f);
                    if (newWeight != statRange.weight)
                    {
                        statRange.weight = newWeight;
                        changed = true;
                    }

                    IncreaseType newIncreaseType = (IncreaseType)EditorGUILayout.EnumPopup("Increase Type", statRange.increaseType);
                    if (newIncreaseType != statRange.increaseType)
                    {
                        statRange.increaseType = newIncreaseType;
                        changed = true;
                    }

                    if (GUILayout.Button("Remove Stat Range"))
                    {
                        ItemDataEditorUtility.RemoveStatRange(CurrentItem, i);
                        i--;
                        changed = true;
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Stat Range"))
            {
                ItemDataEditorUtility.AddStatRange(CurrentItem);
                changed = true;
            }

            if (changed)
            {
                ItemDataEditorUtility.SaveStatRanges(CurrentItem);
            }
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawResources()
    {
        if (CurrentItem == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 현재 아이콘 표시
            if (CurrentItem.Icon != null)
            {
                float size = 64f;
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size), GUILayout.Height(size));
                EditorGUI.DrawPreviewTexture(rect, CurrentItem.Icon.texture);
                EditorGUILayout.Space(5);
            }

            EditorGUI.BeginChangeCheck();
            var newIcon = (Sprite)EditorGUILayout.ObjectField(
                "Icon",
                CurrentItem.Icon,
                typeof(Sprite),
                false
            );

            if (EditorGUI.EndChangeCheck() && newIcon != null)
            {
                string sourceAssetPath = AssetDatabase.GetAssetPath(newIcon);
                if (!string.IsNullOrEmpty(sourceAssetPath))
                {
                    // 먼저 IconPath 설정
                    CurrentItem.IconPath = sourceAssetPath;

                    // 아이템 데이터 저장 (아이콘 포함)
                    ItemDataEditorUtility.SaveItemData(CurrentItem);

                    // 데이터베이스 새로고침 및 UI 갱신
                    string currentId = CurrentItem.ID;

                    // 리소스 캐시 클리어 및 리로드
                    Resources.UnloadUnusedAssets();
                    EditorApplication.delayCall += () =>
                    {
                        RefreshItemDatabase();
                        selectedItemId = currentId;  // 선택된 아이템 유지
                        Repaint();  // 윈도우 강제 갱신
                    };

                    // UI 갱신
                    EditorUtility.SetDirty(this);
                    GUI.changed = true;
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private List<ItemData> FilterItems()
    {
        return itemDatabase.Values.Where(item =>
            (string.IsNullOrEmpty(searchText) ||
                item.Name.ToLower().Contains(searchText.ToLower()))
                &&
            (typeFilter == ItemType.None ||
            item.Type == typeFilter) &&
            (item.Rarity >= rarityFilter)
        ).ToList();
    }

    private void CreateNewItem()
    {
        var newItem = new ItemData();
        newItem.ID = "NEW_ITEM_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        newItem.Name = "New Item";
        newItem.Description = "New item description";
        newItem.Type = ItemType.None;
        newItem.Rarity = ItemRarity.Common;
        newItem.MaxStack = 1;

        // 데이터베이스에 추가
        ItemDataEditorUtility.SaveItemData(newItem);

        // 로컬 데이터베이스 갱신
        RefreshItemDatabase();

        // 새 아이템 선택
        selectedItemId = newItem.ID;
        GUI.changed = true;
    }

    private void DrawDeleteButton()
    {
        EditorGUILayout.Space(20);

        if (GUILayout.Button("Delete Item", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Delete Item",
                $"Are you sure you want to delete '{CurrentItem.Name}'?",
                "Delete", "Cancel"))
            {
                string itemId = CurrentItem.ID;
                ItemDataEditorUtility.DeleteItemData(itemId);
                selectedItemId = null;

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
        EditorUtility.DisplayProgressBar("Loading Data", "Loading items...", 0.3f);

        try
        {
            // 데이터베이스 새로고침 전에 현재 선택된 아이템 ID 저장
            string previousSelectedId = selectedItemId;

            // 모든 데이터 새로고침
            RefreshData();

            // 이전에 선택된 아이템이 여전히 존재하는지 확인
            if (!string.IsNullOrEmpty(previousSelectedId) && !itemDatabase.ContainsKey(previousSelectedId))
            {
                selectedItemId = null;
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
        EditorUtility.DisplayProgressBar("Saving Data", "Saving items...", 0.3f);

        try
        {
            // 변경된 아이템들 저장
            foreach (var item in itemDatabase.Values)
            {
                ItemDataEditorUtility.SaveItemData(item);
            }

            // 데이터베이스 파일 저장
            ItemDataEditorUtility.SaveDatabase();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 저장 후 즉시 리프레시하지 않고, 다음 프레임에서 수행
            EditorApplication.delayCall += () =>
            {
                RefreshItemDatabase();
            };

            Debug.Log("All data saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void DrawEffects()
    {
        if (CurrentItem == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        try
        {
            bool changed = false;
            // 효과 개수 범위 설정
            EditorGUILayout.BeginHorizontal();
            {
                int newMinCount = EditorGUILayout.IntField("Effect Count", CurrentItem.EffectRanges.minEffectCount);
                int newMaxCount = EditorGUILayout.IntField("to", CurrentItem.EffectRanges.maxEffectCount);

                if (newMinCount != CurrentItem.EffectRanges.minEffectCount || newMaxCount != CurrentItem.EffectRanges.maxEffectCount)
                {
                    CurrentItem.EffectRanges.minEffectCount = newMinCount;
                    CurrentItem.EffectRanges.maxEffectCount = newMaxCount;
                    changed = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            // 가능한 효과 목록
            EditorGUILayout.LabelField("Possible Effects", EditorStyles.boldLabel);
            for (int i = 0; i < CurrentItem.EffectRanges.possibleEffects.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var effectRange = CurrentItem.EffectRanges.possibleEffects[i];

                    string newEffectId = EditorGUILayout.TextField("Effect ID", effectRange.effectId);
                    if (newEffectId != effectRange.effectId)
                    {
                        effectRange.effectId = newEffectId;
                        changed = true;
                    }

                    string newEffectName = EditorGUILayout.TextField("Name", effectRange.effectName);
                    if (newEffectName != effectRange.effectName)
                    {
                        effectRange.effectName = newEffectName;
                        changed = true;
                    }

                    string newDescription = EditorGUILayout.TextField("Description", effectRange.description);
                    if (newDescription != effectRange.description)
                    {
                        effectRange.description = newDescription;
                        changed = true;
                    }

                    EffectType newEffectType = (EffectType)EditorGUILayout.EnumPopup("Type", effectRange.effectType);
                    if (newEffectType != effectRange.effectType)
                    {
                        effectRange.effectType = newEffectType;
                        changed = true;
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        float newMinValue = EditorGUILayout.FloatField("Value Range", effectRange.minValue);
                        float newMaxValue = EditorGUILayout.FloatField("to", effectRange.maxValue);
                        if (newMinValue != effectRange.minValue || newMaxValue != effectRange.maxValue)
                        {
                            effectRange.minValue = newMinValue;
                            effectRange.maxValue = newMaxValue;
                            changed = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    float newWeight = EditorGUILayout.Slider("Weight", effectRange.weight, 0f, 1f);
                    if (newWeight != effectRange.weight)
                    {
                        effectRange.weight = newWeight;
                        changed = true;
                    }

                    DrawApplicableSkillTypes(effectRange, ref changed);
                    DrawApplicableElementTypes(effectRange, ref changed);

                    if (GUILayout.Button("Remove Effect Range"))
                    {
                        ItemDataEditorUtility.RemoveEffectRange(CurrentItem, i);
                        i--;
                        changed = true;
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Effect Range"))
            {
                ItemDataEditorUtility.AddEffectRange(CurrentItem);
                changed = true;
            }

            if (changed)
            {
                ItemDataEditorUtility.SaveEffects(CurrentItem);
            }
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawApplicableSkillTypes(ItemEffectRange effectRange, ref bool changed)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Applicable Skill Types", EditorStyles.boldLabel);

            if (effectRange.applicableSkills == null)
                effectRange.applicableSkills = new SkillType[0];

            var skillTypes = System.Enum.GetValues(typeof(SkillType));
            foreach (SkillType skillType in skillTypes)
            {
                bool isSelected = System.Array.IndexOf(effectRange.applicableSkills, skillType) != -1;
                bool newValue = EditorGUILayout.Toggle(skillType.ToString(), isSelected);

                if (newValue != isSelected)
                {
                    ItemDataEditorUtility.UpdateSkillTypes(effectRange, skillType, newValue);
                    changed = true;
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawApplicableElementTypes(ItemEffectRange effectRange, ref bool changed)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Applicable Element Types", EditorStyles.boldLabel);

            if (effectRange.applicableElements == null)
                effectRange.applicableElements = new ElementType[0];

            var elementTypes = System.Enum.GetValues(typeof(ElementType));
            foreach (ElementType elementType in elementTypes)
            {
                bool isSelected = System.Array.IndexOf(effectRange.applicableElements, elementType) != -1;
                bool newValue = EditorGUILayout.Toggle(elementType.ToString(), isSelected);

                if (newValue != isSelected)
                {
                    ItemDataEditorUtility.UpdateElementTypes(effectRange, elementType, newValue);
                    changed = true;
                }
            }
        }
        EditorGUILayout.EndVertical();
    }
}
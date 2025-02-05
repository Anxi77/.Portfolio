using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using EternalWarrior.Editor;

public class ItemDataEditorWindow : EditorWindow
{
    private enum EditorTab
    {
        Items,
        DropTables,
        Settings
    }

    #region Fields
    private Dictionary<string, ItemData> itemDatabase = new();
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
        RefreshItemDatabase();
    }

    private void OnGUI()
    {
        if (headerStyle == null || tabStyle == null)
        {
            InitializeStyles();
        }

        EditorGUILayout.BeginVertical();
        {
            DrawHeader();
            DrawTabs();
            EditorGUILayout.Space(10);
            float contentHeight = position.height - 90f;
            EditorGUILayout.BeginVertical(GUILayout.Height(contentHeight));
            {
                DrawMainContent();
            }
            EditorGUILayout.EndVertical();
            DrawFooter();
        }
        EditorGUILayout.EndVertical();
    }

    private void RefreshItemDatabase()
    {
        Debug.Log("RefreshItemDatabase called");
        itemDatabase = ItemDataEditorUtility.GetItemDatabase();
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
                case EditorTab.Settings:
                    DrawSettingsTab();
                    break;
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            GUILayout.Label("Item Data Editor", headerStyle);
            GUILayout.FlexibleSpace();
            DrawSearchBar();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTabs()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(25));
        {
            if (GUILayout.Toggle(currentTab == EditorTab.Items, "Items", tabStyle))
                currentTab = EditorTab.Items;
            if (GUILayout.Toggle(currentTab == EditorTab.DropTables, "Drop Tables", tabStyle))
                currentTab = EditorTab.DropTables;
            if (GUILayout.Toggle(currentTab == EditorTab.Settings, "Settings", tabStyle))
                currentTab = EditorTab.Settings;
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
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemsTab()
    {
        // 전체 영역을 좌우로 분할
        EditorGUILayout.BeginHorizontal();
        {
            // 왼쪽 패널 - 아이템 리스트 (고정 너비)
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                DrawItemList();
            }
            EditorGUILayout.EndVertical();
            // 구분선
            EditorGUILayout.Space(5);
            DrawVerticalLine(Color.gray);
            EditorGUILayout.Space(5);
            // 오른쪽 패널 - 아이템 상세 정보 (나머지 영역 차지)
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
        // 검색 및 필터 영역
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
        // 아이템 리스트 영역
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
        // 새 아이템 생성 버튼
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
        catch (System.Exception e)
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
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var entry = dropTable.dropEntries[i];
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField($"Entry {i + 1}", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            dropTable.dropEntries.RemoveAt(i);
                            i--;
                            GUI.changed = true;
                            continue;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                    // 아이템 선택
                    var items = itemDatabase.Values.Select(item => item.Name).ToArray();
                    int selectedIndex = Array.FindIndex(items, name =>
                        itemDatabase.Values.FirstOrDefault(item => item.Name == name)?.ID == entry.itemId
                    );
                    EditorGUI.indentLevel++;
                    int newIndex = EditorGUILayout.Popup("Item", selectedIndex, items);
                    if (newIndex != selectedIndex && newIndex >= 0)
                    {
                        entry.itemId = itemDatabase.Values.ElementAt(newIndex).ID;
                    }
                    entry.dropRate = EditorGUILayout.Slider("Drop Rate", entry.dropRate, 0f, 1f);
                    entry.rarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", entry.rarity);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Amount Range", GUILayout.Width(100));
                        entry.minAmount = EditorGUILayout.IntField(entry.minAmount, GUILayout.Width(50));
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        entry.maxAmount = EditorGUILayout.IntField(entry.maxAmount, GUILayout.Width(50));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
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
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawStatRanges()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        try
        {
            // 스탯 개수 범위 설정
            EditorGUILayout.BeginHorizontal();
            {
                CurrentItem.StatRanges.minStatCount = EditorGUILayout.IntField("Stat Count", CurrentItem.StatRanges.minStatCount);
                CurrentItem.StatRanges.maxStatCount = EditorGUILayout.IntField("to", CurrentItem.StatRanges.maxStatCount);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            // 가능한 스탯 목록
            for (int i = 0; i < CurrentItem.StatRanges.possibleStats.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var statRange = CurrentItem.StatRanges.possibleStats[i];
                    statRange.statType = (StatType)EditorGUILayout.EnumPopup("Stat Type", statRange.statType);
                    EditorGUILayout.BeginHorizontal();
                    {
                        statRange.minValue = EditorGUILayout.FloatField("Value Range", statRange.minValue);
                        statRange.maxValue = EditorGUILayout.FloatField("to", statRange.maxValue);
                    }
                    EditorGUILayout.EndHorizontal();
                    statRange.weight = EditorGUILayout.Slider("Weight", statRange.weight, 0f, 1f);
                    statRange.minRarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", statRange.minRarity);
                    statRange.increaseType = (IncreaseType)EditorGUILayout.EnumPopup("Increase Type", statRange.increaseType);
                    statRange.sourceType = (SourceType)EditorGUILayout.EnumPopup("Source Type", statRange.sourceType);

                    if (GUILayout.Button("Remove Stat Range"))
                    {
                        CurrentItem.StatRanges.possibleStats.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Stat Range"))
            {
                CurrentItem.StatRanges.possibleStats.Add(new ItemStatRange());
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
                    RefreshItemDatabase();
                    selectedItemId = currentId;  // 선택된 아이템 유지

                    // UI 갱신
                    EditorUtility.SetDirty(this);
                    GUI.changed = true;
                    Repaint();  // 윈도우 강제 갱신
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawSearchAndFilter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            searchText = EditorGUILayout.TextField("Search", searchText);

            typeFilter = (ItemType)EditorGUILayout.EnumPopup("Type Filter", typeFilter);

            rarityFilter = (ItemRarity)EditorGUILayout.EnumPopup("Rarity Filter", rarityFilter);
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
                itemDatabase.Remove(CurrentItem.ID);
                selectedItemId = null;
                EditorUtility.SetDirty(this);
            }
        }
    }

    private void DrawSearchBar()
    {
        searchText = GUILayout.TextField(searchText ?? "", EditorStyles.toolbarSearchField);
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

            // 데이터베이스 새로고침
            RefreshItemDatabase();

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
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        try
        {
            // 효과 개수 범위 설정
            EditorGUILayout.BeginHorizontal();
            {
                CurrentItem.EffectRanges.minEffectCount = EditorGUILayout.IntField("Effect Count", CurrentItem.EffectRanges.minEffectCount);
                CurrentItem.EffectRanges.maxEffectCount = EditorGUILayout.IntField("to", CurrentItem.EffectRanges.maxEffectCount);
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
                    effectRange.effectId = EditorGUILayout.TextField("Effect ID", effectRange.effectId);
                    effectRange.effectName = EditorGUILayout.TextField("Name", effectRange.effectName);
                    effectRange.description = EditorGUILayout.TextField("Description", effectRange.description);
                    effectRange.effectType = (EffectType)EditorGUILayout.EnumPopup("Type", effectRange.effectType);

                    EditorGUILayout.BeginHorizontal();
                    {
                        effectRange.minValue = EditorGUILayout.FloatField("Value Range", effectRange.minValue);
                        effectRange.maxValue = EditorGUILayout.FloatField("to", effectRange.maxValue);
                    }
                    EditorGUILayout.EndHorizontal();

                    effectRange.weight = EditorGUILayout.Slider("Weight", effectRange.weight, 0f, 1f);
                    effectRange.minRarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", effectRange.minRarity);

                    // 적용 가능한 아이템 타입
                    DrawApplicableItemTypes(effectRange);

                    // 적용 가능한 스킬 타입
                    DrawApplicableSkillTypes(effectRange);

                    // 적용 가능한 속성
                    DrawApplicableElementTypes(effectRange);

                    if (GUILayout.Button("Remove Effect Range"))
                    {
                        CurrentItem.EffectRanges.possibleEffects.RemoveAt(i);
                        i--;
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Effect Range"))
            {
                CurrentItem.EffectRanges.possibleEffects.Add(new ItemEffectRange());
            }
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawApplicableItemTypes(ItemEffectRange effectRange)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Applicable Item Types", EditorStyles.boldLabel);

            if (effectRange.applicableTypes == null)
                effectRange.applicableTypes = new ItemType[0];

            var itemTypes = System.Enum.GetValues(typeof(ItemType));

            foreach (ItemType itemType in itemTypes)
            {
                bool isSelected = System.Array.IndexOf(effectRange.applicableTypes, itemType) != -1;

                bool newValue = EditorGUILayout.Toggle(itemType.ToString(), isSelected);

                if (newValue != isSelected)
                {
                    var list = new List<ItemType>(effectRange.applicableTypes);

                    if (newValue)
                        list.Add(itemType);
                    else
                        list.Remove(itemType);

                    effectRange.applicableTypes = list.ToArray();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawApplicableSkillTypes(ItemEffectRange effectRange)
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
                    var list = new List<SkillType>(effectRange.applicableSkills);

                    if (newValue)
                        list.Add(skillType);
                    else
                        list.Remove(skillType);

                    effectRange.applicableSkills = list.ToArray();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawApplicableElementTypes(ItemEffectRange effectRange)
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
                    var list = new List<ElementType>(effectRange.applicableElements);

                    if (newValue)
                        list.Add(elementType);
                    else
                        list.Remove(elementType);
                    effectRange.applicableElements = list.ToArray();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }
}
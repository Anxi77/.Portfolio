using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json;

public static class ItemDataEditorUtility
{
    #region Constants
    private const string RESOURCE_ROOT = "Assets/Resources";
    private const string ITEM_DB_PATH = "Items/Database";
    private const string ITEM_ICON_PATH = "Items/Icons";
    private const string DROP_TABLES_PATH = "Items/DropTables";
    #endregion

    #region Data Management
    private static Dictionary<string, ItemData> itemDatabase = new();
    private static Dictionary<EnemyType, DropTableData> dropTables = new();

    public static Dictionary<string, ItemData> GetItemDatabase()
    {
        if (!itemDatabase.Any())
        {
            LoadItemDatabase();
        }
        return new Dictionary<string, ItemData>(itemDatabase);
    }

    public static Dictionary<EnemyType, DropTableData> GetDropTables()
    {
        if (!dropTables.Any())
        {
            LoadDropTables();
        }
        return new Dictionary<EnemyType, DropTableData>(dropTables);
    }

    public static void SaveItemData(ItemData itemData)
    {
        if (itemData == null) return;

        try
        {
            if (!string.IsNullOrEmpty(itemData.IconPath))
            {
                string sourceAssetPath = itemData.IconPath;
                if (!string.IsNullOrEmpty(sourceAssetPath))
                {
                    if (sourceAssetPath.Contains("/Resources/"))
                    {
                        itemData.IconPath = GetResourcePath(sourceAssetPath);
                    }
                    else
                    {
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sourceAssetPath);
                        if (sprite != null)
                        {
                            string resourcePath = $"Items/Icons/{itemData.ID}_Icon";
                            ResourceIO<Sprite>.SaveData(resourcePath, sprite);
                            itemData.IconPath = resourcePath;
                            Debug.Log($"Icon saved to Resources path: {resourcePath}");
                        }
                    }
                }
            }

            var clonedData = itemData.Clone();
            itemDatabase[itemData.ID] = clonedData;

            SaveDatabase();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving item data: {e.Message}\n{e.StackTrace}");
        }
    }

    public static void DeleteItemData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        try
        {
            if (itemDatabase.TryGetValue(itemId, out var item) && itemDatabase.Remove(itemId))
            {
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    string iconPath = $"Assets/Resources/{item.IconPath}.png";
                    if (File.Exists(iconPath))
                    {
                        AssetDatabase.DeleteAsset(iconPath);
                        Debug.Log($"Deleted icon file: {iconPath}");
                    }
                }

                SaveDatabase();

                foreach (var dropTable in dropTables.Values)
                {
                    dropTable.dropEntries.RemoveAll(entry => entry.itemId == itemId);
                }
                SaveDropTables();

                AssetDatabase.Refresh();
                Debug.Log($"Item {itemId} and its resources deleted successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting item {itemId}: {e.Message}\n{e.StackTrace}");
        }
    }

    public static void ClearItemDatabase()
    {
        itemDatabase.Clear();
        SaveDatabase();
    }

    public static void ClearDropTables()
    {
        dropTables.Clear();
        SaveDropTables();
    }

    public static void SaveDatabase()
    {
        try
        {
            JSONIO<SerializableItemList>.SetCustomPath("Items/Database");
            var wrapper = new SerializableItemList { items = itemDatabase.Values.ToList() };
            JSONIO<SerializableItemList>.SaveData("ItemDatabase", wrapper);
            Debug.Log($"Database saved successfully");
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving database: {e.Message}\n{e.StackTrace}");
        }
    }

    public static void SaveDropTables()
    {
        try
        {
            if (dropTables == null || !dropTables.Any())
            {
                CreateDefaultDropTables();
                return;
            }

            JSONIO<DropTablesWrapper>.SetCustomPath("Items/DropTables");
            var wrapper = new DropTablesWrapper { dropTables = dropTables.Values.ToList() };
            JSONIO<DropTablesWrapper>.SaveData("DropTables", wrapper);
            Debug.Log("Drop tables saved successfully");
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving drop tables: {e.Message}\n{e.StackTrace}");
        }
    }

    public static void SaveWithBackup()
    {
        string backupPath = Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH, "Backups");
        Directory.CreateDirectory(backupPath);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupFile = Path.Combine(backupPath, $"ItemDatabase_Backup_{timestamp}.json");

        var wrapper = new SerializableItemList { items = itemDatabase.Values.ToList() };
        File.WriteAllText(backupFile, JsonConvert.SerializeObject(wrapper));

        Debug.Log($"Backup created at: {backupFile}");
        AssetDatabase.Refresh();
    }

    public static void SaveStatRanges(ItemData itemData)
    {
        if (itemData == null) return;
        SaveItemData(itemData);
    }

    public static void SaveEffects(ItemData itemData)
    {
        if (itemData == null) return;
        SaveItemData(itemData);
    }

    public static void RemoveStatRange(ItemData itemData, int index)
    {
        if (itemData == null || index < 0 || index >= itemData.StatRanges.possibleStats.Count) return;
        itemData.StatRanges.possibleStats.RemoveAt(index);
        SaveItemData(itemData);
    }

    public static void AddStatRange(ItemData itemData)
    {
        if (itemData == null) return;
        itemData.StatRanges.possibleStats.Add(new ItemStatRange());
        SaveItemData(itemData);
    }

    public static void RemoveEffectRange(ItemData itemData, int index)
    {
        if (itemData == null || index < 0 || index >= itemData.EffectRanges.possibleEffects.Count) return;
        itemData.EffectRanges.possibleEffects.RemoveAt(index);
        SaveItemData(itemData);
    }

    public static void AddEffectRange(ItemData itemData)
    {
        if (itemData == null) return;
        itemData.EffectRanges.possibleEffects.Add(new ItemEffectRange());
        SaveItemData(itemData);
    }

    public static void UpdateSkillTypes(ItemEffectRange effectRange, SkillType skillType, bool isSelected)
    {
        if (effectRange == null) return;
        var list = new List<SkillType>(effectRange.applicableSkills ?? new SkillType[0]);
        if (isSelected)
            list.Add(skillType);
        else
            list.Remove(skillType);
        effectRange.applicableSkills = list.ToArray();
    }

    public static void UpdateElementTypes(ItemEffectRange effectRange, ElementType elementType, bool isSelected)
    {
        if (effectRange == null) return;
        var list = new List<ElementType>(effectRange.applicableElements ?? new ElementType[0]);
        if (isSelected)
            list.Add(elementType);
        else
            list.Remove(elementType);
        effectRange.applicableElements = list.ToArray();
    }

    #endregion

    #region Private Methods
    private static void LoadItemDatabase()
    {
        try
        {
            JSONIO<SerializableItemList>.SetCustomPath("Items/Database");
            var data = JSONIO<SerializableItemList>.LoadData("ItemDatabase");
            if (data != null && data.items != null)
            {
                itemDatabase = data.items.ToDictionary(item => item.ID);
                LoadItemResources();
            }
            else
            {
                Debug.LogWarning("No item database found or empty database");
                itemDatabase = new Dictionary<string, ItemData>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading item database: {e.Message}\n{e.StackTrace}");
            itemDatabase = new Dictionary<string, ItemData>();
        }
    }

    private static void LoadDropTables()
    {
        try
        {
            JSONIO<DropTablesWrapper>.SetCustomPath("Items/DropTables");
            var data = JSONIO<DropTablesWrapper>.LoadData("DropTables");
            if (data != null && data.dropTables != null)
            {
                dropTables = data.dropTables.ToDictionary(dt => dt.enemyType);
            }
            else
            {
                Debug.LogWarning("No drop tables found");
                dropTables = new Dictionary<EnemyType, DropTableData>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading drop tables: {e.Message}");
            dropTables = new Dictionary<EnemyType, DropTableData>();
        }
    }

    public static void SaveItemResources(ItemData itemData)
    {
        Debug.Log($"SaveItemResources called for item {itemData?.ID}");
        if (itemData == null) return;

        try
        {
            if (!string.IsNullOrEmpty(itemData.IconPath))
            {
                string sourceAssetPath = itemData.IconPath;
                Debug.Log($"Source asset path: {sourceAssetPath}");

                // 이미 Resources 폴더에 있는 경우 경로만 업데이트
                if (sourceAssetPath.Contains("/Resources/"))
                {
                    itemData.IconPath = GetResourcePath(sourceAssetPath);
                    Debug.Log($"Asset already in Resources folder. Updated path to: {itemData.IconPath}");
                    return;
                }

                // Resources 폴더로 복사
                string targetPath = Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH, $"{itemData.ID}_Icon{Path.GetExtension(sourceAssetPath)}").Replace('\\', '/');
                Debug.Log($"Target path: {targetPath}");

                // 디렉토리 생성
                string directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"Created directory: {directory}");
                }

                // 기존 파일이 있다면 삭제
                if (File.Exists(targetPath))
                {
                    AssetDatabase.DeleteAsset(targetPath);
                    Debug.Log($"Deleted existing file: {targetPath}");
                }

                // 파일 복사
                AssetDatabase.CopyAsset(sourceAssetPath, targetPath);
                AssetDatabase.Refresh();
                Debug.Log($"Copied asset from {sourceAssetPath} to {targetPath}");

                // Resources 폴더 기준 상대 경로로 변환
                string resourcePath = GetResourcePath(targetPath);
                itemData.IconPath = resourcePath;
                Debug.Log($"Updated IconPath to: {itemData.IconPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving item resources for {itemData.ID}: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void LoadItemResources()
    {
        foreach (var item in itemDatabase.Values)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    var icon = ResourceIO<Sprite>.LoadData(item.IconPath);
                    if (icon != null)
                    {
                        Debug.Log($"Successfully loaded icon for item {item.ID}");
                    }
                    else
                    {
                        string alternativePath = $"Items/Icons/{item.ID}_Icon";
                        Debug.Log($"Trying alternative path for item {item.ID}: {alternativePath}");
                        icon = ResourceIO<Sprite>.LoadData(alternativePath);

                        if (icon != null)
                        {
                            item.IconPath = alternativePath;
                            Debug.Log($"Successfully loaded icon from alternative path for item {item.ID}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load icon for item {item.ID}. Paths tried:\n1. {item.IconPath}\n2. {alternativePath}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No icon path specified for item {item.ID}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading resources for item {item.ID}: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    public static string GetResourcePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return string.Empty;

        // 백슬래시를 슬래시로 변환
        fullPath = fullPath.Replace('\\', '/');

        // Resources/ 이후의 경로 추출
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex == -1) return fullPath;

        string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);

        // 확장자 제거
        relativePath = Path.ChangeExtension(relativePath, null);

        Debug.Log($"Converting path:\nFull path: {fullPath}\nResource path: {relativePath}");

        return relativePath;
    }

    private static void CreateDefaultDropTables()
    {
        dropTables = new Dictionary<EnemyType, DropTableData>
            {
                {
                    EnemyType.Normal,
                    new DropTableData
                    {
                        enemyType = EnemyType.Normal,
                        guaranteedDropRate = 0.1f,
                        maxDrops = 2,
                        dropEntries = new List<DropTableEntry>()
                    }
                },
                {
                    EnemyType.Elite,
                    new DropTableData
                    {
                        enemyType = EnemyType.Elite,
                        guaranteedDropRate = 0.3f,
                        maxDrops = 3,
                        dropEntries = new List<DropTableEntry>()
                    }
                },
                {
                    EnemyType.Boss,
                    new DropTableData
                    {
                        enemyType = EnemyType.Boss,
                        guaranteedDropRate = 1f,
                        maxDrops = 5,
                        dropEntries = new List<DropTableEntry>()
                    }
                }
            };
        SaveDropTables();
    }

    public static void InitializeDefaultData()
    {
        try
        {
            EnsureDirectoryStructure();

            // 기존 아이템들의 리소스 정리
            foreach (var item in itemDatabase.Values)
            {
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    string iconPath = $"Assets/Resources/{item.IconPath}.png";
                    if (File.Exists(iconPath))
                    {
                        AssetDatabase.DeleteAsset(iconPath);
                        Debug.Log($"Deleted icon file: {iconPath}");
                    }
                }
            }

            // 데이터베이스 초기화
            itemDatabase.Clear();
            SaveDatabase();

            // 드롭 테이블 초기화
            CreateDefaultDropTables();

            AssetDatabase.Refresh();
            Debug.Log("Data reset successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error resetting data: {e.Message}");
            throw;
        }
    }

    private static void EnsureDirectoryStructure()
    {
        var paths = new[]
        {
                Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH),
                Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH)
            };

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }
        AssetDatabase.Refresh();
    }

    public static void DrawDropTableEntry(DropTableData dropTable, int index, out bool shouldRemove)
    {
        shouldRemove = false;
        var entry = dropTable.dropEntries[index];

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField($"Entry {index + 1}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                shouldRemove = true;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (!shouldRemove)
        {
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
                GUI.changed = true;
            }

            float newDropRate = EditorGUILayout.Slider("Drop Rate", entry.dropRate, 0f, 1f);
            if (newDropRate != entry.dropRate)
            {
                entry.dropRate = newDropRate;
                GUI.changed = true;
            }

            ItemRarity newRarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", entry.rarity);
            if (newRarity != entry.rarity)
            {
                entry.rarity = newRarity;
                GUI.changed = true;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Amount Range", GUILayout.Width(100));
                int newMinAmount = EditorGUILayout.IntField(entry.minAmount, GUILayout.Width(50));
                if (newMinAmount != entry.minAmount)
                {
                    entry.minAmount = newMinAmount;
                    GUI.changed = true;
                }

                EditorGUILayout.LabelField("to", GUILayout.Width(20));
                int newMaxAmount = EditorGUILayout.IntField(entry.maxAmount, GUILayout.Width(50));
                if (newMaxAmount != entry.maxAmount)
                {
                    entry.maxAmount = newMaxAmount;
                    GUI.changed = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
    }
    #endregion
}